#if UNITY_EDITOR
using System;
using Core.Server;
using Firebase;
using Firebase.Auth;
using LSCore.Async;
using UnityEditor;

namespace LSCore.Auth
{
    [InitializeOnLoad]
    public static class Admin
    {
        public static FirebaseApp App { get; }
        private static readonly FirebaseAuth auth;
        private static bool isSingedIn;
        
        static Admin()
        {
            App = FirebaseApp.Create(FirebaseApp.DefaultInstance.Options, "FIREBASE_EDITOR");
            auth = FirebaseAuth.GetAuth(App);
        }
        
        public static void SignIn(Action onSuccess, Action onError = null)
        {
            if (isSingedIn)
            {
                onSuccess.SafeInvoke();
                return;
            }

            auth.SignInWithEmailAndPasswordAsync("firebase@admin.com", "firebaseadmin")
                .OnComplete(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        isSingedIn = true;
                        Burger.Log("Success Auth as Admin!");
                        onSuccess.SafeInvoke();
                    }
                    else
                    {
                        onError.SafeInvoke();
                        Burger.Error($"Failure Sign In. Error: {task.Exception.Message}");
                    }
                });
        }
    }
}
#endif