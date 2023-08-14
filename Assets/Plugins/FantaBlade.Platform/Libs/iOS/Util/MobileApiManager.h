//
//  MobileApiManager.h
//  Unity-iPhone
//
//  Created by dravens on 2021/3/26.
//

#import "DouyinOpenSDK/DouyinOpenSDK-umbrella.h"
#import <Foundation/Foundation.h>

#ifndef MobileApiManager_h
#define MobileApiManager_h

@interface MobileApiManager : NSObject

+ (instancetype)sharedManager;

- (void)registerApp:(NSString *)appId;

- (void)loginWithViewController:(UIViewController *)viewController;

- (BOOL)isInstalled;

- (BOOL)isSupportAuth;

@end

#endif /* MobileApiManager_h */
