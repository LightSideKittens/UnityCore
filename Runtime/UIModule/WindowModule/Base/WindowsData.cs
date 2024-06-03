using System;
using System.Collections.Generic;

namespace LSCore
{
    public struct WindowsData
    {
        public const int DefaultSortingOrder = 100;
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

        internal static void SetHome<T>() where T : BaseWindow<T>
        {
            Clear();
            goHome += BaseWindow<T>.GoHome;
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

        internal static bool IsPreLast<T>(T window) where T : BaseWindow<T>
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
                    return action.GetInvocationList()[^2].Target == window;
                }
                
                return action.Target == window;
            }
        }
    }
}