#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN
typedef void(^FBSDKAppleLoginCompleteHandler)(BOOL successed,NSString * _Nullable user, NSString *_Nullable familyName, NSString *_Nullable givenName, NSString *_Nullable email,NSString *_Nullable password, NSData *_Nullable identityToken, NSData *_Nullable authorizationCode, NSError *_Nullable error, NSString * msg);

typedef void(^FBSDKAppleLoginObserverHandler)(void);
@interface FBSDKAppleLogin : NSObject

+ (instancetype) shared ;

+ (void) checkAuthorizationStateWithUser:(NSString *) user
                         completeHandler:(void(^)(BOOL authorized, NSString *msg)) completeHandler ;

- (void) loginWithExistingAccount:(FBSDKAppleLoginCompleteHandler)completeHandler ;

- (void) loginWithCompleteHandler:(FBSDKAppleLoginCompleteHandler)completeHandler ;

- (void) startAppleIDObserverWithCompleteHandler:(FBSDKAppleLoginObserverHandler) handler ;

- (BOOL) isInstalled;
@end

NS_ASSUME_NONNULL_END
