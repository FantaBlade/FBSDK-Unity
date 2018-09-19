namespace FbSdk.Internal.Native
{
    public interface IAndroidSdkCallback
    {
        /// <summary>
        ///     登陆成功
        /// </summary>
        /// <param name="token">token</param>
        void onLoginSuccess(string token);

        /// <summary>
        ///     登陆失败
        /// </summary>
        /// <param name="msg">error message</param>
        void onLoginFailure(string msg);

        /// <summary>
        ///     登陆取消
        /// </summary>
        void onLoginCancel();

        /// <summary>
        ///     登出成功
        /// </summary>
        void onLogoutSuccess();

        /// <summary>
        ///     支付成功
        /// </summary>
        void onPaySuccess();

        /// <summary>
        ///     支付失败
        /// </summary>
        /// <param name="msg">msg error message</param>
        void onPayFailure(string msg);

        /// <summary>
        ///     支付取消
        /// </summary>
        void onPayCancel();
    }
}