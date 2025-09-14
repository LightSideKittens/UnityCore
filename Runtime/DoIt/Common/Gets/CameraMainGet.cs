using System;
using UnityEngine;

[Serializable]
public class CameraMainGet : Get<Camera>
{
    public override Camera Data => Camera.main;
}