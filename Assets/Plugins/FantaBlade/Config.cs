namespace FantaBlade
{
    /// <summary>
    ///     幻刃sdk及三方授权需要的所有参数
    /// </summary>
    public static class Config
    {
        public const string WECHAT_APPID = "wxeacc4ec2f2d5f24e";
        public const string WECHAT_UNIVERSAL_LINK = "https://watergun.hotfix.huanrengame.com/fbsdk/";
        public const string WEIBO_APPID = "1858163759";
        public const string WEIBO_REDIRECTURL = "https://www.fantablade.com/phantomoon/index";
        public const string QQ_APPID = "101940541";
        public const string DOUYIN_CLIENTKEY = "awr89o05lcbk46n2";
#if UNITY_IOS
        public const string MOBILE_SECRETINFO = "e5DDJc9PeHEfZFKbY5aiEpsue6Ty3nzQJle0rQNqSYyzJn0u7nMxctv/xhadaLG1tEZfUBY/XKwETzqj66Is+369WNKT8v7bts/VELGXOxp4gyKmxH2GE6a1whHbIE5hArRJTtwHBDuBAYAZbA3S7N/3zdle1B+ixXUAmAC3/OlETDM4LRqO5toiLbag4VCYYSkhWexfOtQTCMXLWWqpQFSE9TWxMp5JefBDLuAac3cdElmIKa3UXXWWjul0riEl8canBmLQtsllvyUdcPq8Ng==";
#else
        public const string MOBILE_SECRETINFO = "4e0zZH5OoxmyDn0ccbIyMWzi885zteHieCGOLmZ/thqVMl1kjCP/Fqjk1KW+OEaZ2QFS7IbHRBruzPQ0BaFu8JQJcBhFvHBRKpKmCFFUKy3qGU4g71es2pKjwdn2oI2A1Wkf4vNm/BUrD0ZhOarv+8mvHhVvOofYXD29g0Y9fVZnWX9XvH5lMswEnX+CzjJHIgRh8LbaI6u/qImf2BOC8oA3/0QrTSjZKAnp3TBYTn2ZblN1sQ1cYLiVsnccceUKcapiOQFAks3UlOsjgtYb8pf1SDsu9yEN4jaCIkVzyJAE4mGj/tP6Go3A1vKkY9VQ";
#endif
    }
}
