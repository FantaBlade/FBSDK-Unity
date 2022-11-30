//
//  WeiboApiManager.h
//  Unity-iPhone
//
//  Created by dravens on 2021/3/26.
//
#import "WeiboSDK.h"
#import <Foundation/Foundation.h>

#ifndef WeiboApiManager_h
#define WeiboApiManager_h

@interface WeiboApiManager : NSObject<WBMediaTransferProtocol>

+ (instancetype)sharedManager;

@property (nonatomic, strong) WBMessageObject *messageObject;

- (void)registerApp:(NSString *)appId
        redirectUrl:(NSString *)redirectUrl;

- (void)loginWithViewController:(UIViewController *)viewController;

- (BOOL)isInstalled;

- (void) shareImage:(NSString *)imagePath
              Title:(NSString *)title
               Desc:(NSString *)desc;

@end

#endif /* WeiboApiManager */
