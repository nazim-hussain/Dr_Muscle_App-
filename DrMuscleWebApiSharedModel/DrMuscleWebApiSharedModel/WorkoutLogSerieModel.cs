using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class WorkoutLogSerieModel : BaseModel, IDisposable
    {
        public long Id { get; set; }
        public ExerciceModel Exercice { get; set; }
        public long? BodypartId { get; set; }
        public string UserId { get; set; }
        public DateTime LogDate { get; set; }
        public int Reps { get; set; }
        public MultiUnityWeight Weight { get; set; }

        public MultiUnityWeight OneRM { get; set; }
        public bool IsWarmups { get; set; }
        public bool Isbodyweight { get; set; }
        public bool IsPlateAvailable { get; set; }
        public bool IsDumbbellAvailable { get; set; }
        public bool IsPulleyAvailable { get; set; }
        //Todo NbPause au niveau du WorkoutLogSerieModel possiblement inutile
        public int? NbPause { get; set; }
        public int? RIR { get; set; }
        public bool IsOneHanded { get; set; }
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose of managed resources here
                    //eventSource.SomeEvent -= OnSomeEvent;
                }

                // Dispose of unmanaged resources here

                disposed = true;
            }
        }
    }
}
