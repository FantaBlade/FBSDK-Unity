//
//  TencentQQManagerApi.m
//  Unity-iPhone
//
//  Created by dravens on 2021/4/12.
//

#import <Foundation/Foundation.h>
#import "TencentOPenApi/QQApiInterface.h"
#import "TencentOPenApi/QQApiInterfaceObject.h"
#import "TencentQQManagerApi.h"
#import "FBSDKiOSApi.h"

@implementation TencentQQManagerApi

TencentOAuth *_tencentOAuth;

+ (instancetype)sharedManager {
    static dispatch_once_t onceToken;
    static TencentQQManagerApi *instance = nil;
    dispatch_once(&onceToken, ^{
        instance = [[self alloc]init];
    });
    return instance;
}

- (void)registerApp:(NSString *)appId
{
    _tencentOAuth = [[TencentOAuth alloc] initWithAppId:appId andDelegate:self];
}

- (void)loginWithViewController:(UIViewController *)viewController
{
    NSArray *permission = [NSArray arrayWithObjects:@"all", nil];
    [_tencentOAuth authorize:permission];
}

- (void)shareImage:(NSString *)localImagePath
             Title:(NSString *)title
              Desc:(NSString *)des
{
    NSData *imgData = [NSData dataWithContentsOfFile:localImagePath];
    QQApiImageObject *imgObj = [QQApiImageObject objectWithData:imgData previewImageData:imgData title:title description:des];
    SendMessageToQQReq *req = [SendMessageToQQReq reqWithContent:imgObj];
    QQApiSendResultCode sendCode = [QQApiInterface sendReq: req];
    if(EQQAPISENDSUCESS == sendCode){
        [[FBSDKApi sharedInstance] onShareCallback:@"" result:true];
    }
}

- (BOOL)isInstalled
{
    return [TencentOAuth iphoneQQInstalled] || [TencentOAuth iphoneTIMInstalled];
}

- (void)tencentDidLogin {
    //
    [[FBSDKApi sharedInstance] onLoginCallback:_tencentOAuth.accessToken result:true];
}

- (void)tencentDidNotLogin:(BOOL)cancelled {
    if(cancelled){
        [[FBSDKApi sharedInstance] onLoginCallback:@"UserCancel" result:false];
    }else{
        [[FBSDKApi sharedInstance] onLoginCallback:@"Error" result:false];
    }
}

- (void)tencentDidNotNetWork {
    [[FBSDKApi sharedInstance] onLoginCallback:@"Error" result:false];
}

@end
