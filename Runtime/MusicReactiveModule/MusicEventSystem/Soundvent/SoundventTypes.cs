using System.Collections.Generic;

public static class SoundventTypes
{
    public const string ShortIII = nameof(ShortIII);
    public const string ShortII = nameof(ShortII);
    public const string ShortI = nameof(ShortI);

    public const string LongII = nameof(LongII);
    public const string LongI = nameof(LongI);

    public const string EnemyIV = nameof(EnemyIV);
    public const string EnemyIII = nameof(EnemyIII);
    public const string EnemyII = nameof(EnemyII);
    public const string EnemyI = nameof(EnemyI);
    public const string Boss = nameof(Boss);

    public static Dictionary<string, (bool isShort, string group)> GroupByName = new()
    {
        {ShortIII, (true, "Sounds")},
        {ShortII, (true, "Sounds")},
        {ShortI, (true, "Sounds")},
        
        {LongII, (false, "Sounds")},
        {LongI, (false, "Sounds")},
        
        {EnemyIV, (true, "Enemies")},
        {EnemyIII, (true, "Enemies")},
        {EnemyII, (true, "Enemies")},
        {EnemyI, (true, "Enemies")},
        {Boss, (true, "Enemies")},
    };

    public static string[] Sounds { get; } =
    {
        ShortIII,
        ShortII,
        ShortI,

        LongII,
        LongI,
    };

    public static string[] Enemies { get; } =
    {
        EnemyIV,
        EnemyIII,
        EnemyII,
        EnemyI,
        Boss
    };
}
