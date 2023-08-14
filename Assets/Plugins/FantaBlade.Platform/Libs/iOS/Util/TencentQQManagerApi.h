//
//  TencentQQManagerApi.h
//  Unity-iPhone
//
//  Created by dravens on 2021/4/12.
//
#import <Foundation/Foundation.h>
#import "TencentOpenAPI/TencentOAuth.h"

#ifndef TencentQQManagerApi_h
#define TencentQQManagerApi_h

@interface TencentQQManagerApi : NSObject<TencentSessionDelegate>

+ (instancetype)sharedManager;

- (void)registerApp:(NSString *)appId;

- (void)loginWithViewController:(UIViewController *)viewController;

- (BOOL)isInstalled;

- (void)shareImage:(NSString *)localImagePath
             Title:(NSString *)title
              Desc:(NSString *)des;

- (void)tencentDidLogin;

- (void)tencentDidNotLogin:(BOOL)cancelled;

- (void)tencentDidNotNetWork;

@end

#endif /* TencentQQManagerApi_h */
