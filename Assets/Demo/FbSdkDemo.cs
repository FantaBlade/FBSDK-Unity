using System.Globalization;
using FbSdk;
using UnityEngine;
using UnityEngine.Purchasing;

public class FbSdkDemo : MonoBehaviour
{
    [ContextMenu("Delete All PlayerPrefs")]
    private void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    // Use this for initialization
    private void Start()
    {
        Sdk.InitializeSuccess += OnInitializeSuccess;
        Sdk.LoginSuccess += OnLoginSuccess;
        Sdk.LogoutSuccess += OnLogoutSuccess;
        Sdk.PaySuccess += OnPaySuccess;
        Sdk.PayCancel += OnPayCancel;
        Sdk.Init("44I1ucBEaIRvm4Re", true, Sdk.PublishRegion.Overseas);
    }

    private void OnInitializeSuccess()
    {
        Sdk.Login();
    }

    private void OnLoginSuccess(string token)
    {
        Debug.Log(token);
    }

    private void OnLogoutSuccess()
    {
        Sdk.Login();
    }

    private void OnPaySuccess()
    {
        Debug.Log("OnPaySuccess");
    }

    private void OnPayCancel()
    {
        Debug.Log("OnPayCancel");
    }

    public void Pay()
    {
        Sdk.Pay("android.test.purchased");
    }

    public void ShowProducts()
    {
        var products = Sdk.GetProducts();
        if (products != null)
        {
            foreach (var product in products)
            {
                LogProduct(product);
            }
        }
    }

    private void LogProduct(Product product)
    {
        Debug.Log(string.Join(" - ",
            new[]
            {
                product.definition.id,
                product.metadata.localizedTitle,
                product.metadata.localizedDescription,
                product.metadata.isoCurrencyCode,
                product.metadata.localizedPriceString,
                product.transactionID
            }));
    }
}