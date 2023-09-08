namespace FantaBlade.AD
{
    public class FantaBladeADConfig
    {
        public string Channel;

        public string SubChannel;
        
        public bool DebugMode;

        public string UserId;

        public TopOnConfig TopOnConfig;
    }
    
    public class TopOnConfig
    {
        public string AppId;

        public string AppKey;

        public TopOnConfig(string appId, string appKey)
        {
            AppId = appId;
            AppKey = appKey;
        }
    }
}