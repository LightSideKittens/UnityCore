# UniText — Project State

## Architecture (TMP-Free)

**7-этапный pipeline с unified buffers:**

```
TextProcessor (координатор)
    │
    ├── RichTextParser          ← парсинг rich text
    ├── BidiEngine              ← BiDi анализ (UAX #9)
    ├── ScriptAnalyzer          ← определение скриптов (UAX #24)
    ├── [inline itemization]    ← разбиение на runs
    ├── IShapingEngine          ← shaping (единственный интерфейс)
    │       ├── UniTextShapingEngine  ← базовый shaping
    │       └── HarfBuzzShapingEngine ← OpenType shaping
    ├── LineBreaker             ← перенос строк (UAX #14)
    └── TextLayout              ← позиционирование

UniTextFontProvider             ← работа с UniTextFontAsset
UniTextMeshGenerator            ← генерация mesh для рендеринга
```

## Pipeline Status

| Component | Implementation | Status |
|-----------|----------------|--------|
| **Parser** | `RichTextParser` | Done |
| **BiDi Engine** | `BidiEngine` | Done (UAX #9) |
| **Script Analyzer** | `ScriptAnalyzer` | Done (UAX #24) |
| **Itemizer** | inline in `TextProcessor` | Done |
| **Shaping** | `UniTextShapingEngine` | Done |
| **Line Breaking** | `LineBreaker` | Done (UAX #14) |
| **Layout** | `TextLayout` | Done |
| **Coordinator** | `TextProcessor` | Done (unified buffers) |

## Interfaces

Единственный интерфейс — `IShapingEngine` — для подключения HarfBuzz:

```csharp
public interface IShapingEngine
{
    ShapingResult Shape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction);
}
```

## Font System (TMP-Free)

| Component | Status | Notes |
|-----------|--------|-------|
| `UniTextFontAsset` | Done | ScriptableObject с SDF атласом, использует Unity TextCore |
| `UniTextFontProvider` | Done | Font lookup, fallback, метрики |
| `UniTextFontAssetEditor` | Done | Custom Editor с preview атласа |

### UniTextFontAsset

```csharp
// Полностью независим от TMP!
// Использует Unity TextCore для генерации SDF атласа.

[CreateAssetMenu(menuName = "UniText/Font Asset")]
public class UniTextFontAsset : ScriptableObject
{
    #if UNITY_EDITOR
    [SerializeField] private Font sourceFont;  // Для генерации
    [SerializeField] private int atlasSize = 1024;
    [SerializeField] private int atlasPadding = 5;
    [SerializeField] private int samplingPointSize = 90;
    [SerializeField] private GlyphRenderMode renderMode = GlyphRenderMode.SDFAA;
    #endif

    // Runtime данные (сгенерированные):
    [SerializeField] private byte[] fontData;      // Для HarfBuzz
    [SerializeField] private Texture2D atlasTexture;
    [SerializeField] private Material material;
    [SerializeField] private FaceInfo faceInfo;
    [SerializeField] private List<Glyph> glyphTable;
    [SerializeField] private List<UniTextCharacter> characterTable;
}
```

## Shaders (TMP-Free)

Собственные шейдеры в папке `Shaders/Shaders/`:

| Shader | Description |
|--------|-------------|
| `UniText/Distance Field` | Основной SDF шейдер |
| `UniText/Mobile/Distance Field` | Мобильный SDF шейдер |
| `UniText/Bitmap` | Bitmap шейдер |
| `UniText/Mobile/Bitmap` | Мобильный bitmap шейдер |
| `TMPro_Properties.cginc` | Uniform переменные (внутренний) |
| `TMPro.cginc` | Вспомогательные функции (внутренний) |
| `TMPro_Mobile.cginc` | Мобильный код (внутренний) |
| `TMPro_Surface.cginc` | Surface shader код (внутренний) |
| `SDFFunctions.hlsl` | Функции для Shader Graph |

## HarfBuzz Integration

| Component | Status | Notes |
|-----------|--------|-------|
| `HarfBuzzShapingEngine` | Done | IShapingEngine implementation |
| Native Libraries | Pending | Требуется добавить libHarfBuzzSharp |

### Установка HarfBuzz
См. `Core/HarfBuzz/README.md`

## Performance Optimizations

| Optimization | Location | Description |
|--------------|----------|-------------|
| **Shared Text Buffers** | `SharedTextBuffers` | Статические буферы для TextProcessor |
| **Shared Font Cache** | `SharedFontCache` | Статический кеш font lookup (256 entries) |
| **Shared Mesh Pool** | `SharedMeshPool` | Статический пул Mesh объектов |
| **Unicode Property Cache** | `UniTextShapingEngine` | Кеширование Unicode properties |
| **Buffer Pre-allocation** | `UniTextMeshGenerator` | Pre-allocate vertex/triangle lists |
| **Loop Unrolling** | `TextLayout` | 4x unrolling для LTR glyph positioning |
| **AggressiveInlining** | Throughout | На hot paths |
| **Local Caching** | Throughout | Кеширование в локальные переменные |

## Shared Static Resources

```csharp
// SharedPools.cs содержит:

SharedTextBuffers    // Буферы TextProcessor (codepoints, runs, glyphs, etc.)
SharedFontCache      // Кеш поиска fallback шрифтов
SharedMeshPool       // Пул Mesh объектов
```

## Unicode Data (Format V8)

| Property | Method | Status |
|----------|--------|--------|
| BidiClass | `GetBidiClass()` | Done |
| Script | `GetScript()` | Done |
| LineBreakClass | `GetLineBreakClass()` | Done |
| GeneralCategory | `GetGeneralCategory()` | Done |
| EastAsianWidth | `GetEastAsianWidth()` | Done |
| GraphemeClusterBreak | `GetGraphemeClusterBreak()` | Done |
| Default_Ignorable | `IsDefaultIgnorable()` | Done |

## Tag System

Встроенные теги в `TagRegistry.CreateDefault()`:
- **Render**: `<color>`, `<alpha>`, `<u>`, `<s>`, `<mark>`, `<link>`
- **Shaping**: `<b>`, `<i>`, `<size>`, `<font>`, `<cspace>`, `<sup>`, `<sub>`
- **Layout**: `<align>`, `<indent>`, `<line-height>`, `<nobr>`
- **Self-closing**: `<br>`, `<nbsp>`, `<zwsp>`, `<shy>`, `<page>`
- **Special**: `<noparse>`

## Usage Example

```csharp
// Создание font asset в Editor:
// 1. Assets → Create → UniText → Font Asset
// 2. Назначить TTF/OTF шрифт
// 3. Нажать "Generate Font Atlas"

// Runtime использование:
var fontProvider = new UniTextFontProvider(uniTextFontAsset);
var processor = new TextProcessor();
processor.SetFontProvider(fontProvider);

var settings = new TextProcessSettings
{
    maxWidth = 300f,
    fontSize = 36f,
    baseDirection = TextDirection.LeftToRight,
    enableRichText = true,
    enableWordWrap = true
};

var glyphs = processor.Process("<color=red>Hello</color> World! مرحبا", settings);

var meshGenerator = new UniTextMeshGenerator(fontProvider);
meshGenerator.FontSize = 36f;
var meshPairs = meshGenerator.GenerateMeshes(glyphs);
```

## File Structure

```
Core/
├── IPipeline.cs              # IShapingEngine, ShapingResult, GlyphMetrics
├── TextStructures.cs         # TextDirection, TextRange, TextRun, etc.
├── TextAttributes.cs         # ColorAttribute, SizeAttribute, etc.
├── TextProcessor.cs          # Main coordinator (uses SharedTextBuffers)
├── TagSystem.cs              # TagRegistry, TagContext
├── RichTextParser.cs         # Rich text parsing
├── UniTextFontAsset.cs       # Font asset (TMP-free, uses TextCore)
├── UniTextFontProvider.cs    # Font provider (TMP-free)
├── UniTextShapingEngine.cs   # IShapingEngine implementation
├── UniTextMeshGenerator.cs   # Mesh generation
├── TextLayout.cs             # Glyph positioning
├── UniText.cs                # Unity UI component
├── UniTextSubMesh.cs         # Sub-mesh for fallback fonts
├── SharedPools.cs            # SharedTextBuffers, SharedFontCache, SharedMeshPool
├── UnicodeData.cs            # Static singleton for Unicode data
├── UniTextSettings.cs        # ScriptableObject for global settings
└── HarfBuzz/
    ├── HarfBuzzShapingEngine.cs  # OpenType shaping
    └── README.md

Editor/
├── UniTextFontAssetEditor.cs    # Custom editor for font asset

Shaders/Shaders/
├── UniText_SDF.shader           # Main SDF shader
├── UniText_SDF-Mobile.shader    # Mobile SDF shader
├── UniText_Bitmap.shader        # Bitmap shader
├── UniText_Bitmap-Mobile.shader # Mobile bitmap shader
├── TMPro_Properties.cginc       # Shader properties (internal)
├── TMPro.cginc                  # Helper functions (internal)
├── TMPro_Mobile.cginc           # Mobile code (internal)
├── TMPro_Surface.cginc          # Surface shader code (internal)
└── SDFFunctions.hlsl            # Shader Graph functions

Unicode/
├── Analysis/
│   ├── ScriptAnalyzer.cs        # UAX #24
│   └── Itemizer.cs              # Run itemization
├── Layout/
│   ├── LineBreaker.cs           # Line breaking
│   └── LineBreakAlgorithm.cs    # UAX #14
├── BidiEngine.cs                # UAX #9
├── BinaryUnicodeDataProvider.cs
└── UnicodeDataTypes.cs
```

## Migration from TMP

| Old (TMP) | New (UniText) |
|-----------|---------------|
| `TMP_FontAsset` | `UniTextFontAsset` |
| `TMPFontProvider` | `UniTextFontProvider` |
| `TMPShapingEngine` | `UniTextShapingEngine` |
| `TMPMeshGenerator` | `UniTextMeshGenerator` |
| `TMPGlyphRenderInfo` | `UniTextGlyphRenderInfo` |
| `TextMeshPro/...` shader | `UniText/...` shader |

## TODO

### Критически важно (без этого проект не считается завершённым)
1. **HarfBuzz интеграция** — добавить HarfBuzzSharp native библиотеки
2. **Применение атрибутов** — ColorAttribute → цвет глифа в рендеринге

### Важно
3. **Emoji система** — нативные платформенные эмодзи
4. **Text effects** — outline, shadow, gradient
5. **Text input** — caret, selection, IME
6. **Kerning support**

### Низкий приоритет
7. **Hit testing** — улучшенный алгоритм
8. **Animation support** — per-character transforms
