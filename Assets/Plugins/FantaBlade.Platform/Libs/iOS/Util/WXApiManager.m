//Tencent is pleased to support the open source community by making WeDemo available.
//Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
//Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
//http://opensource.org/licenses/MIT
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

#import "WXApiManager.h"
#import "RandomKey.h"
#import "FBSDKiOSApi.h"

static NSString* const kWXNotInstallErrorTitle = @"您还没有安装微信，不能使用微信分享功能";

@interface WXApiManager ()

@property (nonatomic, strong) NSString *authState;

@end

@implementation WXApiManager

#pragma mark - Life Cycle
+ (instancetype)sharedManager {
    static dispatch_once_t onceToken;
    static WXApiManager *instance = nil;
    dispatch_once(&onceToken, ^{
        instance = [[self alloc] initInPrivate];
    });
    return instance;
}

- (instancetype)initInPrivate {
    self = [super init];
    if (self) {
        _delegate = nil;
    }
    return self;
}

- (instancetype)init {
    return nil;
}

- (instancetype)copy {
    return nil;
}

#pragma mark - Public Methods
- (void)sendAuthRequestWithController:(UIViewController*)viewController
                             delegate:(id<WXAuthDelegate>)delegate {
    SendAuthReq* req = [[SendAuthReq alloc] init];
    req.scope = @"snsapi_userinfo";
    self.authState = req.state = [NSString randomKey];
    self.delegate = delegate;
    [WXApi sendAuthReq:req viewController:viewController delegate:self completion:^(BOOL success) {
        
    }];
}

- (BOOL)sendLinkContent:(NSString *)urlString
                  Title:(NSString *)title
            Description:(NSString *)description
                AtScene:(enum WXScene)scene {
    
    if (![WXApi isWXAppInstalled]) {
        [[FBSDKApi sharedInstance] onShareCallback:@"" result:FALSE];
        return NO;
    }
    WXWebpageObject *ext = [WXWebpageObject object];
    ext.webpageUrl = urlString;
    
    WXMediaMessage *message = [WXMediaMessage message];
    message.title = title;
    message.description = description;
    message.mediaTagName = @"WECHAT_TAG_JUMP_SHOWRANK";
    message.mediaObject = ext;
    message.thumbData = UIImagePNGRepresentation([UIImage imageNamed:@"wxLogoGreen"]);

    SendMessageToWXReq *req = [[SendMessageToWXReq alloc] init];
    req.message = message;
    req.bText = NO;
    req.scene = scene;
    [WXApi sendReq:req completion:^(BOOL success) {
        
    }];
    return YES;
}

- (BOOL)sendFileData:(NSData *)fileData
       fileExtension:(NSString *)extension
               Title:(NSString *)title
         Description:(NSString *)description
          ThumbImage:(UIImage *)thumbImage
             AtScene:(enum WXScene)scene {
    if (![WXApi isWXAppInstalled]) {
        [[FBSDKApi sharedInstance] onShareCallback:@"" result:FALSE];
        return NO;
    }

    WXFileObject *ext = [WXFileObject object];
    ext.fileExtension = extension;
    ext.fileData = fileData;

    WXMediaMessage *message = [WXMediaMessage message];
    message.mediaObject = ext;
    message.title = title;
    message.description = description;
    [message setThumbImage:thumbImage];
    
    SendMessageToWXReq *req = [[SendMessageToWXReq alloc] init];
    req.message = message;
    req.bText = NO;
    req.scene = scene;
    
    [WXApi sendReq:req completion:^(BOOL success) {
        
    }];
    return YES;
}

- (BOOL)sendImageData:(NSString *)imagePath
               Title:(NSString *)title
         Description:(NSString *)description
             AtScene:(enum WXScene)scene {
    if (![WXApi isWXAppInstalled]) {
        [[FBSDKApi sharedInstance] onShareCallback:@"NotInInstalled" result:FALSE];
        return NO;
    }
    
    UIImage *image = [UIImage imageWithContentsOfFile:imagePath];
    WXImageObject *imageObj = [WXImageObject object];
    imageObj.imageData = UIImagePNGRepresentation(image);
    
    WXMediaMessage *message = [WXMediaMessage message];
    message.mediaObject = imageObj;
    message.title = title;
    message.description = description;

    SendMessageToWXReq *req = [[SendMessageToWXReq alloc] init];
    req.message = message;
    req.bText = NO;
    req.scene = scene;

    [WXApi sendReq:req completion:^(BOOL success) {
        [[FBSDKApi sharedInstance] onShareCallback:@"" result:success];
    }];
    return YES;
}

-(BOOL)isInstalled
{
    return [WXApi isWXAppInstalled];
}

#pragma mark - WXApiDelegate
-(void)onReq:(BaseReq*)req {
    // just leave it here, WeChat will not call our app
}

-(void)onResp:(BaseResp*)resp {    
    if([resp isKindOfClass:[SendAuthResp class]]) {
        SendAuthResp* authResp = (SendAuthResp*)resp;
        /* Prevent Cross Site Request Forgery */
        if (![authResp.state isEqualToString:self.authState]) {
//            if (self.delegate && [self.delegate respondsToSelector:@selector(wxAuthDenied)])
//                [self.delegate wxAuthDenied];
            
            return;
        }
        
        switch (resp.errCode) {
            case WXSuccess:
//                NSLog(@"RESP:code:%@,state:%@\n", authResp.code, authResp.state);
//                if (self.delegate && [self.delegate respondsToSelector:@selector(wxAuthSucceed:)])
//                    [self.delegate wxAuthSucceed:authResp.code];
                [[FBSDKApi sharedInstance] onLoginCallback:authResp.code result:true];
                break;
            case WXErrCodeAuthDeny:
//                if (self.delegate && [self.delegate respondsToSelector:@selector(wxAuthDenied)])
//                    [self.delegate wxAuthDenied];
                [[FBSDKApi sharedInstance] onLoginCallback:@"Failed" result:false];
                break;
            case WXErrCodeUserCancel:
//                if (self.delegate && [self.delegate respondsToSelector:@selector(wxAuthCancel)])
//                    [self.delegate wxAuthCancel];
                [[FBSDKApi sharedInstance] onLoginCallback:@"UserCancel" result:false];
            default:
                break;
        }
    }
}

@end
