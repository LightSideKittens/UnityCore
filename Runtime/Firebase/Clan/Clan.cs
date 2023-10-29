using System;
using System.Collections.Generic;
using Firebase.Firestore;
using LSCore.Async;
using LSCore.Firebase;
using LSCore.Server;
using UnityEngine;

namespace LSCore
{
    public static class Clan
    {
        private const string ClanKey = "clan";
        private const string RoleKey = "role";
        private const string MembersKey = "members";
        private const int PageSize = 10;

        private static FirebaseData clanDeletion;
        private static CollectionReference clansRef;

        public static string Id { get; private set; }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            clansRef = User.Database.Collection("Clans");
            clanDeletion = FirebaseData.Create(ClanKey, FieldValue.Delete);
        }

        public static LSTask Request(Action<LSTask> onAuthSuccess)
        {
            return Request(LSTask.WrapAction(onAuthSuccess));
        }

        public static LSTask<T> Request<T>(Action<LSTask<T>> onSuccess)
        {
            return User.Request<T>(result =>
            {
                GetClan(User.Id).OnComplete(clanTask =>
                {
                    var clanId = clanTask.Result;
                    if (!string.IsNullOrEmpty(clanId))
                    {
                        onSuccess(result);
                        return;
                    }

                    result.Error("Failed GetClan attempt");
                });
            });
        }

        public static LSTask<Data> Create(string clanName)
        {
            return User.Request<Data>(result =>
            {
                GetClan(User.Id).OnComplete(_ =>
                {
                    var clanId = GenerateClanID(clanName);

                    if (!string.IsNullOrEmpty(Id))
                    {
                        result.Error($"Cannot create the clan {clanId}. You are already in the clan {Id}");
                        return;
                    }

                    var data = new Data()
                    {
                        Name = clanName,
                        LeaderId = User.Id,
                        CountryCode = User.CountryCode,
                    };

                    var batch = ClanBatch(clanId, Role.Leader);
                    var clanDoc = clansRef.Document(clanId);
                    batch.Set(clanDoc, data);
                    batch.CommitAsync().SetupOnComplete(result, () =>
                    {
                        Id = clanId;
                        return data;
                    });
                });
            });
        }

        public static LSTask<Data> Join(string clanId)
        {
            return User.Request<Data>(result =>
            {
                GetClan(User.Id).OnComplete(_ =>
                {
                    if (!string.IsNullOrEmpty(Id))
                    {
                        result.Error($"Cannot join the clan {clanId}. You are already in the clan {Id}");
                        return;
                    }

                    clansRef.Document(clanId).GetSnapshotAsync().OnComplete(clanTask =>
                    {
                        if (!clanTask.IsSuccess || !clanTask.Result.Exists)
                        {
                            result.Error();
                            return;
                        }

                        ClanBatch(clanId, Role.Member).CommitAsync().SetupOnComplete(result, () =>
                        {
                            Id = clanId;
                            return clanTask.Result.ConvertTo<Data>();
                        });
                    });
                });
            });
        }

        public static LSTask Delete()
        {
            return Request(result =>
            {
                var clanRef = clansRef.Document(Id);
                var members = clansRef.Document(Id).Collection(MembersKey);

                User.Database.RunTransactionAsync(transaction =>
                {
                    var task = LSTask.Create();

                    CheckRole(Role.Leader).OnComplete(selfTask =>
                    {
                        if (IsFailedRoleTask(selfTask, task, "to delete this clan."))
                        {
                            return;
                        }
                        
                        members.GetSnapshotAsync().OnComplete(membersTask =>
                        {
                            if (!membersTask.IsSuccess || membersTask.Result.Count < 1)
                            {
                                result.Error();
                                task.Error();
                                return;
                            }

                            foreach (var memberDocSnap in membersTask.Result)
                            {
                                var id = memberDocSnap.Id;
                                var memberDoc = User.GetMainInfoDoc(id);
                                var memberClanDoc = members.Document(id);
                                transaction.Update(memberDoc, clanDeletion);
                                transaction.Delete(memberClanDoc);
                            }

                            transaction.Delete(clanRef);
                            task.Success();
                        });
                    });

                    return task;
                }).SetupOnComplete(result);
            });
        }

        public static LSTask AssignRole(string userId, Role role)
        {
            var result = LSTask.Create();
            CheckRole(Role.Leader).OnComplete(roleTask =>
            {
                if (IsFailedRoleTask(roleTask, result, "to assign the role."))
                {
                    return;
                }
                
                RunMemberUpdate(userId, () => GetRole(role)).SetupOnComplete(result);
            });
            
            return result;
        }

        public static LSTask Leave()
        {
            return Kick(User.Id);
        }

        public static LSTask Kick(string userId)
        {
            return Request(result =>
            {
                if (userId == User.Id)
                {
                    KickUser();
                    return;
                }
                
                CheckRole(Role.Leader).OnComplete(selfTask =>
                {
                    if (IsFailedRoleTask(selfTask, result, "to kick members."))
                    {
                        return;
                    }

                    KickUser();
                });
                return;

                void KickUser()
                {
                    User.Database.RunTransactionAsync(transaction =>
                    {
                        var member = clansRef.Document(Id).Collection(MembersKey).Document(userId);
                        var userMainInfo = User.GetMainInfoDoc();
                        transaction.Delete(member);
                        transaction.Update(userMainInfo, clanDeletion);
                        result.Success();
                        return result;
                    });
                }
            });
        }

        
        
        public static LSTask<bool> CheckRole(Role role)
        {
            var result = LSTask<bool>.Create();

            var self = clansRef.Document(Id).Collection(MembersKey).Document(User.Id);
            self.GetSnapshotAsync().OnComplete(selfTask =>
            {
                var selfResult = selfTask.Result;
                if (!selfTask.IsSuccess)
                {
                    result.Error(selfTask.Exception);
                    return;
                }
                
                if(!selfResult.Exists)
                {
                    result.Error($"User {User.Id} is not in a clan {Id}");
                    return;
                }

                var selfRole = (Role)selfResult.GetValue<int>(RoleKey);

                if (selfRole != role)
                {
                    result.Success(false);
                    return;
                }

                result.Success(true);
            }).SetupOnComplete(result);

            return result;
        }

        private static bool IsFailedRoleTask(LSTask<bool> roleTask, LSTask task, string message)
        {
            if (!roleTask.Result)
            {
                if (!roleTask.IsSuccess)
                {
                    task.Error(roleTask.Exception);
                    return true;
                }
                
                task.Error($"You don't have permission {message}");
                return true;
            }

            return false;
        }

        public static LSTask<(List<Data> clans, DocumentSnapshot last)> Search(string name, DocumentSnapshot startAfter = null)
        {
            var result = LSTask<(List<Data> clans, DocumentSnapshot last)>.Create();
            var inRegionQuery = clansRef
                .WhereEqualTo("countryCode", User.CountryCode)
                .OrderBy("name")
                .StartAt(name)
                .EndAt(name + "\uf8ff")
                .Limit(PageSize);

            if (startAfter != null)
            {
                inRegionQuery = inRegionQuery.StartAfter(startAfter);
            }

            inRegionQuery.GetSnapshotAsync().OnComplete(clansDoc =>
            {
                if (clansDoc.Result != null)
                {
                    var clans = new List<Data>();
                    DocumentSnapshot last = null;

                    foreach (var doc in clansDoc.Result)
                    {
                        clans.Add(doc.ConvertTo<Data>());
                        last = doc;
                    }

                    result.Success((clans, last));
                    return;
                }

                result.Error();
            });

            return result;


        }

        public static LSTask<bool> IsExist(string clanId)
        {
            var result = LSTask<bool>.Create();

            clansRef.Document(clanId).GetSnapshotAsync().OnComplete(snapshot =>
            {
                result.Success(snapshot.Result.Exists);
            }).SetupOnComplete(result);

            return result;
        }

        public static LSTask<bool> IsContains()
        {
            return IsContains(User.Id);
        }

        private static LSTask<bool> IsContains(string userId)
        {
            var result = LSTask<bool>.Create();
            GetClan(userId).OnComplete(clanTask => { result.Success(clanTask.Result == Id); }).SetupOnComplete(result);

            return result;
        }

        private static WriteBatch ClanBatch(string clanId, Role role)
        {
            var batch = User.Database.StartBatch();
            var memberDoc = GetMemberDoc(clanId, User.Id);

            batch.Set(memberDoc, GetRole(role));
            batch.Set(User.GetMainInfoDoc(), FirebaseData.Create(ClanKey, clanId));
            return batch;
        }

        private static FirebaseData GetRole(Role role) => FirebaseData.Create(RoleKey, (int)role);

        private static LSTask RunMemberUpdate(string userId, Func<IDictionary<string, object>> updates)
        {
            return Request(result =>
            {
                if (string.IsNullOrEmpty(Id))
                {
                    result.Error();
                    return;
                }

                var memberDoc = GetMemberDoc(Id, userId);

                User.Database.RunTransactionAsync(transaction =>
                {
                    return transaction.GetSnapshotAsync(memberDoc).ContinueWith(memberSnapshotTask =>
                    {
                        if (memberSnapshotTask.IsCompletedSuccessfully && memberSnapshotTask.Result.Exists)
                        {
                            transaction.Update(memberDoc, updates());
                            return;
                        }

                        result.Error();
                    });
                }).SetupOnComplete(result);
            });
        }

        private static DocumentReference GetMemberDoc(string clanId, string userId)
        {
            return clansRef.Document(clanId).Collection(MembersKey).Document(userId);
        }

        public static LSTask<string> GetClan(string userId)
        {
            var result = LSTask<string>.Create();

            var mainInfoDoc = User.GetMainInfoDoc(userId).GetSnapshotAsync();
            mainInfoDoc.SetupOnComplete(result,
                () => Id = mainInfoDoc.Result.TryGetValue(ClanKey, out string id) ? id : string.Empty);

            return result;
        }

        private static string GenerateClanID(string clanName)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"{clanName}{timestamp}";
        }

        [FirestoreData]
        public struct Data
        {
            [FirestoreProperty] public string Name { get; set; }
            [FirestoreProperty] public string LeaderId { get; set; }
            [FirestoreProperty] public string CountryCode { get; set; }
        }

        [FirestoreData]
        public enum Role
        {
            Leader,
            Officer,
            Member
        }

        public struct ChatMessage
        {
            public string sender;
            public string content;
            public object timestamp; // Use object because Firebase's server timestamp is a placeholder value.
        }
    }
}