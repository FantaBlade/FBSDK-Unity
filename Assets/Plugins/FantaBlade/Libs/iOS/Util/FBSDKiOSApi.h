//
//  FBSDKiOSApi.h
//
//  Created by dravens on 2021/3/26.
//  Copyright © 2021 Tencent. All rights reserved.
//

#ifndef FBSDKiOSApi_h
#define FBSDKiOSApi_h

@interface FBSDKApi : NSObject
+ (instancetype)sharedInstance;
- (void)login:(int)channel;
- (bool)isInstalled:(int)channel;
- (void)share:(int)channel
      imgPath:(NSString*)imagePath
        Title:(NSString*)title
         Desc:(NSString*)des;
- (void)registerThirdApp:(int)channel
                   AppId:(NSString*)appId
        weiboRedirectUrl:(NSString*)url;
- (void) onLoginCallback:(NSString*)token
                  result:(BOOL)success;
- (void) onShareCallback:(NSString*)token
                  result:(BOOL)success;
- (void) onLogoutCallback:(NSString*)token
                  result:(BOOL)success;
@end

#ifdef __cplusplus
extern "C"{
#endif
typedef void (*DelegateCallbackFunction) (const char* token, bool success);
void fbsdk_setLoginDelegate(DelegateCallbackFunction callback);
void fbsdk_setShareDelegate(DelegateCallbackFunction callback);
void fbsdk_setLogoutDelegate(DelegateCallbackFunction callback);
bool fbsdk_isInstalled(int channel);
void fbsdk_login(int channel);
void fbsdk_logout();
void fbsdk_share(int channel, const char* imagePath, const char* title, const char* desc);
void fbsdk_registerThirdApp(int channel, const char* appId, const char* weiboRedirectUrl);
#ifdef __cplusplus
}
#endif

#endif /* FBSDKiOSApi_h */
