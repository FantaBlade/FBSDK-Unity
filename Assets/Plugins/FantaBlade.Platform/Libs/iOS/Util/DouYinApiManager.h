//
//  DouYinApiManager.h
//  Unity-iPhone
//
//  Created by dravens on 2021/3/26.
//

#import "DouyinOpenSDK/DouyinOpenSDK-umbrella.h"
#import <Foundation/Foundation.h>

#ifndef DouYinApiManager_h
#define DouYinApiManager_h

@interface DouYinApiManager : NSObject

+ (instancetype)sharedManager;

- (void)registerApp:(NSString *)appId;

- (void)loginWithViewController:(UIViewController *)viewController;

- (BOOL)isInstalled;

@end

#endif /* DouYinApiManager_h */
