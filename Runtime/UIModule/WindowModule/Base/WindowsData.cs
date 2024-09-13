using System;
using System.Collections.Generic;
using Core.Server;

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
            { ShowWindowOption.HidePrevious, HidePrevious },
            { ShowWindowOption.HideAllPrevious, HideAllPrevious },
        };

        internal static bool IsGoBack { get; private set; }
        internal static bool IsHidePrevious { get; private set; }
        internal static bool IsHideAllPrevious { get; private set; }
        internal static Action hidePrevious;
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
        
        internal static void GoBack()
        {
            IsGoBack = true;
            if (states.TryPop(out var action))
            {
                action.InverseInvoke();
            }
            IsGoBack = false;
        }

        internal static void HidePrevious()
        {
            IsHidePrevious = true;
            hidePrevious?.Invoke();
            IsHidePrevious = false;
        }

        internal static void HideAllPrevious()
        {
            IsHideAllPrevious = true;
            hideAllPrevious?.InverseInvoke();
            IsHideAllPrevious = false;
        }

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
            goHome = manager.HideAllPreviousAndShow;
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

        internal static bool IsAt(WindowManager manager, Index index)
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
                    return action.GetInvocationList()[index].Target == manager;
                }
                
                return action.Target == manager;
            }
        }

        public static void CallOption(ShowWindowOption option)
        {
            if(IsGoBack) return;
            
            if (showWindowOptions.TryGetValue(option, out var action))
            {
                action();
            }
        }

        public static bool IsHome(WindowManager manager)
        {
            return goHome.Target == manager;
        }
    }
}