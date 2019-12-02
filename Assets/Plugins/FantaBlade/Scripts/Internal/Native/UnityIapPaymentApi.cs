using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace FantaBlade.Internal.Native
{
    internal class UnityIapPaymentApi : IPaymentApi, IStoreListener
    {
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private IAppleExtensions _appleExtensions;
        private ITransactionHistoryExtensions _transactionHistoryExtensions;

        private bool _purchaseInProgress;

        private readonly Queue<Product> _purchaseQueue = new Queue<Product>();

        public void Init()
        {
            var builder = SdkManager.Order.GetConfigurationBuilder();
            UnityPurchasing.Initialize(this, builder);
            Api.LoginSuccess += OnLoginSuccess;
        }

        public Product GetProductById(string id)
        {
            return _controller.products.WithID(id);
        }

        public Product[] GetProducts()
        {
            return _controller.products.all;
        }

        public void Pay(string productId)
        {
            if (!Api.IsInitialized)
            {
                Log.Warning("FBSDK is not initialized");
                return;
            }

            if (_purchaseInProgress)
            {
                Log.Info("Please wait, purchase in progress");
                return;
            }

            var product = _controller.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                _controller.InitiatePurchase(product);
                _purchaseInProgress = true;
            }
            else
            {
                Api.OnPayFailure("illegal product id:" + productId);
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            _appleExtensions = extensions.GetExtension<IAppleExtensions>();
            _transactionHistoryExtensions = extensions.GetExtension<ITransactionHistoryExtensions>();
            _appleExtensions.RegisterPurchaseDeferredListener(OnDeferred);
            foreach (var item in controller.products.all)
            {
                if (item.availableToPurchase)
                {
                    Log.Debug(string.Join(" - ",
                        new[]
                        {
                            item.definition.id,
                            item.metadata.localizedTitle,
                            item.metadata.localizedDescription,
                            item.metadata.isoCurrencyCode,
                            item.metadata.localizedPriceString,
                            item.transactionID
                        }));

#if INTERCEPT_PROMOTIONAL_PURCHASES
                // Set all these products to be visible in the user's App Store according to Apple's Promotional IAP feature
                // https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/StoreKitGuide/PromotingIn-AppPurchases/PromotingIn-AppPurchases.html
                _appleExtensions.SetStorePromotionVisibility(item, AppleStorePromotionVisibility.Show);
#endif
                }
            }

            Api.OnPaymentInitializeSuccess();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            string errorStr = "Billing failed to initialize!";
            Log.Info(errorStr);
            switch (error)
            {
                case InitializationFailureReason.AppNotKnown:
                    Log.Error("Is your App correctly uploaded on the relevant publisher console?");
                    break;
                case InitializationFailureReason.PurchasingUnavailable:
                    // Ask the user if billing is disabled in device settings.
                    errorStr = "Billing disabled!";
                    Log.Info(errorStr);
                    break;
                case InitializationFailureReason.NoProductsAvailable:
                    // Developer configuration error; check product metadata.
                    errorStr = "No products available for purchase!";
                    Log.Info(errorStr);
                    break;
            }

            Api.OnPaymentInitializeFailure(errorStr);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            var product = e.purchasedProduct;
            Log.Info("Purchase OK: " + product.definition.id);

            _purchaseInProgress = false;
            _purchaseQueue.Enqueue(product);
            HandlePurchaseQueue();

            return PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product item, PurchaseFailureReason p)
        {
            Log.Info("Purchase failed: " + item.definition.id);
            // Detailed debugging information
            Log.Info("Store specific error code: " +
                     _transactionHistoryExtensions.GetLastStoreSpecificPurchaseErrorCode());
            if (_transactionHistoryExtensions.GetLastPurchaseFailureDescription() != null)
            {
                Log.Info("Purchase failure description message: " +
                         _transactionHistoryExtensions.GetLastPurchaseFailureDescription().message);
            }

            _purchaseInProgress = false;

            switch (p)
            {
                case PurchaseFailureReason.UserCancelled:
                    Api.OnPayCancel();
                    break;
                case PurchaseFailureReason.DuplicateTransaction:
                    // 消耗型商品 如果出现重复交易错误 则按照 IAP 1.23.1 release note:
                    // Changes 1.22.0 "Fixed GooglePlay store consumable products
                    // already owned error due to network issues." - developers may
                    // choose to handle the `PurchaseFailureReason.DuplicateTransaction`
                    // for a ProductType.Consumable by rewarding the user with the
                    // product, and presuming that Unity IAP will automatically complete the transaction
                    // 修复离线购买不一致产生OnPurchaseFailed回调的情况。
                    // 更改1.22.0“由于网络问题，已修复的GooglePlay商店消耗品已经拥有错误”。
                    // -开发人员可以选择为 ProductType.Consumable 处理
                    // “ PurchaseFailureReason.DuplicateTransaction”，
                    // 通过奖励用户产品并假定Unity IAP将自动完成交易来进行消耗。
                    // 参考链接:
                    // https://forum.unity.com/threads/unity-iap-store-package-1-23-1-is-now-available.415517/page-2#post-5215856
                    // https://forum.unity.com/threads/unity-iap-1-23-1-duplicatetransaction-for-consumables.784508/
                    if (ProductType.Consumable == item.definition.type)
                    {
                        // 按照 IAP release note,不判定为失败
                        _purchaseQueue.Enqueue(item);
                        HandlePurchaseQueue();
                    }
                    else
                    {
                        Api.OnPayFailure(p.ToString());
                    }
                    break;
                default:
                    Api.OnPayFailure(p.ToString());
                    break;
            }
        }

        private void OnDeferred(Product item)
        {
            Log.Info("Purchase deferred: " + item.definition.id);
        }

        private void OnLoginSuccess(string s)
        {
            HandlePurchaseQueue();
        }

        private void HandlePurchaseQueue()
        {
            while (_purchaseQueue.Count != 0)
            {
                var product = _purchaseQueue.Dequeue();
                var form = new Dictionary<string, string>
                {
                    {"receipt", product.receipt}
                };
                PlatformApi.Iap.PurchaseNotify.Post(form, (err, meta, resp) =>
                {
                    if (err != null)
                    {
                        Log.Info(err);
                    }

                    if (meta.Status == 200)
                    {
                        _controller.ConfirmPendingPurchase(product);
                        Api.OnPaySuccess();
                    }
                    else
                    {
                        _purchaseQueue.Enqueue(product);
                    }
                });
            }
        }
    }
}