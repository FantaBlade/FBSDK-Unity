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


- (TXCustomModel *)buildFullScreenAutorotateModel {
    TXCustomModel *model = [[TXCustomModel alloc] init];
    // 横竖屏切换
    model.supportedInterfaceOrientations = UIInterfaceOrientationMaskAllButUpsideDown;
    // 仅支持竖屏
    // model.supportedInterfaceOrientations = UIInterfaceOrientationMaskPortrait;
    // 仅支持横屏
    // model.supportedInterfaceOrientations = UIInterfaceOrientationMaskLandscape;

    CGSize screenSize = [UIScreen mainScreen].bounds.size;
    bool isPortrait = screenSize.height > screenSize.width;
    int designHeight = (int)(screenSize.height * 0.87);
    int padding = (int)(screenSize.height * 0.13);
    int unit = designHeight / 10;
    // model.navColor = [UIColor orangeColor];
    model.navIsHidden = YES;
    model.prefersStatusBarHidden = YES;
    NSDictionary *attributes = @{
        NSForegroundColorAttributeName : [UIColor whiteColor],
        NSFontAttributeName : [UIFont systemFontOfSize:20.0]
    };
    // model.navTitle = [[NSAttributedString alloc] initWithString:@"一键登录" attributes:attributes];
    // model.navBackImage = [UIImage imageNamed:@"icon_nav_back_light"];
    // model.logoImage = [UIImage imageNamed:@"taobao"];
    model.changeBtnIsHidden = YES;
    model.sloganText = [[NSAttributedString alloc] initWithString:@"登录幻刃账号"attributes:@{NSForegroundColorAttributeName : UIColor.blackColor,NSFontAttributeName : [UIFont systemFontOfSize:20.0]}];
    model.logoFrameBlock = ^CGRect(CGSize screenSize, CGSize superViewSize, CGRect frame) {
        return CGRectZero; //模拟隐藏该控件
    };
    model.checkBoxIsHidden = NO;
    model.checkBoxWH = 17.0;
    model.sloganFrameBlock = ^CGRect(CGSize screenSize, CGSize superViewSize, CGRect frame) {
            frame.origin.y = (int)(unit*1.2)+padding;
            return frame;
    };
    model.numberFont = [UIFont systemFontOfSize:35.0];
    model.numberFrameBlock = ^CGRect(CGSize screenSize, CGSize superViewSize, CGRect frame) {
        if (isPortrait) {
            frame.origin.y = unit * 2+padding;
        } else {
            frame.origin.y = unit * 3+padding;
        }
        return frame;
    };
    model.loginBtnText = [[NSAttributedString alloc] initWithString:@"一键登录"attributes:@{NSForegroundColorAttributeName : UIColor.whiteColor,NSFontAttributeName : [UIFont systemFontOfSize:15.0]}];
    model.loginBtnFrameBlock = ^CGRect(CGSize screenSize, CGSize superViewSize, CGRect frame) {
        if (isPortrait) {
            frame.size.width = superViewSize.width-48;
            frame.origin.x = (int)((superViewSize.width - frame.size.width) / 2);
            frame.origin.y = unit * 3+padding;
        } else {
            frame.size.width = (int)((superViewSize.width-40)/2.5 - 8);
            frame.size.height = 41;
            frame.origin.x = (int)((superViewSize.width - frame.size.width) / 2);
            frame.origin.y = unit * 5+padding;
        }
        return frame;
    };
    
    UIButton *button1 = [UIButton buttonWithType:UIButtonTypeSystem];
    [button1 setTitle:@"切换到其他方式" forState:UIControlStateNormal];
    [button1 addTarget:self action:@selector(hideMobileController) forControlEvents:UIControlEventTouchUpInside];
    model.customViewBlock = ^(UIView * _Nonnull superCustomView) {
        [superCustomView addSubview:button1];
    };
    model.customViewLayoutBlock = ^(CGSize screenSize, CGRect contentViewFrame, CGRect navFrame, CGRect titleBarFrame, CGRect logoFrame, CGRect sloganFrame, CGRect numberFrame, CGRect loginFrame, CGRect changeBtnFrame, CGRect privacyFrame) {
            button1.frame = CGRectMake(CGRectGetMinX(loginFrame),
                                       CGRectGetMaxY(loginFrame) + 20,
                                       CGRectGetWidth(loginFrame),
                                       30);
    };
    model.privacyOne = @[@"幻刃隐私协议", @"https://www.fantablade.com/system/privacy"];
    model.privacyAlignment = NSTextAlignmentCenter;
    model.privacyFont = [UIFont systemFontOfSize:13.0];
    model.privacyAlertIsNeedShow = YES;
    // model.privacyAlertBackgroundColor = [UIColor colorWithRed:68.0f/255.0f green:142.0f/255.0f blue:247.0f/255.0f alpha:1.0];
    // model.privacyAlertContentColors = @[UIColor.grayColor, UIColor.blackColor];
    // model.privacyAlertButtonTextColors = @[UIColor.whiteColor,UIColor.blueColor];
    model.privacyAlertContentAlignment = NSTextAlignmentCenter;
    model.privacyAlertButtonFont = [UIFont systemFontOfSize:15.0];
    model.privacyAlertTitleFrameBlock = ^CGRect(CGSize screenSize, CGSize superViewSize, CGRect frame) {
        return CGRectMake(0, 15, frame.size.width, frame.size.height);
    };
    model.privacyAlertPrivacyContentFrameBlock = ^CGRect(CGSize screenSize, CGSize superViewSize, CGRect frame) {
        return CGRectMake(frame.origin.x, 50, frame.size.width, frame.size.height);
    };
    model.privacyAlertButtonFrameBlock = ^CGRect(CGSize screenSize, CGSize superViewSize, CGRect frame) {
        return CGRectMake(25,superViewSize.height - 50 - 20, superViewSize.width-50, 41);;
    };
    model.privacyAlertFrameBlock = ^CGRect(CGSize screenSize, CGSize superViewSize, CGRect frame) {
        if (isPortrait) {
            return CGRectMake(20, (superViewSize.height - 150)*0.5, screenSize.width-40, 150);
        }else{
            int width = (int)((superViewSize.width-40)/2);
            return CGRectMake((int)((superViewSize.width - width) / 2), (superViewSize.height - 150)*0.5, width, 150);
        }
    };
    return model;
}

- (void)registerApp:(NSString *)appId
{
    // NSLog(@"设置秘钥：%@", appId);
     //设置SDK参数，App⽣命周期内调⽤⼀次即可
     [[TXCommonHandler sharedInstance] setAuthSDKInfo:appId complete:^(NSDictionary * _Nonnull resultDic) {
//         [weakSelf showResult:resultDic];
        // NSLog(@"设置秘钥结果：%@", resultDic);
        // [[[TXCommonHandler sharedInstance] getReporter] setConsolePrintLoggerEnable:YES];
     }];

     //2.检测当前环境是否⽀持⼀键登录
     // [[TXCommonHandler sharedInstance] checkEnvAvailableWithAuthType:PNSAuthTypeLoginToken complete:^(NSDictionary * _Nullable resultDic) {
     //    support = [PNSCodeSuccess isEqualToString:[resultDic objectForKey:@"resultCode"]];  }];
}

- (void)hideMobileController {
    // PNSSmsLoginController *controller = [[PNSSmsLoginController alloc] init];
    // controller.isHiddenNavgationBar = NO;
    // if (self.presentedViewController) {
    //     //找到授权页的导航控制器
    //     [(UINavigationController *)self.presentedViewController pushViewController:controller animated:YES];
    // }
    NSLog(@"获取LoginToken失败回调：%@", @"用户取消登录");
    [[TXCommonHandler sharedInstance] cancelLoginVCAnimated:YES complete:nil];
    [[FBSDKApi sharedInstance] onLoginCallback:@"用户取消登录" result:FALSE];
}

- (void)loginWithViewController:(UIViewController *)viewController
{
    // __weak typeof(self) weakSelf = self;
    TXCustomModel *model = [self buildFullScreenAutorotateModel];
    //3.1调⽤加速授权⻚弹起接⼝，提前获取必要参数，为后⾯弹起授权⻚加速
//     [[TXCommonHandler sharedInstance] accelerateLoginPageWithTimeout:timeout complete:^(NSDictionary * _Nonnull resultDic) {
//     if ([PNSCodeSuccess isEqualToString:[resultDic objectForKey:@"resultCode"]] == NO)     {
//         NSLog(@"取号，加速授权⻚弹起失败");
// //         [weakSelf showResult:resultDic];
//         return ;
//     }
    //3.2调⽤获取登录Token接⼝，可以立刻弹起授权⻚，model的创建需要放在主线程
    [[TXCommonHandler sharedInstance] getLoginTokenWithTimeout:3.0
                                                    controller:viewController
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
            // NSString *privacyUrl = [resultDic objectForKey:@"url"];
            // NSString *privacyName = [resultDic objectForKey:@"urlName"];
            // NSLog(@"如果TXCustomModel的privacyVCIsCustomized设置成YES，则SDK内部不会跳转协议页，需要自己实现");
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
            // NSLog(@"接下来可以拿着Token去服务端换取手机号，有了手机号就可以登录，SDK提供服务到此结束");
            //[weakSelf dismissViewControllerAnimated:YES completion:nil];
            NSString *token = [resultDic objectForKey:@"token"];
            [[FBSDKApi sharedInstance] onLoginCallback:token result:TRUE];
            //[weakSelf dismissViewControllerAnimated:YES completion:nil];
            [[TXCommonHandler sharedInstance] cancelLoginVCAnimated:YES complete:nil];
        } else {
            support = NO;
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
            //[weakSelf dismissViewControllerAnimated:YES completion:nil];
            [[TXCommonHandler sharedInstance] cancelLoginVCAnimated:YES complete:nil];
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
