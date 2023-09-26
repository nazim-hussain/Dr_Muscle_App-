using System;
namespace DrMuscle.Message
{
	public class LoadNextExercise
	{
		public LoadNextExercise()
		{
		}
	}

    public class LoadNormalExercise
    {
        public long exerciseId { get; set; }
        public bool isReloadReco { get; set; }
        public LoadNormalExercise()
        {
        }
    }
}

