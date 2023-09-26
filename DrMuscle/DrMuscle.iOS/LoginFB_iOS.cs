using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DrMuscle.iOS;
using DrMaxMuscle.iOS;
using Foundation;
using Xamarin.Forms;
using Facebook;
using UIKit;
using MonoTouch.Dialog;
using DrMuscle.Dependencies;

[assembly: Dependency(typeof(LoginFB_iOS))]
namespace DrMuscle.iOS
{
    public class LoginFB_iOS : ILoginFB
    {
        public static LoginFB_iOS Instance { get; private set; }
        private const string AppId = "1865252523754972";
        private const string ExtendedPermissions = "user_about_me,public_profile,email";

        FacebookClient fb;
        string accessToken;
        bool isLoggedIn;
        string lastMessageId;
        public event FBLoginSucceded OnFBLoginSucceded;

        public LoginFB_iOS()
        {
            Instance = this;
        }

        public string Login()
        {
			UIWindow _window = UIApplication.SharedApplication.KeyWindow ;
			RootElement _rootElement = new RootElement("Facebook Log in") { new Section() { new FacebookLoginElement(AppId, ExtendedPermissions) } };
            DialogViewController _rootVC = new DialogViewController(_rootElement);
            UINavigationController _nav = new UINavigationController(_rootVC);
            _window.RootViewController = _nav;
            _window.MakeKeyAndVisible();
            return "";
        }

        public void FireLoginSuccess(string id, string email, string gender, string token)
        {
            if (OnFBLoginSucceded != null)
                OnFBLoginSucceded(id, email, gender, token);
        }
    }
}
