#import "FBSDKAppleLogin.h"
#import <AuthenticationServices/AuthenticationServices.h>

@interface FBSDKAppleLogin ()<ASAuthorizationControllerDelegate, ASAuthorizationControllerPresentationContextProviding>

@property (nonatomic, copy) FBSDKAppleLoginCompleteHandler completeHander;
@property (nonatomic, copy) FBSDKAppleLoginObserverHandler observerHander;

+ (instancetype) shared;

@end
@implementation FBSDKAppleLogin

+ (instancetype) shared {
    static dispatch_once_t onceToken;
    static FBSDKAppleLogin *instance ;
    dispatch_once(&onceToken, ^{
        instance = [[FBSDKAppleLogin alloc]init];
    });
    return instance;
}

+ (void) checkAuthorizationStateWithUser:(NSString *) user
                         completeHandler:(void(^)(BOOL authorized, NSString *msg)) completeHandler {
    
    if (user == nil || user.length <= 0) {
        if (completeHandler) {
            completeHandler(NO, @"用户标识符错误");
        }
        return;
    }
    
    if (@available(iOS 13.0, *)) {
        ASAuthorizationAppleIDProvider *provider = [[ASAuthorizationAppleIDProvider alloc]init];
        [provider getCredentialStateForUserID:user completion:^(ASAuthorizationAppleIDProviderCredentialState credentialState, NSError * _Nullable error) {
            
            NSString *msg = @"未知";
            BOOL authorized = NO;
            switch (credentialState) {
                case ASAuthorizationAppleIDProviderCredentialRevoked:
                    msg = @"授权被撤销";
                    authorized = NO;
                    break;
                case ASAuthorizationAppleIDProviderCredentialAuthorized:
                    msg = @"已授权";
                    authorized = YES;
                    break;
                case ASAuthorizationAppleIDProviderCredentialNotFound:
                    msg = @"未查到授权信息";
                    authorized = NO;
                    break;
                case ASAuthorizationAppleIDProviderCredentialTransferred:
                    msg = @"授权信息变动";
                    authorized = NO;
                    break;
                    
                default:
                    authorized = NO;
                    break;
            }
            
            if (completeHandler) {
                completeHandler(authorized, msg);
            }
        }];
    } else {
        // Fallback on earlier versions
    }
}

- (void) startAppleIDObserverWithCompleteHandler:(FBSDKAppleLoginObserverHandler) handler
    API_AVAILABLE(ios(13.0)){
    
    self.observerHander = handler;
    
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(FBSDKSignWithAppleIDStateChanged:) name:ASAuthorizationAppleIDProviderCredentialRevokedNotification object:nil];
}

- (void) FBSDKSignWithAppleIDStateChanged:(NSNotification *) noti
    API_AVAILABLE(ios(13.0)){

    if (noti.name == ASAuthorizationAppleIDProviderCredentialRevokedNotification) {
        if (self.observerHander) {
            self.observerHander();
        }
    }
}

- (void) loginWithExistingAccount:(FBSDKAppleLoginCompleteHandler)completeHandler
    API_AVAILABLE(ios(13.0)){
    
    self.completeHander = completeHandler;
    
    ASAuthorizationAppleIDProvider *provider = [[ASAuthorizationAppleIDProvider alloc]init];
    
    ASAuthorizationAppleIDRequest *req = [provider createRequest];
    ASAuthorizationPasswordProvider *pasProvider = [[ASAuthorizationPasswordProvider alloc]init];
    ASAuthorizationPasswordRequest *pasReq = [pasProvider createRequest];
    NSMutableArray *arr = [NSMutableArray arrayWithCapacity:2];
    if (req) {
        [arr addObject:req];
    }
    
    if (pasReq) {
        [arr addObject:pasReq];
    }
    
    ASAuthorizationController *controller = [[ASAuthorizationController alloc]initWithAuthorizationRequests:arr.copy];
    
    controller.delegate = self;
    controller.presentationContextProvider = self;
    [controller performRequests];
}

- (void) loginWithCompleteHandler:(FBSDKAppleLoginCompleteHandler)completeHandler
    API_AVAILABLE(ios(13.0)){
    self.completeHander = completeHandler;
    ASAuthorizationAppleIDProvider *provider = [[ASAuthorizationAppleIDProvider alloc]init];
    ASAuthorizationAppleIDRequest *req = [provider createRequest];
    req.requestedScopes = @[ASAuthorizationScopeFullName, ASAuthorizationScopeEmail];
    ASAuthorizationController *controller = [[ASAuthorizationController alloc]initWithAuthorizationRequests:@[req]];
    controller.delegate = self;
    controller.presentationContextProvider = self;
    [controller performRequests];
}

// 授权失败的回调
- (void)authorizationController:(ASAuthorizationController *)controller didCompleteWithError:(NSError *)error  API_AVAILABLE(ios(13.0)){

    NSString *msg = @"未知";
    
    switch (error.code) {
        case ASAuthorizationErrorCanceled:
            msg = @"用户取消";
            break;
        case ASAuthorizationErrorFailed:
            msg = @"授权请求失败";
            break;
        case ASAuthorizationErrorInvalidResponse:
            msg = @"授权请求无响应";
            break;
        case ASAuthorizationErrorNotHandled:
            msg = @"授权请求未处理";
            break;
        case ASAuthorizationErrorUnknown:
            msg = @"授权失败，原因未知";
            break;
            
        default:
            break;
    }
    
    if (self.completeHander) {
        self.completeHander(NO, nil, nil, nil, nil, nil, nil, nil, error, msg);
    }
}

// 授权成功的回调
- (void)authorizationController:(ASAuthorizationController *)controller didCompleteWithAuthorization:(ASAuthorization *)authorization  API_AVAILABLE(ios(13.0)){
    
    if ([authorization.credential isKindOfClass:[ASAuthorizationAppleIDCredential class]]) {
        ASAuthorizationAppleIDCredential *credential = authorization.credential;
        NSString *user = credential.user;
        NSString *familyName = credential.fullName.familyName;
        NSString * givenName = credential.fullName.givenName;
        NSString *email = credential.email;
        
        NSData *identityToken = credential.identityToken;
        NSData *code = credential.authorizationCode;
        
        if (self.completeHander) {
            self.completeHander(YES, user, familyName, givenName, email, nil, identityToken, code, nil, @"授权成功");
        }
        
        
    } else if ([authorization.credential isKindOfClass:[ASPasswordCredential class]]) {
        // 使用现有的密码凭证登录
        ASPasswordCredential *credential = authorization.credential;
        
        // 用户唯一标识符
        NSString *user = credential.user;
        NSString *password = credential.password;
        
        if (self.completeHander) {
            self.completeHander(YES, user, nil, nil, nil, password, nil, nil, nil, @"授权成功");
        }
    }
}

- (ASPresentationAnchor)presentationAnchorForAuthorizationController:(ASAuthorizationController *)controller  API_AVAILABLE(ios(13.0)){
    return [UIApplication sharedApplication].windows.firstObject;
}

- (BOOL) isInstalled{
    if (@available(iOS 13.0, *)) {
        return true;
    }else{
        return false;
    }
}


@end
