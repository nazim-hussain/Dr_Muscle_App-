using System;
using SQLite;
using Xamarin.Forms;
using DrMuscle.Dependencies;

namespace DrMuscle
{
    public class LocalDBManager
    {
        private SQLiteConnection database;
        private static LocalDBManager instance;
        public static LocalDBManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new LocalDBManager();
                return instance;
            }
        }
        private LocalDBManager()
        {
            database = DependencyService.Get<ISQLite>().GetConnection();
            InitDatabase();
        }

        private void InitDatabase()
        {
            if (!TableExists<DBSetting>())
            {
                database.CreateTable<DBSetting>();
            }

            if (!TableExists<DBReco>())
            {
                database.CreateTable<DBReco>();
            }
        }

        private bool TableExists<T>()
        {
            const string cmdText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";
            var cmd = database.CreateCommand(cmdText, typeof(T).Name);
            return cmd.ExecuteScalar<string>() != null;
        }

        public DBSetting GetDBSetting(string key)
        {
            return database.Table<DBSetting>().FirstOrDefault(s => s.Key == key);
        }

        public DBReco GetDBReco(string key)
        {
            return database.Table<DBReco>().FirstOrDefault(s => s.Key == key);
        }

        public void SetDBSetting(string key, string value)
        {
            SetDBSetting(new DBSetting() { Key = key, Value = value });
        }

        public void SetDBSetting(DBSetting setting)
        {
            if (database.Table<DBSetting>().Count(x => x.Key == setting.Key) == 0)
                database.Insert(setting);
            else
                database.Update(setting);
        }

        public void SetDBReco(string key, string value)
        {
            SetDBReco(new DBReco() { Key = key, Value = value });
        }
        public void SetDBReco(DBReco reco)
        {
            if (database.Table<DBReco>().Count(x => x.Key == reco.Key) == 0)
                database.Insert(reco);
            else
                database.Update(reco);
        }

        internal void ResetReco()
        {
            if (!TableExists<DBReco>())
                database.DropTable<DBReco>();
            InitDatabase();
        }

        internal void Reset()
        {
			bool onBoardingSeen = GetDBSetting("onboarding_seen") != null;
            string recommendedWorkoutId = GetDBSetting("recommendedWorkoutId") != null ? GetDBSetting("recommendedWorkoutId").Value : "";
            string recommendedWorkoutLabel = GetDBSetting("recommendedWorkoutLabel") != null ? GetDBSetting("recommendedWorkoutLabel").Value : "";
            string recommendedProgramId = GetDBSetting("recommendedProgramId") != null ? GetDBSetting("recommendedProgramId").Value : "";
            string recommendedProgramLabel = GetDBSetting("recommendedProgramLabel") != null ? GetDBSetting("recommendedProgramLabel").Value : "";
            string recommendedRemainingWorkout = GetDBSetting("recommendedRemainingWorkout") != null ? GetDBSetting("recommendedRemainingWorkout").Value : "";
            string environment = GetDBSetting("Environment") != null ? GetDBSetting("Environment").Value : "Production";
            string language = GetDBSetting("AppLanguage") != null ? GetDBSetting("AppLanguage").Value : "en";
            string appBackground = "DrMuscleLogo.png";
            database.DropTable<DBSetting>();
            if (!TableExists<DBReco>())
                database.DropTable<DBReco>();
			InitDatabase();
            if (onBoardingSeen)
            {
                SetDBSetting("onboarding_seen", "true");
                //SetDBSetting("recommendedWorkoutId", recommendedWorkoutId == "" ? null : recommendedWorkoutId);
                SetDBSetting("recommendedWorkoutLabel", recommendedWorkoutLabel);
                SetDBSetting("recommendedProgramId", recommendedProgramId);
                SetDBSetting("recommendedProgramLabel", recommendedProgramLabel);
                SetDBSetting("recommendedRemainingWorkout", recommendedRemainingWorkout);
                
                SetDBSetting("AppLanguage", language);
                SetDBSetting("BackgroundImage", appBackground);
            }
            LocalDBManager.Instance.SetDBSetting("onboarding_features", "true");
            SetDBSetting("Environment", environment);
            var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
            LocalDBManager.Instance.SetDBSetting("PlatesKg", kgString);

            var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
            LocalDBManager.Instance.SetDBSetting("PlatesLb", lbString);
            LocalDBManager.Instance.SetDBSetting("timer_remaining", "60");
            //if (!string.IsNullOrEmpty(firstname))
            //    SetDBSetting("firstname", firstname);
            //instance = new LocalDBManager();
        }
    }
}