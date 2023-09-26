using System.Linq;
using Android.App;
using Android.Content;
using Android.Runtime;
//using Android.Gms.Wearable;
using Android.Gms.Common.Apis;
using Android.Support.V4.Content;
using System;
using System.Collections.Generic;

namespace DrMuscle.Droid.Service
{
    //[Service]
    //[IntentFilter(new[] { "com.google.android.gms.wearable.BIND_LISTENER" })]
    //public class WearService : WearableListenerService
    //{
    //    const string _syncPath = "/DrMuscleWear/Data";
    //    GoogleApiClient _client;

    //    public override void OnCreate() {
    //        base.OnCreate();
    //        _client = new GoogleApiClient.Builder(this.ApplicationContext)
    //                .AddApi(WearableClass.Api)
    //                .Build();

    //        _client.Connect();

    //        Android.Util.Log.Info("WearIntegration", "Created");
    //    }

    //    public override void OnDestroy()
    //    {
    //        base.OnDestroy();
    //    }

        
    //    public override void OnDataChanged(DataEventBuffer dataEvents) {
    //        var dataEvent = Enumerable.Range(0, dataEvents.Count)
    //                                  .Select(i => dataEvents.Get(i).JavaCast<IDataEvent>())
    //                                  .FirstOrDefault(x => x.Type == DataEvent.TypeChanged && x.DataItem.Uri.Path.Equals(_syncPath));
    //        if (dataEvent == null)
    //            return;

    //        //get data from wearable
    //        var dataMapItem = DataMapItem.FromDataItem(dataEvent.DataItem);
    //        var map = dataMapItem.DataMap;
    //        string message = dataMapItem.DataMap.GetString("Message");

    //        Intent intent = new Intent();
    //        intent.SetAction(Intent.ActionSend);
    //        intent.PutExtra("WearMessage", message);
    //        LocalBroadcastManager.GetInstance(this).SendBroadcast(intent);
    //    }
    //}
}