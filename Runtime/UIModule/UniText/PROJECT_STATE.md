# UniText — Project State

## Architecture (Refactored)

**7-этапный pipeline с unified buffers:**

```
TextProcessor (координатор)
    │
    ├── RichTextParser          ← парсинг rich text
    ├── BidiEngine              ← BiDi анализ (UAX #9)
    ├── ScriptAnalyzer          ← определение скриптов (UAX #24)
    ├── [inline itemization]    ← разбиение на runs
    ├── IShapingEngine          ← shaping (единственный интерфейс)
    │       └── TMPShapingEngine
    ├── LineBreaker             ← перенос строк (UAX #14)
    └── TextLayout              ← позиционирование

TMPFontProvider                 ← работа с TMP_FontAsset напрямую
TMPMeshGenerator                ← генерация mesh для рендеринга
```

## Pipeline Status

| Component | Implementation | Status |
|-----------|----------------|--------|
| **Parser** | `RichTextParser` | Done |
| **BiDi Engine** | `BidiEngine` | Done (UAX #9) |
| **Script Analyzer** | `ScriptAnalyzer` | Done (UAX #24) |
| **Itemizer** | inline in `TextProcessor` | Done |
| **Shaping** | `TMPShapingEngine` | Done |
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
        TMPFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction);
}
```

## TMP Integration

| Component | Status | Notes |
|-----------|--------|-------|
| `TMPFontProvider` | Done | Работает с TMP_FontAsset напрямую, font fallback |
| `TMPShapingEngine` | Done | Получает метрики из TMP |
| `TMPMeshGenerator` | Done | Генерация mesh с поддержкой fallback шрифтов |
| `UniText` | Done | Unity UI компонент |

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
// Создание processor
var unicodeData = new BinaryUnicodeDataProvider(unicodeDataBytes);
var processor = new TextProcessor(unicodeData);

// Настройка font provider
var fontProvider = new TMPFontProvider(tmpFont);
processor.SetFontProvider(fontProvider);

// Обработка текста
var settings = new TextProcessSettings
{
    maxWidth = 300f,
    fontSize = 36f,
    baseDirection = TextDirection.LeftToRight,
    enableRichText = true,
    enableWordWrap = true
};

var glyphs = processor.Process("<color=red>Hello</color> World! مرحبا", settings);

// Генерация mesh
var meshGenerator = new TMPMeshGenerator(fontProvider, unicodeData);
meshGenerator.FontSize = 36f;
var meshPairs = meshGenerator.GenerateMeshes(glyphs);
```

## File Structure

```
Core/
├── IPipeline.cs           # IShapingEngine, ShapingResult, GlyphMetrics
├── TextStructures.cs      # TextDirection, TextRange, TextRun, etc.
├── TextAttributes.cs      # ColorAttribute, SizeAttribute, etc.
├── TextProcessor.cs       # Main coordinator (unified buffers)
├── TagSystem.cs           # TagRegistry, TagContext
├── RichTextParser.cs      # Rich text parsing
├── TMPFontAdapter.cs      # TMPFontProvider, TMPGlyphRenderInfo
├── TMPShapingEngine.cs    # IShapingEngine implementation
├── TMPMeshGenerator.cs    # Mesh generation
├── TextLayout.cs          # Glyph positioning
├── UniText.cs             # Unity UI component
└── UniTextSubMesh.cs      # Sub-mesh for fallback fonts

Unicode/
├── Analysis/
│   ├── ScriptAnalyzer.cs      # UAX #24
│   └── Itemizer.cs            # Run itemization
├── Layout/
│   ├── LineBreaker.cs         # Line breaking
│   └── LineBreakAlgorithm.cs  # UAX #14
├── BidiEngine.cs              # UAX #9
├── BinaryUnicodeDataProvider.cs
└── UnicodeDataTypes.cs
```

## Removed (After Refactoring)

- ❌ `ITextParser`, `IParseResult`
- ❌ `IBidiAnalyzer`, `IBidiResult`
- ❌ `IScriptAnalyzer`, `IScriptResult`
- ❌ `IItemizer`, `IItemizeResult`
- ❌ `ITextShaper`, `IShapeResult`
- ❌ `ILineBreaker`, `ILineBreakResult`
- ❌ `ITextLayout`, `ILayoutResult`
- ❌ `IFontProvider`, `IFontAsset`
- ❌ `ITextAttribute`, `IRenderAttribute`, `IShapingAttribute`, `ILayoutAttribute`
- ❌ `IRenderer.cs` (неиспользуемый)
- ❌ `BidiAnalyzer` (обёртка над BidiEngine)
- ❌ `TextShaper` (обёртка над IShapingEngine)
- ❌ `MockShapingEngine`, `MockFontProvider`, `MockFontAsset`
- ❌ Все `*ResultBuffer` классы

## TODO

1. **HarfBuzz integration** — для сложных скриптов (Arabic, Devanagari)
2. **Применение атрибутов** — ColorAttribute → цвет глифа в рендеринге
3. **Caret/Selection** — позиционирование курсора
4. **Hit testing** — улучшенный алгоритм
5. **Text effects** — outline, shadow, gradient
6. **Animation support** — per-character transforms
