//
//  FBSDKiOSApi.m
//
//  Created by dravens on 2021/3/26.
//  Copyright © 2021 Tencent. All rights reserved.
//
#import "FBSDKiOSApi.h"
#import <Foundation/Foundation.h>
#import "FBSDKAppleLogin.h"
#import "WXApiManager.h"
#import "DouYinApiManager.h"
#import "WeiboApiManager.h"

static int const LOGIN_WECHAT = 1;
static int const LOGIN_QQ = 2;
static int const LOGIN_WEIBO = 3;
static int const LOGIN_DOUYIN = 4;
static int const LOGIN_ALIPAY = 5;
static int const LOGIN_APPLE = 6;

static int const SHARE_WECHAT_SESSION = 1;
static int const SHARE_WECHAT_TIMELINE = 2;
static int const SHARE_WECHAT_FAVORITE = 3;
static int const SHARE_QQ_SESSION = 4;
static int const SHARE_WEIBO = 5;

DelegateCallbackFunction loginCBF = NULL;
DelegateCallbackFunction shareCBF = NULL;
DelegateCallbackFunction logoutCBF = NULL;

@implementation FBSDKApi

+ (instancetype)sharedInstance
{
    static dispatch_once_t onceToken;
    static FBSDKApi *instance = nil;
    dispatch_once(&onceToken, ^{
        instance = [[self alloc]init];
    });
    return instance;
}

- (void)login:(int)channel{
    UIViewController *currentVC = [UIApplication sharedApplication].keyWindow.rootViewController;
    switch (channel) {
        case LOGIN_WECHAT:
            [[WXApiManager sharedManager] sendAuthRequestWithController:currentVC delegate:NULL];
            break;
        case LOGIN_DOUYIN:
            [[DouYinApiManager sharedManager] loginWithViewController:currentVC];
            break;
        case LOGIN_APPLE:
            [[FBSDKAppleLogin shared] loginWithCompleteHandler:^(BOOL successed, NSString * _Nullable user, NSString * _Nullable familyName, NSString * _Nullable givenName, NSString * _Nullable email, NSString * _Nullable password, NSData * _Nullable identityToken, NSData * _Nullable authorizationCode, NSError * _Nullable error, NSString * _Nonnull msg) {
                //NSLog(identityToken);
                if (NULL == identityToken){
                    [[FBSDKApi sharedInstance] onLoginCallback:@"Failed" result:false];
                }else{
                    NSString* idStr = [[NSString alloc] initWithData:identityToken encoding:NSUTF8StringEncoding];
                    NSLog(idStr);
                    [[FBSDKApi sharedInstance] onLoginCallback:idStr result:true];
                }
            }];
            break;
        default:
            break;
    }
}

- (BOOL)isInstalled:(int)channel
{
    switch (channel) {
        case LOGIN_WECHAT:
            return [WXApi isWXAppInstalled];
        case LOGIN_DOUYIN:
            return TRUE;//[[DouYinApiManager sharedManager] isInstalled];
        case LOGIN_APPLE:
            return TRUE;
        case LOGIN_WEIBO:
            return [[WeiboApiManager sharedManager] isInstalled];
        default:
            break;
    }
    return FALSE;
}

- (void)share:(int)channel
      imgPath:(NSString*)imagePath
        Title:(NSString*)title
         Desc:(NSString*)des{
    switch (channel) {
        case SHARE_WECHAT_SESSION:
            [[WXApiManager sharedManager] sendImageData:imagePath Title:title Description:des AtScene:WXSceneSession];
            break;
        case SHARE_WECHAT_TIMELINE:
            [[WXApiManager sharedManager] sendImageData:imagePath Title:title Description:des AtScene:WXSceneTimeline];
            break;
        case SHARE_WEIBO:
            [[WeiboApiManager sharedManager] shareImage:imagePath Title:title Desc:des];
            break;
        default:
            break;
    }
}

- (void)registerThirdApp:(int)channel
                   AppId:(NSString*)appId
        weiboRedirectUrl:(NSString*)url
{
    NSLog(@"channel:%d", channel);
    NSLog(@"appId:%@", appId);
    NSLog(@"appId:%@", url);
    switch (channel) {
        case LOGIN_WECHAT:
            [WXApi registerApp:appId universalLink:url];
            break;
        case LOGIN_DOUYIN:
            [[DouYinApiManager sharedManager] registerApp:appId];
            break;
        case LOGIN_APPLE:
            break;
        case LOGIN_WEIBO:
            [[WeiboApiManager sharedManager] registerApp:appId redirectUrl:url];
            break;
        default:
            break;
    }
}

- (void) onLoginCallback:(NSString*)token
                  result:(BOOL)success
{
    const char* chToken = [FBSDKApi cStringCopy:token];
    loginCBF(chToken, success);
}

- (void) onShareCallback:(NSString*)token
                  result:(BOOL)success
{
    const char* chToken = [FBSDKApi cStringCopy:token];
    shareCBF(chToken, success);
}

- (void) onLogoutCallback:(NSString*)token
                  result:(BOOL)success
{
    const char* chToken = [FBSDKApi cStringCopy:token];
    logoutCBF(chToken, success);
}

+(const char*) cStringCopy:(NSString*)str
{
    return [str UTF8String];
}

// This takes a char* you get from Unity and converts it to an NSString* to use in your objective c code. You can mix c++ and objective c all in the same file.
+(NSString*) CreateNSString:(const char*)string
{
    if (string != NULL){
        NSString* ret = [NSString stringWithUTF8String:string];
        NSLog(@"RET:%@", ret);
        return ret;
    }
    else
    {
        return [NSString stringWithUTF8String:""];
    }
}

#ifdef __cplusplus
extern "C"{
#endif
    
void setLoginDelegate(DelegateCallbackFunction callback)
{
    loginCBF = callback;
}

void setShareDelegate(DelegateCallbackFunction callback)
{
    shareCBF = callback;
}

void setLogoutDelegate(DelegateCallbackFunction callback)
{
    logoutCBF = callback;
}

bool isInstalled(int channel)
{
    return [[FBSDKApi sharedInstance] isInstalled:channel];
}

void login(int channel)
{
    [[FBSDKApi sharedInstance] login:channel];
}

void logout()
{
}

void share(int channel, const char* imagePath, const char* title, const char* desc){
    NSString *nsImagePath = [FBSDKApi CreateNSString:imagePath];
    NSString *nsTitle = [FBSDKApi CreateNSString:title];
    NSString *nsDesc = [FBSDKApi CreateNSString:desc];
    [[FBSDKApi sharedInstance] share:channel imgPath:nsImagePath Title:nsTitle Desc:nsDesc];
}

void registerThirdApp(int channel, const char* appId, const char* weiboRedirectUrl){
    NSString *nsAppId = [FBSDKApi CreateNSString:appId];
    NSString *url = [FBSDKApi CreateNSString:weiboRedirectUrl];
    [[FBSDKApi sharedInstance] registerThirdApp:channel
                                          AppId:nsAppId
                               weiboRedirectUrl:url];
}

#ifdef __cplusplus
}
#endif

@end

