//
//  MobileApiManager.m
//  Unity-iPhone
//
//  Created by dravens on 2021/3/26.
//
#import <ATAuthSDK/ATAuthSDK.h>
#import "MobileApiManager.h"
#import "FBSDKiOSApi.h"

@implementation MobileApiManager

+ (instancetype)sharedManager {
    static dispatch_once_t onceToken;
    static MobileApiManager *instance = nil;
    dispatch_once(&onceToken, ^{
        instance = [[self alloc]init];
    });
    return instance;
}
static BOOL support = YES;


+ (TXCustomModel *)buildFullScreenAutorotateModelWithButton1Title:(NSString *)button1Title
                                                          target1:(id)target1
                                                        selector1:(SEL)selector1
                                                     button2Title:(NSString *)button2Title
                                                          target2:(id)target2
                                                        selector2:(SEL)selector2 {
    TXCustomModel *model = [[TXCustomModel alloc] init];
    model.supportedInterfaceOrientations = UIInterfaceOrientationMaskAllButUpsideDown;
    // model.navColor = [UIColor orangeColor];
    model.navIsHidden = YES;
    NSDictionary *attributes = @{
        NSForegroundColorAttributeName : [UIColor whiteColor],
        NSFontAttributeName : [UIFont systemFontOfSize:20.0]
    };
    // model.navTitle = [[NSAttributedString alloc] initWithString:@"一键登录" attributes:attributes];
    // model.navBackImage = [UIImage imageNamed:@"icon_nav_back_light"];
    // model.logoImage = [UIImage imageNamed:@"taobao"];
    model.changeBtnIsHidden = YES;
    model.privacyOne = @[@"幻刃隐私协议", @"https://www.fantablade.com/system/privacy"];
    model.sloganText = [[NSAttributedString alloc] initWithString:@"登录幻刃账号"attributes:@{NSForegroundColorAttributeName : UIColor.orangeColor,NSFontAttributeName : [UIFont systemFontOfSize:16.0]}];
    model.logoFrameBlock = ^CGRect(CGSize screenSize, CGSize superViewSize, CGRect frame) {
        return CGRectZero; //模拟隐藏该控件
    };
    model.sloganFrameBlock = ^CGRect(CGSize screenSize, CGSize superViewSize, CGRect frame) {
        // if (screenSize.height > screenSize.width) {
            // frame.size.width = superViewSize.width - 40;
            // frame.size.height = 20;
            // frame.origin.x = 20;
            frame.origin.y = 20;// + 80 + 20;
            return frame;
        // } else {
        //     return CGRectZero;
        // }
    };
    model.numberFrameBlock = ^CGRect(CGSize screenSize, CGSize superViewSize, CGRect frame) {
        if (screenSize.height > screenSize.width) {
            frame.origin.y = 130 + 20 + 15;
        } else {
            frame.origin.y = 15 + 80 + 15;
        }
        return frame;
    };
    model.loginBtnFrameBlock = ^CGRect(CGSize screenSize, CGSize superViewSize, CGRect frame) {
        if (screenSize.height > screenSize.width) {
            frame.origin.y = 170 + 30 + 20;
        } else {
            frame.origin.y = 110 + 30 + 20;
        }
        return frame;
    };
    
    UIButton *button1 = [UIButton buttonWithType:UIButtonTypeSystem];
    [button1 setTitle:button1Title forState:UIControlStateNormal];
    [button1 addTarget:target1 action:selector1 forControlEvents:UIControlEventTouchUpInside];
    
    UIButton *button2 = [UIButton buttonWithType:UIButtonTypeSystem];
    [button2 setTitle:button2Title forState:UIControlStateNormal];
    [button2 addTarget:target2 action:selector2 forControlEvents:UIControlEventTouchUpInside];
    
    model.customViewBlock = ^(UIView * _Nonnull superCustomView) {
        [superCustomView addSubview:button1];
        [superCustomView addSubview:button2];
    };
    model.customViewLayoutBlock = ^(CGSize screenSize, CGRect contentViewFrame, CGRect navFrame, CGRect titleBarFrame, CGRect logoFrame, CGRect sloganFrame, CGRect numberFrame, CGRect loginFrame, CGRect changeBtnFrame, CGRect privacyFrame) {
        if (screenSize.height > screenSize.width) {
            button1.frame = CGRectMake(CGRectGetMinX(loginFrame),
                                       CGRectGetMaxY(loginFrame) + 20,
                                       CGRectGetWidth(loginFrame),
                                       30);
            
            button2.frame = CGRectMake(CGRectGetMinX(loginFrame),
                                       CGRectGetMaxY(button1.frame) + 15,
                                       CGRectGetWidth(loginFrame),
                                       30);
        } else {
            button1.frame = CGRectMake(CGRectGetMinX(loginFrame),
                                       CGRectGetMaxY(loginFrame) + 20,
                                       CGRectGetWidth(loginFrame) * 0.5,
                                       30);
            
            button2.frame = CGRectMake(CGRectGetMaxX(button1.frame),
                                       CGRectGetMinY(button1.frame),
                                       CGRectGetWidth(loginFrame) * 0.5,
                                       30);
        }
    };
    return model;
}

- (void)registerApp:(NSString *)appId
{
     //设置SDK参数，App⽣命周期内调⽤⼀次即可
     [[TXCommonHandler sharedInstance] setAuthSDKInfo:appId complete:^(NSDictionary * _Nonnull resultDic) {
//         [weakSelf showResult:resultDic];
     }];
     //2.检测当前环境是否⽀持⼀键登录
     [[TXCommonHandler sharedInstance] checkEnvAvailableWithAuthType:PNSAuthTypeLoginToken complete:^(NSDictionary * _Nullable resultDic) {
        support = [PNSCodeSuccess isEqualToString:[resultDic objectForKey:@"resultCode"]];  }];
}

- (void)loginWithViewController:(UIViewController *)viewController
{
    __weak typeof(self) weakSelf = self;
    TXCustomModel *model = [MobileApiManager buildFullScreenAutorotateModelWithButton1Title:@"短信登录（使用系统导航栏）"
                                                               target1:self
                                                             selector1:@selector(gotoSmsControllerAndShowNavBar)
                                                          button2Title:@"短信登录（隐藏系统导航栏）"
                                                               target2:self
                                                             selector2:@selector(gotoSmsControllerAndHiddenNavBar)];
    //3.1调⽤加速授权⻚弹起接⼝，提前获取必要参数，为后⾯弹起授权⻚加速
//     [[TXCommonHandler sharedInstance] accelerateLoginPageWithTimeout:timeout complete:^(NSDictionary * _Nonnull resultDic) {
//     if ([PNSCodeSuccess isEqualToString:[resultDic objectForKey:@"resultCode"]] == NO)     {
//         NSLog(@"取号，加速授权⻚弹起失败");
// //         [weakSelf showResult:resultDic];
//         return ;
//     }
    //3.2调⽤获取登录Token接⼝，可以立刻弹起授权⻚，model的创建需要放在主线程
    [[TXCommonHandler sharedInstance] getLoginTokenWithTimeout:3.0
                                                    controller:self
                                                         model:model
                                                      complete:^(NSDictionary * _Nonnull resultDic) {
        NSString *resultCode = [resultDic objectForKey:@"resultCode"];
        if ([PNSCodeLoginControllerPresentSuccess isEqualToString:resultCode]) {
            NSLog(@"授权页拉起成功回调：%@", resultDic);
            // [MBProgressHUD hideHUDForView:weakSelf.view animated:YES];
        } else if ([PNSCodeLoginControllerClickCancel isEqualToString:resultCode] ||
                   [PNSCodeLoginControllerClickChangeBtn isEqualToString:resultCode] ||
                   [PNSCodeLoginControllerClickLoginBtn isEqualToString:resultCode] ||
                   [PNSCodeLoginControllerClickCheckBoxBtn isEqualToString:resultCode] ||
                   [PNSCodeLoginClickPrivacyAlertView isEqualToString:resultCode] ||
                   [PNSCodeLoginPrivacyAlertViewClickContinue isEqualToString:resultCode] ||
                   [PNSCodeLoginPrivacyAlertViewClose isEqualToString:resultCode]) {
            NSLog(@"页面点击事件回调：%@", resultDic);
        }else if([PNSCodeLoginControllerClickProtocol isEqualToString:resultCode] ||
                 [PNSCodeLoginPrivacyAlertViewPrivacyContentClick isEqualToString:resultCode]){
            NSLog(@"页面点击事件回调：%@", resultDic);
            NSString *privacyUrl = [resultDic objectForKey:@"url"];
            NSString *privacyName = [resultDic objectForKey:@"urlName"];
            NSLog(@"如果TXCustomModel的privacyVCIsCustomized设置成YES，则SDK内部不会跳转协议页，需要自己实现");
            // if(model.privacyVCIsCustomized){
                // PrivacyWebViewController *controller = [[PrivacyWebViewController alloc] initWithUrl:privacyUrl andUrlName:privacyName];
                // controller.isHiddenNavgationBar = NO;
                // UINavigationController *navigationController = weakSelf.navigationController;
                // if (weakSelf.presentedViewController) {
                //     //如果授权页成功拉起，这个时候则需要使用授权页的导航控制器进行跳转
                //     navigationController = (UINavigationController *)weakSelf.presentedViewController;
                // }
                // [navigationController pushViewController:controller animated:YES];
            // }
        } else if ([PNSCodeSuccess isEqualToString:resultCode]) {
            NSLog(@"获取LoginToken成功回调：%@", resultDic);
            //NSString *token = [resultDic objectForKey:@"token"];
            NSLog(@"接下来可以拿着Token去服务端换取手机号，有了手机号就可以登录，SDK提供服务到此结束");
            //[weakSelf dismissViewControllerAnimated:YES completion:nil];
            NSString *token = [resultDic objectForKey:@"token"];
            [[FBSDKApi sharedInstance] onLoginCallback:token result:TRUE];
            [[TXCommonHandler sharedInstance] cancelLoginVCAnimated:YES complete:nil];
        } else {
            NSLog(@"获取LoginToken或拉起授权页失败回调：%@", resultDic);
            // [MBProgressHUD hideHUDForView:weakSelf.view animated:YES];
            //失败后可以跳转到短信登录界面
            // PNSSmsLoginController *controller = [[PNSSmsLoginController alloc] init];
            // controller.isHiddenNavgationBar = NO;
            // UINavigationController *navigationController = weakSelf.navigationController;
            // if (weakSelf.presentedViewController) {
            //     //如果授权页成功拉起，这个时候则需要使用授权页的导航控制器进行跳转
            //     navigationController = (UINavigationController *)weakSelf.presentedViewController;
            // }
            // [navigationController pushViewController:controller animated:YES];
            [[FBSDKApi sharedInstance] onLoginCallback:@"获取登录Token失败" result:FALSE];
        }
    }];
    // }];
}

- (BOOL)isInstalled
{
    return TRUE;
}

- (BOOL)isSupportAuth
{
    return support;
}

@end
