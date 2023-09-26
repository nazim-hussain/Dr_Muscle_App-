using System;
using System.Collections.Generic;

namespace DrMuscleWebApiSharedModel
{
    
    public class Address
    {
        public string country { get; set; }
    }

    public class Customer
    {
        public string email { get; set; }
        public string ip_address { get; set; }
        public Address address { get; set; }
    }

    public class Event
    {
        public string event_id { get; set; }
        public string event_type { get; set; }
        public string date { get; set; }
        public string reference { get; set; }
        public int? amount { get; set; }
        public int? rebill_id { get; set; }
        public string transaction_id { get; set; }
    }

    public class Subscription
    {
        public string status { get; set; }
        public string order_id { get; set; }
        public string invoice_id { get; set; }
        public string subscription_id { get; set; }
        public string subscription_reference { get; set; }
        public string currency { get; set; }
        public string frequency { get; set; }
        public string amount { get; set; }
        public string payments { get; set; }
        public object payments_remaining { get; set; }
        public string total_paid { get; set; }
        public DateTime? last_payment { get; set; }
        public string date_started { get; set; }
        public string processor { get; set; }
        public string item_name { get; set; }
        public string item_type { get; set; }
        public string item_id { get; set; }
        public string remote_subscription_id { get; set; }
        public string next_payment { get; set; }
        public List<Event> events { get; set; }
    }

    public class LifetimeValue
    {
        public int USD { get; set; }
    }

    public class ThriveCartSubscription
    {
        public Customer customer { get; set; }
        public List<object> purchases { get; set; }
        public List<Subscription> subscriptions { get; set; }
       // public LifetimeValue lifetime_value { get; set; }
    }
}
