using System;
using System.Collections.Generic;
using System.Linq;

namespace LSCore.BattleModule.States
{
    [Serializable]
    public class UnitStates : StateManager
    {
        protected override void OnRegister() => Reg(this);
    }

    [Serializable]
    public struct State : IEquatable<State>
    {
        public string name;
        public int priority;

        public bool Equals(State other)
        {
            return name == other.name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
    
    [Serializable]
    public abstract class StateManager : BaseComp
    {
        public HashSet<State> activeStates = new();
        public event Action<State> StateEnabled;
        public event Action<State> StateDisabled;
        
        public bool IsActive(State state)
        {
            return activeStates.Contains(state);
        }

        public void RemoveState(State state)
        {
            activeStates.Remove(state);
            StateDisabled?.Invoke(state);
        }
        
        public bool TrySetState(State state)
        {
            var canAdd = activeStates.All(s => state.priority >= s.priority);
            if (!canAdd) return false;
            
            if (!state.Equals(default) && !activeStates.Contains(state))
            {
                activeStates.RemoveWhere(target => 
                {
                    if (state.priority > target.priority)
                    {
                        StateDisabled?.Invoke(target);
                        return true;
                    }
                        
                    return false;
                });
                
                activeStates.Add(state);
                StateEnabled?.Invoke(state);
                
                return true;
            }
            
            return false;
        }
    }
}