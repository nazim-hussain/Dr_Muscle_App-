//using DrMaxMuscleWebApi.Repository;
using DrMuscleWebApiSharedModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CATest
{
    class Program
    {
        public static decimal RoundDownToNearest(decimal numToRound, decimal step)
        {
            

            if (step == 0)
                return numToRound;

            //Calc the floor value of numToRound
            decimal floor = ((long)(numToRound / step)) * step;

            //round up if more than half way of step
            decimal round = floor;
            decimal remainder = numToRound - floor;
            if (remainder >= 0)
                round += step;

            return round;

        }

		public static string HashPassword(string password)
		{
			byte[] salt;
			byte[] buffer2;
			if (password == null)
			{
				throw new ArgumentNullException("password");
			}
			using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
			{
				salt = bytes.Salt;
				buffer2 = bytes.GetBytes(0x20);
			}
			byte[] dst = new byte[0x31];
			Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
			Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
			return Convert.ToBase64String(dst);
		}

        static void Main(string[] args)
        {
            RecommendationModel result = GetRecommendation(3, 20, 20, 20, 10, 9, 8);
        }

        public static RecommendationModel GetRecommendation(int LastWorkoutNbSeriesForExercise,
                                                        decimal Weight0,
                                                        decimal Weight1,
                                                        decimal Weight2,
                                                        int Reps0,
                                                        int Reps1,
                                                        int Reps2
                                                        )
        {
            // Calcul du 1RM pour les deux précédent sets
            decimal OneRM0 = ComputeOneRM(Weight0, Reps0);
            decimal OneRM1 = ComputeOneRM(Weight1, Reps1);
            decimal OneRM2 = ComputeOneRM(Weight2, Reps2);

            // Calcul du progres en % entre les deux dernier workout
            RecommendationModel result = new RecommendationModel();
            result.OneRMProgress = (OneRM0 - OneRM1) * 100 / OneRM1;

            // REPS
            // En fonction du nombre de reps au dernier workout

            int nbReps = 0;

            if (Reps0 > 11)
                nbReps = 6;

            if (Reps0 == 6)
                nbReps = 9;

            if (Reps0 == 9)
                nbReps = 11;

            if (Reps0 == 11)
                nbReps = 7;

            if (Reps0 == 7)
                nbReps = 10;

            if (Reps0 == 10)
                nbReps = 5;

            if (Reps0 < 6)
                nbReps = 8;

            if (Reps0 == 8)
                nbReps = 12;

            result.Reps = nbReps;

            // POIDS (EN KG)
            // On calcule la progression relative entre le 1RM le plus récent (OneRM0) et le précédent (OneRM1)
            decimal oneRMRelativeProgress = (OneRM0 - OneRM1) / OneRM0;

            // On limite cette progression entre 1% et 5%
            // Si le progress en % entre les deux dernier workout est inférieur à 1 %, on le fixe à 1 %

            if (oneRMRelativeProgress < (decimal)0.01)
                oneRMRelativeProgress = (decimal)0.01;

            // Si le progress en % entre les deux dernier workout est supérieur à 5 %, on le fixe à 5 %
            if (oneRMRelativeProgress > (decimal)0.05)
                oneRMRelativeProgress = (decimal)0.05;

            // On calcule l'augmentation (increase) prévue pour le 1RM aujourd'hui
            decimal OneRMIncrease = OneRM0 * oneRMRelativeProgress;

            // On calcule le 1RM prévu pour aujourd'hui (OneRM00) en additionnant le 1RM le plus récent (OneRM0) et l'augmentation (increase) prévue pour le 1RM aujourd'hui
            decimal OneRM00 = OneRM0 + OneRMIncrease;

            // On calcule le poids recommandé aujourd'hui à partir du 1RM prévu pour aujourd'hui (OneRM00)			
            // Mayhew
            decimal percent = (decimal)(52.2 + 41.9 * Math.Exp(-0.055 * nbReps));
            decimal recommendationInKg = OneRM00 * percent / 100;

            recommendationInKg = Math.Ceiling(recommendationInKg);
            result.Weight = new MultiUnityWeight(recommendationInKg, WeightUnities.kg);

            // SÉRIES			
            // En fonction du nombre de reps au dernier workout (ajustement +1 série aux ~3 workouts)
            if (result.Reps == 5 || result.Reps == 6 || result.Reps == 7)
            {
                result.Series = LastWorkoutNbSeriesForExercise + 1;
            }
            else
            {
                result.Series = LastWorkoutNbSeriesForExercise;
            }
            // Fin en fonction du nombre de reps au dernier workout (ajustement +1 série aux ~3 workouts)


            // Ajustement des séries en fonction du progres réalisé entre les 2 derniers workouts
            if ((double)result.OneRMProgress >= 4)
            {
                result.Series = result.Series + 1;
            }
            else
            {
                if ((double)result.OneRMProgress >= 0 && (double)result.OneRMProgress < 4)
                {
                    result.Series = result.Series;
                }
                else
                {
                    if ((double)result.OneRMProgress >= 0 && result.OneRMProgress < 2)
                    {
                        result.Series = result.Series;
                    }
                    else
                    {
                        if ((double)result.OneRMProgress < 0)
                        {
                            result.Series = LastWorkoutNbSeriesForExercise / 2;

                            // Deload: si le 1RM a baissé, je mets 90%. S'il a baissé depuis 2 workouts, je mets 105%			
                            if (OneRM0 < OneRM2 && OneRM1 < OneRM2)
                            {
                                recommendationInKg = recommendationInKg * (decimal)1.05;
                            }
                            else
                            {
                                recommendationInKg = recommendationInKg * (decimal)0.90;
                            }
                            recommendationInKg = Math.Ceiling(recommendationInKg);
                            result.Weight = new MultiUnityWeight(recommendationInKg, WeightUnities.kg);
                        }
                    }
                }
            }
            // Fin ajustement des séries en fonction du progres réalisé entre les 2 derniers workouts

            // Protection nombre de series < 2 et > 5
            if (result.Series < 2)
                result.Series = 2;
            if (result.Series > 5)
                result.Series = 5;
            // Fin protection nombre de series < 2 et > 5
            return result;
        }

        public static decimal ComputeOneRM(decimal weight, int reps)
        {
            ////////// Mayhew ////////////
            return (decimal)(100 * weight) / (decimal)(52.2 + 41.9 * Math.Exp(-0.055 * reps));
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
