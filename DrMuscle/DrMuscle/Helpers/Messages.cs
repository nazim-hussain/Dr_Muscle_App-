using System;
using System.Collections.ObjectModel;
using DrMuscle.Constants;

namespace DrMuscle.Helpers
{
    public class Messages
    {
        private bool _isUnread { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public long MessageId { get; set; }
        public string SenderId { get; set; }
        public string ProfileUrl { get; set; }
        public string Nickname { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsFromAI { get; set; }
        public bool IsSend { get; set; }
        public bool IsUnread
        {
            get { return _isUnread; }
            set
            {
                _isUnread = value;
            }
        }
        public long CreatedAt { get; set; }
        public ChannelType ChatType { get; set; }
        public string SupportChannelUrl { get; set; }
        public string AdminId { get; set; }
        public string NormalUSerEmail { get; set; }
        public string NormalUSerName { get; set; }
        public string NormalUSerId { get; set; }
        public bool IsBothReplied { get; set; }
        public long ChatRoomId { get; set; }
        public bool IsV1User { get; set; }
        public string TImeAgo
        {
            get { return AppThemeConstants.ChatTimeAgoFromDate(CreatedDate); }
        }
    }

    public class GroupChannelType : ObservableCollection<Messages>
    {
        public string Name { get; set; }
        public ChannelType Type { get; set; }
    }

    public enum ChannelType
    {
        Open,
        Group
    }
}
