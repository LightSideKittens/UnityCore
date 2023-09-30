#if UNITY_EDITOR
using System.Collections.Generic;

namespace LSCore.Async
{
    public static partial class Wait
    {
        public partial class WhenAllActions
        {
            private string createdPlace;
            private HashSet<string> waitingPlaces = new();

            partial void SetCreatePlace()
            {
                createdPlace = UniTrace.Create(3);
            }

            partial void RegisterAction(string place)
            {
                waitingPlaces.Add($"{place}");
            }
            
            partial void UnRegisterAction(string place)
            {
                waitingPlaces.Remove($"{place}");
            }
        }
    }
}
#endif