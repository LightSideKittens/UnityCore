using System;
using UnityEngine;

public class DeviceSimulatorExt : BaseWindowExtender
{
    public static Type Type { get; private set; }
    
    protected override Type GetWindowType()
    {
        Type = Type.GetType("UnityEditor.DeviceSimulation.SimulatorWindow,UnityEditor.DeviceSimulatorModule");
        return Type;
    }

    public override void OnPreGUI()
    {
    }

    public override void OnPostGUI()
    {
    }

    public static void Repaint()
    {
        GUI.changed = true;
    }
}