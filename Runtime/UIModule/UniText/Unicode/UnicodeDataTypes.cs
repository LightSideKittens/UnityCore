#nullable enable


public enum BidiDirection : byte
{
    LeftToRight = 0,
    RightToLeft = 1
}


public enum BidiClass : byte
{
    LeftToRight,
    RightToLeft,
    ArabicLetter,

    EuropeanNumber,
    EuropeanSeparator,
    EuropeanTerminator,
    ArabicNumber,
    CommonSeparator,
    NonspacingMark,

    BoundaryNeutral,
    ParagraphSeparator,
    SegmentSeparator,
    WhiteSpace,
    OtherNeutral,

    LeftToRightEmbedding,
    LeftToRightOverride,
    RightToLeftEmbedding,
    RightToLeftOverride,
    PopDirectionalFormat,

    LeftToRightIsolate,
    RightToLeftIsolate,
    FirstStrongIsolate,
    PopDirectionalIsolate
}


public enum BidiPairedBracketType : byte
{
    None,
    Open,
    Close
}


/// <summary>
/// Unicode General Category (from UnicodeData.txt or DerivedGeneralCategory.txt)
/// </summary>
public enum GeneralCategory : byte
{
    // Letters
    Lu, // Letter, uppercase
    Ll, // Letter, lowercase
    Lt, // Letter, titlecase
    Lm, // Letter, modifier
    Lo, // Letter, other
    
    // Marks
    Mn, // Mark, nonspacing
    Mc, // Mark, spacing combining
    Me, // Mark, enclosing
    
    // Numbers
    Nd, // Number, decimal digit
    Nl, // Number, letter
    No, // Number, other
    
    // Punctuation
    Pc, // Punctuation, connector
    Pd, // Punctuation, dash
    Ps, // Punctuation, open
    Pe, // Punctuation, close
    Pi, // Punctuation, initial quote
    Pf, // Punctuation, final quote
    Po, // Punctuation, other
    
    // Symbols
    Sm, // Symbol, math
    Sc, // Symbol, currency
    Sk, // Symbol, modifier
    So, // Symbol, other
    
    // Separators
    Zs, // Separator, space
    Zl, // Separator, line
    Zp, // Separator, paragraph
    
    // Other
    Cc, // Other, control
    Cf, // Other, format
    Cs, // Other, surrogate
    Co, // Other, private use
    Cn  // Other, not assigned
}


/// <summary>
/// East Asian Width property (from EastAsianWidth.txt)
/// </summary>
public enum EastAsianWidth : byte
{
    N,  // Neutral (not East Asian)
    A,  // Ambiguous
    H,  // Halfwidth
    W,  // Wide
    F,  // Fullwidth
    Na  // Narrow
}


public enum JoiningType : byte
{
    NonJoining,
    Transparent,
    JoinCausing,
    LeftJoining,
    RightJoining,
    DualJoining
}


public enum JoiningGroup : byte
{
    NoJoiningGroup,

    AfricanFeh,
    AfricanNoon,
    AfricanQaf,
    Ain,
    Alaph,
    Alef,
    Beh,
    Beth,
    BurushaskiYehBarree,
    Dal,
    DalathRish,
    E,
    FarsiYeh,
    Fe,
    Feh,
    FinalSemkath,
    Gaf,
    Gamal,
    Hah,
    HanifiRohingyaKinnaYa,
    HanifiRohingyaPa,
    He,
    Heh,
    HehGoal,
    Heth,
    Kaf,
    Kaph,
    KashmiriYeh,
    Khaph,
    KnottedHeh,
    Lam,
    Lamadh,

    MalayalamBha,
    MalayalamJa,
    MalayalamLla,
    MalayalamLlla,
    MalayalamNga,
    MalayalamNna,
    MalayalamNnna,
    MalayalamNya,
    MalayalamRa,
    MalayalamSsa,
    MalayalamTta,

    ManichaeanAleph,
    ManichaeanAyin,
    ManichaeanBeth,
    ManichaeanDaleth,
    ManichaeanDhamedh,
    ManichaeanFive,
    ManichaeanGimel,
    ManichaeanHeth,
    ManichaeanHundred,
    ManichaeanKaph,
    ManichaeanLamedh,
    ManichaeanMem,
    ManichaeanNun,
    ManichaeanOne,
    ManichaeanPe,
    ManichaeanQoph,
    ManichaeanResh,
    ManichaeanSadhe,
    ManichaeanSamekh,
    ManichaeanTaw,
    ManichaeanTen,
    ManichaeanTeth,
    ManichaeanThamedh,
    ManichaeanTwenty,
    ManichaeanWaw,
    ManichaeanYodh,
    ManichaeanZayin,

    Meem,
    Mim,
    Noon,
    Nun,
    Nya,
    Pe,
    Qaf,
    Qaph,
    Reh,
    ReversedPe,
    RohingyaYeh,
    Sad,
    Sadhe,
    Seen,
    Semkath,
    Shin,
    StraightWaw,
    SwashKaf,
    SyriacWaw,
    Tah,
    Taw,
    TehMarbuta,
    TehMarbutaGoal,
    HamzaOnHehGoal,
    Teth,
    ThinYeh,
    VerticalTail,
    Waw,
    Yeh,
    YehBarree,
    YehWithTail,
    Yudh,
    YudhHe,
    Zain,
    Zhain
}


/// <summary>
/// Unicode Script (UAX #24)
/// </summary>
public enum UnicodeScript : byte
{
    Unknown = 0,
    Common,
    Inherited,
    
    // Major scripts
    Latin,
    Greek,
    Cyrillic,
    Armenian,
    Hebrew,
    Arabic,
    Syriac,
    Thaana,
    Devanagari,
    Bengali,
    Gurmukhi,
    Gujarati,
    Oriya,
    Tamil,
    Telugu,
    Kannada,
    Malayalam,
    Sinhala,
    Thai,
    Lao,
    Tibetan,
    Myanmar,
    Georgian,
    Hangul,
    Ethiopic,
    Cherokee,
    CanadianAboriginal,
    Ogham,
    Runic,
    Khmer,
    Mongolian,
    Hiragana,
    Katakana,
    Bopomofo,
    Han,
    Yi,
    OldItalic,
    Gothic,
    Deseret,
    Tagalog,
    Hanunoo,
    Buhid,
    Tagbanwa,
    Limbu,
    TaiLe,
    LinearB,
    Ugaritic,
    Shavian,
    Osmanya,
    Cypriot,
    Braille,
    Buginese,
    Coptic,
    NewTaiLue,
    Glagolitic,
    Tifinagh,
    SylotiNagri,
    OldPersian,
    Kharoshthi,
    Balinese,
    Cuneiform,
    Phoenician,
    PhagsPa,
    Nko,
    Sundanese,
    Lepcha,
    OlChiki,
    Vai,
    Saurashtra,
    KayahLi,
    Rejang,
    Lycian,
    Carian,
    Lydian,
    Cham,
    TaiTham,
    TaiViet,
    Avestan,
    EgyptianHieroglyphs,
    Samaritan,
    Lisu,
    Bamum,
    Javanese,
    MeeteiMayek,
    ImperialAramaic,
    OldSouthArabian,
    InscriptionalParthian,
    InscriptionalPahlavi,
    OldTurkic,
    Kaithi,
    Batak,
    Brahmi,
    Mandaic,
    Chakma,
    MeroiticCursive,
    MeroiticHieroglyphs,
    Miao,
    Sharada,
    SoraSompeng,
    Takri,
    CaucasianAlbanian,
    BassaVah,
    Duployan,
    Elbasan,
    Grantha,
    PahawhHmong,
    Khojki,
    LinearA,
    Mahajani,
    Manichaean,
    MendeKikakui,
    Modi,
    Mro,
    OldNorthArabian,
    Nabataean,
    Palmyrene,
    PauCinHau,
    OldPermic,
    PsalterPahlavi,
    Siddham,
    Khudawadi,
    Tirhuta,
    WarangCiti,
    Ahom,
    AnatolianHieroglyphs,
    Hatran,
    Multani,
    OldHungarian,
    SignWriting,
    Adlam,
    Bhaiksuki,
    Marchen,
    Newa,
    Osage,
    Tangut,
    MasaramGondi,
    Nushu,
    Soyombo,
    ZanabazarSquare,
    Dogra,
    GunjalaGondi,
    Makasar,
    Medefaidrin,
    HanifiRohingya,
    Sogdian,
    OldSogdian,
    Elymaic,
    Nandinagari,
    NyiakengPuachueHmong,
    Wancho,
    Chorasmian,
    DivesAkuru,
    KhitanSmallScript,
    Yezidi,
    CyproMinoan,
    OldUyghur,
    Tangsa,
    Toto,
    Vithkuqi,
    Kawi,
    NagMundari,
    
    // Unicode 16.0
    Garay,
    GurungKhema,
    KiratRai,
    OlOnal,
    Sunuwar,
    Todhri,
    TuluTigalari,
    
    // Unicode 17.0
    BeriaErfe,
    Sidetic,
    TaiYo,
    TolongSiki
}


/// <summary>
/// Line Break Class (UAX #14)
/// </summary>
public enum LineBreakClass : byte
{
    Unknown = 0,
    
    // Non-tailorable Line Breaking Classes
    BK,     // Mandatory Break
    CR,     // Carriage Return
    LF,     // Line Feed
    CM,     // Combining Mark
    NL,     // Next Line
    SG,     // Surrogate
    WJ,     // Word Joiner
    ZW,     // Zero Width Space
    GL,     // Non-breaking (Glue)
    SP,     // Space
    ZWJ,    // Zero Width Joiner
    
    // Break Opportunities
    B2,     // Break Opportunity Before and After
    BA,     // Break After
    BB,     // Break Before
    HY,     // Hyphen
    CB,     // Contingent Break Opportunity
    
    // Characters Prohibiting Certain Breaks
    CL,     // Close Punctuation
    CP,     // Close Parenthesis
    EX,     // Exclamation/Interrogation
    IN,     // Inseparable
    NS,     // Nonstarter
    OP,     // Open Punctuation
    QU,     // Quotation
    
    // Numeric Context
    IS,     // Infix Numeric Separator
    NU,     // Numeric
    PO,     // Postfix Numeric
    PR,     // Prefix Numeric
    SY,     // Symbols Allowing Break After
    
    // Other Characters
    AI,     // Ambiguous (Alphabetic or Ideographic)
    AL,     // Alphabetic
    CJ,     // Conditional Japanese Starter
    EB,     // Emoji Base
    EM,     // Emoji Modifier
    H2,     // Hangul LV Syllable
    H3,     // Hangul LVT Syllable
    HL,     // Hebrew Letter
    ID,     // Ideographic
    JL,     // Hangul L Jamo
    JV,     // Hangul V Jamo
    JT,     // Hangul T Jamo
    RI,     // Regional Indicator
    SA,     // Complex Context Dependent (South East Asian)
    XX,     // Unknown
    
    // Aksara (UAX #14 revision 51)
    AK,     // Aksara
    AP,     // Aksara Pre-Base
    AS,     // Aksara Start
    VF,     // Virama Final
    VI,     // Virama
    
    // Additional classes
    HH      // Unambiguous Hyphen (HYPHEN, EN DASH, MAQAF, etc.)
}


/// <summary>
/// Grapheme_Cluster_Break property values (UAX #29)
/// </summary>
public enum GraphemeClusterBreak : byte
{
    Other = 0,          // XX - Any other
    CR,                 // CR - Carriage Return
    LF,                 // LF - Line Feed
    Control,            // Control, Cf (except ZWJ, ZWNJ, etc)
    Extend,             // Grapheme_Extend = Yes
    ZWJ,                // Zero Width Joiner
    Regional_Indicator, // Regional Indicator
    Prepend,            // Prepended_Concatenation_Mark = Yes
    SpacingMark,        // Mc spacing combining marks
    L,                  // Hangul Syllable Type L
    V,                  // Hangul Syllable Type V
    T,                  // Hangul Syllable Type T
    LV,                 // Hangul Syllable Type LV
    LVT                 // Hangul Syllable Type LVT
}


/// <summary>
/// Indic_Conjunct_Break property values for GB9c rule (UAX #29).
/// Loaded from DerivedCoreProperties.txt (InCB property).
/// </summary>
public enum IndicConjunctBreak : byte
{
    None = 0,    // Default value
    Linker,      // Virama and similar joiners
    Consonant,   // Consonants in conjunct-forming scripts
    Extend       // Extending marks in conjunct context
}


public interface IUnicodeDataProvider
{
    BidiClass GetBidiClass(int codePoint);

    bool IsBidiMirrored(int codePoint);

    int GetBidiMirroringGlyph(int codePoint);

    BidiPairedBracketType GetBidiPairedBracketType(int codePoint);

    int GetBidiPairedBracket(int codePoint);

    JoiningType GetJoiningType(int codePoint);

    JoiningGroup GetJoiningGroup(int codePoint);
    
    /// <summary>
    /// Get Unicode Script (UAX #24)
    /// </summary>
    UnicodeScript GetScript(int codePoint);
    
    /// <summary>
    /// Get Line Break Class (UAX #14)
    /// </summary>
    LineBreakClass GetLineBreakClass(int codePoint);
    
    /// <summary>
    /// Check if codepoint has Extended_Pictographic property (from emoji-data.txt)
    /// Used for LB30b: Extended_Pictographic × EM
    /// </summary>
    bool IsExtendedPictographic(int codePoint);
    
    /// <summary>
    /// Get General Category (from DerivedGeneralCategory.txt)
    /// Used for LB1 SA resolution and LB15a/LB15b QU_Pi/QU_Pf detection
    /// </summary>
    GeneralCategory GetGeneralCategory(int codePoint);
    
    /// <summary>
    /// Get East Asian Width (from EastAsianWidth.txt)
    /// Used for LB30 to detect East Asian context
    /// </summary>
    EastAsianWidth GetEastAsianWidth(int codePoint);
    
    /// <summary>
    /// Check if codepoint is an Unambiguous Hyphen (HH class per UAX #14).
    /// Some HH characters may be marked as BA in LineBreak.txt but should be treated as HH.
    /// Used for LB20a, LB21, LB21a.
    /// </summary>
    bool IsUnambiguousHyphen(int codePoint);
    
    /// <summary>
    /// Check if codepoint is U+25CC DOTTED CIRCLE (placeholder for combining marks).
    /// Used for LB28a Brahmic script rules.
    /// </summary>
    bool IsDottedCircle(int codePoint);
    
    /// <summary>
    /// Check if codepoint belongs to a Brahmic script for LB28a rules.
    /// Per UAX #14, these are: Balinese, Batak, Buginese, Javanese, Kayah Li, Makasar,
    /// Mandaic, Modi, Nandinagari, Sundanese, Tai Le, New Tai Lue, Takri, Tibetan.
    /// </summary>
    bool IsBrahmicForLB28a(int codePoint);
    
    /// <summary>
   /// Get Grapheme_Cluster_Break property (UAX #29)
    /// </summary>
    GraphemeClusterBreak GetGraphemeClusterBreak(int codePoint);
    
    /// <summary>
    /// Get Indic_Conjunct_Break property for GB9c rule (UAX #29).
    /// Loaded from DerivedCoreProperties.txt.
    /// </summary>
    IndicConjunctBreak GetIndicConjunctBreak(int codePoint);
    
    /// <summary>
    /// Get Script_Extensions property (UAX #24).
    /// Returns array of scripts that can use this character.
    /// If character is not in ScriptExtensions.txt, returns array with single Script value.
    /// </summary>
    UnicodeScript[] GetScriptExtensions(int codePoint);
    
    /// <summary>
    /// Check if character has specified script in its Script_Extensions.
    /// </summary>
    bool HasScriptExtension(int codePoint, UnicodeScript script);
}