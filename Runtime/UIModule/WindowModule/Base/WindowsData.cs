using System;
using System.Collections.Generic;
using Core.Server;
using UnityEngine;

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
        public readonly struct UseId : IDisposable
        {
            private readonly string prevId;

            public UseId(string id)
            {
                prevId = Id;
                Id = id;
            }
            
            public void Dispose()
            {
                Id = prevId;
            }
        }
        
        public const int DefaultSortingOrder = 100;
        public const string DefaultId = "Default";

        private static readonly Dictionary<ShowWindowOption, Action> showWindowOptions = new()
        {
            { ShowWindowOption.HidePrevious, HidePrevious },
            { ShowWindowOption.HideAllPrevious, HideAllPrevious },
        };

        internal static int sortingOrder = DefaultSortingOrder;
        internal static Dictionary<string, WindowsDataInstance> instances = new();
        public static string Id { get; private set; } = DefaultId;
        public static WindowsDataInstance Current
        {
            get
            {
                if (!instances.TryGetValue(Id, out var result))
                {
                    instances[Id] = result = new WindowsDataInstance(Id);
                }
                
                return result;
            }
        }

        internal static bool IsGoBack { get; private set; }
        internal static bool IsHidePrevious { get; private set; }
        internal static bool IsHideAllPrevious { get; private set; }

        static WindowsData()
        {
            World.Destroyed += Clear;
        }

        private static void Clear()
        {
            IsGoBack = false;
            IsHidePrevious = false;
            IsHideAllPrevious = false;
            Id = DefaultId;
            sortingOrder = DefaultSortingOrder;
            instances = new ()
            {
                { DefaultId, new WindowsDataInstance(DefaultId) },
            };
            
            ClearInstances();
        }
        
        private static void ClearInstances()
        {
            foreach (var instance in instances.Values)
            {
                instance.Clear();
            }
        }

        internal static void GoBack()
        {
            IsGoBack = true;
            Current.GoBack();
            IsGoBack = false;
        }

        internal static void OnGoBacksEmpty(WindowsDataInstance instance)
        {
            instances.Remove(instance.Id);
        }
        
        internal static void HidePrevious()
        {
            IsHidePrevious = true;
            Current.HidePrevious();
            IsHidePrevious = false;
        }

        internal static void HideAllPrevious()
        {
            IsHideAllPrevious = true;
            Current.HideAllPrevious();
            IsHideAllPrevious = false;
        }

        internal static void StartRecording()
        {
            Current.StartRecording();
        }

        internal static void Record(Action state)
        {
            Current.Record(state);
        }

        internal static void StopRecording()
        {
            Current.StopRecording();
        }

        internal static void GoHome() =>  Current.GoHome();

        internal static void SetHome(WindowManager manager) => Current.SetHome(manager);
        internal static bool IsAt(WindowManager manager, Index index) => Current.IsAt(manager, index);
        public static void CallOption(ShowWindowOption option) => Current.CallOption(option);

        public static bool IsHome(WindowManager manager) => Current.IsHome(manager);

        public class WindowsDataInstance
        {
            internal string Id { get; private set; }

            public WindowsDataInstance(string id)
            {
                Id = id;
            }
            
            internal Action hidePrevious;
            internal Action hideAllPrevious;
            private readonly Stack<Action> states = new();
            private Action goHome;
            private bool recordStates;
            private Action recordedState;

            internal void GoBack()
            {
                IsGoBack = true;
                
                if (states.TryPop(out var action))
                {
                    action.InverseInvoke();
                }
                
                if (states.Count == 0)
                {
                    OnGoBacksEmpty(this);
                }

                IsGoBack = false;
            }

            internal void HidePrevious()
            {
                IsHidePrevious = true;
                hidePrevious?.Invoke();
                IsHidePrevious = false;
            }

            internal void HideAllPrevious()
            {
                IsHideAllPrevious = true;
                hideAllPrevious?.InverseInvoke();
                IsHideAllPrevious = false;
            }

            internal void StartRecording()
            {
                if (recordStates) return;

                recordStates = true;
                recordedState = null;
            }

            internal void Record(Action state)
            {
                if (recordStates)
                {
                    recordedState += state;
                }
            }

            internal void StopRecording()
            {
                if (!recordStates) return;

                recordStates = false;
                if (recordedState != null)
                {
                    states.Push(recordedState);
                }
            }

            internal void GoHome() => goHome();

            internal void SetHome(WindowManager manager)
            {
                Clear();
                goHome = manager.HideAllPreviousAndShow;
            }

            public void Clear()
            {
                states.Clear();
                hidePrevious = null;
                hideAllPrevious = null;
                goHome = null;
                recordStates = false;
                recordedState = null;
            }

            internal bool IsAt(WindowManager manager, Index index)
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

            public void CallOption(ShowWindowOption option)
            {
                if (IsGoBack) return;

                if (showWindowOptions.TryGetValue(option, out var action))
                {
                    action();
                }
            }

            public bool IsHome(WindowManager manager)
            {
                return goHome.Target == manager;
            }
        }
    }
}