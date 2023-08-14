using System;
using System.Collections;
using System.Collections.Generic;
using AnyThinkAds.Api;
using FantaBlade.Platform.SDK.AD.TopOn;

namespace FantaBlade.Platform.AD
{
    public static class FantaBladeAD
    {
        private static FantaBladeADConfig _config;

        public static event EventHandler<ATAdEventArgs> Reward;

        public static void Init(FantaBladeADConfig config)
        {
            _config = config;

            // TopOn
            TopOnSDK.Init(config);
        }

        public static void AddAdPlacementIDs(string[] placementIds, Dictionary<string, string> extraData)
        {
            ATRewardedAutoVideo.Instance.client.onAdLoadEvent += OnAdLoad;
            ATRewardedAutoVideo.Instance.client.onAdLoadFailureEvent += OnAdLoadFail;
            ATRewardedAutoVideo.Instance.client.onAdVideoStartEvent += OnAdVideoStartEvent;
            ATRewardedAutoVideo.Instance.client.onAdVideoEndEvent += OnAdVideoEndEvent;
            ATRewardedAutoVideo.Instance.client.onAdVideoFailureEvent += OnAdVideoPlayFail;
            ATRewardedAutoVideo.Instance.client.onAdClickEvent += OnAdClick;
            ATRewardedAutoVideo.Instance.client.onRewardEvent += OnReward;
            ATRewardedAutoVideo.Instance.client.onAdVideoCloseEvent += OnAdVideoClosedEvent;

            TopOnSDK.AddAdPlacementIDs(placementIds, extraData);
        }

        public static IEnumerator ShowAdCoroutine(string placementId)
        {
            return TopOnSDK.ShowAdCoroutine(placementId);
        }

        private static void OnAdLoad(object sender, ATAdEventArgs e)
        {
        }

        private static void OnAdLoadFail(object sender, ATAdErrorEventArgs e)
        {
        }

        private static void OnAdVideoStartEvent(object sender, ATAdEventArgs e)
        {
        }

        private static void OnAdVideoEndEvent(object sender, ATAdEventArgs e)
        {
        }

        private static void OnAdVideoPlayFail(object sender, ATAdErrorEventArgs e)
        {
        }

        private static void OnAdClick(object sender, ATAdEventArgs e)
        {
        }

        private static void OnReward(object sender, ATAdEventArgs e)
        {
            if (Reward != null)
            {
                Reward(sender, e);
            }
        }

        private static void OnAdVideoClosedEvent(object sender, ATAdRewardEventArgs e)
        {
        }
    }
}