using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DG.Tweening;
using LSCore;
using LSCore.AnimationsModule;
using LSCore.AnimationsModule.Animations;
using LSCore.Attributes;
using LSCore.ConditionModule;
using LSCore.ConfigModule;
using LSCore.DataStructs;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class If_LSImage_Is_Visible : If
{
    public LSImage image;
    protected override bool Check() => image.IsShowed;
}

[Serializable]
public class OnLSImageShowed : DoIt
{
    [GetContext] public Object owner;
    public LSImage image;
    [SerializeReference] public DoIt doIt;
    
    public override void Do()
    {
        DestroyEvent.AddOnDestroy(owner, () => image.Showed -= doIt.Do);
        image.Showed += doIt.Do;
    }
}

[Serializable]
public class OnJTokenGameConfigChanged : DoIt
{
    [GetContext] public Object owner;
    [SerializeReference] public Get<string> path;
    [SerializeReference] public DoIt doIt;
    
    public override void Do()
    {
        var config = JTokenGameConfig.Get(path);
        DestroyEvent.AddOnDestroy(owner, () => config.PropertyChanged -= OnChanged);
        config.PropertyChanged += OnChanged;
    }

    private void OnChanged(object sender, PropertyChangedEventArgs e)
    {
        doIt.Do();
    }
}

[Serializable]
public class DeleteJTokenGameConfig : DoIt
{
    [SerializeReference] public Get<string> path;
    
    public override void Do()
    {
        JTokenGameConfig.GetManager(path).Delete();
    }
}

[Serializable]
public class ViewState : MonoBehaviour
{
    [Serializable]
    public abstract class Changer
    {
        public Save viewSave;
        protected abstract string ViewJObjectKey { get; }
        protected JObject ViewJObject => viewSave.Config.AsJ<JObject>(ViewJObjectKey);
        public abstract void Init();
        public virtual bool CanChange => true;

        public void PrepareToChange()
        {
            if (CanChange)
            {
                OnPrepareToChange();
            }
        }

        public void Change(Action onComplete)
        {
            if (CanChange)
            {
                OnChange(onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }
        
        protected virtual void OnPrepareToChange(){}
        protected abstract void OnChange(Action onComplete);
    }
    
    [Serializable]
    public abstract class BaseSliderChanger : Changer
    {
        public LSSliderAnim sliderAnim;
        
        public override void Init()
        {
            sliderAnim.FirstTarget.value = SavedValue;
        }

        public override bool CanChange => ActualValue != SavedValue;

        protected override void OnChange(Action onComplete)
        {
            sliderAnim.endValue = ActualValue;
            sliderAnim.Animate().OnComplete(onComplete.Invoke);
            SavedValue = ActualValue; 
        }

        protected abstract int ActualValue { get; }
        protected abstract int SavedValue { get; set; }
    }
    
    [Serializable]
    public abstract class Switcher : Changer
    {
        [Serializable]
        public struct StateEntry
        {
            [SerializeReference] public DoIt enter;
            [SerializeReference] public DoIt exit;
        }
        
        [Serializable]
        public struct TransitionAnimKey
        {
            public string from;
            public string to;

            public TransitionAnimKey(string from, string to)
            {
                this.from = from;
                this.to = to;
            }
        }
        
        public UniDict<string, StateEntry> stateSetters;

        public UniDict<string, string> initializationTransitionPaths;
        public UniDict<TransitionAnimKey, AnimSequencer> transitionAnims;
        public MultiListener listener;
        
        private string lastState;
        
        public override void Init()
        {
            lastState = SavedState;
            SetState(lastState, true);

            if (initializationTransitionPaths.TryGetValue(lastState, out var transitionPath))
            {
                var states = transitionPath.Split('/');
                for (int i = 0; i < states.Length - 1; i++)
                {
                    var key = new TransitionAnimKey(states[i], states[i + 1]);
                    var sequencer = transitionAnims[key];
                    var tween = sequencer.Animate();
                    tween.Complete();
                }
            }

            listener.Do();
        }

        private string SavedState
        {
            get
            {
                if (ViewJObject.TryGetValue("viewState", out var state))
                {
                    return state.ToString();
                }
                
                return DefaultState;
            }
            set => ViewJObject["viewState"] = value;
        }
        
        protected abstract string CurrentState { get; }
        protected abstract string DefaultState { get; }
        
        private string currentState;
        private AnimSequencer currentAnimSequencer;
        private Tween currentTween;
        
        protected override void OnPrepareToChange()
        {
            currentState = CurrentState;
            if (currentState == lastState)
            {
                return;
            }

            SetState(lastState, false);
            SetState(currentState, true);
            currentAnimSequencer = transitionAnims[new TransitionAnimKey(lastState, currentState)];
            lastState = currentState;
            SavedState = currentState;
        }
        
        protected override void OnChange(Action onComplete)
        {
            if (currentAnimSequencer == null)
            {
                onComplete();
                return;
            }
            
            currentTween?.Complete();
            currentTween = currentAnimSequencer.Animate().OnComplete(onComplete.Invoke);
            currentAnimSequencer = null;
        }

        private void SetState(string state, bool enter)
        {
            if(stateSetters.Count == 0) return;
            if (enter)
            {
                stateSetters[state].enter.Do();
            }
            else
            {
                stateSetters[state].exit?.Do();
            }
        }
    }
    
    public class Save
    {
        private string viewSavePath;
        public RJObject Config => config ?? JTokenGameConfig.Get(Path.Combine("ViewStates", viewSavePath));
        private RJObject config;

        public Save(string viewSavePath)
        {
            this.viewSavePath = viewSavePath;
        }
    }

    [Box] public string viewSavePath;
    [Box] [SerializeReference] public DoIt init;
    [Box] [SerializeReference] public If canChange;
    [SerializeReference] public Changer[] changers;
    public string[] listenKeysForChange;
    private Save save;
    
    protected virtual void Awake()
    {
        init?.Do();
        save = new Save(viewSavePath);
        for (int i = 0; i < changers.Length; i++)
        {
            changers[i].viewSave = save;
            changers[i].Init();
        }
    }

    protected virtual void OnEnable()
    {
        for (int i = 0; i < listenKeysForChange.Length; i++)
        {
            DoEventListener.Listen(listenKeysForChange[i], TryChange);
        }
        
        TryChange();
    }

    private void OnDisable()
    {
        for (int i = 0; i < listenKeysForChange.Length; i++)
        {
            DoEventListener.UnListen(listenKeysForChange[i], TryChange);
        }
    }

    private void TryChange()
    {
        if (canChange)
        {
            World.Updated -= Change;
            World.Updated += Change;
        }
    }
    
    private void Change()
    {
        World.Updated -= Change;
        
        int i;
        
        for (i = 0; i < changers.Length; i++)
        {
            changers[i].PrepareToChange();
        }

        i = 0;
        OnComplete();
            
        void OnComplete()
        {
            if (i < changers.Length)
            { 
                changers[i++].Change(OnComplete);
            }
        }
    }
}
