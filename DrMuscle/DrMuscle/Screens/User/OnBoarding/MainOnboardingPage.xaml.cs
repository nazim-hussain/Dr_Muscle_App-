using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using Xamarin.Forms;
using DrMuscle.Resx;
using DrMuscle.Constants;
using System.Linq;
using Acr.UserDialogs;
using DrMuscleWebApiSharedModel;
using DrMuscle.Screens.Workouts;
using System.Globalization;
using Xamarin.Forms.PancakeView;
using DrMuscle.Controls;
using DrMuscle.Entity;
using Plugin.GoogleClient;
using DrMuscle.Dependencies;
using Plugin.GoogleClient.Shared;
using Rg.Plugins.Popup.Services;
using DrMuscle.Views;
using Xamarin.Essentials;
using Plugin.Connectivity;
using DrMuscle.Screens.Demo;
using DrMuscle.Services;
using Newtonsoft.Json;
using DrMuscle.Message;
using DrMuscle.Effects;
using System.Threading;
using DrMuscle.Model;
using DrMuscle.OnBoarding;
using Sentry;

namespace DrMuscle.Screens.User.OnBoarding
{
    public partial class MainOnboardingPage : DrMusclePage
    {
        public ObservableCollection<BotModel> BotList = new ObservableCollection<BotModel>();
        public LearnMore learnMore = new LearnMore();
        bool ManMoreMuscle = false;
        bool ManLessFat = false;
        bool ManBetterHealth = false;
        bool ManStorngerSexDrive = false;
        private readonly IGoogleClientManager _googleClientManager;
        IFacebookManager _manager;
        private IAppleSignInService appleSignInService;
        bool FemaleMoreEnergy = false;
        bool FemaleToned = false;
        bool IsHumanSupport = false;
        bool ShouldAnimate = false;
        bool IsBodyweightPopup = false;
        bool IsIncludeCardio = false;
        bool IsIncludeMobility = false;
        bool? IsRecommendedReminder = false;
        bool isDing = false;
        string mobilityLevel;
        bool? SetStyle = false;
        bool IsPyramid = false;
        bool? isBetaExperience = null;
        public static bool? IsRealBetaExperience = null;
        string focusText = "", mainGoal = "";
        private IFirebase _firebase;
        Picker AgePicker, olderAgePicker;
        Picker BodyweightPicker;
        Picker LevelPicker;
        Picker BodyPartPicker;
        public static bool IsMovedToLogin = false;
        MultiUnityWeight IncrementUnit = null;
        bool IsEquipment = false;
        bool IsPully = false;
        bool isDumbbells = false;
        bool IsPlates = false;
        bool IsChinupBar = false;
        bool isProcessing = false;
        bool isBetaFromFirebase=false;
        bool isUpdatePopupShown = false;
        IDisposable emailDisposible;
        IDisposable firstnameDisposible;
        IDisposable passwordDisposible;
        private int workoutDurationLenggth = 0;
        string bodypartName = "";
        string domainList = "AAA,AARP,ABARTH,ABB,ABBOTT,ABBVIE,ABC,ABLE,ABOGADO,ABUDHABI,AC,ACADEMY,ACCENTURE,ACCOUNTANT,ACCOUNTANTS,ACO,ACTOR,AD,ADAC,ADS,ADULT,AE,AEG,AERO,AETNA,AF,AFL,AFRICA,AG,AGAKHAN,AGENCY,AI,AIG,AIRBUS,AIRFORCE,AIRTEL,AKDN,AL,ALFAROMEO,ALIBABA,ALIPAY,ALLFINANZ,ALLSTATE,ALLY,ALSACE,ALSTOM,AM,AMAZON,AMERICANEXPRESS,AMERICANFAMILY,AMEX,AMFAM,AMICA,AMSTERDAM,ANALYTICS,ANDROID,ANQUAN,ANZ,AO,AOL,APARTMENTS,APP,APPLE,AQ,AQUARELLE,AR,ARAB,ARAMCO,ARCHI,ARMY,ARPA,ART,ARTE,AS,ASDA,ASIA,ASSOCIATES,AT,ATHLETA,ATTORNEY,AU,AUCTION,AUDI,AUDIBLE,AUDIO,AUSPOST,AUTHOR,AUTO,AUTOS,AVIANCA,AW,AWS,AX,AXA,AZ,AZURE,BA,BABY,BAIDU,BANAMEX,BANANAREPUBLIC,BAND,BANK,BAR,BARCELONA,BARCLAYCARD,BARCLAYS,BAREFOOT,BARGAINS,BASEBALL,BASKETBALL,BAUHAUS,BAYERN,BB,BBC,BBT,BBVA,BCG,BCN,BD,BE,BEATS,BEAUTY,BEER,BENTLEY,BERLIN,BEST,BESTBUY,BET,BF,BG,BH,BHARTI,BI,BIBLE,BID,BIKE,BING,BINGO,BIO,BIZ,BJ,BLACK,BLACKFRIDAY,BLOCKBUSTER,BLOG,BLOOMBERG,BLUE,BM,BMS,BMW,BN,BNPPARIBAS,BO,BOATS,BOEHRINGER,BOFA,BOM,BOND,BOO,BOOK,BOOKING,BOSCH,BOSTIK,BOSTON,BOT,BOUTIQUE,BOX,BR,BRADESCO,BRIDGESTONE,BROADWAY,BROKER,BROTHER,BRUSSELS,BS,BT,BUILD,BUILDERS,BUSINESS,BUY,BUZZ,BV,BW,BY,BZ,BZH,CA,CAB,CAFE,CAL,CALL,CALVINKLEIN,CAM,CAMERA,CAMP,CANON,CAPETOWN,CAPITAL,CAPITALONE,CAR,CARAVAN,CARDS,CARE,CAREER,CAREERS,CARS,CASA,CASE,CASH,CASINO,CAT,CATERING,CATHOLIC,CBA,CBN,CBRE,CBS,CC,CD,CENTER,CEO,CERN,CF,CFA,CFD,CG,CH,CHANEL,CHANNEL,CHARITY,CHASE,CHAT,CHEAP,CHINTAI,CHRISTMAS,CHROME,CHURCH,CI,CIPRIANI,CIRCLE,CISCO,CITADEL,CITI,CITIC,CITY,CITYEATS,CK,CL,CLAIMS,CLEANING,CLICK,CLINIC,CLINIQUE,CLOTHING,CLOUD,CLUB,CLUBMED,CM,CN,CO,COACH,CODES,COFFEE,COLLEGE,COLOGNE,COM,COMCAST,COMMBANK,COMMUNITY,COMPANY,COMPARE,COMPUTER,COMSEC,CONDOS,CONSTRUCTION,CONSULTING,CONTACT,CONTRACTORS,COOKING,COOKINGCHANNEL,COOL,COOP,CORSICA,COUNTRY,COUPON,COUPONS,COURSES,CPA,CR,CREDIT,CREDITCARD,CREDITUNION,CRICKET,CROWN,CRS,CRUISE,CRUISES,CU,CUISINELLA,CV,CW,CX,CY,CYMRU,CYOU,CZ,DABUR,DAD,DANCE,DATA,DATE,DATING,DATSUN,DAY,DCLK,DDS,DE,DEAL,DEALER,DEALS,DEGREE,DELIVERY,DELL,DELOITTE,DELTA,DEMOCRAT,DENTAL,DENTIST,DESI,DESIGN,DEV,DHL,DIAMONDS,DIET,DIGITAL,DIRECT,DIRECTORY,DISCOUNT,DISCOVER,DISH,DIY,DJ,DK,DM,DNP,DO,DOCS,DOCTOR,DOG,DOMAINS,DOT,DOWNLOAD,DRIVE,DTV,DUBAI,DUNLOP,DUPONT,DURBAN,DVAG,DVR,DZ,EARTH,EAT,EC,ECO,EDEKA,EDU,EDUCATION,EE,EG,EMAIL,EMERCK,ENERGY,ENGINEER,ENGINEERING,ENTERPRISES,EPSON,EQUIPMENT,ER,ERICSSON,ERNI,ES,ESQ,ESTATE,ET,ETISALAT,EU,EUROVISION,EUS,EVENTS,EXCHANGE,EXPERT,EXPOSED,EXPRESS,EXTRASPACE,FAGE,FAIL,FAIRWINDS,FAITH,FAMILY,FAN,FANS,FARM,FARMERS,FASHION,FAST,FEDEX,FEEDBACK,FERRARI,FERRERO,FI,FIAT,FIDELITY,FIDO,FILM,FINAL,FINANCE,FINANCIAL,FIRE,FIRESTONE,FIRMDALE,FISH,FISHING,FIT,FITNESS,FJ,FK,FLICKR,FLIGHTS,FLIR,FLORIST,FLOWERS,FLY,FM,FO,FOO,FOOD,FOODNETWORK,FOOTBALL,FORD,FOREX,FORSALE,FORUM,FOUNDATION,FOX,FR,FREE,FRESENIUS,FRL,FROGANS,FRONTDOOR,FRONTIER,FTR,FUJITSU,FUN,FUND,FURNITURE,FUTBOL,FYI,GA,GAL,GALLERY,GALLO,GALLUP,GAME,GAMES,GAP,GARDEN,GAY,GB,GBIZ,GD,GDN,GE,GEA,GENT,GENTING,GEORGE,GF,GG,GGEE,GH,GI,GIFT,GIFTS,GIVES,GIVING,GL,GLASS,GLE,GLOBAL,GLOBO,GM,GMAIL,GMBH,GMO,GMX,GN,GODADDY,GOLD,GOLDPOINT,GOLF,GOO,GOODYEAR,GOOG,GOOGLE,GOP,GOT,GOV,GP,GQ,GR,GRAINGER,GRAPHICS,GRATIS,GREEN,GRIPE,GROCERY,GROUP,GS,GT,GU,GUARDIAN,GUCCI,GUGE,GUIDE,GUITARS,GURU,GW,GY,HAIR,HAMBURG,HANGOUT,HAUS,HBO,HDFC,HDFCBANK,HEALTH,HEALTHCARE,HELP,HELSINKI,HERE,HERMES,HGTV,HIPHOP,HISAMITSU,HITACHI,HIV,HK,HKT,HM,HN,HOCKEY,HOLDINGS,HOLIDAY,HOMEDEPOT,HOMEGOODS,HOMES,HOMESENSE,HONDA,HORSE,HOSPITAL,HOST,HOSTING,HOT,HOTELES,HOTELS,HOTMAIL,HOUSE,HOW,HR,HSBC,HT,HU,HUGHES,HYATT,HYUNDAI,IBM,ICBC,ICE,ICU,ID,IE,IEEE,IFM,IKANO,IL,IM,IMAMAT,IMDB,IMMO,IMMOBILIEN,IN,INC,INDUSTRIES,INFINITI,INFO,ING,INK,INSTITUTE,INSURANCE,INSURE,INT,INTERNATIONAL,INTUIT,INVESTMENTS,IO,IPIRANGA,IQ,IR,IRISH,IS,ISMAILI,IST,ISTANBUL,IT,ITAU,ITV,JAGUAR,JAVA,JCB,JE,JEEP,JETZT,JEWELRY,JIO,JLL,JM,JMP,JNJ,JO,JOBS,JOBURG,JOT,JOY,JP,JPMORGAN,JPRS,JUEGOS,JUNIPER,KAUFEN,KDDI,KE,KERRYHOTELS,KERRYLOGISTICS,KERRYPROPERTIES,KFH,KG,KH,KI,KIA,KIDS,KIM,KINDER,KINDLE,KITCHEN,KIWI,KM,KN,KOELN,KOMATSU,KOSHER,KP,KPMG,KPN,KR,KRD,KRED,KUOKGROUP,KW,KY,KYOTO,KZ,LA,LACAIXA,LAMBORGHINI,LAMER,LANCASTER,LANCIA,LAND,LANDROVER,LANXESS,LASALLE,LAT,LATINO,LATROBE,LAW,LAWYER,LB,LC,LDS,LEASE,LECLERC,LEFRAK,LEGAL,LEGO,LEXUS,LGBT,LI,LIDL,LIFE,LIFEINSURANCE,LIFESTYLE,LIGHTING,LIKE,LILLY,LIMITED,LIMO,LINCOLN,LINDE,LINK,LIPSY,LIVE,LIVING,LK,LLC,LLP,LOAN,LOANS,LOCKER,LOCUS,LOFT,LOL,LONDON,LOTTE,LOTTO,LOVE,LPL,LPLFINANCIAL,LR,LS,LT,LTD,LTDA,LU,LUNDBECK,LUXE,LUXURY,LV,LY,MA,MACYS,MADRID,MAIF,MAISON,MAKEUP,MAN,MANAGEMENT,MANGO,MAP,MARKET,MARKETING,MARKETS,MARRIOTT,MARSHALLS,MASERATI,MATTEL,MBA,MC,MCKINSEY,MD,ME,MED,MEDIA,MEET,MELBOURNE,MEME,MEMORIAL,MEN,MENU,MERCKMSD,MG,MH,MIAMI,MICROSOFT,MIL,MINI,MINT,MIT,MITSUBISHI,MK,ML,MLB,MLS,MM,MMA,MN,MO,MOBI,MOBILE,MODA,MOE,MOI,MOM,MONASH,MONEY,MONSTER,MORMON,MORTGAGE,MOSCOW,MOTO,MOTORCYCLES,MOV,MOVIE,MP,MQ,MR,MS,MSD,MT,MTN,MTR,MU,MUSEUM,MUSIC,MUTUAL,MV,MW,MX,MY,MZ,NA,NAB,NAGOYA,NAME,NATURA,NAVY,NBA,NC,NE,NEC,NET,NETBANK,NETFLIX,NETWORK,NEUSTAR,NEW,NEWS,NEXT,NEXTDIRECT,NEXUS,NF,NFL,NG,NGO,NHK,NI,NICO,NIKE,NIKON,NINJA,NISSAN,NISSAY,NL,NO,NOKIA,NORTHWESTERNMUTUAL,NORTON,NOW,NOWRUZ,NOWTV,NP,NR,NRA,NRW,NTT,NU,NYC,NZ,OBI,OBSERVER,OFFICE,OKINAWA,OLAYAN,OLAYANGROUP,OLDNAVY,OLLO,OM,OMEGA,ONE,ONG,ONL,ONLINE,OOO,OPEN,ORACLE,ORANGE,ORG,ORGANIC,ORIGINS,OSAKA,OTSUKA,OTT,OVH,PA,PAGE,PANASONIC,PARIS,PARS,PARTNERS,PARTS,PARTY,PASSAGENS,PAY,PCCW,PE,PET,PF,PFIZER,PG,PH,PHARMACY,PHD,PHILIPS,PHONE,PHOTO,PHOTOGRAPHY,PHOTOS,PHYSIO,PICS,PICTET,PICTURES,PID,PIN,PING,PINK,PIONEER,PIZZA,PK,PL,PLACE,PLAY,PLAYSTATION,PLUMBING,PLUS,PM,PN,PNC,POHL,POKER,POLITIE,PORN,POST,PR,PRAMERICA,PRAXI,PRESS,PRIME,PRO,PROD,PRODUCTIONS,PROF,PROGRESSIVE,PROMO,PROPERTIES,PROPERTY,PROTECTION,PRU,PRUDENTIAL,PS,PT,PUB,PW,PWC,PY,QA,QPON,QUEBEC,QUEST,RACING,RADIO,RE,READ,REALESTATE,REALTOR,REALTY,RECIPES,RED,REDSTONE,REDUMBRELLA,REHAB,REISE,REISEN,REIT,RELIANCE,REN,RENT,RENTALS,REPAIR,REPORT,REPUBLICAN,REST,RESTAURANT,REVIEW,REVIEWS,REXROTH,RICH,RICHARDLI,RICOH,RIL,RIO,RIP,RO,ROCHER,ROCKS,RODEO,ROGERS,ROOM,RS,RSVP,RU,RUGBY,RUHR,RUN,RW,RWE,RYUKYU,SA,SAARLAND,SAFE,SAFETY,SAKURA,SALE,SALON,SAMSCLUB,SAMSUNG,SANDVIK,SANDVIKCOROMANT,SANOFI,SAP,SARL,SAS,SAVE,SAXO,SB,SBI,SBS,SC,SCA,SCB,SCHAEFFLER,SCHMIDT,SCHOLARSHIPS,SCHOOL,SCHULE,SCHWARZ,SCIENCE,SCOT,SD,SE,SEARCH,SEAT,SECURE,SECURITY,SEEK,SELECT,SENER,SERVICES,SES,SEVEN,SEW,SEX,SEXY,SFR,SG,SH,SHANGRILA,SHARP,SHAW,SHELL,SHIA,SHIKSHA,SHOES,SHOP,SHOPPING,SHOUJI,SHOW,SHOWTIME,SI,SILK,SINA,SINGLES,SITE,SJ,SK,SKI,SKIN,SKY,SKYPE,SL,SLING,SM,SMART,SMILE,SN,SNCF,SO,SOCCER,SOCIAL,SOFTBANK,SOFTWARE,SOHU,SOLAR,SOLUTIONS,SONG,SONY,SOY,SPA,SPACE,SPORT,SPOT,SR,SRL,SS,ST,STADA,STAPLES,STAR,STATEBANK,STATEFARM,STC,STCGROUP,STOCKHOLM,STORAGE,STORE,STREAM,STUDIO,STUDY,STYLE,SU,SUCKS,SUPPLIES,SUPPLY,SUPPORT,SURF,SURGERY,SUZUKI,SV,SWATCH,SWISS,SX,SY,SYDNEY,SYSTEMS,SZ,TAB,TAIPEI,TALK,TAOBAO,TARGET,TATAMOTORS,TATAR,TATTOO,TAX,TAXI,TC,TCI,TD,TDK,TEAM,TECH,TECHNOLOGY,TEL,TEMASEK,TENNIS,TEVA,TF,TG,TH,THD,THEATER,THEATRE,TIAA,TICKETS,TIENDA,TIFFANY,TIPS,TIRES,TIROL,TJ,TJMAXX,TJX,TK,TKMAXX,TL,TM,TMALL,TN,TO,TODAY,TOKYO,TOOLS,TOP,TORAY,TOSHIBA,TOTAL,TOURS,TOWN,TOYOTA,TOYS,TR,TRADE,TRADING,TRAINING,TRAVEL,TRAVELCHANNEL,TRAVELERS,TRAVELERSINSURANCE,TRUST,TRV,TT,TUBE,TUI,TUNES,TUSHU,TV,TVS,TW,TZ,UA,UBANK,UBS,UG,UK,UNICOM,UNIVERSITY,UNO,UOL,UPS,US,UY,UZ,VA,VACATIONS,VANA,VANGUARD,VC,VE,VEGAS,VENTURES,VERISIGN,VERSICHERUNG,VET,VG,VI,VIAJES,VIDEO,VIG,VIKING,VILLAS,VIN,VIP,VIRGIN,VISA,VISION,VIVA,VIVO,VLAANDEREN,VN,VODKA,VOLKSWAGEN,VOLVO,VOTE,VOTING,VOTO,VOYAGE,VU,VUELOS,WALES,WALMART,WALTER,WANG,WANGGOU,WATCH,WATCHES,WEATHER,WEATHERCHANNEL,WEBCAM,WEBER,WEBSITE,WED,WEDDING,WEIBO,WEIR,WF,WHOSWHO,WIEN,WIKI,WILLIAMHILL,WIN,WINDOWS,WINE,WINNERS,WME,WOLTERSKLUWER,WOODSIDE,WORK,WORKS,WORLD,WOW,WS,WTC,WTF,XBOX,XEROX,XFINITY,XIHUAN,XIN,XXX,XYZ,YACHTS,YAHOO,YAMAXUN,YANDEX,YE,YODOBASHI,YOGA,YOKOHAMA,YOU,YOUTUBE,YT,YUN,ZA,ZAPPOS,ZARA,ZERO,ZIP,ZM,ZONE,ZUERICH,ZW,";

        CustomImageButton bodypart1, bodypart2, bodypart3, bodypartBalanced;
        public MainOnboardingPage()
        {
            InitializeComponent();

            lstChats.ItemsSource = BotList;
            NavigationPage.SetHasBackButton(this, false);
            Title = AppResources.DrMuslce;
            _firebase = DependencyService.Get<IFirebase>();
            this.ToolbarItems.Clear();
            var generalToolbarItem = new ToolbarItem("Buy", "menu.png", SlideGeneralBotAction, ToolbarItemOrder.Primary, 0);
            this.ToolbarItems.Add(generalToolbarItem);
            LocalDBManager.Instance.SetDBSetting("BackgroundImage", "DrMuscleLogo.png");
            MessagingCenter.Send(this, "BackgroundImageUpdated");
            if (Device.RuntimePlatform == Device.Android)
            lstChats.VerticalOptions = LayoutOptions.EndAndExpand;
            var tapLinkTermsOfUseGestureRecognizer = new TapGestureRecognizer();
            tapLinkTermsOfUseGestureRecognizer.Tapped += (s, e) =>
            {
                //Device.OpenUri(new Uri("http://drmuscleapp.com/news/terms/"));
                Browser.OpenAsync("http://drmuscleapp.com/news/terms/", BrowserLaunchMode.SystemPreferred);
            };
            TermsOfUse.GestureRecognizers.Add(tapLinkTermsOfUseGestureRecognizer);

            var tapLinkPrivacyPolicyGestureRecognizer = new TapGestureRecognizer();
            tapLinkPrivacyPolicyGestureRecognizer.Tapped += (s, e) =>
            {
                //Device.OpenUri(new Uri("http://drmuscleapp.com/news/privacy/"));
                Browser.OpenAsync("http://drmuscleapp.com/news/privacy/", BrowserLaunchMode.SystemPreferred);
            };
            PrivacyPolicy.GestureRecognizers.Add(tapLinkPrivacyPolicyGestureRecognizer);
            var tapLinkTermsOfUseGestureRecognizerBeta = new TapGestureRecognizer();
            tapLinkTermsOfUseGestureRecognizerBeta.Tapped += (s, e) =>
            {
                //Device.OpenUri(new Uri("http://drmuscleapp.com/news/terms/"));
                Browser.OpenAsync("http://drmuscleapp.com/news/terms/", BrowserLaunchMode.SystemPreferred);
            };
            TermsOfUseBeta.GestureRecognizers.Add(tapLinkTermsOfUseGestureRecognizer);

            var tapLinkPrivacyPolicyGestureRecognizerBeta = new TapGestureRecognizer();
            tapLinkPrivacyPolicyGestureRecognizerBeta.Tapped += (s, e) =>
            {
                //Device.OpenUri(new Uri("http://drmuscleapp.com/news/privacy/"));
                Browser.OpenAsync("http://drmuscleapp.com/news/privacy/", BrowserLaunchMode.SystemPreferred);
            };
            PrivacyPolicyBeta.GestureRecognizers.Add(tapLinkPrivacyPolicyGestureRecognizer);
            MessagingCenter.Subscribe<Message.BodyweightMessage>(this, "BodyweightMessage", (obj) =>
            {
                IsBodyweightPopup = false;
                BodyWeightMassUnitMessage(obj.BodyWeight);
            });
            MessagingCenter.Subscribe<Message.GoalWeightMessage>(this, "GoalWeightMessage", (obj) =>
            {
                IsBodyweightPopup = false;
                GoalWeightMassUnitMessage(obj.WeightGoal);
            });
            
            MessagingCenter.Subscribe<Message.GeneralMessage>(this, "GeneralMessage", (obj) =>
            {
                IsBodyweightPopup = false;
                HandleGeneralMessage(obj);
            });

            
            _googleClientManager = CrossGoogleClient.Current;
            _manager = DependencyService.Get<IFacebookManager>();
            LoginWithFBButton.Clicked += LoginWithFBButton_Clicked;
            LoginWithGoogleButton.Clicked += LoginWithGoogleAsync;
            LoginWithEmailButton.Clicked += ConnectWithEmail;

            LoginButton.HeightRequest = Device.RuntimePlatform.Equals(Device.Android) ? 120 : 170;
            
            BtnAppleSignIn.IsVisible = false;
            // BtnAppleSignIn2.IsVisible = false;

            appleSignInService = DependencyService.Get<IAppleSignInService>();
            if (appleSignInService != null)
            {
                if (appleSignInService.IsAvailable)
                {
                    LoginButton.HeightRequest = 220;
                    BtnAppleSignIn.IsVisible = true;
                    // BtnAppleSignIn2.IsVisible = true;
                    BtnAppleSignIn.Clicked += LoginWithAppleAsync;
                }
            }

            Timer.Instance.OnTimerChange -= OnTimerChange;
            Timer.Instance.OnTimerDone -= OnTimerDone;
            Timer.Instance.OnTimerStop -= OnTimerStop;
            LoadAB();


        }

        private async void LoadAB()
        {
            try
            {

                string val = LocalDBManager.Instance.GetDBSetting("BetaVersion")?.Value;
                if (!string.IsNullOrEmpty(val))
                {
                    if (val.Equals("Beta") || val.Equals("Beta1"))
                    {

                        isBetaExperience = true;
                    }
                    else
                    {
                        isBetaExperience = false;
                    }
                    if (LocalDBManager.Instance.GetDBSetting("Environment") != null && LocalDBManager.Instance.GetDBSetting("Environment").Value != "Production")
                    { }
                    else
                    {
                        if (isBetaExperience == true)
                            _firebase.LogEvent("beta_experience", null);
                        else
                            _firebase.LogEvent("normal_experience", null);
                    }
                        return;
                }
                return;
                IRemoteConfigurationService remoteConfigurationService = DependencyService.Get<IRemoteConfigurationService>();
                await remoteConfigurationService.FetchAndActivateAsync();
                var configuration = await remoteConfigurationService.GetAsync<FeatureConfiguration>(Device.RuntimePlatform.Equals(Device.Android) ? "new_preview_feature" : "Features");

                if (LocalDBManager.Instance.GetDBSetting("Environment") != null && LocalDBManager.Instance.GetDBSetting("Environment").Value != "Production")
                { }
                else
                {
                    
                    if (configuration != null)
                    {
                        if (configuration.ShowPreview)
                        {
                            isBetaExperience = true;
                            IsRealBetaExperience = true;
                            _firebase.LogEvent("beta_experience", null);
                            LocalDBManager.Instance.SetDBSetting("BetaLoaded", "true");
                        }
                        else
                        {
                            isBetaExperience = false;
                            IsRealBetaExperience = false;
                            _firebase.LogEvent("normal_experience", null);
                            LocalDBManager.Instance.SetDBSetting("BetaLoaded", "false");
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            
        }


        private async void LoadAB1()
        {
            try
            {

                IRemoteConfigurationService remoteConfigurationService = DependencyService.Get<IRemoteConfigurationService>();
                await remoteConfigurationService.FetchAndActivateAsync();
                var configuration = await remoteConfigurationService.GetAsync<FeatureConfiguration>(Device.RuntimePlatform.Equals(Device.Android) ? "new_preview_feature" : "Features");

                if (LocalDBManager.Instance.GetDBSetting("Environment") != null && LocalDBManager.Instance.GetDBSetting("Environment").Value != "Production")
                { }
                else
                {
                    if (LocalDBManager.Instance.GetDBSetting("BetaVersion")?.Value == "Beta1")
                    {
                        isBetaExperience = true;
                        _firebase.LogEvent("beta_experience", null);
                    }
                    else
                    {
                        isBetaExperience = false;
                        _firebase.LogEvent("normal_experience", null);
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }
        private async void LoadIntro()
        {
            if (LocalDBManager.Instance.GetDBSetting("Environment") != null && LocalDBManager.Instance.GetDBSetting("Environment").Value != "Production")
            { }
            else
            {
                //var deviceId = DependencyService.Get<IVersionInfoService>().GetDeviceUniqueId();
                //if (string.IsNullOrEmpty(deviceId))
                //{
                //    isBetaExperience = true;
                //    _firebase.LogEvent("no_idfound_experience", deviceId);
                //}
                //else
                //{
                //    try
                //    {
                //        if (int.Parse($"{deviceId.Last()}") % 2 == 0)
                //        {
                //            isBetaExperience = true;
                //            _firebase.LogEvent("beta_experience", deviceId);
                //        }
                //        else
                //        {
                //            isBetaExperience = false;
                //            _firebase.LogEvent("normal_experience", deviceId);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        if (!"ACEGIKMOQSUWY".Contains($"{ deviceId.Last()}"))
                //        {
                //            isBetaExperience = true;
                //            _firebase.LogEvent("beta_experience", deviceId);
                //        }
                //        else
                //        {
                //            isBetaExperience = false;
                //            _firebase.LogEvent("normal_experience", deviceId);
                //        }
                //    }
                //}
            }
            //string val = LocalDBManager.Instance.GetDBSetting("BetaVersion")?.Value;
            //if (!string.IsNullOrEmpty(val))
            //{
            //    if (val == "Beta")
            //    {
            //        App.IsIntroBack = false;
            //        DrMusclePage page = new IntroPage1();
            //        var navigation = new NavigationPage(page);
            //        navigation.BackgroundImageSource = "nav.png";
            //        navigation.BarTextColor = Color.White;
            //        page.OnBeforeShow();
            //        await Navigation.PushModalAsync(navigation,false);
            //    }
            //    else
            //        Features_Clicked();
            //}
            //else
            //{
            //    if (isBetaExperience)
            //    {
            //        App.IsIntroBack = false;
            //        DrMusclePage page = new IntroPage1();
            //        var navigation = new NavigationPage(page);
            //        navigation.BackgroundImageSource = "nav.png";
            //        navigation.BarTextColor = Color.White;
            //        page.OnBeforeShow();
            //        await Navigation.PushModalAsync(navigation,false);
            //    }
            //    else
            //        Features_Clicked();
            //}

            App.IsIntroBack = false;
            DrMusclePage page = new IntroPage1();
            var navigation = new NavigationPage(page);
            navigation.BackgroundImageSource = "nav.png";
            navigation.BarTextColor = Color.White;
            page.OnBeforeShow();
            await Navigation.PushModalAsync(navigation, false);


        }


        private async void BodyWeightMassUnitMessage(string bodyWeight)
        {
            try
            {

                LocalDBManager.Instance.SetDBSetting("BodyWeight", new MultiUnityWeight(Convert.ToDecimal(bodyWeight, CultureInfo.InvariantCulture), LocalDBManager.Instance.GetDBSetting("massunit").Value).Kg.ToString().ReplaceWithDot());
                await AddAnswer(bodyWeight);

lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);

                //SetupMainGoal();
                await AddQuestion("What is your target weight?");
                await PopupNavigation.Instance.PushAsync(new WeightGoalPopup());

            }
            catch (Exception ex)
            {

            }
        }

        private async void GoalWeightMassUnitMessage(string WeightGoal)
        {
            try
            {

                LocalDBManager.Instance.SetDBSetting("WeightGoal", new MultiUnityWeight(Convert.ToDecimal(WeightGoal, CultureInfo.InvariantCulture), LocalDBManager.Instance.GetDBSetting("massunit").Value).Kg.ToString().ReplaceWithDot());
                await AddAnswer(WeightGoal);

                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                

                if (new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight")?.Value, CultureInfo.InvariantCulture), "kg").Lb > 150)
                {
                    await AddQuestion("How tall are you?");
                    await PopupNavigation.Instance.PushAsync(new UserHeightView());
                }
                else
                    SetupMainGoal();
                

            }
            catch (Exception ex)
            {

            }
        }

        private async void HandleGeneralMessage(GeneralMessage general)
        {
                if (!App.IsNUX)
                    return;
                //App.IsAskingForMeal = true;
                if (general.PopupEnum == Enums.GeneralPopupEnum.UserHeight)
                {
                    //AllergyText = general.GeneralText;
                    await AddAnswer(general.GeneralText);
                    LocalDBManager.Instance.SetDBSetting("Height", Config.UserHeight.ToString());
                    await Task.Delay(200);
                    SetupMainGoal();
                }
       

        }
        private async void BodyweightPicker_Unfocused(object sender, FocusEventArgs e)
        {
            try
            {
                int age = Convert.ToInt32(BodyweightPicker.SelectedItem, CultureInfo.InvariantCulture);
                LocalDBManager.Instance.SetDBSetting("BodyWeight", Convert.ToString(age));
                await AddAnswer(Convert.ToString(age));

                if (LocalDBManager.Instance.GetDBSetting("ExLevel").Value == "Exp")
                {
                    LocalDBManager.Instance.SetDBSetting("workout_place", "gym");
                    LocalDBManager.Instance.SetDBSetting("experience", "more3years");

                    if (LocalDBManager.Instance.GetDBSetting("experience").Value == "beginner")
                        AddCardio();
                    else
                        workoutPlace();
                    return;
                }
                LocalDBManager.Instance.SetDBSetting("experience", "less1year");
                // await AddQuestion("Are you training at home with no equipment?");
                Device.BeginInvokeOnMainThread(() =>
                {
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                });
                BeginnerSetup();

            }
            catch (Exception ex)
            {

            }
        }

        private async void AgePicker_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void LevelPicker_Unfocused(object sender, FocusEventArgs e)
        {
            try
            {
                if (LevelPicker.SelectedIndex == -1)
                    LevelPicker.SelectedIndex = 0;
                int level = LevelPicker.SelectedIndex + 1;
                LocalDBManager.Instance.SetDBSetting("MainLevel", level.ToString());
                LocalDBManager.Instance.SetDBSetting("CustomMainLevel", level.ToString());
                await AddAnswer($"Level {level}");
                //SetupMassUnit();
                if (LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "new to training" || LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "returning from a break")
                {
                    AskSetStyle();
                }
                else
                {
                    AskSetStyle();
                }
            }
            catch (Exception ex)
            {

            }
        }


        private async void AgePicker_Unfocused(object sender, FocusEventArgs e)
        {
            try
            {
                int age = Convert.ToInt32(AgePicker.SelectedItem, CultureInfo.InvariantCulture);
                LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(age));
                //        await AddAnswer(Convert.ToString(age));

                await AddAnswer($"{age}");
                if (age > 50)
                    learnMore.AgeDesc = $"Recovery is slower at {age}. So, I added easy days to your program.";
                else if (age > 30)
                    learnMore.AgeDesc = $"Recovery is a bit slower at {age}. So, I'm updating your program to make sure you train each muscle max 2x a week.";
                else
                    learnMore.AgeDesc = "Recovery is optimal at your age. You can train each muscle as often as 3x a week.";

                if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("PPL"))
                {
                    if (age > 50)
                        learnMore.AgeDesc = $"Recovery is slower at {age}. So, I'm updating your program to make sure you train each muscle max 6x a week.";
                    else if (age > 30)
                        learnMore.AgeDesc = $"Recovery is a bit slower at {age}. So, I'm updating your program to make sure you train each muscle max 6x a week.";
                    else
                        learnMore.AgeDesc = "Recovery is optimal at your age. You can train each muscle as often as 6x a week.";
                }
                var gender = LocalDBManager.Instance.GetDBSetting("gender").Value == "Man" ? "Man" : "Woman";
                var xDays = 0;
                if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("PPL"))
                {
                    xDays = 6;
                }
                    else if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Split")) //|| 
                {
                    if (age < 30)
                        xDays = 4;
                    else if (age >= 30 && age <= 50)
                        xDays = 4;
                    else
                        xDays = 3;
                }
                else 
                {
                    if (age < 30)
                        xDays = 4;
                    else if (age >= 30 && age <= 50)
                        xDays = 3;
                    else
                        xDays = 2;
                }
                await AddQuestion($"{gender} aged {age}? I recommend {xDays} workouts a week.");

              

                RecommendationProgram(xDays);

            }
            catch (Exception ex)
            {

            }
        }

        
        private async void ExerciseVariety()
        {
            await AddQuestion("Exercise variety?");
            var btn1 = new DrMuscleButton()
            {
                Text = "More exercises (can be more fun)",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn1.Clicked += (o, ev) => {
                AddAnswer("More exercises (can be more fun)");
                AskSetStyle();
                LocalDBManager.Instance.SetDBSetting("ExerciseVariety", "More");
            };
            stackOptions.Children.Add(btn1);
            await AddOptions($"Fewer exercises (faster progress)", (o, ev) => {
                AddAnswer($"Fewer exercises (faster progress)");
                LocalDBManager.Instance.SetDBSetting("ExerciseVariety", "Less");
                AskSetStyle();
            });

            
        }

        private async void RecommendationProgram(int xDays)
        {
            var btn1 = new DrMuscleButton()
            {
                Text = "Another schedule",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn1.Clicked += (o, ev) => {
                AddAnswer("Another schedule");
                ShowWorkoutReminder(true);
            };
            stackOptions.Children.Add(btn1);
            await AddOptions($"Recommended {xDays}x/week", (o, ev) => {
                AddAnswer($"Recommended {xDays}x/week");
                ShowWorkoutReminder(false);
                IsRecommendedReminder = true;
            });
        }

        public override void OnBeforeShow()
        {
            base.OnBeforeShow();
            try
            {
                App.IsNUX = true;
                CurrentLog.Instance.IsMovingOnBording = false;
                this.ToolbarItems.Clear();
                var generalToolbarItem = new ToolbarItem("Buy", "menu.png", SlideGeneralBotAction, ToolbarItemOrder.Primary, 0);
                this.ToolbarItems.Add(generalToolbarItem);
                IsMovedToLogin = false;
                List<string> age = new List<string>();
                List<string> bodyweight = new List<string>();
                for (int i = 10; i < 125; i++)
                {
                    age.Add($"{i}");
                }
                List<string> level = new List<string>();
                for (int i = 1; i < 7; i++)
                {
                    level.Add($"Level {i}");
                }

                if (AgePicker != null)
                {
                    AgePicker.Unfocused -= AgePicker_Unfocused;
                    olderAgePicker = AgePicker;
                    AgePicker.SelectedIndexChanged -= AgePicker_SelectedIndexChanged;
                }
                
                AgePicker = new Picker()
                {

                    Title = "Age?"
                };
                AgePicker.VerticalOptions = LayoutOptions.Start;
                AgePicker.HorizontalOptions = LayoutOptions.Center;
                AgePicker.BackgroundColor = Color.Transparent;
                AgePicker.Margin = new Thickness(20);
                AgePicker.HeightRequest = 1;
                AgePicker.ItemsSource = age;
                AgePicker.SelectedItem = "35";
                AgePicker.Unfocused += AgePicker_Unfocused;
                AgePicker.SelectedIndexChanged += AgePicker_SelectedIndexChanged;


                if (LevelPicker != null)
                    LevelPicker.Unfocused -= LevelPicker_Unfocused;

                LevelPicker = new Picker()
                {
                    Title = "Select level"
                };
                LevelPicker.ItemsSource = level;
                LevelPicker.SelectedIndex = 0;
                LevelPicker.Unfocused += LevelPicker_Unfocused;

                

                if (BodyweightPicker != null)
                    BodyweightPicker.Unfocused -= BodyweightPicker_Unfocused;
                BodyweightPicker = new Picker()
                {
                    Title = "what is your body weight?"
                };
                BodyweightPicker.ItemsSource = bodyweight;
                BodyweightPicker.SelectedItem = "160";
                BodyweightPicker.Unfocused += BodyweightPicker_Unfocused;
                StackMain.Children.Insert(0, AgePicker);
                StackMain.Children.Insert(0, BodyweightPicker);
                StackMain.Children.Insert(0, LevelPicker);
                AgePicker.IsVisible = true;
                BodyweightPicker.IsVisible = true;
                LevelPicker.IsVisible = true;
                if (Device.RuntimePlatform == Device.iOS)
                    StackMain.IsVisible = false;
                if (App.IsIntro)
                {
                    App.IsIntro = false;
                    App.IsIntroBack = false;
                    Features_Clicked();
                }
                else if (!App.IsDemoProgress)
                    StartSetup();
                else
                {
                    if (BotList.Count == 1)
                        BotList.Clear();
                    SetUpRestOnboarding();
                }

            }
            catch (Exception ex)
            {

            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {

                DependencyService.Get<IFirebase>().SetScreenName("onboarding_account");
                if (Device.RuntimePlatform.Equals(Device.Android))
                {
                    DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1351);
                    var dt = DateTime.Now.AddMinutes(1);
                    var timeSpan = new TimeSpan(0, dt.Hour, dt.Minute, 0);// DateTime.Now.AddMinutes(2) - DateTime.Now;////
                    DependencyService.Get<IAlarmAndNotificationService>().ScheduleNotification("Dr. Muscle", "Oops! You're 12 seconds away from custom, smart workouts", timeSpan, 1351, NotificationInterval.Week);
                    DependencyService.Get<INotificationRequestService>().RequestNotificationRequest();
                }

                DBSetting dbOnBoardingSeen = LocalDBManager.Instance.GetDBSetting("onboarding_features");
                if ((dbOnBoardingSeen == null || dbOnBoardingSeen.Value == "false") && !isUpdatePopupShown)
                {
                    isUpdatePopupShown = true;
                    var waitHandle2 = new EventWaitHandle(false, EventResetMode.AutoReset);
                    var modalPage2 = new Views.GeneralPopup("TrueState.png", "October update 2!", "✓ Assisted Exercises\n✓ Cleaner workout UI\n✓ Smarter AI chat\n⮕ Soon: New programs (PPL 7)\n", "Continue", null, false, false, "false", "false", "false", "false", "false", "true");


                    modalPage2.Disappearing += (sender2, e2) =>
                    {
                        waitHandle2.Set();
                    };
                    await PopupNavigation.Instance.PushAsync(modalPage2);

                    await Task.Run(() => waitHandle2.WaitOne());
                }
                LocalDBManager.Instance.SetDBSetting("onboarding_features", "true");
            }
            catch (Exception ex)
            {

            }
        }

        
        protected override bool OnBackButtonPressed()
        {

            if (IsBodyweightPopup)
                return true;
            ((App)Application.Current).displayCreateNewAccount = true;
            PagesFactory.PushAsync<WelcomePage>();
            return true;

        }

        private async Task ClearOptions()
        {
            //if (Device.RuntimePlatform.Equals(Device.iOS))
            //{
            //    stackOptions.Children.Clear();
            //    return;
            //}
            var count = stackOptions.Children.Count;
            for (var i = 0; i < count; i++)
            {
                stackOptions.Children.RemoveAt(0);
            }
            BottomViewHeight.Height = 65;
        }

        void Handle_ItemAppearing(object sender, Xamarin.Forms.ItemVisibilityEventArgs e)
        {

        }
        bool Isrestarted = false;
        async Task StartSetup()
        {
            try
            {
                StackSignupMenu.IsVisible = false;
                try
                {

                if (olderAgePicker != null && olderAgePicker.IsFocused) 
                    Device.BeginInvokeOnMainThread(() => { olderAgePicker?.Unfocus(); });

                }
                catch (Exception ex)
                {

                }
                BotList.Clear();
                await ClearOptions();

//#if DEBUG
//                SetTimerOption();
//                return;
//#endif

                IsPlates = false;
                isDumbbells = false;
                IsPully = false;
                IsEquipment = false;
                IsChinupBar = false;
                SetStyle = false;
                IncrementUnit = null;
                IsPyramid = false;
                bodypartName = "";
                var welcomeNote = "";
                var time = DateTime.Now.Hour;
                if (time < 12)
                    welcomeNote = AppResources.GoodMorning;
                else if (time < 18)
                    welcomeNote = AppResources.GoodAfternoon;
                else
                    welcomeNote = AppResources.GoodEvening;
                
                if (BotList.Count == 0)
                {
                    BotList.Add(new BotModel()
                    {
                        Question = "Welcome to Dr. Muscle! I'm Dr. Carl Juneau and I'll help you get in shape fast with smart, custom workouts.",
                        Type = BotType.Ques
                    });
                }
                else
                    return;
                ////

                
                await Task.Delay(2500);
                if (BotList.Count == 1)
                {
                    BotList.Add(new BotModel()
                    {
                        Question = "Why trust me? I've been a coach for 17 years and a trainer for the Canadian Forces.",
                        Type = BotType.Ques
                    });
                }
                else
                    return;
                if (BotList.Count == 1 || BotList.Count>2)
                    return;

                await Task.Delay(2500);
                if (BotList.Count == 2)
                {
                    BotList.Add(new BotModel()
                    {
                        Question = $"",
                        Answer = "",
                        Type = BotType.Photo
                    });
                }
                else
                    return;

                await ClearOptions();
                
                SetNotifications();
                await Task.Delay(2500);

                if (IsMovedToLogin)
                    return;
                if (BotList.Count == 3)
                {
                    BotList.Add(new BotModel()
                    {
                        Question = "With this app, you get me as your coach—without the expensive fees. Are you...",
                        Type = BotType.Ques
                    });
                }
                else
                    return;
                if (PopupNavigation.Instance.PopupStack.Count == 0)
                { 
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                }
                else
                {
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                    await Task.Delay(700);
                    lstChats.ScrollTo(BotList.First(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.First(), ScrollToPosition.Start, false);
                }

                await Task.Delay(1000);
                if (BotList.Count < 3)
                    return;
                await ClearOptions();
                var btn = await AddOptions("New to lifting weights", async (ss, ee) =>
                {




                    SetNotifications();
                    ShouldAnimate = false;
                    _firebase.LogEvent("start_onboarding", "new_to_training");
                    _firebase.LogEvent("new_to_training", "");
                    LocalDBManager.Instance.SetDBSetting("CustomExperience", "new to training");
                    await AddAnswer("New to lifting weights");
                    //if (Device.RuntimePlatform.Equals(Device.Android))
                    //    await Task.Delay(300);
                    //await ClearOptions();
                    
                    

                    //await AddQuestion("Congrats! New users like you get 34% stronger in 30 days. This app makes it easy:");

                    //BotList.Add(new BotModel() { Type = BotType.Empty });
                    //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);

                    LocalDBManager.Instance.SetDBSetting("ExLevel", "New");
                    LocalDBManager.Instance.SetDBSetting("NewLevel", "All");
                    if (LocalDBManager.Instance.GetDBSetting("experience")?.Value != "")
                        LocalDBManager.Instance.SetDBSetting("experience", "");
                    //await Task.Delay(2000);
                    //BotList.Remove(BotList.Last());
                    if (BotList.Count==5)
                    {
                        if (LocalDBManager.Instance.GetDBSetting("FirstStepCompleted")?.Value != "true")
                        {
                            LoadIntro();
                        }
                        else
                        {
                            SetUpRestOnboarding();
                        }
                        
                        //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                        //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);

                        
                        //await Task.Delay(2000);
                        //await AddOptions(AppResources.GotIt, GotItAfterExperienceLevel);
                        
                    }
                });
                Device.BeginInvokeOnMainThread(async () =>
                {
                    Grid grid = (Xamarin.Forms.Grid)btn.Parent;
                    ShouldAnimate = true;
                    animate(grid);

                });
                var btn1 = await AddOptions("Returning after a break", async (ss, ee) =>
                {
                    SetNotifications();
                    ShouldAnimate = false;
                    LocalDBManager.Instance.SetDBSetting("CustomExperience", "returning from a break");
                    _firebase.LogEvent("start_onboarding", "returning_after_a_break");
                    _firebase.LogEvent("returning_after_a_break", "");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    await AddAnswer("Returning after a break");
                    //if (Device.RuntimePlatform.Equals(Device.Android))
                    //    await Task.Delay(300);
                    if (LocalDBManager.Instance.GetDBSetting("experience")?.Value != "")
                        LocalDBManager.Instance.SetDBSetting("experience", "");
                    //ClearOptions();
                    //await AddQuestion("This app can help you get back in shape and stay on track:");
                    //BotList.Add(new BotModel() { Type = BotType.Empty });
                    //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                    LocalDBManager.Instance.SetDBSetting("ExLevel", "Return");

                    // await AddOptions(AppResources.GotIt, GotItAfterExperienceLevel);
                    
                    //await Task.Delay(2000);
                    //BotList.Remove(BotList.Last());
                    if (BotList.Count == 5)
                    {
                        if (LocalDBManager.Instance.GetDBSetting("FirstStepCompleted")?.Value != "true")
                        {



                            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                            //await Task.Delay(2000);

                            LoadIntro();
                        }
                        else
                            SetUpRestOnboarding();
                    }
                });
                Device.BeginInvokeOnMainThread(async () =>
                {
                    Grid grid = (Xamarin.Forms.Grid)btn1.Parent;
                    ShouldAnimate = true;
                    animate(grid);

                });

                var btn2 = await AddOptions("Already lifting weights", async (ss, ee) =>
                {
                    SetNotifications();
                    ShouldAnimate = false;
                    LocalDBManager.Instance.SetDBSetting("CustomExperience", "an active, experienced lifter");
                    _firebase.LogEvent("start_onboarding", "Active_experienced_lifter");
                    _firebase.LogEvent("Active_experienced_lifter", "");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    await AddAnswer("Already lifting weights");
                    //if (Device.RuntimePlatform.Equals(Device.Android))
                    //    await Task.Delay(300);

                    //ClearOptions();
                    //await AddQuestion("This app can help you break through plateaus and reach your genetic potential:");
                    //BotList.Add(new BotModel() { Type = BotType.Empty });
                    //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                    

                    LocalDBManager.Instance.SetDBSetting("ExLevel", "Exp");
                    //await AddOptions(AppResources.GotIt, GotItAfterExperienceLevel);
                    //await Task.Delay(2000);
                    //BotList.Remove(BotList.Last());
                    if (BotList.Count ==5)
                    {
                        if (LocalDBManager.Instance.GetDBSetting("FirstStepCompleted")?.Value != "true")
                        {
                            //BotList.Add(new BotModel()
                            //{
                            //    Question = "Where should we send your custom program?",
                            //    Type = BotType.Ques
                            //});
                            //BotList.Add(new BotModel()
                            //{
                            //    Part1 = "User reviews",
                            //    Part2 = "\"For strength training this app out performs...\"",
                            //    Part3 = "\"The many methods/app I have tried in my 30+ years of body/strength training. Takes the brain work out of weights, reps, and sets.\"",
                            //    Answer = "-TijFamily916",
                            //    Type = BotType.ReviewFullCell
                            //});
                            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                            //await Task.Delay(2000);

                            LoadIntro();
                        }
                        else
                            SetUpRestOnboarding();
                    }
                });
                Device.BeginInvokeOnMainThread(async () =>
                {
                    Grid grid = (Xamarin.Forms.Grid)btn2.Parent;
                    ShouldAnimate = true;
                    animate(grid);

                });
                //SetupMassUnit();
            }
            catch (Exception ex)
            {

            }
        }


        
        private async void Features_Clicked()
        {
            //await AddAnswer(((DrMuscleButton)sender).Text);
            ClearOptions();
            //var btn = await AddOptions("Start demo", GotItAfterExperienceLevel);
            //Device.BeginInvokeOnMainThread(async () =>
            //{
            //    Grid grid = (Xamarin.Forms.Grid)btn.Parent;
            //    ShouldAnimate = true;
            //    animate(grid);

            //});
            //stackOptions.Children.Add(TermsConditionStack);
            //TermsConditionStack.IsVisible = true;
            
            //await Task.Delay(2000);
            
            string cweight = "", todayweight = "", liftedweight = "";
            LocalDBManager.Instance.SetDBSetting("massunit", "lb");
            
            if (LocalDBManager.Instance.GetDBSetting("FirstStepCompleted")?.Value != "true")
                SetMenu();
            else
                SetUpRestOnboarding();
            if (LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg")
            {
                cweight = "95 kg";
                todayweight = "120 kg";
                liftedweight = "242 kg";
            }
            else
            {
                cweight = "210 lbs";
                todayweight = "265 lbs";
                liftedweight = "535 lbs";
            }
            if (LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "new to training")
            {
                if (BotList.Count < 4)
                    return;
                
                BotList.Add(new BotModel()
                {
                    Part1 = "User reviews",
                    Part2 = "\"AI is great and makes it very easy\"",
                    Part3 = "\"Easy for me to know how many reps to do and how much weight to lift. No more guessing. This really is something different.\"",
                    Answer = "MKJ&MKJ",
                    Source = "",
                    Type = BotType.ReviewFullCell
                });
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                //if (IsRealBetaExperience == false || IsRealBetaExperience == null)
                //{
                    
                //    await Task.Delay(2500);
                //    if (BotList.Count < 4)
                //        return;
                //    BotList.Add(new BotModel()
                //    {
                //        Part1 = "Expert reviews",
                //        Part2 = "\"I haven’t skipped a workout since I started using it\"",
                //        Part3 = "\"This app creates new workouts automatically. They're challenging enough for me to keep building muscle without overtraining. It also keeps me motivated by showing me my progress.\"",
                //        Answer = "Artin Entezarjou, MD, PhD(c)",
                //        Source = "artin.png",
                //        Type = BotType.ReviewFullCell
                //    });
                //    if (Device.RuntimePlatform.Equals(Device.Android))
                //        await Task.Delay(300);
                //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);

                //    await Task.Delay(3200);
                //}
                //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);

                //await Task.Delay(1500);
            }
            else if (LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "returning from a break")
            {
                if (BotList.Count < 4)
                    return;
                BotList.Add(new BotModel()
                {
                    Part1 = "User reviews",
                    Part2 = "\"Gained 10 lbs\"",
                    Part3 = "\"Have been in and out of the gym for a few years with modest gains, however, this app helped me gain 10 lbs and become significantly more defined. Very easy to use.\"",
                    Answer = "Potero2122",
                    Source = "",
                    Type = BotType.ReviewFullCell
                });
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);

                //if (IsRealBetaExperience == false || IsRealBetaExperience == null)
                //{
                //    await Task.Delay(2500);
                //    if (BotList.Count < 4)
                //        return;
                //    BotList.Add(new BotModel()
                //    {
                //        Part1 = "Expert reviews",
                //        Part2 = "\"Skeptical at first but I gave it a shot\"",
                //        Part3 = "\"Most apps track what you do. This one tracks but the main focus is figuring out based on your actual progress, what's next and how to improve. I hit new PRs.\"",
                //        Answer = "Marc David, author of The NoBull Approach To Building Muscle",
                //        Source = "",
                //        Type = BotType.ReviewFullCell
                //    });
                //    if (Device.RuntimePlatform.Equals(Device.Android))
                //        await Task.Delay(300);
                //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                //    await Task.Delay(2500);
                //}
            }
            else
            {
                if (BotList.Count < 4)
                    return;
                BotList.Add(new BotModel()
                {
                    Part1 = "Expert reviews",
                    Part2 = $"\"I was never that heavy ({cweight})\"",
                    Part3 = $"\"My strength on hip trusts exploded from something like {todayweight} to {liftedweight}. The app has scientific algorithms... simple stupid and effective wether its raining or sunshine, just follow the app and don't overthink to much.\"",
                    Answer = "Jonas Notter, World Natural Bodybuilding Champion",
                    Source = "jonus.png",
                    Type = BotType.ReviewFullCell
                });
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);

                //if (IsRealBetaExperience == false || IsRealBetaExperience == null)
                //{
                    
                //    await Task.Delay(3500);

                //    if (BotList.Count < 4)
                //        return;
                //    BotList.Add(new BotModel()
                //    {
                //        Part1 = "Expert reviews",
                //        Part2 = "\"I haven’t skipped a workout since I started using it\"",
                //        Part3 = "\"This app creates new workouts automatically. They're challenging enough for me to keep building muscle without overtraining. It also keeps me motivated by showing me my progress.\"",
                //        Answer = "Artin Entezarjou, MD, PhD(c)",
                //        Source = "artin.png",
                //        Type = BotType.ReviewFullCell
                //    });
                //    if (Device.RuntimePlatform.Equals(Device.Android))
                //        await Task.Delay(300);
                //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                //    await Task.Delay(3500);

                //}
            }
            //await Task.Delay(1200);
            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            //if (LocalDBManager.Instance.GetDBSetting("FirstStepCompleted")?.Value != "true")
            //    BotList.Add(new BotModel()
            //{
            //    Question = "Where should we send your custom program?",
            //    Type = BotType.Ques
            //});
            //else
            //    SetUpRestOnboarding();

            if (Device.RuntimePlatform.Equals(Device.Android))
            await Task.Delay(300);
            if (BotList.Count < 4)
                return;
            var botModel = new BotModel()
            {
                Question = "Sign in to customize your program:\n+ Save plan & get by email\n+ Never lose your progress\n+ Train on any device",
                Type = BotType.Ques
            };
            BotList.Add(botModel);
            //if (IsRealBetaExperience == false || IsRealBetaExperience == null)
            //{ 
            //    if (Device.RuntimePlatform.Equals(Device.Android))
            //    await Task.Delay(300);

            //    lstChats.ScrollTo(botModel, ScrollToPosition.MakeVisible, false);
            //    await Task.Delay(1000);
            //    TooltipEffect.SetHasShowTooltip(LoginButtonStack, true);
            //}
        }
        async Task AskforProgramsWithABExperience()
        {
            string val = LocalDBManager.Instance.GetDBSetting("BetaVersion")?.Value;
            if (!string.IsNullOrEmpty(val))
            {
                //if (val == "Beta")
                //{
                    RecommendedProgram_clicked(new DrMuscleButton(), EventArgs.Empty);
                //}
                //else
                //{
                //    if (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "less1year")
                //        await AddQuestion("Less than 1 year? I recommend a full-body program.");
                //    else if (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "1-3years")
                //        await AddQuestion("1-3 years? I recommend an upper/lower-body split program.");
                //    else
                //        await AddQuestion("3+ years? I recommend an upper/lower-body split program.");
                    
                //    AskForProgram();
                //}
            }
            else
            {
                //if (isBetaExperience == true)
                //{

                    RecommendedProgram_clicked(new DrMuscleButton(), EventArgs.Empty);

                //}
                //else
                //{                    
                //    if (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "less1year")
                //        await AddQuestion("Less than 1 year? I recommend a full-body program.");
                //    else if (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "1-3years")
                //        await AddQuestion("1-3 years? I recommend an upper/lower-body split program.");
                //    else
                //        await AddQuestion("3+ years? I recommend an upper/lower-body split program.");
                //    AskForProgram();
                //}
            }


        }
        async Task AskforProgramsWithAB()
        {
            string val = LocalDBManager.Instance.GetDBSetting("BetaVersion")?.Value;
            if (!string.IsNullOrEmpty(val))
            {
                //if (val == "Beta")
                //{

                    RecommendedProgram_clicked(new DrMuscleButton(), EventArgs.Empty);
                //}
                //else
                //{
                //    var msgText = "";
                //    if (LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "new to training")
                //        msgText = "Since you are new to lifting weights, I recommend a full-body program.";
                //    else
                //        msgText = "Since you are returning after a break, I recommend a full-body program.";
                //    BotList.Add(new BotModel()
                //    {
                //        Question = msgText,
                //        Type = BotType.Ques
                //    });
                //    AskForProgram();
                //}
            }
            else
            {
                //if (isBetaExperience == true)
                //{

                    RecommendedProgram_clicked(new DrMuscleButton(), EventArgs.Empty);

                //}
                //else
                //{
                //    var msgText = "";
                //    if (LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "new to training")
                //        msgText = "Since you are new to lifting weights, I recommend a full-body program.";
                //    else
                //        msgText = "Since you are returning after a break, I recommend a full-body program.";
                //    BotList.Add(new BotModel()
                //    {
                //        Question = msgText,
                //        Type = BotType.Ques
                //    });
                //    AskForProgram();
                //}
            }

            
        }
        private async void SetUpRestOnboarding()
        {
            ClearOptions();
            
            //BotList.Add(new BotModel()
            //{
            //    Question = "Congrats—you smashed the demo!",
            //    Type = BotType.Ques
            //});
            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            //await Task.Delay(1000);

            if (LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "new to training" || LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "returning from a break")
            {
                //Since you are [new to training / returning after a break], I recommend you train your full body 2-4 times a week for best results.
                
                LocalDBManager.Instance.SetDBSetting("MainProgram", "Full body");
                AskforProgramsWithAB();
            }
            else
            {
                NoAdvancedClicked(new DrMuscleButton(), EventArgs.Empty);
            }
            //    BotList.Add(new BotModel()
            //{
            //    Question = "This app is new. Features like smart watch integration and calendar view are not yet available. But if you’re an early adopter who wants to get in shape fast, you'll love your new custom workouts. Give us a shot: we release new features every month and we'll treat your feedback like gold.",
            //    Type = BotType.Ques
            //});
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            await Task.Delay(2500);

            //GotItAfterImage(new DrMuscleButton(), EventArgs.Empty);

        }

        

        async void GotItAfterExperienceLevel(object sender, EventArgs e)
        {

            await AddAnswer("Start demo");
            ShouldAnimate = false;
            ClearOptions();
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);

            //SetupMassUnit();
            //Move to demo
            

        }

        private void SetNotifications()
        {
            if (Device.RuntimePlatform.Equals(Device.iOS))
            {
                CancelNotification();
                var dt = DateTime.Now.AddMinutes(5);
                var timeSpan = new TimeSpan(DateTime.Now.AddMinutes(5).Day - DateTime.Now.Day, dt.Hour, dt.Minute, 0);// DateTime.Now.AddMinutes(2) - DateTime.Now;////
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleNotification("Dr. Muscle", "Oops! You're 12 seconds away from custom, smart workouts", timeSpan, 1351, NotificationInterval.Week);
            }
            else
                DependencyService.Get<INotificationRequestService>().RequestNotificationRequest();

        }

        private void SetTrialUserNotifications()
        {
            try
            {

                CancelNotification();
                var fName = LocalDBManager.Instance.GetDBSetting("firstname").Value;
                var dt = DateTime.Now.AddDays(2);
                var timeSpan = new TimeSpan(2, dt.Hour, dt.Minute, 0);// new TimeSpan(DateTime.Now.AddMinutes(10).Day - DateTime.Now.Day, dt.Hour, dt.Minute, 0);////
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleOnceNotification("Dr. Muscle", $"{fName}, you can do this!", timeSpan, 1451);

                var dt1 = DateTime.Now.AddDays(4);
                var timeSpan1 = new TimeSpan(4, dt1.Hour, dt1.Minute, 0);// new TimeSpan(DateTime.Now.AddMinutes(15).Day - DateTime.Now.Day, dt1.Hour, dt1.Minute, 0);////
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleOnceNotification("Dr. Muscle", Device.RuntimePlatform.Equals(Device.Android) ? $"New users like you improve 34% in 30 days" : $"New users like you improve 34%% in 30 days", timeSpan1, 1551);

                var dt2 = DateTime.Now.AddDays(10);
                var timeSpan2 = new TimeSpan(10, dt2.Hour, dt2.Minute, 0);// new TimeSpan(DateTime.Now.AddMinutes(20).Day - DateTime.Now.Day, dt2.Hour, dt2.Minute, 0);////
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleOnceNotification("Dr. Muscle", $"You're 12 seconds away from custom, smart workouts", timeSpan2, 1651);

            }
            catch (Exception ex)
            {

            }
        }

        private void CancelNotification()
        {
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1051);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1151);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1251);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1351);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1451);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1551);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1651);

        }
        async void animate(Grid grid)
        {
            try
            {
                if (Battery.EnergySaverStatus == EnergySaverStatus.On && Device.RuntimePlatform.Equals(Device.Android))
                    return;
                var a = new Animation();
                a.Add(0, 0.5, new Animation((v) =>
                {
                    grid.Scale = v;
                }, 1.0, 0.8, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION A"); }));
                a.Add(0.5, 1, new Animation((v) =>
                {
                    grid.Scale = v;
                }, 0.8, 1.0, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION B"); }));
                a.Commit(grid, "animation", 16, 2000, null, (d, f) =>
                {
                    grid.Scale = 1.0;
                    if (ShouldAnimate)
                        animate(grid);
                });

            }
            catch (Exception ex)
            {
                ShouldAnimate = false;
            }
        }

        async void GotItAfterImage(object sender, EventArgs e)
        {
            //await AddAnswer("Hi Carl");
            await AddQuestion("Man or woman?");
            await Task.Delay(300);
            SetupGender();

        }
        void SetDefaultButtonStyle(Button btn)
        {
            btn.BackgroundColor = Color.Transparent;
            btn.BorderWidth = 0;
            btn.CornerRadius = 0;
            btn.Margin = new Thickness(25, 2, 25, 2);
            btn.FontAttributes = FontAttributes.Bold;
            btn.BorderColor = Color.Transparent;
            btn.TextColor = Color.White;
            btn.HeightRequest = 55;

        }

        void SetEmphasisButtonStyle(Button btn)
        {
            btn.TextColor = Color.White;
            btn.BackgroundColor = Color.Transparent;
            btn.Margin = new Thickness(25, 2, 25, 2);
            btn.HeightRequest = 55;
            btn.BorderWidth = 6;
            btn.CornerRadius = 0;
            btn.FontAttributes = FontAttributes.Bold;
        }

        async void YesButton_Clicked(object sender, EventArgs e)
        {

            await AddAnswer(AppResources.Yes);

            await AddQuestion(AppResources.DoYouUseLbsOrKgs, false);

            //Yes-No Button
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddOptions(AppResources.Man, ManButton_Clicked);
            await AddOptions(AppResources.Woman, WomanButton_Clicked);


        }

        async void NoButton_Clicked(object sender, EventArgs e)
        {
            //Move back
            ((App)Application.Current).displayCreateNewAccount = true;
            await PagesFactory.PushAsync<WelcomePage>();
        }

        async void ManButton_Clicked(object sender, EventArgs e)
        {

            BotList.Add(new BotModel()
            {
                Answer = AppResources.Man,
                Type = BotType.Ans
            });

            await ClearOptions();


            LocalDBManager.Instance.SetDBSetting("gender", "Man");
            //await AddQuestion("Men often want (tap all that apply):");
            //await Task.Delay(300);

            //await AddCheckbox("More muscle", Man_MoreMuscle_Clicked);
            //await AddCheckbox("Less fat", Man_LessFat_Clicked);
            //await AddCheckbox("Better health", Man_BetterHealth_Clicked);
            //await AddCheckbox("Stronger sex drive", Man_StorngerSexDrive_Clicked);

            //await AddOptions("Continue", ManTakeActionBasedOnInput);
            //SetupMassUnit();
            //SetupMainGoal();
            await AddQuestion("Age?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            GetAge();
            return;

        }

        private async void SetupMainGoal()
        {

            //await Task.Delay(300);

            ManLessFat = false;
            ManBetterHealth = false;
            FemaleMoreEnergy = false;
            FemaleToned = false;
            ManMoreMuscle = false;
            ManStorngerSexDrive = false;

            var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value == "Woman";
            if (IsWoman)
            {

                await AddQuestion("What are your goals?");

                await AddCheckbox("Less fat", Man_LessFat_Clicked);
                await AddCheckbox("Better health", Man_BetterHealth_Clicked);
                await AddCheckbox("More energy", WoMan_MoreEnergy_Clicked);
                await AddCheckbox("Toned muscles", WoMan_FemaleToned_Clicked);

                await AddOptions("Continue", WomanTakeActionBasedOnInput);
            }
            else
            {
                await AddQuestion("What are your goals?");
                await Task.Delay(300);

                await AddCheckbox("More muscle", Man_MoreMuscle_Clicked);
                await AddCheckbox("Less fat", Man_LessFat_Clicked);
                await AddCheckbox("Better health", Man_BetterHealth_Clicked);
                await AddCheckbox("Stronger sex drive", Man_StorngerSexDrive_Clicked);

                await AddOptions("Continue", ManTakeActionBasedOnInput);
            }

        }
        private async void GetMainGoalAction(PromptResult response)
        {
            try
            {

                mainGoal = null;

                if (string.IsNullOrEmpty(response.Text))
                {
                    SetupMainGoal();
                    return;
                }
                else
                {
                    mainGoal = response.Text.ToLower();
                    await AddAnswer(response.Text);
                    LocalDBManager.Instance.SetDBSetting("PopupMainGoal", mainGoal);
                }
                //GotItAfterImage(new Button(), EventArgs.Empty);
                var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value == "Woman";
                if (IsWoman)
                {

                    await AddQuestion("What are your goals?");

                    await AddCheckbox("Less fat", Man_LessFat_Clicked);
                    await AddCheckbox("Better health", Man_BetterHealth_Clicked);
                    await AddCheckbox("More energy", WoMan_MoreEnergy_Clicked);
                    await AddCheckbox("Toned muscles", WoMan_FemaleToned_Clicked);

                    await AddOptions("Continue", WomanTakeActionBasedOnInput);
                }
                else
                {
                    await AddQuestion("What are your goals?");
                    await Task.Delay(300);

                    await AddCheckbox("More muscle", Man_MoreMuscle_Clicked);
                    await AddCheckbox("Less fat", Man_LessFat_Clicked);
                    await AddCheckbox("Better health", Man_BetterHealth_Clicked);
                    await AddCheckbox("Stronger sex drive", Man_StorngerSexDrive_Clicked);

                    await AddOptions("Continue", ManTakeActionBasedOnInput);
                }

            }
            catch (Exception ex)
            {

            }

        }

        void Man_MoreMuscle_Clicked(object sender, EventArgs e)
        {
            ManMoreMuscle = !ManMoreMuscle;
            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
            img.Source = ManMoreMuscle ? "done.png" : "Undone.png";
        }

        void WoMan_MoreEnergy_Clicked(object sender, EventArgs e)
        {
            FemaleMoreEnergy = !FemaleMoreEnergy;
            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
            img.Source = FemaleMoreEnergy ? "done.png" : "Undone.png";
        }

        void WoMan_FemaleToned_Clicked(object sender, EventArgs e)
        {
            FemaleToned = !FemaleToned;
            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
            img.Source = FemaleToned ? "done.png" : "Undone.png";
        }

        void Man_LessFat_Clicked(object sender, EventArgs e)
        {
            ManLessFat = !ManLessFat;
            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
            img.Source = ManLessFat ? "done.png" : "Undone.png";
        }

        void Man_BetterHealth_Clicked(object sender, EventArgs e)
        {
            ManBetterHealth = !ManBetterHealth;
            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
            img.Source = ManBetterHealth ? "done.png" : "Undone.png";
        }

        void Man_StorngerSexDrive_Clicked(object sender, EventArgs e)
        {
            ManStorngerSexDrive = !ManStorngerSexDrive;
            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
            img.Source = ManStorngerSexDrive ? "done.png" : "Undone.png";
        }
        async void ManTakeActionBasedOnInput(object sender, EventArgs e)
        {
            try
            {
                var count = 0;
                count += ManMoreMuscle ? 1 : 0;
                count += ManLessFat ? 1 : 0;
                count += ManBetterHealth ? 1 : 0;
                count += ManStorngerSexDrive ? 1 : 0;
                var responseText = "";
                if (ManMoreMuscle)
                    responseText = "More muscle";
                if (ManLessFat)
                    responseText += responseText == "" ? "Less fat" : "\nLess fat";
                if (ManBetterHealth)
                    responseText += responseText == "" ? "Better health" : "\nBetter health";
                if (ManStorngerSexDrive)
                    responseText += responseText == "" ? "Stronger sex drive" : "\nStronger sex drive";
                if (responseText != "")
                    await AddAnswer(responseText);
                focusText = responseText;
                LocalDBManager.Instance.SetDBSetting("focusText", focusText);
                _firebase.LogEvent("chose_goals", focusText);
                if (ManMoreMuscle && ManLessFat)//&& count > 2
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
                    //await AddQuestion("Got it. You can build muscle 59% faster with rest-pause sets. I'm adding them to your program. High reps burn more fat.");


                }
                else if (ManMoreMuscle)
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscle");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscle");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "5");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "12");
                   // await AddQuestion("Got it. You can build muscle 59% faster with rest-pause sets. Adding them to your program...");

                }
                else if (ManLessFat)
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "FatBurning");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "FatBurning");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "12");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "20");

                   // await AddQuestion("OK. High reps burn more fat. I'm setting yours at 12-20.");

                }
                else if (ManBetterHealth || ManStorngerSexDrive)
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscleBurnFat");

                    LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
                    //await AddQuestion("Got it.");

                }
                else
                    return;
                if (ManLessFat && ManMoreMuscle)
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscleBurnFat");

                //if (ManLessFat)
                //    FatLossOption();
                //else
                AskForHumanSupport();

            }
            catch (Exception ex)
            {

            }

        }

        async void TakeBodyWeight()
        {

            try
            {
                //BotList.Add(new BotModel()
                //{ Type = BotType.Empty });
                await AddQuestion("What is your body weight?");
                var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value == "Woman";

                if (IsWoman)
                {

                    BodyweightPicker.SelectedItem = LocalDBManager.Instance.GetDBSetting("massunit").Value == "lb" ? "140" : "65";
                }
                else
                    BodyweightPicker.SelectedItem = LocalDBManager.Instance.GetDBSetting("massunit").Value == "lb" ? "180" : "80";
                BodyweightPicker.IsVisible = true;
                Device.BeginInvokeOnMainThread(() =>
                {
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                });
                try
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        BodyweightPicker.Focus();
                    });
                }
                catch (Exception ex)
                {

                }

            }
            catch (Exception ex)
            {

            }
            //PromptConfig firsttimeExercisePopup = new PromptConfig()
            //{
            //    InputType = InputType.Number,
            //    IsCancellable = false,
            //    Title = "What is your body weight?",
            //    Message = $"",
            //    Placeholder = "Enter your bodyweight here",
            //    OkText = AppResources.Ok,
            //    //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogFirstTimeExercise),
            //    OnAction = async (ageResponse) =>
            //    {
            //        if (string.IsNullOrWhiteSpace(ageResponse.Value) || Convert.ToDecimal(ageResponse.Value, CultureInfo.InvariantCulture) < 1)
            //        {
            //            BotList.Remove(BotList.Last());
            //            TakeBodyWeight();
            //            return;
            //        }
            //        BotList.Remove(BotList.Last());
            //        int age = Convert.ToInt32(ageResponse.Value, CultureInfo.InvariantCulture);
            //        LocalDBManager.Instance.SetDBSetting("BodyWeight", Convert.ToString(age));
            //        await AddAnswer(Convert.ToString(age));



            //        SetupMassUnit();
            //    }
            //};
            //firsttimeExercisePopup.OnTextChanged += FirsttimeExercisePopup_OnTextChanged;
            //UserDialogs.Instance.Prompt(firsttimeExercisePopup);

        }

        async void WomanTakeActionBasedOnInput(object sender, EventArgs e)
        {
            try
            {
                var count = 0;
                count += FemaleMoreEnergy ? 1 : 0;
                count += ManLessFat ? 1 : 0;
                count += ManBetterHealth ? 1 : 0;
                count += FemaleToned ? 1 : 0;

                var responseText = "";
                if (ManLessFat)
                    responseText = "Less fat";
                if (ManBetterHealth)
                    responseText += responseText == "" ? "Better health" : "\nBetter health";
                if (FemaleMoreEnergy)
                    responseText += responseText == "" ? "More energy" : "\nMore energy";
                if (FemaleToned)
                    responseText += responseText == "" ? "Toned muscles" : "\nToned muscles";
                if (responseText != "")
                    await AddAnswer(responseText);
                focusText = responseText;
                LocalDBManager.Instance.SetDBSetting("focusText", focusText);
                _firebase.LogEvent("chose_goals", focusText);
                if (FemaleToned && ManLessFat) //&& count > 2
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");

                   // await AddQuestion("Got it. You can build muscle 59% faster with rest-pause sets. I'm adding them to your program. High reps burn more fat.");

                }
                else if (FemaleToned)
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscle");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscle");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "5");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "12");
                    //await AddQuestion("Got it. You can tone up 59% faster with rest-pause sets. Adding them to your program...");


                }
                else if (ManLessFat)
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "FatBurning");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "FatBurning");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "12");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "20");
                    //await AddQuestion("OK. High reps burn more fat. I'm setting yours at 12-20.");

                }
                else if (ManBetterHealth || FemaleMoreEnergy)
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
                    //await AddQuestion("Got it.");

                }
                else
                    return;
                if (ManLessFat && FemaleToned)
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscleBurnFat");
                //if (ManLessFat)
                //    FatLossOption();
                //else
                AskForHumanSupport();

            }
            catch (Exception ex)
            {

            }
        }

        async void FatLossOption()
        {
            try
            {
                await ClearOptions();
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                await AddOptions("Yes", async (o, ee) =>
                {
                    await AddAnswer("Yes");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);

                    //await AddQuestion("According to the International Society of Sports Nutrition, \"A wide range of dietary approaches can be similarly effective for improving body composition.\" In other words, you don’t need to follow a specific diet (e.g. low fat or low carb). Instead, the key is finding the one that works best for you, and that you can stick to long-term.");
                    await AddQuestion("Great! Look for an email from my assistant Victoria Kingsley within one business day.");
                    await AddOptions(AppResources.GotIt, async (oo, eee) =>
                    {
                        await AddAnswer(AppResources.GotIt);
                        if (Device.RuntimePlatform.Equals(Device.Android))
                            await Task.Delay(300);
                        IsHumanSupport = true;
                        SetupEpeBegginer();
                    });
                });

                await AddOptions("No", async (o, ee) =>
                {
                    await AddAnswer("No");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    if (ManLessFat)
                        SetupEpeBegginer();
                    else
                        AskForHumanSupport();
                });

            }
            catch (Exception ex)
            {

            }
        }
        async void AskForHumanSupport()
        {
            //try
            //{
            //    await AddQuestion("Would you like a human to get in touch about your goals?");
            //    await AddOptions("Yes", async (o, ee) =>
            //    {
            //        IsHumanSupport = true;
            //        await AddAnswer("Yes");
            //        ClearOptions();
            //        if (Device.RuntimePlatform.Equals(Device.Android))
            //            await Task.Delay(300);

            //        await AddQuestion("Great! Look for an email from my assistant Victoria Kingsley within one business day.");

            //        await ClearOptions();

            //    SetupEpeBegginer();
            //    });

            //    await AddOptions("No", async (o, ee) =>
            //    {
            //        IsHumanSupport = false;
            //        await AddAnswer("No");
            //        ClearOptions();
            //        if (Device.RuntimePlatform.Equals(Device.Android))
            //            await Task.Delay(300);
            //        await AddQuestion(AppResources.GotIt);

            //        await ClearOptions();

            //    SetupEpeBegginer();


            //    });

            //}
            //catch (Exception ex)
            //{

            //}
            IsHumanSupport = false;
            SetupEpeBegginer();
        }

        async void SetupEpeBegginer()
        {
            if (LocalDBManager.Instance.GetDBSetting("ExLevel").Value == "Exp")
            {
                LocalDBManager.Instance.SetDBSetting("workout_place", "gym");
               // LocalDBManager.Instance.SetDBSetting("experience", "more3years");
                //NoAdvancedClicked(new DrMuscleButton(), EventArgs.Empty);
                //await AddQuestion("How old are you?");
                //if (Device.RuntimePlatform.Equals(Device.Android))
                //    await Task.Delay(300);
                //GetAge();
                if (LocalDBManager.Instance.GetDBSetting("experience").Value == "beginner")
                    AddCardio();// 
                else
                    workoutPlace();
                return;
            }
            LocalDBManager.Instance.SetDBSetting("experience", "less1year");
            // await AddQuestion("Are you training at home with no equipment?");
            Device.BeginInvokeOnMainThread(() =>
            {
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            });
            BeginnerSetup();
        }
        async void MenNext_Clicked(object sender, EventArgs e)
        {

        }

        async void WomanButton_Clicked(object sender, EventArgs e)
        {
            BotList.Add(new BotModel()
            {
                Answer = AppResources.Woman,
                Type = BotType.Ans
            });
            await Task.Delay(300);
            await ClearOptions();
            LocalDBManager.Instance.SetDBSetting("gender", "Woman");

            //await AddQuestion("Woman often want (tap all that apply):");

            //await AddCheckbox("Less fat", Man_LessFat_Clicked);
            //await AddCheckbox("Better health", Man_BetterHealth_Clicked);
            //await AddCheckbox("More energy", WoMan_MoreEnergy_Clicked);
            //await AddCheckbox("Toned muscles", WoMan_FemaleToned_Clicked);

            //await AddOptions("Continue", WomanTakeActionBasedOnInput);
            //SetupMassUnit();
            await AddQuestion("Age?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            GetAge();
            //SetupMainGoal();

        }

        async void WomanThinClicked(object sender, System.EventArgs e)
        {

            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.ThinWomenOftenSay);
            await AddQuestion(AppResources.TheyWantToGetFitAndStrongWhileMaintaingLeanPhysiqueAddSizeToLegsBootyDenseLookingMuscleOverall, false);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddOptions(AppResources.Yes, WomanYesSkinnyClicked);
            await AddOptions(AppResources.NoChooseOtherGoal, WomanOtherGoalClicked);
        }

        async void WomanYesSkinnyClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscle");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "5");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "12");
            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion(AppResources.GotItAreYouABeginnerWithNoEquipment);

            BeginnerSetup();
        }

        async void WomanOtherGoalClicked(object sender, System.EventArgs e)
        {

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion(AppResources.PleaseChooseAGoal);
            await AddQuestion(AppResources.DontWorryLiftingWightsWontMakeyouBulky);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddOptions(AppResources.FocusOnToningUp, WomanFocusMuscleClicked);
            await AddOptions(AppResources.ToneUpAndSlimDown, WomanFocusBothClicked);
            await AddOptions(AppResources.FocusOnSlimmingDown, WomanFocusBurnFatClicked);
        }

        async void WomanFocusMuscleClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscle");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "5");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "12");
            await AddAnswer(((DrMuscleButton)sender).Text);

            BeginnerSetup();
        }

        async void WomanFocusBothClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
            await AddAnswer(((DrMuscleButton)sender).Text);
            BeginnerSetup();
        }

        async void WomanFocusBurnFatClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "FatBurning");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "12");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "20");
            await AddAnswer(((DrMuscleButton)sender).Text);
            BeginnerSetup();
        }

        async void WomanMidsizeClicked(object sender, System.EventArgs e)
        {

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion(AppResources.MidsizeWomenOftenSay);
            await AddQuestion(AppResources.TheyWantToGetFitAndStrongLeanerAndComfortableInMyBody);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddOptions(AppResources.Yes, WomanYesMidsizeClicked);
            await AddOptions(AppResources.NoChooseOtherGoal, WomanOtherGoalClicked);
        }

        //Start from here
        async void WomanYesMidsizeClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion(AppResources.GotItAreYouABeginnerWithNoEquipment);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            BeginnerSetup();
        }


        async void WomanFullFClicked(object sender, System.EventArgs e)
        {
            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.FullFiguredOften);
            await AddQuestion(AppResources.HaveAHardTimeLosingWeightGetFatLookingAtFood);

            await AddOptions(AppResources.YesICanGainWeightEasily, FullFClicked);
            await AddOptions(AppResources.NoIDontGainWeightThatEasily, FullFClicked);
        }

        async void FullFClicked(object sender, System.EventArgs e)
        {
            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.ThankYouTitle);
            await AddQuestion(AppResources.FullFiguredWomenAlsoOftenSay);
            await AddQuestion(AppResources.TheyWantToGetFitAndStrongWhileDroppingBodyFatShapeArms);

            await AddOptions(AppResources.Yes, WomanYesBigClicked);
            await AddOptions(AppResources.NoChooseOtherGoal, WomanOtherGoalClicked);
        }

        async void WomanYesBigClicked(object sender, System.EventArgs e)
        {
            await AddAnswer(((DrMuscleButton)sender).Text);
            LocalDBManager.Instance.SetDBSetting("reprange", "FatBurning");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "12");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "20");

            await AddQuestion(AppResources.BurningFatGotItAreYouBegginerWithNoEquipment);
            BeginnerSetup();
        }

        async void SkinnyManButton_Clicked(object sender, EventArgs e)
        {
            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.SkinnyMenOften);
            await AddQuestion(AppResources.HaveAHardTimeGainingWeightSomeSayIEatConstantlyAndWorkMyButtOff);

            await ClearOptions();

            await AddOptions(AppResources.YesIHaveAHardTimeGaining, ManBodyTypeClicked);
            await AddOptions(AppResources.NoIDontHaveAHardTime, ManBodyTypeClicked);
            await AddOptions(AppResources.NotSureIveNeverLiftedBefore, ManBodyTypeClicked);

        }
        async void BigManButton_Clicked(object sender, EventArgs e)
        {

            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.BigMenOftenSay);
            await AddQuestion(AppResources.TheyWantToGetRidOfThisBodyFatAndLoseMyGut);

            await AddOptions(AppResources.Yes, ManYesBigClicked);
            await AddOptions(AppResources.NoChooseOtherGoal, ManOtherGoalClicked);

        }

        async void ManYesBigClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "FatBurning");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "12");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "20");
            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.BurningFatGotItAreYouBegginerWithNoEquipment);
            BeginnerSetup();
        }
        async void MidsizeManButton_Clicked(object sender, EventArgs e)
        {

            await AddAnswer(((DrMuscleButton)sender).Text);
            await AddQuestion(AppResources.MidsizeMenOftenSay);
            await AddQuestion(AppResources.TheyWantToGetFitStrongAndMoreMuscularGainLeanMassAndHaveAVisibleSetOf);

            await AddOptions(AppResources.Yes, ManYesMidsizeClicked);
            await AddOptions(AppResources.NoChooseOtherGoal, ManOtherGoalClicked);
        }

        async void ManYesMidsizeClicked(object sender, System.EventArgs e)
        {
            await AddAnswer(((DrMuscleButton)sender).Text);
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
            await AddQuestion(AppResources.BuildingMuscleBuriningFatGotItAreYouBeginner);
            BeginnerSetup();
        }


        async void ManBodyTypeClicked(object sender, System.EventArgs e)
        {

            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.GotIt);
            await AddQuestion(AppResources.SkinnyMenAlsoOftenSay);
            await AddQuestion(AppResources.TheyWantToPutOnLeanMassWhileKeepingmyAbsDefinedGainHealthy);

            await AddOptions(AppResources.Yes, ManYesSkinnyClicked);
            await AddOptions(AppResources.NoChooseOtherGoal, ManOtherGoalClicked);

        }

        async void ManYesSkinnyClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscle");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "5");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "12");

            await AddAnswer(((DrMuscleButton)sender).Text);
            await AddQuestion(AppResources.BuildingMuscleGotItAreYouABeginnerWithNoEquipment);
            BeginnerSetup();
        }

        async void ManOtherGoalClicked(object sender, System.EventArgs e)
        {
            try
            {
                await AddAnswer(((DrMuscleButton)sender).Text);

                BotList.Add(new BotModel()
                {
                    Question = AppResources.DontWorryYouCanCustomizeLater,
                    Type = BotType.Ques
                });
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                await Task.Delay(300);
                BotList.Add(new BotModel()
                {
                    Question = AppResources.PleaseChooseAGoal,
                    Type = BotType.Ques
                });

                await Task.Delay(300);

                await ClearOptions();
                var manBurnFatButton = new DrMuscleButton()
                {
                    Text = AppResources.FocusOnBurningFat,
                    TextColor = Color.White
                };
                manBurnFatButton.Clicked += ManFocusBurnFatClicked;
                SetDefaultButtonStyle(manBurnFatButton);
                var grid = new Grid();
                var pancakeView = new PancakeView() {HeightRequest = 50, Margin = new Thickness(25, 2) };
                pancakeView.OffsetAngle = 45;
                pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#0C2432"), Offset = 0 });
                pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#195276"), Offset = 1 });
                grid.Children.Add(pancakeView);
                grid.Children.Add(manBurnFatButton);

                stackOptions.Children.Add(grid);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                await Task.Delay(300);

                var manBuildMuscleBurnFatButton = new DrMuscleButton()
                {
                    Text = AppResources.BuildMuscleAndBurnFat,
                    TextColor = Color.White
                };
                manBuildMuscleBurnFatButton.Clicked += ManFocusBothClicked;
                SetDefaultButtonStyle(manBuildMuscleBurnFatButton);
                stackOptions.Children.Add(manBuildMuscleBurnFatButton);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                await Task.Delay(300);

                var manBuildMuscleButton = new DrMuscleButton()
                {
                    Text = AppResources.FocusOnBuildingMuscle,
                    TextColor = Color.White,
                };
                manBuildMuscleButton.Clicked += ManFocusMuscleClicked;
                SetDefaultButtonStyle(manBuildMuscleButton);

                stackOptions.Children.Add(manBuildMuscleButton);
                await Task.Delay(300);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);

            }
            catch (Exception ex)
            {

            }
        }

        async void BeginnerSetup()
        {
            try
            {
                await ClearOptions();

                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                //await AddOptions("At home, no equipment", YesBeginnerClicked);
                //await AddOptions("Gym or home gym", async (sender, e) => {

                LocalDBManager.Instance.SetDBSetting("experience", "less1year");

                //await AddAnswer(((DrMuscleButton)sender).Text);
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                if (LocalDBManager.Instance.GetDBSetting("experience").Value == "beginner")
                    AddCardio();// 
                else
                    workoutPlace();
                //});

            }
            catch (Exception ex)
            {

            }
        }

        async void ManFocusMuscleClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscle");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "5");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "12");

            await AddAnswer(AppResources.FocusOnBuildingMuscle);

            BeginnerSetup();

        }

        async void ManFocusBothClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");

            await AddAnswer(AppResources.BuildMuscleAndBurnFat);
            BeginnerSetup();

        }

        async void ManFocusBurnFatClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "FatBurning");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "12");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "20");

            await AddAnswer(AppResources.FocusOnBurningFat);
            BeginnerSetup();
        }

        async void YesBeginnerClicked(object sender, System.EventArgs e)
        {
            await ClearOptions();
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
            LocalDBManager.Instance.SetDBSetting("experience", "beginner");
            LocalDBManager.Instance.SetDBSetting("workout_place", "homeBodyweightOnly");


            await AddAnswer(((DrMuscleButton)sender).Text);

            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion("Age?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            GetAge();
        }


        async void AddGotItBeforeAge()
        {
            try
            {
                await ClearOptions();
                await AddOptions(AppResources.GotIt, GotIt_Clicked);
                async void GotIt_Clicked(object sender, EventArgs e)
                {

                    await AddAnswer(((DrMuscleButton)sender).Text);
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    await AddQuestion("Age?");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    GetAge();
                }

            }
            catch (Exception ex)
            {

            }
        }
        async void NoAdvancedClicked(object sender, System.EventArgs e)
        {
            //await AddAnswer(((DrMuscleButton)sender).Text);
            //if (Device.RuntimePlatform.Equals(Device.Android))

            //    await Task.Delay(300);
            await AddQuestion("Experience lifting weights?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await ClearOptions();
            await AddOptions(AppResources.LessThan1Year, LessOneYearClicked);
            await AddOptions(AppResources.OneToThreeYears, OneThreeYearsClicked);
            await AddOptions(AppResources.MoreThan3Years, More3YearsClicked);
        }

        async void AskForProgram()
        {
            ClearOptions();
            var btn1 = new DrMuscleButton()
            {
                Text = "Another program",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius=0
            };
            btn1.Clicked += AnotherProgram_clicked;
            stackOptions.Children.Add(btn1);

            await AddOptions("Recommended program", RecommendedProgram_clicked);
        }
        async void AnotherProgram_clicked(object sender, EventArgs args)
        {
            //[Full body, 2-4x/week]
            //[Split body, 3-5x/week]
            await AddAnswer("Another program");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            ClearOptions();
            await AddOptions("Full-body (2-4 workouts / week)", (o,e) => {
                LocalDBManager.Instance.SetDBSetting("MainProgram", "Full body");
                LocalDBManager.Instance.SetDBSetting("CustomProgramName", "Full-body");
                GetGender();
            });

            await AddOptions("Upper/lower (3-4 / week)", (o, e) => {
                LocalDBManager.Instance.SetDBSetting("MainProgram", "Split body");
                LocalDBManager.Instance.SetDBSetting("CustomProgramName", "Up/Low Split");
                GetGender();
            });

            await AddOptions("Push/pull/legs (6 / week)", (o, e) =>
            {
                LocalDBManager.Instance.SetDBSetting("MainProgram", "PPL");
                LocalDBManager.Instance.SetDBSetting("CustomProgramName", "Push/Pull/Legs");
                GetGender();
            });

            await AddOptions("Powerlifting (2-4 / week)", (o, e) =>
            {
                LocalDBManager.Instance.SetDBSetting("MainProgram", "Powerlifting");
                LocalDBManager.Instance.SetDBSetting("CustomProgramName", "Powerlifting");
                GetGender();
            });

            await AddOptions("Bands only (2-4 / week)", (o, e) =>
            {
                LocalDBManager.Instance.SetDBSetting("MainProgram", "Bands only");
                LocalDBManager.Instance.SetDBSetting("CustomProgramName", "Buffed w/ Bands");
                GetGender();
            });

            await AddOptions("Bodyweight only (2-4 / week)", (o, e) =>
            {
                LocalDBManager.Instance.SetDBSetting("MainProgram", "Bodyweight");
                LocalDBManager.Instance.SetDBSetting("CustomProgramName", "Bodyweight");
                var level = 2;
                if (LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "new to training")
                {
                    level = 1;
                }

                else
                {
                    level = 2;
                }
                LocalDBManager.Instance.SetDBSetting("MainLevel", level.ToString());
                GetGender();
            });

        }

        async void ShowWorkoutReminder(bool isShowReminder)
        {
            LocalDBManager.Instance.SetDBSetting("Registring", "true");
            if (isShowReminder)
            await PopupNavigation.Instance.PushAsync(new ReminderPopup());
            ClearOptions();
            //GotItAfterImage(new DrMuscleButton(), EventArgs.Empty);

            //[Another level][Recommended level]



            //Setup Program




            string ProgramLabel = "";
            string programInfo = "";
            int level = 0;
            int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);


            if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Split"))
            {

                if (age > 50)
                {
                    ProgramLabel = "Up/Low Split Level 6";
                    programInfo = "This level includes A/B/C easy workouts to help you recover.";
                    level = 6;
                }
                else
                //if (age > 30)
                {
                    if (LocalDBManager.Instance.GetDBSetting("experience") != null && (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "more3years"))
                    {
                        ProgramLabel = "Up/Low Split Level 2";
                        programInfo = "This level includes new and harder exercises.";
                        level = 2;
                    }
                    else if (LocalDBManager.Instance.GetDBSetting("experience") != null && (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "1-3years"))
                    {
                        ProgramLabel = "Up/Low Split Level 1";
                        programInfo = "This level includes simple and effective exercises.";
                        level = 1;
                    }
                    else
                    {
                        ProgramLabel = "Full-Body Level 2";
                        programInfo = "This level includes new and harder exercises.";
                        level = 2;
                    }

                }
            }
            else if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("PPL"))
            {
                    ProgramLabel = "Push/Pull/Legs Level 1";
                    programInfo = "This level includes simple and effective exercises.";
                    level = 1;
            }
            else if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Powerlifting"))
            {

                if (age > 50)
                {
                    ProgramLabel = "Powerlifting Level 1";
                    programInfo = "This level includes simple and effective exercises.";
                    level = 1;
                }
                else
                //if (age > 30)
                {
                    if (LocalDBManager.Instance.GetDBSetting("experience") != null && (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "more3years"))
                    {
                        ProgramLabel = "Powerlifting Level 2";
                        programInfo = "This level includes new and harder exercises.";
                        level = 2;
                    }
                    else if (LocalDBManager.Instance.GetDBSetting("experience") != null && (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "1-3years"))
                    {
                        ProgramLabel = "Powerlifting Level 1";
                        programInfo = "This level includes simple and effective exercises.";
                        level = 1;
                    }
                    else
                    {
                        ProgramLabel = "Powerlifting Level 2";
                        programInfo = "This level includes new and harder exercises.";
                        level = 2;
                    }

                }
            }
            else if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Bands only"))
            {

                if (age > 50)
                {
                    ProgramLabel = "Buffed w/ Bands Level 1";
                    programInfo = "This level includes simple and effective exercises.";
                    level = 2;
                }
                else
                {
                    if (LocalDBManager.Instance.GetDBSetting("experience") != null && (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "more3years"))
                    {
                        ProgramLabel = "Buffed w/ Bands Level 2";
                        programInfo = "This level includes A/B workouts to help you recover.";
                        level = 2;
                    }
                    else if (LocalDBManager.Instance.GetDBSetting("experience") != null && (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "1-3years"))
                    {
                        ProgramLabel = "Buffed w/ Bands Level 1";
                        programInfo = "This level includes simple and effective exercises.";
                        level = 1;
                    }
                    else
                    {
                        ProgramLabel = "Buffed w/ Bands Level 2";
                        programInfo = "This level includes A/B workouts to help you recover.";
                        level = 2;
                    }

                }

            }
            else if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Bodyweight"))
            {

                //if (age > 50)
                //{
                //    ProgramLabel = "Bodyweight Level 1";
                //    programInfo = "This level includes simple and effective exercises.";
                //    level = 2;
                //}
                //else
                ////if (age > 30)
                //{
                    if (LocalDBManager.Instance.GetDBSetting("experience") != null && (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "more3years") || (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "1-3years"))
                    {
                        ProgramLabel = "Bodyweight Level 2";
                        programInfo = "This level includes new and harder exercises.";
                        level = 2;
                    }
                    
                    else
                    {
                        ProgramLabel = "Bodyweight Level 1";
                        programInfo = "This level includes simple and effective exercises.";
                        level = 1;
                    }

                if (LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "new to training")
                {
                    level = 1;
                }

                else
                {
                    level = 2;
                }
                LocalDBManager.Instance.SetDBSetting("MainLevel", level.ToString());

                //}
            }
            else
            {
                if (age > 50)
                {
                    ProgramLabel = "Full-Body Level 6";
                    programInfo = "This level includes A/B/C easy workouts to help you recover.";
                    level = 6;
                }
                else
                //if (age > 30)
                {
                    ProgramLabel = "Full-Body Level 1";
                    programInfo = "This level includes simple and effective exercises.";
                    level = 1;
                }
            }
            //await AddQuestion($"Based on your age and experience, I recommend you start on level {level}. {programInfo} Higher levels have more exercise variations.");



            //var btn1 = new DrMuscleButton()
            //{
            //    Text = "Another level",
            //    TextColor = Color.FromHex("#195377"),
            //    BackgroundColor = Color.Transparent,
            //    HeightRequest = 55,
            //    BorderWidth = 2,
            //    BorderColor = AppThemeConstants.BlueColor,
            //    Margin = new Thickness(25, 0),
            //    CornerRadius = 0
            //};
            //btn1.Clicked += async (s, e) => {

            //    //await UserDialogs.Instance.AlertAsync(new AlertConfig()
            //    //{
            //    //    Message = "Higher levels have more recovery and exercise variations. You can change this later.",
            //    //    OkText = AppResources.GotIt,
            //    //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
            //    //});
            //    List<string> levels = new List<string>();
            //    int lvl = 7;
            //    if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Split"))
            //    {
            //        lvl = 7;
            //    }
            //    else if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Powerlifting"))
            //    {
            //        lvl = 4;
            //    }
            //        else if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Bands only"))
            //    {
            //        lvl = 2;
            //    }
            //    else if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Bodyweight"))
            //    {
            //        lvl = 4;
            //    }


            //        for (int i = 1; i <= lvl; i++)
            //    {
            //        levels.Add($"Level {i}");
            //    }
            //    LevelPicker.ItemsSource = levels;
            //    LevelPicker.IsVisible = true;
            //    Device.BeginInvokeOnMainThread(() =>
            //    {
            //        LevelPicker.Focus();
            //    });
            //    LocalDBManager.Instance.SetDBSetting("Registring", "");
            //};
            //stackOptions.Children.Add(btn1);

            //await AddOptions($"Recommended level {level}", async (s, e) => {
            //    LocalDBManager.Instance.SetDBSetting("MainLevel", level.ToString());
            //    LocalDBManager.Instance.SetDBSetting("Registring", "");
            //    //SetupMassUnit();
            //    if (LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "new to training" || LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "returning from a break")
            //    {
            //        AskSetStyle();
            //        //SetupMassUnit();
            //    }
            //    else
            //    {
            //        AskSetStyle();
            //    }
            //});

            LocalDBManager.Instance.SetDBSetting("MainLevel", level.ToString());
            LocalDBManager.Instance.SetDBSetting("Registring", "");
            //SetupMassUnit();
            //if (LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "new to training" || LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "returning from a break")
            //{
            //    AskSetStyle();
            //    //SetupMassUnit();
            //}
            //else
            //{
            //    AskSetStyle();
            //}
            ExerciseVariety();
        }

        private async void AskSetStyle()
        {
            
                await ClearOptions();
                await AddQuestion("Try rest-pause sets? They're harder, but make your workouts 59% faster.");
                

            var btn1 = new DrMuscleButton()
            {
                Text = "Normal sets",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn1.Clicked += (o, ev) => {
                AddAnswer("Normal sets");
                SetStyle = true;
                SetupMassUnit();
            };
            stackOptions.Children.Add(btn1);
            if (LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "new to training" || LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "returning from a break")
            { }
            else {


                var btnPyramid = new DrMuscleButton()
                {
                    Text = "Pyramid sets",
                    TextColor = Color.FromHex("#195377"),
                    BackgroundColor = Color.Transparent,
                    HeightRequest = 55,
                    BorderWidth = 2,
                    BorderColor = AppThemeConstants.BlueColor,
                    Margin = new Thickness(25, 2),
                    CornerRadius = 0
                };
                btnPyramid.Clicked += (o, ev) => {
                    AddAnswer("Pyramid sets");
                    SetStyle = false;
                    IsPyramid = true;
                    SetupMassUnit();
                };
                stackOptions.Children.Add(btnPyramid);

                var btn2 = new DrMuscleButton()
            {
                Text = "Reverse pyramid sets",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                    CornerRadius = 0
                };
            btn2.Clicked += (o, ev) => {
                AddAnswer("Reverse pyramid sets");
                SetStyle = null;
                SetupMassUnit();
            };
            stackOptions.Children.Add(btn2);
            }
            await AddOptions("Rest-pause sets", (ss, ee) =>
                {
                    AddAnswer("Rest-pause sets");
                    SetStyle = false;
                    SetupMassUnit();
                });
                
            
        }

        async void GetGender()
        {
            
            ClearOptions();
            GotItAfterImage(new DrMuscleButton(), EventArgs.Empty);
        }

        async void RecommendedProgram_clicked(object sender, EventArgs args)
        {

            try
            {

                if (LocalDBManager.Instance.GetDBSetting("experience") != null && (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "more3years") || (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "1-3years"))//
                {
                    LocalDBManager.Instance.SetDBSetting("MainProgram", "Split body");
                    LocalDBManager.Instance.SetDBSetting("CustomProgramName", "Split body");
                }
                else
                {
                    LocalDBManager.Instance.SetDBSetting("CustomProgramName", "Full-body");
                    LocalDBManager.Instance.SetDBSetting("MainProgram", "Full body");
                }

            }
            catch (Exception ex)
            {
                LocalDBManager.Instance.SetDBSetting("CustomProgramName", "Full-body");
                LocalDBManager.Instance.SetDBSetting("MainProgram", "Full body");
            }
            GetGender();
        }

        async void LessOneYearClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("experience", "less1year");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);

            //await AddQuestion("Less than 1 year? I recommend a full-body program.");
            AskforProgramsWithABExperience();
            LocalDBManager.Instance.SetDBSetting("MainProgram", "Full body");
            //RecommendedProgram_clicked(sender, e);
           
        }

        async void OneThreeYearsClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("experience", "1-3years");

            await AddAnswer(((DrMuscleButton)sender).Text);
            //await AddQuestion("1-3 years? I recommend an upper/lower-body split program.");
            LocalDBManager.Instance.SetDBSetting("MainProgram", "Split body");
            AskforProgramsWithABExperience();
            //RecommendedProgram_clicked(sender, e);
            //await AddQuestion("How old are you?");
            //if (Device.RuntimePlatform.Equals(Device.Android))
            //    await Task.Delay(300);
            //GetAge();
        }

        async void More3YearsClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("experience", "more3years");

            await AddAnswer(((DrMuscleButton)sender).Text);
            //await AddQuestion("3+ years? I recommend an upper/lower-body split program.");
            LocalDBManager.Instance.SetDBSetting("MainProgram", "Split body");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            AskforProgramsWithABExperience();
            //RecommendedProgram_clicked(sender, e);
            //await AddQuestion("How old are you?");
            //if (Device.RuntimePlatform.Equals(Device.Android))
            //    await Task.Delay(300);
            //GetAge();
        }

        async void workoutPlace()
        {
            await AddQuestion("Where do you train?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);

            var btn1 = new DrMuscleButton()
            {
                Text = AppResources.HomeBodtweightOnly,
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn1.Clicked += BodyweightClicked;
            stackOptions.Children.Add(btn1);
            //
            var btnBands = new DrMuscleButton()
            {
                Text = "Home (bodyweight & bands)",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btnBands.Clicked += BodyweightBandsClicked;
            stackOptions.Children.Add(btnBands);
            var btn2 = new DrMuscleButton()
            {
                Text = AppResources.HomeGymBasicEqipment,
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn2.Clicked += HomeClicked;
            stackOptions.Children.Add(btn2);

            //await AddOptions(AppResources.HomeBodtweightOnly, BodyweightClicked);
            //await AddOptions(AppResources.HomeGymBasicEqipment, HomeClicked);
            await AddOptions(AppResources.Gym, GymClicked);

        }

        async void GymClicked(object senders, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("workout_place", "gym");

            await AddAnswer(((DrMuscleButton)senders).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion("Confirm equipment");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            IsPully = true;
            await AddCheckbox("Pulley", (sender, ev) =>
            {
                IsPully = !IsPully;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = IsPully ? "done.png" : "Undone.png";
            }, true);
            IsPlates = true;
            await AddCheckbox("Plates", (sender, ev) =>
            {
                IsPlates = !IsPlates;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = IsPlates ? "done.png" : "Undone.png";
            }, true);
            IsChinupBar = true;
            await AddCheckbox("Chin-up bar", (sender, ev) =>
            {
                IsChinupBar = !IsChinupBar;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = IsChinupBar ? "done.png" : "Undone.png";
            }, true);

            isDumbbells = true;
            await AddCheckbox("Dumbbells", (sender, ev) =>
            {
                isDumbbells = !isDumbbells;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = isDumbbells ? "done.png" : "Undone.png";
            }, true);
            IsEquipment = true;

            await AddOptions("Continue", async (sender, ee) =>
            {
                await AddAnswer("Continue");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                //AskforBodypartPriority();
                AskForIncrements();
            });
        }

        async void HomeClicked(object senders, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("workout_place", "home");

            await AddAnswer(((DrMuscleButton)senders).Text);
            ClearOptions();
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion("What equipment do you have?");

            await AddCheckbox("Pulley", (sender, ev) =>
            {
                IsPully = !IsPully;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = IsPully ? "done.png" : "Undone.png";
            });

            await AddCheckbox("Plates", (sender, ev) =>
            {
                IsPlates = !IsPlates;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = IsPlates ? "done.png" : "Undone.png";
            });
            await AddCheckbox("Chin-up bar", (sender, ev) =>
            {
                IsChinupBar = !IsChinupBar;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = IsChinupBar ? "done.png" : "Undone.png";
            });

            isDumbbells = true;
            await AddCheckbox("Dumbbells", (sender, ev) =>
            {
                isDumbbells = !isDumbbells;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = isDumbbells ? "done.png" : "Undone.png";
            }, true);

            IsEquipment = true;

            await AddOptions("Continue", async (sender, ee) =>
            {
                await AddAnswer("Continue");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                //AskforBodypartPriority();
                AskForIncrements();
               
            });
        }

        async void AskForIncrements()
        {
            ClearOptions();
            await AddQuestion("Your weights go up in what increments? Customize detailed dumbbells, plates, and more later in Settings.");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";

            
            var btn2 = new DrMuscleButton()
            {
                Text = "Not sure (set up later in Settings)",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn2.Clicked += async (s, e) => {
                await AddAnswer("Not sure (set up later in Settings)");
                IncrementUnit = null;
                AskforBodypartPriority();

            };
            stackOptions.Children.Add(btn2);

            var btn1 = new DrMuscleButton()
            {
                Text = isKg ? "1 kg" : "2.5 lbs",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn1.Clicked += async (s, e) => {
                await AddAnswer(isKg ? "1 kg" : "2.5 lbs");
                IncrementUnit = new MultiUnityWeight(isKg ? (decimal)1 : (decimal)2.5, isKg ? "kg" : "lb");
                AskforBodypartPriority();
            };
            stackOptions.Children.Add(btn1);

            await AddOptions(isKg ? "2.5 kg" : "5 lbs", async (s, e) => {
                await AddAnswer(isKg ? "2.5 kg" : "5 lbs");
                IncrementUnit = new MultiUnityWeight(isKg ? (decimal)2.5 : (decimal)5, isKg ? "kg" : "lb");
                AskforBodypartPriority();
            });
        }

        async void BodyweightClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("workout_place", "homeBodyweightOnly");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            var level = 0;
            if (LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "new to training")
            {
                level = 1;
            }

            else
            {
                level = 2;
            }
            LocalDBManager.Instance.SetDBSetting("MainLevel", level.ToString());
            AskforBodypartPriority();
        }

        async void BodyweightBandsClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("workout_place", "homeBodyweightBandsOnly");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            //await AddQuestion("OK, bodyweight exercises only. No problem.");
            //if (Device.RuntimePlatform.Equals(Device.Android))
            //    await Task.Delay(300);
            //AddCardio();
            AskforBodypartPriority();
        }
        private async void BodypartPicker_Unfocused(object sender, FocusEventArgs e)
        {
            //bodypartName = (sender as Picker).SelectedIndex == -1 || (sender as Picker).SelectedIndex == 0 ? "" : (string)(sender as Picker).SelectedItem;
            bodypartName =  (string)(sender as Picker).SelectedItem;
            if ((sender as Picker).SelectedIndex != -1)
            {
                await AddAnswer((string)(sender as Picker).SelectedItem);
                LocalDBManager.Instance.SetDBSetting("BodypartPriority", bodypartName);

            }
                
            else
                await AddAnswer("No thanks");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await ClearOptions();
            AddCardio();
        }

        async void AskforBodypartPriority()
        {
            bodypartName = "";
            await AddQuestion("Prioritize a body part?");
            LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");



            await AddOptions("Yes, select part", async (sender, e) =>
            {
                if (BodyPartPicker != null)
                {
                    BodyPartPicker.Unfocused -= BodypartPicker_Unfocused;
                }
                BodyPartPicker = new Picker();
                List<string> bodyParts = new List<string>();
                //bodyParts.Add("");
                //bodyParts.Add("No thanks");
                //bodyParts.Add("Biceps");
                //bodyParts.Add("Chest");
                //bodyParts.Add("Abs");
                //bodyParts.Add("Legs");
                //bodyParts.Add("Glutes");
                
                bodyParts.Add("Abs");
                bodyParts.Add("Biceps");
                bodyParts.Add("Calves");
                bodyParts.Add("Chest");
                bodyParts.Add("Glutes");
                bodyParts.Add("Legs");
                bodyParts.Add("Shoulders");
                bodyParts.Add("Traps");
                bodyParts.Add("Triceps");
                bodyParts.Add("Upper back");

                BodyPartPicker.ItemsSource = bodyParts;
                StackMain.Children.Insert(0, BodyPartPicker);
                BodyPartPicker.Unfocused += BodypartPicker_Unfocused;
                BodyPartPicker.Focus();
            });

            await AddOptions("No thanks", async (sender, e) =>
            {
                bodypartName = "";
                await AddAnswer("No thanks");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                AddCardio();
            });

        }

        async void AddCardio()
        {
            await AddQuestion("Cardio?", false);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);

            await AddOptions("Include cardio", async (sender, e) =>
            {
                await AddAnswer("Include cardio");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                IsIncludeCardio = true;
                AskForMobility();
            });

            await AddOptions("No cardio", async (sender, e) =>
            {
                IsIncludeCardio = false;
                await AddAnswer("No cardio");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                AskForMobility();
            });
        }

        async void AskForMobility()
        {
            await AddQuestion("Guided warm-ups? Prevent injuries and boost flexibility, mobility, and performance.", false);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            if (LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "new to training" || LocalDBManager.Instance.GetDBSetting("CustomExperience").Value == "returning from a break")
                mobilityLevel = "Beginner";
            else
            {
                mobilityLevel = "Intermediate";
                if (LocalDBManager.Instance.GetDBSetting("experience") != null && (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "more3years") || (LocalDBManager.Instance.GetDBSetting("experience")?.Value == "1-3years"))
                    mobilityLevel = "Advanced";
            }
            await AddOptions("No mobility warm-ups", async (sender, e) =>
            {
                IsIncludeMobility = false;
                await AddAnswer("No mobility warm-ups");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                if (Device.RuntimePlatform == Device.iOS)
                    SetupAppleSync();
                else
                    SetTimerOption();
            });

            await AddOptions("Include mobility warm-ups", async (sender, e) =>
            {
                await AddAnswer("Include mobility warm-ups");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                IsIncludeMobility = true;
                if (Device.RuntimePlatform == Device.iOS)
                    SetupAppleSync();
                else
                    SetTimerOption();
            });

            
        }

        async void SetupAppleSync()
        {
            await AddQuestion("Sync with Apple Health?", false);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            //await AddOptions("30 min", QuickmodeOnClicked);
            //await AddOptions("45 min", QuickmodeMediumClicked);

            var btn1 = new DrMuscleButton()
            {
                Text = "No",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn1.Clicked += async (sender, e) => {
                await AddAnswer("No");
                LocalDBManager.Instance.SetDBSetting("AppleSync", "false");
                SetTimerOption();
            };
            stackOptions.Children.Add(btn1);

            //var btn2 = new DrMuscleButton()
            //{
            //    Text = "45 min",
            //    TextColor = Color.FromHex("#195377"),
            //    BackgroundColor = Color.Transparent,
            //    HeightRequest = 55,
            //    BorderWidth = 2,
            //    BorderColor = AppThemeConstants.BlueColor,
            //    Margin = new Thickness(25, 0)
            //};
            //btn2.Clicked += QuickmodeMediumClicked;
            //stackOptions.Children.Add(btn2);


            await AddOptions("Yes, sync", async (sender, e) => {
                LocalDBManager.Instance.SetDBSetting("AppleSync", "true");
                await AddAnswer("Yes, sync");
                    IHealthData _healthService = DependencyService.Get<IHealthData>();
                    await _healthService.GetHealthPermissionAsync(async (r) =>
                    {
                        SetTimerOption();
                    });
            });
        }

        async void SetTimerOption()
        {
            await AddQuestion("Timer sound?", false);

            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);

            var dingButton = new Button()
            {
                Text = "Ding", ImageSource= "Play_dark_blue.png",
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left,10),
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.FromHex("#e1e1e1"),
                HeightRequest = 50,
                BorderWidth = 0,
                Padding = new Thickness(10, 0),
                WidthRequest = 180,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(25, 2),
                CornerRadius = 25
            };
            dingButton.Clicked += dingSoundClicked;
            stackOptions.Children.Add(dingButton);

            var repsToDoButton = new Button()
            {
                Text = "Reps to do",
                ImageSource = "Play_dark_blue.png",
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10),
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.FromHex("#e1e1e1"),
                HeightRequest = 50,
                BorderWidth = 0,
                Padding = new Thickness(10,0),
                HorizontalOptions = LayoutOptions.Center,
                WidthRequest = 180,
                Margin = new Thickness(25, 2),
                CornerRadius = 25
            };
            repsToDoButton.Clicked += repsToDoSoundClicked;
            stackOptions.Children.Add(repsToDoButton);

            var btn1 = new DrMuscleButton()
            {
                Text = "Ding",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn1.Clicked += dingClicked;
            stackOptions.Children.Add(btn1);

            await AddOptions("Reps to do", repsToDoClicked);
        }
        async void dingSoundClicked(object sender, System.EventArgs e)
        {
            Timer.Instance.NextRepsCount = 0;
            Device.OnPlatform(
                   Android: async () => {
                       
                       var fileName = "alarma.mp3";
                       DependencyService.Get<IAudio>().PlayAudioFile(fileName, true, false);
                   }, iOS: async () => {
                       
                       var message = new PlayAudioFileMessage();
                       MessagingCenter.Send(message, "PlayAudioFileMessage");
                   });
        }
        async void repsToDoSoundClicked(object sender, System.EventArgs e)
        {
            Timer.Instance.NextRepsCount = 5;
            Device.OnPlatform(
                   Android: async () => {

                       var fileName = "alarma.mp3";
                       fileName = $"reps5.mp3";
                          
                       DependencyService.Get<IAudio>().PlayAudioFile(fileName, true, false);
                   }, iOS: async () => {

                       var message = new PlayAudioFileMessage();
                       MessagingCenter.Send(message, "PlayAudioFileMessage");
                   });
        }

        async void dingClicked(object sender, System.EventArgs e)
        {
            isDing = true;
            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);

            SetupQuickMode();

        }

        async void repsToDoClicked(object sender, System.EventArgs e)
        {
            isDing = false;
            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);

            SetupQuickMode();

        }

        async void SetupQuickMode()
        {

            // if (LocalDBManager.Instance.GetDBSetting("NewLevel") != null && LocalDBManager.Instance.GetDBSetting("ExLevel") != null && LocalDBManager.Instance.GetDBSetting("NewLevel").Value == "Streamline" && LocalDBManager.Instance.GetDBSetting("ExLevel").Value == "New")
            // {
            //     LocalDBManager.Instance.SetDBSetting("QuickMode", "false");
            //     ProgramReadyInstruction();
            //     return;
            // }
            LocalDBManager.Instance.SetDBSetting("QuickMode", "false");
            await AddQuestion("Workout length?", false);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            
            var btn1 = new DrMuscleButton()
            {
                Text = "30 min",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn1.Clicked += QuickmodeOnClicked;
            stackOptions.Children.Add(btn1);
            
            var btn2 = new DrMuscleButton()
            {
                Text = "45 min",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn2.Clicked += QuickmodeOnClicked;
            stackOptions.Children.Add(btn2);
            
            var btn3 = new DrMuscleButton()
            {
                Text = "1 hour",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn3.Clicked += QuickmodeOnClicked;
            stackOptions.Children.Add(btn3);
            
            await AddOptions("Flexible (adapts to you)", QuickmodeOffClicked);
        }
        async void QuickmodeOnClicked(object sender, System.EventArgs e)
        {
            if (((DrMuscleButton)sender).Text == "30 min")
            {
                workoutDurationLenggth = 1;
            }
            if (((DrMuscleButton)sender).Text == "45 min")
            {
                workoutDurationLenggth = 2;
            }
            if (((DrMuscleButton)sender).Text == "1 hour")
            {
                workoutDurationLenggth = 3;
            }
            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            LearnMoreTimeline();


        }

        async void LearnMoreTimeline()
        {
            //BotList.Add(new BotModel()
            //{
            //    Question = "Features like smart watch integration and calendar view are not yet available. But if you’re an early adopter who wants to get in shape fast, you'll love your new custom workouts. Give us a shot: we'll treat your feedback like gold. Got a suggestion? Get in touch. We release new features every month.",
            //    Type = BotType.Ques
            //});
            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);

            //await AddQuestion("More features are coming! Got a suggestion? Get in touch. We release new features every week.", false);

            ProgramReadyInstruction();
        }

        async void QuickmodeOffClicked(object sender, System.EventArgs e)
        {
            workoutDurationLenggth = 0;
            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            LearnMoreTimeline();

        }

        async void QuickmodeMediumClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("QuickMode", "null");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            LearnMoreTimeline();

        }

        async void SetupMassUnit()
        {
            ClearOptions();
            IsBodyweightPopup = true;
            await AddQuestion("What is your body weight?");
            await PopupNavigation.Instance.PushAsync(new BodyweightPopup());
            //await AddQuestion(AppResources.DoYouUseLbsOrKgs, false);

            //await AddOptions(AppResources.Lbs, LbsClicked);
            //await AddOptions(AppResources.Kg, KgClicked);


        }

        async void SetupGender()
        {
            ManBetterHealth = false;
            ManLessFat = false;
            ManStorngerSexDrive = false;
            ManMoreMuscle = false;
            FemaleMoreEnergy = false;
            FemaleToned = false;

            await AddOptions(AppResources.Woman, WomanButton_Clicked);
            await AddOptions(AppResources.Man, ManButton_Clicked);


        }

        async void GetAge()
        {
            BotList.Add(new BotModel()
            { Type = BotType.Empty });
            AgePicker.IsVisible = true;
            Device.BeginInvokeOnMainThread(() =>
            {
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            });
            try
            {

                Device.BeginInvokeOnMainThread(() =>
                {
                    AgePicker.Focus();
                });

            }
            catch (Exception ex)
            {

            }
           
        }

        async void LbsClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("massunit", "lb");

            await AddAnswer(((DrMuscleButton)sender).Text);
            TakeBodyWeight();
        }

        async void KgClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("massunit", "kg");

            await AddAnswer(((DrMuscleButton)sender).Text);


            TakeBodyWeight();

        }

        async void GotoLevelUp()
        {


            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                GotoLevelUp();
                return;
            }
            //await AddQuestion($"All right! Your custom program is ready. Learn more?");
            //await AddOptions(AppResources.LearnMore, LearnMoreButton_Clicked);
            //await AddOptions(AppResources.Skip, LearnMoreSkipButton_Clicked);
            LearnMoreSkipButton_Clicked(new DrMuscleButton(), EventArgs.Empty);

        }
        private void GetFirstName()
        {
            StackSignupMenu.IsVisible = false;
            PromptConfig p = new PromptConfig()
            {
                InputType = InputType.Default,
                IsCancellable = false,
                Title = "Your first name",
                Placeholder = "Enter first name",
                OkText = AppResources.Continue,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = new Action<PromptResult>(GetFirstNameAction)
            };
            p.OnTextChanged += Name_OnTextChanged;
            firstnameDisposible = UserDialogs.Instance.Prompt(p);
        }
        private async void GetFirstNameAction(PromptResult response)
        {
            if (response.Ok)
            {
                string text = response.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    text = text.Replace("<", "");
                    text = text.Replace(">", "");
                    text = text.Replace("/", "");
                }
                    if (string.IsNullOrEmpty(text))
                {
                    GetFirstName();
                    return;
                }
                await AddAnswer(text);
                if (Device.RuntimePlatform.Equals(Device.iOS))
                {
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                }
                LocalDBManager.Instance.SetDBSetting("firstname", text);
                LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                CurrentLog.Instance.ShowWelcomePopUp = true;
                ((App)Application.Current).displayCreateNewAccount = false;

                GetPassword();
            }
        }

        private void GetEmail()
        {
            PromptConfig p = new PromptConfig()
            {
                InputType = InputType.Email,
                IsCancellable = false,
                Title = "Your email",
                Placeholder = "Enter your email",
                OkText = AppResources.Continue,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = new Action<PromptResult>(GetEmailAction)
            };
            
            UserDialogs.Instance.Prompt(p);
        }
        private async void GetEmailAction(PromptResult response)
        {

            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                //{
                //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                //    Message = AppResources.PleaseCheckInternetConnection,
                //    Title = AppResources.ConnectionError
                //});
                GetEmailAction(response);
                return;
            }
            if (response.Ok)
            {
                var text = response.Text;
                if (!string.IsNullOrEmpty(response.Text) && text.Contains("@"))
                {
                    var email = response.Text.Substring(0, response.Text.IndexOf('@'));
                    if (email.Length == 1)
                        text = $"a{text}";
                }
                if (!Emails.ValidateEmail(text))
                {

                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        Message = AppResources.InvalidEmailError,
                        Title = AppResources.InvalidEmailAddress,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });
                    GetEmail();
                    return;
                }
                if (response.Text.Contains("#") || response.Text.Contains("%") || response.Text.Contains("{") || response.Text.Contains("}") || response.Text.Contains("(") || response.Text.Contains("}") || response.Text.Contains("$") || response.Text.Contains("^") || response.Text.Contains("&") || response.Text.Contains("=") || response.Text.Contains("`") || response.Text.Contains("'") || response.Text.Contains("\"") || response.Text.Contains(",") || response.Text.Contains("?") || response.Text.Contains("/") || response.Text.Contains("\\") || response.Text.Contains("<") || response.Text.Contains(">") || response.Text.Contains(":") || response.Text.Contains(";") || response.Text.Contains("|") || response.Text.Contains("[") || response.Text.Contains("]") || response.Text.Contains("*") || response.Text.Contains("*") || response.Text.Contains("!") || response.Text.Contains("~") || response.Text.Count(t => t == '@') > 1)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        Message = AppResources.InvalidEmailError,
                        Title = AppResources.InvalidEmailAddress,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });
                    GetEmail();
                    return;
                }
                
                try
                {
                    var domain = response.Text.Substring(response.Text.IndexOf('@'));
                    var extension = response.Text.Substring(response.Text.IndexOf('.')+1).ToUpper();
                    if (domain.Contains("gnail") || domain.Contains("gmaill") || domain.Contains(".cam"))
                    {
                        await UserDialogs.Instance.AlertAsync(new AlertConfig()
                        {
                            Message = AppResources.InvalidEmailError,
                            Title = AppResources.InvalidEmailAddress,
                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                        });
                        GetEmail();
                        return;
                    }
                }
                catch (Exception ex)
                {

                }
                
                checkEmailExist(response.Text);
                //GetEmailAction(response);

                App.IsNewUser = true;

                LocalDBManager.Instance.SetDBSetting("email", response.Text);
                await AddAnswer(response.Text);
                await AddQuestion("Enter first name");
                if (Device.RuntimePlatform.Equals(Device.iOS))
                {
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                }

                GetFirstName();
            }
        }

        private async void checkEmailExist(string email)
        {
            BooleanModel existingUser = await DrMuscleRestClient.Instance.IsEmailAlreadyExistWithoutLoader(new IsEmailAlreadyExistModel() { email = email });
            if (existingUser != null)
            {
                if (existingUser.Result)
                {
                    try
                    {
                        if (firstnameDisposible != null)
                            firstnameDisposible.Dispose();
                        if (passwordDisposible != null)
                            passwordDisposible.Dispose();
                        
                    }
                    catch (Exception ex)
                    {

                    } finally
                    {
                        await Task.Delay(500);
                    }
                    
                    ConfirmConfig ShowAlertPopUp = new ConfirmConfig()
                    {
                        Title = "Email already in use",
                        Message = "Use another email or log into your existing account.",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = "Use another email",
                        CancelText = AppResources.LogIn,

                    };
                    var actionOk = await UserDialogs.Instance.ConfirmAsync(ShowAlertPopUp);
                    if (actionOk)
                    {
                        GetEmail();
                    }
                    else
                    {
                        ((App)Application.Current).displayCreateNewAccount = true;
                        await PagesFactory.PushAsync<WelcomePage>();
                    }

                    return;
                }

            }
            

        }
        private void GetPassword()
        {
            PromptConfig p = new PromptConfig()
            {
                InputType = InputType.Password,
                IsCancellable = false,
                Title = "Create password",
                Message = "At least 6 characters",
                Placeholder = "Create password",
                OkText = AppResources.Create,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = new Action<PromptResult>(GetPasswordAction)
            };

            passwordDisposible = UserDialogs.Instance.Prompt(p);
        }
        private async void GetPasswordAction(PromptResult response)
        {
            if (response.Ok)
            {

                if (response.Text.Length < 6)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        Message = AppResources.PasswordLengthError,
                        Title = AppResources.Error,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });
                    GetPassword();
                    return;
                }
                await AddAnswer(response.Text);
                LocalDBManager.Instance.SetDBSetting("password", response.Text);
                await Task.Delay(100);
                //CreateAccountBeforeDemoButton_Clicked();
                CreateAccountBeforeDemoButton_Clicked();
                //await AccountCreatedPopup();
                SetUpRestOnboarding();
            }

        }
        async void CreateAccountButton_Clicked()
        {
            DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
            DBSetting workoutPlaceSetting = LocalDBManager.Instance.GetDBSetting("workout_place");
            int? workoutId = null;
            int? programId = null;
            int? remainingWorkout = null;
            var WorkoutInfo2 = "";
            

                string ProgramLabel = AppResources.NotSetUp;
                int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            

            var level = 0;
            if(LocalDBManager.Instance.GetDBSetting("MainLevel") != null)
                level = int.Parse(LocalDBManager.Instance.GetDBSetting("MainLevel").Value);
            if (LocalDBManager.Instance.GetDBSetting("ExerciseVariety")?.Value == "More")
                level += 1;
            bool isSplit = LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Split");
                bool isGym = workoutPlaceSetting?.Value == "gym";
                var mo = AppThemeConstants.GetLevelProgram(level,isGym,!isSplit, LocalDBManager.Instance.GetDBSetting("MainProgram").Value);

            if (workoutPlaceSetting?.Value == "homeBodyweightOnly")
            {
                if (LocalDBManager.Instance.GetDBSetting("CustomMainLevel") != null && LocalDBManager.Instance.GetDBSetting("CustomMainLevel")?.Value == "1" && LocalDBManager.Instance.GetDBSetting("ExerciseVariety")?.Value == "Less")
                {
                    mo.workoutName = "Bodyweight 1";
                    mo.workoutid = 12645;
                    mo.programid = 487;
                    mo.reqWorkout = 12;
                    mo.programName = "Bodyweight Level 1";
                }
                else if (level <= 1)
                {
                    mo.workoutName = "Bodyweight 2";
                    mo.workoutid = 12646;
                    mo.programid = 488;
                    mo.reqWorkout = 12;
                    mo.programName = "Bodyweight Level 2";
                }
                else if (level == 2)
                {
                    mo.workoutName = "Bodyweight 2";
                    mo.workoutid = 12646;
                    mo.programid = 488;
                    mo.reqWorkout = 12;
                    mo.programName = "Bodyweight Level 2";
                }
                else if (level == 3)
                {
                    mo.workoutName = "Bodyweight 3";
                    mo.workoutid = 14017;
                    mo.programid = 923;
                    mo.reqWorkout = 15;
                    mo.programName = "Bodyweight Level 3";
                }
                else if (level >= 4)
                {
                    mo.workoutName = "Bodyweight 4";
                    mo.workoutid = 14019;
                    mo.programid = 924;
                    mo.reqWorkout = 15;
                    mo.programName = "Bodyweight Level 4";
                }

            }


            LocalDBManager.Instance.SetDBSetting("recommendedWorkoutId", mo.workoutid.ToString());
            LocalDBManager.Instance.SetDBSetting("recommendedWorkoutLabel", mo.workoutName);
            LocalDBManager.Instance.SetDBSetting("recommendedProgramId", mo.programid.ToString());
            LocalDBManager.Instance.SetDBSetting("recommendedRemainingWorkout", mo.reqWorkout.ToString());

            LocalDBManager.Instance.SetDBSetting("recommendedProgramLabel", mo.programName);
            //}
            //SignUp here

            workoutId = mo.workoutid;
            WorkoutInfo2 = mo.workoutName;
            programId = mo.programid;
            ProgramLabel = mo.programName;
            remainingWorkout = mo.reqWorkout;


            RegisterModel registerModel = new RegisterModel();

            registerModel.Firstname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
            registerModel.EmailAddress = LocalDBManager.Instance.GetDBSetting("email").Value;
            registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
            registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
            if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                registerModel.IsQuickMode = false;
            else
            {
                if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
                    registerModel.IsQuickMode = null;
                else
                    registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
            }
            if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
            registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
            if (LocalDBManager.Instance.GetDBSetting("WeightGoal") != null)
                registerModel.WeightGoal = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("WeightGoal").Value, CultureInfo.InvariantCulture), "kg");

            registerModel.Password = LocalDBManager.Instance.GetDBSetting("password").Value;
            registerModel.ConfirmPassword = LocalDBManager.Instance.GetDBSetting("password").Value;
            registerModel.LearnMoreDetails = learnMore;
            registerModel.IsHumanSupport = IsHumanSupport;
            registerModel.IsCardio = IsIncludeCardio;
            registerModel.BodyPartPrioriy = bodypartName;
            registerModel.SetStyle = SetStyle;
            registerModel.IsPyramid = IsPyramid;
            registerModel.isDing = isDing;
            registerModel.WorkoutDuration = workoutDurationLenggth;
            if (LocalDBManager.Instance.GetDBSetting("Height")?.Value != null && Config.UserHeight != 0)
            {
                registerModel.Height = Config.UserHeight;
            }
            if (LocalDBManager.Instance.GetDBSetting("email") != null && LocalDBManager.Instance.GetDBSetting("email").Value.Contains("yopmail") || (LocalDBManager.Instance.GetDBSetting("Environment") != null && LocalDBManager.Instance.GetDBSetting("Environment").Value != "Production"))
            { registerModel.MainGoal = ""; }
            else
                registerModel.MainGoal = isBetaExperience == true ? "Beta" : isBetaExperience == false ? "Normal" : "";

            if (LocalDBManager.Instance.GetDBSetting("email") != null && LocalDBManager.Instance.GetDBSetting("email").Value.Contains("yopmail") || (LocalDBManager.Instance.GetDBSetting("Environment") != null && LocalDBManager.Instance.GetDBSetting("Environment").Value != "Production"))
            { registerModel.MainGoal = ""; }
            else
                registerModel.MainGoal = isBetaExperience == true ? "Beta" : isBetaExperience == false ? "Normal" : "";

            if (LocalDBManager.Instance.GetDBSetting("ReminderDays") != null && LocalDBManager.Instance.GetDBSetting("ReminderTime") != null)
            {
                try
                {
                    registerModel.ReminderTime = TimeSpan.Parse(LocalDBManager.Instance.GetDBSetting("ReminderTime").Value);
                    registerModel.ReminderDays = LocalDBManager.Instance.GetDBSetting("ReminderDays")?.Value;
                }
                catch (Exception ex)
                {

                }
                
            }
            if (IsEquipment)
            {
                var model = new EquipmentModel();

                if (LocalDBManager.Instance.GetDBSetting("workout_place")?.Value == "gym")
                {
                    model.IsEquipmentEnabled = true;
                    model.IsDumbbellEnabled = isDumbbells;
                    model.IsPlateEnabled = IsPlates;
                    model.IsPullyEnabled = IsPully;
                    model.IsChinUpBarEnabled = IsChinupBar;
                    model.Active = "gym";

                    model.IsHomeEquipmentEnabled = false;
                    model.IsHomeDumbbell = true;
                    model.IsHomePlate = true;
                    model.IsHomePully = true;
                    model.IsHomeChinupBar = true;
                    
                }
                else
                {
                    model.IsEquipmentEnabled = false;
                    model.IsDumbbellEnabled = true;
                    model.IsPlateEnabled = true;
                    model.IsPullyEnabled = true;
                    model.IsChinUpBarEnabled = true;

                    model.IsOtherEquipmentEnabled = false;
                    model.IsOtherDumbbell = true;
                    model.IsOtherPlate = true;
                    model.IsOtherPully = true;
                    model.IsOtherChinupBar = true;

                    model.IsHomeEquipmentEnabled = true;
                    model.IsHomeDumbbell = isDumbbells;
                    model.IsHomePlate = IsPlates;
                    model.IsHomePully = IsPully;
                    model.IsHomeChinupBar = IsChinupBar;
                    model.Active = "home";
                }
                var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
                model.AvilablePlate = kgString;
                model.AvilableHomePlate = kgString;
                model.AvilableOtherPlate = kgString;
                var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
                model.AvilableLbPlate = lbString;
                model.AvilableHomeLbPlate = lbString;
                model.AvilableOtherLbPlate = lbString;
             

                registerModel.EquipmentModel = model;
            }
            if (IncrementUnit != null)
            {
                registerModel.Increments = IncrementUnit.Kg;
            }
            try
            {


                BooleanModel registerResponse = await DrMuscleRestClient.Instance.RegisterUser(registerModel);
                if (registerResponse.Result)
                {
                    DependencyService.Get<IFirebase>().LogEvent("account_created", "");
                }
            }
            catch (Exception ex)
            {

            }
            //Login
            LoginSuccessResult lr = await DrMuscleRestClient.Instance.Login(new LoginModel()
            {
                Username = registerModel.EmailAddress,
                Password = registerModel.Password
            });

            if (lr != null && !string.IsNullOrEmpty(lr.access_token))
            {
                DateTime current = DateTime.Now;

                UserInfosModel uim = await DrMuscleRestClient.Instance.GetUserInfo();

                LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                LocalDBManager.Instance.SetDBSetting("password", registerModel.Password);
                LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                LocalDBManager.Instance.SetDBSetting("token_expires_date", current.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());

                LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                LocalDBManager.Instance.SetDBSetting("reprangeType", uim.ReprangeType.ToString());
                LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                
                LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_123_sound", uim.IsTimer321 ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_reps_sound", uim.IsRepsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); if (uim.Age != null)
                    LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));

                if (uim.TargetIntake != null && uim.TargetIntake != 0)
                    LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
                SetupEquipment(uim);
                if (uim.IsPyramid)
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                    LocalDBManager.Instance.SetDBSetting("IsRPyramid", "true");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
                }
                else if (uim.IsNormalSet == null || uim.IsNormalSet == true)
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "Normal");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", uim.IsNormalSet == null ? "true" : "false");
                }
                else
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
                }
                if (uim.WarmupsValue != null)
                {
                    LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
                }
                if (uim.Increments != null)
                    LocalDBManager.Instance.SetDBSetting("workout_increments", uim.Increments.Kg.ToString().ReplaceWithDot());
                if (uim.Max != null)
                    LocalDBManager.Instance.SetDBSetting("workout_max", uim.Max.Kg.ToString().ReplaceWithDot());
                if (uim.Min != null)
                    LocalDBManager.Instance.SetDBSetting("workout_min", uim.Min.Kg.ToString().ReplaceWithDot());
                if (uim.BodyWeight != null)
                {
                    LocalDBManager.Instance.SetDBSetting("BodyWeight", uim.BodyWeight.Kg.ToString().ReplaceWithDot());
                }
                if (uim.WeightGoal != null)
                {
                    LocalDBManager.Instance.SetDBSetting("WeightGoal", uim.WeightGoal.Kg.ToString().ReplaceWithDot());
                }
                //if (uim.EquipmentModel != null)
                //{
                //    LocalDBManager.Instance.SetDBSetting("Equipment", uim.EquipmentModel.IsEquipmentEnabled ? "true" : "false");
                //    LocalDBManager.Instance.SetDBSetting("ChinUp", uim.EquipmentModel.IsChinUpBarEnabled ? "true" : "false");
                //    LocalDBManager.Instance.SetDBSetting("Dumbbell", uim.EquipmentModel.IsDumbbellEnabled ? "true" : "false");
                //    LocalDBManager.Instance.SetDBSetting("Plate", uim.EquipmentModel.IsPlateEnabled ? "true" : "false");
                //    LocalDBManager.Instance.SetDBSetting("Pully", uim.EquipmentModel.IsPullyEnabled ? "true" : "false");
                //}
                //else
                //{
                //    LocalDBManager.Instance.SetDBSetting("Equipment", "false");
                //    LocalDBManager.Instance.SetDBSetting("ChinUp", "true");
                //    LocalDBManager.Instance.SetDBSetting("Dumbbell", "true");
                //    LocalDBManager.Instance.SetDBSetting("Plate", "true");
                //    LocalDBManager.Instance.SetDBSetting("Pully", "true");
                //}
                SetupEquipment(uim);

                if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                else
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());

                LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");

                LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("1By1Side", uim.Is1By1Side ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");
                ((App)Application.Current).displayCreateNewAccount = true;

                if (uim.Gender.Trim().ToLowerInvariant().Equals("man"))
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "Background2.png");
                else
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "BackgroundFemale.png");
                App.IsNewFirstTime = false;
                if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
                        LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
                        LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
                        LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
                        LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
                {
                    try
                    {
                        long workoutTemplateId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId").Value);
                        long pId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedProgramId").Value);
                        var upi = new GetUserProgramInfoResponseModel()
                        {
                            NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value },
                            RecommendedProgram = new WorkoutTemplateGroupModel() { Id = pId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
                        };
                        if (upi != null)
                        {
                            WorkoutTemplateModel nextWorkout = upi.NextWorkoutTemplate;
                            if (upi.NextWorkoutTemplate.Exercises == null || upi.NextWorkoutTemplate.Exercises.Count() == 0)
                            {
                                try
                                {
                                    nextWorkout = await DrMuscleRestClient.Instance.GetUserCustomizedCurrentWorkout(workoutTemplateId);
                                }
                                catch (Exception ex)
                                {
                                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                                    {
                                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                        Message = AppResources.PleaseCheckInternetConnection,
                                        Title = AppResources.ConnectionError
                                    });
                                    
                                    return;
                                }

                            }
                            App.IsNUX = false;
                            if (nextWorkout != null)
                            {
                                CurrentLog.Instance.CurrentWorkoutTemplate = nextWorkout;
                                CurrentLog.Instance.WorkoutTemplateCurrentExercise = nextWorkout.Exercises.First();
                                CurrentLog.Instance.WorkoutStarted = true;
                                if (Device.RuntimePlatform.Equals(Device.Android))
                                {
                                    App.IsDemoProgress = false;
                                    App.IsWelcomeBack = true;
                                    App.IsNewUser = true;
                                    LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                                    CurrentLog.Instance.Exercise1RM.Clear();
                                    //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                                    Device.BeginInvokeOnMainThread(async () =>
                                    {
                                        await PagesFactory.PopToRootAsync(true);
                                    });
                                    MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
                                }
                                else
                                {

                                    App.IsDemoProgress = false;
                                    App.IsWelcomeBack = true;
                                    App.IsNewUser = true;
                                    LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                                    CurrentLog.Instance.Exercise1RM.Clear();
                                    //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                                    await PagesFactory.PopToRootMoveAsync(true);
                                    MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
                                }

                            }
                            else
                            {
                                await PagesFactory.PopToRootAsync(true);
                                App.IsDemoProgress = false;
                                App.IsWelcomeBack = true;
                                App.IsNewUser = true;
                                LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                                
                            }

                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }

                try
                {
                    OpenPreview(registerModel);
                }
                catch (Exception ex)
                {

                }




            }
            else
            {
                UserDialogs.Instance.Alert(new AlertConfig()
                {
                    Message = AppResources.EmailAndPasswordDoNotMatch,
                    Title = AppResources.UnableToLogIn,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
        }
        async void CreateAccountBeforeDemoButton_Clicked()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError
                });
                return;
            }
            LocalDBManager.Instance.SetDBSetting("LoginType", "Email");

            int? workoutId = null;
            int? programId = null;
            int? remainingWorkout = null;
            var WorkoutInfo2 = "";
            //Setup Program

            //SignUp here
            RegisterModel registerModel = new RegisterModel();

            registerModel.Firstname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
            registerModel.EmailAddress = LocalDBManager.Instance.GetDBSetting("email").Value;
            registerModel.MassUnit = "lb";
            registerModel.BodyWeight = new MultiUnityWeight(150, "lb");
            registerModel.Password = LocalDBManager.Instance.GetDBSetting("password").Value;
            registerModel.ConfirmPassword = LocalDBManager.Instance.GetDBSetting("password").Value;

            try
            {

                BooleanModel registerResponse = await DrMuscleRestClient.Instance.RegisterUserBeforeDemo(registerModel);
                if (registerResponse.Result)
                {
                    DependencyService.Get<IFirebase>().LogEvent("account_created", "");
                }
            }
            catch (Exception ex)
            {

            }
            //Login
            LoginSuccessResult lr = await DrMuscleRestClient.Instance.LoginWithoutLoader(new LoginModel()
            {
                Username = registerModel.EmailAddress,
                Password = registerModel.Password
            });

            if (lr != null && !string.IsNullOrEmpty(lr.access_token))
            {
                DateTime current = DateTime.Now;

                UserInfosModel uim = await DrMuscleRestClient.Instance.GetUserInfoWithoutLoader();

                LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                //LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                LocalDBManager.Instance.SetDBSetting("password", registerModel.Password);
                LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                LocalDBManager.Instance.SetDBSetting("token_expires_date", current.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                LocalDBManager.Instance.SetDBSetting("reprangeType", uim.ReprangeType.ToString());
                LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_123_sound", uim.IsTimer321 ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_reps_sound", uim.IsRepsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false");                //if (uim.ReminderTime != null)

                if (uim.WarmupsValue != null)
                {
                    LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
                }
                if (uim.Increments != null)
                    LocalDBManager.Instance.SetDBSetting("workout_increments", uim.Increments.Kg.ToString().ReplaceWithDot());
                if (uim.Max != null)
                    LocalDBManager.Instance.SetDBSetting("workout_max", uim.Max.Kg.ToString().ReplaceWithDot());
                if (uim.Min != null)
                    LocalDBManager.Instance.SetDBSetting("workout_min", uim.Min.Kg.ToString().ReplaceWithDot());
                if (uim.BodyWeight != null)
                {
                    LocalDBManager.Instance.SetDBSetting("BodyWeight", uim.BodyWeight.Kg.ToString().ReplaceWithDot());
                }
                if (uim.WeightGoal != null)
                {
                    LocalDBManager.Instance.SetDBSetting("WeightGoal", uim.WeightGoal.Kg.ToString().ReplaceWithDot());
                }
                LocalDBManager.Instance.SetDBSetting("FirstStepCompleted","true");
                //await AccountCreatedPopup();
                //SetUpRestOnboarding();
            }
            else
            {
                UserDialogs.Instance.Alert(new AlertConfig()
                {
                    Message = AppResources.EmailAndPasswordDoNotMatch,
                    Title = AppResources.UnableToLogIn,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
        }

        private async Task AccountCreatedPopup()
        {
            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            var modalPage = new Views.GeneralPopup("TrueState.png", "Success!", "Account created", "Customize program");
            modalPage.Disappearing += (sender2, e2) =>
            {
                waitHandle.Set();
            };
            await PopupNavigation.Instance.PushAsync(modalPage);

            await Task.Run(() => waitHandle.WaitOne());

        }

        private void SetupEquipment(UserInfosModel uim)
        {
            LocalDBManager.Instance.SetDBSetting("KgBarWeight", uim.KgBarWeight == null ? "20" : Convert.ToString(uim.KgBarWeight).ReplaceWithDot());
            LocalDBManager.Instance.SetDBSetting("LBBarWeight", uim.LbBarWeight == null ? "45" : Convert.ToString(uim.LbBarWeight).ReplaceWithDot());

            if (uim.EquipmentModel != null)
            {
                LocalDBManager.Instance.SetDBSetting("Equipment", uim.EquipmentModel.IsEquipmentEnabled ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("ChinUp", uim.EquipmentModel.IsChinUpBarEnabled ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("Dumbbell", uim.EquipmentModel.IsDumbbellEnabled ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("Plate", uim.EquipmentModel.IsPlateEnabled ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("Pully", uim.EquipmentModel.IsPullyEnabled ? "true" : "false");

                LocalDBManager.Instance.SetDBSetting("HomeMainEquipment", uim.EquipmentModel.IsHomeEquipmentEnabled ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("HomeChinUp", uim.EquipmentModel.IsHomeChinupBar ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("HomeDumbbell", uim.EquipmentModel.IsHomeDumbbell ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("HomePlate", uim.EquipmentModel.IsHomePlate ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("HomePully", uim.EquipmentModel.IsHomePully ? "true" : "false");


                LocalDBManager.Instance.SetDBSetting("OtherMainEquipment", uim.EquipmentModel.IsOtherEquipmentEnabled ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("OtherChinUp", uim.EquipmentModel.IsOtherChinupBar ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("OtherDumbbell", uim.EquipmentModel.IsOtherDumbbell ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("OtherPlate", uim.EquipmentModel.IsOtherPlate ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("OtherPully", uim.EquipmentModel.IsOtherPully ? "true" : "false");

               
                if (uim.EquipmentModel.Active == "gym")
                    LocalDBManager.Instance.SetDBSetting("GymEquipment", "true");
                if (uim.EquipmentModel.Active == "home")
                    LocalDBManager.Instance.SetDBSetting("HomeEquipment", "true");
                if (uim.EquipmentModel.Active == "other")
                    LocalDBManager.Instance.SetDBSetting("OtherEquipment", "true");
                if (!string.IsNullOrEmpty(uim.EquipmentModel.AvilablePlate))
                {
                    LocalDBManager.Instance.SetDBSetting("PlatesKg", uim.EquipmentModel.AvilablePlate);
                    LocalDBManager.Instance.SetDBSetting("HomePlatesKg", uim.EquipmentModel.AvilableHomePlate);
                    LocalDBManager.Instance.SetDBSetting("OtherPlatesKg", uim.EquipmentModel.AvilableOtherPlate);

                    LocalDBManager.Instance.SetDBSetting("PlatesLb", uim.EquipmentModel.AvilableLbPlate);
                    LocalDBManager.Instance.SetDBSetting("HomePlatesLb", uim.EquipmentModel.AvilableHomeLbPlate);
                    LocalDBManager.Instance.SetDBSetting("OtherPlatesLb", uim.EquipmentModel.AvilableHomeLbPlate);
                }
                else
                {
                    var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
                    LocalDBManager.Instance.SetDBSetting("PlatesKg", kgString);
                    LocalDBManager.Instance.SetDBSetting("HomePlatesKg", kgString);
                    LocalDBManager.Instance.SetDBSetting("OtherPlatesKg", kgString);

                    var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
                    LocalDBManager.Instance.SetDBSetting("PlatesLb", lbString);
                    LocalDBManager.Instance.SetDBSetting("HomePlatesLb", lbString);
                    LocalDBManager.Instance.SetDBSetting("OtherPlatesLb", lbString);
                }
                if (!string.IsNullOrEmpty(uim.EquipmentModel.AvilableDumbbell))
                {
                    LocalDBManager.Instance.SetDBSetting("DumbbellKg", uim.EquipmentModel.AvilableDumbbell);
                    LocalDBManager.Instance.SetDBSetting("HomeDumbbellKg", uim.EquipmentModel.AvilableHomeDumbbell);
                    LocalDBManager.Instance.SetDBSetting("OtherDumbbellKg", uim.EquipmentModel.AvilableOtherDumbbell);

                    LocalDBManager.Instance.SetDBSetting("DumbbellLb", uim.EquipmentModel.AvilableLbDumbbell);
                    LocalDBManager.Instance.SetDBSetting("HomeDumbbellLb", uim.EquipmentModel.AvilableHomeLbDumbbell);
                    LocalDBManager.Instance.SetDBSetting("OtherDumbbellLb", uim.EquipmentModel.AvilableHomeLbDumbbell);
                }
                else
                {
                    var kgString = "50_2_True|47.5_2_True|45_2_True|42.5_2_True|40_2_True|37.5_2_True|35_2_True|32.5_2_True|30_2_True|27.5_2_True|25_2_True|22.5_2_True|20_2_True|17.5_2_True|15_2_True|12.5_2_True|10_2_True|7.5_2_True|5_2_True|2.5_2_True|1_2_True";
                    LocalDBManager.Instance.SetDBSetting("DumbbellKg", kgString);
                    LocalDBManager.Instance.SetDBSetting("HomeDumbbellKg", kgString);
                    LocalDBManager.Instance.SetDBSetting("OtherDumbbellKg", kgString);

                    var lbString = "90_2_True|85_2_True|80_2_True|75_2_True|70_2_True|65_2_True|60_2_True|55_2_True|50_2_True|45_2_True|40_2_True|35_2_True|30_2_True|25_2_True|20_2_True|15_2_True|12_2_True|10_2_True|8_2_True|5_2_True|3_2_True|2_2_True";
                    LocalDBManager.Instance.SetDBSetting("DumbbellLb", lbString);
                    LocalDBManager.Instance.SetDBSetting("HomeDumbbellLb", lbString);
                    LocalDBManager.Instance.SetDBSetting("OtherDumbbellLb", lbString);
                }

                if (!string.IsNullOrEmpty(uim.EquipmentModel.AvilablePulley))
                {
                    
                    LocalDBManager.Instance.SetDBSetting("PulleyKg", uim.EquipmentModel.AvilablePulley);
                    LocalDBManager.Instance.SetDBSetting("HomePulleyKg", uim.EquipmentModel.AvilableHomePulley);
                    LocalDBManager.Instance.SetDBSetting("OtherPulleyKg", uim.EquipmentModel.AvilableOtherPulley);

                    
                    LocalDBManager.Instance.SetDBSetting("PulleyLb", uim.EquipmentModel.AvilableLbPulley);
                    LocalDBManager.Instance.SetDBSetting("HomePulleyLb", uim.EquipmentModel.AvilableHomeLbPulley);
                    LocalDBManager.Instance.SetDBSetting("OtherPulleyLb", uim.EquipmentModel.AvilableOtherLbPulley);
                }
                else
                {
                    var kgString = "5_20_True|1.5_2_True";
                    var lbString = "10_20_True|5_2_True|2.5_2_True";
                    LocalDBManager.Instance.SetDBSetting("PulleyKg", kgString);
                    LocalDBManager.Instance.SetDBSetting("HomePulleyKg", kgString);
                    LocalDBManager.Instance.SetDBSetting("OtherPulleyKg", kgString);

                    
                    LocalDBManager.Instance.SetDBSetting("PulleyLb", lbString);
                    LocalDBManager.Instance.SetDBSetting("HomePulleyLb", lbString);
                    LocalDBManager.Instance.SetDBSetting("OtherPulleyLb", lbString);
                }

            }
            else
            {
                LocalDBManager.Instance.SetDBSetting("Equipment", "false");
                LocalDBManager.Instance.SetDBSetting("ChinUp", "true");
                LocalDBManager.Instance.SetDBSetting("Dumbbell", "true");
                LocalDBManager.Instance.SetDBSetting("Plate", "true");
                LocalDBManager.Instance.SetDBSetting("Pully", "true");

                LocalDBManager.Instance.SetDBSetting("HomeMainEquipment", "false");
                LocalDBManager.Instance.SetDBSetting("HomeChinUp", "true");
                LocalDBManager.Instance.SetDBSetting("HomeDumbbell", "true");
                LocalDBManager.Instance.SetDBSetting("HomePlate", "true");
                LocalDBManager.Instance.SetDBSetting("HomePully", "true");

                LocalDBManager.Instance.SetDBSetting("OtherEquipment", "false");
                LocalDBManager.Instance.SetDBSetting("OtherChinUp", "true");
                LocalDBManager.Instance.SetDBSetting("OtherDumbbell", "true");
                LocalDBManager.Instance.SetDBSetting("OtherPlate", "true");
                LocalDBManager.Instance.SetDBSetting("OtherPully", "true");

               
            }
        }

        async void CreateAccountAfterDemoButton_Clicked()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {

                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }
            if (isProcessing)
                return;
            isProcessing = true;
            //LocalDBManager.Instance.SetDBSetting("ExerciseVariety", "Less");
            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
            DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
            DBSetting workoutPlaceSetting = LocalDBManager.Instance.GetDBSetting("workout_place");
            int? workoutId = null;
            int? programId = null;
            int? remainingWorkout = null;
            var WorkoutInfo2 = "";
            
            

            string ProgramLabel = AppResources.NotSetUp;
            int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);


            var level = 0;
            if (LocalDBManager.Instance.GetDBSetting("MainLevel") != null)
                level = int.Parse(LocalDBManager.Instance.GetDBSetting("MainLevel").Value);
            if (LocalDBManager.Instance.GetDBSetting("ExerciseVariety")?.Value == "More")
                level += 1;
            bool isSplit = LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Split");
            bool isGym = workoutPlaceSetting?.Value == "gym";
            var mo = AppThemeConstants.GetLevelProgram(level, isGym, !isSplit, LocalDBManager.Instance.GetDBSetting("MainProgram").Value);

            if (workoutPlaceSetting?.Value == "homeBodyweightOnly")
            {
                
                
                if (level <= 1)
                {
                    mo.workoutName = "Bodyweight 1";
                    mo.workoutid = 12645;
                    mo.programid = 487;
                    mo.reqWorkout = 12;
                    mo.programName = "Bodyweight Level 1";
                }
                else if (level <= 2 )
                {
                    mo.workoutName = "Bodyweight 2";
                    mo.workoutid = 12646;
                    mo.programid = 488;
                    mo.reqWorkout = 12;
                    mo.programName = "Bodyweight Level 2";
                }
                
                else if (level == 3)
                {
                    mo.workoutName = "Bodyweight 3";
                    mo.workoutid = 14017;
                    mo.programid = 923;
                    mo.reqWorkout = 15;
                    mo.programName = "Bodyweight Level 3";
                }
                else if (level >= 4)
                {
                    mo.workoutName = "Bodyweight 4";
                    mo.workoutid = 14019;
                    mo.programid = 924;
                    mo.reqWorkout = 15;
                    mo.programName = "Bodyweight Level 4";
                }

            }
            else if (workoutPlaceSetting.Value == "homeBodyweightBandsOnly")
            {
                mo.workoutName = "[Home] Buffed w/ Bands";
                mo.programName = "[Home] Buffed w/ Bands Level 1";
                mo.workoutid = 15377;
                mo.programid = 1339;
                mo.reqWorkout = 15;

                if (LocalDBManager.Instance.GetDBSetting("CustomExperience")?.Value == "an active, experienced lifter" && LocalDBManager.Instance.GetDBSetting("ExerciseVariety")?.Value == "Less")
                {
                    mo.workoutName = "[Home] Buffed w/ Bands 2A";
                    mo.programName = "[Home] Buffed w/ Bands Level 2";
                    mo.workoutid = 15375;
                    mo.programid = 1338;
                    mo.reqWorkout = 18;
                }
                else if (LocalDBManager.Instance.GetDBSetting("CustomExperience")?.Value == "an active, experienced lifter" && LocalDBManager.Instance.GetDBSetting("ExerciseVariety")?.Value == "More")
                {
                    mo.workoutName = "[Home] Buffed w/ Bands 3A";
                    mo.programName = "[Home] Buffed w/ Bands Level 3";
                    mo.workoutid = 17321;
                    mo.programid = 2032;
                    mo.reqWorkout = 24;
                }
            }
            
            LocalDBManager.Instance.SetDBSetting("recommendedWorkoutId", mo.workoutid.ToString());
            LocalDBManager.Instance.SetDBSetting("recommendedWorkoutLabel", mo.workoutName);
            LocalDBManager.Instance.SetDBSetting("recommendedProgramId", mo.programid.ToString());
            LocalDBManager.Instance.SetDBSetting("recommendedRemainingWorkout", mo.reqWorkout.ToString());

            LocalDBManager.Instance.SetDBSetting("recommendedProgramLabel", mo.programName);
            //}
            //SignUp here

            workoutId = mo.workoutid;
            WorkoutInfo2 = mo.workoutName;
            programId = mo.programid;
            ProgramLabel = mo.programName;
            remainingWorkout = mo.reqWorkout;

            RegisterModel registerModel = new RegisterModel();

            registerModel.Firstname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
            registerModel.EmailAddress = LocalDBManager.Instance.GetDBSetting("email").Value;
            registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
            registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
            registerModel.IsMobility = IsIncludeMobility;
            registerModel.MobilityLevel = mobilityLevel;
            registerModel.IsReminderEmail = true;
            registerModel.TimeBeforeWorkout = 5;
            if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                registerModel.IsQuickMode = false;
            else
            {
                if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
                    registerModel.IsQuickMode = null;
                else
                    registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
            }
            if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
            registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
            if (LocalDBManager.Instance.GetDBSetting("WeightGoal") != null)
                registerModel.WeightGoal = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("WeightGoal").Value, CultureInfo.InvariantCulture), "kg");

            registerModel.Password = LocalDBManager.Instance.GetDBSetting("password").Value;
            registerModel.ConfirmPassword = LocalDBManager.Instance.GetDBSetting("password").Value;
            registerModel.LearnMoreDetails = learnMore;
            registerModel.IsHumanSupport = IsHumanSupport;
            registerModel.IsCardio = IsIncludeCardio;
            registerModel.BodyPartPrioriy = bodypartName;
            registerModel.SetStyle = SetStyle;
            registerModel.IsPyramid = IsPyramid;
            registerModel.isDing = isDing;
            registerModel.WorkoutDuration = workoutDurationLenggth;
            if (LocalDBManager.Instance.GetDBSetting("Height")?.Value != null && Config.UserHeight != 0)
            {
                registerModel.Height = Config.UserHeight;
            }
            if (LocalDBManager.Instance.GetDBSetting("email") != null && LocalDBManager.Instance.GetDBSetting("email").Value.Contains("yopmail") || (LocalDBManager.Instance.GetDBSetting("Environment") != null && LocalDBManager.Instance.GetDBSetting("Environment").Value != "Production"))
            { registerModel.MainGoal = ""; }
            else
                registerModel.MainGoal = isBetaExperience == true ? "Beta" : isBetaExperience == false ? "Normal" : "";
            registerModel.ProgramId = programId;
            if (LocalDBManager.Instance.GetDBSetting("ReminderDays") != null && LocalDBManager.Instance.GetDBSetting("ReminderTime") != null)
            {
                try
                {
                    registerModel.ReminderTime = TimeSpan.Parse(LocalDBManager.Instance.GetDBSetting("ReminderTime").Value);
                    registerModel.ReminderDays = LocalDBManager.Instance.GetDBSetting("ReminderDays")?.Value;
                    if (registerModel.ReminderDays.Contains("1"))
                        registerModel.IsRecommendedReminder = false;

                }
                catch (Exception ex)
                {

                }

            }
            else
                registerModel.IsRecommendedReminder = IsRecommendedReminder;
            if (IsEquipment)
            {
                var model = new EquipmentModel();

                if (LocalDBManager.Instance.GetDBSetting("workout_place")?.Value == "gym")
                {
                    model.IsEquipmentEnabled = true;
                    model.IsDumbbellEnabled = isDumbbells;
                    model.IsPlateEnabled = IsPlates;
                    model.IsPullyEnabled = IsPully;
                    model.IsChinUpBarEnabled = IsChinupBar;
                    model.Active = "gym";

                    model.IsHomeEquipmentEnabled = false;
                    model.IsHomeDumbbell = true;
                    model.IsHomePlate = true;
                    model.IsHomePully = true;
                    model.IsHomeChinupBar = true;

                }
                else
                {
                    model.IsEquipmentEnabled = false;
                    model.IsDumbbellEnabled = true;
                    model.IsPlateEnabled = true;
                    model.IsPullyEnabled = true;
                    model.IsChinUpBarEnabled = true;

                    model.IsOtherEquipmentEnabled = false;
                    model.IsOtherDumbbell = true;
                    model.IsOtherPlate = true;
                    model.IsOtherPully = true;
                    model.IsOtherChinupBar = true;

                    model.IsHomeEquipmentEnabled = true;
                    model.IsHomeDumbbell = isDumbbells;
                    model.IsHomePlate = IsPlates;
                    model.IsHomePully = IsPully;
                    model.IsHomeChinupBar = IsChinupBar;
                    model.Active = "home";
                }
                var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
                model.AvilablePlate = kgString;
                model.AvilableHomePlate = kgString;
                model.AvilableOtherPlate = kgString;
                var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
                model.AvilableLbPlate = lbString;
                model.AvilableHomeLbPlate = lbString;
                model.AvilableOtherLbPlate = lbString;

                var kgString1 = "50_2_True|47.5_2_True|45_2_True|42.5_2_True|40_2_True|37.5_2_True|35_2_True|32.5_2_True|30_2_True|27.5_2_True|25_2_True|22.5_2_True|20_2_True|17.5_2_True|15_2_True|12.5_2_True|10_2_True|7.5_2_True|5_2_True|2.5_2_True|1_2_True";
                model.AvilableDumbbell = kgString1;
                model.AvilableHomeDumbbell = kgString1;
                model.AvilableOtherDumbbell = kgString1;
                var lbString1 = "90_2_True|85_2_True|80_2_True|75_2_True|70_2_True|65_2_True|60_2_True|55_2_True|50_2_True|45_2_True|40_2_True|35_2_True|30_2_True|25_2_True|20_2_True|15_2_True|12_2_True|10_2_True|8_2_True|5_2_True|3_2_True|2_2_True";
                model.AvilableLbDumbbell = lbString1;
                model.AvilableHomeLbDumbbell = lbString1;
                model.AvilableOtherLbDumbbell = lbString1;

                var kgString2 = "5_20_True|1.5_2_True";
                var lbString2 = "10_20_True|5_2_True|2.5_2_True";
                model.AvilablePulley = kgString2;
                model.AvilableHomePulley = kgString2;
                model.AvilableOtherPulley = kgString2;

                model.AvilableLbPulley = lbString2;
                model.AvilableHomeLbPulley = lbString2;
                model.AvilableOtherLbPulley = lbString2;

                registerModel.EquipmentModel = model;
            }
            if (IncrementUnit != null)
            {
                registerModel.Increments = IncrementUnit.Kg;
            }
            //if (LocalDBManager.Instance.GetDBSetting("workout_increments") != null)
            //{
            //    var increments = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("workout_increments").Value, System.Globalization.CultureInfo.InvariantCulture);
            //    var incrementsWeight = new MultiUnityWeight(increments, LocalDBManager.Instance.GetDBSetting("massunit").Value);
            //    registerModel.Increments = incrementsWeight.Kg;
            //}

            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }
            //await BoomSuccessPopup();
             OpenPreview(registerModel);
            UserInfosModel registerResponse = await DrMuscleRestClient.Instance.RegisterUserAfterDemo(registerModel);

            if (registerResponse != null)
            {
                CancelNotification();
                SetTrialUserNotifications();
                DateTime current = DateTime.Now;

                UserInfosModel uim = registerResponse;

                LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                LocalDBManager.Instance.SetDBSetting("password", registerModel.Password);
                //LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                //LocalDBManager.Instance.SetDBSetting("token_expires_date", current.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("IsMobility", uim.IsMobility == null ? null : uim.IsMobility == false ? "false" : "true");
                LocalDBManager.Instance.SetDBSetting("MaxWorkoutDuration", uim.WorkoutDuration.ToString());
                LocalDBManager.Instance.SetDBSetting("IsExerciseQuickMode", uim.IsExerciseQuickMode == null ? null : uim.IsExerciseQuickMode == false ? "false" : "true");
                LocalDBManager.Instance.SetDBSetting("MobilityLevel", uim.MobilityLevel);
                LocalDBManager.Instance.SetDBSetting("MobilityRep", uim.MobilityRep == null ? "" : Convert.ToString(uim.MobilityRep));
                LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                LocalDBManager.Instance.SetDBSetting("reprangeType", uim.ReprangeType.ToString());
                LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                if (uim.IsPyramid)
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                    LocalDBManager.Instance.SetDBSetting("IsRPyramid", "true");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
                }
                else if (uim.IsNormalSet == null || uim.IsNormalSet == true)
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "Normal");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", uim.IsNormalSet == null ? "true" : "false");
                }
                else
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
                }
                LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_123_sound", uim.IsTimer321 ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_reps_sound", uim.IsRepsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); if (uim.Age != null)
                    LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                if (uim.TargetIntake != null && uim.TargetIntake != 0)
                    LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
                LocalDBManager.Instance.SetDBSetting("RecommendedReminder", uim.IsRecommendedReminder == true ? "true" : uim.IsRecommendedReminder == null ? "null" : "false");
                LocalDBManager.Instance.SetDBSetting("IsEmailReminder", uim.IsReminderEmail ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("ReminderHours", uim.ReminderBeforeHours.ToString());
                if (uim.IsRecommendedReminder == true)
                {
                    var timeSpan = new TimeSpan(0, 22, 0, 0);
                    IAlarmAndNotificationService alarmAndNotificationService = new AlarmAndNotificationService();
                    alarmAndNotificationService.CancelNotification(101);
                    alarmAndNotificationService.CancelNotification(102);
                    alarmAndNotificationService.CancelNotification(103);
                    alarmAndNotificationService.CancelNotification(104);
                    alarmAndNotificationService.CancelNotification(105);
                    alarmAndNotificationService.CancelNotification(106);
                    alarmAndNotificationService.CancelNotification(107);
                    alarmAndNotificationService.CancelNotification(108); alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout? You got this!", timeSpan, 1111, NotificationInterval.Week);
                }
                if (uim.WarmupsValue != null)
                {
                    LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
                }
                if (uim.Increments != null)
                    LocalDBManager.Instance.SetDBSetting("workout_increments", uim.Increments.Kg.ToString().ReplaceWithDot());
                if (uim.Max != null)
                    LocalDBManager.Instance.SetDBSetting("workout_max", uim.Max.Kg.ToString().ReplaceWithDot());
                if (uim.Min != null)
                    LocalDBManager.Instance.SetDBSetting("workout_min", uim.Min.Kg.ToString().ReplaceWithDot());
                if (uim.BodyWeight != null)
                {
                    LocalDBManager.Instance.SetDBSetting("BodyWeight", uim.BodyWeight.Kg.ToString().ReplaceWithDot());
                }
                if (uim.WeightGoal != null)
                {
                    LocalDBManager.Instance.SetDBSetting("WeightGoal", uim.WeightGoal.Kg.ToString().ReplaceWithDot());
                }
                SetupEquipment(uim);

                if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                else
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());

                LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");

                LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("1By1Side", uim.Is1By1Side ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");

                ((App)Application.Current).displayCreateNewAccount = true;

                if (uim.Gender.Trim().ToLowerInvariant().Equals("man"))
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "Background2.png");
                else
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "BackgroundFemale.png");
                App.IsNewFirstTime = false;

                long workoutTemplateId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId").Value);
                long pId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedProgramId").Value);
                var upi = new GetUserProgramInfoResponseModel()
                {
                    NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value },
                    RecommendedProgram = new WorkoutTemplateGroupModel() { Id = pId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
                };
                
                LocalDBManager.Instance.SetDBSetting("ReadyRegisterModel", JsonConvert.SerializeObject(registerModel));
                DrMuscleRestClient.Instance.SaveWorkoutV3WithoutLoader(new SaveWorkoutModel() { WorkoutId = workoutTemplateId });
                if (upi != null)
                {
                    WorkoutTemplateModel nextWorkout = upi.NextWorkoutTemplate;
                    if (upi.NextWorkoutTemplate.Exercises == null || upi.NextWorkoutTemplate.Exercises.Count() == 0)
                    {
                        try
                        {
                            nextWorkout = await DrMuscleRestClient.Instance.GetUserCustomizedCurrentWorkoutWithoutLoader(workoutTemplateId);
                            //nextWorkout = w.Workouts.First(ww => ww.Id == upi.NextWorkoutTemplate.Id);
                        }
                        catch (Exception ex)
                        {
                            await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            {
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                Message = AppResources.PleaseCheckInternetConnection,
                                Title = AppResources.ConnectionError
                            });
                            // await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Message = AppResources.PleaseCheckInternetConnection,
                            //    Title = AppResources.ConnectionError
                            //});
                            return;
                        }

                    }
                    LocalDBManager.Instance.SetDBSetting("FirstStepCompleted", "");
                    
                    
                }


            }
            else
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = "Something went wrong, please try again.",
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            isProcessing = false;
        }

        async void OpenDemo()
        {
            CurrentLog.Instance.CurrentExercise = new ExerciceModel()
            {
                BodyPartId = 7,
                VideoUrl = "https://youtu.be/Plh1CyiPE_Y",
                IsBodyweight = true,
                IsEasy = false,
                IsFinished = false,
                IsMedium = false,
                IsNextExercise = false,
                IsNormalSets = false,
                IsSwapTarget = false,
                IsSystemExercise = true,
                IsTimeBased = false,
                IsUnilateral = false,
                Label = "Crunch",
                RepsMaxValue = null,
                RepsMinValue = null,
                Timer = null,
                Id = 864
            };
            App.IsDemoProgress = true;
            LocalDBManager.Instance.SetDBSetting("DemoProgress", "true");
            await PagesFactory.PushAsync<NewDemoPage>();

        }
        async Task OpenPreview(RegisterModel registerModel)
        {
            //string val = LocalDBManager.Instance.GetDBSetting("BetaVersion")?.Value;
            //if (!string.IsNullOrEmpty(val))
            //{
            //    if (val == "Beta")
            //    {
            //        var waitHandle1 = new EventWaitHandle(false, EventResetMode.AutoReset);
            //        var modalPage1 = new Views.PreviewOverlay();
            //        modalPage1._registerModel = registerModel;
            //        modalPage1.Disappearing += (sender2, e2) =>
            //        {
            //            waitHandle1.Set();
            //        };
            //        await PopupNavigation.Instance.PushAsync(modalPage1);

            //        await Task.Run(() => waitHandle1.WaitOne());
            //        //OpenDemo();
            //    }
            //    else
            //    {
            //        await BoomSuccessPopup();
            //        OpenDemo();
            //    }
            //}
            //else
            //{
            //    if (isBetaExperience == true)
            //    {
            
            SentrySdk.ConfigureScope(scope =>
            {
                scope.User = new Sentry.User
                {
                    Email = LocalDBManager.Instance.GetDBSetting("email")?.Value
                };
            });
            
                    var waitHandle1 = new EventWaitHandle(false, EventResetMode.AutoReset);
                    var modalPage1 = new Views.PreviewOverlay();
                    modalPage1.Disappearing += (sender2, e2) =>
                    {
                        waitHandle1.Set();
                    };
                    await PopupNavigation.Instance.PushAsync(modalPage1);

                    await Task.Run(() => waitHandle1.WaitOne());
                    
                    
            //    }
            //    else
            //        OpenDemo();
            //}

            try
            {
                App.RegisterDeviceToken();
            }
            catch (Exception ex)
            {

            }
        }

        async Task BoomSuccessPopup()
        {
            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            var modalPage = new Views.GeneralPopup("Lists.png", "Settings saved", "Do a demo workout to unlock the full experience", "Demo workout", new Thickness(18,0,0,0));
            modalPage.Disappearing += (sender2, e2) =>
            {
                waitHandle.Set();
            };
            await PopupNavigation.Instance.PushAsync(modalPage);

            await Task.Run(() => waitHandle.WaitOne());

        }
        async void LearnMoreButton_Clicked(object sender, EventArgs e)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }
            await AddAnswer(((DrMuscleButton)sender).Text);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            await ClearOptions();
            await Task.Delay(300);



            DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
            learnMore.Exp = "";
            learnMore.ExpDesc = "";
            if (experienceSetting != null)
            {
                if (experienceSetting.Value == "less1year")
                {
                    learnMore.Exp = $"Less than 1 year";
                    learnMore.ExpDesc = "You're new to lifting, so I recommend you train each muscle 3x a week on a full-body program. You will progress faster that way.";
                }
                if (experienceSetting.Value == "1-3years")
                {
                    learnMore.Exp = $"1-3 years";
                    learnMore.ExpDesc = "You have been lifting for over a year, so I recommend you train each muscle 2x a week on a split-body program. This gives you more time to recover between workouts for each muscle.";
                }
                if (experienceSetting.Value == "more3years")
                {
                    learnMore.Exp = $"More than 3 years";
                    learnMore.ExpDesc = "You have been lifting for 3+ years, so I recommend you train each muscle 2x a week on a split-body program with A and B days. This gives you more time to recover between workouts for each muscle and different exercises on A and B days. At your level, this is almost always needed to continue making progress.";
                }
                if (!string.IsNullOrEmpty(learnMore.Exp))
                {
                    await AddQuestion($"Your experience: {learnMore.Exp}");
                    await AddQuestion(learnMore.ExpDesc);
                    await AddOptions(AppResources.GotIt, LearnMoreSteps2);
                    return;
                }
            }

            LearnMoreSteps2(sender, e);

        }

        async void LearnMoreSteps2(object sender, EventArgs e)
        {
            await ClearOptions();
            var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value == "Woman";
            if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "BuildMuscle")
            {
                learnMore.Focus = IsWoman ? "Getting stronger" : "Building muscle";
                learnMore.FocusDesc = IsWoman ? "To get stronger, I recommend you repeat each exercise 5-12 times. You will also get stronger by lifting in that range." : "To build muscle, I recommend you repeat each exercise 5-12 times. You will also get stronger by lifting in that range.";
            }
            else if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "BuildMuscleBurnFat")
            {
                learnMore.Focus = IsWoman ? "Overall fitness" : "Building muscle and burning fat";
                learnMore.FocusDesc = IsWoman ? "For overall fitness, I recommend you repeat each exercise 8-15 times." : "To build muscle and burn fat, I recommend you repeat each exercise 8-15 times.";
            }
            else if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "FatBurning")
            {
                learnMore.Focus = "Burning fat";
                learnMore.FocusDesc = "To burn fat, I recommend you repeat each exercise 12-20 times. You will burn more calories by lifting in that range.";
            }
            await AddQuestion($"Your focus: {learnMore.Focus}");
            await AddQuestion(learnMore.FocusDesc);
            await AddOptions(AppResources.GotIt, LearnMoreSteps3);

        }
        async void LearnMoreSteps3(object sender, EventArgs e)
        {
            await ClearOptions();
            int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            learnMore.Age = age;
            await AddQuestion($"Your age: {age}");
            if (age > 50)
                learnMore.AgeDesc = $"Recovery is slower at {age}. So, I added easy days to your program.";
            else if (age > 30)
                learnMore.AgeDesc = $"Recovery is a bit slower at {age}. So, I'm updating your program to make sure you train each muscle max 2x a week.";
            else
                learnMore.AgeDesc = "Recovery is optimal at your age. You can train each muscle as often as 3x a week.";
            //await AddQuestion(learnMore.AgeDesc);
            //await Task.Delay(100);
            if (Device.RuntimePlatform.Equals(Device.iOS))
            {
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            }
            //await AddOptions(AppResources.GotIt, LearnMoreComplete
            LearnMoreComplete(sender, e);
        }

        async void LearnMoreComplete(object sender, EventArgs e)
        {
            await ClearOptions();
            await ProgramReadyInstruction();

            await AddOptions(AppResources.GotIt, GotItButton_Clicked);

            stackOptions.Children.Add(TermsConditionStack);
            TermsConditionStack.IsVisible = true;

        }
        async Task ProgramReadyInstruction()
        {


            string goalLabel = "";
            try
            {

                if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "BuildMuscle")
                {
                    goalLabel = AppResources.IUpdateItEveryTimeYouWorkOutBuild;
                }
                else if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "BuildMuscleBurnFat")
                {
                    goalLabel = AppResources.IUpdateItEveryTimeYouWorkOutBuildNBuildFat;
                }
                else if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "FatBurning")
                {
                    goalLabel = AppResources.IUpdateItEveryTimeYouWorkOutBurnFatFaster;
                }


            }
            catch (Exception ex)
            { }

            try
            {
                await ClearOptions();
                DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
                DBSetting workoutPlaceSetting = LocalDBManager.Instance.GetDBSetting("workout_place");
                var programId = 0;
                if (experienceSetting != null && workoutPlaceSetting != null)
                {
                    if (workoutPlaceSetting.Value == "gym")
                    {

                        if (experienceSetting.Value == "less1year")
                        {
                            programId = 10;
                        }
                        if (experienceSetting.Value == "1-3years")
                        {
                            programId = 15;
                        }
                        if (experienceSetting.Value == "more3years")
                        {
                            programId = 16;
                        }
                    }
                    else if (workoutPlaceSetting.Value == "home")
                    {
                        if (experienceSetting.Value == "less1year")
                        {
                            programId = 17;
                        }
                        if (experienceSetting.Value == "1-3years")
                        {
                            programId = 21;
                        }
                        if (experienceSetting.Value == "more3years")
                        {
                            programId = 22;
                        }
                    }
                    else if (workoutPlaceSetting.Value == "homeBodyweightOnly")
                    {
                        programId = 487;
                    }
                    else if (workoutPlaceSetting.Value == "homeBodyweightBandsOnly")
                    {
                        programId = 1339;
                        if (experienceSetting?.Value == "more3years")
                        {
                            programId = 1338;
                        }
                    }

                    if (experienceSetting.Value == "beginner")
                    {
                        programId = 488;
                    }
                    var ProgramLabel = "";
                    int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
                    learnMore.Age = age;
                    switch (programId)
                    {
                        case 10:
                            ProgramLabel = "[Gym] Full-Body Level 1";
                            if (age > 50)
                            {

                                ProgramLabel = "[Gym] Full-Body Level 6";
                                programId = 395;
                            }
                            else if (age > 30)
                            {
                                ProgramLabel = "[Gym] Up/Low Split Level 1";
                                programId = 15;
                            }

                            break;
                        case 15:
                            ProgramLabel = "[Gym] Up/Low Split Level 1";
                            if (age > 50)
                            {

                                ProgramLabel = "[Gym] Up/Low Split Level 6";
                                programId = 401;
                            }
                            break;
                        case 16:
                            ProgramLabel = "[Gym] Up/Low Split Level 2";
                            if (age > 50)
                            {

                                ProgramLabel = "[Gym] Up/Low Split Level 6";
                                programId = 401;
                            }
                            break;
                        case 17:
                            ProgramLabel = "[Home] Full-Body Level 1";

                            if (age > 50)
                            {
                                ProgramLabel = "[Home] Full-Body Level 6";
                                programId = 398;
                            }
                            else if (age > 30)
                            {
                                ProgramLabel = "[Home] Up/Low Split Level 1";
                                programId = 21;
                            }
                            break;
                        case 21:
                            ProgramLabel = "[Home] Up/Low Split Level 1";

                            if (age > 50)
                            {
                                ProgramLabel = "[Home] Up/Low Split Level 6";
                                programId = 404;
                            }
                            break;
                        case 22:
                            ProgramLabel = "[Home] Up/Low Split Level 2";
                            if (age > 50)
                            {
                                ProgramLabel = "[Home] Up/Low Split Level 6";
                                programId = 404;
                            }
                            break;
                        case 487:
                            ProgramLabel = "Bodyweight Level 2";
                            break;
                        case 488:
                            ProgramLabel = "Bodyweight Level 1";
                            break;
                        case 1339:
                            ProgramLabel = "[Home] Buffed w/ Bands Level 1";
                            break;
                        case 1338:
                            ProgramLabel = "[Home] Buffed w/ Bands Level 2";
                                break;
                    }

                    if (age > 51)
                    {

                    }
                    var weekX = 0;
                    var dayText = "";
                    var instructionText = "";
                    instructionText += "- This template is flexible\n";
                    instructionText += AppResources.YouCanChangeWorkoutDays + "\n";
                    if (LocalDBManager.Instance.GetDBSetting("experience").Value == "more3years" || LocalDBManager.Instance.GetDBSetting("experience").Value == "1-3years" || ProgramLabel.ToLower().Contains("split"))
                    {
                        weekX = 4;
                        dayText += "Your week should look like this:" + "\n";
                        dayText += AppResources.MondayUpperBody1More1Year + "\n";
                        dayText += AppResources.TuesdayLowerBodyMore1Year + "\n";
                        dayText += AppResources.WednesdayOffMore1Year + "\n";
                        dayText += AppResources.ThursdayUpperBodyMore1Year + "\n";
                        dayText += AppResources.FridayOrSaturdayLowerBodyMore1Year + "\n";
                        dayText += AppResources.SundayOffMore1Year;
                        instructionText += AppResources.WorkOutYourUpperAndYourLowerBody2xWeekForBestResultsMore1Year + "\n";
                        instructionText += "- Don't worry: you can change everything later.";
                    }
                    else
                    {
                        weekX = 3;
                        dayText += "Your week should look like this:" + "\n";
                        dayText += AppResources.MondayFullBody + "\n";
                        dayText += AppResources.TuesdayOff + "\n";
                        dayText += AppResources.WednesdayFullBody + "\n";
                        dayText += AppResources.ThursdayOff + "\n";
                        dayText += AppResources.FridayOrSaturdayFullBody + "\n";
                        dayText += AppResources.SundayOff;
                        instructionText += AppResources.WorkOutYourFullBody3xWeekForBestResults + "\n";
                        instructionText += "- Don't worry: you can change everything later.";
                    }

                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);


                    LocalDBManager.Instance.SetDBSetting("ReadyToSignup", "true");
                    try
                    {

                        int? workoutId = null;

                        int? remainingWorkout = null;
                        var WorkoutInfo2 = "";
                        //Setup Program
                        if (experienceSetting != null && workoutPlaceSetting != null)
                        {
                            if (workoutPlaceSetting.Value == "gym")
                            {

                                if (experienceSetting.Value == "less1year")
                                {
                                    WorkoutInfo2 = "[Gym] Full-Body";
                                    workoutId = 104;
                                    programId = 10;
                                    remainingWorkout = 18;
                                }
                                if (experienceSetting.Value == "1-3years")
                                {
                                    WorkoutInfo2 = "[Gym] Lower-Body";
                                    workoutId = 106;
                                    programId = 15;
                                    remainingWorkout = 32;
                                }
                                if (experienceSetting.Value == "more3years")
                                {
                                    WorkoutInfo2 = "[Gym] Upper-Body Level 2";
                                    workoutId = 424;
                                    programId = 16;
                                    remainingWorkout = 40;
                                }
                            }
                            else if (workoutPlaceSetting.Value == "home")
                            {
                                if (experienceSetting.Value == "less1year")
                                {
                                    WorkoutInfo2 = "[Home] Full-Body";
                                    workoutId = 108;
                                    programId = 17;
                                    remainingWorkout = 18;
                                }
                                if (experienceSetting.Value == "1-3years")
                                {
                                    WorkoutInfo2 = "[Home] Upper-Body";
                                    workoutId = 109;
                                    programId = 21;
                                    remainingWorkout = 24;
                                }
                                if (experienceSetting.Value == "more3years")
                                {
                                    WorkoutInfo2 = "[Home] Upper-Body Level 2";
                                    workoutId = 428;
                                    programId = 22;
                                    remainingWorkout = 40;
                                }
                            }
                            else if (workoutPlaceSetting.Value == "homeBodyweightOnly")
                            {
                                WorkoutInfo2 = "Bodyweight Level 2";
                                workoutId = 12646;
                                programId = 487;
                                remainingWorkout = 12;
                            }
                            else if (workoutPlaceSetting.Value == "homeBodyweightBandsOnly")
                            {
                                WorkoutInfo2 = "[Home] Buffed w/ Bands";
                                workoutId = 15377;
                                programId = 1339;
                                remainingWorkout = 15;

                                if (experienceSetting?.Value == "more3years")
                                {
                                    WorkoutInfo2 = "[Home] Buffed w/ Bands 2A";
                                    workoutId = 15375;
                                    programId = 1338;
                                    remainingWorkout = 18;
                                }
                            }
                            if (experienceSetting.Value == "beginner")
                            {
                                WorkoutInfo2 = "Bodyweight Level 1";
                                workoutId = 12645;
                                programId = 488;
                                remainingWorkout = 6;
                            }


                            switch (programId)
                            {
                                case 10:
                                    ProgramLabel = "[Gym] Full-Body Level 1";
                                    if (age > 50)
                                    {
                                        ProgramLabel = "[Gym] Full-Body Level 6";
                                        programId = 395;
                                        WorkoutInfo2 = "[Gym] Full-Body 6A (easy)";
                                        workoutId = 2312;
                                    }
                                    else if (age > 30)
                                    {
                                        ProgramLabel = "[Gym] Up/Low Split Level 1";
                                        programId = 15;
                                        WorkoutInfo2 = "[Gym] Lower Body";
                                        workoutId = 107;
                                    }
                                    break;
                                case 15:
                                    ProgramLabel = "[Gym] Up/Low Split Level 1";
                                    if (age > 50)
                                    {
                                        ProgramLabel = "[Gym] Up/Low Split Level 6";
                                        programId = 401;
                                        WorkoutInfo2 = "[Gym] Lower Body 6A (easy)";
                                        workoutId = 2337;
                                    }
                                    break;
                                case 16:
                                    ProgramLabel = "[Gym] Up/Low Split Level 2";
                                    if (age > 50)
                                    {
                                        ProgramLabel = "[Gym] Up/Low Split Level 6";
                                        programId = 401;
                                        WorkoutInfo2 = "[Gym] Lower Body 6A (easy)";
                                        workoutId = 2337;
                                    }
                                    break;
                                case 17:
                                    ProgramLabel = "[Home] Full-Body Level 1";
                                    if (age > 50)
                                    {
                                        ProgramLabel = "[Home] Full-Body Level 6";
                                        programId = 398;
                                        WorkoutInfo2 = "[Home] Full-Body 6A (easy)";
                                        workoutId = 2325;
                                    }
                                    else if (age > 30)
                                    {
                                        ProgramLabel = "[Home] Up/Low Split Level 1";
                                        programId = 21;
                                        WorkoutInfo2 = "[Home] Lower Body";
                                        workoutId = 110;
                                    }
                                    break;
                                case 21:
                                    ProgramLabel = "[Home] Up/Low Split Level 1";
                                    if (age > 50)
                                    {
                                        ProgramLabel = "[Home] Up/Low Split Level 6";
                                        programId = 404;
                                        WorkoutInfo2 = "[Home] Lower Body 6A (easy)";
                                        workoutId = 2361;
                                    }
                                    break;
                                case 22:
                                    ProgramLabel = "[Home] Up/Low Split Level 2";
                                    if (age > 50)
                                    {
                                        ProgramLabel = "[Home] Up/Low Split Level 6";
                                        programId = 404;
                                        WorkoutInfo2 = "[Home] Lower Body 6A (easy)";
                                        workoutId = 2361;
                                    }
                                    break;
                                case 487:
                                    ProgramLabel = "Bodyweight Level 2";
                                    break;
                                case 488:
                                    ProgramLabel = "Bodyweight Level 1";
                                    break;
                            }
                            LocalDBManager.Instance.SetDBSetting("recommendedWorkoutId", workoutId.ToString());
                            LocalDBManager.Instance.SetDBSetting("recommendedWorkoutLabel", WorkoutInfo2);
                            LocalDBManager.Instance.SetDBSetting("recommendedProgramId", programId.ToString());
                            LocalDBManager.Instance.SetDBSetting("recommendedRemainingWorkout", remainingWorkout.ToString());

                            LocalDBManager.Instance.SetDBSetting("recommendedProgramLabel", ProgramLabel);
                        }
                        //SignUp here
                        RegisterModel registerModel = new RegisterModel();

                        registerModel.Firstname = "";
                        registerModel.EmailAddress = "";
                        registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
                        registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                        if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                            registerModel.IsQuickMode = false;
                        else
                        {
                            if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
                                registerModel.IsQuickMode = null;
                            else
                                registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
                        }
                        if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                            registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
                        registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
                        registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
                        if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                            registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
                        if (LocalDBManager.Instance.GetDBSetting("WeightGoal") != null)
                            registerModel.WeightGoal = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("WeightGoal").Value, CultureInfo.InvariantCulture), "kg");

                        registerModel.Password = "";
                        registerModel.ConfirmPassword = "";
                        registerModel.LearnMoreDetails = learnMore;
                        registerModel.IsHumanSupport = IsHumanSupport;
                        registerModel.IsCardio = IsIncludeCardio;
                        registerModel.BodyPartPrioriy = bodypartName;
                        registerModel.IsMobility = IsIncludeMobility;
                        registerModel.MobilityLevel = mobilityLevel;
                        registerModel.IsReminderEmail = true;
                        registerModel.TimeBeforeWorkout = 5;

                        if (LocalDBManager.Instance.GetDBSetting("email") != null && LocalDBManager.Instance.GetDBSetting("email").Value.Contains("yopmail") || (LocalDBManager.Instance.GetDBSetting("Environment") != null && LocalDBManager.Instance.GetDBSetting("Environment").Value != "Production"))
                        { registerModel.MainGoal = ""; }
                        else
                            registerModel.MainGoal = isBetaExperience == true ? "Beta" : isBetaExperience == false ? "Normal" : "";
                        if (IsEquipment)
                        {
                            var model = new EquipmentModel();

                            if (LocalDBManager.Instance.GetDBSetting("workout_place")?.Value == "gym")
                            {
                                model.IsEquipmentEnabled = true;
                                model.IsDumbbellEnabled = isDumbbells;
                                model.IsPlateEnabled = IsPlates;
                                model.IsPullyEnabled = IsPully;
                                model.IsChinUpBarEnabled = IsChinupBar;
                                model.Active = "gym";
                            }
                            else
                            {
                                model.IsHomeEquipmentEnabled = true;
                                model.IsHomeDumbbell = isDumbbells;
                                model.IsHomePlate = IsPlates;
                                model.IsHomePully = IsPully;
                                model.IsHomeChinupBar = IsChinupBar;
                                model.Active = "home";
                            }
                            registerModel.EquipmentModel = model;
                        }
                        if (LocalDBManager.Instance.GetDBSetting("workout_increments") != null)
                        {
                            var increments = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("workout_increments").Value, System.Globalization.CultureInfo.InvariantCulture);
                            var incrementsWeight = new MultiUnityWeight(increments, LocalDBManager.Instance.GetDBSetting("massunit").Value);
                            registerModel.Increments = incrementsWeight.Kg;
                        }
                        LocalDBManager.Instance.SetDBSetting("ReadyRegisterModel", JsonConvert.SerializeObject(registerModel));
                    }
                    catch (Exception ex)
                    {

                    }
                    _firebase.LogEvent("got_program", ProgramLabel);

                    LearnMoreSkipButton_Clicked(new DrMuscleButton(), EventArgs.Empty);
                    //await AddQuestion("Congratulations! Your custom, smart program is ready. Learn more?");
                    //await AddOptions("Learn more", async (sender, esc) =>
                    //{
                    //    await AddAnswer(((DrMuscleButton)sender).Text, false);

                    //    await AddQuestion($"Based on your age, experience, and preferences, I recommend you work out {weekX} times a week on this program: '{ProgramLabel}'.");
                    //    _firebase.LogEvent("got_program", ProgramLabel);
                    //    await ClearOptions();
                    //    await AddOptions(AppResources.GotIt, BtnGotItProgram);
                    //    //Day

                    //    if (Device.RuntimePlatform.Equals(Device.iOS))
                    //    {
                    //        lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    //        lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                    //    }
                    //    async void BtnGotItProgram(object sse, EventArgs evr)
                    //    {
                    //        //await AddAnswer(((DrMuscleButton)sse).Text, false);

                    //        //    ((DrMuscleButton)sse).Clicked -= BtnGotItProgram;
                    //        //    ((DrMuscleButton)sse).Clicked += BtnGotItNext;
                    //        //    await AddQuestion($"Your smart program:\n- Updates in real time\n- Matches your progress\n- Speeds up future progress");

                    //        //    if (Device.RuntimePlatform.Equals(Device.iOS))
                    //        //    {
                    //        //        lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    //        //        lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                    //        //    }

                    //        //    async void BtnGotItNext(object s, EventArgs e)
                    //        //{
                    //        //    _firebase.LogEvent("gotten_program", ProgramLabel);
                    //        //    if (Device.RuntimePlatform.Equals(Device.Android))
                    //        //        await Task.Delay(300);
                    //        LearnMoreSkipButton_Clicked(sse, evr);
                    //        //await AddQuestion(AppResources.WarningIMNOtLikeOtherAppsIGuideYouInRealTimeBased, false);
                    //        //if (Device.RuntimePlatform.Equals(Device.iOS))
                    //        //{
                    //        //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    //        //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                    //        //}
                    //        //    ((DrMuscleButton)s).Clicked -= BtnGotItNext;
                    //        //((DrMuscleButton)s).Clicked += btnGotIt4_Clicked;
                    //        //async void btnGotIt4_Clicked(object ob, EventArgs ar)
                    //        //{
                    //        //    await AddAnswer(((DrMuscleButton)ob).Text, false);
                    //        //    await ClearOptions();
                    //        //    //
                    //        //    await AddQuestion("This app uses the latest science, but it can't correct your form, or allow for a medical condition. It may be wrong at times. When in doubt, trust your judgment. Features like smart watch integration and calendar view are not yet available. But if you’re an early adopter who wants to get in shape fast, you'll love your new custom workouts. Give us a shot: we'll treat your feedback like gold. Got a suggestion? Get in touch. We release new features every month.");
                    //        //    if (Device.RuntimePlatform.Equals(Device.Android))
                    //        //        await Task.Delay(300);
                    //        //    await AddOptions("View latest features", async (sand, ees) =>
                    //        //    {
                    //        //        if (Device.RuntimePlatform.Equals(Device.Android))
                    //        //            await Task.Delay(300);
                    //        //        //Device.OpenUri(new Uri("https://dr-muscle.com/timeline"));
                    //        //        await Browser.OpenAsync("https://dr-muscle.com/timeline", BrowserLaunchMode.SystemPreferred);
                    //        //        LearnMoreSkipButton_Clicked(sand, ees);
                    //        //    });
                    //        //    await AddOptions("Continue", async (sende, eee) =>
                    //        //    {
                    //        //        if (Device.RuntimePlatform.Equals(Device.Android))
                    //        //            await Task.Delay(300);
                    //        //        LearnMoreSkipButton_Clicked(sende, eee);
                    //        //    });
                    //        //    stackOptions.Children.Add(TermsConditionStack);
                    //        //    TermsConditionStack.IsVisible = true;
                    //        //}
                    //    }
                    //    //}
                    //});
                    //await AddOptions("Skip", async (sender, esc) => {
                    //    //await AddAnswer(((DrMuscleButton)sender).Text, false);
                    //    _firebase.LogEvent("got_program", ProgramLabel);

                    //    LearnMoreSkipButton_Clicked(sender, esc);
                    //});

                }
            }
            catch (Exception ex)
            {

            }

        }

        async void LearnMoreSkipButton_Clicked(object sender, EventArgs e)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }
            //await AddAnswer(((DrMuscleButton)sender).Text);
            await ClearOptions();
            int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            learnMore.Age = age;

            if (age > 50)
                learnMore.AgeDesc = $"Recovery is slower at {age}.";
            else if (age > 30)
                learnMore.AgeDesc = $"Recovery is a bit slower at {age}. So, I recommend you train each muscle 2x a week (instead of 3x a week).";
            else
                learnMore.AgeDesc = "Recovery is optimal at your age. You can train each muscle as often as 3x a week.";



            var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value == "Woman";
            if (!string.IsNullOrEmpty(focusText))
            {
                focusText = focusText.Replace("\nStronger sex drive", "");
                focusText = focusText.Replace("Stronger sex drive", "");
            }
            if (string.IsNullOrEmpty(focusText))
                focusText = "Better health";

            if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "BuildMuscle")
            {
                learnMore.Focus = focusText.Replace("\n", ", ").ToLower();
                if (learnMore.Focus.Contains(","))
                {
                    int ind = learnMore.Focus.LastIndexOf(",");
                    var subStr = learnMore.Focus.Substring(ind);
                    var newStr = subStr.Replace(",", " and");
                    learnMore.Focus = learnMore.Focus.Replace(subStr, newStr);
                }

                learnMore.FocusDesc = IsWoman ? "To get stronger, I recommend you repeat each exercise 5-12 times. You will also get stronger by lifting in that range." : "To build muscle, I recommend you repeat each exercise 5-12 times. You will also get stronger by lifting in that range.";
            }
            else if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "BuildMuscleBurnFat")
            {
                learnMore.Focus = focusText.Replace("\n", ", ").ToLower();
                if (learnMore.Focus.Contains(","))
                {
                    int ind = learnMore.Focus.LastIndexOf(",");
                    var subStr = learnMore.Focus.Substring(ind);
                    var newStr = subStr.Replace(",", " and");
                    learnMore.Focus = learnMore.Focus.Replace(subStr, newStr);
                }
                learnMore.FocusDesc = IsWoman ? "For overall fitness, I recommend you repeat each exercise 8-15 times." : "To build muscle and burn fat, I recommend you repeat each exercise 8-15 times.";
            }
            else if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "FatBurning")
            {
                learnMore.Focus = focusText.Replace("\n", ", ").ToLower();
                if (learnMore.Focus.Contains(","))
                {
                    int ind = learnMore.Focus.LastIndexOf(",");
                    var subStr = learnMore.Focus.Substring(ind);
                    var newStr = subStr.Replace(",", " and");
                    learnMore.Focus = learnMore.Focus.Replace(subStr, newStr);
                }
                learnMore.FocusDesc = "To burn fat, I recommend you repeat each exercise 12-20 times. You will burn more calories by lifting in that range.";
            }
            LocalDBManager.Instance.SetDBSetting("DBFocus", learnMore.Focus);

            DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
            learnMore.Exp = "";
            learnMore.ExpDesc = "";
            if (experienceSetting != null)
            {
                if (experienceSetting.Value == "less1year")
                {
                    learnMore.Exp = $"Less than 1 year";
                    learnMore.ExpDesc = "You're new to lifting, so I recommend you train each muscle 3x a week on a full-body program. You will progress faster that way.";
                }
                if (experienceSetting.Value == "1-3years")
                {
                    learnMore.Exp = $"1-3 years";
                    learnMore.ExpDesc = "You have been lifting for over a year, so I recommend you train each muscle 2x a week on a split-body program. This gives you more time to recover between working out each muscle.";
                }
                if (experienceSetting.Value == "more3years")
                {
                    learnMore.Exp = $"More than 3 years";
                    learnMore.ExpDesc = "You have been lifting for 3+ years, so I recommend you train each muscle 2x a week on a split-body program with A and B days. This gives you more time to recover between working out each muscle and more exercise variation. At your level, it's important. ";
                }

            }


            //SignupCode here:
            await Task.Delay(1000);
            
            if (LocalDBManager.Instance.GetDBSetting("LoginType") != null && LocalDBManager.Instance.GetDBSetting("LoginType").Value == "Social")
                GoogleFbLoginAfterDemo();
            else
                CreateAccountAfterDemoButton_Clicked();
            // SetMenu();
        }

        async void SetMenu()
        {
            //if (IsRealBetaExperience == true)
            //{
                LoginWithEmailButton.Text = "Sign in with email";
            //}
            stackOptions.Children.Add(StackSignupMenu);
            BottomViewHeight.Height = GridLength.Auto;
            StackSignupMenu.IsVisible = true;
            //if (Device.RuntimePlatform.Equals(Device.iOS))
            //{
            //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            //}
            //if (IsRealBetaExperience == true)
            //{
                StackSignupMenu.Padding = new Thickness(20, 20, 20, 10);
                stackOptions.Children.Add(TermsConditionStackBeta);
                TermsConditionStackBeta.IsVisible = true;

            //}
            //else
            //{
            //    stackOptions.Children.Add(TermsConditionStack);
            //    TermsConditionStack.IsVisible = true;

            //}
            //
            //
            
        }
        async void GotItButton_Clicked(object sender, EventArgs e)
        {
            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            await ClearOptions();
            await Task.Delay(300);
            GetEmail();
        }

        async void ConnectWithEmail(object sender, EventArgs e)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError
                });
                //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                //{
                //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                //    Message = AppResources.PleaseCheckInternetConnection,
                //    Title = AppResources.ConnectionError
                //});
                return;
            }
            await ClearOptions();
            //GetFirstName();
            GetEmail();
        }


        async Task AddQuestion(string question, bool isAnimated = true)
        {
            BotList.Add(new BotModel()
            {
                Question = question,
                Type = BotType.Ques
            });
            if (isAnimated)
            {
                await Task.Delay(300);
            }
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
        }

        async Task AddAnswer(string answer, bool isClearOptions = true)
        {
            BotList.Add(new BotModel()
            {
                Answer = answer,
                Type = BotType.Ans
            });
            if (isClearOptions)
                await ClearOptions();
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);

            await Task.Delay(300);
        }

        async Task<CustomImageButton> AddCheckbox(string title, EventHandler handler, bool ischecked = false)
        {
            CustomImageButton imgBtn = new CustomImageButton()
            {
                Text = title,
                Source = ischecked ? "done.png" : "Undone.png",
                BackgroundColor = Color.White,
                TextFontColor = AppThemeConstants.OffBlackColor,
                Margin = new Thickness(25, 1),
                Padding = new Thickness(2)
            };
            imgBtn.Clicked += handler;
            stackOptions.Children.Add(imgBtn);
            return imgBtn;
        }

        async Task<DrMuscleButton> AddOptions(string title, EventHandler handler)
        {
            var grid = new Grid();
            var pancakeView = new PancakeView() {  HeightRequest = 55, Margin = new Thickness(25, 2) };
            pancakeView.OffsetAngle = Device.RuntimePlatform.Equals(Device.Android) ? 45 : 90;
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#195276"), Offset = 0 });
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#0C2432"), Offset = 1 });
            grid.Children.Add(pancakeView);


            var btn = new DrMuscleButton()
            {
                Text = title,
                TextColor = Color.Black,
                BackgroundColor = Color.White,
                FontSize = Device.RuntimePlatform.Equals(Device.Android) ? 15 : 17,
                CornerRadius = 0,
                HeightRequest = 55
            };
            btn.Clicked += handler;
            SetDefaultButtonStyle(btn);
            grid.Children.Add(btn);
            stackOptions.Children.Add(grid);

            BottomViewHeight.Height = GridLength.Auto;
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);

            return btn;
        }

        public async void LoginWithAppleAsync(object sender, EventArgs ee)
        {
            var account = await appleSignInService.SignInAsync();
            if (account != null)
            {
                if (!string.IsNullOrEmpty(account.Email))
                {
                    await SecureStorage.SetAsync("Email", account.Email);
                    if (!string.IsNullOrEmpty(account.Name))
                        await SecureStorage.SetAsync("Name", account.Name);
                    else if (!string.IsNullOrEmpty(account.GivenName))
                        await SecureStorage.SetAsync("Name", account.GivenName);
                    else if (!string.IsNullOrEmpty(account.FamilyName))
                        await SecureStorage.SetAsync("Name", account.FamilyName);
                    else
                        await SecureStorage.SetAsync("Name", "  ");
                }
                else
                {
                    string email = await SecureStorage.GetAsync("Email");
                    string name = await SecureStorage.GetAsync("Name");
                    account.Email = email;
                    account.Name = name;
                }
                if (string.IsNullOrEmpty(account.Email))
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Message = "We haven't get email. Please login with email.",
                        Title = AppResources.Error
                    });

                    return;
                }
                OnLoginCompleted(null, new GoogleClientResultEventArgs<GoogleUser>(new GoogleUser() { Email = account.Email, Name = account.Name }, GoogleActionStatus.Completed));
            }

        }
        //Google Login
        public async void LoginWithGoogleAsync(object sender, EventArgs ee)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError
                });
                //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                //{
                //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                //    Message = AppResources.PleaseCheckInternetConnection,
                //    Title = AppResources.ConnectionError
                //});
                return;
            }
            _googleClientManager.OnLogin += OnLoginCompleted;
            try
            {
                await _googleClientManager.LoginAsync();
            }
            catch (GoogleClientSignInNetworkErrorException e)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = e.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientSignInCanceledErrorException e)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = e.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientSignInInvalidAccountErrorException e)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = e.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientSignInInternalErrorException e)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = e.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientNotInitializedErrorException e)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = e.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientBaseException e)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = e.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }

        }


        private async void OnLoginCompleted(object sender, GoogleClientResultEventArgs<GoogleUser> loginEventArgs)
        {
            _googleClientManager.OnLogin -= OnLoginCompleted;
            if (loginEventArgs.Data != null)
            {
                GoogleUser googleUser = loginEventArgs.Data;
                UserProfile user = new UserProfile();
                user.Name = googleUser.Name;
                user.Email = googleUser.Email;
                if (user.Picture != null)
                    user.Picture = googleUser.Picture;
                //var token = CrossGoogleClient.Current.ActiveToken;
                LocalDBManager.Instance.SetDBSetting("LoginType", "Social");
                LocalDBManager.Instance.SetDBSetting("GToken", "");
                if (user.Picture != null)
                    LocalDBManager.Instance.SetDBSetting("ProfilePic", user.Picture.OriginalString);

                //IsLoggedIn = true;
                bool IsExistingUser = false;
                BooleanModel existingUser = await DrMuscleRestClient.Instance.IsEmailAlreadyExist(new IsEmailAlreadyExistModel() { email = user.Email });
                if (existingUser != null)
                {
                    if (existingUser.Result)
                    {

                        ConfirmConfig ShowAlertPopUp = new ConfirmConfig()
                        {
                            Title = "You are already registered",
                            Message = "Use another account or log into your existing account.",
                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            OkText = "Use another account",
                            CancelText = AppResources.LogIn,

                        };
                        var actionOk = await UserDialogs.Instance.ConfirmAsync(ShowAlertPopUp);
                        if (actionOk)
                        {
                            return;
                        }
                        else
                        {
                            IsExistingUser = true;
                            //((App)Application.Current).displayCreateNewAccount = true;
                            //await PagesFactory.PushAsync<WelcomePage>();
                        }

                        //return;
                    }

                }


                string mass = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                string body = null;
                if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                    body = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), mass).Kg.ToString();
                else
                    body = "60";
               

                LoginSuccessResult lr = await DrMuscleRestClient.Instance.GoogleLogin("", user.Email, user.Name, body, mass);
                if (lr != null)
                {
                    UserInfosModel uim = null;
                    if (existingUser.Result)
                    {
                        uim = await DrMuscleRestClient.Instance.GetUserInfo();
                    }
                    else
                    {
                        //RegisterModel registerModel = new RegisterModel();
                        //registerModel.Firstname = user.Name;
                        //registerModel.EmailAddress = user.Email;
                        //registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
                        //registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                        //if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                        //    registerModel.IsQuickMode = false;
                        //else
                        //{
                        //    if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
                        //        registerModel.IsQuickMode = null;
                        //    else
                        //        registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
                        //}
                        //if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                        //    registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
                        //registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
                        //registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
                        //registerModel.Password = "";
                        //registerModel.ConfirmPassword = "";
                        //registerModel.LearnMoreDetails = learnMore;
                        //registerModel.IsHumanSupport = IsHumanSupport;
                        //registerModel.IsCardio = IsIncludeCardio;
                        //registerModel.BodyPartPrioriy = bodypartName;

                        //registerModel.MainGoal = mainGoal;
                        //if (IsEquipment)
                        //{
                        //    registerModel.EquipmentModel = new EquipmentModel()
                        //    {
                        //        IsEquipmentEnabled = true,
                        //        IsDumbbellEnabled = isDumbbells,
                        //        IsPlateEnabled = IsPlates,
                        //        IsPullyEnabled = IsPully,
                        //        IsChinUpBarEnabled = IsChinupBar,

                        //    };
                        //}
                        //if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                        //    registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
                        RegisterModel registerModel = new RegisterModel();
                        registerModel.Firstname = user.Name;
                        registerModel.EmailAddress = user.Email;
                        LocalDBManager.Instance.SetDBSetting("email", user.Email);
                        LocalDBManager.Instance.SetDBSetting("firstname", user.Name);
                        registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                        registerModel.Password = "";
                        registerModel.ConfirmPassword = "";
                        if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                            registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
                        if (LocalDBManager.Instance.GetDBSetting("WeightGoal") != null)
                            registerModel.WeightGoal = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("WeightGoal").Value, CultureInfo.InvariantCulture), "kg");

                        //await DrMuscleRestClient.Instance.RegisterUserBeforeDemo(registerModel);
                        DependencyService.Get<IFirebase>().LogEvent("account_created", "");
                        LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                       LocalDBManager.Instance.SetDBSetting("token_expires_date", DateTime.Now.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                        await AccountCreatedPopup();
                        SetUpRestOnboarding();
                        LocalDBManager.Instance.SetDBSetting("FirstStepCompleted", "true");
                        return;
                    }
                    try
                    {
                        //LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                        //LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                        LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                        LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                        LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                        LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                       LocalDBManager.Instance.SetDBSetting("token_expires_date", DateTime.Now.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                        LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
                        LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                        LocalDBManager.Instance.SetDBSetting("reprangeType", uim.ReprangeType.ToString());
                        LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                        LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                        LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); LocalDBManager.Instance.SetDBSetting("WorkoutTypeList", "0");
                        LocalDBManager.Instance.SetDBSetting("ExerciseTypeList", "0");
                        LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                        if (uim.Age != null)
                            LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                        if (uim.TargetIntake != null && uim.TargetIntake != 0)
                            LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
                        //if (uim.ReminderTime != null)
                        //    LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                        //if (uim.ReminderDays != null)
                        //    LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);

                        LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_123_sound", uim.IsTimer321 ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_reps_sound", uim.IsRepsSound ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
                        LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
                        LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");

                        LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("1By1Side", uim.Is1By1Side ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");
                        if (uim.IsNormalSet == null || uim.IsNormalSet == true)
                        {
                            LocalDBManager.Instance.SetDBSetting("SetStyle", "Normal");
                            LocalDBManager.Instance.SetDBSetting("IsPyramid", uim.IsNormalSet == null ? "true" : "false");
                        }
                        else
                        {
                            LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                            LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
                        }
                        if (uim.Increments != null)
                            LocalDBManager.Instance.SetDBSetting("workout_increments", uim.Increments.Kg.ToString().ReplaceWithDot());
                        if (uim.Max != null)
                            LocalDBManager.Instance.SetDBSetting("workout_max", uim.Max.Kg.ToString().ReplaceWithDot());
                        if (uim.Min != null)
                            LocalDBManager.Instance.SetDBSetting("workout_min", uim.Min.Kg.ToString().ReplaceWithDot());
                        if (uim.BodyWeight != null)
                        {
                            LocalDBManager.Instance.SetDBSetting("BodyWeight", uim.BodyWeight.Kg.ToString().ReplaceWithDot());
                        }
                        if (uim.WeightGoal != null)
                        {
                            LocalDBManager.Instance.SetDBSetting("WeightGoal", uim.WeightGoal.Kg.ToString().ReplaceWithDot());
                        }
                        if (uim.WarmupsValue != null)
                        {
                            LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
                        }

                        if (uim.EquipmentModel != null)
                        {
                            LocalDBManager.Instance.SetDBSetting("Equipment", uim.EquipmentModel.IsEquipmentEnabled ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("ChinUp", uim.EquipmentModel.IsChinUpBarEnabled ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("Dumbbell", uim.EquipmentModel.IsDumbbellEnabled ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("Plate", uim.EquipmentModel.IsPlateEnabled ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("Pully", uim.EquipmentModel.IsPullyEnabled ? "true" : "false");
                        }
                        else
                        {
                            LocalDBManager.Instance.SetDBSetting("Equipment", "false");
                            LocalDBManager.Instance.SetDBSetting("ChinUp", "true");
                            LocalDBManager.Instance.SetDBSetting("Dumbbell", "true");
                            LocalDBManager.Instance.SetDBSetting("Plate", "true");
                            LocalDBManager.Instance.SetDBSetting("Pully", "true");
                        }
                        if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                            LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                        else
                            LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());

                        ((App)Application.Current).displayCreateNewAccount = true;

                        if (uim.Gender.Trim().ToLowerInvariant().Equals("man"))
                            LocalDBManager.Instance.SetDBSetting("BackgroundImage", "Background2.png");
                        else
                            LocalDBManager.Instance.SetDBSetting("BackgroundImage", "BackgroundFemale.png");

                        if (IsExistingUser)
                        {
                            App.IsDemoProgress = false;
                            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                            await PagesFactory.PopToRootAsync(true);
                            return;
                        }
                        await AccountCreatedPopup();
                        SetUpRestOnboarding();
                        LocalDBManager.Instance.SetDBSetting("FirstStepCompleted", "true");
                        // CancelNotification();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    UserDialogs.Instance.Alert(new AlertConfig()
                    {
                        Message = AppResources.EmailAndPasswordDoNotMatch,
                        Title = AppResources.UnableToLogIn,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });
                }
            }
            else
            {
                UserDialogs.Instance.Alert(new AlertConfig()
                {
                    Message = loginEventArgs.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });

            }

            _googleClientManager.OnLogin -= OnLoginCompleted;

        }


        private async void GoogleFbLoginAfterDemo()
        {
            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
            DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
            DBSetting workoutPlaceSetting = LocalDBManager.Instance.GetDBSetting("workout_place");
            int? workoutId = null;
            int? programId = null;
            int? remainingWorkout = null;
            var WorkoutInfo2 = "";


            string ProgramLabel = AppResources.NotSetUp;
            int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);


            var level = 0;
            if (LocalDBManager.Instance.GetDBSetting("MainLevel") != null)
                level = int.Parse(LocalDBManager.Instance.GetDBSetting("MainLevel").Value);
            bool isSplit = LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Split");
            if (LocalDBManager.Instance.GetDBSetting("ExerciseVariety")?.Value == "More")
                level += 1;

            bool isGym = workoutPlaceSetting?.Value == "gym";
            var mo = AppThemeConstants.GetLevelProgram(level, isGym, !isSplit, LocalDBManager.Instance.GetDBSetting("MainProgram").Value);

            if (workoutPlaceSetting?.Value == "homeBodyweightOnly")
            {
                if (level <= 1)
                {
                    mo.workoutName = "Bodyweight 1";
                    mo.workoutid = 12645;
                    mo.programid = 487;
                    mo.reqWorkout = 12;
                    mo.programName = "Bodyweight Level 1";
                }
                else if (level <= 2)
                {
                    mo.workoutName = "Bodyweight 2";
                    mo.workoutid = 12646;
                    mo.programid = 488;
                    mo.reqWorkout = 12;
                    mo.programName = "Bodyweight Level 2";
                }
                else if (level == 3)
                {
                    mo.workoutName = "Bodyweight 3";
                    mo.workoutid = 14017;
                    mo.programid = 923;
                    mo.reqWorkout = 15;
                    mo.programName = "Bodyweight Level 3";
                }
                else if (level >= 4)
                {
                    mo.workoutName = "Bodyweight 4";
                    mo.workoutid = 14019;
                    mo.programid = 924;
                    mo.reqWorkout = 15;
                    mo.programName = "Bodyweight Level 4";
                }

            }
            else if (workoutPlaceSetting.Value == "homeBodyweightBandsOnly")
            {
                mo.workoutName = "[Home] Buffed w/ Bands";
                mo.programName = "[Home] Buffed w/ Bands Level 1";
                mo.workoutid = 15377;
                mo.programid = 1339;
                mo.reqWorkout = 15;

                if (LocalDBManager.Instance.GetDBSetting("CustomExperience")?.Value == "an active, experienced lifter" && LocalDBManager.Instance.GetDBSetting("ExerciseVariety")?.Value == "Less")
                {
                    mo.workoutName = "[Home] Buffed w/ Bands 2A";
                    mo.programName = "[Home] Buffed w/ Bands Level 2";
                    mo.workoutid = 15375;
                    mo.programid = 1338;
                    mo.reqWorkout = 18;
                }
                else if (LocalDBManager.Instance.GetDBSetting("CustomExperience")?.Value == "an active, experienced lifter" && LocalDBManager.Instance.GetDBSetting("ExerciseVariety")?.Value == "More")
                {
                    mo.workoutName = "[Home] Buffed w/ Bands 3A";
                    mo.programName = "[Home] Buffed w/ Bands Level 3";
                    mo.workoutid = 17321;
                    mo.programid = 2032;
                    mo.reqWorkout = 24;
                }
            }

            LocalDBManager.Instance.SetDBSetting("recommendedWorkoutId", mo.workoutid.ToString());
            LocalDBManager.Instance.SetDBSetting("recommendedWorkoutLabel", mo.workoutName);
            LocalDBManager.Instance.SetDBSetting("recommendedProgramId", mo.programid.ToString());
            LocalDBManager.Instance.SetDBSetting("recommendedRemainingWorkout", mo.reqWorkout.ToString());

            LocalDBManager.Instance.SetDBSetting("recommendedProgramLabel", mo.programName);
            //}
            //SignUp here

            workoutId = mo.workoutid;
            WorkoutInfo2 = mo.workoutName;
            programId = mo.programid;
            ProgramLabel = mo.programName;
            remainingWorkout = mo.reqWorkout;

            RegisterModel registerModel = new RegisterModel();
            registerModel.Firstname = LocalDBManager.Instance.GetDBSetting("firstname")?.Value;
            registerModel.EmailAddress = LocalDBManager.Instance.GetDBSetting("email")?.Value;
            registerModel.IsMobility = IsIncludeMobility;
            registerModel.MobilityLevel = mobilityLevel;
            registerModel.IsReminderEmail = true;
            registerModel.TimeBeforeWorkout = 5;

            registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
            registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
            if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                registerModel.IsQuickMode = false;
            else
            {
                if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
                    registerModel.IsQuickMode = null;
                else
                    registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
            }
            if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
            registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
            registerModel.Password = "";
            registerModel.ConfirmPassword = "";
            registerModel.LearnMoreDetails = learnMore;
            registerModel.IsHumanSupport = IsHumanSupport;
            registerModel.IsCardio = IsIncludeCardio;
            registerModel.BodyPartPrioriy = bodypartName;
            registerModel.SetStyle = SetStyle;
            registerModel.IsPyramid = IsPyramid;
            registerModel.isDing = isDing;
            registerModel.WorkoutDuration = workoutDurationLenggth;
            registerModel.ProgramId = programId;
            registerModel.IsMobility = IsIncludeMobility;
            registerModel.MobilityLevel = mobilityLevel;
            registerModel.IsReminderEmail = true;
            registerModel.TimeBeforeWorkout = 5;

            if (LocalDBManager.Instance.GetDBSetting("Height")?.Value != null && Config.UserHeight != 0)
            {
                registerModel.Height = Config.UserHeight;
            }
            if (LocalDBManager.Instance.GetDBSetting("email") != null && LocalDBManager.Instance.GetDBSetting("email").Value.Contains("yopmail") || (LocalDBManager.Instance.GetDBSetting("Environment") != null && LocalDBManager.Instance.GetDBSetting("Environment").Value != "Production"))
            { registerModel.MainGoal = ""; }
            else
                registerModel.MainGoal = isBetaExperience == true ? "Beta" : isBetaExperience == false ? "Normal" : "";
            if (LocalDBManager.Instance.GetDBSetting("ReminderDays") != null && LocalDBManager.Instance.GetDBSetting("ReminderTime") != null)
            {
                try
                {
                    registerModel.ReminderTime = TimeSpan.Parse(LocalDBManager.Instance.GetDBSetting("ReminderTime").Value);
                    registerModel.ReminderDays = LocalDBManager.Instance.GetDBSetting("ReminderDays")?.Value;
                    if (registerModel.ReminderDays.Contains("1"))
                        registerModel.IsRecommendedReminder = true;
                    else
                        registerModel.IsRecommendedReminder = null;

                }
                catch (Exception ex)
                {

                }

            }
            else
                registerModel.IsRecommendedReminder = IsRecommendedReminder;
            if (IsEquipment)
            {
                var model = new EquipmentModel();

                if (LocalDBManager.Instance.GetDBSetting("workout_place")?.Value == "gym")
                {
                    model.IsEquipmentEnabled = true;
                    model.IsDumbbellEnabled = isDumbbells;
                    model.IsPlateEnabled = IsPlates;
                    model.IsPullyEnabled = IsPully;
                    model.IsChinUpBarEnabled = IsChinupBar;
                    model.Active = "gym";

                    model.IsHomeEquipmentEnabled = false;
                    model.IsHomeDumbbell = true;
                    model.IsHomePlate = true;
                    model.IsHomePully = true;
                    model.IsHomeChinupBar = true;

                }
                else
                {
                    model.IsEquipmentEnabled = false;
                    model.IsDumbbellEnabled = true;
                    model.IsPlateEnabled = true;
                    model.IsPullyEnabled = true;
                    model.IsChinUpBarEnabled = true;

                    model.IsOtherEquipmentEnabled = false;
                    model.IsOtherDumbbell = true;
                    model.IsOtherPlate = true;
                    model.IsOtherPully = true;
                    model.IsOtherChinupBar = true;

                    model.IsHomeEquipmentEnabled = true;
                    model.IsHomeDumbbell = isDumbbells;
                    model.IsHomePlate = IsPlates;
                    model.IsHomePully = IsPully;
                    model.IsHomeChinupBar = IsChinupBar;
                    model.Active = "home";
                }
                var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
                model.AvilablePlate = kgString;
                model.AvilableHomePlate = kgString;
                model.AvilableOtherPlate = kgString;
                var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
                model.AvilableLbPlate = lbString;
                model.AvilableHomeLbPlate = lbString;
                model.AvilableOtherLbPlate = lbString;
                registerModel.EquipmentModel = model;
            }
            if (IncrementUnit != null)
            {
                registerModel.Increments = IncrementUnit.Kg;
            }
            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
            if (LocalDBManager.Instance.GetDBSetting("WeightGoal") != null)
                registerModel.WeightGoal = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("WeightGoal").Value, CultureInfo.InvariantCulture), "kg");

            if (string.IsNullOrEmpty(registerModel.Firstname))
                registerModel.Firstname = "";
            var uim = await DrMuscleRestClient.Instance.RegisterUserAfterDemo(registerModel);

            try
            {
                CancelNotification();
                SetTrialUserNotifications();
                LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                LocalDBManager.Instance.SetDBSetting("reprangeType", uim.ReprangeType.ToString());
                LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); LocalDBManager.Instance.SetDBSetting("WorkoutTypeList", "0");
                LocalDBManager.Instance.SetDBSetting("ExerciseTypeList", "0");
                LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                if (uim.Age != null)
                    LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                if (uim.TargetIntake != null && uim.TargetIntake != 0)
                    LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
                //if (uim.ReminderTime != null)
                //    LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                //if (uim.ReminderDays != null)
                //    LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);

                LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_123_sound", uim.IsTimer321 ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_reps_sound", uim.IsRepsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
                LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
                LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("IsMobility", uim.IsMobility == null ? null : uim.IsMobility == false ? "false" : "true");
                LocalDBManager.Instance.SetDBSetting("MaxWorkoutDuration", uim.WorkoutDuration.ToString());
                LocalDBManager.Instance.SetDBSetting("IsExerciseQuickMode", uim.IsExerciseQuickMode == null ? null : uim.IsExerciseQuickMode == false ? "false" : "true");
                LocalDBManager.Instance.SetDBSetting("MobilityLevel", uim.MobilityLevel);
                LocalDBManager.Instance.SetDBSetting("MobilityRep", uim.MobilityRep == null ? "" : Convert.ToString(uim.MobilityRep));
                LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("1By1Side", uim.Is1By1Side ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");
                if (uim.IsPyramid)
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                    LocalDBManager.Instance.SetDBSetting("IsRPyramid", "true");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
                }
                else if (uim.IsNormalSet == null || uim.IsNormalSet == true)
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "Normal");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", uim.IsNormalSet == null ? "true" : "false");
                }
                else
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
                }
                if (uim.Increments != null)
                    LocalDBManager.Instance.SetDBSetting("workout_increments", uim.Increments.Kg.ToString().ReplaceWithDot());
                if (uim.Max != null)
                    LocalDBManager.Instance.SetDBSetting("workout_max", uim.Max.Kg.ToString().ReplaceWithDot());
                if (uim.Min != null)
                    LocalDBManager.Instance.SetDBSetting("workout_min", uim.Min.Kg.ToString().ReplaceWithDot());
                if (uim.BodyWeight != null)
                {
                    LocalDBManager.Instance.SetDBSetting("BodyWeight", uim.BodyWeight.Kg.ToString().ReplaceWithDot());
                }
                if (uim.WeightGoal != null)
                {
                    LocalDBManager.Instance.SetDBSetting("WeightGoal", uim.WeightGoal.Kg.ToString().ReplaceWithDot());
                }
                if (uim.WarmupsValue != null)
                {
                    LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
                }

                SetupEquipment(uim);
                ((App)Application.Current).displayCreateNewAccount = true;
                if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                else
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());
                if (uim.Gender.Trim().ToLowerInvariant().Equals("man"))
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "Background2.png");
                else
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "BackgroundFemale.png");

                LocalDBManager.Instance.SetDBSetting("IsEmailReminder", uim.IsReminderEmail ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("ReminderHours", uim.ReminderBeforeHours.ToString());

                LocalDBManager.Instance.SetDBSetting("RecommendedReminder", uim.IsRecommendedReminder == true ? "true" : uim.IsRecommendedReminder == null ? "null" : "false");
                if (uim.IsRecommendedReminder == true)
                {
                    var timeSpan = new TimeSpan(0, 22, 0, 0);
                    IAlarmAndNotificationService alarmAndNotificationService = new AlarmAndNotificationService();
                    alarmAndNotificationService.CancelNotification(101);
                    alarmAndNotificationService.CancelNotification(102);
                    alarmAndNotificationService.CancelNotification(103);
                    alarmAndNotificationService.CancelNotification(104);
                    alarmAndNotificationService.CancelNotification(105);
                    alarmAndNotificationService.CancelNotification(106);
                    alarmAndNotificationService.CancelNotification(107);
                    alarmAndNotificationService.CancelNotification(108);
                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout? You got this!", timeSpan, 1111, NotificationInterval.Week);
                }
                LocalDBManager.Instance.SetDBSetting("FirstStepCompleted", "");
                try
                {
                    OpenPreview(registerModel);
                }
                catch (Exception ex)
                {

                }


            }
            catch (Exception ex)
            {

            }
        }

        public void Logout()
        {
            _googleClientManager.OnLogout += OnLogoutCompleted;
            _googleClientManager.Logout();
        }

        private void OnLogoutCompleted(object sender, EventArgs loginEventArgs)
        {

        }

        //Facebook Login
        private async void LoginWithFBButton_Clicked(object sender, EventArgs e)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError
                });
                //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                //{
                //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                //    Message = AppResources.PleaseCheckInternetConnection,
                //    Title = AppResources.ConnectionError
                //});
                return;
            }
            FacebookUser result = await _manager.Login();
            if (result == null)
            {
                UserDialogs.Instance.Alert(new AlertConfig()
                {
                    Message = AppResources.AnErrorOccursWhenSigningIn,
                    Title = AppResources.UnableToLogIn,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                await WelcomePage_OnFBLoginSucceded(result.Id, result.Email, "", result.Token, result.FirstName);
            });
        }

        private async Task WelcomePage_OnFBLoginSucceded(string FBId, string FBEmail, string FBGender, string FBToken, string firstname)
        {
            if (string.IsNullOrEmpty(FBEmail))
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = "Your Facebook account is not connected with email (or we do not have permission to access it). Please sign up with email.",
                    Title = AppResources.Error
                });

                return;
            }
            LocalDBManager.Instance.SetDBSetting("LoginType", "Social");
            LocalDBManager.Instance.SetDBSetting("FBId", FBId);
            LocalDBManager.Instance.SetDBSetting("FBEmail", FBEmail);
            LocalDBManager.Instance.SetDBSetting("firstname", firstname);
            LocalDBManager.Instance.SetDBSetting("FBGender", FBGender);
            LocalDBManager.Instance.SetDBSetting("FBToken", FBToken);
            var url = $"http://graph.facebook.com/{FBId}/picture?type=square";
            LocalDBManager.Instance.SetDBSetting("ProfilePic", url);



            BooleanModel existingUser = await DrMuscleRestClient.Instance.IsEmailAlreadyExist(new IsEmailAlreadyExistModel() { email = FBEmail });
            bool IsExistingUser = false;
            if (existingUser != null)
            {
                if (existingUser.Result)
                {

                    ConfirmConfig ShowAlertPopUp = new ConfirmConfig()
                    {
                        Title = "You are already registered",
                        Message = "Use another account or log into your existing account.",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = "Use another account",
                        CancelText = AppResources.LogIn,

                    };
                    var actionOk = await UserDialogs.Instance.ConfirmAsync(ShowAlertPopUp);
                    if (actionOk)
                    {
                        return;
                    }
                    else
                    {
                        //((App)Application.Current).displayCreateNewAccount = true;
                        //await PagesFactory.PushAsync<WelcomePage>();
                        IsExistingUser = true;
                    }

                    //return;
                }

            }
            //Log in d'un compte existant avec Facebook
            string mass = "lb";
            string body = null;
                body = new MultiUnityWeight(150, "lb").Kg.ToString();
            try
            {


                LoginSuccessResult lr = await DrMuscleRestClient.Instance.FacebookLogin(FBToken, body, mass);
                if (lr != null)
                {
                    DateTime current = DateTime.Now;
                    UserInfosModel uim = null;
                    if (existingUser.Result)
                    {
                        uim = await DrMuscleRestClient.Instance.GetUserInfo();
                    }
                    else
                    {
                        RegisterModel registerModel = new RegisterModel();
                        registerModel.Firstname = firstname;
                        registerModel.EmailAddress = FBEmail;

                        registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;

                        if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                            registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
                       
                        registerModel.BodyWeight = new MultiUnityWeight(150, "lb");
                        registerModel.Password = "";
                        registerModel.ConfirmPassword = "";

                        //await DrMuscleRestClient.Instance.RegisterUserBeforeDemo(registerModel);
                        await AccountCreatedPopup();
                        SetUpRestOnboarding();
                        LocalDBManager.Instance.SetDBSetting("FirstStepCompleted", "true");
                        DependencyService.Get<IFirebase>().LogEvent("account_created", "");
                        return;
                    }
                    LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                    if (!string.IsNullOrEmpty(uim.Firstname))
                    LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                    LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                    LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                    LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                    LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                    LocalDBManager.Instance.SetDBSetting("token_expires_date", DateTime.Now.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                    LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
                    LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                    LocalDBManager.Instance.SetDBSetting("reprangeType", uim.ReprangeType.ToString());
                    LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                    LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); LocalDBManager.Instance.SetDBSetting("WorkoutTypeList", "0");
                    LocalDBManager.Instance.SetDBSetting("ExerciseTypeList", "0");
                    LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                    if (uim.Age != null)
                        LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                    if (uim.TargetIntake != null && uim.TargetIntake != 0)
                        LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
                    //if (uim.ReminderTime != null)
                    //    LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                    //if (uim.ReminderDays != null)
                    //    LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);

                    LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("timer_123_sound", uim.IsTimer321 ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("timer_reps_sound", uim.IsRepsSound ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
                    LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
                    LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");

                    LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("1By1Side", uim.Is1By1Side ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");
                    if (uim.IsNormalSet == null || uim.IsNormalSet == true)
                    {
                        LocalDBManager.Instance.SetDBSetting("SetStyle", "Normal");
                        LocalDBManager.Instance.SetDBSetting("IsPyramid", uim.IsNormalSet == null ? "true" : "false");
                    }
                    else
                    {
                        LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                        LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
                    }
                    if (uim.Increments != null)
                        LocalDBManager.Instance.SetDBSetting("workout_increments", uim.Increments.Kg.ToString().ReplaceWithDot());
                    if (uim.Max != null)
                        LocalDBManager.Instance.SetDBSetting("workout_max", uim.Max.Kg.ToString().ReplaceWithDot());
                    if (uim.Min != null)
                        LocalDBManager.Instance.SetDBSetting("workout_min", uim.Min.Kg.ToString().ReplaceWithDot());
                    if (uim.BodyWeight != null)
                    {
                        LocalDBManager.Instance.SetDBSetting("BodyWeight", uim.BodyWeight.Kg.ToString().ReplaceWithDot());
                    }
                    if (uim.WeightGoal != null)
                    {
                        LocalDBManager.Instance.SetDBSetting("WeightGoal", uim.WeightGoal.Kg.ToString().ReplaceWithDot());
                    }
                    if (uim.WarmupsValue != null)
                    {
                        LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
                    }
                    if (uim.EquipmentModel != null)
                    {
                        LocalDBManager.Instance.SetDBSetting("Equipment", uim.EquipmentModel.IsEquipmentEnabled ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("ChinUp", uim.EquipmentModel.IsChinUpBarEnabled ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("Dumbbell", uim.EquipmentModel.IsDumbbellEnabled ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("Plate", uim.EquipmentModel.IsPlateEnabled ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("Pully", uim.EquipmentModel.IsPullyEnabled ? "true" : "false");
                    }
                    else
                    {
                        LocalDBManager.Instance.SetDBSetting("Equipment", "false");
                        LocalDBManager.Instance.SetDBSetting("ChinUp", "true");
                        LocalDBManager.Instance.SetDBSetting("Dumbbell", "true");
                        LocalDBManager.Instance.SetDBSetting("Plate", "true");
                        LocalDBManager.Instance.SetDBSetting("Pully", "true");
                    }
                    ((App)Application.Current).displayCreateNewAccount = true;

                    if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                        LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                    else
                        LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());

                    //await PagesFactory.PopToRootAsync(true);
                    //await PagesFactory.PushAsync<MainAIPage>();
                    //    App.IsWelcomeBack = true;
                    //    App.IsDemoProgress = false;
                    //LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                    //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                    if (IsExistingUser)
                    {
                        App.IsDemoProgress = false;
                        LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                        await PagesFactory.PopToRootAsync(true);
                        return;
                    }
                    await AccountCreatedPopup();
                    SetUpRestOnboarding();
                    LocalDBManager.Instance.SetDBSetting("FirstStepCompleted", "true");
                }
                else
                {
                    UserDialogs.Instance.Alert(new AlertConfig()
                    {
                        Message = AppResources.EmailAndPasswordDoNotMatch,
                        Title = AppResources.UnableToLogIn,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });
                }
            }
            catch (Exception ex)
            {

                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = "We are facing problem to signup with your facebook account. Please sign up with email.",
                    Title = AppResources.Error
                });

            }
            //if (string.IsNullOrEmpty(FBEmail))
            //{
            //    await UserDialogs.Instance.AlertAsync(new AlertConfig()
            //    {
            //        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //        Message = "Your Facebook account is not connected with email (or we do not have permission to access it). Please sign up with email.",
            //        Title = AppResources.Error
            //    });

            //    return;
            //}
            //LocalDBManager.Instance.SetDBSetting("LoginType", "Social");
            //LocalDBManager.Instance.SetDBSetting("FBId", FBId);
            //LocalDBManager.Instance.SetDBSetting("FBEmail", FBEmail);
            //LocalDBManager.Instance.SetDBSetting("FBGender", FBGender);
            //LocalDBManager.Instance.SetDBSetting("FBToken", FBToken);
            //var url = $"http://graph.facebook.com/{FBId}/picture?type=square";
            //LocalDBManager.Instance.SetDBSetting("ProfilePic", url);



            //BooleanModel existingUser = await DrMuscleRestClient.Instance.IsEmailAlreadyExist(new IsEmailAlreadyExistModel() { email = FBEmail });
            //bool IsExistingUser = false;
            //if (existingUser != null)
            //{
            //    if (existingUser.Result)
            //    {

            //        ConfirmConfig ShowAlertPopUp = new ConfirmConfig()
            //        {
            //            Title = "You are already registered",
            //            Message = "Use another account or log into your existing account.",
            //            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //            OkText = "Use another account",
            //            CancelText = AppResources.LogIn,

            //        };
            //        var actionOk = await UserDialogs.Instance.ConfirmAsync(ShowAlertPopUp);
            //        if (actionOk)
            //        {
            //            return;
            //        }
            //        else
            //        {
            //            //((App)Application.Current).displayCreateNewAccount = true;
            //            //await PagesFactory.PushAsync<WelcomePage>();
            //            IsExistingUser = true;
            //        }

            //        //return;
            //    }

            //}
            ////Log in d'un compte existant avec Facebook
            //string mass = LocalDBManager.Instance.GetDBSetting("massunit").Value;
            //string body = null;
            //if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
            //    body = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg").Kg.ToString();
            //try
            //{


            //    LoginSuccessResult lr = await DrMuscleRestClient.Instance.FacebookLogin(FBToken, body, mass);
            //    if (lr != null)
            //    {
            //        DateTime current = DateTime.Now;
            //        UserInfosModel uim = null;
            //        if (existingUser.Result)
            //        {
            //            uim = await DrMuscleRestClient.Instance.GetUserInfo();
            //        }
            //        else
            //        {
            //            RegisterModel registerModel = new RegisterModel();
            //            registerModel.Firstname = firstname;
            //            registerModel.EmailAddress = FBEmail;
            //            registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
            //            registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
            //            if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
            //                registerModel.IsQuickMode = false;
            //            else
            //            {
            //                if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
            //                    registerModel.IsQuickMode = null;
            //                else
            //                    registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
            //            }
            //            if (LocalDBManager.Instance.GetDBSetting("Age") != null)
            //                registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            //            registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
            //            registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
            //            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
            //                registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
            //            registerModel.Password = "";
            //            registerModel.ConfirmPassword = "";
            //            registerModel.LearnMoreDetails = learnMore;
            //            registerModel.IsHumanSupport = IsHumanSupport;
            //            registerModel.IsCardio = IsIncludeCardio;
            //            registerModel.BodyPartPrioriy = bodypartName;
            //            registerModel.SetStyle = SetStyle;
            //            if (IncrementUnit != null)
            //                registerModel.Increments = IncrementUnit.Kg;
            //            registerModel.MainGoal = mainGoal;
            //            if (IsEquipment)
            //            {
            //                var model = new EquipmentModel();

            //                if (LocalDBManager.Instance.GetDBSetting("workout_place")?.Value == "gym")
            //                {
            //                    model.IsEquipmentEnabled = true;
            //                    model.IsDumbbellEnabled = isDumbbells;
            //                    model.IsPlateEnabled = IsPlates;
            //                    model.IsPullyEnabled = IsPully;
            //                    model.IsChinUpBarEnabled = IsChinupBar;
            //                    model.Active = "gym";
            //                }
            //                else
            //                {
            //                    model.IsHomeEquipmentEnabled = true;
            //                    model.IsHomeDumbbell = isDumbbells;
            //                    model.IsHomePlate = IsPlates;
            //                    model.IsHomePully = IsPully;
            //                    model.IsHomeChinupBar = IsChinupBar;
            //                    model.Active = "home";
            //                }
            //                registerModel.EquipmentModel = model;

            //            }
            //            //RegisterModel registerModel = new RegisterModel();
            //            //registerModel.Firstname = firstname;
            //            //registerModel.EmailAddress = FBEmail;

            //            //registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;

            //            //if (LocalDBManager.Instance.GetDBSetting("Age") != null)
            //            //    registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            //            //if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
            //            //    registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
            //            //registerModel.Password = "";
            //            //registerModel.ConfirmPassword = "";

            //            uim = await DrMuscleRestClient.Instance.RegisterWithUser(registerModel);
            //            try
            //            {
            //                DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
            //                DBSetting workoutPlaceSetting = LocalDBManager.Instance.GetDBSetting("workout_place");

            //                var level = 0;
            //                if (LocalDBManager.Instance.GetDBSetting("MainLevel") != null)
            //                    level = int.Parse(LocalDBManager.Instance.GetDBSetting("MainLevel").Value);
            //                bool isSplit = LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Split");
            //                bool isGym = workoutPlaceSetting?.Value == "gym";
            //                var mo = AppThemeConstants.GetLevelProgram(level, isGym, !isSplit);
            //                if (workoutPlaceSetting?.Value == "homeBodyweightOnly")
            //                {
            //                    if (LocalDBManager.Instance.GetDBSetting("CustomMainLevel") != null && LocalDBManager.Instance.GetDBSetting("CustomMainLevel")?.Value == "1")
            //                    {
            //                        mo.workoutName = "Bodyweight 1";
            //                        mo.workoutid = 12645;
            //                        mo.programid = 487;
            //                        mo.reqWorkout = 12;
            //                        mo.programName = "Bodyweight Level 1";
            //                    }
            //                    else if (level <= 1)
            //                    {
            //                        mo.workoutName = "Bodyweight 2";
            //                        mo.workoutid = 12646;
            //                        mo.programid = 488;
            //                        mo.reqWorkout = 12;
            //                        mo.programName = "Bodyweight Level 2";
            //                    }
            //                    else if (level == 2)
            //                    {
            //                        mo.workoutName = "Bodyweight 2";
            //                        mo.workoutid = 12646;
            //                        mo.programid = 488;
            //                        mo.reqWorkout = 12;
            //                        mo.programName = "Bodyweight Level 2";
            //                    }
            //                    else if (level == 3)
            //                    {
            //                        mo.workoutName = "Bodyweight 3";
            //                        mo.workoutid = 14017;
            //                        mo.programid = 923;
            //                        mo.reqWorkout = 15;
            //                        mo.programName = "Bodyweight Level 3";
            //                    }
            //                    else if (level >= 4)
            //                    {
            //                        mo.workoutName = "Bodyweight 4";
            //                        mo.workoutid = 14019;
            //                        mo.programid = 924;
            //                        mo.reqWorkout = 15;
            //                        mo.programName = "Bodyweight Level 4";
            //                    }

            //                }
            //                LocalDBManager.Instance.SetDBSetting("recommendedWorkoutId", mo.workoutid.ToString());
            //                LocalDBManager.Instance.SetDBSetting("recommendedWorkoutLabel", mo.workoutName);
            //                LocalDBManager.Instance.SetDBSetting("recommendedProgramId", mo.programid.ToString());
            //                LocalDBManager.Instance.SetDBSetting("recommendedRemainingWorkout", mo.reqWorkout.ToString());

            //                LocalDBManager.Instance.SetDBSetting("recommendedProgramLabel", mo.programName);


            //            }
            //            catch (Exception ex)
            //            {

            //            }
            //        }
            //        LocalDBManager.Instance.SetDBSetting("email", uim.Email);
            //        LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
            //        LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
            //        LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
            //        LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
            //        LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
            //        LocalDBManager.Instance.SetDBSetting("token_expires_date", DateTime.Now.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
            //        LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
            //        LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
            //        LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
            //        LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
            //        LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); LocalDBManager.Instance.SetDBSetting("WorkoutTypeList", "0");
            //        LocalDBManager.Instance.SetDBSetting("ExerciseTypeList", "0");
            //        LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
            //        if (uim.Age != null)
            //            LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
            //        //if (uim.ReminderTime != null)
            //        //    LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
            //        //if (uim.ReminderDays != null)
            //        //    LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);

            //        LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
            //        LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
            //        LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
            //        LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
            //        LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
            //        LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
            //        LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
            //        LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");

            //        LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
            //        if (uim.IsNormalSet == null || uim.IsNormalSet == true)
            //        {
            //            LocalDBManager.Instance.SetDBSetting("SetStyle", "Normal");
            //            LocalDBManager.Instance.SetDBSetting("IsPyramid", uim.IsNormalSet == null ? "true" : "false");
            //        }
            //        else
            //        {
            //            LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
            //            LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
            //        }
            //        if (uim.Increments != null)
            //            LocalDBManager.Instance.SetDBSetting("workout_increments", uim.Increments.Kg.ToString().ReplaceWithDot());
            //        if (uim.Max != null)
            //            LocalDBManager.Instance.SetDBSetting("workout_max", uim.Max.Kg.ToString().ReplaceWithDot());
            //        if (uim.Min != null)
            //            LocalDBManager.Instance.SetDBSetting("workout_min", uim.Min.Kg.ToString().ReplaceWithDot());
            //        if (uim.BodyWeight != null)
            //        {
            //            LocalDBManager.Instance.SetDBSetting("BodyWeight", uim.BodyWeight.Kg.ToString().ReplaceWithDot());
            //        }
            //        if (uim.WarmupsValue != null)
            //        {
            //            LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
            //        }
            //        SetupEquipment(uim);
            //        ((App)Application.Current).displayCreateNewAccount = true;

            //        if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
            //            LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
            //        else
            //            LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());

            //        //await PagesFactory.PopToRootAsync(true);
            //        //await PagesFactory.PushAsync<MainAIPage>();
            //        //    App.IsWelcomeBack = true;
            //        //    App.IsDemoProgress = false;
            //        //LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
            //        //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
            //        if (IsExistingUser)
            //        {
            //            App.IsNUX = false;
            //            App.IsDemoProgress = false;
            //            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
            //            await PagesFactory.PopToRootAsync(true);
            //            return;
            //        }
            //        if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
            //            LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
            //            LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
            //            LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
            //            LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
            //        {
            //            try
            //            {
            //                long workoutTemplateId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId").Value);
            //                long pId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedProgramId").Value);
            //                var upi = new GetUserProgramInfoResponseModel()
            //                {
            //                    NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value },
            //                    RecommendedProgram = new WorkoutTemplateGroupModel() { Id = pId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
            //                };
            //                if (upi != null)
            //                {
            //                    WorkoutTemplateModel nextWorkout = upi.NextWorkoutTemplate;
            //                    if (upi.NextWorkoutTemplate.Exercises == null || upi.NextWorkoutTemplate.Exercises.Count() == 0)
            //                    {
            //                        try
            //                        {
            //                            nextWorkout = await DrMuscleRestClient.Instance.GetUserCustomizedCurrentWorkout(workoutTemplateId);
            //                            //nextWorkout = w.Workouts.First(ww => ww.Id == upi.NextWorkoutTemplate.Id);
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            await UserDialogs.Instance.AlertAsync(new AlertConfig()
            //                            {
            //                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //                                Message = AppResources.PleaseCheckInternetConnection,
            //                                Title = AppResources.ConnectionError
            //                            });
            //                            // await UserDialogs.Instance.AlertAsync(new AlertConfig()
            //                            //{
            //                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //                            //    Message = AppResources.PleaseCheckInternetConnection,
            //                            //    Title = AppResources.ConnectionError
            //                            //});
            //                            return;
            //                        }

            //                    }
            //                    App.IsNUX = false;
            //                    if (nextWorkout != null)
            //                    {
            //                        CurrentLog.Instance.CurrentWorkoutTemplate = nextWorkout;
            //                        CurrentLog.Instance.WorkoutTemplateCurrentExercise = nextWorkout.Exercises.First();
            //                        CurrentLog.Instance.WorkoutStarted = true;
            //                        if (Device.RuntimePlatform.Equals(Device.Android))
            //                        {
            //                            await PagesFactory.PopToRootThenPushAsync<KenkoDemoWorkoutExercisePage>(true);
            //                            App.IsDemoProgress = false;
            //                            App.IsWelcomeBack = true;
            //                            App.IsNewUser = true;
            //                            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
            //                            CurrentLog.Instance.Exercise1RM.Clear();
            //                            //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
            //                            Device.BeginInvokeOnMainThread(async () =>
            //                            {
            //                                await PagesFactory.PopToRootAsync(true);
            //                            });
            //                            MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
            //                        }
            //                        else
            //                        {

            //                            App.IsDemoProgress = false;
            //                            App.IsWelcomeBack = true;
            //                            App.IsNewUser = true;
            //                            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
            //                            CurrentLog.Instance.Exercise1RM.Clear();
            //                            //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
            //                            await PagesFactory.PopToRootMoveAsync(true);
            //                            await PagesFactory.PushMoveAsync<KenkoDemoWorkoutExercisePage>();
            //                            MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
            //                        }

            //                    }
            //                    else
            //                    {
            //                        await PagesFactory.PopToRootAsync(true);
            //                        App.IsDemoProgress = false;
            //                        App.IsWelcomeBack = true;
            //                        App.IsNewUser = true;
            //                        LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");

            //                    }

            //                }
            //            }
            //            catch (Exception ex)
            //            {

            //            }

            //        }
            //    }
            //    else
            //    {
            //        UserDialogs.Instance.Alert(new AlertConfig()
            //        {
            //            Message = AppResources.EmailAndPasswordDoNotMatch,
            //            Title = AppResources.UnableToLogIn,
            //            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
            //        });
            //    }
            //}
            //catch (Exception ex)
            //{

            //    await UserDialogs.Instance.AlertAsync(new AlertConfig()
            //    {
            //        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //        Message = "We are facing problem to signup with your facebook account. Please sign up with email.",
            //        Title = AppResources.Error
            //    });

            //}
        }

    }
}
