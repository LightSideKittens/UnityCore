using System;
using System.Collections.Generic;
using LSCore.Extensions;
using UnityEngine;

namespace LSCore
{
    public enum ShowWindowOption
    {
        None,
        HidePrevious,
        HideAllPrevious,
    }
    
    public struct UIViewBoss
    {
        public readonly struct UseId : IDisposable
        {
            private readonly string prevId;

            public UseId(string id)
            {
                prevId = Id;
                
                if (!string.IsNullOrEmpty(id))
                {
                    Id = id;
                }
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
        internal static Dictionary<string, WindowsGroup> groups = new();
        
        internal static string Id { get; private set; } = DefaultId;
        
        internal static WindowsGroup Current
        {
            get
            {
                if (!groups.TryGetValue(Id, out var result))
                {
                    groups[Id] = result = new WindowsGroup(Id);
                }
                
                return result;
            }
        }

        public static bool IsGoBack { get; private set; }
        public static bool IsHidePrevious { get; private set; }
        public static bool IsHideAllPrevious { get; private set; }


        static UIViewBoss()
        {
#if UNITY_EDITOR
            World.Destroyed += Clear;
#endif
            World.Updated += Update;
        }

        private static void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var lastManager = Current.LastManager;
                if (lastManager.blockSystemGoBack) return;
                if (lastManager.systemGoBackOverride != null)
                {
                    lastManager.systemGoBackOverride.Do();
                }
                else
                {
                    GoBack();
                }
            }
        }

        private static void Clear()
        {
            IsGoBack = false;
            IsHidePrevious = false;
            IsHideAllPrevious = false;
            Id = DefaultId;
            sortingOrder = DefaultSortingOrder;
            groups = new ()
            {
                { DefaultId, new WindowsGroup(DefaultId) },
            };
            
            ClearInstances();
        }
        
        private static void ClearInstances()
        {
            foreach (var instance in groups.Values)
            {
                instance.Clear();
            }
        }

        public static void GoBack()
        {
            IsGoBack = true;
            Current.GoBack();
            IsGoBack = false;
        }

        private static void OnGoBacksEmpty(WindowsGroup instance)
        {
            groups.Remove(instance.Id);
        }

        private static void HidePrevious()
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

        public static void GoHome() =>  Current.GoHome();

        internal static void SetHome(WindowManager manager) => Current.SetHome(manager);
        internal static bool IsAt(WindowManager manager, Index index) => Current.IsAt(manager, index);
        public static void CallOption(ShowWindowOption option) => Current.CallOption(option);

        public static bool IsHome(WindowManager manager) => Current.IsHome(manager);

        public class WindowsGroup
        {
            internal string Id { get; }

            public WindowsGroup(string id)
            {
                Id = id;
            }
            
            internal Action hideAllPrevious;
            private readonly Stack<Action> states = new();
            private Action goHome;
            private bool isRecording;
            private Action recordedState;

            public WindowManager LastManager
            {
                get
                {
                    var delegates = hideAllPrevious.GetInvocationList();
                    return (WindowManager)delegates[^1].Target;
                }
            }
            
            internal void GoBack()
            {
                if (goHome != null)
                {
                    var delegates = hideAllPrevious.GetInvocationList();
                    if (delegates.Length == 1 && delegates[0].Target == goHome.Target)
                    {
                        return;
                    }
                }
                
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
                ((Action)hideAllPrevious?.GetInvocationList()[^1])?.Invoke();
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
                if (isRecording) return;

                isRecording = true;
                recordedState = null;
            }

            internal void Record(Action state)
            {
                if (isRecording)
                {
                    recordedState += state;
                }
            }

            internal void StopRecording()
            {
                if (!isRecording) return;

                isRecording = false;
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
                hideAllPrevious = null;
                goHome = null;
                isRecording = false;
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
                if(goHome == null) return false;
                return goHome.Target == manager;
            }
        }
    }
}