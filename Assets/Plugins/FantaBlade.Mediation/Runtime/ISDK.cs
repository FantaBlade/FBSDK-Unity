namespace FantaBlade.Mediation
{
    public interface ISDK
    {
        /// <summary>
        ///     渠道名称
        /// </summary>
        /// <returns></returns>
        string GetChannel();
    
        /// <summary>
        ///     是否实名认证
        /// </summary>
        /// <returns></returns>
        bool IsVerify();

        int Age();

        void Login(bool forceShowUi = false, bool useQuickLoginFirstTime = false);

        void Logout();

        void OpenUserCenter();
    }
}