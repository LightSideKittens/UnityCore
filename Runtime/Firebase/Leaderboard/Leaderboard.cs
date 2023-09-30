using System;
using System.Collections.Generic;
using Core.Server;
using Firebase.Database;
using Firebase.Extensions;
using LSCore.Firebase;
using LSCore.Server;

namespace LSCore.Leaderboard
{
    public static class Leaderboard
    {
        public static List<(string playerId, string nickName, int score)> Entries { get; } = new();
        private static DatabaseReference databaseReference;

        static Leaderboard() => Disposer.Disposed += () => databaseReference = null;
        
        private static void Init(Action onAuth, Action onError)
        {
            databaseReference ??= FirebaseDatabase.DefaultInstance.RootReference.Child("players");
            User.SignIn(onAuth, onError);
        }

        public static void AddScore(int score)
        {
            Init(() =>
            {
                DatabaseReference playerRef = databaseReference.Child(User.Id);
                playerRef.Child("nickName").SetValueAsync(User.NickName);
                playerRef.Child("score").SetValueAsync(score);
            }, null);
        }
        
        public static void FetchLeaderboardData(Action onComplete = null, Action onError = null)
        {
            Init(() =>
            {
                Entries.Clear();
                var query = databaseReference.OrderByChild("score");

                query.GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    var snapshot = task.Result;

                    if (snapshot == null)
                    {
                        onError.SafeInvoke();
                        return;
                    }
                
                    foreach (DataSnapshot playerSnapshot in snapshot.Children)
                    {
                        var playerId = playerSnapshot.Key;
                        var nickName = playerSnapshot.Child("nickName").Value.ToString();
                        var score = int.Parse(playerSnapshot.Child("score").Value.ToString());
                        Entries.Add((playerId, nickName, score));
                    }

                    Entries.Reverse();
                    onComplete.SafeInvoke();
                });
            }, onError);
        }
    }
}