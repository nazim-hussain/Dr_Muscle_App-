using System;
namespace DrMuscle
{
	public interface IFirebase
	{
		void LogEvent(string key, string val);
        void SetScreenName(string val);
        void SetUserId(string name);
    }
}
