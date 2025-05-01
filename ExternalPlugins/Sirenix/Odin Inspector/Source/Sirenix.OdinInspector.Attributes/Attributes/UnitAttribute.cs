//-----------------------------------------------------------------------
// <copyright file="UnitAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>Unit is used on number fields, and draws a number field in the inspector that allows for converting between units.</para>
    /// <para>
    /// Use this to clearly indicate what a number field is. Or to display a more human friendly value in the inspector, without
    /// changing how the value is used in code. For example, you can display a meter value in millimeters, for a more sensitive controls
    /// when editing the value.
    /// </para>
    /// <para>
    /// Unit fields also typing values in other units and automatically converting them to a value that the script can use.
    /// For example, typing in '5 in' into a Meter field will convert the inches to meters.
    /// </para>
    /// <para>
    /// The user can also change the displayed unit while editing the field. Right click > Units
    /// This behaviour can be disabled by setting <ForceDisplayUnit> to <c>true</c>.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>The following example shows different uses of the Unit attribute.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     // Declaring just the unit of the field enables all of the unit field functionality.
    ///     [Unit(Units.Meter)]
    ///     public float A;
    ///
    ///     // The value of the field is 0.005m, but in the inspector it will be displayed as 5mm.
    ///     [Unit(Units.Meter, Units.Millimeter)]
    ///     public float B = 0.005f;
    ///
    ///     // It is also possible to add custom units with <see cref="Sirenix.Utilities.Editor.UnitNumberUtility"/>.
    ///     // (Note, this example will not work unless 'My Custom Unit' has been created.)
    ///     [Unit("My Custom Unit")]
    ///     public float C;
    ///
    ///     // Display a value as a string instead of an editable number field.
    ///     // This can be especially useful with <see cref="ShowInInspectorAttribute"> on get properties,
    ///     // to display the same value in different units.
    ///     [Unit(Units.Kilogram, Units.MetricTon, DisplayAsString = true)]
    ///     public float D = 1200f;
    ///
    ///     // Disallow the user from changing the display unit.
    ///     [Unit(Units.Degree, Units.Radian, ForceDisplayUnit = true)]
    ///     public float E = 180f;
    /// }
    /// </code>
    /// </example>
    public class UnitAttribute : Attribute
    {
        /// <summary>
        /// The unit of underlying value.
        /// </summary>
        public Units Base = Units.Unset;
        /// <summary>
        /// The unit displayed in the number field.
        /// </summary>
        public Units Display = Units.Unset;
        /// <summary>
        /// Name of the underlying unit.
        /// </summary>
        public string BaseName;
        /// <summary>
        /// Name of the unit displayed in the number field.
        /// </summary>
        public string DisplayName;
        /// <summary>
        /// If <c>true</c> the number field is drawn as read-only text.
        /// </summary>
        public bool DisplayAsString;
        /// <summary>
        /// If <c>true</c> disables the option to change display unit with the right-click context menu.
        /// </summary>
        public bool ForceDisplayUnit;

        /// <summary>
        /// Displays the number as a unit field.
        /// </summary>
        /// <param name="unit">The unit of underlying value.</param>
        public UnitAttribute(Units unit)
        {
            this.Base = unit;
            this.Display = unit;
        }
        /// <summary>
        /// Displays the number as a unit field.
        /// </summary>
        /// <param name="unit">The name of the underlying value.</param>
        public UnitAttribute(string unit)
        {
            this.BaseName = unit;
            this.DisplayName = unit;
        }
        /// <summary>
        /// Displays the number as a unit field.
        /// </summary>
        /// <param name="base">The unit of underlying value.</param>
        /// <param name="display">The unit to display the value as in the inspector.</param>
        public UnitAttribute(Units @base, Units display)
        {
            this.Base = @base;
            this.Display = display;
        }
        /// <summary>
        /// Displays the number as a unit field.
        /// </summary>
        /// <param name="base">The unit of underlying value.</param>
        /// <param name="display">The unit to display the value as in the inspector.</param>
        public UnitAttribute(Units @base, string display)
        {
            this.Base = @base;
            this.DisplayName = display;
        }
        /// <summary>
        /// Displays the number as a unit field.
        /// </summary>
        /// <param name="base">The unit of underlying value.</param>
        /// <param name="display">The unit to display the value as in the inspector.</param>
        public UnitAttribute(string @base, Units display)
        {
            this.BaseName = @base;
            this.Display = display;
        }
        /// <summary>
        /// Displays the number as a unit field.
        /// </summary>
        /// <param name="base">The unit of underlying value.</param>
        /// <param name="display">The unit to display the value as in the inspector.</param>
        public UnitAttribute(string @base, string display)
        {
            this.BaseName = @base;
            this.DisplayName = display;
        }
    }

    /// <summary>
    /// Units for use with <see cref="UnitAttribute"/> and <see cref="Sirenix.Utilities.Editor.UnitNumberUtility"/>.
    /// </summary>
    public enum Units
    {
        Unset = -1,

        // Distance
        Nanometer,
        Micrometer,
        Millimeter,
        Centimeter,
        Meter,
        Kilometer,
        Inch,
        Feet,
        Mile,
        Yard,
        NauticalMile,
        LightYear,
        Parsec,
        AstronomicalUnit,

        // Volume
        CubicMeter,
        CubicKilometer,
        CubicCentimeter,
        CubicMillimeter,
        Liter,
        Milliliter,
        Centiliter,
        Deciliter,
        Hectoliter,
        CubicInch,
        CubicFeet,
        CubicYard,
        AcreFeet,
        BarrelOil,
        TeaspoonUS,
        TablespoonUS,
        CupUS,
        GillUS,
        PintUS,
        QuartUS,
        GallonUS,
        BarrelUS,
        FluidOunceUS,
        BarrelUK,
        FluidOunceUK,
        TeaspoonUK,
        TablespoonUK,
        CupUK,
        GillUK,
        PintUK,
        QuartUK,
        GallonUK,

        // Area
        SquareMeter,
        SquareKilometer,
        SquareCentimeter,
        SquareMillimeter,
        SquareMicrometer,
        SquareInch,
        SquareFeet,
        SquareYard,
        SquareMile,
        Hectare,
        Acre,
        Are,

        // Energy
        Joule,
        Kilojoule,
        WattHour,
        KilowattHour,
        HorsepowerHour,

        // Force
        Newton,
        Kilonewton,
        Meganewton,
        Giganewton,
        Teranewton,
        Centinewton,
        Millinewton,
        JouleMeter,
        JouleCentimeter,
        GramForce,
        KilogramForce,
        TonForce,
        PoundForce,
        KilopoundForce,
        OunceForce,

        // Speed
        MetersPerSecond,
        MetersPerMinute,
        MetersPerHour,
        KilometersPerSecond,
        KilometersPerMinute,
        KilometersPerHour,
        CentimetersPerSecond,
        CentimetersPerMinute,
        CentimetersPerHour,
        MillimetersPerSecond,
        MillimetersPerMinute,
        MillimetersPerHour,
        FeetPerSecond,
        FeetPerMinute,
        FeetPerHour,
        YardsPerSecond,
        YardsPerMinute,
        YardsPerHour,
        MilesPerSecond,
        MilesPerMinute,
        MilesPerHour,
        Knot,
        KnotUK,
        SpeedOfLight,

        // Digital storage
        Bit,
        Kilobit,
        Megabit,
        Gigabit,
        Terabit,
        Petabit,
        Byte,
        Kilobyte,
        Kibibyte,
        Megabyte,
        Mebibyte,
        Gigabyte,
        Gibibyte,
        Terabyte,
        Tebibyte,
        Petabyte,
        Pebibyte,

        // Weight
        Kilogram,
        Hectogram,
        Dekagram,
        Gram,
        Decigram,
        Centigram,
        Milligram,
        MetricTon,
        Pounds,
        ShortTon,
        LongTon,
        Ounce,
        StoneUS,
        StoneUK,
        QuarterUS,
        QuarterUK,
        Slug,
        Grain,

        // Temperature
        Celsius,
        Fahrenheit,
        Kelvin,

        // Pressure
        Pascal,
        Decipascal,
        Centipascal,
        Millipascal,
        Micropascal,
        Kilopascal,
        Megapascal,
        Gigapascal,
        Bar,
        Millibar,
        Microbar,
        PSI,
        KSI,
        StandardAtmosphere,

        // Power
        Watt,
        Kilowatt,
        Megawatt,
        Gigawatt,
        Terawatt,
        Horsepower,
        JouleSecond,
        JouleMinute,
        JouleHour,
        KilojouleSecond,
        KilojouleMinute,
        KilojouleHour,

        // Time
        Second,
        Millisecond,
        Microsecond,
        Nanosecond,
        Minute,
        Hour,
        Day,
        Week,

        // Angles
        Radian,
        Degree,
        Turn,
        Grad,
        SecondsOfAngle,
        MinutesOfAngle,
        Mil,

        // Accelaration
        MetersPerSecondSquared,
        DecimetersPerSecondSquared,
        CentimetersPerSecondSquared,
        MillimetersPerSecondSquared,
        MicrometersPerSecondSquared,
        DekametersPerSecondSquared,
        HectometersPerSecondSquared,
        KilometersPerSecondSquared,
        MilePerSecondSquared,
        YardPerSecondSquared,
        FeetPerSecondSquared,
        InchPerSecondSquared,
        GForce,

        // Torque
        NewtonMeter,
        NewtonCentimeter,
        NewtonMillimeter,
        KilonewtonMeter,
        KilogramForceMeter,
        KilogramForceCentimeter,
        KilogramForceMillimeter,
        GramForceMeter,
        GramForceCentimeter,
        GramForceMillimeter,
        PoundFeet,
        PoundInch,
        OuncecFeet,
        OuncecInch,

        // Angular Velocity
        RadiansPerSecond,
        RadiansPerMinute,
        RadiansPerHour,
        RadiansPerDay,
        DegreesPerSecond,
        DegreesPerMinute,
        DegreesPerHour,
        DegreesPerDay,
        RevolutionsPerSecond,
        RevolutionsPerMinute,
        RevolutionsPerHour,
        RevolutionsPerDay,

        // Frequency
        Hertz,
        Kilohertz,
        Megahertz,
        Gigahertz,

        // Percentage
        PercentMultiplier,
        Percent,
        Permille,
        Permyriad,
    }
}