#if UNITY_IOS

using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Purchasing;

namespace FbSdk.Internal.Native
{
    public class IosNativeApi : INativeApi, IStoreListener
    {
        private IStoreController _controller;

        private IExtensionProvider _extensions;
        private IAppleExtensions _appleExtensions;
        private ITransactionHistoryExtensions _transactionHistoryExtensions;

        private string _lastTransactionId;

        private bool _purchaseInProgress;

        public void Init()
        {
            var module = StandardPurchasingModule.Instance();

            // The FakeStore supports: no-ui (always succeeding), basic ui (purchase pass/fail), and
            // developer ui (initialization, purchase, failure code setting). These correspond to
            // the FakeStoreUIMode Enum values passed into StandardPurchasingModule.useFakeStoreUIMode.
            module.useFakeStoreUIMode = FakeStoreUIMode.DeveloperUser;

            var builder = ConfigurationBuilder.Instance(module);

            // Use the products defined in the IAP Catalog GUI.
            // E.g. Menu: "Window" > "Unity IAP" > "IAP Catalog", then add products, then click "App Store Export".
            var catalog = ProductCatalog.LoadDefaultCatalog();

            foreach (var product in catalog.allValidProducts)
            {
                if (product.allStoreIDs.Count > 0)
                {
                    var ids = new IDs();
                    foreach (var storeId in product.allStoreIDs)
                    {
                        ids.Add(storeId.id, storeId.store);
                    }

                    builder.AddProduct(product.id, product.type, ids);
                }
                else
                {
                    builder.AddProduct(product.id, product.type);
                }
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public void Pay(string productId, string name, int price)
        {
            if (!Sdk.IsInitialized)
            {
                Debug.LogWarning("FBSDK is not initialized");
                return;
            }
            if (_purchaseInProgress == true)
            {
                Debug.Log("Please wait, purchase in progress");
                return;
            }

            var product = _controller.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                _controller.InitiatePurchase(product);
            }
            else
            {
                Sdk.OnPayFailure("illegal product id:" + productId);
            }

            _purchaseInProgress = true;
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
                    Debug.Log(string.Join(" - ",
                        new[]
                        {
                            item.metadata.localizedTitle,
                            item.metadata.localizedDescription,
                            item.metadata.localizedDescription,
                            item.metadata.isoCurrencyCode,
                            item.metadata.localizedPrice.ToString(CultureInfo.InvariantCulture),
                            item.metadata.localizedPriceString,
                            item.transactionID,
                            item.receipt
                        }));

#if INTERCEPT_PROMOTIONAL_PURCHASES
// Set all these products to be visible in the user's App Store according to Apple's Promotional IAP feature
// https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/StoreKitGuide/PromotingIn-AppPurchases/PromotingIn-AppPurchases.html
                m_AppleExtensions.SetStorePromotionVisibility(item, AppleStorePromotionVisibility.Show);
#endif
                }
            }

            Sdk.OnInitializeSuccess();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("Billing failed to initialize!");
            switch (error)
            {
                case InitializationFailureReason.AppNotKnown:
                    Debug.LogError("Is your App correctly uploaded on the relevant publisher console?");
                    break;
                case InitializationFailureReason.PurchasingUnavailable:
                    // Ask the user if billing is disabled in device settings.
                    Debug.Log("Billing disabled!");
                    break;
                case InitializationFailureReason.NoProductsAvailable:
                    // Developer configuration error; check product metadata.
                    Debug.Log("No products available for purchase!");
                    break;
            }
            
            _purchaseInProgress = false;

            Sdk.OnInitializeFailure(error.ToString());
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            var product = e.purchasedProduct;
            Debug.Log("Purchase OK: " + product.definition.id);
            Debug.Log("Receipt: " + product.receipt);

            _lastTransactionId = product.transactionID;
            _purchaseInProgress = false;

            var form = new Dictionary<string, string>
            {
                {"transactionId", product.transactionID},
                {"receipt", product.receipt}
            };
            PlatformApi.Iap.PurchaseNotify.Post(form, (err, meta, resp) =>
            {
                Debug.Log(err);
                _controller.ConfirmPendingPurchase(product);
            });
            return PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product item, PurchaseFailureReason p)
        {
            Debug.Log("Purchase failed: " + item.definition.id);
            // Detailed debugging information
            Debug.Log("Store specific error code: " +
                      _transactionHistoryExtensions.GetLastStoreSpecificPurchaseErrorCode());
            if (_transactionHistoryExtensions.GetLastPurchaseFailureDescription() != null)
            {
                Debug.Log("Purchase failure description message: " +
                          _transactionHistoryExtensions.GetLastPurchaseFailureDescription().message);
            }

            switch (p)
            {
                case PurchaseFailureReason.UserCancelled:
                    Sdk.OnPayCancel();
                    break;
                default:
                    Sdk.OnPayFailure(p.ToString());
                    break;
            }
        }

        private void OnDeferred(Product item)
        {
            Debug.Log("Purchase deferred: " + item.definition.id);
        }
    }
}
#endif