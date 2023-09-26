using System;
using DrMuscle.Helpers;
using Xamarin.Forms;

namespace DrMuscle.Cells
{
    public class BotDataTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate questionTemplate;
        private readonly DataTemplate answerDataTemplate;
        private readonly DataTemplate photoDataTemplate;
        private readonly DataTemplate loaderDataTemplate;
        private readonly DataTemplate chartDataTemplate;
        private readonly DataTemplate emptyDataTemplate;
        private readonly DataTemplate statsDataTemplate;
        private readonly DataTemplate linkDataTemplate;
        private readonly DataTemplate anchorLinkTemplate;
        private readonly DataTemplate levelUpDataTemplate;
        private readonly DataTemplate restRecoveredDataTemplate;
        private readonly DataTemplate linkGestureTemplate;
        private readonly DataTemplate liftedTemplate;
        private readonly DataTemplate attributedTemplate;
        private readonly DataTemplate workoutTemplate;
        private readonly DataTemplate newRecordTemplate;
        private readonly DataTemplate congratulationsTemplate;
        private readonly DataTemplate congratulationsCardTemplate;
        private readonly DataTemplate explainerTemplate;
        private readonly DataTemplate summaryLevelupTemplate;
        private readonly DataTemplate summaryRestTemplate;
        private readonly DataTemplate reviewTemplate;
        private readonly DataTemplate weightTrackerTemplate;
        private readonly DataTemplate reviewTestimonialTemplate;
        private readonly DataTemplate reviewFullCellTemplate;
        private readonly DataTemplate learnDayTemplate;
        private readonly DataTemplate nextWorkoutLoadTemplate;
        private readonly DataTemplate nextWorkoutLoadingCardTemplate;
        private readonly DataTemplate tipCardTemplate;
        private readonly DataTemplate newRecordCardTemplate;
        private readonly DataTemplate lastWorkoutWasTemplate;
        private readonly DataTemplate AITemplate;

        public BotDataTemplateSelector()
        {
            this.questionTemplate = new DataTemplate(typeof(QuestionCell));
            this.answerDataTemplate = new DataTemplate(typeof(AnswerCell));
            this.photoDataTemplate = new DataTemplate(typeof(PhotoCell));
            this.loaderDataTemplate = new DataTemplate(typeof(LoaderCell));
            this.chartDataTemplate = new DataTemplate(typeof(ChartCell));
            this.emptyDataTemplate = new DataTemplate(typeof(EmptyCell));
            this.statsDataTemplate = new DataTemplate(typeof(StatsCell));
            this.linkDataTemplate = new DataTemplate(typeof(LinkCell));
            this.anchorLinkTemplate = new DataTemplate(typeof(AnchorLinkCell));
            this.levelUpDataTemplate = new DataTemplate(typeof(LevelUpCell));
            this.restRecoveredDataTemplate = new DataTemplate(typeof(RestRecoveredCell));
            this.linkGestureTemplate = new DataTemplate(typeof(LinkGestureCell));
            this.liftedTemplate = new DataTemplate(typeof(LiftedCell));
            this.attributedTemplate = new DataTemplate(typeof(AttributedLabel));
            this.newRecordTemplate = new DataTemplate(typeof(NewRecordCell));
            this.workoutTemplate = new DataTemplate(typeof(WorkoutCell));
            this.congratulationsTemplate = new DataTemplate(typeof(CongratulationsCell));
            this.congratulationsCardTemplate = new DataTemplate(typeof(CongratulationsCardCell));
            this.explainerTemplate = new DataTemplate(typeof(ExplainerCell));
            this.summaryLevelupTemplate = new DataTemplate(typeof(SummaryLevelup));
            this.summaryRestTemplate = new DataTemplate(typeof(SummaryRest));
            this.reviewTemplate = new DataTemplate(typeof(ReviewCell));
            this.reviewTestimonialTemplate = new DataTemplate(typeof(ReviewTestimonialCell));
            this.reviewFullCellTemplate = new DataTemplate(typeof(ReviewFullCell));
            this.learnDayTemplate = new DataTemplate(typeof(LearnDayCell));
            this.weightTrackerTemplate = new DataTemplate(typeof(WeightCell));
            this.nextWorkoutLoadTemplate = new DataTemplate(typeof(NextWorkoutLoadingCell));
            this.nextWorkoutLoadingCardTemplate = new DataTemplate(typeof(NextWorkoutLoadingCardCell));
            this.tipCardTemplate = new DataTemplate(typeof(TipCell));
            this.newRecordCardTemplate = new DataTemplate(typeof(NewRecordCardCell));
            this.lastWorkoutWasTemplate = new DataTemplate(typeof(LastWorkoutWasCardCell));
            this.AITemplate = new DataTemplate(typeof(AIAnalysisCell));
        }
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var botModel = item as BotModel;
            if (botModel == null)
                return null;
            try
            {
                if (botModel.Type == BotType.Photo)
                    return this.photoDataTemplate;
                if (botModel.Type == BotType.Loader)
                    return this.loaderDataTemplate;
                if (botModel.Type == BotType.Chart)
                    return this.chartDataTemplate;
                if (botModel.Type == BotType.WeightTracker)
                    return this.weightTrackerTemplate;
                if (botModel.Type == BotType.AnchorLink)
                    return this.anchorLinkTemplate;
                if (botModel.Type == BotType.AttributedLabelLink)
                    return this.attributedTemplate;
                if (botModel.Type == BotType.Stats)
                    return this.statsDataTemplate;
                if (botModel.Type == BotType.Empty)
                    return this.emptyDataTemplate;
                if (botModel.Type == BotType.Link)
                    return this.linkDataTemplate;
                if (botModel.Type == BotType.LevelUp)
                    return this.levelUpDataTemplate;
                if (botModel.Type == BotType.RestRecovered)
                    return restRecoveredDataTemplate;
                if (botModel.Type == BotType.LinkGesture)
                    return linkGestureTemplate;
                if (botModel.Type == BotType.NewRecord)
                    return newRecordTemplate;
                if (botModel.Type == BotType.NewRecordCard)
                    return newRecordCardTemplate;
                if (botModel.Type == BotType.Workout)
                    return workoutTemplate;
                if (botModel.Type == BotType.LearnDay)
                    return learnDayTemplate;
                if  (botModel.Type == BotType.Lifted)
                    return liftedTemplate;
                if (botModel.Type == BotType.ExplainerCell)
                    return explainerTemplate;
                if (botModel.Type == BotType.SummaryRest)
                    return summaryRestTemplate;
                if (botModel.Type == BotType.SummaryLevelup)
                    return summaryLevelupTemplate;
                if (botModel.Type == BotType.Congratulations)
                    return congratulationsTemplate;
                if (botModel.Type == BotType.CongratulationsCard)
                    return congratulationsCardTemplate;
                if (botModel.Type == BotType.NextWorkoutLoadingCard)
                    return nextWorkoutLoadingCardTemplate;
                if (botModel.Type == BotType.TipCard)
                    return tipCardTemplate;
                if (botModel.Type == BotType.NextWorkoutLoad)
                    return nextWorkoutLoadTemplate;
                if (botModel.Type == BotType.Review)
                    return reviewTemplate;
                if (botModel.Type == BotType.ReviewTestimonial)
                    return reviewTestimonialTemplate;
                if (botModel.Type == BotType.ReviewFullCell)
                    return this.reviewFullCellTemplate;
                if (botModel.Type == BotType.LastWorkoutWas)
                    return this.lastWorkoutWasTemplate;
                if (botModel.Type == BotType.AICard)
                    return this.AITemplate;
                return botModel.Type == BotType.Ques ? this.questionTemplate : this.answerDataTemplate;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
