using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.RemoteConfig;
using LSCore;

public static class Server
{
    public class Services
    {
        public FirebaseApp App { get; }
        public FirebaseAuth Auth => FirebaseAuth.GetAuth(App);
        public FirebaseFirestore DB => FirebaseFirestore.GetInstance(App);
        public FirebaseRemoteConfig RemoteConfig => FirebaseRemoteConfig.GetInstance(App);
        
        public Services(FirebaseApp app)
        {
            App = app;
        }
    }

    [ResetStatic] private static Services common;
    public static Services Common
    {
        get
        {
            if (common == null)
            {
                var sharedOptions = new AppOptions()
                {
                    AppId = "1:227253067810:android:5d3323aa6f70b5b6746a7b",
                    ProjectId = "light-side-llc",
                    ApiKey = "AIzaSyD3eA9AoJR0cR5prhBU2lt-SZu2Zi-Fzt4",
                    MessageSenderId = "227253067810",
                    StorageBucket = "light-side-llc.firebasestorage.app",
                    DatabaseUrl = new System.Uri("https://light-side-llc-default-rtdb.firebaseio.com")
                };
                
                common = new(FirebaseApp.Create(sharedOptions));
            }

            return common;
        }
    }

    [ResetStatic] private static Services thiz;
    public static Services This => thiz ??= new(FirebaseApp.DefaultInstance);
}