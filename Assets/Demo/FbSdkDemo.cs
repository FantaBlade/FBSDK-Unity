﻿using System.Globalization;
using FantaBlade;
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
        new SRDebugger.Services.Implementation.StandardConsoleService();
        SRDebug.Init();
        
        Api.InitializeSuccess += OnInitializeSuccess;
        Api.LoginSuccess += OnLoginSuccess;
        Api.LogoutSuccess += OnLogoutSuccess;
        Api.PaySuccess += OnPaySuccess;
        Api.PayCancel += OnPayCancel;
//        Api.Init("44I1ucBEaIRvm4Re", true, PublishRegion.China);
        Api.Init("zsN9eQcEqcmWnBCT", true, PublishRegion.SoutheastAsia);
        if (Api.IsGooglePlayServiceNecessary)
        {
            Debug.Log("Need GooglePlay");
            Debug.Log("GooglePlay is " + Api.IsGooglePlayServiceValid);
        }
    }

    private void OnInitializeSuccess()
    {
        Api.Login();
    }

    private void OnLoginSuccess(string token)
    {
        Debug.Log(token);
    }

    private void OnLogoutSuccess()
    {
        Api.Login();
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
//        Api.Pay("android.test.purchased");
        Api.Pay("com.fantablade.watergun.currency_charge_1001");
    }

    public void ShowProducts()
    {
        var products = Api.GetProducts();
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