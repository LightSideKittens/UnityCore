using System;
using System.Threading.Tasks;
using Core.Server;
using Firebase.Auth;
using LSCore.Async;
using LSCore.ConfigModule;
using LSCore.Firebase;
using Newtonsoft.Json;
using Random = UnityEngine.Random;

namespace LSCore.Server
{
    public class User : BaseConfig<User>
    {
        public const long MaxAllowedSize = 4 * 1024 * 1024;
        private const string Undefined = nameof(Undefined);
        [JsonProperty] private string id = Undefined;
        [JsonProperty] private string nickName = "Non-Authorized";

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

        public static string Id
        {
            get => Config.id;
            private set
            {
                var config = Config;
                config.id = value;
                config.nickName = nickNames[Random.Range(0, nickNames.Length)];
            }
        }
        
        public static string NickName => Config.nickName;
        public static bool FakeSignIn { get; set; }

        private static FirebaseAuth Auth => FirebaseAuth.DefaultInstance;

        static User() => Disposer.Disposed += () => Auth.Dispose();
        
        public static void SignIn(Action onSuccess, Action onError = null)
        {
            var userId = Id;
            if (userId == Undefined)
            {
                var utc = DateTime.UtcNow;
                Id = $"Y{utc.Year}M{utc.Month}D{utc.Day}h{utc.Hour}m{utc.Minute}s{utc.Second}ms{utc.Millisecond}";
                SignInByEmail(true, onSuccess, onError);
            }
            else if(Auth.CurrentUser == null)
            {
                SignInByEmail(false, onSuccess, onError);
            }
            else
            {
                onSuccess.SafeInvoke();
            }
        }
        

        private static void SignInByEmail(bool isNewUser, Action onSuccess, Action onError)
        {
            if (FakeSignIn)
            {
                onSuccess.SafeInvoke();
                return;
            }
            
            GetTask(isNewUser).OnComplete(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Burger.Log(isNewUser
                        ? $"[{nameof(User)}] New user created! UserId: {Id}"
                        : $"[{nameof(User)}] Sign In! UserId: {Id}");
                    onSuccess.SafeInvoke();
                }
                else
                {
                    var exceptionText = task.Exception.ToString();
                    
                    if (exceptionText.Contains("One or more errors occurred"))
                    {
                        exceptionText = "The Authentication service may not be enabled in the Firebase console. Link: https://console.firebase.google.com";
                    }
                    
                    Burger.Error($"[{nameof(User)}] { task.Exception} {exceptionText}");
                    onError.SafeInvoke();
                }
            });
        }

        private static Task GetTask(bool isNewUser)
        {
            var email = $"{Id}@player.com";
            var password = Id;

            return isNewUser
                ? Auth.CreateUserWithEmailAndPasswordAsync(email, password)
                : Auth.SignInWithEmailAndPasswordAsync(email, password);
        }
    }
}