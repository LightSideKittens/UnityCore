using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using LSCore.Async;
using LSCore.ConfigModule;
using LSCore.Extensions;
using LSCore.Firebase;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace LSCore.Server
{
    public class User : BaseConfig<User>
    {
        public const long MaxAllowedSize = 4 * 1024 * 1024;
        private const string FailAuthError = "Failed authentication attempt";
        private const string Undefined = nameof(Undefined);
        public static string Id => Auth.CurrentUser.UserId;
        
        [JsonProperty] private string nickname;
        [JsonProperty] private string countryCode;
        public static string Nickname => Config.nickname;
        public static string CountryCode => Config.countryCode;
        public static new User Config => BaseConfig<User>.Config;

        private static readonly string[] nickNames = 
        {
            "Captain Crunchwrap",
            "Sir Spam-a-lot",
            "Princess Pudding pop",
            "Count Quackula",
            "Duke of Deliciousness",
            "Lady Lollygag",
            "Baron Von Burrito",
            "The Great Gatsbyburger",
            "Dr. Doughnutstein",
            "The Burgermeister",
        };
        
        public static FirebaseAuth Auth => FirebaseAuth.DefaultInstance;
        public static FirebaseFirestore Database => FirebaseFirestore.DefaultInstance;

        static User() => Disposer.Disposed += () => Auth.Dispose();
        
        private static CollectionReference usersMainInfo;
        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            usersMainInfo = Database.Collection("UsersMainInfo");
        }

#if UNITY_EDITOR
        [MenuItem(LSConsts.Path.MenuItem.Root + "/Firebase/SignOut")]
        private static void SignOut() => Auth.SignOut();
#endif

        public static LSTask SignIn()
        {
            if (Auth.CurrentUser != null)
            {
                return LSTask.Completed;
            }
            
            var authTask = LSTask.Create();
            var nicknameTask = LSTask.Create();
            var setCountryTask = LSTask.Create();


            Auth.SignInAnonymouslyAsync().OnComplete(task =>
            {
                if (task.IsSuccess)
                {
                    Burger.Log($"[{nameof(User)}] New user created! UserId: {Id}");
                    var nickname = nickNames.Random();
                    SetNickName(nickname).SetupOnComplete(nicknameTask);
                    authTask.Success();
                    
                    Network.GetCountry().OnComplete(countryCode =>
                    {
                        if (string.IsNullOrEmpty(countryCode.Result))
                        {
                            setCountryTask.Error();
                        }
                        else
                        {
                            SetRegion(countryCode.Result).SetupOnComplete(setCountryTask);
                        }
                    });
                }
                else
                {
                    var error = task.Exception;
                    var exceptionText = error.ToString();
                    
                    if (exceptionText.Contains("One or more errors occurred"))
                    {
                        exceptionText = "The Authentication service may not be enabled in the Firebase console. Link: https://console.firebase.google.com";
                    }
                    
                    Burger.Error($"[{nameof(User)}] {error} {exceptionText}");
                    authTask.Error();
                }
            });
            
            return Task.WhenAll(authTask, nicknameTask, setCountryTask).OnComplete(result =>
            {
                if (result.IsError || result.IsCanceled)
                {
                    Auth.CurrentUser?.DeleteAsync();
                }
            });
        }

        public static void RunMainInfoTransaction(LSTask result, string userId, Action<Transaction, DocumentReference> onTransaction)
        {
            var mainInfoDoc = GetMainInfoDoc(userId);
            
            Database.RunTransactionAsync(transaction =>
            {
                return transaction.GetSnapshotAsync(mainInfoDoc).ContinueWith(memberSnapshotTask =>
                {
                    if (memberSnapshotTask.IsCompletedSuccessfully && memberSnapshotTask.Result.Exists)
                    {
                        onTransaction(transaction, mainInfoDoc);
                        return;
                    }
                    
                    result.Error();
                });
            }).SetupOnComplete(result);
        }

        public static LSTask Request(Action<LSTask> onAuthSuccess)
        {
            return Request(WrapAction(onAuthSuccess));
        }
        
        public static LSTask<T> Request<T>(Action<LSTask<T>> onAuthSuccess)
        {
            var task = LSTask<T>.Create();
            SignIn().OnComplete(authTask =>
            {
                if (authTask.IsSuccess)
                {
                    onAuthSuccess(task);
                    return;
                }
                
                task.Error(FailAuthError);
            });

            return task;
        }

        public static LSTask SetNickName(string nickname) => SetMainInfoField("nickname", nickname).OnComplete(task =>
        {
            if (task.IsSuccess)
            {
                Config.nickname = nickname;
            }
        });
        
        public static LSTask SetRegion(string countryCode) => SetMainInfoField("region", countryCode).OnComplete(task =>
        {
            if (task.IsSuccess)
            {
                Config.countryCode = countryCode;
            }
        });
        
        public static LSTask SetMainInfoField(string fieldName, object value)
        {
            var data = FirebaseData.Create(fieldName, value);
            return GetMainInfoDoc().SetAsync(data, SetOptions.MergeAll);
        }

        public static DocumentReference GetMainInfoDoc() => GetMainInfoDoc(Id);
        
        public static DocumentReference GetMainInfoDoc(string userId)
        {
            return usersMainInfo.Document(userId);
        }
        
        public static Action<LSTask<object>> WrapAction(Action<LSTask> action) => task => action(task);
        public static Action<LSTask<object>> WrapNullAction(Action<LSTask> action) => task => action?.Invoke(task);
    }
}