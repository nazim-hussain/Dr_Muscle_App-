using System;
using System.Collections.Generic;
using System.Linq;
using DrMuscle.Layout;
using DrMuscleWebApiSharedModel;
using static SQLite.SQLite3;

namespace DrMuscle.Constants
{
    public static class RecoComputation
    {
       
        public static decimal ComputeOneRM(decimal weight, int reps)
        {
            // Mayhew
            //return (100 * weight) / (decimal)(52.2 + 41.9 * Math.Exp(-0.055 * reps));
            // Epey
            return (decimal)(AppThemeConstants.Coeficent * reps) * weight + weight;
        }

        public static MultiUnityWeight GetPlatesWeight(string availablePlates, decimal weight, double bar, bool isKg)
        {

            var finals = weight - (decimal)bar;

            var platesItems = new List<AvailablePlateModel>();
            // calculating total weight and the difference
            var keyVal = availablePlates;
            if (weight < (decimal)bar)
            {
                return new MultiUnityWeight((decimal)bar, isKg ? "kg" : "lb");
            }
            string[] items = keyVal.Split('|');
            foreach (var item in items)
            {
                string[] pair = item.Split('_');
                var model = new AvailablePlateModel();
                if (pair.Length == 3)
                {
                    model.Key = pair[0];
                    try
                    {
                        model.Weight = double.Parse(pair[0]);
                    }
                    catch (Exception ex)
                    {
                        model.Weight = 0;
                    }
                    model.Value = Int32.Parse(pair[1]);
                    model.IsSystemPlates = pair[2] == "True" ? true : false;
                    if (model.Value != 0)
                        platesItems.Add(model);
                }
            }
            platesItems.Sort(delegate (AvailablePlateModel c1, AvailablePlateModel c2) { return c2.Weight.CompareTo(c1.Weight); });

            for (var i = 0; i < platesItems.Count; i++)
            {
                platesItems[i].CalculatedPlatesCount = 0;
                for (var a = 1; a <= platesItems[i].Value / 2; a++)
                {
                    if (finals >= (decimal)(platesItems[i].Weight * 2))
                    {
                        platesItems[i].CalculatedPlatesCount++;
                        platesItems[i].isAvailable = true;
                        finals -= (decimal)(platesItems[i].Weight * 2);
                    }
                }
            }

            var platesWeight = 0.0;
            for (int i = 0; i < platesItems.Count(); i++)
            {
                for (int j = 0; j < platesItems[i].CalculatedPlatesCount; j++)
                {
                    platesWeight += platesItems[i].Weight;

                }
            }
            var totalWeight = bar + (platesWeight * 2);
            if (platesWeight == 0 && bar == 0 && platesItems.Count()>0)
            {
                totalWeight = platesItems.Last().Weight;
            }
            return new MultiUnityWeight((decimal)totalWeight, isKg ? "kg" : "lb");

        }

        public static MultiUnityWeight GetDumbbellWeight(string availableDumbbells, decimal weight, bool isKg)
        {

            var finals = weight;

            var dumbbellItems = new List<AvailablePlateModel>();
            // calculating total weight and the difference
            var keyVal = availableDumbbells;

            string[] items = keyVal.Split('|');
            foreach (var item in items)
            {
                string[] pair = item.Split('_');
                var model = new AvailablePlateModel();
                if (pair.Length == 3)
                {
                    model.Key = pair[0];
                    try
                    {
                        model.Weight = double.Parse(pair[0]);
                    }
                    catch (Exception ex)
                    {
                        model.Weight = 0;
                    }
                    model.Value = Int32.Parse(pair[1]);
                    if (model.Value > 1)
                        model.Value = 1;
                    model.IsSystemPlates = pair[2] == "True" ? true : false;
                    if (model.Value != 0)
                        dumbbellItems.Add(model);
                }
            }
            dumbbellItems.Sort(delegate (AvailablePlateModel c1, AvailablePlateModel c2) { return c2.Weight.CompareTo(c1.Weight); });
            var isDumbbellSelected = false;
            for (var i = 0; i < dumbbellItems.Count; i++)
            {
                dumbbellItems[i].CalculatedPlatesCount = 0;
                for (var a = 1; a <= dumbbellItems[i].Value; a++)
                {
                    if (finals >= (decimal)(dumbbellItems[i].Weight))
                    {
                        dumbbellItems[i].CalculatedPlatesCount++;
                        dumbbellItems[i].isAvailable = true;
                        finals -= (decimal)(dumbbellItems[i].Weight);
                        isDumbbellSelected = true;
                        break;
                    }
                }
                if (isDumbbellSelected)
                    break;
            }

            var platesWeight = 0.0;
            for (int i = 0; i < dumbbellItems.Count(); i++)
            {
                for (int j = 0; j < dumbbellItems[i].CalculatedPlatesCount; j++)
                {
                    platesWeight += dumbbellItems[i].Weight;

                }
            }
            var totalWeight = platesWeight;
            if (platesWeight == 0)
            {
                totalWeight = dumbbellItems.Last().Weight;
            }
            return new MultiUnityWeight((decimal)totalWeight, isKg ? "kg" : "lb");

        }

        public static decimal RoundToNearestIncrement(decimal numToRound, decimal Increments, decimal? min, decimal? max)
        {
            if (Increments == 0)
            {
                return numToRound;
            }

            if (min != null)
            {
                var numAdjustedForMin = (decimal)min;
                while (numAdjustedForMin < numToRound)
                {
                    numAdjustedForMin += Increments;
                }
                var numRounded = numAdjustedForMin;

                if (max != null)
                {
                    if (numRounded > max)
                        numRounded = (decimal)max;
                }

                return numRounded;
            }
            else
            {
                //Calc the floor value of numToRound
                decimal floor = ((long)(numToRound / Increments)) * Increments;

                //Round up if more than 50% way to Increments
                decimal numRounded = floor;
                decimal remainder = numToRound - floor;
                if (remainder > (Increments * (decimal)0.50))
                    numRounded += Increments;

                if (max != null)
                {
                    if (numRounded > max)
                        numRounded = (decimal)max;
                }

                return numRounded;
            }

        }

        public static decimal RoundDownNearestIncrement(decimal numToRound, decimal Increments, decimal? min, decimal? max)
        {
            if (Increments == 0)
            {
                return numToRound;
            }

            if (min != null)
            {
                var numAdjustedForMin = (decimal)min;
                while (numAdjustedForMin < numToRound)
                {
                    numAdjustedForMin += Increments;
                }
                var numRounded = numAdjustedForMin;

                if (max != null)
                {
                    if (numRounded > max)
                        numRounded = (decimal)max;
                }

                return numRounded;
            }
            else
            {
                //Calc the floor value of numToRound
                decimal floor = ((long)(numToRound / Increments)) * Increments;

                //Round up if more than 50% way to Increments
                decimal numRounded = floor;
                decimal remainder = numToRound - floor;
                if (max != null)
                {
                    if (numRounded > max)
                        numRounded = (decimal)max;
                }

                return numRounded;
            }

        }

        public static decimal RoundToNearestIncrementPyramid(decimal numToRound, decimal Increments, decimal? min, decimal? max)
        {
            
            if (min != null)
            {
                var numAdjustedForMin = (decimal)min;
                while (numAdjustedForMin < numToRound-Increments)
                {
                    numAdjustedForMin += Increments;
                }

                var numRounded = numAdjustedForMin;
                if (numRounded > numToRound)
                {
                    if (numRounded - Increments >= min)
                        numRounded = numRounded - Increments;
                }
                if (max != null)
                {
                    if (numRounded > max)
                        numRounded = (decimal)max;
                }
                if (numRounded == 3)
                {

                }
                if (numRounded < min)
                {
                    numRounded = (decimal)min;
                }
                return numRounded;
            }
            else
            {
                var backup = numToRound;
                //Calc the floor value of numToRound
                decimal floor = ((long)(numToRound / Increments)) * Increments;

                //Round up if more than 50% way to Increments
                decimal numRounded = floor;
                decimal remainder = numToRound - floor;
                //if (remainder > (Increments * (decimal)0.50))
                //    numRounded += Increments;
                

                if (max != null)
                {
                    if (numRounded > max)
                        numRounded = (decimal)max;
                }
                if (backup == numRounded)
                {
                     if (numRounded - Increments > 0)
                        return numRounded - Increments;
                }
                if (numRounded < 0)
                    numRounded = 2;
                return numRounded;
            }

        }

        public static bool IsWeightedExercise(long exerciceId)
        {
            if (exerciceId == 18627 || exerciceId == 18628 || exerciceId == 21234 || exerciceId == 862 || exerciceId == 863 || exerciceId == 6992 || exerciceId == 6993 || exerciceId == 13446 || exerciceId == 13449 || exerciceId == 14297)
                return true;
            return false;
        }

        public static int ComputeReps(MultiUnityWeight weight, decimal OneRM0)
        {
            /*
            //Mayhew for reps
            //Need to fix here:
            //return (int)((decimal)18.18 * (((decimal)41.9 * OneRM0) / (decimal)((decimal)100 * weight.Kg - (decimal)52.2)));
            try
            {
                var calc = ((decimal)41.9 * OneRM0) / ((decimal)100 * weight.Kg - ((decimal)52.2 * (decimal)OneRM0));
                var log = Math.Log((double)calc);
                if (log < 0)
                    return 0;
                var reps = (int)Math.Abs((decimal)18.181818181818181818181818181818 * (decimal)log);

                System.Diagnostics.Debug.WriteLine($"Calculation= {calc}");
                System.Diagnostics.Debug.WriteLine($"Log= {log}");
                System.Diagnostics.Debug.WriteLine($"OneRM= {OneRM0}");
                System.Diagnostics.Debug.WriteLine($"Weight= {weight.Kg}");
                System.Diagnostics.Debug.WriteLine($"Reps= {reps}");
                System.Diagnostics.Debug.WriteLine($"==================");
               
                return reps;
            }
            catch (Exception ex)
            {
                return (int)OneRM0;
            }
            */

            //return (int)((double)18.181818181818 * Math.Log(((double)41.9 * (double)OneRM0)/((double)(100 * Convert.ToDecimal(weight.Kg)) - ((double)52.2 * (double)OneRM0))));

            //Epley for reps
            try
            {
                var reps = (int)((OneRM0 - weight.Kg) / (AppThemeConstants.Coeficent * weight.Kg))+1;

                /*
                System.Diagnostics.Debug.WriteLine($"Calculation= {calc}");
                System.Diagnostics.Debug.WriteLine($"Log= {log}");
                System.Diagnostics.Debug.WriteLine($"OneRM= {OneRM0}");
                System.Diagnostics.Debug.WriteLine($"Weight= {weight.Kg}");
                System.Diagnostics.Debug.WriteLine($"Reps= {reps}");
                System.Diagnostics.Debug.WriteLine($"==================");
                */
                return reps;
            }
            catch (Exception ex)
            {
                return (int)OneRM0;
            }

            //return (int)((double)18.181818181818 * Math.Log(((double)41.9 * (double)OneRM0)/((double)(100 * Convert.ToDecimal(weight.Kg)) - ((double)52.2 * (double)OneRM0))));
        }


        public static RecommendationModel GetNormalDeload(RecommendationModel reco, string availablePlates = "", bool iskg = true)
        {
            var incrementValue = reco.Increments != null ? (decimal)reco.Increments.Kg : 1;
            decimal? min = null, max = null;
            if (reco.Min != null)
                min = reco.Min.Kg;
            if (reco.Max != null)
                max = reco.Max.Kg;
            double? sliderVal = null;
            try
            {

                if (iskg)
                {
                    sliderVal = 20;
                    if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("KgBarWeight")?.Value))
                    {
                        sliderVal = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("KgBarWeight")?.Value, System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    sliderVal = 45;
                    if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("LbBarWeight")?.Value))
                    {
                        sliderVal = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("LbBarWeight")?.Value, System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            if (!reco.IsBodyweight)
            {
                if (reco.Increments != null || reco.Max != null || reco.Min != null)
                {
                    //Put recommendationInKg through RoundToNearestIncrement and return that result as result.Weight 
                    reco.Weight = new MultiUnityWeight(RoundDownNearestIncrement(reco.RecommendationInKg, incrementValue, min, max), WeightUnities.kg);
                }
                else
                    reco.Weight = new MultiUnityWeight(reco.RecommendationInKg, WeightUnities.kg);
            }
            if (reco.isPlateAvailable && !string.IsNullOrEmpty(availablePlates))
            {
                reco.Weight = GetPlatesWeight(availablePlates, iskg ? reco.Weight.Kg : reco.Weight.Lb, (double)sliderVal, iskg);
            }
            if (reco.isDumbbellAvailable && !string.IsNullOrEmpty(availablePlates))
            {
                reco.Weight = GetDumbbellWeight(availablePlates, iskg ? reco.Weight.Kg : reco.Weight.Lb, iskg);
            }
            if (reco.isPulleyAvailable && !string.IsNullOrEmpty(availablePlates))
            {
                reco.Weight = GetPlatesWeight(availablePlates, iskg ? reco.Weight.Kg : reco.Weight.Lb, 0, iskg);
            }
            reco.Series = reco.Series / 2;

            if (reco.Series < 2)
                reco.Series = 2;
            if (reco.Series > 5)
                reco.Series = 5;
            if (reco.IsMedium)
                reco.Series = 2;
            if (reco.IsReversePyramid)
            {
                reco.NbPauses = 2;
                reco.Series = 0;
            }
            if (reco.IsBodyweight)
            {
                reco.Reps = (int)Math.Floor((reco.Reps - (reco.Reps * 0.2)));
                if (reco.FirstWorkSet1RM != null && ComputeOneRM(reco.Weight.Kg, reco.Reps) >  reco.FirstWorkSet1RM.Kg)
                {
                    reco.Reps = reco.Reps - 1;
                }
            }

            if (reco.WarmUpsList.Count > 0)
            {
                try
                {
                    var WarmupsValue = reco.WarmUpsList.Count > 2 ? 2 : reco.WarmUpsList.Count;
                    var IsBodyweight = reco.IsBodyweight;

                    List<WarmUps> WarmUpsList = new List<WarmUps>();
                    var intialWeight = reco.Weight.Kg / 2;
                    var newWarmup = WarmupsValue > 1 ? WarmupsValue - 1 : WarmupsValue;

                    var weightIncrement = (((reco.Weight.Kg * (decimal)0.85) -
(reco.Weight.Kg * (decimal)0.5)) / (int)newWarmup);

                    var initialReps = (decimal)reco.Reps * (decimal)0.75;

                    if (IsBodyweight && WarmupsValue == 1)
                        initialReps = (decimal)reco.Reps * (decimal)0.675;
                    var repsIncrement = IsBodyweight ? (WarmupsValue == 1 ? reco.Reps * (decimal)0.675 : reco.Reps * (decimal)0.75 - reco.Reps * (decimal)0.5) / newWarmup : (reco.Reps * (decimal)0.75 - reco.Reps * (decimal)0.4) / newWarmup;
                    if (IsBodyweight == false && initialReps < (decimal)5.01)
                    {
                        if (!reco.isPlateAvailable)
                            initialReps = 6;
                    }
                    if (reco.isPlateAvailable && initialReps < 1)
                        initialReps = 1;
                    var warmupCount = 0;

                    while (warmupCount < (int)WarmupsValue)
                    {
                        var newWarmsup = new WarmUps()
                        {
                            WarmUpWeightSet = IsBodyweight ? reco.Weight : new MultiUnityWeight(RoundDownNearestIncrement(intialWeight + (weightIncrement * warmupCount), incrementValue, min, max), WeightUnities.kg),
                            WarmUpReps = IsBodyweight ? (int)Math.Ceiling(initialReps - ((decimal)repsIncrement * ((decimal)WarmupsValue - (decimal)(warmupCount + 1)))) : (int)Math.Ceiling(initialReps - ((decimal)repsIncrement * (decimal)warmupCount))
                        };
                        if (!IsBodyweight)
                            newWarmsup.WarmUpReps = newWarmsup.WarmUpReps < 3 ? 3 : newWarmsup.WarmUpReps;
                        if ((reco.isDumbbellAvailable || reco.isPulleyAvailable) && !reco.IsBodyweight)
                        {
                            newWarmsup.WarmUpWeightSet = GetDumbbellWeight(availablePlates, iskg ? newWarmsup.WarmUpWeightSet.Kg : newWarmsup.WarmUpWeightSet.Lb, iskg);
                        }

                        if (reco.isPulleyAvailable && !reco.IsBodyweight)
                        {
                            newWarmsup.WarmUpWeightSet = GetPlatesWeight(availablePlates, iskg ? newWarmsup.WarmUpWeightSet.Kg : newWarmsup.WarmUpWeightSet.Lb, 0, iskg);
                        }
                        WarmUpsList.Add(newWarmsup);
                        warmupCount++;
                    }
                    reco.WarmUpsList = WarmUpsList;
                }
                catch (Exception ex)
                {

                }

            }
            if (reco.BackOffSetWeight != null)
                reco.BackOffSetWeight = new MultiUnityWeight(RoundToNearestIncrement(reco.Weight.Kg - ((reco.Weight.Kg * (decimal)30) / 100), incrementValue, min, max), "kg");

            return reco;
        }

        public static RecommendationModel GetRestPauseDeload(RecommendationModel reco, string availablePlates = "", bool iskg = true)
        {
            var incrementValue = reco.Increments != null ? (decimal)reco.Increments.Kg : 1;
            decimal? min = null, max = null;
            if (reco.Min != null)
                min = reco.Min.Kg;
            if (reco.Max != null)
                max = reco.Max.Kg;
            double? sliderVal = null;
            try
            {
                
                if (iskg)
                {
                    sliderVal = 20;
                    if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("KgBarWeight")?.Value))
                    {
                        sliderVal = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("KgBarWeight")?.Value, System.Globalization.CultureInfo.InvariantCulture);
                    }    
                }
                else
                {
                    sliderVal = 45;
                    if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("LbBarWeight")?.Value))
                    {
                        sliderVal = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("LbBarWeight")?.Value, System.Globalization.CultureInfo.InvariantCulture);
                    }   
                }
            }
            catch (Exception ex)
            {

            }
            if (!reco.IsBodyweight)
            { 
            if (reco.Increments != null || reco.Max != null || reco.Min != null)
            {
                //Put recommendationInKg through RoundToNearestIncrement and return that result as result.Weight 
                
                reco.Weight = new MultiUnityWeight(RoundDownNearestIncrement(reco.RecommendationInKg, incrementValue, min, max), WeightUnities.kg);
            }
            else
                reco.Weight = new MultiUnityWeight(reco.RecommendationInKg, WeightUnities.kg);
            }
            if (reco.isPlateAvailable && !string.IsNullOrEmpty(availablePlates))
            {
                reco.Weight = GetPlatesWeight(availablePlates, iskg ? reco.Weight.Kg : reco.Weight.Lb, (double)sliderVal, iskg);
            }
            if ((reco.isDumbbellAvailable ) && !string.IsNullOrEmpty(availablePlates))
            {
                reco.Weight = GetDumbbellWeight(availablePlates, iskg ? reco.Weight.Kg : reco.Weight.Lb, iskg);
            }
            if ((reco.isPulleyAvailable) && !string.IsNullOrEmpty(availablePlates))
            {
                reco.Weight = GetPlatesWeight(availablePlates, iskg ? reco.Weight.Kg : reco.Weight.Lb, 0, iskg);
            }
            reco.NbPauses = 1;
            if (reco.IsReversePyramid)
            {
                reco.NbPauses = 2;
                reco.Series = 0;
            }
            if (reco.NbRepsPauses <= 0)
                reco.NbRepsPauses = 1;

            if (reco.IsBodyweight)
            {
                reco.Reps = (int)Math.Floor((reco.Reps - (reco.Reps * 0.2)));
                if (reco.FirstWorkSet1RM != null && ComputeOneRM(reco.Weight.Kg, reco.Reps) > reco.FirstWorkSet1RM.Kg)
                {
                    reco.Reps = reco.Reps - 1;
                }
            }
                if (reco.WarmUpsList.Count > 0)
            {
                var WarmupsValue = reco.WarmUpsList.Count > 2 ? 2 : reco.WarmUpsList.Count; ;
                var IsBodyweight = reco.IsBodyweight;
                try
                {

                    List<WarmUps> WarmUpsList = new List<WarmUps>();

                    var intialWeight = reco.Weight.Kg / 2;
                    var newWarmup = WarmupsValue > 1 ? WarmupsValue - 1 : WarmupsValue;
                    var weightIncrement = (((reco.Weight.Kg * (decimal)0.85) - (reco.Weight.Kg * (decimal)0.5)) / (int)newWarmup);
                    
                    var initialReps = (decimal)reco.Reps * (decimal)0.75;

                    if (IsBodyweight && WarmupsValue == 1)
                        initialReps = (decimal)reco.Reps * (decimal)0.675;
                    var repsIncrement = IsBodyweight ? (WarmupsValue == 1 ? reco.Reps * (decimal)0.675 : reco.Reps * (decimal)0.75 - reco.Reps * (decimal)0.5) / newWarmup : (reco.Reps * (decimal)0.75 - reco.Reps * (decimal)0.4) / newWarmup;

                    if (IsBodyweight == false && initialReps < (decimal)5.01)
                    {
                        if (!reco.isPlateAvailable)
                            initialReps = 6;
                    }
                    if (reco.isPlateAvailable && initialReps < 1)
                        initialReps = 1;
                    var warmupCount = 0;
                    
                    while (warmupCount < (int)WarmupsValue)
                    {
                        var newWarmsup = new WarmUps()
                        {
                            WarmUpWeightSet = IsBodyweight ? reco.Weight : new MultiUnityWeight(RoundDownNearestIncrement(intialWeight + (weightIncrement * warmupCount), incrementValue, min, max), WeightUnities.kg),
                            WarmUpReps = IsBodyweight ? (int)Math.Ceiling(initialReps - ((decimal)repsIncrement * ((decimal)WarmupsValue - (decimal)(warmupCount + 1)))) : (int)Math.Ceiling(initialReps - ((decimal)repsIncrement * (decimal)warmupCount))
                        };
                        if (!IsBodyweight)
                            newWarmsup.WarmUpReps = newWarmsup.WarmUpReps < 3 ? 3 : newWarmsup.WarmUpReps;
                        if (reco.isDumbbellAvailable && !reco.IsBodyweight)
                        {
                            newWarmsup.WarmUpWeightSet = GetDumbbellWeight(availablePlates, iskg ? newWarmsup.WarmUpWeightSet.Kg : newWarmsup.WarmUpWeightSet.Lb, iskg);
                        }
                        if (reco.isPulleyAvailable && !reco.IsBodyweight)
                        {
                            newWarmsup.WarmUpWeightSet = GetPlatesWeight(availablePlates, iskg ? newWarmsup.WarmUpWeightSet.Kg : newWarmsup.WarmUpWeightSet.Lb, 0,iskg);
                        }
                        WarmUpsList.Add(newWarmsup);
                        warmupCount++;
                    }
                    reco.WarmUpsList = WarmUpsList;
                }
                catch (Exception ex)
                {

                }
            }
            if (reco.BackOffSetWeight != null)
                reco.BackOffSetWeight = new MultiUnityWeight(RoundToNearestIncrement(reco.Weight.Kg - ((reco.Weight.Kg * (decimal)30) / 100), incrementValue, min, max), "kg");


            return reco;
        }

        public static RecommendationModel ComputeWarmups(RecommendationModel result, long exerciseId, ExerciseWorkSetsModel model, string availablePlates = "", bool iskg = true, decimal userBodyWeight = 0)
        {
            try
            {
                if (result.WarmupsCount == 0)
                    return result;
                result.WarmUpsList = new List<WarmUps>();
                int WarmupsValue = result.WarmupsCount;

                var incrementValue = result.Increments != null ? (decimal)result.Increments.Kg : 1;

                decimal registenceWeight = 0;
                var weight1 = result.Weight;
                if (model.IsAssisted)
                {
                    registenceWeight = result.Weight.Kg;//130 - 30
                    var assistedWeight =  registenceWeight;// * (decimal)0.5; //* (decimal)0.5;
                    if (assistedWeight <= 0)
                        assistedWeight = 0;
                    weight1 = new MultiUnityWeight(assistedWeight, WeightUnities.kg);
                }
                    
                var intialWeight = weight1.Kg / 2;
                var newWarmup = WarmupsValue > 1 ? WarmupsValue - 1 : WarmupsValue;
                var weightIncrement = (((weight1.Kg * (decimal)0.85) - (weight1.Kg * (decimal)0.5)) / (int)newWarmup);
                // if (model.IsAssisted)
                //  weightIncrement = (((weight1.Kg) - (weight1.Kg * (decimal)0.5)) / (int)newWarmup);
                var initialReps = (decimal)result.Reps * (decimal)0.75;

                if (result.IsBodyweight)
                    initialReps = (decimal)result.Reps * (decimal)0.60;
                var repsIncrement = result.IsBodyweight ? (WarmupsValue == 1 ? result.Reps * (decimal)0.60 : result.Reps * (decimal)0.6 - result.Reps * (decimal)0.5) / newWarmup : (result.Reps * (decimal)0.75 - result.Reps * (decimal)0.4) / newWarmup;
                
                if (result.IsBodyweight == false && initialReps < (decimal)5.01 && result.Weight.Kg != new MultiUnityWeight(RoundToNearestIncrement(weight1.Kg, incrementValue, result.Min?.Kg, result.Max?.Kg), WeightUnities.kg).Kg)
                {
                    if (!result.isPlateAvailable)
                        initialReps = 6;
                }
                if (result.isPlateAvailable && initialReps < 1)
                    initialReps = 1;
                double? sliderVal = null;
                try
                {

                    if (iskg)
                    {
                        sliderVal = 20;
                        if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("KgBarWeight")?.Value))
                        {
                            sliderVal = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("KgBarWeight")?.Value, System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        sliderVal = 45;
                        if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("LbBarWeight")?.Value))
                        {
                            sliderVal = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("LbBarWeight")?.Value, System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                var warmupCount = 0;
                while (warmupCount < (int)WarmupsValue)
                {
                    var newWarmsup = new WarmUps()
                    {
                        WarmUpWeightSet = result.IsBodyweight ? result.Weight : new MultiUnityWeight(RoundToNearestIncrement(model.IsAssisted ? userBodyWeight - (intialWeight + (weightIncrement * warmupCount)) : intialWeight + (weightIncrement * warmupCount), incrementValue, result.Min?.Kg, result.Max?.Kg), WeightUnities.kg),
                        WarmUpReps = result.IsBodyweight ? (int)Math.Ceiling(initialReps - ((decimal)repsIncrement * ((decimal)WarmupsValue - (decimal)(warmupCount + 1)))) : (int)Math.Ceiling(initialReps - ((decimal)repsIncrement * (decimal)warmupCount))
                    };
                    if (!result.IsBodyweight && weight1.Kg != new MultiUnityWeight(RoundToNearestIncrement(weight1.Kg,
incrementValue, result.Min?.Kg, result.Max?.Kg), WeightUnities.kg).Kg)
                        newWarmsup.WarmUpReps = result.isPlateAvailable ? newWarmsup.WarmUpReps : newWarmsup.WarmUpReps < 3 ? 3 : newWarmsup.WarmUpReps;
                    if (result.isPlateAvailable && !result.IsBodyweight)
                    {
                        newWarmsup.WarmUpWeightSet = GetPlatesWeight(availablePlates, iskg ? newWarmsup.WarmUpWeightSet.Kg : newWarmsup.WarmUpWeightSet.Lb, (double)sliderVal, iskg);
                    }
                    if (result.isDumbbellAvailable && !result.IsBodyweight)
                    {
                        newWarmsup.WarmUpWeightSet = GetDumbbellWeight(availablePlates, iskg ? newWarmsup.WarmUpWeightSet.Kg : newWarmsup.WarmUpWeightSet.Lb, iskg);
                    }
                    if ( result.isPulleyAvailable && !result.IsBodyweight)
                    {
                        newWarmsup.WarmUpWeightSet = GetPlatesWeight(availablePlates, iskg ? newWarmsup.WarmUpWeightSet.Kg : newWarmsup.WarmUpWeightSet.Lb, 0, iskg);
                    }
                    result.WarmUpsList.Add(newWarmsup);
                    warmupCount++;
                }
            }
            catch (Exception ex)
            {

            }
            if(model.IsWeighted)
            {
                result.WarmUpsList[0].WarmUpWeightSet = new MultiUnityWeight(0, "kg");
            }
            return result;
        }

        public static bool IsInStrengthPhase(string programName, int age, int levelReq, int totalworkouts)
        {
            try
            {
                if (LocalDBManager.Instance.GetDBSetting("StrengthPhase").Value == "false")
                    return false;

                if (programName.ToLower().Contains("bodyweight") || programName.ToLower().Contains("bands"))
                    return false;
                if (totalworkouts == 0)
                    return false;
                if (levelReq <= 0)
                    return false;
                var xDays = 0;
                if (programName.ToLower().Contains("split") || programName.ToLower().Contains("upper body") || programName.ToLower().Contains("lower body"))
                {
                    if (age < 30)
                        xDays = 5;
                    else if (age >= 30 && age <= 50)
                        xDays = 4;
                    else
                        xDays = 3;
                }
                else if (programName.ToLower().Contains("full-body") || programName.Contains("[Gym] PL") || programName.ToLower().Contains("powerlifting"))
                {
                    if (age < 30)
                        xDays = 4;
                    else if (age >= 30 && age <= 50)
                        xDays = 3;
                    else
                        xDays = 2;
                }
                else if (programName.ToLower().Contains("[home] push") || programName.ToLower().Contains("[home] pull") || programName.ToLower().Contains("[home] legs") || programName.ToLower().Contains("[gym] push") || programName.ToLower().Contains("[gym] pull") || programName.ToLower().Contains("[gym] legs"))
                {
                    xDays = 6;
                }
                if (xDays == 0)
                    return false;
                var requiredWorkoutToStrengthPhase = xDays * 3;
                var finishedWorkout = (totalworkouts - levelReq);
                if ((finishedWorkout) >= (totalworkouts - requiredWorkoutToStrengthPhase))
                    return true;
            }
            catch (Exception ex)
            {

            }
            return false;
        }
    }
}
