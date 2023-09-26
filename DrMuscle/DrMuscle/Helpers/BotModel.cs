using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Xamarin.Forms;
namespace DrMuscle.Helpers
{
    public class BotModel : INotifyPropertyChanged
    {
        public BotModel()
        {
        }
        public string StrengthImage { get; set; }
        public string StrengthPerText { get; set; }
        public string SetsImage { get; set; }
        public string SetsPerText { get; set; }
        public string LevelUpMessage { get; set; }
        public string LevelUpText { get; set; }

        public Color StrengthTextColor { get; set; }
        public Color SetTextColor { get; set; }
        public bool IsLastVisible { get
            {
                return !string.IsNullOrEmpty(LevelUpMessage);
            }
        }

        public bool IsNewRecordAvailable { get; set; }

        private string _question;

        public string Question
        {
            get { return _question; }
            set
            {
                _question = value;
                OnPropertyChanged("Question");
            }
        }
        private string _part1;
        public string Part1
        {
            get { return _part1; }
            set
            {
                _part1 = value;
                OnPropertyChanged("Part1");
            }
        }

        public string Answer { get; set; }
        public string Options { get; set; }
        public BotType Type { get; set; }


        public string TrainRest { get; set; }
        public string TrainRestText { get; set; }
        

        public string SinceTime { get; set; }
        public string LastWorkoutText { get; set; }
        public string WorkoutDoneText { get; set; }
        public string WorkoutDone { get; set; }

        public string LbsLiftedText { get; set; }
        public string LbsLifted { get; set; }

        public string Part2 { get; set; }
        public string Part3 { get; set; }

        public string CaloriesBurned { get; set; }
        public string ExerciseCount { get; set; }
        public string WorksetCount { get; set; }
        public string RecordCount { get; set; }
        public string MinuteCount { get; set; }
        public string ChainCount { get; set; }

        public string Source { get; set; } = "carlPhoto.png";

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum BotType
    {
        Ques,
        Ans,
        Photo,
        Loader,
        Chart,
        WeightTracker,
        Stats,
        Link,
        AnchorLink,
        AttributedLabelLink,
        LevelUp,
        LearnDay,
        RestRecovered,
        LinkGesture,
        Lifted,
        NewRecord,
        NewRecordCard,
        Workout,
        Congratulations,
        CongratulationsCard,
        ExplainerCell,
        SummaryLevelup,
        SummaryRest,
        Review,
        ReviewTestimonial,
        ReviewFullCell,
        Empty,
        NextWorkoutLoad,
        NextWorkoutLoadingCard,
        TipCard,
        LastWorkoutWas,
        AICard
    }
}

public class ChatGPTResponse
{
    [JsonProperty("choices")]
    public List<Choice> choices { get; set; }
}

public class Choice
{
    [JsonProperty("message")]
    public GptMessage message { get; set; }

    [JsonProperty("finish_reason")]
    public string finish_reason { get; set; }

    [JsonProperty("index")]
    public int index { get; set; }
}

public class GptMessage
{
    [JsonProperty("role")]
    public string role { get; set; }

    [JsonProperty("content")]
    public string content { get; set; }
}


public class ChatBotAIRequest
{
    [JsonProperty("messages")]
    public GptMessage[] Messages { get; set; }

    [JsonProperty("chatbotId")]
    public string ChatbotId { get; set; }

    [JsonProperty("stream")]
    public bool Stream { get; set; }

    [JsonProperty("temperature")]
    public long Temperature { get; set; }

    [JsonProperty("model")]
    public string Model { get; set; }

    [JsonProperty("conversationId")]
    public string ConversationId { get; set; }
}

public class ChatBotAIResponse
{
    public string text { get; set; }
    public string chatbotId { get; set; }
}


