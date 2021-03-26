#import "UnityAppController.h"
#import "WXApi.h"
#import "WXApiManager.h"
#import "DouYinOpenSDK/DouYinOpenSDKApplicationDelegate.h"
@interface OverrideAppDelegate : UnityAppController
@end

IMPL_APP_CONTROLLER_SUBCLASS(OverrideAppDelegate)

@implementation OverrideAppDelegate
-(BOOL)application:(UIApplication*) application didFinishLaunchingWithOptions:(NSDictionary*) options
{
    [[DouyinOpenSDKApplicationDelegate sharedInstance] application:application didFinishLaunchingWithOptions:options];
    return [super application:application didFinishLaunchingWithOptions:options];
}

- (BOOL)application:(UIApplication *)application continueUserActivity:(nonnull NSUserActivity *)userActivity restorationHandler:(nonnull void (^)(NSArray<id<UIUserActivityRestoring>> * _Nullable))restorationHandler
{
    return [WXApi handleOpenUniversalLink:userActivity delegate:[WXApiManager sharedManager]];
}

- (BOOL)application:(UIApplication *)application handleOpenURL:(NSURL *)url
{
    [WXApi handleOpenURL:url delegate:[WXApiManager sharedManager]];
    return [super application:application didFinishLaunchingWithOptions:@{}];
}

- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options
{
    [[DouyinOpenSDKApplicationDelegate sharedInstance] application:app openURL:url sourceApplication:options[UIApplicationOpenURLOptionsSourceApplicationKey] annotation:options[UIApplicationOpenURLOptionsAnnotationKey]];
    [WXApi handleOpenURL:url delegate:[WXApiManager sharedManager]];
    return [super application:app openURL:url options:options];
}

- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation
{
    [[DouyinOpenSDKApplicationDelegate sharedInstance] application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
    [WXApi handleOpenURL:url delegate:[WXApiManager sharedManager]];
    return [super application:application didFinishLaunchingWithOptions:@{}];
}

@end
