using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Purchasing;

namespace FantaBlade.Internal
{
    internal class OrderManager
    {
        private ProductCatalog _catalog;

        public void Pay(string productId)
        {
            SdkManager.PaymentApi.Pay(productId);
        }

        public void SetProductCatalog(string productCatalogJson)
        {
            if (!string.IsNullOrEmpty(productCatalogJson))
            {
                _catalog = ProductCatalog.Deserialize(productCatalogJson);
            }
        }

        public ProductCatalog GetProductCatalog()
        {
            // Use the products defined in the IAP Catalog GUI.
            // E.g. Menu: "Window" > "Unity IAP" > "IAP Catalog", then add products, then click "App Store Export".
            return _catalog ?? (_catalog = ProductCatalog.LoadDefaultCatalog());
        }

        public ConfigurationBuilder GetConfigurationBuilder()
        {
            var catalog = GetProductCatalog();
            var module = StandardPurchasingModule.Instance();
            var builder = ConfigurationBuilder.Instance(module);

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

            return builder;
        }

        /// <summary>
        ///     返回自定义商品集，用于自建渠道与Stub
        /// </summary>
        /// <param name="catalog">ProductCatalog</param>
        /// <returns>ProductCollection</returns>
        public ProductCollection GetCustomProducts()
        {
            var catalog = GetProductCatalog();
            Assembly asm = Assembly.GetAssembly(typeof(UnityPurchasing));
            Type productCollectionType = asm.GetType("UnityEngine.Purchasing.ProductCollection");
            ConstructorInfo productCollectionConstructor =
                productCollectionType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.DefaultBinder,
                    new[] {typeof(Product[])}, null);
            Type productType = asm.GetType("UnityEngine.Purchasing.Product");
            ConstructorInfo productConstructor =
                productType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.DefaultBinder,
                    new[] {typeof(ProductDefinition), typeof(ProductMetadata)}, null);

            Debug.Assert(productCollectionConstructor != null, "productCollectionConstructor != null");
            Debug.Assert(productConstructor != null, "productConstructor != null");

            return (ProductCollection) productCollectionConstructor.Invoke(new object[]
            {
                catalog.allProducts
                    .Select(
                        item =>
                        {
                            return (Product)
                                productConstructor.Invoke(new object[]
                                {
                                    new ProductDefinition(item.id, item.id, item.type, true,
                                        item.Payouts.Select(payout =>
                                            new PayoutDefinition((PayoutType) payout.type, payout.subtype,
                                                payout.quantity))),
                                    new ProductMetadata("¥" + ((int)(item.Payouts[0].quantity / 10)).ToString(),
                                        item.defaultDescription.Title, item.defaultDescription.Description, "CNY",
                                        (int)(item.Payouts[0].quantity / 10))
                                });
                        })
                    .ToArray()
            });
        }

        /// <summary>
        /// 判断是否可以支付, 应该在 IPaymentApi 添加接口,当前只简单实现 android平台下 是否支持GooglePlay支付
        /// </summary>
        public bool IsPaymentSupport()
        {
#if UNITY_ANDROID
            try
            {
                const string GoogleApiAvailability_Classname =
                    "com.google.android.gms.common.GoogleApiAvailability";
                AndroidJavaClass clazz =
                    new AndroidJavaClass(GoogleApiAvailability_Classname);
                AndroidJavaObject obj =
                    clazz.CallStatic<AndroidJavaObject>("getInstance");

                var androidJC = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity = androidJC.GetStatic<AndroidJavaObject>("currentActivity");

                int value = obj.Call<int>("isGooglePlayServicesAvailable", activity);

                // result codes from https://developers.google.com/android/reference/com/google/android/gms/common/ConnectionResult

                // 0 == success
                // 1 == service_missing
                // 2 == update service required
                // 3 == service disabled
                // 18 == service updating
                // 9 == service invalid
                return value != 1 && value != 9;
            }
            catch (AndroidJavaException javaException)
            {
                Debug.Log(javaException.Message);
            }

            return false;
#else
            return true;
#endif
        }

        private static readonly int[] XiaomiPriceTierPrices =
        {
            0,
            1,
            3,
            6,
            8,
            12,
            18,
            25,
            28,
            30,
            40,
            45,
            50,
            60,
            68,
            73,
            78,
            88,
            93,
            98,
            108,
            113,
            118,
            123,
            128,
            138,
            148,
            153,
            158,
            163,
            168,
            178,
            188,
            193,
            198,
            208,
            218,
            223,
            228,
            233,
            238,
            243,
            248,
            253,
            258,
            263,
            268,
            273,
            278,
            283,
            288,
            298,
            308,
            318,
            328,
            348,
            388,
            418,
            448,
            488,
            518,
            548,
            588,
            618,
            648,
            698,
            798,
            818,
            848,
            898,
            998,
            1048,
            1098,
            1148,
            1198,
            1248,
            1298,
            1398,
            1448,
            1498,
            1598,
            1648,
            1998,
            2298,
            2598,
            2998,
            3298,
            3998,
            4498,
            4998,
            5898,
            6498
        };
    }
}