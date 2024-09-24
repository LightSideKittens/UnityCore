using LSCore.Attributes;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class FindTargetFactory : ScriptableObject
    {
        [Unwrap] [SerializeField] private FindTargetComp comp;
        public FindTargetComp Create()
        {
            var so = Instantiate(this);
            var result = so.comp;
            Destroy(so);
            return result;
        }
    }
}