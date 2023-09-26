using DrMuscleWebApiSharedModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CATest
{
    public class Algo
    {
        public RecommendationModel GetRecommendation(int LastWorkoutNbSeriesForExercise,
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

            // Calcul du progres en % entre les deux dernier workout
            RecommendationModel result = new RecommendationModel();
            result.OneRMProgress = (OneRM0 - OneRM1) * 100 / OneRM1;

            // Calcul du nombre de reps de la recommendation
            // NOUVEAU CODE
            // En fonction du nombre de reps au dernier workout

            int nbReps = 0;

            if (Reps0 > 11)
                nbReps = 6;

            if (Reps0 == 6)
                nbReps = 9;

            if (Reps0 == 9)
                nbReps = 11;

            if (Reps0 == 11)
                nbReps = 5;

            if (Reps0 == 5)
                nbReps = 8;

            if (Reps0 == 8)
                nbReps = 10;

            if (Reps0 == 10)
                nbReps = 4;

            if (Reps0 < 5)
                nbReps = 7;

            if (Reps0 == 7)
                nbReps = 12;

            result.Reps = nbReps;

            // FIN DU NOUVEAU CODE

            // Calcul du poids (en Kg) de la recommendation
            decimal percent = (decimal)(102.78 - 2.78 * nbReps);
            decimal recommendationInKg = OneRM0 * percent / 100;
            decimal oneRMRelativeProgress = (OneRM0 - OneRM1) / OneRM0;

            // Code pour limiter le progress à entre 1.1% et 10%

            // Si le progres en % entre les deux dernier workout est inférieur à 1.1 %, on le fixe à 1.1 %

            if (oneRMRelativeProgress < (decimal)0.011)
                oneRMRelativeProgress = (decimal)0.011;

            // Si le progres en % entre les deux dernier workout est supérieur à 5 %, on le fixe à 5 %

            if (oneRMRelativeProgress > (decimal)0.05)
                oneRMRelativeProgress = (decimal)0.05;

            // FIN DU NOUVEAU CODE

            recommendationInKg = recommendationInKg + (recommendationInKg * (oneRMRelativeProgress * (decimal)0.9));
            recommendationInKg = Math.Floor(recommendationInKg);
            result.Weight = new MultiUnityWeight(recommendationInKg, WeightUnities.kg);

            // Calcul du nombre de séries de recommendation 
            // En fonction du progres réalisé entre les
            if ((double)result.OneRMProgress >= 7.5)
            {
                result.Series = LastWorkoutNbSeriesForExercise + 1;
            }
            else
            {
                if ((double)result.OneRMProgress >= 2 && (double)result.OneRMProgress < 7.5)
                {
                    result.Series = LastWorkoutNbSeriesForExercise;
                }
                else
                {
                    if ((double)result.OneRMProgress >= 0.1 && result.OneRMProgress < 2)
                    {
                        result.Series = LastWorkoutNbSeriesForExercise - 1;
                        if (result.Series == 0)
                            result.Series = 1;
                    }
                    else
                    {
                        if ((double)result.OneRMProgress < 0.1)
                        {
                            result.Series = LastWorkoutNbSeriesForExercise / 2;
                        }
                    }
                }
            }
            return result;
        }

        public decimal ComputeOneRM(decimal weight, int reps)
        {
            return (decimal)(weight / (decimal)(1.0278 - (0.0278 * reps)));
        }
    }
}
