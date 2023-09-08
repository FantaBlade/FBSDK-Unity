using System.Collections.Generic;
using AnyThinkAds.Api;
using FantaBlade.Mediation;
using FantaBlade.Platform;
using FantaBlade.AD;
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
        FantaBladeMediation.InitializeSuccess += OnInitializeSuccess;
        FantaBladeMediation.LoginSuccess += OnLoginSuccess;
        FantaBladeMediation.LogoutSuccess += OnLogoutSuccess;
        FantaBladeMediation.PaySuccess += OnPaySuccess;
        FantaBladeMediation.PayCancel += OnPayCancel;
        FantaBladeMediation.Init("44I1ucBEaIRvm4Re", true, PublishRegion.LocalDev);
//        Api.Init("zsN9eQcEqcmWnBCT", true, PublishRegion.SoutheastAsia, null, true);

        // 广告
        var placements = new[]
        {
            "b64fac7e9a4918", // 商店获得免费地图
            "b64fac7e94f38c", // 商店获得免费金币
            "b64fac7e8ee753", // 营地挂机结束奖励提
            "b64fac7e895ebc", // 宝箱奖励提高
            "b64fac7e83fdbe", // 副本奖励提高
            "b64fac7e7dab20", // 面包房招募刷新次数
            "b64fac7e785f27", // 高级营地挂机
            "b64d9e14835dc8" // 全军覆没复活
        };
        FantaBladeAD.Init(new FantaBladeADConfig()
        {
            Channel = FantaBladeMediation.Channel,
            DebugMode = true,
            UserId = "AAAAAAA",
            TopOnConfig = new TopOnConfig("a64d9b54901bd3", "9a8753daa6d353462a836351efd577a2")
        });
        FantaBladeAD.Reward += OnReward;
        FantaBladeAD.AddAdPlacementIDs(placements, new Dictionary<string, string>());
    }

    private void OnInitializeSuccess()
    {
        FantaBladePlatform.LoginChannel[] channels = new[]
        {
            FantaBladePlatform.LoginChannel.CHANNEL_WECHAT,
            FantaBladePlatform.LoginChannel.CHANNEL_QQ,
            FantaBladePlatform.LoginChannel.CHANNEL_WEIBO,
            FantaBladePlatform.LoginChannel.CHANNEL_DOUYIN,
            FantaBladePlatform.LoginChannel.CHANNEL_APPLE,
            FantaBladePlatform.LoginChannel.CHANNEL_MOBILE,
        };
        FantaBladePlatform.EnableThirdChannel(channels);
        FantaBladePlatform.Login();
    }

    private void OnLoginSuccess(string token)
    {
        Debug.Log(token);
    }

    private void OnLogoutSuccess()
    {
        FantaBladePlatform.Login();
    }

    private void OnPaySuccess()
    {
        Debug.Log("OnPaySuccess");
    }

    private void OnPayCancel()
    {
        Debug.Log("OnPayCancel");
    }

    private void OnReward(object sender, ATAdEventArgs e)
    {
        Debug.Log(
            $"placementId: {e.placementId},callbackInfo: {e.callbackInfo.getOriginJSONString()}, isTimeout: {e.isTimeout}, isDeeplinkSucceed: {e.isDeeplinkSucceed}");
    }

    public void Pay()
    {
        string orderId = "com.fantablade.watergun.currency_charge_1001";
        FantaBladePlatform.Pay(orderId);
    }

    public void UserCenter()
    {
        FantaBladePlatform.OpenUserCenter();
    }

    public void ShowProducts()
    {
        var products = FantaBladePlatform.GetProducts();
        if (products != null)
        {
            foreach (var product in products)
            {
                LogProduct(product);
            }
        }
    }

    public void ShareImage(int shareChannel)
    {
        string fileName = "share_img.png";
        string path =
            System.IO.Path.Combine(Application.persistentDataPath, fileName);
        UnityEngine.ScreenCapture.CaptureScreenshot(fileName);
        FantaBladeShare.Share(5, path, "", "");
    }

    public void ShowAd()
    {
        var placementId = "b64fac7e895ebc";
        StartCoroutine(FantaBladeAD.ShowAdCoroutine(placementId));
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