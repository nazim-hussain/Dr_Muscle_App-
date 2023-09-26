using System;
using System.Collections.Generic;

namespace DrMuscleWebApiSharedModel
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Metadata
    {
    }

    public class Sender
    {
        public string nickname { get; set; }
        public string user_id { get; set; }
        public string profile_url { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Metadata2
    {
    }

    public class Member
    {
        public bool is_blocking_sender { get; set; }
        public int unread_message_count { get; set; }
        public int total_unread_message_count { get; set; }
        public bool is_active { get; set; }
        public bool is_online { get; set; }
        public int is_hidden { get; set; }
        public int channel_mention_count { get; set; }
        public string nickname { get; set; }
        public bool is_blocked_by_sender { get; set; }
        public string user_id { get; set; }
        public int channel_unread_message_count { get; set; }
        public bool do_not_disturb { get; set; }
        public string state { get; set; }
        public bool push_enabled { get; set; }
        public string push_trigger_option { get; set; }
        public string profile_url { get; set; }
        public Metadata2 metadata { get; set; }
    }

    public class Translations
    {
    }

    public class Payload
    {
        public string custom_type { get; set; }
        public long created_at { get; set; }
        public Translations translations { get; set; }
        public string message { get; set; }
        public string data { get; set; }
        public long message_id { get; set; }
    }

    public class Channel
    {
        public bool is_distinct { get; set; }
        public string name { get; set; }
        public string custom_type { get; set; }
        public bool is_ephemeral { get; set; }
        public string channel_url { get; set; }
        public bool is_public { get; set; }
        public bool is_super { get; set; }
        public string data { get; set; }
        public bool is_discoverable { get; set; }
    }

    public class SendBirdNotificationModel
    {
        public string category { get; set; }
        public Sender sender { get; set; }
        public bool silent { get; set; }
        public string custom_type { get; set; }
        public string mention_type { get; set; }
        public List<object> mentioned_users { get; set; }
        public string app_id { get; set; }
        public string sender_ip_addr { get; set; }
        public List<Member> members { get; set; }
        public string type { get; set; }
        public Payload payload { get; set; }
        public Channel channel { get; set; }
        public string sdk { get; set; }
    }


}
