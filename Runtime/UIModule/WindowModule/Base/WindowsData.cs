using System;
using System.Collections.Generic;

namespace LSCore
{
    public enum ShowWindowOption
    {
        None,
        HidePrevious,
        HideAllPrevious,
    }
    
    public struct WindowsData
    {
        public const int DefaultSortingOrder = 100;

        private static readonly Dictionary<ShowWindowOption, Action> showWindowOptions = new()
        {
            { ShowWindowOption.HidePrevious, HidePrevious},
            { ShowWindowOption.HideAllPrevious, HideAllPrevious},
        };
        
        internal static Action hideAllPrevious;
        internal static int sortingOrder = DefaultSortingOrder;
        
        private static readonly Stack<Action> states = new();
        private static Action goHome;
        private static bool recordStates;
        private static Action recordedState;

        static WindowsData()
        {
            World.Destroyed += Clear;
        }
        
        internal static void GoBack() => states.Pop()();
        internal static void HidePrevious() => ((Action)hideAllPrevious?.GetInvocationList()[^1])?.Invoke();
        internal static void HideAllPrevious() => hideAllPrevious?.Invoke();

        internal static void StartRecording()
        {
            if(recordStates) return;
            
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
            if(!recordStates) return;
            
            recordStates = false;
            if (recordedState != null)
            {
                states.Push(recordedState);
            }
        }

        internal static void GoHome() => goHome();

        internal static void SetHome(WindowManager manager)
        {
            Clear();
            goHome += manager.GoHome;
        }

        private static void Clear()
        {
            states.Clear();
            hideAllPrevious = null;
            goHome = null;
            recordStates = false;
            recordedState = null;
            sortingOrder = DefaultSortingOrder;
        }

        internal static bool IsPreLast(WindowManager manager)
        {
            if (states.TryPeek(out var state) && hideAllPrevious != null)
            {
                return Check(state) || Check(hideAllPrevious);
            }
            
            return false;

            bool Check(Action action)
            {
                if (action.Target == null)
                {
                    return action.GetInvocationList()[^2].Target == manager;
                }
                
                return action.Target == manager;
            }
        }

        public static void CallOption(ShowWindowOption option)
        {
            if (showWindowOptions.TryGetValue(option, out var action))
            {
                action();
            }
        }
    }
}