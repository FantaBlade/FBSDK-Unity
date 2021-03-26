//
//  DouYinApiManager.m
//  Unity-iPhone
//
//  Created by dravens on 2021/3/26.
//
#import "DouYinApiManager.h"
#import "FBSDKiOSApi.h"

@implementation DouYinApiManager

+ (instancetype)sharedManager {
    static dispatch_once_t onceToken;
    static DouYinApiManager *instance = nil;
    dispatch_once(&onceToken, ^{
        instance = [[self alloc]init];
    });
    return instance;
}

- (void)registerApp:(NSString *)appId
{
    [[DouyinOpenSDKApplicationDelegate sharedInstance] registerAppId:appId];
}

- (void)loginWithViewController:(UIViewController *)viewController
{
    DouyinOpenSDKAuthRequest *request = [[DouyinOpenSDKAuthRequest alloc] init];
    request.permissions = [NSOrderedSet orderedSetWithObject:@"user_info"];
    //可选附加权限（如有），用户可选择勾选/不勾选
//    request.additionalPermissions = [NSOrderedSet orderedSetWithObjects:@{@"permission":@"friend_relation",@"defaultChecked":@"1"}, @{@"permission":@"message",@"defaultChecked":@"0"}, nil];
    
    [request sendAuthRequestViewController:viewController completeBlock:^(DouyinOpenSDKAuthResponse * _Nonnull resp) {
//        NSString *alertString = nil;
        if (resp.errCode == 0) {
//            alertString = [NSString stringWithFormat:@"Author Success Code : %@, permission : %@",resp.code, resp.grantedPermissions];
            [[FBSDKApi sharedInstance] onLoginCallback:resp.code result:TRUE];
        } else{
//            alertString = [NSString stringWithFormat:@"Author failed code : %@, msg : %@",@(resp.errCode), resp.errString];
            [[FBSDKApi sharedInstance] onLoginCallback:resp.errString result:FALSE];
        }
    }];
}

- (BOOL)isInstalled
{
    return [[DouyinOpenSDKApplicationDelegate sharedInstance] isAppInstalled];
}

@end
