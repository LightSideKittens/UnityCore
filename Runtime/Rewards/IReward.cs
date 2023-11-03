using System;

namespace LSCore
{
    public interface IReward
    {
        bool Claim(out Action claim);
    }
}