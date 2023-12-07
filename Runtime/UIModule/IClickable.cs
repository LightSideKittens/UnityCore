using System;
using UnityEngine;

namespace LightSideCore.Runtime.UIModule
{
    public interface IClickable
    {
        Transform Transform { get; }
        Action Clicked { get; set; }
    }
}