using System;
using Xamarin.Forms;
using DrMuscle.iOS;
using Foundation;
using HealthKit;
using System.Threading.Tasks;
using DrMuscle.Dependencies;
using System.Collections.Generic;
using DrMuscle.Screens.Demo;
using System.Runtime.InteropServices.ComTypes;
using System.Linq;
using DrMuscleWebApiSharedModel;

[assembly: Dependency(typeof(HealthData))]
namespace DrMuscle.iOS
{
    public class HealthData : IHealthData
    {
        NSNumberFormatter numberFormatter;
        public HKHealthStore HealthStore { get; set; }

        NSSet DataTypesToWrite
        {
            get
            {
                return NSSet.MakeNSObjectSet<HKObjectType>(new HKObjectType[] {

                });
            }
        }

        NSSet DataTypesToRead
        {
            get
            {
                return NSSet.MakeNSObjectSet<HKObjectType>(new HKObjectType[] {
                   // HKQuantityType.Create(HKQuantityTypeIdentifier.Height),
                    HKQuantityType.Create(HKQuantityTypeIdentifier.AppleExerciseTime),
                  HKQuantityType.Create(HKQuantityTypeIdentifier.ActiveEnergyBurned)
                  //  HKQuantityType.Create(HKQuantityTypeIdentifier.AppleExerciseTime),
                  //  HKQuantityType.Create(HKQuantityTypeIdentifier.ActiveEnergyBurned),
                  //  HKQuantityType.Create(HKQuantityTypeIdentifier.HeartRate),
                 
                });
            }
        }
        public async Task GetWeightPermissionAsync(Action<bool> completion)
        {
            if (HKHealthStore.IsHealthDataAvailable)
            {
                HealthStore = new HKHealthStore();

                var writeTypes = NSSet.MakeNSObjectSet<HKObjectType>(new HKObjectType[] {  HKQuantityType.Create(HKQuantityTypeIdentifier.BodyMass) });
                var readTypes = NSSet.MakeNSObjectSet<HKObjectType>(new HKObjectType[] {   HKQuantityType.Create(HKQuantityTypeIdentifier.BodyMass) });

                // HealthStore.RequestAuthorizationToShareAsync(DataTypesToRead,DataTypesToWrite,)

                //var result = await HealthStore.RequestAuthorizationToShareAsync(null, writeTypes);
                var result = await HealthStore.RequestAuthorizationToShareAsync(writeTypes, readTypes);

                completion(result.Item1);

            }
            else
            {
                completion(false);
            }
        }

        public async Task GetHealthPermissionAsync(Action<bool> completion)
        {
            if (HKHealthStore.IsHealthDataAvailable)
            {
                HealthStore = new HKHealthStore();

                var writeTypes = NSSet.MakeNSObjectSet<HKObjectType>(new HKObjectType[]  { HKObjectType.GetWorkoutType(), HKQuantityType.Create(HKQuantityTypeIdentifier.ActiveEnergyBurned) });
                var readTypes = NSSet.MakeNSObjectSet<HKObjectType>(new HKObjectType[] { HKObjectType.GetWorkoutType() });

                // HealthStore.RequestAuthorizationToShareAsync(DataTypesToRead,DataTypesToWrite,)

                //var result = await HealthStore.RequestAuthorizationToShareAsync(null, writeTypes);
                var result = await HealthStore.RequestAuthorizationToShareAsync(writeTypes, readTypes);

                completion(result.Item1);
                
            }
            else
            {
                completion(false);
            }
        }



        void ReactToHealthCarePermissions(bool success, NSError error)
        {
            /*
			 * The success and error arguments specify whether the user interacted
			 * with the permissions dialog. This sample doesn't use that information.
			 */

            //Instead, the important thing is to confirm that we can write heart-rate data
            var access = HealthStore.GetAuthorizationStatus(HKObjectType.GetQuantityType(HKQuantityTypeIdentifierKey.AppleExerciseTime));
            if (access.HasFlag(HKAuthorizationStatus.SharingAuthorized))
            {
                //completion(false);
            }
            else
            {
                //completion(false);
            }
        }

        public void FetchSteps(DateTime startDate, DateTime endDate, Action<double> completionHandler)
        {
            var calendar = NSCalendar.CurrentCalendar;
            //var startDate = DateTime.Today;
            //var endDate = DateTime.Now;
            try
            {

            var stepsQuantityType = HKQuantityType.Create(HKQuantityTypeIdentifier.StepCount);

            var predicate = HKQuery.GetPredicateForSamples((NSDate)startDate, (NSDate)endDate, HKQueryOptions.StrictStartDate);

            var query = new HKStatisticsQuery(stepsQuantityType, predicate, HKStatisticsOptions.CumulativeSum,
                            (HKStatisticsQuery resultQuery, HKStatistics results, NSError error) =>
                            {
                                if (error != null && completionHandler != null)
                                    completionHandler(0.0f);

                                var totalSteps = results?.SumQuantity();
                                if (totalSteps == null)
                                    totalSteps = HKQuantity.FromQuantity(HKUnit.Count, 0.0);

                                completionHandler(totalSteps.GetDoubleValue(HKUnit.Count));
                            });
            HealthStore.ExecuteQuery(query);

            }
            catch (Exception ex)
            {

            }
        }

        public void FetchStairsSteps(DateTime startDate, DateTime endDate, Action<double> completionHandler)
        {
            try
            {
                
                var calendar = NSCalendar.CurrentCalendar;
                //var startDate = DateTime.Today;
                //var endDate = DateTime.Now;
                var stairsQuantityType = HKQuantityType.Create(HKQuantityTypeIdentifier.FlightsClimbed);

                var predicate = HKQuery.GetPredicateForSamples((NSDate)startDate, (NSDate)endDate, HKQueryOptions.StrictStartDate);

                var query = new HKStatisticsQuery(stairsQuantityType, predicate, HKStatisticsOptions.CumulativeSum,
                                (HKStatisticsQuery resultQuery, HKStatistics results, NSError error) =>
                                {
                                    if (error != null && completionHandler != null)
                                        completionHandler(0.0f);

                                    var totalSteps = results?.SumQuantity();
                                    if (totalSteps == null)
                                        totalSteps = HKQuantity.FromQuantity(HKUnit.Count, 0.0);

                                    completionHandler(totalSteps.GetDoubleValue(HKUnit.Count));
                                });
                HealthStore.ExecuteQuery(query);
            } catch(Exception)
            {

            }
        }

        public void FetchMetersWalked(Action<double> completionHandler)
        {
            var calendar = NSCalendar.CurrentCalendar;
            var startDate = DateTime.Today;
            var endDate = DateTime.Now;
            var stepsQuantityType = HKQuantityType.Create(HKQuantityTypeIdentifier.DistanceWalkingRunning);

            var predicate = HKQuery.GetPredicateForSamples((NSDate)startDate, (NSDate)endDate, HKQueryOptions.StrictStartDate);

            var query = new HKStatisticsQuery(stepsQuantityType, predicate, HKStatisticsOptions.CumulativeSum,
                            (HKStatisticsQuery resultQuery, HKStatistics results, NSError error) =>
                            {
                                if (error != null && completionHandler != null)
                                    completionHandler(0);

                                var distance = results.SumQuantity();
                                if (distance == null)
                                    distance = HKQuantity.FromQuantity(HKUnit.Meter, 0);

                                completionHandler(distance.GetDoubleValue(HKUnit.Meter));
                            });
            HealthStore.ExecuteQuery(query);
        }

        public void FetchWeight(Action<double> completionHandler)
        {

            try
            {

            var bodyMassType = HKQuantityTypeIdentifierKey.BodyMass;
            var sortDescriptor = new NSSortDescriptor(HKSample.SortIdentifierStartDate, false);
            

            var query1 = new HKSampleQuery(HKSampleType.GetQuantityType(bodyMassType), null, 1, new [] { sortDescriptor } , (HKSampleQuery resultQuery, HKSample[] results, NSError error) =>
            {
                if (error != null && completionHandler != null)
                    completionHandler(0);

                //guard let currData:HKQuantitySample = results![iter] as? HKQuantitySample else { return }


                if (results.Length > 0)
                {
                    var unit = LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg" ? NSMassFormatterUnit.Kilogram : NSMassFormatterUnit.Pound;
                    HKQuantitySample sample = (HKQuantitySample)results.Last();
                    completionHandler(sample.Quantity.GetDoubleValue(HKUnit.FromMassFormatterUnit(unit)));
                }

            });
            
            HealthStore.ExecuteQuery(query1);

            }
            catch (Exception ex)
            {

            }
        }

        public void SetWeight(double bodyweight)
        {
            var bodyMassType = HKQuantityTypeIdentifierKey.BodyMass;
            var sortDescriptor = new NSSortDescriptor(HKSample.SortIdentifierStartDate, false);

            var bodyWeightType = HKQuantityType.GetQuantityType(bodyMassType);
            var unit = LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg" ? NSMassFormatterUnit.Kilogram : NSMassFormatterUnit.Pound;
            HKQuantity userBodyweight = HKQuantity.FromQuantity(HKUnit.FromMassFormatterUnit( unit), bodyweight);
            var weight = HKQuantitySample.FromType(bodyWeightType, userBodyweight, new NSDate(), new NSDate());

            using (var healthKitStore = new HKHealthStore())
            {
                healthKitStore.SaveObject(weight, (success, error) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (success)
                        {
                            //HeartRateStored(this, new GenericEventArgs<Double>(quantity.GetDoubleValue(bpm)));
                        }
                        else
                        {
                            //ErrorMessageChanged(this, new GenericEventArgs<string>("Save failed"));
                        }
                        if (error != null)
                        {

                        }
                    });
                });
            }
            }

        public void FetchActiveMinutes(Action<double> completionHandler)
		{
			var calendar = NSCalendar.CurrentCalendar;
			var startDate = DateTime.Today;
			var endDate = DateTime.Now;
			var stepsQuantityType = HKQuantityType.Create(HKQuantityTypeIdentifier.AppleExerciseTime);

			var predicate = HKQuery.GetPredicateForSamples((NSDate)startDate, (NSDate)endDate, HKQueryOptions.StrictStartDate);

			var query = new HKStatisticsQuery(stepsQuantityType, predicate, HKStatisticsOptions.CumulativeSum,
							(HKStatisticsQuery resultQuery, HKStatistics results, NSError error) =>
							{
								if (error != null && completionHandler != null)
									completionHandler(0);

								var totalMinutes = results.SumQuantity();
								if (totalMinutes == null)
									totalMinutes = HKQuantity.FromQuantity(HKUnit.Minute, 0);

								completionHandler(totalMinutes.GetDoubleValue(HKUnit.Minute));
							});
			HealthStore.ExecuteQuery(query);
		}


		public void FetchActiveEnergyBurned(Action<double> completionHandler)
		{
			var calendar = NSCalendar.CurrentCalendar;
			var startDate = DateTime.Today;
			var endDate = DateTime.Now;
			var stepsQuantityType = HKQuantityType.Create(HKQuantityTypeIdentifier.ActiveEnergyBurned);

			var predicate = HKQuery.GetPredicateForSamples((NSDate)startDate, (NSDate)endDate, HKQueryOptions.StrictStartDate);

			var query = new HKStatisticsQuery(stepsQuantityType, predicate, HKStatisticsOptions.CumulativeSum,
							(HKStatisticsQuery resultQuery, HKStatistics results, NSError error) =>
							{
								if (error != null && completionHandler != null)
									completionHandler(0);

								var energyBurned = results.SumQuantity();
								if (energyBurned == null)
									energyBurned = HKQuantity.FromQuantity(HKUnit.Calorie, 0);

								completionHandler(energyBurned.GetDoubleValue(HKUnit.Calorie));
							});
			HealthStore.ExecuteQuery(query);
		}

        public void SaveActiveMinutes(double minute)
        {
            try
            {
                    
            var finish = new NSDate(); // Now
            var start = finish.AddSeconds(-(minute) * 60); // 1 hour ago

                var item = HKWorkoutType.ActivitySummaryType;
                var stairsQuantityType = HKQuantityType.Create(HKQuantityTypeIdentifier.ElectrodermalActivity);
                var metadataDict = new NSDictionary<NSString, NSObject>(new[] { new NSString("Volume"), new NSString("New record"), new NSString("Exercise") }, new[] { new NSString("10"), new NSString("2"), new NSString("5") }); ;
                 
                var metadata = new HKMetadata(metadataDict);


                var workout = HKWorkoutType.GetWorkoutType();
                var work = HKWorkout.Create(HKWorkoutActivityType.Other, start, finish);

                //var work = HKWorkout.Create(HKWorkoutActivityType.TraditionalStrengthTraining, start, finish, (minute) * 60, null, null,metadata) ;
                using (var healthKitStore = new HKHealthStore())
            {
                healthKitStore.SaveObject(work, (success, error) => {
                    Device.BeginInvokeOnMainThread(() => {
                        if (success)
                        {
                            //HeartRateStored(this, new GenericEventArgs<Double>(quantity.GetDoubleValue(bpm)));
                        }
                        else
                        {
                            //ErrorMessageChanged(this, new GenericEventArgs<string>("Save failed"));
                        }
                        if (error != null)
                        {
                           
                        }
                    });
                });
            }

            }
            catch (Exception ex)
            {

            }
        }
        public void SaveActiveMinutes(double minutes, int newrecord, int exercise, string volume, string totalStrength, string maxStrength, double calories, List<string> exercisesName, List<string> exercisesStrength)
        {
            try
            {
                if (exercise == 0)
                    return;
                var finish = new NSDate(); // Now
                var start = finish.AddSeconds(-(minutes) * 60); // 1 hour ago
                List<NSString> keyArray = new List<NSString>();
                List<NSString> valueArray = new List<NSString>();
                var item = HKWorkoutType.ActivitySummaryType;
                var stairsQuantityType = HKQuantityType.Create(HKQuantityTypeIdentifier.ElectrodermalActivity);




                for (int i = 0; i < exercisesName.Count; i++)
                {
                    keyArray.Add(new NSString(exercisesName[i]));
                    valueArray.Add(new NSString(exercisesStrength[i]));
                    //metadataDict.Add(new NSString(exercisesName[i]), new NSString(exercisesStrength[i]));
                }

                keyArray.Add(new NSString(newrecord > 0 ? "New records" : "New record"));
                valueArray.Add(new NSString(newrecord.ToString()));

                keyArray.Add(new NSString("Volume (work sets)"));
                valueArray.Add(new NSString(volume));

                keyArray.Add(new NSString("Exercise"));
                valueArray.Add(new NSString(exercise.ToString()));

                keyArray.Add(new NSString("Max strength"));
                valueArray.Add(new NSString(maxStrength));

                keyArray.Add(new NSString("Total strength"));
                valueArray.Add(new NSString(totalStrength));
                
                var metadata = new HKMetadata(new NSDictionary<NSString, NSObject>(keyArray.ToArray(),valueArray.ToArray()));

                HKQuantity energyBurned = HKQuantity.FromQuantity(HKUnit.LargeCalorie, calories);

                
                var work = HKWorkout.Create(HKWorkoutActivityType.TraditionalStrengthTraining, start, finish, (minutes) * 60, energyBurned, null,metadata) ;
                
                using (var healthKitStore = new HKHealthStore())
                {
                    healthKitStore.SaveObject(work, (success, error) =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if (success)
                            {
                                //HeartRateStored(this, new GenericEventArgs<Double>(quantity.GetDoubleValue(bpm)));
                            }
                            else
                            {
                                //ErrorMessageChanged(this, new GenericEventArgs<string>("Save failed"));
                            }
                            if (error != null)
                            {

                            }
                        });
                    });
                    //healthKitStore.SaveObject(burned, (success, error) => {
                    //    Device.BeginInvokeOnMainThread(() => {
                    //        if (success)
                    //        {
                    //            //HeartRateStored(this, new GenericEventArgs<Double>(quantity.GetDoubleValue(bpm)));
                    //        }
                    //        else
                    //        {
                    //            //ErrorMessageChanged(this, new GenericEventArgs<string>("Save failed"));
                    //        }
                    //        if (error != null)
                    //        {

                    //        }
                    //    });
                    //});

                }


                /*
        
                var activeenergyBurned = HKQuantityTypeIdentifier.ActiveEnergyBurned;
                var AEBurnedQuantityType = HKQuantityType.Create(activeenergyBurned);// HKQuantityType.GetQuantityType(activeenergyBurned);


                var confuguaration = new HKWorkoutConfiguration();
                confuguaration.ActivityType = HKWorkoutActivityType.TraditionalStrengthTraining;

                
                var cumulativeEnergy = HKCumulativeQuantitySeriesSample.FromType(AEBurnedQuantityType, energyBurned, start, finish, metadata);
                //HKCumulativeQuantitySample.FromType(HKQuantityType.cre, energyBurned, start, finish, metadata);
                var workoutBuilder = new HKWorkoutBuilder(new HKHealthStore(), confuguaration, HKDevice.LocalDevice);
                workoutBuilder.BeginCollection(start, (success, error) =>
                {
                    if (error != null)
                    {

                    }
                });
                workoutBuilder.Add(new[] { cumulativeEnergy }, (success, error) =>
                {
                    if (error != null)
                    {
                        System.Diagnostics.Debug.WriteLine(error.DebugDescription);
                    }
                });
                workoutBuilder.EndCollection(finish, (success, error) =>
                {
                    if (error != null)
                    {

                    }
                });

                workoutBuilder.FinishWorkout((success, error) =>
                {
                    if (error != null)
                    {

                    }
                });*/
            }
            catch (Exception ex)
            {

            }
        }
    }
}
