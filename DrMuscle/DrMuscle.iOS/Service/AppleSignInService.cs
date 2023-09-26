using System;
using System.Threading.Tasks;
using AuthenticationServices;
using Foundation;
using DrMuscle.iOS.Services;
using DrMuscle.Services;
using UIKit;
using DrMuscle.Models;

[assembly: Xamarin.Forms.Dependency(typeof(AppleSignInService))]
namespace DrMuscle.iOS.Services
{
    public class AppleSignInService: NSObject, IAppleSignInService,IASAuthorizationControllerDelegate, IASAuthorizationControllerPresentationContextProviding
    {
        public bool IsAvailable => UIDevice.CurrentDevice.CheckSystemVersion(13, 0);

        TaskCompletionSource<ASAuthorizationAppleIdCredential> tcsCredential;

        public async Task<AppleSignInCredentialState> GetCredentialStateAsync(string userId)
        {
            var appleIdProvider = new ASAuthorizationAppleIdProvider();
            var credentialState = await appleIdProvider.GetCredentialStateAsync(userId);
            switch (credentialState)
            {
                case ASAuthorizationAppleIdProviderCredentialState.Authorized:
                    // The Apple ID credential is valid.
                    return AppleSignInCredentialState.Authorized;
                case ASAuthorizationAppleIdProviderCredentialState.Revoked:
                    // The Apple ID credential is revoked.
                    return AppleSignInCredentialState.Revoked;
                case ASAuthorizationAppleIdProviderCredentialState.NotFound:
                    // No credential was found, so show the sign-in UI.
                    return AppleSignInCredentialState.NotFound;
                default:
                    return AppleSignInCredentialState.Unknown;
            }

        }

        public async Task<AppleAccount> SignInAsync()
        {
            var appleIdProvider = new ASAuthorizationAppleIdProvider();
            var request = appleIdProvider.CreateRequest();
            request.RequestedScopes = new[] { ASAuthorizationScope.Email, ASAuthorizationScope.FullName };

            var authorizationController = new ASAuthorizationController(new[] { request });
            authorizationController.Delegate = this;
            authorizationController.PresentationContextProvider = this;
            authorizationController.PerformRequests();

            tcsCredential = new TaskCompletionSource<ASAuthorizationAppleIdCredential>();

            var creds = await tcsCredential.Task;

            if (creds == null)
                return null;


            //var appleSignInRequest = new ASAuthorizationAppleIdProvider().CreateRequest();
            ////appleSignInRequest.RequestedScopes = new[] { ASAuthorizationScope.Email, ASAuthorizationScope.FullName };

            //var authorizationController = new ASAuthorizationController(new[] { appleSignInRequest });
            //authorizationController.Delegate = this;
            //authorizationController.PresentationContextProvider = this;

            //authorizationController.PerformRequests();


           var appleAccount = new AppleAccount();
            appleAccount.Token = new NSString(creds.IdentityToken, NSStringEncoding.UTF8).ToString();
            appleAccount.Email = creds.Email;
            appleAccount.UserId = creds.User;
            //appleAccount.Name = NSPersonNameComponentsFormatter.GetLocalizedString(creds.FullName, NSPersonNameComponentsFormatterStyle.Default, NSPersonNameComponentsFormatterOptions.Phonetic);
            appleAccount.Name = NSPersonNameComponentsFormatter.GetLocalizedString(creds.FullName, NSPersonNameComponentsFormatterStyle.Default, 0);

            appleAccount.GivenName = creds.FullName.GivenName;
            appleAccount.FamilyName = creds.FullName.FamilyName;

            appleAccount.RealUserStatus = creds.RealUserStatus.ToString();

            return appleAccount;
        }

        #region IASAuthorizationController Delegate

        [Export("authorizationController:didCompleteWithAuthorization:")]
        public void DidComplete(ASAuthorizationController controller, ASAuthorization authorization)
        {
            var creds = authorization.GetCredential<ASAuthorizationAppleIdCredential>();
            tcsCredential?.TrySetResult(creds);
        }

        [Export("authorizationController:didCompleteWithError:")]
        public void DidComplete(ASAuthorizationController controller, NSError error)
        {
            // Handle error
            
            //Acr.UserDialogs.UserDialogs.Instance.Alert($"{error}", "Error", "OK");
            // if controller.authorizationRequests.contains(where: { $0 is ASAuthorizationPasswordRequest }) {
            //if (error != null && error.Code == 1000 && controller.AuthorizationRequests.Length>0 && controller.AuthorizationRequests[0] is ASAuthorizationAppleIdRequest)
            //{
            //    var requestAppleID = new ASAuthorizationAppleIdProvider().CreateRequest();
            //    requestAppleID.RequestedScopes = new[] { ASAuthorizationScope.Email, ASAuthorizationScope.FullName };
            //    requestAppleID.RequestedOperation = ASAuthorizationOperation.Implicit;
            //    performRequest(requestAppleID);
            //}
            //else
                tcsCredential?.TrySetResult(null);
            Console.WriteLine(error);
        }

        private void performRequest(ASAuthorizationRequest request)
        {
            var controller = new ASAuthorizationController(authorizationRequests: new[] { request });
            controller.Delegate = this;
            controller.PresentationContextProvider = this;
            controller.PerformRequests();
        }
        #endregion

        #region IASAuthorizationControllerPresentation Context Providing

        public UIWindow GetPresentationAnchor(ASAuthorizationController controller)
        {
            return UIApplication.SharedApplication.KeyWindow;
        }

        #endregion

    }
}
