using System;
using Xamarin.Forms;
using Android.Media;
using Android.Content.Res;
using DrMuscle.Droid;
using Plugin.Vibrate;

[assembly: Dependency(typeof(AudioService))]
namespace DrMuscle.Droid
{
	public class AudioService : IAudio
	{
		MediaPlayer player;
        public AudioService()
		{
		}

		public void PlayAudioFile(string fileName, bool sound, bool vibrate)
		{
			if (sound)
			{
                
                player = new MediaPlayer();
				var fd = global::Android.App.Application.Context.Assets.OpenFd(fileName);
				player.Prepared += (s, e) =>
				{
					player.Start();
				};
				player.SetDataSource(fd.FileDescriptor, fd.StartOffset, fd.Length);
				player.Prepare();
                try
                {
                    player.Completion += (sender, e) => {
						try
						{
                            player.Release();
                            player = null;
                        }
						catch (Exception ex)
						{

						}
                    };
                }
                catch (Exception ex)
                {

                }
            }

			if (vibrate)
				CrossVibrate.Current.Vibration(TimeSpan.FromSeconds(3));
		}
	}
}
