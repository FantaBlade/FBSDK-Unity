using FbSdk;
using UnityEngine;

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
        Sdk.Init("44I1ucBEaIRvm4Re");
        Sdk.LoginSuccess += OnLoginSuccess;
        Sdk.PaySuccess += OnPaySuccess;
        Sdk.PayCancel += OnPayCancel;
        Sdk.Login();
    }

    private void OnLoginSuccess(string token)
    {
        Debug.Log(token);
    }

    private void OnPaySuccess()
    {
        Debug.Log("OnPaySuccess");
    }

    private void OnPayCancel()
    {
        Debug.Log("OnPayCancel");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("pay", GUILayout.Width(300), GUILayout.Height(300))) Sdk.Pay("currency_charge_1001", "充值6元", 6);
    }
}