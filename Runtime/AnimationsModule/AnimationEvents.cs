using System.Collections.Generic;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    public class AnimationEvents : MonoBehaviour
    {
        [SerializeReference] public List<LSAction> actions;

        public void Invoke(int index)
        {
            actions[index].Invoke();
        }
    }
}