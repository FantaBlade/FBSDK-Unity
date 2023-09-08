using System.Collections;
using System.Collections.Generic;
using AnyThinkAds.Api;

namespace FantaBlade.AD.TopOn
{
    public static class TopOnSDK
    {
        private static FantaBladeADConfig _config;

        public static void Init(FantaBladeADConfig config)
        {
            _config = config;
            
            ATSDKAPI.setChannel(config.Channel);

            ATSDKAPI.setSubChannel(config.SubChannel);

            ATSDKAPI.setLogDebug(config.DebugMode);

            if (config.TopOnConfig != null)
            {
                ATSDKAPI.initSDK(config.TopOnConfig.AppId, config.TopOnConfig.AppKey);
            }
        }

        public static void AddAdPlacementIDs(string[] placementIds, Dictionary<string, string> extraData)
        {
            SetAutoLoadExtra(placementIds, extraData);
            ATRewardedAutoVideo.Instance.addAutoLoadAdPlacementID(placementIds);
        }

        public static bool IsReadyFor(string placementId)
        {
            return ATRewardedAutoVideo.Instance.autoLoadRewardedVideoReadyForPlacementID(placementId);
        }
        
        public static void ShowAd(string placementId)
        {
            ATRewardedAutoVideo.Instance.showAutoAd(placementId);
        }

        public static IEnumerator ShowAdCoroutine(string placementId)
        {
            while (!IsReadyFor(placementId))
            {
                yield return null;
            }
            ShowAd(placementId);
        }

        private static void SetAutoLoadExtra(string[] placementIds, Dictionary<string, string> extraDataDic)
        {
            Dictionary<string, string> normalJsonMap = new Dictionary<string, string>();
            //如果需要通过开发者的服务器进行奖励的下发（部分广告平台支持此服务器激励），则需要传递下面两个key
            //ATConst.USERID_KEY必传，用于标识每个用户;ATConst.USER_EXTRA_DATA为可选参数，传入后将透传到开发者的服务器
            normalJsonMap.Add(ATConst.USERID_KEY, _config.UserId);
            normalJsonMap.Add(ATConst.USER_EXTRA_DATA, "");

            foreach (string placementId in placementIds)
            {
                if (extraDataDic.TryGetValue(placementId, out var extraData))
                {
                    Dictionary<string, string> jsonMap = new Dictionary<string, string>();
                    //如果需要通过开发者的服务器进行奖励的下发（部分广告平台支持此服务器激励），则需要传递下面两个key
                    //ATConst.USERID_KEY必传，用于标识每个用户;ATConst.USER_EXTRA_DATA为可选参数，传入后将透传到开发者的服务器
                    jsonMap.Add(ATConst.USERID_KEY, _config.UserId);
                    jsonMap.Add(ATConst.USER_EXTRA_DATA, extraData);
                    ATRewardedAutoVideo.Instance.setAutoLocalExtra(placementId, jsonMap);
                }
                else
                {
                    ATRewardedAutoVideo.Instance.setAutoLocalExtra(placementId, normalJsonMap);
                }
            }
        }
    }
}