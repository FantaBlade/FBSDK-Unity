using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TapDBMiniJSON;
using System;

// version 2.1.3

// 帐户类型
public enum TGTUserType{
	TGTTypeAnonymous = 0, // 匿名用户
	TGTTypeRegistered = 1,// 注册用户
}

// 用户性别
public enum TGTUserSex{
	TGTSexMale = 0, // 男性
	TGTSexFemale = 1, // 女性
	TGTSexUnknown = 2, // 性别未知
}

public class TapDB
{
#if UNITY_IOS
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetHost(string host);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetCustomEventHost(string host);
	
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeOnStart(string appId, string channel, string gameVersion);
	
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetUser(string userId);
	
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetLevel(int level);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetName(string name);
	
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetServer(string server);
	
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeOnChargeRequest(string orderId, string product, Int32 amount, string currencyType, string payment);
	
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeOnChargeSuccess(string orderId, string product, Int32 amount, string currencyType, string payment);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeDeprecatedOnChargeSuccess(string orderId);
	
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeOnChargeFail(string orderId, string reason);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeOnChargeOnlySuccess(string orderId, string product, Int32 amount, string currencyType, Int32 virtualCurrencyAmount, string payment);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeOnEvent(string eventCode, string properties);

#elif UNITY_ANDROID
	public static string JAVA_CLASS = "com.tapdb.sdk.TapDB";
	private static string UNTIFY_CLASS = "com.unity3d.player.UnityPlayer";
	private static AndroidJavaClass agent = null;
	private static AndroidJavaClass unityClass = null;

	private static AndroidJavaClass getAgent() {
		if (agent == null) {
			agent = new AndroidJavaClass(JAVA_CLASS);
		}
		return agent;
	}

	private static AndroidJavaClass getUnityClass(){
		if (unityClass == null) {
			unityClass = new AndroidJavaClass(UNTIFY_CLASS);
		}
		return unityClass;
	}

	private static void TapDB_nativeInit(string appId, string channel, string gameVersion){

		AndroidJavaObject activity = getUnityClass().GetStatic<AndroidJavaObject>("currentActivity");
		getAgent().CallStatic("init", activity, appId, channel, gameVersion);
	}
	
	private static void TapDB_nativeOnResume(){
		AndroidJavaObject activity = getUnityClass().GetStatic<AndroidJavaObject>("currentActivity");
		getAgent().CallStatic("onResume", activity);
	}
	
	private static void TapDB_nativeOnStop(){
		AndroidJavaObject activity = getUnityClass().GetStatic<AndroidJavaObject>("currentActivity");
		getAgent().CallStatic("onStop", activity);
	}

#elif UNITY_STANDALONE_WIN
	[DllImport ("TapDB")]
	public static extern void UnitySetHost(string host);
	
	[DllImport ("TapDB")]
	public static extern void UnityOnStart(string appId, string channel, string version);
	
	[DllImport ("TapDB")]
	public static extern void UnitySetUser(string userId, string userName);
	
	[DllImport ("TapDB")]
	public static extern void UnitySetLevel(int level);
	
	[DllImport ("TapDB")]
	public static extern void UnitySetServer(string server);

	[DllImport ("TapDB")]
	public static extern void UnityOnCharge(string orderId, string product, int amount, string currencyType, int virtualCurrencyAmount, string payment);

#endif


	/**
	 * 调用该接口修改数据发送的域名，有特殊需要时调用，调用必须位于初始化之前
	 * 域名必须是https://abc.example.com/的格式，不能为空
	 */
	public static void setHost(string host){
		if (host == null) {
			host = "";
		}
#if UNITY_IOS
		TapDB_nativeSetHost(host);
#elif UNITY_ANDROID
		getAgent().CallStatic("setHost", host);
#elif UNITY_STANDALONE_WIN
		UnitySetHost(host);
#endif
	}

	/**
	 * 调用该接口修改数据发送的域名，有特殊需要时调用，调用必须位于初始化之前
	 * 域名必须是https://abc.example.com/的格式，不能为空
	 */
	public static void setCustomEventHost(string host){
		if (host == null) {
			host = "";
		}
#if UNITY_IOS
		TapDB_nativeSetCustomEventHost(host);
#elif UNITY_ANDROID
		getAgent().CallStatic("setCustomEventHost", host);
#endif
	}

	/**
	 * 初始化，尽早调用
	 * appId: TapDB注册得到的appId
	 * channel: 分包渠道名称，可为空
	 * gameVersion: 游戏版本，可为空，为空时，自动获取游戏安装包的版本
	 */
	public static void onStart(string appId, string channel, string gameVersion){
		if (appId == null) {
			appId = "";
		}
		if (channel == null) {
			channel = "";
		}
		if (gameVersion == null) {
			gameVersion = "";
		}
#if UNITY_IOS
		TapDB_nativeOnStart(appId, channel, gameVersion);
#elif UNITY_ANDROID
		TapDB_nativeInit(appId, channel, gameVersion);
		TapDB_nativeOnResume();
#elif UNITY_STANDALONE_WIN
		UnityOnStart(appId, channel, gameVersion);
#endif	
	}

	public static void onResume(){
#if UNITY_ANDROID
		TapDB_nativeOnResume();
#endif
	}

	public static void onStop(){
#if UNITY_ANDROID
		TapDB_nativeOnStop();
#endif
	}

	/**
	 * 记录一个用户（注意是平台用户，不是游戏角色！！！！），需要保证唯一性
	 * userId: 用户的ID（注意是平台用户ID，不是游戏角色ID！！！！），如果是匿名用户，由游戏生成，需要保证不同平台用户的唯一性
	 * userType: 用户类型
	 * userSex: 用户性别
	 * userAge: 用户年龄，年龄未知传递0
	 */
	[Obsolete("接口已弃用,调用setUser(string userId)")]
	public static void setUser(string userId, TGTUserType userType, TGTUserSex userSex, int userAge, string userName){
		if (userId == null) {
			userId = "";
		}
		if (userName == null) {
			userName = "";
		}
#if UNITY_IOS
		TapDB_nativeSetUser(userId);
		TapDB_nativeSetName(userName);
#elif UNITY_ANDROID
		getAgent().CallStatic("setUser", userId);
		getAgent().CallStatic("setName", userName);
#elif UNITY_STANDALONE_WIN
		UnitySetUser(userId, userName);
#endif	
	}

	/**
	 * 记录一个用户（注意是平台用户，不是游戏角色！！！！），需要保证唯一性
	 * userId: 用户的ID（注意是平台用户ID，不是游戏角色ID！！！！），如果是匿名用户，由游戏生成，需要保证不同平台用户的唯一性
	 */
	public static void setUser(string userId){
		if (userId == null) {
			userId = "";
		}

#if UNITY_IOS
		TapDB_nativeSetUser(userId);
#elif UNITY_ANDROID
		getAgent().CallStatic("setUser", userId);
#elif UNITY_STANDALONE_WIN
		UnitySetUser(userId, "");
#endif	
	}

	/**
	 * 设置用户等级，初次设置时或升级时调用
	 * level: 等级
	 */
	public static void setLevel(int level){
#if UNITY_IOS
		TapDB_nativeSetLevel(level);
#elif UNITY_ANDROID
		getAgent().CallStatic("setLevel", level);
#elif UNITY_STANDALONE_WIN
		UnitySetLevel(level);
#endif	
	}

	/**
	 * 设置用户名
	 * name: 用户名
	 */
	public static void setName(string name){
#if UNITY_IOS
		TapDB_nativeSetName(name);
#elif UNITY_ANDROID
		getAgent().CallStatic("setName", name);
#elif UNITY_STANDALONE_WIN

#endif	
	}

	/**
	 * 设置用户服务器，初次设置或更改服务器的时候调用
	 * server: 服务器
	 */
	public static void setServer(string server){
		if (server == null) {
			server = "";
		}
#if UNITY_IOS
		TapDB_nativeSetServer(server);
#elif UNITY_ANDROID
		getAgent().CallStatic("setServer", server);
#elif UNITY_STANDALONE_WIN
		UnitySetServer(server);
#endif	
	}

	/**
	 * 发起充值请求时调用
	 * orderId: 订单ID，不能为空
	 * product: 产品名称，可为空
	 * amount: 充值金额（分）
	 * currencyType: 货币类型，可为空，参考：人民币 CNY，美元 USD；欧元 EUR
	 * virtualCurrencyAmount: 充值获得的虚拟币
	 * payment: 支付方式，可为空，如：支付宝
	 */
	 [Obsolete("接口已弃用")]
	public static void onChargeRequest(string orderId, string product, Int32 amount, string currencyType, Int32 virtualCurrencyAmount, string payment){
#if UNITY_IOS
		TapDB_nativeOnChargeRequest(orderId, product, amount, currencyType, payment);
#elif UNITY_ANDROID
		getAgent().CallStatic("onChargeRequest", orderId, product, (long)amount, currencyType, (long)virtualCurrencyAmount, payment);
#endif	
	}

	/**
	 * 充值成功时调用
	 * orderId: 订单ID，不能为空，与上一个接口的orderId对应
	 */
	 [Obsolete("已弃用,请调用onChargeSuccess(string orderId, string product, Int32 amount, string currencyType, string payment)")]
	public static void onChargeSuccess(string orderId){
#if UNITY_IOS
		TapDB_nativeDeprecatedOnChargeSuccess(orderId);
#elif UNITY_ANDROID
		getAgent().CallStatic("onChargeSuccess", orderId);
#endif	
	}

	/**
	 * 充值成功时调用
	 * orderId: 订单ID，不能为空，与上一个接口的orderId对应
	* product: 产品名称，可为空
	* amount: 充值金额（单位分，即无论什么币种，都需要乘以100）
	* currencyType: 货币类型，可为空，参考：人民币 CNY，美元 USD；欧元 EUR
	* payment: 支付方式，可为空，如：支付宝
	 */
	public static void onChargeSuccess(string orderId, string product, Int32 amount, string currencyType, string payment){
#if UNITY_IOS
		TapDB_nativeOnChargeSuccess(orderId,product,amount,currencyType,payment);
#elif UNITY_ANDROID
		getAgent().CallStatic("onCharge", orderId,product,(long)amount,currencyType,payment);
#endif	
	}

	/**
	 * 充值失败时调用
	 * orderId: 订单ID，不能为空，与上一个接口的orderId对应
	 * reason: 失败原因，可为空
	 */
	 [Obsolete("接口已弃用")]
	public static void onChargeFail(string orderId, string reason){
#if UNITY_IOS
		TapDB_nativeOnChargeFail(orderId, reason);
#elif UNITY_ANDROID
		getAgent().CallStatic("onChargeFail", orderId, reason);
#endif	
	}

	/**
	 * 当客户端无法跟踪充值请求发起，只能跟踪到充值成功的事件时，调用该接口记录充值信息
	 * orderId: 订单ID，可为空
	 * product: 产品名称，可为空
	 * amount: 充值金额（单位分，即无论什么币种，都需要乘以100）
	 * currencyType: 货币类型，可为空，参考：人民币 CNY，美元 USD；欧元 EUR
	 * virtualCurrencyAmount: 充值获得的虚拟币
	 * payment: 支付方式，可为空，如：支付宝
	 */
	 [Obsolete("已弃用,请调用onChargeSuccess:product:amount:currencyType:payment")]
	public static void onChargeOnlySuccess(string orderId, string product, Int32 amount, string currencyType, Int32 virtualCurrencyAmount, string payment){
		if (orderId == null) {
			orderId = "";
		}
		if (product == null) {
			product = "";
		}
		if (currencyType == null) {
			currencyType = "";
		}
		if (payment == null) {
			payment = "";
		}
#if UNITY_IOS
		TapDB_nativeOnChargeOnlySuccess(orderId, product, amount, currencyType, virtualCurrencyAmount, payment);
#elif UNITY_ANDROID
		getAgent().CallStatic("onChargeOnlySuccess", orderId, product, (long)amount, currencyType, (long)virtualCurrencyAmount, payment);
#elif UNITY_STANDALONE_WIN
		UnityOnCharge(orderId, product, amount, currencyType, virtualCurrencyAmount, payment);
#endif	
	}

	/**
	 * 自定义事件
	 * eventCode: 事件代码，需要在控制后台预先进行配置
	 * properties: 事件属性，需要在控制后台预先进行配置
	 */
	public static void onEvent(string eventCode, Dictionary<string, object> properties) {
		if (eventCode == null) {
			eventCode = "";
		}
		if (properties == null) {
			properties = new Dictionary<string, object>();
		}
		string stringProperties = Json.Serialize(properties);

#if UNITY_IOS
		TapDB_nativeOnEvent(eventCode, stringProperties);
#elif UNITY_ANDROID
		getAgent().CallStatic("onEventDeprecated", eventCode, stringProperties);
#endif	
	}

}


