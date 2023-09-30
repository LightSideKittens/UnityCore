using System.Collections.Generic;

public partial class TouchMacros
{
    private static Dictionary<Key, string> labelByKey = new()
    {
        { Key.Space, "⌴"},
        { Key.UpArrow, "⇧"},
        { Key.RightArrow, "⇨"},
        { Key.LeftArrow, "⇦"},
        { Key.DownArrow, "⇩"},
    };
    
    private enum Key
    {
        /// <summary>
        ///   <para>Space key.</para>
        /// </summary>
        Space = 32, // 0x00000020
        /// <summary>
        ///   <para>The tab key.</para>
        /// </summary>
        Tab = 9,
        /// <summary>
        ///   <para>'a' key.</para>
        /// </summary>
        A = 97, // 0x00000061

        /// <summary>
        ///   <para>'b' key.</para>
        /// </summary>
        B = 98, // 0x00000062

        /// <summary>
        ///   <para>'c' key.</para>
        /// </summary>
        C = 99, // 0x00000063

        /// <summary>
        ///   <para>'d' key.</para>
        /// </summary>
        D = 100, // 0x00000064

        /// <summary>
        ///   <para>'e' key.</para>
        /// </summary>
        E = 101, // 0x00000065

        /// <summary>
        ///   <para>'f' key.</para>
        /// </summary>
        F = 102, // 0x00000066

        /// <summary>
        ///   <para>'g' key.</para>
        /// </summary>
        G = 103, // 0x00000067

        /// <summary>
        ///   <para>'h' key.</para>
        /// </summary>
        H = 104, // 0x00000068

        /// <summary>
        ///   <para>'i' key.</para>
        /// </summary>
        I = 105, // 0x00000069

        /// <summary>
        ///   <para>'j' key.</para>
        /// </summary>
        J = 106, // 0x0000006A

        /// <summary>
        ///   <para>'k' key.</para>
        /// </summary>
        K = 107, // 0x0000006B

        /// <summary>
        ///   <para>'l' key.</para>
        /// </summary>
        L = 108, // 0x0000006C

        /// <summary>
        ///   <para>'m' key.</para>
        /// </summary>
        M = 109, // 0x0000006D

        /// <summary>
        ///   <para>'n' key.</para>
        /// </summary>
        N = 110, // 0x0000006E

        /// <summary>
        ///   <para>'o' key.</para>
        /// </summary>
        O = 111, // 0x0000006F

        /// <summary>
        ///   <para>'p' key.</para>
        /// </summary>
        P = 112, // 0x00000070

        /// <summary>
        ///   <para>'q' key.</para>
        /// </summary>
        Q = 113, // 0x00000071

        /// <summary>
        ///   <para>'r' key.</para>
        /// </summary>
        R = 114, // 0x00000072

        /// <summary>
        ///   <para>'s' key.</para>
        /// </summary>
        S = 115, // 0x00000073

        /// <summary>
        ///   <para>'t' key.</para>
        /// </summary>
        T = 116, // 0x00000074

        /// <summary>
        ///   <para>'u' key.</para>
        /// </summary>
        U = 117, // 0x00000075

        /// <summary>
        ///   <para>'v' key.</para>
        /// </summary>
        V = 118, // 0x00000076

        /// <summary>
        ///   <para>'w' key.</para>
        /// </summary>
        W = 119, // 0x00000077

        /// <summary>
        ///   <para>'x' key.</para>
        /// </summary>
        X = 120, // 0x00000078

        /// <summary>
        ///   <para>'y' key.</para>
        /// </summary>
        Y = 121, // 0x00000079

        /// <summary>
        ///   <para>'z' key.</para>
        /// </summary>
        Z = 122, // 0x0000007A
        
        UpArrow = 273, // 0x00000111
        /// <summary>
        ///   <para>Down arrow key.</para>
        /// </summary>
        DownArrow = 274, // 0x00000112
        /// <summary>
        ///   <para>Right arrow key.</para>
        /// </summary>
        RightArrow = 275, // 0x00000113
        /// <summary>
        ///   <para>Left arrow key.</para>
        /// </summary>
        LeftArrow = 276, // 0x00000114
    }
}