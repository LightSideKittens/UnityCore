using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using Newtonsoft.Json.Linq;
using Sendbird.Chat;

public static partial class BlaBla
{
    public static FirebaseFirestore Db => FirebaseFirestore.DefaultInstance; 
    
    public class Club
    {
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

            dict["freePlaceCount"] = 29;
            dict["updatedAt"] = FieldValue.ServerTimestamp;
            
            SendbirdChat.GroupChannel.CreateChannel(createParams, (channel, error) =>
            {
                if(HandleError(error)) return;
                
                var doc = Ref.Document(channel.Url);
                doc.SetAsync(dict).ContinueWithOnMainThread(task =>
                {
                    if (HandleError(task.Exception))
                    {
                        channel.Delete(inError => HandleError(inError));
                        return;
                    }
                    
                    callback(channel);
                });
            });
        }
        
        public static void Join(SbGroupChannel channel, Action joined)
        {
            var doc = Ref.Document(channel.Url);
            doc.GetSnapshotAsync(Source.Server).ContinueWithOnMainThread(task =>
            {
                if(HandleError(task.Exception)) return;
                var result = task.Result.ToDictionary();

                if ((long)result["freePlaceCount"] > 0)
                {
                    channel.Join(error =>
                    {
                        if(HandleError(error)) return;
                        
                        var dict = MetaTemplate;
                        dict["freePlaceCount"] = FieldValue.Increment(-1);
                        doc.SetAsync(dict, SetOptions.MergeAll).ContinueWithOnMainThread(task2 =>
                        {
                            if (HandleError(task2.Exception))
                            {
                                channel.Leave(inError => HandleError(inError));
                                return;
                            }
                            
                            joined();
                        });
                    });
                }
            });
        }

        public static void Leave(SbGroupChannel channel, Action leaved)
        {
            var doc = Ref.Document(channel.Url);
            channel.Leave(error =>
            {
                if(HandleError(error)) return;
                        
                var dict = MetaTemplate;
                dict["freePlaceCount"] = FieldValue.Increment(1);
                doc.SetAsync(dict, SetOptions.MergeAll).ContinueWithOnMainThread(task2 =>
                {
                    if (HandleError(task2.Exception))
                    {
                        channel.Join(inError => HandleError(inError));
                        return;
                    }
                    
                    leaved();
                });
            });
        }

        public static void GetPageLoader(Query query, Action<SbPublicGroupChannelListQuery> callback)
        {
            query.GetSnapshotAsync(Source.Server).ContinueWithOnMainThread(task =>
            {
                var snapshot = task.Result;
                var result = new List<string>();
                foreach (var doc in snapshot.Documents)
                {
                    result.Add(doc.Id);
                }

                if (result.Count == 0)
                {
                    callback(null);
                    return;
                }

                SbPublicGroupChannelListQueryParams paramz = new SbPublicGroupChannelListQueryParams();
                paramz.ChannelUrlsFilter = result;
                paramz.Limit = 50;
                var query2 = SendbirdChat.GroupChannel.CreatePublicGroupChannelListQuery(paramz);
                callback(query2);
            });
        }
    }
}