using DrMuscleWebApiSharedModel;
//using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Resx;

namespace DrMuscle
{
    internal class DrMuscleRestClient
    {
        public int ApiVersion { get { return 1; } }
        public string BaseUrl;
        private static DrMuscleRestClient _instance;




        public delegate void OnStartPost();
        public event OnStartPost StartPost;


        public async Task<BooleanModel> ForgotPassword(ForgotPasswordModel model)
        {
            return await PostJson<BooleanModel>("api/Account/ForgotPassword", model);
        }


       

        public async Task<BooleanModel> SaveWorkoutV2(SaveWorkoutModel model)
        {
            return await PostJson<BooleanModel>("api/Workout/SaveWorkoutV2Pro", model);
        }
        //
        public async Task<BooleanModel> SaveWorkoutV3(SaveWorkoutModel model)
        {
            return await PostJson<BooleanModel>("api/Workout/SaveWorkoutV3Pro", model);
        }
        public async Task<BooleanModel> SaveWorkoutV3WithoutLoader(SaveWorkoutModel model)
        {
            return await PostJsonWithoughtLoader<BooleanModel>("api/Workout/SaveWorkoutV3Pro", model);
        }
        public async Task<GetUserProgramInfoResponseModel> SaveGetWorkoutInfo(SaveWorkoutModel model)
        {
            return await PostJson<GetUserProgramInfoResponseModel>("api/Workout/SaveGetWorkoutInfoPro", model);
        }

        public delegate void OnEndPost();
        public event OnUnauthorized Unauthorized;

        public async Task<GetUserWorkoutTemplateResponseModel> GetUserWorkout()
        {
            return await PostJson<GetUserWorkoutTemplateResponseModel>("api/Workout/GetUserWorkout", null);
        }

        public async Task<WorkoutTemplateModel> GetUserCustomizedCurrentWorkout(long workoutid)
        {
            return await PostJson<WorkoutTemplateModel>("api/Workout/GetUserCustomizedCurrentWorkout", workoutid);
        }
       
        public event OnEndPost EndPost;

        public delegate void OnUnauthorized();
        //As a user doing a bodyweight exercise for the first time, I want the app to ask me how many times I can do it "easily", so that I don't get injured the first time I do it
        public async Task<GetUserWorkoutTemplateGroupResponseModel> GetUserWorkoutGroup()
        {
            return await PostJson<GetUserWorkoutTemplateGroupResponseModel>("api/Workout/GetUserWorkoutTemplateGroup", null);
        }


        public async Task<RecommendationModel> GetRecommendationNormalRIRForExercise(GetRecommendationForExerciseModel model)
        {
            return await PostJson<RecommendationModel>("api/Exercise/GetRecommendationNormalRIRForExercise", model);
        }

        public async Task<RecommendationModel> GetRecommendationRestPauseRIRForExerciseWithoutLoader(GetRecommendationForExerciseModel model)
        {
            return await PostJsonWithoughtLoader<RecommendationModel>("api/Exercise/GetRecommendationRestPauseRIRForExercise", model);
        }

        public async Task<RecommendationModel> GetRecommendationNormalRIRForExerciseWithoutLoader(GetRecommendationForExerciseModel model)
        {
            return await PostJsonWithoughtLoader<RecommendationModel>("api/Exercise/GetRecommendationNormalRIRForExercise", model);
        }

     
        public async Task<List<OneRMModel>> GetOneRMForExerciseWithoutLoader(GetOneRMforExerciseModel model)
        {
            return await PostJsonWithoughtLoader<List<OneRMModel>>("api/Exercise/GetOneRMForExercise", model);
        }



        public async Task<UserInfosModel> GetUserInfo()
        {
            return await PostJson<UserInfosModel>("api/Account/GetUserInfoPyramid", null);
        }

        public async Task<UserInfosModel> GetUserInfoWithoutLoader()
        {
            return await PostJsonWithoughtLoader<UserInfosModel>("api/Account/GetUserInfoPyramid", null);
        }


        public async Task<UserInfosModel> GetTargetIntake()
        {
            return await PostJson<UserInfosModel>("api/Account/GetTargetIntake", null);
        }

        public async Task<UserInfosModel> GetTargetIntakeWithoutLoader()
        {
            return await PostJsonWithoughtLoader<UserInfosModel>("api/Account/GetTargetIntake", null);
        }

        public async Task<UserInfosModel> SetUserBodyWeight(UserInfosModel userInfosModel)
        {
            return await PostJson<UserInfosModel>("", userInfosModel);
        }

        public async Task<UserInfosModel> SetUserBarWeight(UserInfosModel userInfosModel)
        {
            return await PostJsonWithoughtLoader<UserInfosModel>("", userInfosModel);
        }

        public async Task<UserInfosModel> SetUserWeightGoal(UserInfosModel userInfosModel)
        {
            return await PostJson<UserInfosModel>("", userInfosModel);
        }

        public async Task<UserInfosModel> SetUserAge(UserInfosModel userInfosModel)
        {
            return await PostJson<UserInfosModel>("", userInfosModel);
        }
        
        public async Task<UserInfosModel> SetUserHeight(UserInfosModel userInfosModel)
        {
            return await PostJson<UserInfosModel>("api/Account/SetUserHeight", userInfosModel);
        }

        public async Task<UserInfosModel> SetUserEquipmentPlateSettings(EquipmentModel equipModel)
        {
            return await PostJsonWithoughtLoader<UserInfosModel>("api/Account/SetUserEquipmentPlateSettings", equipModel);
        }


        public async Task<UserInfosModel> SetUserReminderTimeWithoutLoader(UserInfosModel userInfosModel)
        {
            return await PostJsonWithoughtLoader<UserInfosModel>("api/Account/SetUserReminderTime", userInfosModel);
        }

        public async Task<UserInfosModel> SetUserMassUnit(UserInfosModel userInfosModel)
        {
            return await PostJson<UserInfosModel>("", userInfosModel);
        }
        


        //Chat started

        private string _token;
        public static DrMuscleRestClient Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DrMuscleRestClient();
                return _instance;
            }
        }

        public void SetToken(string token)
        {
            _token = token;
        }



        private DrMuscleRestClient()
        {
            ResetBaseUrl();
        }

        public void ResetBaseUrl()
        {

            if (LocalDBManager.Instance.GetDBSetting("Environment") == null)
            {
                BaseUrl = "https://drmuscle.azurewebsites.net/";
                return;
            }
            if (LocalDBManager.Instance.GetDBSetting("Environment").Value == "Production")
                BaseUrl = "https://drmuscle.azurewebsites.net/";
            else
                BaseUrl = "https://drmuscle2.azurewebsites.net/";
        }

        public async Task<LoginSuccessResult> Login(LoginModel model)
        {
            HttpResponseMessage response;
            LoginSuccessResult token = null;
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(BaseUrl);
                    var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type","password"),
                        new KeyValuePair<string, string>("password",model.Password),
                        new KeyValuePair<string, string>("username",model.Username)
                    };
                    var content = new FormUrlEncodedContent(body);
                    StartPost?.Invoke();
                    response = await client.PostAsync("token", content);

                    string raw = await response.Content.ReadAsStringAsync();
                    token = JsonConvert.DeserializeObject<LoginSuccessResult>(raw);
                    SetToken(token.access_token);

                }
                catch (Exception error)
                {
                    token = null;
                    response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
#if DEBUG
                    response.Content = new StringContent(string.Concat(error.Message, error.StackTrace));
#else
                    response.Content = new StringContent("Oops, an error occured, please try again");
#endif
                }
                finally
                {
                    EndPost?.Invoke();

                }
                return token;
            }
        }

        public async Task<LoginSuccessResult> LoginWithoutLoader(LoginModel model)
        {
            HttpResponseMessage response;
            LoginSuccessResult token = null;
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(BaseUrl);
                    var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type","password"),
                        new KeyValuePair<string, string>("password",model.Password),
                        new KeyValuePair<string, string>("username",model.Username)
                    };
                    var content = new FormUrlEncodedContent(body);
                    //StartPost?.Invoke();
                    response = await client.PostAsync("token", content);

                    string raw = await response.Content.ReadAsStringAsync();
                    token = JsonConvert.DeserializeObject<LoginSuccessResult>(raw);
                    SetToken(token.access_token);

                }
                catch (Exception error)
                {
                    token = null;
                    response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
#if DEBUG
                    response.Content = new StringContent(string.Concat(error.Message, error.StackTrace));
#else
                    response.Content = new StringContent("Oops, an error occured, please try again");
#endif
                }
                finally
                {
                    //EndPost?.Invoke();

                }
                return token;
            }
        }

        public async Task<LoginSuccessResult> GoogleLogin(string GoogleToken, string email, string name, string bodyWeight, string massUnit)
        {
            HttpResponseMessage response;
            LoginSuccessResult token = null;
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(BaseUrl);
                    var body = new List<KeyValuePair<string, string>>
                    {
                       
                    };
                    var content = new FormUrlEncodedContent(body);
                    StartPost?.Invoke();
                    response = await client.PostAsync("token", content);

                    string raw = await response.Content.ReadAsStringAsync();
                    token = JsonConvert.DeserializeObject<LoginSuccessResult>(raw);
                    SetToken(token.access_token);

                }
                catch (Exception error)
                {
                    token = null;
                    response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
#if DEBUG
                    response.Content = new StringContent(string.Concat(error.Message, error.StackTrace));
#else
                    response.Content = new StringContent("Oops, an error occured, please try again");
#endif
                }
                finally
                {
                    EndPost?.Invoke();

                }
                return token;
            }
        }

        public async Task<LoginSuccessResult> FacebookLogin(string FacebookToken, string bodyWeight, string massUnit)
        {
            HttpResponseMessage response;
            LoginSuccessResult token = null;
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(BaseUrl);
                    var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type","facebook"),
                        new KeyValuePair<string, string>("accesstoken", FacebookToken),
                        new KeyValuePair<string, string>("provider", "facebook"),
                        new KeyValuePair<string, string>("bodyweight", bodyWeight),
                        new KeyValuePair<string, string>("massunit", massUnit)
                    };
                    var content = new FormUrlEncodedContent(body);
                    StartPost?.Invoke();
                    response = await client.PostAsync("token", content);

                    string raw = await response.Content.ReadAsStringAsync();
                    token = JsonConvert.DeserializeObject<LoginSuccessResult>(raw);
                    SetToken(token.access_token);

                }
                catch (Exception error)
                {
                    token = null;
                    response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
#if DEBUG
                    response.Content = new StringContent(string.Concat(error.Message, error.StackTrace));
#else
                    response.Content = new StringContent("Oops, an error occured, please try again");
#endif
                }
                finally
                {
                    EndPost?.Invoke();

                }
                return token;
            }
        }


        public async Task<BooleanModel> RegisterUser(RegisterModel model)
        {
            return await PostJson<BooleanModel>("api/Account/Register", model);
        }

        //
        public async Task<BooleanModel> RegisterUserBeforeDemo(RegisterModel model)
        {
            return await PostJsonWithoughtLoader<BooleanModel>("api/Account/RegisterUserBeforeDemo", model);
        }

        public async Task<UserInfosModel> RegisterUserAfterDemo(RegisterModel model)
        {
            return await PostJsonWithoughtLoader<UserInfosModel>("api/Account/RegisterUserAfterDemoV2", model);
        }
//
        public async Task<UserInfosModel> RegisterWithUser(RegisterModel model)
        {
            return await PostJson<UserInfosModel>("api/Account/RegisterWithUser", model);
        }

        public async Task<BooleanModel> IsV1User()
        {
            return new BooleanModel() { Result = true, IsMealPlan = true, IsTraining = true };
        }

        public async Task<BooleanModel> IsV1UserWithoutLoader()
        {
            return new BooleanModel() { Result = true, IsMealPlan = true, IsTraining = true };
        }

        public async Task<List<UserWeight>> GetUserWeights()
        {
            return await PostJsonWithoughtLoader<List<UserWeight>>("api/Account/GetUserWeights", null);
        }
        
        
        public async Task<DmmMeal> AddUserMealAsync(DmmMeal model)
        {
            return await PostJson<DmmMeal>("api/Account/PostDmmMeal", model);
        }

        public async Task<MealPlanModel> AddMealPlanAsync(DmmMealPlan model)
        {
            return await PostJson<MealPlanModel>("api/Account/PostDmmMealPlan", model);
        }
        
        public async Task<BooleanModel> IsMonthlyUser(bool isSilent=false)
        {
            return new BooleanModel() { Result = true, IsMealPlan = true, IsTraining = true  };
        }

        public async Task<BooleanModel> IsV1UserWithoutLoaderQuick()
        {
            return new BooleanModel() { Result = true, IsMealPlan = true, IsTraining = true };
        }

        public async Task<BooleanModel> SubscriptionDetail(SubscriptionModel model)
        {
            return new BooleanModel() { Result = true, IsMealPlan = true, IsTraining = true };
        }

        private async Task<T> PostJson<T>(string route, object model)
        {
            return await PostJson<T>(route, model, 1);
        }

        private async Task<T> PostJsonWithoughtLoader<T>(string route, object model)
        {
            return await PostJsonWithoughtLoader<T>(route, model, 1);
        }
    
        public async Task<WorkoutTemplateModel> GetUserCustomizedCurrentWorkoutWithoutLoader(long workoutid)
        {
            return await PostJsonWithoughtLoader<WorkoutTemplateModel>("api/Workout/GetUserCustomizedCurrentWorkout", workoutid);
        }
        public async Task<GetUserWorkoutTemplateResponseModel> GetCustomizedUserWorkout(EquipmentModel model)
        {
            return await PostJson<GetUserWorkoutTemplateResponseModel>("api/Workout/GetCustomizedUserWorkout", model);
        }


        private async Task<T> PostJson<T>(string route, object model, int attempNr)
        {
            //Analytics.TrackEvent("PostJson", new Dictionary<string, string>() { { "route", route } });
            HttpResponseMessage httpResponse;
            using (var client = new HttpClient())
            {

                StartPost?.Invoke();

                try
                {
                    client.BaseAddress = new Uri(BaseUrl);
                    if (!string.IsNullOrEmpty(_token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _token);

                    HttpContent content = new StringContent(JsonConvert.SerializeObject(model),
                                                            Encoding.UTF8,
                                                            "application/json");
                    System.Diagnostics.Debug.WriteLine($"From With Loader: {route}");
                    System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(model));
                    if (route.Contains("GetLogAverageWithSetsV2") || route.Contains("FetchInbox") || route.Contains("FetchChatBox"))
                    {}
                    else
                    {
                        client.Timeout = TimeSpan.FromSeconds(15);
                    }
                        
                    httpResponse = await client.PostAsync(route, content);

                    //httpResponse = await client.PostAsJsonAsync(route, model);
                }
               
                catch (TaskCanceledException error)
                {
                    AlertConfig ShowAlertPopUp = new AlertConfig()
                    {
                        Title = "Connection error",
                        Message = AppResources.PleaseCheckInternetConnection,
                        AndroidStyleId = Xamarin.Forms.DependencyService.Get<IStyles>()
                            .GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = "Try again",
                        // CancelText = "Cancel",

                    };
                     await UserDialogs.Instance.AlertAsync(ShowAlertPopUp);
                    // if (actionOk)
                    // {
                        // return await PostJson<T>(route, model, 1);
//                     }
//                     else
//                     {
                        httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
#if DEBUG
                        httpResponse.Content =
                            new StringContent(string.Concat(error.Message, error.StackTrace));
#else
                    httpResponse.Content = new StringContent("Oops, an error occured, please try again");
#endif
//                     }
//                     if (attempNr >= 0 && route != "api/Account/RegisterUserAfterDemo")
//                         return await PostJson<T>(route, model, attempNr - 1);
//                     httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
// #if DEBUG
//                     httpResponse.Content = new StringContent(string.Concat(error.Message, error.StackTrace));
// #else
//                     httpResponse.Content = new StringContent("Oops, an error occured, please try again");
// #endif
                }
                
                catch (Exception error)
                {
                    httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
#if DEBUG
                    httpResponse.Content = new StringContent(string.Concat(error.Message, error.StackTrace));
#else
                    httpResponse.Content = new StringContent("Oops, an error occured, please try again");
#endif
                }

                EndPost?.Invoke();
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.OK:
                        break;
                    case HttpStatusCode.Unauthorized:
                        Unauthorized?.Invoke();
                        break;
                    default:

                        break;
                }

                ApiResponse apiResponse;

                try
                {
                    string rawResponse = await httpResponse.Content.ReadAsStringAsync();
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse>(rawResponse);
                }
                catch (Exception error)
                {
#if MOBILE_CENTER
                    //Analytics.TrackEvent("PostJson.DeserializeObjectException", new Dictionary<string, string>() { { "route", route }, { "ExceptionMessage", error.Message } });
#endif
#if DEBUG
                    apiResponse = new ApiResponse(HttpStatusCode.InternalServerError, new ErrorResponse() { Ex = error, Response = httpResponse }, error.Message);
#else
                    apiResponse = new ApiResponse(HttpStatusCode.InternalServerError, "", "Oops, an error occured, please try again later");
#endif
                }

                if (apiResponse.Result == null)
                {
                    return default(T); //(T)Activator.CreateInstance<T>();
                }

                if (apiResponse.Result.GetType() == typeof(JObject))
                    return ((JObject)apiResponse.Result).ToObject<T>();

                try
                {
                    return ((JArray)apiResponse.Result).ToObject<T>();
                }
                catch (Exception)
                {
#if MOBILE_CENTER
                    //Analytics.TrackEvent("PostJson.UnknownJsonType", new Dictionary<string, string>() { { "route", route } });
#endif
                    return default(T);
                }
            }
        }

        private async Task<T> PostJsonWithoughtLoader<T>(string route, object model, int attempNr)
        {
            //Analytics.TrackEvent("PostJson", new Dictionary<string, string>() { { "route", route } });
            HttpResponseMessage httpResponse;
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(BaseUrl);
                    if (!string.IsNullOrEmpty(_token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _token);

                    HttpContent content = new StringContent(JsonConvert.SerializeObject(model),
                        Encoding.UTF8,
                        "application/json");

                    System.Diagnostics.Debug.WriteLine($"From WithoughtLoader: {route}");
                    httpResponse = await client.PostAsync(route, content);

                    //httpResponse = await client.PostAsJsonAsync(route, model);
                }
                catch (TaskCanceledException error)
                {
                    if (attempNr >= 0)
                        return await PostJsonWithoughtLoader<T>(route, model, attempNr - 1);
                    httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
#if DEBUG
                    httpResponse.Content = new StringContent(string.Concat(error.Message, error.StackTrace));
#else
                    httpResponse.Content = new StringContent("Oops, an error occured, please try again");
#endif
                }
                catch (Exception error)
                {
                    httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
#if DEBUG
                    httpResponse.Content = new StringContent(string.Concat(error.Message, error.StackTrace));
#else
                    httpResponse.Content = new StringContent("Oops, an error occured, please try again");
#endif
                }


                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.OK:
                        break;
                    case HttpStatusCode.Unauthorized:
                        Unauthorized?.Invoke();
                        break;
                    default:

                        break;
                }

                ApiResponse apiResponse;

                try
                {
                    string rawResponse = await httpResponse.Content.ReadAsStringAsync();
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse>(rawResponse);
                }
                catch (Exception error)
                {
#if MOBILE_CENTER
                    //Analytics.TrackEvent("PostJson.DeserializeObjectException", new Dictionary<string, string>() { { "route", route }, { "ExceptionMessage", error.Message } });
#endif
#if DEBUG
                    apiResponse = new ApiResponse(HttpStatusCode.InternalServerError, new ErrorResponse() { Ex = error, Response = httpResponse }, error.Message);
#else
                    apiResponse = new ApiResponse(HttpStatusCode.InternalServerError, "", "Oops, an error occured, please try again later");
#endif
                }

                if (apiResponse.Result == null)
                {
                    return default(T); //(T)Activator.CreateInstance<T>();
                }

                if (apiResponse.Result.GetType() == typeof(JObject))
                    return ((JObject)apiResponse.Result).ToObject<T>();

                try
                {
                    return ((JArray)apiResponse.Result).ToObject<T>();
                }
                catch (Exception)
                {
        #if MOBILE_CENTER
                    //Analytics.TrackEvent("PostJson.UnknownJsonType", new Dictionary<string, string>() { { "route", route } });
        #endif
                    return default(T);
                }
            }
        }

         private async Task<T> PostJsonWithoughtLoaderTimeout<T>(string route, object model, int attempNr)
        {
            //Analytics.TrackEvent("PostJson", new Dictionary<string, string>() { { "route", route } });
            HttpResponseMessage httpResponse;
            using (var client = new HttpClient())
            {



                try
                {
                    client.BaseAddress = new Uri(BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(2);
                    if (!string.IsNullOrEmpty(_token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _token);
                    System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(model));
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(model),
                        Encoding.UTF8,
                        "application/json");

                    System.Diagnostics.Debug.WriteLine($"From WithoughtLoader Timeout: {route}");
                    httpResponse = await client.PostAsync(route, content);

                    //httpResponse = await client.PostAsJsonAsync(route, model);
                }
               
               
                catch (TaskCanceledException ex)
                {
                    
                    AlertConfig ShowAlertPopUp = new AlertConfig()
                    {
                        Title = "Connection error",
                        Message = AppResources.PleaseCheckInternetConnection,
                        AndroidStyleId = Xamarin.Forms.DependencyService.Get<IStyles>()
                            .GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = "Try again",
                        // CancelText = "Cancel",

                    };
                    await UserDialogs.Instance.AlertAsync(ShowAlertPopUp);
                    httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
#if DEBUG
                    httpResponse.Content = new StringContent(string.Concat(ex.Message, ex.StackTrace));
#else
                    httpResponse.Content = new StringContent("Oops, an error occured, please try again");
#endif
                    
                }
                catch (Exception error)
                {
                    httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
#if DEBUG
                    httpResponse.Content = new StringContent(string.Concat(error.Message, error.StackTrace));
#else
                    httpResponse.Content = new StringContent("Oops, an error occured, please try again");
#endif
                }


                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.OK:
                        break;
                    case HttpStatusCode.Unauthorized:
                        Unauthorized?.Invoke();
                        break;
                    default:

                        break;
                }

                ApiResponse apiResponse;

                try
                {
                    string rawResponse = await httpResponse.Content.ReadAsStringAsync();
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse>(rawResponse);
                }
                catch (Exception error)
                {
#if MOBILE_CENTER
                    //Analytics.TrackEvent("PostJson.DeserializeObjectException", new Dictionary<string, string>() { { "route", route }, { "ExceptionMessage", error.Message } });
#endif
#if DEBUG
                    apiResponse = new ApiResponse(HttpStatusCode.InternalServerError, new ErrorResponse() { Ex = error, Response = httpResponse }, error.Message);
#else
                    apiResponse = new ApiResponse(HttpStatusCode.InternalServerError, "", "Oops, an error occured, please try again later");
#endif
                }

                if (apiResponse.Result == null)
                {
                    return default(T); //(T)Activator.CreateInstance<T>();
                }

                if (apiResponse.Result.GetType() == typeof(JObject))
                    return ((JObject)apiResponse.Result).ToObject<T>();

                try
                {
                    return ((JArray)apiResponse.Result).ToObject<T>();
                }
                catch (Exception)
                {
        #if MOBILE_CENTER
                    //Analytics.TrackEvent("PostJson.UnknownJsonType", new Dictionary<string, string>() { { "route", route } });
        #endif
                    return default(T);
                }
            }
        }

        public async Task<BooleanModel> ResetExercise(ExerciceModel model)
        {
            return await PostJson<BooleanModel>("api/Exercise/ResetExercise", model);
        }

        public async Task<BooleanModel> ResetAllExercise()
        {
            return await PostJson<BooleanModel>("api/Exercise/ResetAllExercise", null);
        }

        public async Task<BooleanModel> RenameWorkoutTemplateGroup(WorkoutTemplateGroupModel model)
        {
            return await PostJson<BooleanModel>("api/Workout/RenameWorkoutTemplateGroup", model);
        }

        public async Task<BooleanModel> CreateNewWorkoutTemplate(WorkoutTemplateModel model)
        {
            return await PostJson<BooleanModel>("api/Workout/CreateNewWorkoutTemplate", model);
        }
        //
        public async Task<BooleanModel> CreateNewUserWorkoutTemplate(WorkoutTemplateModel model)
        {
            return await PostJson<BooleanModel>("api/Workout/CreateNewUserWorkoutTemplate", model);
        }
        public async Task<BooleanModel> RenameExercise(ExerciceModel model)
        {
            return await PostJson<BooleanModel>("api/Exercise/RenameExercise", model);
        }

        public async Task<BooleanModel> DeleteWorkoutTemplateGroupModel(WorkoutTemplateGroupModel model)
        {
            return await PostJson<BooleanModel>("api/Workout/DeleteWorkoutGroupTemplate", model);
        }

        public async Task<BooleanModel> DeleteExercise(ExerciceModel model)
        {
            return await PostJson<BooleanModel>("api/Exercise/DeleteExercise", model);
        }

        public async Task<BooleanModel> DeleteAccount()
        {
            return await PostJson<BooleanModel>("api/Account/DeleteUser", null);
        }
        public async Task<BooleanModel> IsEmailAlreadyExist(IsEmailAlreadyExistModel email)
        {
            return await PostJson<BooleanModel>("api/Account/IsEmailAlreadyExist", email);
        }

        public async Task<BooleanModel> IsEmailAlreadyExistWithoutLoader(IsEmailAlreadyExistModel email)
        {
            return await PostJsonWithoughtLoader<BooleanModel>("api/Account/IsEmailAlreadyExist", email);
        }

        public async Task<BooleanModel> UpdateEmail(IsEmailAlreadyExistModel email)
        {
            return await PostJson<BooleanModel>("api/Account/UpdateEmail", email);
        }

        public async Task<GetUserProgramInfoResponseModel> GetUserProgramInfo()
        {
            return await PostJson<GetUserProgramInfoResponseModel>("api/Workout/GetUserProgramInfo", null);
        }

        public async Task<GetUserExerciseResponseModel> GetUserExercise(string userName)
        {
            return await PostJson<GetUserExerciseResponseModel>("api/Exercise/GetUserExercise", userName);
        }


        public async Task<GetUserExerciseResponseModel> GetCustomExerciseForUser(string userName)
        {
            return await PostJsonWithoughtLoader<GetUserExerciseResponseModel>("api/Exercise/GetCustomExerciseForUser", userName);
        }
        public async Task<GetUserExerciseResponseModel> GetCustomExerciseForUserWithLoader(string userName)
        {
            return await PostJson<GetUserExerciseResponseModel>("api/Exercise/GetCustomExerciseForUser", userName);
        }

        public async Task<BooleanModel> IsAlive()
        {
            return await PostJson<BooleanModel>("api/Account/IsAlive", null);
        }

        public async Task<GetUserWorkoutLogAverageResponse> GetUserWorkoutLogAverage()
        {
            return await PostJson<GetUserWorkoutLogAverageResponse>("/api/WorkoutLog/GetUserWorkoutLogAverageV2", null);
        }
        public async Task<GetUserWorkoutLogAverageResponse> GetUserWorkoutLogAverageWithoutLoader()
        {
            return await PostJsonWithoughtLoader<GetUserWorkoutLogAverageResponse>("/api/WorkoutLog/GetUserWorkoutLogAverageV2", null);
        }
        //
        public async Task<GetUserWorkoutLogAverageResponse> GetUserWorkoutLogAverageWithUserStats()
        {
            return await PostJson<GetUserWorkoutLogAverageResponse>("/api/WorkoutLog/GetUserWorkoutLogAverageWithUserStatsV2", null);
        }
        public async Task<GetUserWorkoutLogAverageResponse> GetUserWorkoutLogAverageWithUserStatsWithoutLoader()
        {
            return await PostJsonWithoughtLoader<GetUserWorkoutLogAverageResponse>("/api/WorkoutLog/GetUserWorkoutLogAverageWithUserStatsV2", null);
        }
        public async Task<GetUserWorkoutLogAverageResponse> GetUserWorkoutLogAverageWithSets(TimeZoneInfo local)
        {
            //GetUserWorkoutLogAverageWithSetsTimeZoneInfo
            return await PostJson<GetUserWorkoutLogAverageResponse>("/api/WorkoutLog/GetUserWorkoutLogAverageWithSetsTimeZoneInfoV2", local);
        }

        public async Task<GetUserWorkoutLogAverageResponse> GetUserWorkoutLogAverageWithSetsWithoutLoader(TimeZoneInfo local)
        {
            return await PostJsonWithoughtLoader<GetUserWorkoutLogAverageResponse>("/api/WorkoutLog/GetUserWorkoutLogAverageWithSetsTimeZoneInfoV2", local);
        }

        public async Task<GetUserWorkoutLogAverageResponse> GetUserWorkoutProgramTimeZoneInfoWithoutLoader(TimeZoneInfo local)
        {
            return await PostJsonWithoughtLoader<GetUserWorkoutLogAverageResponse>("/api/WorkoutLog/GetUserWorkoutProgramTimeZoneInfo", local);
        }

        public async Task<GetUserWorkoutLogAverageResponse> GetUserWorkoutProgramTimeZoneInfo(TimeZoneInfo local)
        {
            return await PostJson<GetUserWorkoutLogAverageResponse>("/api/WorkoutLog/GetUserWorkoutProgramTimeZoneInfo", local);
        }
        public async Task<GetUserWorkoutLogAverageResponse> GetLogAverageWithSetsWithoughtLoader()
        {
            return await PostJsonWithoughtLoader<GetUserWorkoutLogAverageResponse>("/api/WorkoutLog/GetLogAverageWithSetsV2", null);
        }

        public async Task<GetUserWorkoutLogAverageResponse> GetLogAverageWithSets()
        {
            return await PostJson<GetUserWorkoutLogAverageResponse>("/api/WorkoutLog/GetLogAverageWithSetsV2", null);
        }

        public async Task<BooleanModel> SetRepsMinimum(SingleIntegerModel val)
        {
            return await PostJson<BooleanModel>("/api/Account/SetRepsMinimum", val);
        }

        public async Task<BooleanModel> SetRepsMaximum(SingleIntegerModel val)
        {
            return await PostJson<BooleanModel>("/api/Account/SetRepsMaximum", val);
        }
        public async Task<BooleanModel> SetRepsRangeType(SingleIntegerModel val)
        {
            return await PostJson<BooleanModel>("/api/Account/SetRepsRangeType", val);
        }
        //
        public async Task<SingleIntegerModel> GetRepsMinimum()
        {
            return await PostJson<SingleIntegerModel>("/api/Account/GetRepsMinimum", null);
        }

        public async Task<SingleIntegerModel> GetRepsMaximum()
        {
            return await PostJson<SingleIntegerModel>("/api/Account/GetRepsMaximum", null);
        }
    }
}