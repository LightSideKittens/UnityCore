//-----------------------------------------------------------------------
// <copyright file="UnitExample.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [AttributeExample(typeof(UnitAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "Sirenix.Utilities.Editor" })]
    internal class UnitExample
    {
        // Kilogram unit. Change the display by right-clicking.
        // Try entering '6 lb'.
        [Unit(Units.Kilogram)]
        public float Weight;

        // Meters per second unit, displayed as kilometers per hour in the inspector.
        // Try entering '15 mph'.
        [Unit(Units.MetersPerSecond, Units.KilometersPerHour)]
        public float Speed;

        // Meters, displayed as centimeters for finer control.
        [Unit(Units.Meter, Units.Centimeter)]
        public float Distance;

        // The speed value, shown as miles per hours. Excellent for debugging values in the inspector.
        [ShowInInspector, Unit(Units.MetersPerSecond, Units.MilesPerHour, DisplayAsString = true, ForceDisplayUnit = true)]
        public float SpeedMilesPerHour => Speed;

        // Add custom units. (Disabled to not add custom units to your project)
        //[InitializeOnLoadMethod]
        //public static void AddCustomUnit()
        //{
        //    UnitNumberUtility.AddCustomUnit(
        //      name: "Odin",
        //      symbols: new string[] { "odin" },
        //      unitCategory: UnitCategory.Distance,
        //      multiplier: 1m / 42m);
        //}

        // Use the custom unit by referencing it by name.
        //[Unit(Units.Meter, "Odin")]
        //public float Odins;
    }
}
#endif