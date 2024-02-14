using System;

namespace LSCore.BattleModule
{
    public class CompData
    {
        public Action onInit;
        public Action reset;
        public Action enable;
        public Action disable;
        public Action destroy;
        public Action update;
        public Action fixedUpdate;
    }
}