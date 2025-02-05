using System;
using DG.Tweening;
using LSCore.Async;

namespace LSCore
{
    public partial class PlayerWebSocketClient : BaseWebSocketClient
    {
        protected override bool IsEditor => false;

        private Tween loop;

        protected override void OnOpen()
        {
            base.OnOpen();
            loop = Wait.InfinityLoop(1, OnLoop);
        }

        private void OnLoop()
        {
            
        }
    }
}