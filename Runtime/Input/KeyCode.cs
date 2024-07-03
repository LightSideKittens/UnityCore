#if UNITY_EDITOR
public enum LSKeyCode
{
    /// <summary>Left mouse button</summary>
    LBUTTON = 1,

    /// <summary>Right mouse button</summary>
    RBUTTON = 2,

    /// <summary>Control-break processing</summary>
    CANCEL = 3,

    /// <summary>
    /// Middle mouse button (three-button mouse) - NOT contiguous with LBUTTON and RBUTTON
    /// </summary>
    MBUTTON = 4,

    /// <summary>
    /// Windows 2000/XP: X1 mouse button - NOT contiguous with LBUTTON and RBUTTON
    /// </summary>
    XBUTTON1 = 5,

    /// <summary>
    /// Windows 2000/XP: X2 mouse button - NOT contiguous with LBUTTON and RBUTTON
    /// </summary>
    XBUTTON2 = 6,

    /// <summary>BACKSPACE key</summary>
    BACK = 8,

    /// <summary>TAB key</summary>
    TAB = 9,

    /// <summary>CLEAR key</summary>
    CLEAR = 12, // 0x0000000C

    /// <summary>ENTER key</summary>
    RETURN = 13, // 0x0000000D

    /// <summary>SHIFT key</summary>
    SHIFT = 16, // 0x00000010

    /// <summary>CTRL key</summary>
    CONTROL = 17, // 0x00000011

    /// <summary>ALT key</summary>
    MENU = 18, // 0x00000012

    /// <summary>PAUSE key</summary>
    PAUSE = 19, // 0x00000013

    /// <summary>CAPS LOCK key</summary>
    CAPITAL = 20, // 0x00000014

    /// <summary>
    /// IME Hanguel mode (maintained for compatibility; use HANGUL)
    /// </summary>
    HANGEUL = 21, // 0x00000015

    /// <summary>IME Hangul mode</summary>
    HANGUL = 21, // 0x00000015

    /// <summary>Input Method Editor (IME) Kana mode</summary>
    KANA = 21, // 0x00000015

    /// <summary>IME Junja mode</summary>
    JUNJA = 23, // 0x00000017

    /// <summary>IME final mode</summary>
    FINAL = 24, // 0x00000018

    /// <summary>IME Hanja mode</summary>
    HANJA = 25, // 0x00000019

    /// <summary>IME Kanji mode</summary>
    KANJI = 25, // 0x00000019

    /// <summary>ESC key</summary>
    ESCAPE = 27, // 0x0000001B

    /// <summary>IME convert</summary>
    CONVERT = 28, // 0x0000001C

    /// <summary>IME nonconvert</summary>
    NONCONVERT = 29, // 0x0000001D

    /// <summary>IME accept</summary>
    ACCEPT = 30, // 0x0000001E

    /// <summary>IME mode change request</summary>
    MODECHANGE = 31, // 0x0000001F

    /// <summary>SPACEBAR</summary>
    SPACE = 32, // 0x00000020

    /// <summary>PAGE UP key</summary>
    PRIOR = 33, // 0x00000021

    /// <summary>PAGE DOWN key</summary>
    NEXT = 34, // 0x00000022

    /// <summary>END key</summary>
    END = 35, // 0x00000023

    /// <summary>HOME key</summary>
    HOME = 36, // 0x00000024

    /// <summary>LEFT ARROW key</summary>
    LEFT = 37, // 0x00000025

    /// <summary>UP ARROW key</summary>
    UP = 38, // 0x00000026

    /// <summary>RIGHT ARROW key</summary>
    RIGHT = 39, // 0x00000027

    /// <summary>DOWN ARROW key</summary>
    DOWN = 40, // 0x00000028

    /// <summary>SELECT key</summary>
    SELECT = 41, // 0x00000029

    /// <summary>PRINT key</summary>
    PRINT = 42, // 0x0000002A

    /// <summary>EXECUTE key</summary>
    EXECUTE = 43, // 0x0000002B

    /// <summary>PRINT SCREEN key</summary>
    SNAPSHOT = 44, // 0x0000002C

    /// <summary>INS key</summary>
    INSERT = 45, // 0x0000002D

    /// <summary>DEL key</summary>
    DELETE = 46, // 0x0000002E

    /// <summary>HELP key</summary>
    HELP = 47, // 0x0000002F

    /// <summary>0 key</summary>
    Num0 = 48, // 0x00000030

    /// <summary>1 key</summary>
    Num1 = 49, // 0x00000031

    /// <summary>2 key</summary>
    Num2 = 50, // 0x00000032

    /// <summary>3 key</summary>
    Num3 = 51, // 0x00000033

    /// <summary>4 key</summary>
    Num4 = 52, // 0x00000034

    /// <summary>5 key</summary>
    Num5 = 53, // 0x00000035

    /// <summary>6 key</summary>
    Num6 = 54, // 0x00000036

    /// <summary>7 key</summary>
    Num7 = 55, // 0x00000037

    /// <summary>8 key</summary>
    Num8 = 56, // 0x00000038

    /// <summary>9 key</summary>
    Num9 = 57, // 0x00000039

    /// <summary>A key</summary>
    A = 65, // 0x00000041

    /// <summary>B key</summary>
    B = 66, // 0x00000042

    /// <summary>C key</summary>
    C = 67, // 0x00000043

    /// <summary>D key</summary>
    D = 68, // 0x00000044

    /// <summary>E key</summary>
    E = 69, // 0x00000045

    /// <summary>F key</summary>
    F = 70, // 0x00000046

    /// <summary>G key</summary>
    G = 71, // 0x00000047

    /// <summary>H key</summary>
    H = 72, // 0x00000048

    /// <summary>I key</summary>
    I = 73, // 0x00000049

    /// <summary>J key</summary>
    J = 74, // 0x0000004A

    /// <summary>K key</summary>
    K = 75, // 0x0000004B

    /// <summary>L key</summary>
    L = 76, // 0x0000004C

    /// <summary>M key</summary>
    M = 77, // 0x0000004D

    /// <summary>N key</summary>
    N = 78, // 0x0000004E

    /// <summary>O key</summary>
    O = 79, // 0x0000004F

    /// <summary>P key</summary>
    P = 80, // 0x00000050

    /// <summary>Q key</summary>
    Q = 81, // 0x00000051

    /// <summary>R key</summary>
    R = 82, // 0x00000052

    /// <summary>S key</summary>
    S = 83, // 0x00000053

    /// <summary>T key</summary>
    T = 84, // 0x00000054

    /// <summary>U key</summary>
    U = 85, // 0x00000055

    /// <summary>V key</summary>
    V = 86, // 0x00000056

    /// <summary>W key</summary>
    W = 87, // 0x00000057

    /// <summary>X key</summary>
    X = 88, // 0x00000058

    /// <summary>Y key</summary>
    Y = 89, // 0x00000059

    /// <summary>Z key</summary>
    Z = 90, // 0x0000005A

    /// <summary>Left Windows key (Microsoft Natural keyboard)</summary>
    LWIN = 91, // 0x0000005B

    /// <summary>Right Windows key (Natural keyboard)</summary>
    RWIN = 92, // 0x0000005C

    /// <summary>Applications key (Natural keyboard)</summary>
    APPS = 93, // 0x0000005D

    /// <summary>Computer Sleep key</summary>
    SLEEP = 95, // 0x0000005F

    /// <summary>Numeric keypad 0 key</summary>
    NUMPAD0 = 96, // 0x00000060

    /// <summary>Numeric keypad 1 key</summary>
    NUMPAD1 = 97, // 0x00000061

    /// <summary>Numeric keypad 2 key</summary>
    NUMPAD2 = 98, // 0x00000062

    /// <summary>Numeric keypad 3 key</summary>
    NUMPAD3 = 99, // 0x00000063

    /// <summary>Numeric keypad 4 key</summary>
    NUMPAD4 = 100, // 0x00000064

    /// <summary>Numeric keypad 5 key</summary>
    NUMPAD5 = 101, // 0x00000065

    /// <summary>Numeric keypad 6 key</summary>
    NUMPAD6 = 102, // 0x00000066

    /// <summary>Numeric keypad 7 key</summary>
    NUMPAD7 = 103, // 0x00000067

    /// <summary>Numeric keypad 8 key</summary>
    NUMPAD8 = 104, // 0x00000068

    /// <summary>Numeric keypad 9 key</summary>
    NUMPAD9 = 105, // 0x00000069

    /// <summary>Multiply key</summary>
    MULTIPLY = 106, // 0x0000006A

    /// <summary>Add key</summary>
    ADD = 107, // 0x0000006B

    /// <summary>Separator key</summary>
    SEPARATOR = 108, // 0x0000006C

    /// <summary>Subtract key</summary>
    SUBTRACT = 109, // 0x0000006D

    /// <summary>Decimal key</summary>
    DECIMAL = 110, // 0x0000006E

    /// <summary>Divide key</summary>
    DIVIDE = 111, // 0x0000006F

    /// <summary>F1 key</summary>
    F1 = 112, // 0x00000070

    /// <summary>F2 key</summary>
    F2 = 113, // 0x00000071

    /// <summary>F3 key</summary>
    F3 = 114, // 0x00000072

    /// <summary>F4 key</summary>
    F4 = 115, // 0x00000073

    /// <summary>F5 key</summary>
    F5 = 116, // 0x00000074

    /// <summary>F6 key</summary>
    F6 = 117, // 0x00000075

    /// <summary>F7 key</summary>
    F7 = 118, // 0x00000076

    /// <summary>F8 key</summary>
    F8 = 119, // 0x00000077

    /// <summary>F9 key</summary>
    F9 = 120, // 0x00000078

    /// <summary>F10 key</summary>
    F10 = 121, // 0x00000079

    /// <summary>F11 key</summary>
    F11 = 122, // 0x0000007A

    /// <summary>F12 key</summary>
    F12 = 123, // 0x0000007B

    /// <summary>F13 key</summary>
    F13 = 124, // 0x0000007C

    /// <summary>F14 key</summary>
    F14 = 125, // 0x0000007D

    /// <summary>F15 key</summary>
    F15 = 126, // 0x0000007E

    /// <summary>F16 key</summary>
    F16 = 127, // 0x0000007F

    /// <summary>F17 key</summary>
    F17 = 128, // 0x00000080

    /// <summary>F18 key</summary>
    F18 = 129, // 0x00000081

    /// <summary>F19 key</summary>
    F19 = 130, // 0x00000082

    /// <summary>F20 key</summary>
    F20 = 131, // 0x00000083

    /// <summary>F21 key</summary>
    F21 = 132, // 0x00000084

    /// <summary>F22 key</summary>
    F22 = 133, // 0x00000085

    /// <summary>F23 key</summary>
    F23 = 134, // 0x00000086

    /// <summary>F24 key</summary>
    F24 = 135, // 0x00000087

    /// <summary>NUM LOCK key</summary>
    NUMLOCK = 144, // 0x00000090

    /// <summary>SCROLL LOCK key</summary>
    SCROLL = 145, // 0x00000091

    /// <summary>
    /// Left SHIFT key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
    /// </summary>
    LSHIFT = 160, // 0x000000A0

    /// <summary>
    /// Right SHIFT key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
    /// </summary>
    RSHIFT = 161, // 0x000000A1

    /// <summary>
    /// Left CONTROL key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
    /// </summary>
    LCONTROL = 162, // 0x000000A2

    /// <summary>
    /// Right CONTROL key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
    /// </summary>
    RCONTROL = 163, // 0x000000A3

    /// <summary>
    /// Left MENU key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
    /// </summary>
    LMENU = 164, // 0x000000A4

    /// <summary>
    /// Right MENU key - Used only as parameters to GetAsyncKeyState() and GetKeyState()
    /// </summary>
    RMENU = 165, // 0x000000A5

    /// <summary>Windows 2000/XP: Browser Back key</summary>
    BROWSER_BACK = 166, // 0x000000A6

    /// <summary>Windows 2000/XP: Browser Forward key</summary>
    BROWSER_FORWARD = 167, // 0x000000A7

    /// <summary>Windows 2000/XP: Browser Refresh key</summary>
    BROWSER_REFRESH = 168, // 0x000000A8

    /// <summary>Windows 2000/XP: Browser Stop key</summary>
    BROWSER_STOP = 169, // 0x000000A9

    /// <summary>Windows 2000/XP: Browser Search key</summary>
    BROWSER_SEARCH = 170, // 0x000000AA

    /// <summary>Windows 2000/XP: Browser Favorites key</summary>
    BROWSER_FAVORITES = 171, // 0x000000AB

    /// <summary>Windows 2000/XP: Browser Start and Home key</summary>
    BROWSER_HOME = 172, // 0x000000AC

    /// <summary>Windows 2000/XP: Volume Mute key</summary>
    VOLUME_MUTE = 173, // 0x000000AD

    /// <summary>Windows 2000/XP: Volume Down key</summary>
    VOLUME_DOWN = 174, // 0x000000AE

    /// <summary>Windows 2000/XP: Volume Up key</summary>
    VOLUME_UP = 175, // 0x000000AF

    /// <summary>Windows 2000/XP: Next Track key</summary>
    MEDIA_NEXT_TRACK = 176, // 0x000000B0

    /// <summary>Windows 2000/XP: Previous Track key</summary>
    MEDIA_PREV_TRACK = 177, // 0x000000B1

    /// <summary>Windows 2000/XP: Stop Media key</summary>
    MEDIA_STOP = 178, // 0x000000B2

    /// <summary>Windows 2000/XP: Play/Pause Media key</summary>
    MEDIA_PLAY_PAUSE = 179, // 0x000000B3

    /// <summary>Windows 2000/XP: Start Mail key</summary>
    LAUNCH_MAIL = 180, // 0x000000B4

    /// <summary>Windows 2000/XP: Select Media key</summary>
    LAUNCH_MEDIA_SELECT = 181, // 0x000000B5

    /// <summary>Windows 2000/XP: Start Application 1 key</summary>
    LAUNCH_APP1 = 182, // 0x000000B6

    /// <summary>Windows 2000/XP: Start Application 2 key</summary>
    LAUNCH_APP2 = 183, // 0x000000B7

    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the ';:' key
    /// </summary>
    OEM_1 = 186, // 0x000000BA

    /// <summary>Windows 2000/XP: For any country/region, the '+' key</summary>
    OEM_PLUS = 187, // 0x000000BB

    /// <summary>Windows 2000/XP: For any country/region, the ',' key</summary>
    OEM_COMMA = 188, // 0x000000BC

    /// <summary>Windows 2000/XP: For any country/region, the '-' key</summary>
    OEM_MINUS = 189, // 0x000000BD

    /// <summary>Windows 2000/XP: For any country/region, the '.' key</summary>
    OEM_PERIOD = 190, // 0x000000BE

    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '/?' key
    /// </summary>
    OEM_2 = 191, // 0x000000BF

    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '`~' key
    /// </summary>
    OEM_3 = 192, // 0x000000C0

    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '[{' key
    /// </summary>
    OEM_4 = 219, // 0x000000DB

    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '\|' key
    /// </summary>
    OEM_5 = 220, // 0x000000DC

    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the ']}' key
    /// </summary>
    OEM_6 = 221, // 0x000000DD

    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the 'single-quote/double-quote' key
    /// </summary>
    OEM_7 = 222, // 0x000000DE

    /// <summary>
    /// Used for miscellaneous characters; it can vary by keyboard.
    /// </summary>
    OEM_8 = 223, // 0x000000DF

    /// <summary>
    /// Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
    /// </summary>
    OEM_102 = 226, // 0x000000E2

    /// <summary>
    /// Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
    /// </summary>
    PROCESSKEY = 229, // 0x000000E5

    /// <summary>
    /// Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes. The PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information, see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
    /// </summary>
    PACKET = 231, // 0x000000E7

    /// <summary>Attn key</summary>
    ATTN = 246, // 0x000000F6

    /// <summary>CrSel key</summary>
    CRSEL = 247, // 0x000000F7

    /// <summary>ExSel key</summary>
    EXSEL = 248, // 0x000000F8

    /// <summary>Erase EOF key</summary>
    EREOF = 249, // 0x000000F9

    /// <summary>Play key</summary>
    PLAY = 250, // 0x000000FA

    /// <summary>Zoom key</summary>
    ZOOM = 251, // 0x000000FB

    /// <summary>Reserved</summary>
    NONAME = 252, // 0x000000FC

    /// <summary>PA1 key</summary>
    PA1 = 253, // 0x000000FD

    /// <summary>Clear key</summary>
    OEM_CLEAR = 254, // 0x000000FE
}
#endif