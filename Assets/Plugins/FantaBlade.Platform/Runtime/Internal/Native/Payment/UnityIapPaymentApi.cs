using System.Collections.Generic;
using FantaBlade.Common;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace FantaBlade.Platform.Internal.Native
{
    internal class UnityIapPaymentApi : IPaymentApi, IDetailedStoreListener
    {
        private IStoreController _controller;

        private IExtensionProvider _extensions;
        // private IAppleExtensions _appleExtensions;
        // private ITransactionHistoryExtensions _transactionHistoryExtensions;

        private bool _purchaseInProgress;

        private readonly Queue<Product> _purchaseQueue = new Queue<Product>();

        public void Init()
        {
            var builder = SdkManager.Order.GetConfigurationBuilder();
            UnityPurchasing.Initialize(this, builder);
            FantaBladePlatform.LoginSuccess += OnLoginSuccess;
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
            if (!FantaBladePlatform.IsInitialized)
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
                FantaBladePlatform.OnPayFailure("illegal product id:" + productId);
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            // _appleExtensions = extensions.GetExtension<IAppleExtensions>();
            // _transactionHistoryExtensions = extensions.GetExtension<ITransactionHistoryExtensions>();
            // _appleExtensions.RegisterPurchaseDeferredListener(OnDeferred);
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

            FantaBladePlatform.OnPaymentInitializeSuccess();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            string errorStr = "Billing failed to initialize!";

            OnInitializeFailed(error, errorStr);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Log.Info(message);
            switch (error)
            {
                case InitializationFailureReason.AppNotKnown:
                    Log.Error("Is your App correctly uploaded on the relevant publisher console?");
                    break;
                case InitializationFailureReason.PurchasingUnavailable:
                    // Ask the user if billing is disabled in device settings.
                    message = "Billing disabled!";
                    Log.Info(message);
                    break;
                case InitializationFailureReason.NoProductsAvailable:
                    // Developer configuration error; check product metadata.
                    message = "No products available for purchase!";
                    Log.Info(message);
                    break;
            }

            FantaBladePlatform.OnPaymentInitializeFailure(message);
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

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchaseFailed(product, new PurchaseFailureDescription(null, failureReason, failureReason.ToString()));
        }
        
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Log.Info("Purchase failed: " + product.definition.id);
            // Detailed debugging information
            // Log.Info("Store specific error code: " +
            //          _transactionHistoryExtensions.GetLastStoreSpecificPurchaseErrorCode());
            // if (_transactionHistoryExtensions.GetLastPurchaseFailureDescription() != null)
            // {
            //     Log.Info("Purchase failure description message: " +
            //              _transactionHistoryExtensions.GetLastPurchaseFailureDescription().message);
            // }

            _purchaseInProgress = false;

            switch (failureDescription.reason)
            {
                case PurchaseFailureReason.UserCancelled:
                    FantaBladePlatform.OnPayCancel();
                    break;
                default:
                    FantaBladePlatform.OnPayFailure(failureDescription.message);
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
            int failedCount = 0;
            while (_purchaseQueue.Count != 0 && failedCount <= 5)
            {
                var product = _purchaseQueue.Dequeue();
                var form = new Dictionary<string, string>
                {
                    { "receipt", product.receipt }
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
                        FantaBladePlatform.OnPaySuccess();
                    }
                    else
                    {
                        _purchaseQueue.Enqueue(product);
                        failedCount++;
                    }
                });
            }
        }
    }
}