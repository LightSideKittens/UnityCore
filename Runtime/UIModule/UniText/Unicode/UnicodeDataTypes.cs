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


public enum GeneralCategory : byte
{
    Lu,
    Ll,
    Lt,
    Lm,
    Lo,

    Mn,
    Mc,
    Me,

    Nd,
    Nl,
    No,

    Pc,
    Pd,
    Ps,
    Pe,
    Pi,
    Pf,
    Po,

    Sm,
    Sc,
    Sk,
    So,

    Zs,
    Zl,
    Zp,

    Cc,
    Cf,
    Cs,
    Co,
    Cn
}


public enum EastAsianWidth : byte
{
    N,
    A,
    H,
    W,
    F,
    Na
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


public enum UnicodeScript : byte
{
    Unknown = 0,
    Common,
    Inherited,

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

    Garay,
    GurungKhema,
    KiratRai,
    OlOnal,
    Sunuwar,
    Todhri,
    TuluTigalari,

    BeriaErfe,
    Sidetic,
    TaiYo,
    TolongSiki
}


public enum LineBreakClass : byte
{
    Unknown = 0,

    BK,
    CR,
    LF,
    CM,
    NL,
    SG,
    WJ,
    ZW,
    GL,
    SP,
    ZWJ,

    B2,
    BA,
    BB,
    HY,
    CB,

    CL,
    CP,
    EX,
    IN,
    NS,
    OP,
    QU,

    IS,
    NU,
    PO,
    PR,
    SY,

    AI,
    AL,
    CJ,
    EB,
    EM,
    H2,
    H3,
    HL,
    ID,
    JL,
    JV,
    JT,
    RI,
    SA,
    XX,

    AK,
    AP,
    AS,
    VF,
    VI,

    HH
}


public enum GraphemeClusterBreak : byte
{
    Other = 0,
    CR,
    LF,
    Control,
    Extend,
    ZWJ,
    Regional_Indicator,
    Prepend,
    SpacingMark,
    L,
    V,
    T,
    LV,
    LVT
}


public enum IndicConjunctBreak : byte
{
    None = 0,
    Linker,
    Consonant,
    Extend
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


    UnicodeScript GetScript(int codePoint);


    LineBreakClass GetLineBreakClass(int codePoint);


    bool IsExtendedPictographic(int codePoint);


    GeneralCategory GetGeneralCategory(int codePoint);


    EastAsianWidth GetEastAsianWidth(int codePoint);


    bool IsUnambiguousHyphen(int codePoint);


    bool IsDottedCircle(int codePoint);


    bool IsBrahmicForLB28a(int codePoint);


    GraphemeClusterBreak GetGraphemeClusterBreak(int codePoint);


    IndicConjunctBreak GetIndicConjunctBreak(int codePoint);


    UnicodeScript[] GetScriptExtensions(int codePoint);


    bool HasScriptExtension(int codePoint, UnicodeScript script);


    bool IsDefaultIgnorable(int codePoint);
}