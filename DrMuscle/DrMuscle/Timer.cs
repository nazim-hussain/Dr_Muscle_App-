using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DrMuscle.Message;
using Plugin.Vibrate.Abstractions;
using Xamarin.Forms;

namespace DrMuscle
{
	public delegate void TimerChange(int remaining);
	public delegate void TimerDone();

	public class Timer
	{
		private static Timer _instance;
		public event TimerChange OnTimerChange;
		public event TimerDone OnTimerDone;
        public event TimerDone OnTimerStop;

        public string State = "STOPPED";
		public bool stopRequest = false;
		public int NextRepsCount = 0;
        public bool IsWorkTimer = false;

        private Timer()
		{
			Remaining = 60;
		}

		public static Timer Instance
		{
			get
			{
				if (_instance == null)
					_instance = new Timer();
				return _instance;
			}
		}

		public int Remaining { get; set; }

		public async Task StartTimer()
		{
			if (stopRequest)
				await Task.Run(() => { while (stopRequest) { } });
			Debug.WriteLine("StartTimer");
			Remaining = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("timer_remaining").Value);
			if (Remaining > 0)
			{
				State = "RUNNING";
                Device.OnPlatform(
                    Android: () =>
                    {
                        PCLStartTimer();
                    }, 
                    iOS: () => 
                    {
						var message = new StartTimerMessage();
						MessagingCenter.Send(message, "StartTimerMessage");
                    }
                );
				
			}
		}

        public void PCLStartTimer()
        {
			 Xamarin.Forms.Device.StartTimer(new TimeSpan(0, 0, 1), () =>
						{
							if (Remaining > 0)
							{
								Remaining--;
								//Debug.WriteLine("Remaining : " + Remaining.ToString());
								if (OnTimerChange != null)
									OnTimerChange(Remaining);
                                if (Remaining == 3)
                                    Timer321Done();
                                if (Remaining > 0 && !stopRequest)
									return true;
							}
							TimerDone();
							return false;
						});
        }

		private void DecreaseRemaining()
		{
			Remaining--;
			//Debug.WriteLine("Remaining : " + Remaining.ToString());
			if (OnTimerChange != null)
				OnTimerChange(Remaining);
            if (Remaining == 9)
                TimerPlayEmptyAudio();

            if (Remaining == 4)
                Timer321Done();

            if (Remaining > 0 && !stopRequest)
				Xamarin.Forms.Device.StartTimer(new TimeSpan(0, 0, 1), () =>
				{
					DecreaseRemaining();
					return false;
				});
			else
			{
					TimerDone();
			}
		}
        public void TimerPlayEmptyAudio()
        {
            Device.OnPlatform(
                   Android: () => {
                       var fileName = "emptyAudio.mp3";
                       if (LocalDBManager.Instance.GetDBSetting("timer_123_sound").Value == "true")
                           DependencyService.Get<IAudio>().PlayAudioFile(fileName, true, false);
                   }, iOS: () => {

                       var message = new PlayAudioFileMessage();
                       message.IsEmptyAudio = true;
                       if (LocalDBManager.Instance.GetDBSetting("timer_123_sound").Value == "true")
                           MessagingCenter.Send(message, "PlayAudioFileMessage");
                   });
        }

        public void Timer321Done()
		{
            Device.OnPlatform(
                   Android: () => {
                       var fileName = "timer123.mp3";
                       if ( LocalDBManager.Instance.GetDBSetting("timer_123_sound").Value == "true" && !Timer.Instance.IsWorkTimer)
					         DependencyService.Get<IAudio>().PlayAudioFile(fileName, true, false);
                   }, iOS: () => {

                       var message = new PlayAudioFileMessage();
					   message.Is321 = true;
                       if (LocalDBManager.Instance.GetDBSetting("timer_123_sound").Value == "true" && !Timer.Instance.IsWorkTimer)
                           MessagingCenter.Send(message, "PlayAudioFileMessage");
                   });
        }

		public void TimerDone()
		{
            try
            {

			Debug.WriteLine("TimerDone");
			if (!stopRequest)
			{
                if (LocalDBManager.Instance.GetDBSetting("timer_sound") == null)
                    LocalDBManager.Instance.SetDBSetting("timer_sound", "true");

                if (LocalDBManager.Instance.GetDBSetting("timer_reps_sound") == null)
                    LocalDBManager.Instance.SetDBSetting("timer_reps_sound", "true");
					bool isSound = LocalDBManager.Instance.GetDBSetting("timer_reps_sound").Value == "true";

					Device.OnPlatform(
                   Android: async () => {
                       if (NextRepsCount != 0)
                           await Task.Delay(700);

                       var fileName = "alarma.mp3";
                       if (LocalDBManager.Instance.GetDBSetting("timer_sound").Value == "true" || LocalDBManager.Instance.GetDBSetting("timer_reps_sound").Value == "true")
                       {

                           if (Timer.Instance.NextRepsCount <= 0 || Timer.Instance.NextRepsCount > 60)
                           {

                           }
                           else if (LocalDBManager.Instance.GetDBSetting("timer_reps_sound").Value == "true")
                           {
                               fileName = $"reps{Timer.Instance.NextRepsCount}.mp3";
                           }
                       }
                       DependencyService.Get<IAudio>().PlayAudioFile(fileName, LocalDBManager.Instance.GetDBSetting("timer_sound").Value == "true" || isSound, LocalDBManager.Instance.GetDBSetting("timer_vibrate").Value == "true");
                }, iOS: async () => {
                    if(NextRepsCount != 0)
                        await Task.Delay(700);
                    var message = new PlayAudioFileMessage();
					MessagingCenter.Send(message, "PlayAudioFileMessage");
                });
				
			}


			}
			catch (Exception ex)
			{

			}

			if (OnTimerDone != null && !stopRequest)
				OnTimerDone();

			stopRequest = false;
			State = "STOPPED";
		}

		public async Task StopTimer()
		{
            Device.OnPlatform(
                Android: async () =>
	            {
                    await PCLStopTimer();
	            },
                iOS: () =>
                {
                    var message = new StopTimerMessage();
					MessagingCenter.Send(message, "StopTimerMessage");
                }
            );
            if (OnTimerStop != null) 
            {
                OnTimerStop();
            }
        }


        public async Task StopAllTimer()
        {
            Device.OnPlatform(
                Android: async () =>
                {
                    await PCLStopTimer();
                },
                iOS: () =>
                {
                    var message = new StopTimerMessage();
                    MessagingCenter.Send(message, "StopTimerMessageOff");
                }
            );
            if (OnTimerStop != null)
            {
                OnTimerStop();
            }
        }

        public async Task PCLStopTimer()
        {
			stopRequest = true;
			await Task.Run(() => { while (stopRequest) { } });
        }
	}
}
