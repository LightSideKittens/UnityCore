using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using LSCore.Async;
using LSCore.Firebase;
using LSCore.Server;

namespace LSCore
{
    public static class Leaderboard
    {
        public static List<(string playerId, string nickName, int score)> Entries { get; } = new();
        private static DatabaseReference databaseReference;

        static Leaderboard() => Disposer.Disposed += () => databaseReference = null;
        
        private static LSTask Init()
        {
            databaseReference ??= FirebaseDatabase.DefaultInstance.RootReference.Child("players");
            return User.SignIn();
        }

        public static LSTask AddScore(int score, string nickname)
        {
            return Init().OnComplete(task =>
            {
                if(!task.IsSuccess) return;
                
                var playerRef = databaseReference.Child(User.Id);
                playerRef.Child("nickName").SetValueAsync(nickname);
                playerRef.Child("score").SetValueAsync(score);
            });
        }
        
        public static LSTask FetchLeaderboardData()
        {
            var result = LSTask.Create();
            
            Init().OnComplete(authTask =>
            {
                if (!authTask.IsSuccess)
                {
                    result.Error();
                    return;
                }
                
                Entries.Clear();
                var query = databaseReference.OrderByChild("score");

                query.GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    var snapshot = task.Result;

                    if (snapshot == null)
                    {
                        result.Error();
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
                    result.Success();
                });
            });

            return result;
        }
    }
}