using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using Newtonsoft.Json.Linq;
using Sendbird.Chat;

public static partial class BlaBla
{
    public static FirebaseFirestore Db => FirebaseFirestore.DefaultInstance; 
    
    public static class Club
    {
        public abstract class BaseJoiner
        {
            public abstract void Join(SbGroupChannel channel, Action joined);
        }
        
        public class DefaultJoiner : BaseJoiner
        {
            public override void Join(SbGroupChannel channel, Action joined)
            {
                var doc = Ref.Document(channel.Url);
                doc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
                {
                    
                });
                
                channel.Join(error =>
                {
                    if(HandleError(error)) return;
                    var dict = MetaTemplate;
                    doc.SetAsync(dict, SetOptions.MergeAll);
                    joined();
                });
            }
        }

        public static BaseJoiner Joiner { get; set; }
        public static Dictionary<string, object> MetaTemplate => new()
        {
            { "updatedAt", FieldValue.ServerTimestamp }
        };
        
        public static CollectionReference Ref => Db.Collection("clubs");
        
        public static void Create(SbGroupChannelCreateParams createParams, Action<SbGroupChannel> callback)
        {
            createParams.IsPublic = true;
            createParams.CustomType = "club";
            Dictionary<string, object> dict = new();
            
            if (!string.IsNullOrEmpty(createParams.Data))
            { 
                dict = JObject.Parse(createParams.Data).ToObject<Dictionary<string, object>>();
            }
            
            dict["updatedAt"] = FieldValue.ServerTimestamp;
            
            SendbirdChat.GroupChannel.CreateChannel(createParams, (channel, error) =>
            {
                if(HandleError(error)) return;
                
                var doc = Ref.Document(channel.Url);
                doc.SetAsync(dict).ContinueWithOnMainThread(task =>
                {
                    
                });
                callback(channel);
            });
        }

        public static void GetPageLoader(Query query, Action<SbPublicGroupChannelListQuery> callback)
        {
            query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                var snapshot = task.Result;
                var result = new List<string>();
                foreach (var doc in snapshot.Documents)
                {
                    result.Add(doc.Id);
                }
                
                if (result.Count == 0) return;

                SbPublicGroupChannelListQueryParams paramz = new SbPublicGroupChannelListQueryParams();
                paramz.ChannelUrlsFilter = result;
                paramz.Limit = 50;
                var query2 = SendbirdChat.GroupChannel.CreatePublicGroupChannelListQuery(paramz);
                callback(query2);
            });
        }
    }
}