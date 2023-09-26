using System;
using Xamarin.Forms;
using System.IO;
using Foundation;
using AVFoundation;
using DrMuscle.iOS;
using Plugin.Vibrate;

[assembly: Dependency(typeof(AudioService))]
namespace DrMuscle.iOS
{
	public class AudioService : IAudio
	{
		public AudioService()
		{
		}

		public void PlayAudioFile(string fileName, bool sound, bool vibrate)
		{
			if (sound)
			{
				string sFilePath = NSBundle.MainBundle.PathForResource
				(Path.GetFileNameWithoutExtension(fileName), Path.GetExtension(fileName));
				var url = NSUrl.FromString(sFilePath);
				var _player = AVAudioPlayer.FromUrl(url);
				_player.FinishedPlaying += (object sender, AVStatusEventArgs e) =>
				{
					_player = null;
				};
				_player.Play();
			}
			if (vibrate)
				CrossVibrate.Current.Vibration(TimeSpan.FromSeconds(3));
		}
	}
}
