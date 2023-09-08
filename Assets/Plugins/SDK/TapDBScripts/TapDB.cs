#define DEBUG_IN_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TapDBMiniJSON;
using System;
using TapTap.TapDB.PC;

// version 3.1.3

public class TapDB
{
#if UNITY_IOS && !UNITY_EDITOR
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeEnableLog(bool enable);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetAdvertiserIDCollectionEnabled(bool enable);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetHost(string host);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetCustomEventHost(string host);
	
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeOnStart(string appId, string channel, string gameVersion);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeOnStartWithProperties(string appId, string channel, string gameVersion,string propertiesString);
	
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetUser(string userId);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetUserWithProperties(string userId,string propertiesString);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeClearUser();

	[DllImport ("__Internal")]
	public static extern string TapDB_getDeviceId();
	
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetLevel(int level);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetName(string name);
	
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeSetServer(string server);
	
	[DllImport ("__Internal")]
	public static extern void TapDB_nativeOnChargeSuccess(string orderId, string product, Int32 amount, string currencyType, string payment);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeOnChargeSuccessWithProperties(string orderId, string product, Int32 amount, string currencyType, string payment, string properties);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeOnEvent(string eventCode, string properties);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeTrackEvent(string eventName, string properties);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeDeviceInitialize(string propertiesString);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeDeviceUpdate(string propertiesString);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeDeviceAdd(string propertiesString);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeUserUpdate(string propertiesString);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeUserInitialize(string propertiesString);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeUserAdd(string propertiesString);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeRegisterStaticProperties(string propertiesString);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeUnregisterStaticProperty(string propertyName);

	[DllImport ("__Internal")]
	public static extern void TapDB_nativeClearStaticProperties();

#elif UNITY_ANDROID && !UNITY_EDITOR
	public static string JAVA_CLASS = "com.tds.tapdb.sdk.TapDB";
	private static string UNTIFY_CLASS = "com.unity3d.player.UnityPlayer";
	private static AndroidJavaClass agent = null;
	private static AndroidJavaClass unityClass = null;

	private static AndroidJavaClass getAgent() {
		if (agent == null) {
			agent = new AndroidJavaClass(JAVA_CLASS);
		}
		return agent;
	}

	private static AndroidJavaObject getJsonObject(string properties)
    {
		if(properties == null)
        {
			return null;
        }
		return new AndroidJavaObject("org.json.JSONObject", properties);
    }

	private static AndroidJavaClass getUnityClass(){
		if (unityClass == null) {
			unityClass = new AndroidJavaClass(UNTIFY_CLASS);
		}
		return unityClass;
	}

	private static void TapDB_nativeInit(string appId, string channel, string gameVersion,string propertiesString){
		AndroidJavaObject activity = getUnityClass().GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject jsonObject = getJsonObject(propertiesString);
		getAgent().CallStatic("init", activity, appId, channel, gameVersion, jsonObject);
        
	}
	
#endif

	/**
		是否打开广告标识符（IDFA）收集,默认关闭。仅 iOS
	 */
	public static void enableAdvertiserIDCollection(bool enable) {
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeSetAdvertiserIDCollectionEnabled(enable);
#elif UNITY_ANDROID && !UNITY_EDITOR

#endif
	}

	/**
		是否打开 SDK 日志输出
	 */
	public static void enableLog(bool enable) {
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeEnableLog(enable);
#elif UNITY_ANDROID && !UNITY_EDITOR
		getAgent().CallStatic("enableLog", enable);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.EnableLog(enable);
#endif
	}
	/**
	 * 调用该接口修改数据发送的域名，有特殊需要时调用，调用必须位于初始化之前
	 * 域名必须是https://abc.example.com/的格式，不能为空
	 */
	public static void setHost(string host){
		if (host == null) {
			host = "";
		}
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeSetHost(host);
#elif UNITY_ANDROID && !UNITY_EDITOR
		getAgent().CallStatic("setHost", host);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
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
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeSetCustomEventHost(host);
#elif UNITY_ANDROID && !UNITY_EDITOR
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
		TapDB.onStartWithProperties(appId,channel,gameVersion,null);
	}

		/**
	 * 初始化，尽早调用
	 * appId: TapDB注册得到的appId
	 * channel: 分包渠道名称，可为空
	 * gameVersion: 游戏版本，可为空，为空时，自动获取游戏安装包的版本
	 * properties: 自定义属性，随初始化上传
	 */
	public static void onStartWithProperties(string appId, string channel, string gameVersion,Dictionary<string, object> properties){
		if (appId == null) {
			appId = "";
		}
		if (channel == null) {
			channel = "";
		}
		if (gameVersion == null) {
			gameVersion = "";
		}
		Debug.Log("TapDB init --- start --");
		string stringProperties = TapDB.jsonString(properties);
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeOnStartWithProperties(appId, channel, gameVersion,stringProperties);
#elif UNITY_ANDROID && !UNITY_EDITOR
		TapDB_nativeInit(appId, channel, gameVersion,stringProperties);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.InitWithAppId(appId, channel, gameVersion);
#endif
	}

	/**
	 * 记录一个用户（注意是平台用户，不是游戏角色！！！！），需要保证唯一性
	 * userId: 用户的ID（注意是平台用户ID，不是游戏角色ID！！！！），如果是匿名用户，由游戏生成，需要保证不同平台用户的唯一性
	 */
	public static void setUser(string userId){
		// TapDB.setUserWithProperties(userId,null);
		if (userId == null) {
			userId = "";
		}
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeSetUser(userId);
#elif UNITY_ANDROID && !UNITY_EDITOR
		getAgent().CallStatic("setUser", userId);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.SetUser(userId);
#endif
	}

	/**
	 * 记录一个用户（注意是平台用户，不是游戏角色！！！！），需要保证唯一性
	 * userId: 用户的ID（注意是平台用户ID，不是游戏角色ID！！！！），如果是匿名用户，由游戏生成，需要保证不同平台用户的唯一性
	 * properties: 自定义属性，随用户事件上传
	 */
	// 	public static void setUserWithProperties(string userId,Dictionary<string, object> properties){
	// 		if (userId == null) {
	// 			userId = "";
	// 		}

	// 		if (properties == null) {
	// 			properties = new Dictionary<string, object>();
	// 		}
	// 		string stringProperties = Json.Serialize(properties);
	// #if UNITY_IOS
	// 		TapDB_nativeSetUserWithProperties(userId,stringProperties);
	// #elif UNITY_ANDROID
	//      AndroidJavaObject jsonObject = getJsonObject(stringProperties);
	// 		getAgent().CallStatic("setUser", userId, jsonObject);
	// #elif UNITY_STANDALONE_WIN
	// 		UnitySetUser(userId, "");
	// #endif	
	// 	}

	/**
		退出登录时清理用户
	 */
	public static void clearUser() {
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeClearUser();
#elif UNITY_ANDROID && !UNITY_EDITOR
		getAgent().CallStatic("clearUser");
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.ClearUser();
#endif
	}


	/**
	 * 设置用户等级，初次设置时或升级时调用
	 * level: 等级
	 */
	public static void setLevel(int level){
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeSetLevel(level);
#elif UNITY_ANDROID && !UNITY_EDITOR
		getAgent().CallStatic("setLevel", level);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.SetLevel(level);
#endif
	}

	/**
	 * 设置用户名
	 * name: 用户名
	 */
	public static void setName(string name){
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeSetName(name);
#elif UNITY_ANDROID && !UNITY_EDITOR
		getAgent().CallStatic("setName", name);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.SetName(name);
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
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeSetServer(server);
#elif UNITY_ANDROID && !UNITY_EDITOR
		getAgent().CallStatic("setServer", server);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.SetServer(server);
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
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeOnChargeSuccess(orderId,product,amount,currencyType,payment);
#elif UNITY_ANDROID && !UNITY_EDITOR
		getAgent().CallStatic("onCharge", orderId,product,(long)amount,currencyType,payment);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.OnCharge(orderId, product, amount, currencyType, payment);
#endif
	}

	public static void onChargeSuccessWithProperties(string orderId, string product, Int32 amount, string currencyType, string payment, Dictionary<string, object> properties)
	{
		if (properties == null)
		{
			properties = new Dictionary<string, object>();
		}
		string stringProperties = Json.Serialize(properties);

#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeOnChargeSuccessWithProperties(orderId,product,amount,currencyType,payment,stringProperties);
#elif UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject jsonObject = getJsonObject(stringProperties);
		getAgent().CallStatic("onCharge", orderId,product,(long)amount,currencyType,payment,jsonObject);
#endif
	}

	/**
	 * 自定义事件
	 * eventCode: 事件代码，需要在控制后台预先进行配置
	 * properties: 事件属性，需要在控制后台预先进行配置
	 */
	[Obsolete("已弃用,请调用trackEvent(string eventName, Dictionary<string, object> properties)")]
	public static void onEvent(string eventCode, Dictionary<string, object> properties) {
		if (eventCode == null) {
			eventCode = "";
		}
		if (properties == null) {
			properties = new Dictionary<string, object>();
		}
		string stringProperties = Json.Serialize(properties);

#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeOnEvent(eventCode, stringProperties);
#elif UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject jsonObject = getJsonObject(stringProperties);
		getAgent().CallStatic("onEvent", eventCode, jsonObject);
#endif
	}

	/**
	 * 自定义事件
	 * eventName: 事件名
	 * properties: 事件属性
	 */
	public static void trackEvent(string eventName, Dictionary<string, object> properties) {
		if (eventName == null) {
			eventName = "";
		}

		string stringProperties = TapDB.jsonString(properties);
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeTrackEvent(eventName, stringProperties);
#elif UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject jsonObject = getJsonObject(stringProperties);
		getAgent().CallStatic("trackEvent", eventName, jsonObject);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.TrackEvent(eventName, properties);
#endif
	}

	/// 初始化设备属性操作
	/// @param properties 属性字典
	public static void deviceInitialize(Dictionary<string, object> properties) {
		string stringProperties = TapDB.jsonString(properties);
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeDeviceInitialize(stringProperties);
#elif UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject jsonObject = getJsonObject(stringProperties);
		getAgent().CallStatic("deviceInitialize", jsonObject);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.DeviceInitialize(properties);
#endif
	}

	/// 更新设备属性操作
	/// @param properties 属性字典
	public static void deviceUpdate(Dictionary<string, object> properties) {
		string stringProperties = TapDB.jsonString(properties);
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeDeviceUpdate(stringProperties);
#elif UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject jsonObject = getJsonObject(stringProperties);
		getAgent().CallStatic("deviceUpdate", jsonObject);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.DeviceUpdate(properties);
#endif
	}

	/// 设备属性增加操作
	/// @param properties 属性字典 暂时只支持数字属性
	public static void deviceAdd(Dictionary<string, object> properties) {
		string stringProperties = TapDB.jsonString(properties);
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeDeviceAdd(stringProperties);
#elif UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject jsonObject = getJsonObject(stringProperties);
		getAgent().CallStatic("deviceAdd", jsonObject);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.DeviceAdd(properties);
#endif
	}

	/// 初始化用户属性操作
	/// @param properties 属性字典
	public static void userInitialize(Dictionary<string, object> properties) {
		string stringProperties = TapDB.jsonString(properties);
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeUserInitialize(stringProperties);
#elif UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject jsonObject = getJsonObject(stringProperties);
		getAgent().CallStatic("userInitialize", jsonObject);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.UserInitialize(properties);
#endif
	}

	/// 更新用户属性操作
	/// @param properties 属性字典
	public static void userUpdate(Dictionary<string, object> properties) {
		string stringProperties = TapDB.jsonString(properties);
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeUserUpdate(stringProperties);
#elif UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject jsonObject = getJsonObject(stringProperties);
		getAgent().CallStatic("userUpdate", jsonObject);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.UserUpdate(properties);
#endif
	}

	/// 用户属性增加操作
	/// @param properties 属性字典 暂时只支持数字属性
	public static void userAdd(Dictionary<string, object> properties) {
		string stringProperties = TapDB.jsonString(properties);
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeUserAdd(stringProperties);
#elif UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject jsonObject = getJsonObject(stringProperties);
		getAgent().CallStatic("userAdd", jsonObject);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.UserAdd(properties);
#endif
	}

	/// 添加静态事件属性，每个事件都将会发送
	/// @param staticProperties 属性字典
	public static void registerStaticProperties(Dictionary<string, object> properties) {
		string stringProperties = TapDB.jsonString(properties);
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeRegisterStaticProperties(stringProperties);
#elif UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject jsonObject = getJsonObject(stringProperties);
		getAgent().CallStatic("registerStaticProperties", jsonObject);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.RegisterStaticProperties(properties);
#endif
	}

	/// 删除添加的静态事件属性
	public static void clearStaticProperties() {
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeClearStaticProperties();
#elif UNITY_ANDROID && !UNITY_EDITOR
		getAgent().CallStatic("clearStaticProperties");
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.ClearStaticProperties();
#endif
	}

	/// 删除添加的某个静态事件属性
	/// @param propertyName 属性Key
	public static void unregisterStaticProperty(string propertyName) {
		// TODO
#if UNITY_IOS && !UNITY_EDITOR
		TapDB_nativeUnregisterStaticProperty(propertyName);
#elif UNITY_ANDROID && !UNITY_EDITOR
		getAgent().CallStatic("unregisterStaticProperty", propertyName);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || (UNITY_EDITOR && DEBUG_IN_EDITOR)
		TapDBPC.UnregisterStaticProperty(propertyName);
#endif
	}

	public static void closeFetchTapTapDeviceId()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		getAgent().CallStatic("closeFetchTapTapDeviceId");
#endif
	}




	public static string jsonString(Dictionary<string, object> properties) {
		if (properties == null) {
			properties = new Dictionary<string, object>();
		}
		string stringProperties = Json.Serialize(properties);

		return stringProperties;
	}
 
}


