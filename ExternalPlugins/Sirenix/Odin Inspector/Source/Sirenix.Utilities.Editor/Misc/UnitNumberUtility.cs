//-----------------------------------------------------------------------
// <copyright file="UnitNumberUtility.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using Sirenix.OdinInspector;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Categories of units. A unit value can only be converted to another of the same category.
    /// </summary>
    public enum UnitCategory
    {
        Distance,
        Volume,
        Area,
        Energy,
        Force,
        Speed,
        DataStorage,
        Weight,
        Temperature,
        Pressure,
        Power,
        Time,
        Angle,
        Torque,
        Acceleration,
        AngularVelocity,
        Frequency,
        Percent,
    }

    /// <summary>
    /// Tools for converting between units, for example, converting from inches to meters.
    /// </summary>
    /// <seealso cref="UnitInfo"/>
    /// <seealso cref="UnitAttribute"/>
    public static class UnitNumberUtility
    {
        // https://www.unitconverters.net

        private readonly static UnitInfo[] enumUnitInfoMap;
        private readonly static List<UnitInfo> customUnits;

        static UnitNumberUtility()
        {
            customUnits = new List<UnitInfo>();

            int enumUnitInfoMapLength = 0;
            var enumValues = Enum.GetValues(typeof(Units));
            foreach (var e in enumValues)
            {
                enumUnitInfoMapLength = Math.Max((int)e, enumUnitInfoMapLength);
            }

            enumUnitInfoMap = new UnitInfo[enumUnitInfoMapLength + 1];

            // Distance
            enumUnitInfoMap[(int)Units.Nanometer] =                     new UnitInfo("Nanometer",                       new[] { "nm" },                 UnitCategory.Distance,          1000000000.0m);
            enumUnitInfoMap[(int)Units.Micrometer] =                    new UnitInfo("Micrometer",                      new[] { "µm", "um" },           UnitCategory.Distance,          1000000.0m);
            enumUnitInfoMap[(int)Units.Millimeter] =                    new UnitInfo("Millimeter",                      new[] { "mm" },                 UnitCategory.Distance,          1000.0m);
            enumUnitInfoMap[(int)Units.Centimeter] =                    new UnitInfo("Centimeter",                      new[] { "cm" },                 UnitCategory.Distance,          100.0m);
            enumUnitInfoMap[(int)Units.Meter] =                         new UnitInfo("Meter",                           new[] { "m" },                  UnitCategory.Distance,          1.0m);
            enumUnitInfoMap[(int)Units.Kilometer] =                     new UnitInfo("Kilometer",                       new[] { "km" },                 UnitCategory.Distance,          0.001m);
            enumUnitInfoMap[(int)Units.Inch] =                          new UnitInfo("Inch",                            new[] { "\"", "in" },           UnitCategory.Distance,          39.37007874m);
            enumUnitInfoMap[(int)Units.Feet] =                          new UnitInfo("Feet",                            new[] { "\'", "ft" },           UnitCategory.Distance,          3.280839895m);
            enumUnitInfoMap[(int)Units.Mile] =                          new UnitInfo("Mile",                            new[] { "mi" },                 UnitCategory.Distance,          0.0006213712m);
            enumUnitInfoMap[(int)Units.Yard] =                          new UnitInfo("Yard",                            new[] { "yd" },                 UnitCategory.Distance,          1.0936132983m);
            enumUnitInfoMap[(int)Units.NauticalMile] =                  new UnitInfo("Nautical Mile",                   new[] { "nmi" },                UnitCategory.Distance,          0.0005399568m);
            enumUnitInfoMap[(int)Units.LightYear] =                     new UnitInfo("Light Year",                      new[] { "ly" },                 UnitCategory.Distance,          1.057000834E-16m);
            enumUnitInfoMap[(int)Units.Parsec] =                        new UnitInfo("Parsec",                          new[] { "pc" },                 UnitCategory.Distance,          3.240779289E-17m);
            enumUnitInfoMap[(int)Units.AstronomicalUnit] =              new UnitInfo("Astronomical Unit" ,              new[] { "AU" },                 UnitCategory.Distance,          6.684587122E-12m);

            // Volume
            enumUnitInfoMap[(int)Units.CubicMeter] =                    new UnitInfo("Cubic Meter",                     new[] { "m³" },                 UnitCategory.Volume,            1.0m);
            enumUnitInfoMap[(int)Units.CubicKilometer] =                new UnitInfo("Cubic Kilometer",                 new[] { "km³" },                UnitCategory.Volume,            1e-9m);
            enumUnitInfoMap[(int)Units.CubicCentimeter] =               new UnitInfo("Cubic Centimeter",                new[] { "cm³" },                UnitCategory.Volume,            1000000m);
            enumUnitInfoMap[(int)Units.CubicMillimeter] =               new UnitInfo("Cubic Millimeter",                new[] { "mm³" },                UnitCategory.Volume,            1000000000m);
            enumUnitInfoMap[(int)Units.Liter] =                         new UnitInfo("Liter",                           new[] { "L" },                  UnitCategory.Volume,            1m / 0.001m);
            enumUnitInfoMap[(int)Units.Milliliter] =                    new UnitInfo("Milliliter",                      new[] { "ml" },                 UnitCategory.Volume,            1m / 1e-6m);
            enumUnitInfoMap[(int)Units.Centiliter] =                    new UnitInfo("Centiliter",                      new[] { "cl" },                 UnitCategory.Volume,            100000.0m);
            enumUnitInfoMap[(int)Units.Deciliter] =                     new UnitInfo("Deciliter",                       new[] { "dl" },                 UnitCategory.Volume,            10000.0m);
            enumUnitInfoMap[(int)Units.Hectoliter] =                    new UnitInfo("Hectoliter",                      new[] { "hl" },                 UnitCategory.Volume,            10.0m);
            enumUnitInfoMap[(int)Units.CubicInch] =                     new UnitInfo("Cubic Inch",                      new[] { "in³" },                UnitCategory.Volume,            61023.744095m);
            enumUnitInfoMap[(int)Units.CubicFeet] =                     new UnitInfo("Cubic Feet",                      new[] { "ft³" },                UnitCategory.Volume,            35.314666721m);
            enumUnitInfoMap[(int)Units.CubicYard] =                     new UnitInfo("Cubic Yard",                      new[] { "yd³" },                UnitCategory.Volume,            1.3079506193m);
            enumUnitInfoMap[(int)Units.AcreFeet] =                      new UnitInfo("Acre Feet",                       new[] { "acre ft" },            UnitCategory.Volume,            0.0008107132m);
            enumUnitInfoMap[(int)Units.BarrelOil] =                     new UnitInfo("Barrel Oil",                      new[] { "bbl (oil)" },          UnitCategory.Volume,            6.2898107704m);
            enumUnitInfoMap[(int)Units.BarrelUS] =                      new UnitInfo("Barrel (US)",                     new[] { "bbl (US)" },           UnitCategory.Volume,            8.3864143606m);
            enumUnitInfoMap[(int)Units.FluidOunceUS] =                  new UnitInfo("Fluid Ounce (US)",                new[] { "fl oz (US)" },         UnitCategory.Volume,            33814.022702m);
            enumUnitInfoMap[(int)Units.TeaspoonUS] =                    new UnitInfo("Teaspoon (US)",                   new[] { "tsp (US)" },           UnitCategory.Volume,            202884.13621m);
            enumUnitInfoMap[(int)Units.TablespoonUS] =                  new UnitInfo("Tablespoon (US)",                 new[] { "tbsp (US)" },          UnitCategory.Volume,            67628.045404m);
            enumUnitInfoMap[(int)Units.CupUS] =                         new UnitInfo("Cup (US)",                        new[] { "cup (US)" },           UnitCategory.Volume,            4226.7528377m);
            enumUnitInfoMap[(int)Units.GillUS] =                        new UnitInfo("Gill (US)",                       new[] { "gill (US)" },          UnitCategory.Volume,            8453.5056755m);
            enumUnitInfoMap[(int)Units.PintUS] =                        new UnitInfo("Pint (US)",                       new[] { "pt (US)" },            UnitCategory.Volume,            2113.3764189m);
            enumUnitInfoMap[(int)Units.QuartUS] =                       new UnitInfo("Quart (US)",                      new[] { "qt (US)" },            UnitCategory.Volume,            1056.6882094m);
            enumUnitInfoMap[(int)Units.GallonUS] =                      new UnitInfo("Gallon (US)",                     new[] { "gal (US)" },           UnitCategory.Volume,            264.17205236m);
            enumUnitInfoMap[(int)Units.BarrelUK] =                      new UnitInfo("Barrel (UK)",                     new[] { "bbl (UK)" },           UnitCategory.Volume,            6.1102568972m);
            enumUnitInfoMap[(int)Units.FluidOunceUK] =                  new UnitInfo("Fluid Ounce (UK)",                new[] { "fl oz (UK)" },         UnitCategory.Volume,            35195.079728m);
            enumUnitInfoMap[(int)Units.TeaspoonUK] =                    new UnitInfo("Teaspoon (UK)",                   new[] { "tsp (UK)" },           UnitCategory.Volume,            168936.38269m);
            enumUnitInfoMap[(int)Units.TablespoonUK] =                  new UnitInfo("Tablespoon (UK)",                 new[] { "tbsp (UK)" },          UnitCategory.Volume,            56312.127565m);
            enumUnitInfoMap[(int)Units.CupUK] =                         new UnitInfo("Cup (UK)",                        new[] { "cup (UK)" },           UnitCategory.Volume,            3519.5079728m);
            enumUnitInfoMap[(int)Units.GillUK] =                        new UnitInfo("Gill (UK)",                       new[] { "gill (UK)" },          UnitCategory.Volume,            7039.0159456m);
            enumUnitInfoMap[(int)Units.PintUK] =                        new UnitInfo("Pint (UK)",                       new[] { "pt (UK)" },            UnitCategory.Volume,            1759.7539864m);
            enumUnitInfoMap[(int)Units.QuartUK] =                       new UnitInfo("Quart (UK)",                      new[] { "qt (UK)" },            UnitCategory.Volume,            879.8769932m);
            enumUnitInfoMap[(int)Units.GallonUK] =                      new UnitInfo("Gallon (UK)",                     new[] { "gal (UK)" },           UnitCategory.Volume,            219.9692483m);

            // Area
            enumUnitInfoMap[(int)Units.SquareMeter] =                   new UnitInfo("Square Meter",                    new[] { "m²" },                 UnitCategory.Area,              1.0m);
            enumUnitInfoMap[(int)Units.Hectare] =                       new UnitInfo("Hectare",                         new[] { "ha" },                 UnitCategory.Area,              0.0001m);
            enumUnitInfoMap[(int)Units.SquareKilometer] =               new UnitInfo("Square Kilometer",                new[] { "km²" },                UnitCategory.Area,              0.000001m);
            enumUnitInfoMap[(int)Units.SquareCentimeter] =              new UnitInfo("Square Centimeter",               new[] { "cm²" },                UnitCategory.Area,              10000m);
            enumUnitInfoMap[(int)Units.SquareMillimeter] =              new UnitInfo("Square Millimeter",               new[] { "mm²" },                UnitCategory.Area,              1000000m);
            enumUnitInfoMap[(int)Units.SquareMicrometer] =              new UnitInfo("Square Micrometer",               new[] { "µm²", "um²" },         UnitCategory.Area,              1000000000000m);
            enumUnitInfoMap[(int)Units.SquareInch] =                    new UnitInfo("Square Inch",                     new[] { "in²" },                UnitCategory.Area,              1550.0031m);
            enumUnitInfoMap[(int)Units.SquareFeet] =                    new UnitInfo("Square Feet",                     new[] { "ft²" },                UnitCategory.Area,              10.763910417m);
            enumUnitInfoMap[(int)Units.SquareYard] =                    new UnitInfo("Square Yard",                     new[] { "yd²" },                UnitCategory.Area,              1.1959900463m);
            enumUnitInfoMap[(int)Units.SquareMile] =                    new UnitInfo("Square Mile",                     new[] { "mi²" },                UnitCategory.Area,              3.861021585E-7m);
            enumUnitInfoMap[(int)Units.Acre] =                          new UnitInfo("Acre",                            new[] { "ac" },                 UnitCategory.Area,              0.0002471054m);
            enumUnitInfoMap[(int)Units.Are] =                           new UnitInfo("Are",                             new[] { "a" },                  UnitCategory.Area,              0.01m);

            // Energy
            enumUnitInfoMap[(int)Units.Joule] =                         new UnitInfo("Joule",                           new[] { "J" },                  UnitCategory.Energy,            1m);
            enumUnitInfoMap[(int)Units.Kilojoule] =                     new UnitInfo("Kilojoule",                       new[] { "kJ" },                 UnitCategory.Energy,            0.001m);
            enumUnitInfoMap[(int)Units.WattHour] =                      new UnitInfo("Watt-hour",                       new[] { "W*h" },                UnitCategory.Energy,            0.0002777778m);
            enumUnitInfoMap[(int)Units.KilowattHour] =                  new UnitInfo("Kilowatt-Hour",                   new[] { "kW*h" },               UnitCategory.Energy,            2.777777777E-7m);
            enumUnitInfoMap[(int)Units.HorsepowerHour] =                new UnitInfo("Horsepower-Hour",                 new[] { "hp*h" },               UnitCategory.Energy,            3.725061361E-7m);

            // Force
            enumUnitInfoMap[(int)Units.Newton] =                        new UnitInfo("Newton",                          new[] { "N" },                  UnitCategory.Force,             1.0m);
            enumUnitInfoMap[(int)Units.Kilonewton] =                    new UnitInfo("Kilonewton",                      new[] { "kN" },                 UnitCategory.Force,             0.001m);
            enumUnitInfoMap[(int)Units.Meganewton] =                    new UnitInfo("Meganewton",                      new[] { "MN" },                 UnitCategory.Force,             0.000001m);
            enumUnitInfoMap[(int)Units.Giganewton] =                    new UnitInfo("Giganewton",                      new[] { "GN" },                 UnitCategory.Force,             1e-9m);
            enumUnitInfoMap[(int)Units.Teranewton] =                    new UnitInfo("Teranewton",                      new[] { "TN" },                 UnitCategory.Force,             1e-12m);
            enumUnitInfoMap[(int)Units.Centinewton] =                   new UnitInfo("Centinewton",                     new[] { "cN" },                 UnitCategory.Force,             100m);
            enumUnitInfoMap[(int)Units.Millinewton] =                   new UnitInfo("Millinewton",                     new[] { "mN" },                 UnitCategory.Force,             1000m);
            enumUnitInfoMap[(int)Units.JouleMeter] =                    new UnitInfo("Joule/Meter",                     new[] { "J/m" },                UnitCategory.Force,             1m);
            enumUnitInfoMap[(int)Units.JouleCentimeter] =               new UnitInfo("Joule/Centimeter",                new[] { "J/cm" },               UnitCategory.Force,             100m);
            enumUnitInfoMap[(int)Units.GramForce] =                     new UnitInfo("Gram-Force",                      new[] { "gf" },                 UnitCategory.Force,             101.9716213m);
            enumUnitInfoMap[(int)Units.KilogramForce] =                 new UnitInfo("Kilogram-Force",                  new[] { "kgf" },                UnitCategory.Force,             0.1019716213m);
            enumUnitInfoMap[(int)Units.TonForce] =                      new UnitInfo("Ton-Force",                       new[] { "tf" },                 UnitCategory.Force,             0.0001019716m);
            enumUnitInfoMap[(int)Units.PoundForce] =                    new UnitInfo("Pound-Force",                     new[] { "lbf" },                UnitCategory.Force,             0.2248089431m);
            enumUnitInfoMap[(int)Units.KilopoundForce] =                new UnitInfo("Kilopound-Force",                 new[] { "klbf" },               UnitCategory.Force,             0.0002248089m);
            enumUnitInfoMap[(int)Units.OunceForce] =                    new UnitInfo("Ounce-Force",                     new[] { "ozf" },                UnitCategory.Force,             3.5969430896m);

            // Speed
            enumUnitInfoMap[(int)Units.MetersPerSecond] =               new UnitInfo("Meters per Second",               new[] { "m/s" },                UnitCategory.Speed,             1.0m);
            enumUnitInfoMap[(int)Units.MetersPerMinute] =               new UnitInfo("Meters per Minute",               new[] { "m/min" },              UnitCategory.Speed,             60m);
            enumUnitInfoMap[(int)Units.MetersPerHour] =                 new UnitInfo("Meters per Hour",                 new[] { "m/h" },                UnitCategory.Speed,             3600m);
            enumUnitInfoMap[(int)Units.KilometersPerSecond] =           new UnitInfo("Kilometers per Second",           new[] { "km/s" },               UnitCategory.Speed,             0.001m);
            enumUnitInfoMap[(int)Units.KilometersPerMinute] =           new UnitInfo("Kilometers per Minute",           new[] { "km/min" },             UnitCategory.Speed,             0.06m);
            enumUnitInfoMap[(int)Units.KilometersPerHour] =             new UnitInfo("Kilometers per Hour",             new[] { "km/h" },               UnitCategory.Speed,             3.6m);
            enumUnitInfoMap[(int)Units.CentimetersPerSecond] =          new UnitInfo("Centimeters per Second",          new[] { "cm/s" },               UnitCategory.Speed,             100m);
            enumUnitInfoMap[(int)Units.CentimetersPerMinute] =          new UnitInfo("Centimeters per Minute",          new[] { "cm/min" },             UnitCategory.Speed,             6000m);
            enumUnitInfoMap[(int)Units.CentimetersPerHour] =            new UnitInfo("Centimeters per Hour",            new[] { "cm/h" },               UnitCategory.Speed,             360000m);
            enumUnitInfoMap[(int)Units.MillimetersPerSecond] =          new UnitInfo("Millimeters per Second",          new[] { "mm/s" },               UnitCategory.Speed,             1000m);
            enumUnitInfoMap[(int)Units.MillimetersPerMinute] =          new UnitInfo("Millimeters per Minute",          new[] { "mm/min" },             UnitCategory.Speed,             60000m);
            enumUnitInfoMap[(int)Units.MillimetersPerHour] =            new UnitInfo("Millimeters per Hour",            new[] { "mm/h" },               UnitCategory.Speed,             3600000m);
            enumUnitInfoMap[(int)Units.FeetPerSecond] =                 new UnitInfo("Feet per Second",                 new[] { "ft/s", "\"/s" },       UnitCategory.Speed,             3.280839895m);
            enumUnitInfoMap[(int)Units.FeetPerMinute] =                 new UnitInfo("Feet per Minute",                 new[] { "ft/min", "\"/min" },   UnitCategory.Speed,             196.8503937m);
            enumUnitInfoMap[(int)Units.FeetPerHour] =                   new UnitInfo("Feet per Hour",                   new[] { "ft/h", "\"/h" },       UnitCategory.Speed,             11811.023622m);
            enumUnitInfoMap[(int)Units.YardsPerSecond] =                new UnitInfo("Yards per Second",                new[] { "yd/s" },               UnitCategory.Speed,             1.0936132983m);
            enumUnitInfoMap[(int)Units.YardsPerMinute] =                new UnitInfo("Yards per Minute",                new[] { "yd/min" },             UnitCategory.Speed,             65.616797m);
            enumUnitInfoMap[(int)Units.YardsPerHour] =                  new UnitInfo("Yards per Hour",                  new[] { "yd/h" },               UnitCategory.Speed,             3937.007874m);
            enumUnitInfoMap[(int)Units.MilesPerSecond] =                new UnitInfo("Miles per Second",                new[] { "mi/s" },               UnitCategory.Speed,             0.0006213712m);
            enumUnitInfoMap[(int)Units.MilesPerMinute] =                new UnitInfo("Miles per Minute",                new[] { "mi/min" },             UnitCategory.Speed,             0.0372822715m);
            enumUnitInfoMap[(int)Units.MilesPerHour] =                  new UnitInfo("Miles per Hour",                  new[] { "mi/h" },               UnitCategory.Speed,             2.2369362921m);
            enumUnitInfoMap[(int)Units.Knot] =                          new UnitInfo("Knots",                           new[] { "kn" },                 UnitCategory.Speed,             1.9438444924m);
            enumUnitInfoMap[(int)Units.KnotUK] =                        new UnitInfo("Knots (UK)",                      new[] { "kt (UK)" },            UnitCategory.Speed,             1.9426025694m);
            enumUnitInfoMap[(int)Units.SpeedOfLight] =                  new UnitInfo("Speed of light (Vacuum)",         new[] { "c" },                  UnitCategory.Speed,             3.335640951E-9m);

            // Digital storage
            enumUnitInfoMap[(int)Units.Bit] =                           new UnitInfo("Bit",                             new[] { "bit" },                UnitCategory.DataStorage,       8e+6m);
            enumUnitInfoMap[(int)Units.Kilobit] =                       new UnitInfo("Kilobit",                         new[] { "kbit" },               UnitCategory.DataStorage,       8000m);
            enumUnitInfoMap[(int)Units.Megabit] =                       new UnitInfo("Megabit",                         new[] { "Mbit" },               UnitCategory.DataStorage,       8m);
            enumUnitInfoMap[(int)Units.Gigabit] =                       new UnitInfo("Gigabit",                         new[] { "Gbit" },               UnitCategory.DataStorage,       0.008m);
            enumUnitInfoMap[(int)Units.Terabit] =                       new UnitInfo("Terabit",                         new[] { "Tbit" },               UnitCategory.DataStorage,       0.000008m);
            enumUnitInfoMap[(int)Units.Petabit] =                       new UnitInfo("Petabit",                         new[] { "Pbit" },               UnitCategory.DataStorage,       0.000000008m);
            enumUnitInfoMap[(int)Units.Byte] =                          new UnitInfo("Byte",                            new[] { "B" },                  UnitCategory.DataStorage,       1e+6m);
            enumUnitInfoMap[(int)Units.Kilobyte] =                      new UnitInfo("Kilobyte",                        new[] { "kB" },                 UnitCategory.DataStorage,       1000m);
            enumUnitInfoMap[(int)Units.Megabyte] =                      new UnitInfo("Megabyte",                        new[] { "MB" },                 UnitCategory.DataStorage,       1m);
            enumUnitInfoMap[(int)Units.Gigabyte] =                      new UnitInfo("Gigabyte",                        new[] { "GB" },                 UnitCategory.DataStorage,       0.001m);
            enumUnitInfoMap[(int)Units.Terabyte] =                      new UnitInfo("Terabyte",                        new[] { "TB" },                 UnitCategory.DataStorage,       0.000001m);
            enumUnitInfoMap[(int)Units.Petabyte] =                      new UnitInfo("Petabyte",                        new[] { "PB" },                 UnitCategory.DataStorage,       0.000000001m);
            
            enumUnitInfoMap[(int)Units.Kibibyte] =                      new UnitInfo("Kibibyte",                        new[] { "kiB" },                UnitCategory.DataStorage,       976.5625m);
            enumUnitInfoMap[(int)Units.Mebibyte] =                      new UnitInfo("Mebibyte",                        new[] { "MiB" },                UnitCategory.DataStorage,       0.9536743164m);
            enumUnitInfoMap[(int)Units.Gibibyte] =                      new UnitInfo("Gibibyte",                        new[] { "GiB" },                UnitCategory.DataStorage,       0.0009313226m);
            enumUnitInfoMap[(int)Units.Tebibyte] =                      new UnitInfo("Tebibyte",                        new[] { "TiB" },                UnitCategory.DataStorage,       9.094947017E-7m);
            enumUnitInfoMap[(int)Units.Pebibyte] =                      new UnitInfo("Pebibyte",                        new[] { "PiB" },                UnitCategory.DataStorage,       8.881784197E-10m);

            // Weight
            enumUnitInfoMap[(int)Units.Kilogram] =                      new UnitInfo("Kilogram",                        new[] { "kg" },                 UnitCategory.Weight,            1.0m);
            enumUnitInfoMap[(int)Units.Hectogram] =                     new UnitInfo("Hectogram",                       new[] { "hg" },                 UnitCategory.Weight,            10m);
            enumUnitInfoMap[(int)Units.Dekagram] =                      new UnitInfo("Dekagram",                        new[] { "dag" },                UnitCategory.Weight,            100m);
            enumUnitInfoMap[(int)Units.Gram] =                          new UnitInfo("Gram",                            new[] { "g" },                  UnitCategory.Weight,            1000m);
            enumUnitInfoMap[(int)Units.Decigram] =                      new UnitInfo("Decigram",                        new[] { "dg" },                 UnitCategory.Weight,            10000m);
            enumUnitInfoMap[(int)Units.Centigram] =                     new UnitInfo("Centigram",                       new[] { "cg" },                 UnitCategory.Weight,            100000m);
            enumUnitInfoMap[(int)Units.Milligram] =                     new UnitInfo("Milligram",                       new[] { "mg" },                 UnitCategory.Weight,            1000000m);
            enumUnitInfoMap[(int)Units.MetricTon] =                     new UnitInfo("Metric Ton",                      new[] { "t", "Mg" },            UnitCategory.Weight,            0.001m);
            enumUnitInfoMap[(int)Units.Pounds] =                        new UnitInfo("Pounds",                          new[] { "lbs" },                UnitCategory.Weight,            2.20462m);
            enumUnitInfoMap[(int)Units.ShortTon] =                      new UnitInfo("Short Ton",                       new[] { "sh.tn.", "sh.t." },    UnitCategory.Weight,            0.00110231m);
            enumUnitInfoMap[(int)Units.LongTon] =                       new UnitInfo("Long Ton",                        new[] { "l.tn.", "l.t." },      UnitCategory.Weight,            0.000984207m);
            enumUnitInfoMap[(int)Units.Ounce] =                         new UnitInfo("Ounce",                           new[] { "oz" },                 UnitCategory.Weight,            35.27396195m);
            enumUnitInfoMap[(int)Units.StoneUS] =                       new UnitInfo("Stone (US)",                      new[] { "stone (US)" },         UnitCategory.Weight,            0.1763698097m);
            enumUnitInfoMap[(int)Units.StoneUK] =                       new UnitInfo("Stone (UK)",                      new[] { "stone (UK)" },         UnitCategory.Weight,            0.1574730444m);
            enumUnitInfoMap[(int)Units.QuarterUS] =                     new UnitInfo("Quarter (US)",                    new[] { "qr (US)" },            UnitCategory.Weight,            0.0881849049m);
            enumUnitInfoMap[(int)Units.QuarterUK] =                     new UnitInfo("Quarter (UK)",                    new[] { "qr (UK)" },            UnitCategory.Weight,            0.0787365222m);
            enumUnitInfoMap[(int)Units.Slug] =                          new UnitInfo("Slug",                            new[] { "slug" },               UnitCategory.Weight,            0.0685217659m);
            enumUnitInfoMap[(int)Units.Grain] =                         new UnitInfo("Grain",                           new[] { "gr" },                 UnitCategory.Weight,            15432.358353m);

            // Temperature
            enumUnitInfoMap[(int)Units.Kelvin] =                        new UnitInfo("Kelvin",                          new[] { "°K", "K" },            UnitCategory.Temperature,       1m);
            enumUnitInfoMap[(int)Units.Fahrenheit] =                    new UnitInfo("Fahrenheit",                      new[] { "°F", "F" },            UnitCategory.Temperature,       f => (f - 32m) * 5m/9m + 273.15m, k => (k - 273.15m) * 9m/5m + 32m);
            enumUnitInfoMap[(int)Units.Celsius] =                       new UnitInfo("Celsius",                         new[] { "°C", "C" },            UnitCategory.Temperature,       c => c + 273.15m, k => k - 273.15m);

            // Pressure
            enumUnitInfoMap[(int)Units.Pascal] =                        new UnitInfo("Pascal",                          new[] { "Pa" },                 UnitCategory.Pressure,          1m);
            enumUnitInfoMap[(int)Units.Decipascal] =                    new UnitInfo("Decipascal",                      new[] { "dPa" },                UnitCategory.Pressure,          10m);
            enumUnitInfoMap[(int)Units.Centipascal] =                   new UnitInfo("Centipascal",                     new[] { "cPa" },                UnitCategory.Pressure,          100m);
            enumUnitInfoMap[(int)Units.Millipascal] =                   new UnitInfo("Millipascal",                     new[] { "mPa" },                UnitCategory.Pressure,          1000m);
            enumUnitInfoMap[(int)Units.Micropascal] =                   new UnitInfo("Micropascal",                     new[] { "µPa", "uPa" },         UnitCategory.Pressure,          1000000m);
            enumUnitInfoMap[(int)Units.Kilopascal] =                    new UnitInfo("Kilopascal",                      new[] { "kPa" },                UnitCategory.Pressure,          0.001m);
            enumUnitInfoMap[(int)Units.Megapascal] =                    new UnitInfo("Megapascal",                      new[] { "MPa" },                UnitCategory.Pressure,          0.000001m);
            enumUnitInfoMap[(int)Units.Gigapascal] =                    new UnitInfo("Gigapascal",                      new[] { "GPa" },                UnitCategory.Pressure,          1E-9m);
            enumUnitInfoMap[(int)Units.Bar] =                           new UnitInfo("Bar",                             new[] { "bar" },                UnitCategory.Pressure,          0.00001m);
            enumUnitInfoMap[(int)Units.Millibar] =                      new UnitInfo("Millibar",                        new[] { "mbar" },               UnitCategory.Pressure,          0.01m);
            enumUnitInfoMap[(int)Units.Microbar] =                      new UnitInfo("Microbar",                        new[] { "µbar", "ubar" },       UnitCategory.Pressure,          10m);
            enumUnitInfoMap[(int)Units.PSI] =                           new UnitInfo("PSI",                             new[] { "psi" },                UnitCategory.Pressure,          0.0001450377m);
            enumUnitInfoMap[(int)Units.KSI] =                           new UnitInfo("KSI",                             new[] { "ksi" },                UnitCategory.Pressure,          1.450377377E-7m);
            enumUnitInfoMap[(int)Units.StandardAtmosphere] =            new UnitInfo("Standard Atmosphere",             new[] { "atm" },                UnitCategory.Pressure,          0.0000098692m);

            // Power
            enumUnitInfoMap[(int)Units.Watt] =                          new UnitInfo("Watt",                            new[] { "W" },                  UnitCategory.Power,             1m);
            enumUnitInfoMap[(int)Units.Kilowatt] =                      new UnitInfo("Kilowatt",                        new[] { "kW" },                 UnitCategory.Power,             0.001m);
            enumUnitInfoMap[(int)Units.Megawatt] =                      new UnitInfo("Megawatt",                        new[] { "MW" },                 UnitCategory.Power,             0.000001m);
            enumUnitInfoMap[(int)Units.Gigawatt] =                      new UnitInfo("Gigawatt",                        new[] { "GW" },                 UnitCategory.Power,             1E-9m);
            enumUnitInfoMap[(int)Units.Terawatt] =                      new UnitInfo("Terawatt",                        new[] { "TW" },                 UnitCategory.Power,             1E-12m);
            enumUnitInfoMap[(int)Units.Horsepower] =                    new UnitInfo("Horsepower",                      new[] { "hp", "ft*lbf/s" },     UnitCategory.Power,             0.0013410221m);
            enumUnitInfoMap[(int)Units.JouleSecond] =                   new UnitInfo("Joule/Second",                    new[] { "J/s" },                UnitCategory.Power,             1m);
            enumUnitInfoMap[(int)Units.JouleMinute] =                   new UnitInfo("Joule/Minute",                    new[] { "J/min" },              UnitCategory.Power,             60m);
            enumUnitInfoMap[(int)Units.JouleHour] =                     new UnitInfo("Joule/Hour",                      new[] { "J/h" },                UnitCategory.Power,             3600m);
            enumUnitInfoMap[(int)Units.KilojouleSecond] =               new UnitInfo("Kilojoule/Second",                new[] { "kJ/s" },               UnitCategory.Power,             0.001m);
            enumUnitInfoMap[(int)Units.KilojouleMinute] =               new UnitInfo("Kilojoule/Minute",                new[] { "kJ/min" },             UnitCategory.Power,             0.06m);
            enumUnitInfoMap[(int)Units.KilojouleHour] =                 new UnitInfo("Kilojoule/Hour",                  new[] { "kJ/h" },               UnitCategory.Power,             3.6m);

            // Time
            enumUnitInfoMap[(int)Units.Second] =                        new UnitInfo("Second",                          new[] { "s" },                  UnitCategory.Time,              1.0m);
            enumUnitInfoMap[(int)Units.Millisecond] =                   new UnitInfo("Millisecond",                     new[] { "ms" },                 UnitCategory.Time,              1000m);
            enumUnitInfoMap[(int)Units.Microsecond] =                   new UnitInfo("Microsecond",                     new[] { "µs", "us" },           UnitCategory.Time,              1000000m);
            enumUnitInfoMap[(int)Units.Nanosecond] =                    new UnitInfo("Nanosecond",                      new[] { "ns" },                 UnitCategory.Time,              1000000000m);
            enumUnitInfoMap[(int)Units.Minute] =                        new UnitInfo("Minute",                          new[] { "min" },                UnitCategory.Time,              1m / 60.0m);
            enumUnitInfoMap[(int)Units.Hour] =                          new UnitInfo("Hour",                            new[] { "h" },                  UnitCategory.Time,              1m / 3600.0m);
            enumUnitInfoMap[(int)Units.Day] =                           new UnitInfo("Day",                             new[] { "d" },                  UnitCategory.Time,              1m / 86400.0m);
            enumUnitInfoMap[(int)Units.Week] =                          new UnitInfo("Week",                            new[] { "week" },               UnitCategory.Time,              0.0000016534m);

            // Angles
            enumUnitInfoMap[(int)Units.Degree] =                        new UnitInfo("Degree",                          new[] { "°", "d" },             UnitCategory.Angle,             1.0m);
            enumUnitInfoMap[(int)Units.Radian] =                        new UnitInfo("Radian",                          new[] { "rad" },                UnitCategory.Angle,             (decimal)Math.PI / 180m);
            enumUnitInfoMap[(int)Units.Grad] =                          new UnitInfo("Grad",                            new[] { "^g" },                 UnitCategory.Angle,             1.1111111111m);
            enumUnitInfoMap[(int)Units.SecondsOfAngle] =                new UnitInfo("Seconds of Angle",                new[] { "\"" },                 UnitCategory.Angle,             3600m);
            enumUnitInfoMap[(int)Units.MinutesOfAngle] =                new UnitInfo("Minutes of Angle",                new[] { "'", "MOA" },           UnitCategory.Angle,             60m);
            enumUnitInfoMap[(int)Units.Mil] =                           new UnitInfo("Mil",                             new[] { "mil" },                UnitCategory.Angle,             17.777777778m);
            enumUnitInfoMap[(int)Units.Turn] =                          new UnitInfo("Turn",                            new[] { "turns" },              UnitCategory.Angle,             1m / 360.0m);

            // Torque
            enumUnitInfoMap[(int)Units.NewtonMeter] =                   new UnitInfo("Newton Meter",                    new[] { "N⋅m" },                UnitCategory.Torque,            1m);
            enumUnitInfoMap[(int)Units.NewtonCentimeter] =              new UnitInfo("Newton Centimeter",               new[] { "N⋅cm" },               UnitCategory.Torque,            100m);
            enumUnitInfoMap[(int)Units.NewtonMillimeter] =              new UnitInfo("Newton Millimeter",               new[] { "N⋅mm" },               UnitCategory.Torque,            1000m);
            enumUnitInfoMap[(int)Units.KilonewtonMeter] =               new UnitInfo("Kilonewton Meter",                new[] { "kN⋅m" },               UnitCategory.Torque,            0.001m);
            enumUnitInfoMap[(int)Units.KilogramForceMeter] =            new UnitInfo("Kilogram-force Meter",            new[] { "kgf⋅m" },              UnitCategory.Torque,            0.1019716213m);
            enumUnitInfoMap[(int)Units.KilogramForceCentimeter] =       new UnitInfo("Kilogram-force Centimeter",       new[] { "kgf⋅cm" },             UnitCategory.Torque,            10.19716213m);
            enumUnitInfoMap[(int)Units.KilogramForceMillimeter] =       new UnitInfo("Kilogram-force Millimeter",       new[] { "kgf⋅mm" },             UnitCategory.Torque,            101.9716213m);
            enumUnitInfoMap[(int)Units.GramForceMeter] =                new UnitInfo("Gram-force Meter",                new[] { "gf⋅m" },               UnitCategory.Torque,            101.9716213m);
            enumUnitInfoMap[(int)Units.GramForceCentimeter] =           new UnitInfo("Gram-force Centimeter",           new[] { "gf⋅cm" },              UnitCategory.Torque,            10197.16213m);
            enumUnitInfoMap[(int)Units.GramForceMillimeter] =           new UnitInfo("Gram-force Millimeter",           new[] { "gf⋅mm" },              UnitCategory.Torque,            101971.6213m);
            enumUnitInfoMap[(int)Units.PoundFeet] =                     new UnitInfo("Pound-force Feet",                new[] { "lb⋅ft", "lbf-ft" },    UnitCategory.Torque,            0.7375621212m);
            enumUnitInfoMap[(int)Units.PoundInch] =                     new UnitInfo("Pound-force Inch",                new[] { "lb⋅in", "lbf-in" },    UnitCategory.Torque,            8.850745454m);
            enumUnitInfoMap[(int)Units.OuncecFeet] =                    new UnitInfo("Ounce-force Feet",                new[] { "oz⋅ft" },              UnitCategory.Torque,            11.800994078m);
            enumUnitInfoMap[(int)Units.OuncecInch] =                    new UnitInfo("Ounce-force Inch",                new[] { "oz⋅in" },              UnitCategory.Torque,            141.61192894m);

            // Acceleration
            enumUnitInfoMap[(int)Units.MetersPerSecondSquared] =        new UnitInfo("Meters per second squared",       new[] { "m/s²", "m/s/s" },      UnitCategory.Acceleration,      1.0m);
            enumUnitInfoMap[(int)Units.DecimetersPerSecondSquared] =    new UnitInfo("Decimeters per second squared",   new[] { "dm/s²", "dm/s/s" },    UnitCategory.Acceleration,      10m);
            enumUnitInfoMap[(int)Units.CentimetersPerSecondSquared] =   new UnitInfo("Centimeters per second squared",  new[] { "cm/s²", "cm/s/s" },    UnitCategory.Acceleration,      100m);
            enumUnitInfoMap[(int)Units.MillimetersPerSecondSquared] =   new UnitInfo("Millimeters per second squared",  new[] { "mm/s²", "mm/s/s" },    UnitCategory.Acceleration,      1000m);
            enumUnitInfoMap[(int)Units.MicrometersPerSecondSquared] =   new UnitInfo("Micrometers per second squared",  new[] { "µm/s²", "µm/s/s" },    UnitCategory.Acceleration,      1000000m);
            enumUnitInfoMap[(int)Units.DekametersPerSecondSquared] =    new UnitInfo("Dekameters per second squared",   new[] { "Dm/s²", "Dm/s/s" },    UnitCategory.Acceleration,      0.1m);
            enumUnitInfoMap[(int)Units.HectometersPerSecondSquared] =   new UnitInfo("Hectometers per second squared",  new[] { "hm/s²", "hm/s/s" },    UnitCategory.Acceleration,      0.01m);
            enumUnitInfoMap[(int)Units.KilometersPerSecondSquared] =    new UnitInfo("Kilometers per second squared",   new[] { "km/s²", "km/s/s" },    UnitCategory.Acceleration,      0.001m);
            enumUnitInfoMap[(int)Units.MilePerSecondSquared] =          new UnitInfo("Mile per second squared",         new[] { "mi/s²", "mi/s/s" },    UnitCategory.Acceleration,      0.0006213712m);
            enumUnitInfoMap[(int)Units.YardPerSecondSquared] =          new UnitInfo("Yard per second squared",         new[] { "yd/s²", "yd/s/s" },    UnitCategory.Acceleration,      1.0936132983m);
            enumUnitInfoMap[(int)Units.FeetPerSecondSquared] =          new UnitInfo("Feet per second squared",         new[] { "ft/s²", "ft/s/s" },    UnitCategory.Acceleration,      3.280839895m);
            enumUnitInfoMap[(int)Units.InchPerSecondSquared] =          new UnitInfo("Inch per second squared",         new[] { "in/s²", "in/s/s" },    UnitCategory.Acceleration,      39.37007874m);
            enumUnitInfoMap[(int)Units.GForce] =                        new UnitInfo("G-Force",                         new[] { "g" },                  UnitCategory.Acceleration,      0.1019716213m);

            // Angular Velocity
            enumUnitInfoMap[(int)Units.RadiansPerSecond] =              new UnitInfo("Radians per Second",              new[] { "rad/s", "r/s" },       UnitCategory.AngularVelocity,   1m);
            enumUnitInfoMap[(int)Units.RadiansPerMinute] =              new UnitInfo("Radians per Minute",              new[] { "rad/min", "r/min" },   UnitCategory.AngularVelocity,   60m);
            enumUnitInfoMap[(int)Units.RadiansPerHour] =                new UnitInfo("Radians per Hour",                new[] { "rad/h", "r/h" },       UnitCategory.AngularVelocity,   3600m);
            enumUnitInfoMap[(int)Units.RadiansPerDay] =                 new UnitInfo("Radians per Day",                 new[] { "rad/d", "r/d" },       UnitCategory.AngularVelocity,   86400m);
            enumUnitInfoMap[(int)Units.DegreesPerSecond] =              new UnitInfo("Degrees per Second",              new[] { "°/s", "d/s" },         UnitCategory.AngularVelocity,   57.295779513m);
            enumUnitInfoMap[(int)Units.DegreesPerMinute] =              new UnitInfo("Degrees per Minute",              new[] { "°/min", "d/min" },     UnitCategory.AngularVelocity,   3437.7467708m);
            enumUnitInfoMap[(int)Units.DegreesPerHour] =                new UnitInfo("Degrees per Hour",                new[] { "°/h", "d/h" },         UnitCategory.AngularVelocity,   206264.80625m);
            enumUnitInfoMap[(int)Units.DegreesPerDay] =                 new UnitInfo("Degrees per Day",                 new[] { "°/d", "d/d" },         UnitCategory.AngularVelocity,   4950355.3499m);
            enumUnitInfoMap[(int)Units.RevolutionsPerSecond] =          new UnitInfo("Revolutions per Second",          new[] { "rps" },                UnitCategory.AngularVelocity,   0.1591549431m);
            enumUnitInfoMap[(int)Units.RevolutionsPerMinute] =          new UnitInfo("Revolutions per Minute",          new[] { "rpm" },                UnitCategory.AngularVelocity,   9.5492965855m);
            enumUnitInfoMap[(int)Units.RevolutionsPerHour] =            new UnitInfo("Revolutions per Hour",            new[] { "rph" },                UnitCategory.AngularVelocity,   572.95779513m);
            enumUnitInfoMap[(int)Units.RevolutionsPerDay] =             new UnitInfo("Revolutions per Day",             new[] { "rpd" },                UnitCategory.AngularVelocity,   13750.987083m);

            // Frequency
            enumUnitInfoMap[(int)Units.Hertz] =                         new UnitInfo("Hertz",                           new[] { "Hz" },                 UnitCategory.Frequency,         1.0m);
            enumUnitInfoMap[(int)Units.Kilohertz] =                     new UnitInfo("Kilohertz",                       new[] { "kHz" },                UnitCategory.Frequency,         1m / 1000.0m);
            enumUnitInfoMap[(int)Units.Megahertz] =                     new UnitInfo("Megahertz",                       new[] { "MHz" },                UnitCategory.Frequency,         1m / 1000000.0m);
            enumUnitInfoMap[(int)Units.Gigahertz] =                     new UnitInfo("Gigahertz",                       new[] { "GHz" },                UnitCategory.Frequency,         1m / 1000000000.0m);

            // Percent
            enumUnitInfoMap[(int)Units.PercentMultiplier] =             new UnitInfo("Percent Multiplier",              new[] { "%m" },                 UnitCategory.Percent,           1.0m);
            enumUnitInfoMap[(int)Units.Percent] =                       new UnitInfo("Percent",                         new[] { "%" },                  UnitCategory.Percent,           100.0m);
            enumUnitInfoMap[(int)Units.Permille] =                      new UnitInfo("Permille",                        new[] { "‰" },                  UnitCategory.Percent,           1000.0m);
            enumUnitInfoMap[(int)Units.Permyriad] =                     new UnitInfo("Permyriad",                       new[] { "‱" },                 UnitCategory.Percent,           10000.0m);
        }

        /// <summary>
        /// Gets all UnitInfo registered, both built-in and custom.
        /// </summary>
        /// <returns>Enumerable of both built-in and custom units.</returns>
        public static IEnumerable<UnitInfo> GetAllUnitInfos()
        {
            for (int i = 0; i < enumUnitInfoMap.Length; i++)
            {
                if (enumUnitInfoMap[i] != null)
                {
                    yield return enumUnitInfoMap[i];
                }
            }

            for (int i = 0; i < customUnits.Count; i++)
            {
                yield return customUnits[i];
            }
        }

        /// <summary>
        /// Gets the UnitInfo for the given Units enum value.
        /// </summary>
        /// <param name="unit">Units enum value.</param>
        /// <returns>UnitInfo for the unit.</returns>
        /// <exception cref="Exception">Throws for invalid unit input.</exception>
        public static UnitInfo GetUnitInfo(Units unit)
        {
            if (TryGetUnitInfo(unit, out UnitInfo unitInfo))
            {
                return unitInfo;
            }

            throw new Exception($"Failed to find unit info for '{unit}'.");
        }
        /// <summary>
        /// Gets the UnitInfo with the corrosponding name.
        /// </summary>
        /// <param name="unitName">The name of the unit.</param>
        /// <returns>UnitInfo for the name.</returns>
        /// <exception cref="Exception">Throws when no unit with the given name is found.</exception>
        public static UnitInfo GetUnitInfoByName(string unitName)
        {
            if (TryGetUnitInfoByName(unitName, out UnitInfo unitInfo))
            {
                return unitInfo;
            }

            throw new Exception($"Failed to find unit info by name for '{unitName}'.");
        }
        /// <summary>
        /// Finds the UnitInfo that best fits the given symbol within the given category.
        /// </summary>
        /// <param name="symbol">The symbol to find a unit for.</param>
        /// <param name="unitCategory">The category to look for units within.</param>
        /// <returns>The UnitInfo that best matches the given symbol.</returns>
        /// <exception cref="Exception">Throws when no match was found.</exception>
        public static UnitInfo MatchUnitInfoBySymbol(string symbol, string unitCategory)
        {
            if (TryMatchUnitInfoBySymbol(symbol, unitCategory, out var unitInfo))
            {
                return unitInfo;
            }

            throw new Exception($"Could not match any UnitInfo to symbol '{symbol}'.");
        }

        /// <summary>
        /// Gets the UnitInfo for the given Units enum value.
        /// </summary>
        /// <param name="unit">Units enum value.</param>
        /// <param name="unitInfo">The UnitInfo matching the given unit value.</param>
        /// <returns><c>true</c> when a UnitInfo was found. Otherwise <c>false</c>.</returns>
        public static bool TryGetUnitInfo(Units unit, out UnitInfo unitInfo)
        {
            int i = (int)unit;

            if (i >= 0 && i < enumUnitInfoMap.Length)
            {
                unitInfo = enumUnitInfoMap[i];
                return unitInfo != null;
            }

            unitInfo = null;
            return false;
        }
        /// <summary>
        /// Gets the UnitInfo with the given name.
        /// </summary>
        /// <param name="unitName">The name of the unit.</param>
        /// <param name="unitInfo">The UnitInfo matching the given name.</param>
        /// <returns><c>true</c> when a UnitInfo was found. Otherwise <c>false</c>.</returns>
        public static bool TryGetUnitInfoByName(string unitName, out UnitInfo unitInfo)
        {
            unitInfo = GetAllUnitInfos().FirstOrDefault(ui => ui.Name == unitName)
                       ?? enumUnitInfoMap.FirstOrDefault(ui => ui.Name == unitName);

            return unitInfo != null;
        }
        /// <summary>
        /// Finds the UnitInfo that best fits the given symbol within the given category.
        /// </summary>
        /// <param name="symbol">The symbol to find a unit for.</param>
        /// <param name="unitCategory">The category to look for units within.</param>
        /// <param name="unitInfo">The UnitInfo that best matches the given symbol.</param>
        /// <returns><c>true</c> when a UnitInfo was found. Otherwise <c>false</c>.</returns>
        public static bool TryMatchUnitInfoBySymbol(string symbol, string unitCategory, out UnitInfo unitInfo)
        {
            unitInfo = null;

            if (string.IsNullOrEmpty(symbol))
            {
                return false;
            }

            int score = int.MinValue;
            //FuzzySearch.Contains(symbol, symbol, out int maxScore);
            string matchedOn = null;

            foreach (var x in GetAllUnitInfos().Where(x => x.UnitCategory == unitCategory))
            {
                int s;

                if (string.Equals(x.Name, symbol, StringComparison.OrdinalIgnoreCase))
                {
                    //UnityEngine.Debug.Log($"Exact match on name for unit {x.Name}.");
                    unitInfo = x;
                    return true;
                }

                if (FuzzySearch.Contains(symbol, x.Name, out s) && s > score)
                {
                    unitInfo = x;
                    score = s;
                    matchedOn = x.Name;
                }

                if (x.Symbols == null)
                {
                    continue;
                }

                for (int i = 0; i < x.Symbols.Length; i++)
                {
                    if (string.IsNullOrEmpty(x.Symbols[i]))
                    {
                        continue;
                    }

                    if (x.Symbols[i] == symbol)
                    {
                        unitInfo = x;
                        return true;
                    }

                    if (FuzzySearch.Contains(symbol, x.Symbols[i], out s) && s > score)
                    {
                        unitInfo = x;
                        score = s;
                        matchedOn = x.Symbols[i];
                    }
                }
            }

            return unitInfo != null;
        }

        /// <summary>
        /// Converts between two units. The units must be of the same category.
        /// </summary>
        /// <param name="value">The value to convert. Should be in the <c>from</c> units.</param>
        /// <param name="from">The unit to convert the value from. <c>value</c> should be in this unit.</param>
        /// <param name="to">To unit to convert the value to. Must be the same category as <c>from</c>.</param>
        /// <returns>The <c>value</c> converted to <c>to</c> units.</returns>
        /// <exception cref="Exception">Throws when either 'from' or 'to' units are invalid, or when the units are of different categories.</exception>
        /// <example>
        /// <code>
        /// decimal meters = 5m;
        /// decimal centimeters = ConvertUnitsFromTo(meters, Units.Meter, Units.Centimeter);
        /// // centimeters = 500
        /// </code>
        /// </example>
        public static decimal ConvertUnitFromTo(decimal value, Units from, Units to)
        {
            if (TryConvertUnitFromTo(value, from, to, out var r))
            {
                return r;
            }
            else
            {
                throw new Exception($"Unable to convert '{from}' to '{to}'.");
            }
        }

        /// <summary>
        /// Converts between two units. The units must be of the same category.
        /// </summary>
        /// <param name="value">The value to convert. Should be in the <c>fromUnitInfo</c> units.</param>
        /// <param name="fromUnitInfo">The unit to convert the value from. <c>value</c> should be in this unit.</param>
        /// <param name="toUnitInfo">To unit to convert the value to. Must be the same category as <c>fromUnitInfo</c>.</param>
        /// <returns>The <c>value</c> converted to <c>toUnitInfo</c> units.</returns>
        /// <exception cref="Exception">Throws when either 'fromUnitInfo' or 'toUnitInfo' units are invalid, or when the units are of different categories.</exception>
        /// <example>
        /// <code>
        /// decimal meters = 5m;
        /// decimal centimeters = ConvertUnitsFromTo(meters, meterUnitInfo, centimeterUnitInfo);
        /// // centimeters = 500
        /// </code>
        /// </example>
        public static decimal ConvertUnitFromTo(decimal value, UnitInfo fromUnitInfo, UnitInfo toUnitInfo)
        {
            if (TryConvertUnitFromTo(value, fromUnitInfo, toUnitInfo, out var r))
            {
                return r;
            }
            else
            {
                throw new Exception($"Unable to convert '{fromUnitInfo.Name}' to '{toUnitInfo.Name}'.");
            }
        }

        /// <summary>
        /// Converts between two units. The units must be of the same category.
        /// </summary>
        /// <param name="value">The value to convert. Should be in the <c>from</c> units.</param>
        /// <param name="from">The unit to convert the value from. <c>value</c> should be in this unit.</param>
        /// <param name="to">To unit to convert the value to. Must be the same category as <c>from</c>.</param>
        /// <param name="converted">The <c>value</c> converted to <c>to</c> units.</param>
        /// <returns><c>true</c> when the unit was successfully converted. Otherwise <c>false</c>.</returns>
        /// <example>
        /// <code>
        /// decimal meters = 5m;
        /// if (TryConvertUnitsFromTo(meters, Units.Meter, Units.Centimeter, out decimal centimeters)
        /// {
        ///     // centimeters = 500
        /// }
        /// </code>
        /// </example>
        public static bool TryConvertUnitFromTo(decimal value, Units from, Units to, out decimal converted)
        {
            if (TryGetUnitInfo(from, out var fromUnitInfo) && TryGetUnitInfo(to, out var toUnitInfo))
            {
                return TryConvertUnitFromTo(value, fromUnitInfo, toUnitInfo, out converted);
            }

            converted = 0.0m;
            return false;
        }
        /// <summary>
        /// Converts between two units. The units must be of the same category.
        /// </summary>
        /// <param name="value">The value to convert. Should be in the <c>fromUnitInfo</c> units.</param>
        /// <param name="fromUnitInfo">The unit to convert the value from. <c>value</c> should be in this unit.</param>
        /// <param name="toUnitInfo">To unit to convert the value to. Must be the same category as <c>fromUnitInfo</c>.</param>
        /// <param name="converted">The <c>value</c> converted to <c>toUnitInfo</c> units.</param>
        /// <returns><c>true</c> when the unit was successfully converted. Otherwise <c>false</c>.</returns>
        /// <example>
        /// <code>
        /// decimal meters = 5m;
        /// if (TryConvertUnitsFromTo(meters, meterUnitInfo, centimeterUnitInfo, out decimal centimeters))
        /// {
        ///     // centimeters = 500
        /// }
        /// </code>
        /// </example>
        /// <exception cref="ArgumentNullException">Throws if either fromUnitInfo or toUnitInfo is null.</exception>
        public static bool TryConvertUnitFromTo(decimal value, UnitInfo fromUnitInfo, UnitInfo toUnitInfo, out decimal converted)
        {
            _ = fromUnitInfo ?? throw new ArgumentNullException(nameof(fromUnitInfo));
            _ = toUnitInfo ?? throw new ArgumentNullException(nameof(toUnitInfo));

            if (fromUnitInfo == toUnitInfo)
            {
                converted = value;
                return true;
            }

            if (!CanConvertBetween(fromUnitInfo, toUnitInfo))
            {
                converted = 0.0m;
                return false;
            }

            if (fromUnitInfo.UsesCustomConversion || toUnitInfo.UsesCustomConversion)
            {
                converted = 0m;

                if (fromUnitInfo.ConvertFromBase != null)
                {
                    converted = fromUnitInfo.ConvertToBase(value);
                }
                else
                {
                    converted = value / fromUnitInfo.Multiplier;
                }

                if (toUnitInfo.ConvertFromBase != null)
                {
                    converted = toUnitInfo.ConvertFromBase(converted);
                }
                else
                {
                    converted = converted * toUnitInfo.Multiplier;
                }
            }
            else
            {
                var m = toUnitInfo.Multiplier / fromUnitInfo.Multiplier;
                converted = value * m;
            }

            return true;
        }

        public static decimal ConvertUnitFromToWithError(decimal value, UnitInfo fromUnitInfo, UnitInfo toUnitInfo, out string error)
        {
            _ = fromUnitInfo ?? throw new ArgumentNullException(nameof(fromUnitInfo));
            _ = toUnitInfo ?? throw new ArgumentNullException(nameof(toUnitInfo));

            if (fromUnitInfo == toUnitInfo)
            {
                error = null;
                return value;
            }

            if (!CanConvertBetween(fromUnitInfo, toUnitInfo))
            {
                error = $"Invalid conversion";
                return value;
            }

            try
            {
                decimal converted;

                if (fromUnitInfo.UsesCustomConversion || toUnitInfo.UsesCustomConversion)
                {
                    converted = 0m;

                    if (fromUnitInfo.ConvertFromBase != null)
                    {
                        converted = fromUnitInfo.ConvertToBase(value);
                    }
                    else
                    {
                        converted = value / fromUnitInfo.Multiplier;
                    }

                    if (toUnitInfo.ConvertFromBase != null)
                    {
                        converted = toUnitInfo.ConvertFromBase(converted);
                    }
                    else
                    {
                        converted = converted * toUnitInfo.Multiplier;
                    }
                }
                else
                {
                    var m = toUnitInfo.Multiplier / fromUnitInfo.Multiplier;
                    converted = value * m;
                }

                error = null;
                return converted;
            }
            catch (OverflowException)
            {
                error = "Overflow";
                return value;
            }
        }

        /// <summary>
        /// Indicates whether or not a value can be converted between the given <c>a</c> and <c>b</c> units.
        /// </summary>
        /// <param name="a">Unit a.</param>
        /// <param name="b">Unit b.</param>
        /// <returns><c>true</c> if both units have the same category. Otherwise <c>false</c>.</returns>
        public static bool CanConvertBetween(Units a, Units b)
        {
            if (TryGetUnitInfo(a, out var unitA) == false) throw new ArgumentException($"Unit not found for '{a}'.", nameof(a));
            if (TryGetUnitInfo(b, out var unitB) == false) throw new ArgumentException($"Unit not found for '{b}'.", nameof(b));

            return CanConvertBetween(unitA, unitB);
        }
        /// <summary>
        /// Indicates whether or not a value can be converted between the given <c>a</c> and <c>b</c> units.
        /// </summary>
        /// <param name="a">Unit a.</param>
        /// <param name="b">Unit b.</param>
        /// <returns><c>true</c> if both units have the same category. Otherwise <c>false</c>.</returns>
        public static bool CanConvertBetween(UnitInfo a, UnitInfo b)
        {
            _ = a ?? throw new ArgumentNullException(nameof(a));
            _ = b ?? throw new ArgumentNullException(nameof(b));

            return a.UnitCategory == b.UnitCategory;
        }

        /// <summary>
        /// Adds a custom unit to the UnitNumberUtility, that can also be used with the <see cref=">UnitAttribute"/>.
        /// Call this using InitializeOnLoad or InitializeOnLoadMethod.
        /// </summary>
        /// <param name="name">The name of the unit. Duplicate names are not allowed.</param>
        /// <param name="symbols">Symbols used for the unit. First value in the array will be used as the primary symbol. Atleast 1 value required. Duplicate symbols are not allowed within the same category.</param>
        /// <param name="unitCategory">The category of the unit. Units can only be converted to another of the same category. Custom categories are allowed.</param>
        /// <param name="multiplier">The multiplier to convert the unit from the base value. For example, meters are the base unit of the distance category, therefore centimeters have a multipler of 100.</param>
        /// <example>
        /// <para>Example of adding centimeters as a custom unit.</para>
        /// <code>
        /// UnitNumberUtility.AddCustomUnit("Centimeter", new string[]{ "cm" }, "Distance", 100m);
        /// </code>
        /// </example>
        public static void AddCustomUnit(string name, string[] symbols, string unitCategory, decimal multiplier)
        {
            InternalAddCustomUnit(name, symbols, unitCategory, false, multiplier, null, null);
        }
        /// <summary>
        /// Adds a custom unit to the UnitNumberUtility, that can also be used with the <see cref=">UnitAttribute"/>.
        /// Call this using InitializeOnLoad or InitializeOnLoadMethod.
        /// </summary>
        /// <param name="name">The name of the unit. Duplicate names are not allowed.</param>
        /// <param name="symbols">Symbols used for the unit. First value in the array will be used as the primary symbol. Atleast 1 value required. Duplicate symbols are not allowed within the same category.</param>
        /// <param name="unitCategory">The category of the unit. Units can only be converted to another of the same category. Custom categories are allowed.</param>
        /// <param name="multiplier">The multiplier to convert the unit from the base value. For example, meters are the base unit of the distance category, therefore centimeters have a multipler of 100.</param>
        /// <example>
        /// <para>Example of adding centimeters as a custom unit.</para>
        /// <code>
        /// UnitNumberUtility.AddCustomUnit("Centimeter", new string[]{ "cm" }, UnitCategory.Distance, 100m);
        /// </code>
        /// </example>
        public static void AddCustomUnit(string name, string[] symbols, UnitCategory unitCategory, decimal multiplier)
        {
            InternalAddCustomUnit(name, symbols, unitCategory.ToString(), false, multiplier, null, null);
        }
        /// <summary>
        /// Adds a custom unit to the UnitNumberUtility, that can also be used with the <see cref=">UnitAttribute"/>.
        /// This overload allows for custom conversion methods but, if possible, the multiplier overloads should be prefered.
        /// Call this using InitializeOnLoad or InitializeOnLoadMethod.
        /// </summary>
        /// <param name="name">The name of the unit. Duplicate names are not allowed.</param>
        /// <param name="symbols">Symbols used for the unit. First value in the array will be used as the primary symbol. Atleast 1 value required. Duplicate symbols are not allowed within the same category.</param>
        /// <param name="unitCategory">The category of the unit. Units can only be converted to another of the same category. Custom categories are allowed.</param>
        /// <param name="convertToBase">Method for converting a given value of the custom unit to the base unit. For example, for centimeter, use: <c>x =&gt; x / 100m;</c>.</param>
        /// <param name="convertFromBase">Method for converting a given value of the base unit to the custom unit. For example, for centimeter, use: <c>x =&gt; x * 100m;</c>.</param>
        /// <example>
        /// <para>Example of adding centimeters as a custom unit.</para>
        /// <code>
        /// UnitNumberUtility.AddCustomUnit("Centimeter", new string[]{ "cm" }, "Distance", x =&gt; x / 100m, x = &gt; x * 100m);
        /// </code>
        /// </example>
        public static void AddCustomUnit(string name, string[] symbols, string unitCategory, Func<decimal, decimal> convertToBase, Func<decimal, decimal> convertFromBase)
        {
            InternalAddCustomUnit(name, symbols, unitCategory, true, 0m, convertToBase, convertFromBase);
        }
        /// <summary>
        /// Adds a custom unit to the UnitNumberUtility, that can also be used with the <see cref=">UnitAttribute"/>.
        /// This overload allows for custom conversion methods but, if possible, the multiplier overloads should be prefered.
        /// Call this using InitializeOnLoad or InitializeOnLoadMethod.
        /// </summary>
        /// <param name="name">The name of the unit. Duplicate names are not allowed.</param>
        /// <param name="symbols">Symbols used for the unit. First value in the array will be used as the primary symbol. Atleast 1 value required. Duplicate symbols are not allowed within the same category.</param>
        /// <param name="unitCategory">The category of the unit. Units can only be converted to another of the same category. Custom categories are allowed.</param>
        /// <param name="convertToBase">Method for converting a given value of the custom unit to the base unit. For example, for centimeter, use: <c>x =&gt; x / 100m;</c>.</param>
        /// <param name="convertFromBase">Method for converting a given value of the base unit to the custom unit. For example, for centimeter, use: <c>x =&gt; x * 100m;</c>.</param>
        /// /// <example>
        /// <para>Example of adding centimeters as a custom unit.</para>
        /// <code>
        /// UnitNumberUtility.AddCustomUnit("Centimeter", new string[]{ "cm" }, UnitCategory.Distance, x =&gt; x / 100m, x = &gt; x * 100m);
        /// </code>
        /// </example>
        public static void AddCustomUnit(string name, string[] symbols, UnitCategory unitCategory, Func<decimal, decimal> convertToBase, Func<decimal, decimal> convertFromBase)
        {
            InternalAddCustomUnit(name, symbols, unitCategory.ToString(), true, 0m, convertToBase, convertFromBase);
        }
        private static void InternalAddCustomUnit(string name, string[] symbols, string unitCategory, bool useCustomConversion, decimal multiplier, Func<decimal, decimal> convertToBase, Func<decimal, decimal> convertFromBase)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (symbols == null)
            {
                throw new ArgumentNullException(nameof(symbols));
            }
            if (symbols.Length == 0)
            {
                throw new ArgumentException("Atleast 1 symbol is required.", nameof(symbols));
            }

            if (string.IsNullOrEmpty(unitCategory))
            {
                throw new ArgumentNullException(nameof(unitCategory));
            }

            if (useCustomConversion)
            {
                if (convertToBase == null)
                {
                    throw new ArgumentNullException(nameof(convertToBase));
                }
                if (convertFromBase == null)
                {
                    throw new ArgumentNullException(nameof(convertFromBase));
                }
            }
            else if (multiplier == 0)
            {
                throw new ArgumentException(nameof(multiplier));
            }

            if (TryGetUnitInfoByName(name, out _))
            {
                throw new Exception($"Duplicate unit name '{name}' is not allowed.");
            }

            for (var i = 0; i < symbols.Length; i++)
            {
                var symbol = symbols[i];

                if (string.IsNullOrEmpty(symbol))
                {
                    throw new ArgumentNullException($"Elements of symbols are not allowed to be null or empty.", nameof(symbols));
                }

                foreach (var x in GetAllUnitInfos().Where(x => x.UnitCategory == unitCategory))
                {
                    if (x.Symbols.Contains(symbol, StringComparer.Ordinal))
                    {
                        throw new Exception($"Duplicate unit symbol '{symbol}' is not allowed in within type '{unitCategory}'.");
                    }
                }
            }

            if (useCustomConversion)
            {
                customUnits.Add(new UnitInfo(name, symbols, unitCategory, convertToBase, convertFromBase));
            }
            else
            {
                customUnits.Add(new UnitInfo(name, symbols, unitCategory, multiplier));
            }
        }
    }

    /// <summary>
    /// Object describing units, including name, symbols and how to convert it to other units.
    /// </summary>
    public class UnitInfo
    {
        /// <summary>
        /// Name of the unit.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Symbols of the unit. First symbol is considered the primary symbol.
        /// </summary>
        public readonly string[] Symbols;
        /// <summary>
        /// The category of the unit. Units can only be converted within the same category.
        /// </summary>
        public readonly string UnitCategory;
        /// <summary>
        /// Multiplier for converting from the base unit.
        /// </summary>
        public readonly decimal Multiplier;
        /// <summary>
        /// Custom method for converting from the base unit.
        /// </summary>
        public readonly Func<decimal, decimal> ConvertFromBase;
        /// <summary>
        /// Custom method for converting to the base unit.
        /// </summary>
        public readonly Func<decimal, decimal> ConvertToBase;
        /// <summary>
        /// Indicates whether the UnitInfo should use the <c>multiplier</c> or the <c>ConvertFromBase</c> and <c>ConvertToBase</c> methods.
        /// </summary>
        public readonly bool UsesCustomConversion;

        internal UnitInfo(string name, string[] symbols, UnitCategory unitCategory, decimal multiplier)
            : this(name, symbols, unitCategory.ToString(), multiplier)
        { }
        internal UnitInfo(string name, string[] symbols, string unitCategory, decimal multiplier)
        {
            this.Name = name;
            this.Symbols = symbols;
            this.UnitCategory = unitCategory;
            this.Multiplier = multiplier;
            this.UsesCustomConversion = false;
            this.ConvertToBase = null;
            this.ConvertFromBase = null;
        }
        internal UnitInfo(string name, string[] symbols, UnitCategory unitCategory, Func<decimal, decimal> convertToBase, Func<decimal, decimal> convertFromBase)
            : this(name, symbols, unitCategory.ToString(), convertToBase, convertFromBase)
        { }
        internal UnitInfo(string name, string[] symbols, string unitCategory, Func<decimal, decimal> convertToBase, Func<decimal, decimal> convertFromBase)
        {
            _ = convertToBase ?? throw new ArgumentNullException(nameof(convertToBase));
            _ = convertFromBase ?? throw new ArgumentNullException(nameof(convertFromBase));

            this.Name = name;
            this.Symbols = symbols;
            this.UnitCategory = unitCategory;
            this.Multiplier = 0m;
            this.UsesCustomConversion = true;
            this.ConvertToBase = convertToBase;
            this.ConvertFromBase = convertFromBase;
        }
    }
}
#endif