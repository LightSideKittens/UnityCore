using System;
using UnityEngine;

namespace LSCore
{
    public interface IClickable
    {
        Transform Transform { get; }
        Action Clicked { get; set; }
    }
}