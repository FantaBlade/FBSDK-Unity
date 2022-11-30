//
//  WeiboApiManager.h
//  Unity-iPhone
//
//  Created by dravens on 2021/3/26.
//
#import "WeiboSDK.h"
#import "WeiboApiManager.h"
#import "FBSDKiOSApi.h"

@implementation WeiboApiManager

+ (instancetype)sharedManager {
    static dispatch_once_t onceToken;
    static WeiboApiManager *instance = nil;
    dispatch_once(&onceToken, ^{
        instance = [[self alloc]init];
    });
    return instance;
}

- (void)registerApp:(NSString *)appId
        redirectUrl:(NSString *)redirectUrl
{
    [WeiboSDK registerApp:appId universalLink:redirectUrl];
}

- (void)loginWithViewController:(UIViewController *)viewController
{
    [[FBSDKApi sharedInstance] onLoginCallback:@"NotSupport" result:false];
}

- (BOOL)isInstalled
{
    // 未安装会调用网页端
    return TRUE;
}

- (void) shareImage:(NSString *)imagePath
              Title:(NSString *)title
               Desc:(NSString *)desc
{
    UIImage *image = [UIImage imageWithContentsOfFile:imagePath];
    WBImageObject *imageObj = [WBImageObject object];
    imageObj.delegate = self;
    _messageObject = [WBMessageObject message];
    _messageObject.text = desc;
    _messageObject.imageObject = imageObj;
    imageObj.imageData = UIImagePNGRepresentation(image);
    [self realShare];
}

- (void)realShare
{
    WBSendMessageToWeiboRequest *request = [WBSendMessageToWeiboRequest requestWithMessage:_messageObject];
    request.userInfo = @{@"ShareMessageFrom": @"SendMessageToWeiboViewController"};
    [WeiboSDK sendRequest:request completion:^(BOOL success) {
        [[FBSDKApi sharedInstance] onShareCallback:@"" result:success];
    }];
}

/**
 数据准备成功回调
 */
-(void)wbsdk_TransferDidReceiveObject:(id)object
{
    if (![NSThread isMainThread])
    {
        dispatch_async(dispatch_get_main_queue(), ^{
           [self realShare];
        });
    }else{
        [self realShare];
    }
}

/**
 数据准备失败回调
 */
-(void)wbsdk_TransferDidFailWithErrorCode:(WBSDKMediaTransferErrorCode)errorCode andError:(NSError*)error
{
    if (![NSThread isMainThread])
    {
        dispatch_async(dispatch_get_main_queue(), ^{
            [[FBSDKApi sharedInstance] onShareCallback:@"" result:FALSE];
        });
    }else{
        [[FBSDKApi sharedInstance] onShareCallback:@"" result:FALSE];
    }
}

@end
