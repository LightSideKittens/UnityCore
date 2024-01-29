using System;
using System.Collections.Generic;

namespace LSCore
{
    public struct WindowsData
    {
        internal static Action hidePrevious;
        internal static int maxSortingOrder;
        
        private static readonly Stack<Action> states = new();
        private static Action goHome;
        private static Action hideHome;
        private static bool recordStates;
        private static Action recordedState;

        static WindowsData()
        {
            World.Created += Clear;
        }
        
        internal static void GoBack() => states.Pop()();
        internal static void HidePrevious() => hidePrevious?.Invoke();

        internal static void StartRecording()
        {
            recordStates = true;
            recordedState = null;
        }

        internal static void Record(Action state)
        {
            if (recordStates)
            {
                recordedState += state;
            }
        }

        internal static void StopRecording()
        {
            recordStates = false;
            if (recordedState != null)
            {
                states.Push(recordedState);
            }
        }

        internal static void GoHome()
        {
            states.Clear();
            var hideAction = hidePrevious;
            hidePrevious = null;
            maxSortingOrder = 0;
            goHome();
            var hideDelegate = (Delegate)hideHome;

            foreach (var delegat in hideAction.GetInvocationList())
            {
                if (delegat == hideDelegate)
                {
                    continue;
                }
                
                ((Action)delegat)();
            }

            hidePrevious = hideHome;
            maxSortingOrder = 1;
        }
        
        internal static void SetHome<T>(Action hide) where T : BaseWindow<T>
        {
            Clear();
            hideHome = hide;
            goHome += BaseWindow<T>.Show;
        }

        private static void Clear()
        {
            states.Clear();
            hidePrevious = null;
            goHome = null;
            hideHome = null;
            recordStates = false;
            recordedState = null;
            maxSortingOrder = 0;
        }

        public static bool IsPreLast<T>(T window) where T : BaseWindow<T>
        {
            if (states.TryPeek(out var state))
            {
                return Check(state) || Check(hidePrevious);
            }
            
            return false;

            bool Check(Action action)
            {
                if (action.Target == null)
                {
                    return action.GetInvocationList()[^2].Target == window;
                }
                
                return action.Target == window;
            }
        }
    }
}