// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace DrMuscleWatch.WatchOSExtension
{
	[Register ("InterfaceController")]
	partial class InterfaceController
	{
		[Outlet]
		WatchKit.WKInterfaceButton BtnFinishAndSave { get; set; }

		[Outlet]
		WatchKit.WKInterfaceButton BtnFinishExercise { get; set; }

		[Outlet]
		WatchKit.WKInterfaceButton BtnHidePicker { get; set; }

		[Outlet]
		WatchKit.WKInterfaceButton BtnHideTimer { get; set; }

		[Outlet]
		WatchKit.WKInterfaceButton BtnNextExercise { get; set; }

		[Outlet]
		WatchKit.WKInterfaceButton BtnOpenWorkout { get; set; }

		[Outlet]
		WatchKit.WKInterfaceButton BtnSaveSet { get; set; }

		[Outlet]
		WatchKit.WKInterfaceGroup GroupPicker { get; set; }

		[Outlet]
		WatchKit.WKInterfaceGroup GroupReps { get; set; }

		[Outlet]
		WatchKit.WKInterfaceGroup GroupTimer { get; set; }

		[Outlet]
		WatchKit.WKInterfaceGroup GroupWeight { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel LblExerciseName { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel LblLoading { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel LblReps { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel LblRepsValue { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel LblWeighting { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel LblWeightValue { get; set; }

		[Outlet]
		WatchKit.WKInterfacePicker PickerRIR { get; set; }

		[Outlet]
		WatchKit.WKInterfaceTimer timer { get; set; }

		[Action ("BtnFinishandSaveClicked")]
		partial void BtnFinishandSaveClicked ();

		[Action ("BtnFinishExerciseClicked")]
		partial void BtnFinishExerciseClicked ();

		[Action ("BtnHidePickerTapped")]
		partial void BtnHidePickerTapped ();

		[Action ("BtnHideTimerClicked")]
		partial void BtnHideTimerClicked ();

		[Action ("BtnNextExerrciseClicked")]
		partial void BtnNextExerrciseClicked ();

		[Action ("BtnOpenWorkoutClicked")]
		partial void BtnOpenWorkoutClicked ();

		[Action ("BtnRepsLess")]
		partial void BtnRepsLess ();

		[Action ("BtnRepsMore")]
		partial void BtnRepsMore ();

		[Action ("BtnSavesetclicked")]
		partial void BtnSavesetclicked ();

		[Action ("BtnWeightLess")]
		partial void BtnWeightLess ();

		[Action ("BtnWeightMore")]
		partial void BtnWeightMore ();

		[Action ("RIRPickerSelected:")]
		partial void RIRPickerSelected (Foundation.NSObject value);

		[Action ("TimerGestureTapped:")]
		partial void TimerGestureTapped (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (BtnFinishAndSave != null) {
				BtnFinishAndSave.Dispose ();
				BtnFinishAndSave = null;
			}

			if (BtnFinishExercise != null) {
				BtnFinishExercise.Dispose ();
				BtnFinishExercise = null;
			}

			if (BtnHidePicker != null) {
				BtnHidePicker.Dispose ();
				BtnHidePicker = null;
			}

			if (BtnHideTimer != null) {
				BtnHideTimer.Dispose ();
				BtnHideTimer = null;
			}

			if (BtnNextExercise != null) {
				BtnNextExercise.Dispose ();
				BtnNextExercise = null;
			}

			if (BtnSaveSet != null) {
				BtnSaveSet.Dispose ();
				BtnSaveSet = null;
			}

			if (BtnOpenWorkout != null) {
				BtnOpenWorkout.Dispose ();
				BtnOpenWorkout = null;
			}

			if (GroupPicker != null) {
				GroupPicker.Dispose ();
				GroupPicker = null;
			}

			if (GroupReps != null) {
				GroupReps.Dispose ();
				GroupReps = null;
			}

			if (GroupTimer != null) {
				GroupTimer.Dispose ();
				GroupTimer = null;
			}

			if (GroupWeight != null) {
				GroupWeight.Dispose ();
				GroupWeight = null;
			}

			if (LblExerciseName != null) {
				LblExerciseName.Dispose ();
				LblExerciseName = null;
			}

			if (LblLoading != null) {
				LblLoading.Dispose ();
				LblLoading = null;
			}

			if (LblReps != null) {
				LblReps.Dispose ();
				LblReps = null;
			}

			if (LblRepsValue != null) {
				LblRepsValue.Dispose ();
				LblRepsValue = null;
			}

			if (LblWeighting != null) {
				LblWeighting.Dispose ();
				LblWeighting = null;
			}

			if (LblWeightValue != null) {
				LblWeightValue.Dispose ();
				LblWeightValue = null;
			}

			if (PickerRIR != null) {
				PickerRIR.Dispose ();
				PickerRIR = null;
			}

			if (timer != null) {
				timer.Dispose ();
				timer = null;
			}
		}
	}
}
