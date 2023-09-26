using System;
using System.Collections.Generic;

namespace DrMuscleWebApiSharedModel
{
    
    public class LatestExpiredReceiptInfo
    {
        public string original_purchase_date_pst { get; set; }
        public string cancellation_date_ms { get; set; }
        public string quantity { get; set; }
        public string subscription_group_identifier { get; set; }
        public string cancellation_reason { get; set; }
        public string unique_vendor_identifier { get; set; }
        public string original_purchase_date_ms { get; set; }
        public string expires_date_formatted { get; set; }
        public string is_in_intro_offer_period { get; set; }
        public string purchase_date_ms { get; set; }
        public string expires_date_formatted_pst { get; set; }
        public string is_trial_period { get; set; }
        public string item_id { get; set; }
        public string unique_identifier { get; set; }
        public string original_transaction_id { get; set; }
        public string expires_date { get; set; }
        public string app_item_id { get; set; }
        public string transaction_id { get; set; }
        public string bvrs { get; set; }
        public string web_order_line_item_id { get; set; }
        public string version_external_identifier { get; set; }
        public string bid { get; set; }
        public string cancellation_date { get; set; }
        public string product_id { get; set; }
        public string purchase_date { get; set; }
        public string cancellation_date_pst { get; set; }
        public string purchase_date_pst { get; set; }
        public string original_purchase_date { get; set; }
    }

    public class PendingRenewalInfo
    {
        public string original_transaction_id { get; set; }
        public string product_id { get; set; }
        public string auto_renew_status { get; set; }
        public string auto_renew_product_id { get; set; }
    }

    public class LatestReceiptInfo
    {
        public string cancellation_reason { get; set; }
        public string purchase_date { get; set; }
        public string purchase_date_ms { get; set; }
        public string original_purchase_date_ms { get; set; }
        public string product_id { get; set; }
        public string transaction_id { get; set; }
        public string original_transaction_id { get; set; }
        public string subscription_group_identifier { get; set; }
        public string expires_date_ms { get; set; }
        public string original_purchase_date_pst { get; set; }
        public string cancellation_date { get; set; }
        public string expires_date_pst { get; set; }
        public string quantity { get; set; }
        public string is_in_intro_offer_period { get; set; }
        public string web_order_line_item_id { get; set; }
        public string expires_date { get; set; }
        public string cancellation_date_ms { get; set; }
        public string cancellation_date_pst { get; set; }
        public string original_purchase_date { get; set; }
        public string purchase_date_pst { get; set; }
        public string is_trial_period { get; set; }
    }

    public class UnifiedReceipt
    {
        public string latest_receipt { get; set; }
        public List<PendingRenewalInfo> pending_renewal_info { get; set; }
        public string environment { get; set; }
        public int status { get; set; }
        public List<LatestReceiptInfo> latest_receipt_info { get; set; }
    }

    public class AppleNotificationModel
    {
        public string latest_expired_receipt { get; set; }
        public LatestExpiredReceiptInfo latest_expired_receipt_info { get; set; }
        public string password { get; set; }
        public string cancellation_date { get; set; }
        public UnifiedReceipt unified_receipt { get; set; }
        public string auto_renew_product_id { get; set; }
        public string notification_type { get; set; }
        public string environment { get; set; }
        public string auto_renew_status { get; set; }
        public string web_order_line_item_id { get; set; }
        public string cancellation_date_ms { get; set; }
        public string cancellation_date_pst { get; set; }
    }
}
