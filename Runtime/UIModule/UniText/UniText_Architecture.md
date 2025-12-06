# UniText — Unicode-Compliant Text Engine for Unity

## Полная спецификация архитектуры

**Версия:** 1.0  
**Дата:** Декабрь 2024  
**Статус:** Спецификация для реализации

---

## Содержание

1. [Обзор и цели](#1-обзор-и-цели)
2. [Архитектура высокого уровня](#2-архитектура-высокого-уровня)
3. [Core Pipeline](#3-core-pipeline)
4. [Parsing & Attributes](#4-parsing--attributes)
5. [Unicode Analysis](#5-unicode-analysis)
6. [Text Shaping (HarfBuzz)](#6-text-shaping-harfbuzz)
7. [Line Breaking & Layout](#7-line-breaking--layout)
8. [Font System](#8-font-system)
9. [Hit Testing & Interaction](#9-hit-testing--interaction)
10. [Caching System](#10-caching-system)
11. [Rendering Backend](#11-rendering-backend)
12. [Platform Integration](#12-platform-integration)
13. [API Reference](#13-api-reference)
14. [Implementation Plan](#14-implementation-plan)
15. [Testing Strategy](#15-testing-strategy)

---

## 1. Обзор и цели

### 1.1 Миссия

Создать промышленный текстовый движок для Unity, соответствующий всем стандартам Unicode и лучшим практикам индустрии (Pango, DirectWrite, Core Text).

### 1.2 Ключевые требования

| Требование | Описание | Приоритет |
|------------|----------|-----------|
| Unicode Compliance | Полное соответствие UAX #9, #14, #24, #29 | P0 |
| Complex Scripts | Arabic, Hebrew, Indic, Thai, CJK | P0 |
| BiDi Support | Bidirectional text rendering | P0 |
| HarfBuzz Integration | Professional text shaping | P0 |
| Performance | < 5ms initial layout, < 1ms incremental | P0 |
| Extensibility | Plugin architecture for tags, renderers | P1 |
| Vertical Text | CJK vertical writing modes | P1 |
| Color Fonts | Emoji (COLR/CPAL) | P1 |
| Variable Fonts | OpenType variations | P2 |
| Accessibility | Screen reader support | P2 |

### 1.3 Не-цели (Out of Scope)

- Полноценный текстовый редактор (только базовое редактирование)
- Таблицы и сложная вёрстка (отдельный модуль)
- PDF/Print rendering

### 1.4 Сравнение с индустриальными решениями

```
┌────────────────┬──────────┬────────────┬───────────┬──────────┐
│ Feature        │ UniText    │ DirectWrite│ Core Text │ Pango    │
├────────────────┼──────────┼────────────┼───────────┼──────────┤
│ BiDi           │ ✓        │ ✓          │ ✓         │ ✓        │
│ Complex Shapes │ HarfBuzz │ Uniscribe  │ AAT+OT    │ HarfBuzz │
│ Line Breaking  │ UAX #14  │ Custom     │ Custom    │ UAX #14  │
│ Vertical       │ ✓        │ ✓          │ ✓         │ ✓        │
│ Color Fonts    │ ✓        │ ✓          │ ✓         │ Limited  │
│ Variable Fonts │ ✓        │ ✓          │ ✓         │ ✓        │
│ Hit Testing    │ ✓        │ ✓          │ ✓         │ ✓        │
│ Inline Objects │ ✓        │ ✓          │ ✓         │ ✓        │
└────────────────┴──────────┴────────────┴───────────┴──────────┘
```

---

## 2. Архитектура высокого уровня

### 2.1 Слои системы

```
┌─────────────────────────────────────────────────────────────────────┐
│                        APPLICATION LAYER                             │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │  UniTextText (MonoBehaviour)                                   │    │
│  │  - Unity component for UGUI                                  │    │
│  │  - Handles Unity lifecycle                                   │    │
│  └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        HIGH-LEVEL API                                │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │  TextBlock                                                   │    │
│  │  - Main entry point for text processing                      │    │
│  │  - Manages text, attributes, layout                          │    │
│  │  - Provides hit testing, selection                           │    │
│  └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       PROCESSING PIPELINE                            │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐  │
│  │ Parsing  │→│ Analysis │→│Itemizing │→│ Shaping  │→│ Layout   │  │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        SUBSYSTEMS                                    │
│  ┌───────────────┐  ┌───────────────┐  ┌───────────────────────┐   │
│  │ Font System   │  │ Cache System  │  │ Unicode Data Provider │   │
│  └───────────────┘  └───────────────┘  └───────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       RENDERING BACKEND                              │
│  ┌───────────────┐  ┌───────────────┐  ┌───────────────────────┐   │
│  │ Mesh Builder  │  │ Glyph Atlas   │  │ Material Manager      │   │
│  └───────────────┘  └───────────────┘  └───────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       PLATFORM LAYER                                 │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐  │
│  │ Windows  │ │  macOS   │ │  Linux   │ │ Android  │ │   iOS    │  │
│  │ HarfBuzz │ │CoreText/ │ │ HarfBuzz │ │ HarfBuzz │ │CoreText  │  │
│  │          │ │ HarfBuzz │ │          │ │ (system) │ │          │  │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### 2.2 Namespace Structure

```
UniText
├── UniText.Core                    // Core interfaces and structures
│   ├── ITextProcessor
│   ├── TextBlock
│   ├── TextRange
│   └── ...
├── UniText.Parsing                 // Text parsing
│   ├── ITextParser
│   ├── RichTextParser
│   ├── TagRegistry
│   └── ...
├── UniText.Attributes              // Text attributes
│   ├── ITextAttribute
│   ├── AttributeStore
│   ├── FontAttribute
│   └── ...
├── UniText.Unicode                 // Unicode algorithms
│   ├── BidiEngine
│   ├── ScriptAnalyzer
│   ├── LineBreakAlgorithm
│   ├── GraphemeBreaker
│   └── ...
├── UniText.Shaping                 // Text shaping
│   ├── ITextShaper
│   ├── HarfBuzzShaper
│   ├── ShapedBuffer
│   └── ...
├── UniText.Layout                  // Text layout
│   ├── ITextLayout
│   ├── LineBreaker
│   ├── ParagraphLayout
│   └── ...
├── UniText.Fonts                   // Font management
│   ├── IFontProvider
│   ├── FontFallbackChain
│   ├── FontAsset
│   └── ...
├── UniText.Interaction             // Hit testing, selection
│   ├── IHitTestable
│   ├── HitTestResult
│   ├── SelectionManager
│   └── ...
├── UniText.Caching                 // Caching subsystem
│   ├── GlyphCache
│   ├── LayoutCache
│   ├── ShapingCache
│   └── ...
├── UniText.Rendering               // Rendering
│   ├── ITextRenderer
│   ├── MeshBuilder
│   ├── GlyphAtlas
│   └── ...
└── UniText.Platform                // Platform-specific code
    ├── Windows
    ├── macOS
    ├── Linux
    ├── Android
    ├── iOS
    └── WebGL
```

### 2.3 Dependency Graph

```
                    ┌──────────────────┐
                    │  UniText.Core      │
                    └────────┬─────────┘
                             │
           ┌─────────────────┼─────────────────┐
           │                 │                 │
           ▼                 ▼                 ▼
    ┌─────────────┐  ┌─────────────┐  ┌─────────────┐
    │UniText.Unicode│  │UniText.Parsing│  │UniText.Attrib │
    └──────┬──────┘  └──────┬──────┘  └──────┬──────┘
           │                │                 │
           └────────────────┼─────────────────┘
                            │
                            ▼
                    ┌─────────────────┐
                    │ UniText.Shaping   │◄────┐
                    └────────┬────────┘     │
                             │              │
                             ▼              │
                    ┌─────────────────┐     │
                    │ UniText.Fonts     │─────┘
                    └────────┬────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │ UniText.Layout    │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
              ▼              ▼              ▼
    ┌──────────────┐ ┌─────────────┐ ┌─────────────┐
    │UniText.Interact│ │UniText.Caching│ │UniText.Render │
    └──────────────┘ └─────────────┘ └──────────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │ UniText.Platform  │
                    └─────────────────┘
```

---

## 3. Core Pipeline

### 3.1 Pipeline Overview

```
INPUT: "Hello <b>مرحبا</b> World!"
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STAGE 1: PARSING                                                     │
│ ─────────────────                                                    │
│ Input:  Rich text string                                            │
│ Output: Plain text (codepoints) + Attributes                        │
│                                                                      │
│ "Hello مرحبا World!" + [Bold: range(6,10)]                          │
└─────────────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STAGE 2: UNICODE ANALYSIS                                            │
│ ────────────────────────                                             │
│ 2a. BiDi Analysis (UAX #9)                                          │
│     → Embedding levels: [0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0]        │
│                                                                      │
│ 2b. Script Detection (UAX #24)                                      │
│     → Scripts: [Latn,Latn,Latn,Latn,Latn,Latn,Arab,Arab,Arab,       │
│                 Arab,Arab,Latn,Latn,Latn,Latn,Latn,Latn]            │
│                                                                      │
│ 2c. Grapheme Clustering (UAX #29)                                   │
│     → Grapheme boundaries for cursor positioning                     │
│                                                                      │
│ 2d. Line Break Analysis (UAX #14)                                   │
│     → Break opportunities: [0,0,0,0,0,1,0,0,0,0,0,1,0,0,0,0,0,1]   │
└─────────────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STAGE 3: ITEMIZATION                                                 │
│ ───────────────────                                                  │
│ Split into runs based on:                                           │
│ - BiDi level changes                                                │
│ - Script changes                                                     │
│ - Font changes (from attributes or fallback)                        │
│ - Style changes (bold, italic, size)                                │
│                                                                      │
│ Runs:                                                                │
│ [0] "Hello " - Level:0, Script:Latn, Font:Default                   │
│ [1] "مرحبا"  - Level:1, Script:Arab, Font:Default, Bold             │
│ [2] " World!" - Level:0, Script:Latn, Font:Default                  │
└─────────────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STAGE 4: SHAPING (HarfBuzz)                                          │
│ ──────────────────────────                                           │
│ For each run:                                                        │
│ - Select font (with fallback)                                       │
│ - Apply OpenType features                                           │
│ - Get glyph IDs and positions                                       │
│                                                                      │
│ Run[1] "مرحبا" → Glyphs: [gid:245, gid:312, gid:287, gid:198]       │
│                  Positions: [(0,0), (12,0), (24,0), (36,0)]         │
│                  (with Arabic shaping: init, medi, fina forms)      │
└─────────────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STAGE 5: LINE BREAKING                                               │
│ ─────────────────────                                                │
│ - Apply UAX #14 break opportunities                                 │
│ - Fit runs into lines based on maxWidth                             │
│ - Handle hyphenation (optional)                                     │
│                                                                      │
│ Lines:                                                               │
│ [0] Runs: [0, 1, 2], Width: 145px                                   │
└─────────────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STAGE 6: BIDI REORDERING                                             │
│ ────────────────────────                                             │
│ Reorder runs within each line for visual display                    │
│                                                                      │
│ Logical: [Run0: "Hello "] [Run1: "مرحبا"] [Run2: " World!"]         │
│ Visual:  [Run0: "Hello "] [Run1: "ابحرم"] [Run2: " World!"]         │
│          (Run1 glyphs reversed for RTL)                             │
└─────────────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│ STAGE 7: POSITIONING                                                 │
│ ───────────────────                                                  │
│ Calculate final (x, y) for each glyph                               │
│ Apply:                                                               │
│ - Alignment (left, center, right, justify)                          │
│ - Line spacing                                                       │
│ - Baseline alignment                                                 │
│                                                                      │
│ Output: PositionedGlyph[]                                           │
│ [0] gid:72  @ (0, 0)     // 'H'                                     │
│ [1] gid:101 @ (12, 0)    // 'e'                                     │
│ ...                                                                  │
└─────────────────────────────────────────────────────────────────────┘
                    │
                    ▼
OUTPUT: LayoutResult (ready for rendering)
```

### 3.2 Core Data Structures

```csharp
namespace UniText.Core
{
    /// <summary>
    /// Immutable range in text.
    /// </summary>
    public readonly struct TextRange : IEquatable<TextRange>
    {
        public readonly int start;
        public readonly int length;
        
        public int End => start + length;
        public bool IsEmpty => length == 0;
        
        public TextRange(int start, int length)
        {
            this.start = start;
            this.length = length;
        }
        
        public bool Contains(int index) => index >= start && index < End;
        public bool Overlaps(TextRange other) => start < other.End && End > other.start;
        
        public TextRange Intersect(TextRange other)
        {
            int s = Math.Max(start, other.start);
            int e = Math.Min(End, other.End);
            return e > s ? new TextRange(s, e - s) : default;
        }
    }
    
    /// <summary>
    /// A run of text with uniform properties.
    /// </summary>
    public readonly struct TextRun
    {
        public readonly TextRange textRange;
        public readonly byte bidiLevel;
        public readonly UnicodeScript script;
        public readonly int fontId;
        public readonly int attributeSnapshotId;
        
        public TextDirection Direction => (bidiLevel & 1) == 0 
            ? TextDirection.LeftToRight 
            : TextDirection.RightToLeft;
    }
    
    /// <summary>
    /// A shaped glyph with positioning information.
    /// </summary>
    public struct ShapedGlyph
    {
        public int glyphId;
        public int clusterIndex;
        public float advanceX;
        public float advanceY;
        public float offsetX;
        public float offsetY;
        public GlyphFlags flags;
    }
    
    [Flags]
    public enum GlyphFlags : byte
    {
        None = 0,
        Ligature = 1,
        Mark = 2,
        RightToLeft = 4,
        Color = 8,
        Whitespace = 16
    }
    
    /// <summary>
    /// A line of text after line breaking.
    /// </summary>
    public readonly struct TextLine
    {
        public readonly TextRange textRange;
        public readonly int runStart;
        public readonly int runCount;
        public readonly float width;
        public readonly float height;
        public readonly float baseline;
        public readonly float y;
        public readonly int lineIndex;
        public readonly bool isHardBreak;
    }
    
    /// <summary>
    /// A positioned glyph ready for rendering.
    /// </summary>
    public struct PositionedGlyph
    {
        public int glyphId;
        public float x;
        public float y;
        public int fontId;
        public int attributeSnapshotId;
        public int characterIndex;
        public GlyphFlags flags;
    }
    
    /// <summary>
    /// Complete layout result.
    /// </summary>
    public sealed class LayoutResult
    {
        public ReadOnlyMemory<int> Codepoints { get; }
        public ReadOnlyMemory<PositionedGlyph> Glyphs { get; }
        public ReadOnlyMemory<TextLine> Lines { get; }
        public ReadOnlyMemory<ShapedRun> Runs { get; }
        public float Width { get; }
        public float Height { get; }
        public TextDirection BaseDirection { get; }
        public int LineCount => Lines.Length;
        public bool IsTruncated { get; }
        public IAttributeStore Attributes { get; }
    }
}
```

---

## 4. Text Shaping (HarfBuzz Integration)

### 4.1 Why HarfBuzz is Mandatory

HarfBuzz is the **industry standard** text shaping engine, used by:
- All major browsers (Chrome, Firefox, Edge, Safari)
- Android and ChromeOS
- Qt, GTK+, LibreOffice
- Adobe products
- Game engines (Unreal, Godot)
- PlayStation SDK

**Without HarfBuzz, these scripts CANNOT be rendered correctly:**
- Arabic (contextual forms: initial, medial, final, isolated)
- Hebrew (with vowel marks)
- All Indic scripts (Devanagari, Tamil, Bengali, etc.)
- Thai, Khmer, Myanmar
- Many others

### 4.2 HarfBuzz P/Invoke Interface

```csharp
namespace UniText.Shaping
{
    internal static class HarfBuzzNative
    {
        private const string LibName = "harfbuzz";
        
        // Buffer operations
        [DllImport(LibName)] 
        public static extern IntPtr hb_buffer_create();
        
        [DllImport(LibName)] 
        public static extern void hb_buffer_destroy(IntPtr buffer);
        
        [DllImport(LibName)] 
        public static extern void hb_buffer_clear_contents(IntPtr buffer);
        
        [DllImport(LibName)] 
        public static extern void hb_buffer_set_direction(IntPtr buffer, int direction);
        
        [DllImport(LibName)] 
        public static extern void hb_buffer_set_script(IntPtr buffer, uint script);
        
        [DllImport(LibName)]
        public static extern unsafe void hb_buffer_add_codepoints(
            IntPtr buffer, uint* text, int textLength, uint itemOffset, int itemLength);
        
        [DllImport(LibName)]
        public static extern unsafe HbGlyphInfo* hb_buffer_get_glyph_infos(
            IntPtr buffer, out uint length);
        
        [DllImport(LibName)]
        public static extern unsafe HbGlyphPosition* hb_buffer_get_glyph_positions(
            IntPtr buffer, out uint length);
        
        // Shape function
        [DllImport(LibName)]
        public static extern unsafe void hb_shape(
            IntPtr font, IntPtr buffer, HbFeature* features, uint numFeatures);
        
        // Font operations
        [DllImport(LibName)] 
        public static extern IntPtr hb_font_create(IntPtr face);
        
        [DllImport(LibName)] 
        public static extern void hb_font_destroy(IntPtr font);
        
        // Face operations  
        [DllImport(LibName)]
        public static extern IntPtr hb_face_create(IntPtr blob, uint index);
        
        [DllImport(LibName)]
        public static extern unsafe IntPtr hb_blob_create(
            byte* data, uint length, int memoryMode, IntPtr userData, IntPtr destroy);
        
        // Constants
        public const int HB_DIRECTION_LTR = 4;
        public const int HB_DIRECTION_RTL = 5;
        public const int HB_DIRECTION_TTB = 6;
        public const int HB_DIRECTION_BTT = 7;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct HbGlyphInfo
    {
        public uint codepoint;  // After shaping = glyph ID
        public uint mask;
        public uint cluster;
        public uint var1;
        public uint var2;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct HbGlyphPosition
    {
        public int xAdvance;
        public int yAdvance;
        public int xOffset;
        public int yOffset;
        public uint var;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct HbFeature
    {
        public uint tag;
        public uint value;
        public uint start;
        public uint end;
    }
}
```

### 4.3 HarfBuzz Shaper Implementation

```csharp
namespace UniText.Shaping
{
    public sealed class HarfBuzzShaper : ITextShaper
    {
        private readonly IntPtr _buffer;
        private bool _disposed;
        
        public HarfBuzzShaper()
        {
            _buffer = HarfBuzzNative.hb_buffer_create();
        }
        
        public void Shape(
            ReadOnlySpan<int> codepoints,
            IFontFace font,
            TextDirection direction,
            UnicodeScript script,
            string language,
            ReadOnlySpan<OpenTypeFeature> features,
            IShapedBuffer result)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HarfBuzzShaper));
            
            var hbFont = font.GetHarfBuzzFont();
            
            // Configure buffer
            HarfBuzzNative.hb_buffer_clear_contents(_buffer);
            HarfBuzzNative.hb_buffer_set_direction(_buffer, 
                direction == TextDirection.RightToLeft 
                    ? HarfBuzzNative.HB_DIRECTION_RTL 
                    : HarfBuzzNative.HB_DIRECTION_LTR);
            HarfBuzzNative.hb_buffer_set_script(_buffer, ScriptToTag(script));
            
            // Add codepoints
            unsafe
            {
                fixed (int* ptr = codepoints)
                {
                    HarfBuzzNative.hb_buffer_add_codepoints(
                        _buffer, (uint*)ptr, codepoints.length, 0, codepoints.length);
                }
            }
            
            // Shape
            unsafe
            {
                Span<HbFeature> hbFeatures = stackalloc HbFeature[features.length];
                for (int i = 0; i < features.length; i++)
                {
                    hbFeatures[i] = new HbFeature
                    {
                        tag = features[i].Tag,
                        value = features[i].Value,
                        start = 0,
                        end = uint.MaxValue
                    };
                }
                
                fixed (HbFeature* featPtr = hbFeatures)
                {
                    HarfBuzzNative.hb_shape(hbFont, _buffer, featPtr, (uint)features.length);
                }
            }
            
            // Extract results
            ExtractGlyphs(result, font);
        }
        
        private void ExtractGlyphs(IShapedBuffer result, IFontFace font)
        {
            uint glyphCount;
            var infos = HarfBuzzNative.hb_buffer_get_glyph_infos(_buffer, out glyphCount);
            var positions = HarfBuzzNative.hb_buffer_get_glyph_positions(_buffer, out _);
            
            float scale = font.Size / font.UnitsPerEm;
            result.Clear();
            
            unsafe
            {
                for (uint i = 0; i < glyphCount; i++)
                {
                    result.Add(new ShapedGlyph
                    {
                        glyphId = (int)infos[i].codepoint,
                        clusterIndex = (int)infos[i].cluster,
                        advanceX = positions[i].xAdvance * scale,
                        advanceY = positions[i].yAdvance * scale,
                        offsetX = positions[i].xOffset * scale,
                        offsetY = positions[i].yOffset * scale
                    });
                }
            }
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                HarfBuzzNative.hb_buffer_destroy(_buffer);
                _disposed = true;
            }
        }
        
        private static uint ScriptToTag(UnicodeScript script) => script switch
        {
            UnicodeScript.Latin => 0x4C61746E,    // 'Latn'
            UnicodeScript.Arabic => 0x41726162,   // 'Arab'
            UnicodeScript.Hebrew => 0x48656272,   // 'Hebr'
            UnicodeScript.Devanagari => 0x44657661, // 'Deva'
            UnicodeScript.Han => 0x48616E69,      // 'Hani'
            UnicodeScript.Hiragana => 0x48697261, // 'Hira'
            UnicodeScript.Katakana => 0x4B616E61, // 'Kana'
            UnicodeScript.Hangul => 0x48616E67,   // 'Hang'
            _ => 0x5A797979                       // 'Zyyy' (Common)
        };
    }
}
```

---

## 5. Font System

### 5.1 Font Interfaces

```csharp
namespace UniText.Fonts
{
    public interface IFontFace : IDisposable
    {
        string FamilyName { get; }
        FontStyles Style { get; }
        int Weight { get; }
        int UnitsPerEm { get; }
        int Ascender { get; }
        int Descender { get; }
        int LineGap { get; }
        
        bool HasGlyph(int codepoint);
        bool TryGetGlyphIndex(int codepoint, out int glyphIndex);
        bool TryGetGlyphMetrics(int glyphIndex, out GlyphMetrics metrics);
        
        IntPtr GetHarfBuzzFont();
        ReadOnlySpan<byte> GetFontData();
        
        bool IsColorFont { get; }
        bool IsVariableFont { get; }
    }
    
    public interface IFontProvider
    {
        IFont GetFont(int fontId);
        int GetFontId(FontRequest request);
        int FindFontForCodepoint(int codepoint, int preferredFontId);
        int DefaultFontId { get; }
        int RegisterFont(IFontFace face);
    }
    
    public readonly struct FontRequest
    {
        public readonly string familyName;
        public readonly FontStyles style;
        public readonly int weight;
        public readonly float size;
    }
}
```

### 5.2 Font Fallback Chain

```csharp
namespace UniText.Fonts
{
    public sealed class FontFallbackChain
    {
        private readonly List<FallbackRule> _rules = new();
        private readonly IFontProvider _fontProvider;
        
        public int FindFontForCodepoint(int codepoint, int preferredFontId)
        {
            // 1. Try preferred font
            var preferred = _fontProvider.GetFont(preferredFontId);
            if (preferred?.Face.HasGlyph(codepoint) == true)
                return preferredFontId;
            
            // 2. Try fallback rules
            foreach (var rule in _rules)
            {
                if (rule.Matches(codepoint))
                {
                    var fontId = _fontProvider.GetFontId(rule.FontRequest);
                    var font = _fontProvider.GetFont(fontId);
                    if (font?.Face.HasGlyph(codepoint) == true)
                        return fontId;
                }
            }
            
            // 3. Default font
            return _fontProvider.DefaultFontId;
        }
        
        public static FontFallbackChain CreateDefault(IFontProvider provider)
        {
            var chain = new FontFallbackChain(provider);
            
            // Emoji (highest priority)
            chain.AddRule(new FallbackRule
            {
                Priority = 100,
                CodepointRanges = new[] { (0x1F300, 0x1F9FF), (0x2600, 0x26FF) },
                FontRequest = new FontRequest("Noto Color Emoji", 0)
            });
            
            // Arabic
            chain.AddRule(new FallbackRule
            {
                Priority = 50,
                Scripts = new[] { UnicodeScript.Arabic },
                FontRequest = new FontRequest("Noto Sans Arabic", 0)
            });
            
            // Hebrew
            chain.AddRule(new FallbackRule
            {
                Priority = 50,
                Scripts = new[] { UnicodeScript.Hebrew },
                FontRequest = new FontRequest("Noto Sans Hebrew", 0)
            });
            
            // CJK
            chain.AddRule(new FallbackRule
            {
                Priority = 50,
                Scripts = new[] { UnicodeScript.Han },
                FontRequest = new FontRequest("Noto Sans CJK SC", 0)
            });
            
            // Devanagari
            chain.AddRule(new FallbackRule
            {
                Priority = 50,
                Scripts = new[] { UnicodeScript.Devanagari },
                FontRequest = new FontRequest("Noto Sans Devanagari", 0)
            });
            
            // Default fallback
            chain.AddRule(new FallbackRule
            {
                Priority = 0,
                FontRequest = new FontRequest("Noto Sans", 0)
            });
            
            return chain;
        }
    }
}
```

---

## 6. Hit Testing & Interaction

### 6.1 Hit Testing Interface

```csharp
namespace UniText.Interaction
{
    public readonly struct HitTestResult
    {
        public readonly int characterIndex;
        public readonly bool isTrailing;
        public readonly int lineIndex;
        public readonly bool isInside;
        public readonly float distance;
        
        public int CaretPosition => isTrailing ? characterIndex + 1 : characterIndex;
        
        public static HitTestResult Miss => new(-1, false, -1, false, float.MaxValue);
    }
    
    public readonly struct CaretInfo
    {
        public readonly int characterIndex;
        public readonly Rect strongCaret;
        public readonly Rect? weakCaret;  // For BiDi boundaries
        public readonly int lineIndex;
        
        public bool IsBidiBoundary => weakCaret.HasValue;
    }
    
    public interface IHitTestable
    {
        HitTestResult HitTest(Vector2 point);
        CaretInfo GetCaretInfo(int characterIndex);
        Rect GetCharacterRect(int characterIndex);
        void GetRangeRects(TextRange range, IList<Rect> rects);
        int GetLineFromCharacter(int characterIndex);
        int GetLineStart(int lineIndex);
        int GetLineEnd(int lineIndex);
    }
    
    public readonly struct TextSelection
    {
        public readonly int anchor;
        public readonly int focus;
        
        public int Start => Math.Min(anchor, focus);
        public int End => Math.Max(anchor, focus);
        public int Length => End - Start;
        public bool IsEmpty => anchor == focus;
        public TextRange Range => new TextRange(Start, Length);
        
        public static TextSelection Caret(int position) => new(position, position);
    }
    
    public enum NavigationUnit { Character, Grapheme, Word, Line, Paragraph, Document }
    public enum NavigationDirection { Forward, Backward, Up, Down, LineStart, LineEnd }
    
    public interface ITextNavigator
    {
        int Navigate(int position, NavigationDirection direction, NavigationUnit unit);
        TextRange GetWordBoundary(int position);
        TextRange GetLineBoundary(int position);
    }
}
```

---

## 7. Caching System

### 7.1 Multi-Level Cache Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                     CACHE LEVELS                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  Level 1: GLYPH CACHE                                               │
│  ────────────────────                                                │
│  Key: (FontId, GlyphId, Size)                                       │
│  Value: Texture rect, UV coords, SDF data                           │
│  Invalidation: Never (glyphs don't change)                          │
│  Typical size: 2-10 MB                                              │
│                                                                      │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  Level 2: SHAPING CACHE                                             │
│  ──────────────────────                                              │
│  Key: (Text hash, FontId, Features, Script, Direction)              │
│  Value: ShapedGlyph[]                                               │
│  Invalidation: Font change, feature change                          │
│  Hit rate: 80-95% for typical UI text                               │
│                                                                      │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  Level 3: LAYOUT CACHE                                              │
│  ─────────────────────                                               │
│  Key: (Shaped runs hash, MaxWidth, Alignment, etc)                  │
│  Value: TextLine[], positions                                       │
│  Invalidation: Width change, style change                           │
│  Hit rate: 60-80% for static text                                   │
│                                                                      │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  Level 4: MESH CACHE                                                │
│  ──────────────────                                                  │
│  Key: Layout hash                                                   │
│  Value: Unity Mesh                                                  │
│  Invalidation: Any layout change                                    │
│  Hit rate: 90%+ for completely static text                          │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

### 7.2 Cache Implementation

```csharp
namespace UniText.Caching
{
    public interface ICacheSystem
    {
        IGlyphCache GlyphCache { get; }
        IShapingCache ShapingCache { get; }
        ILayoutCache LayoutCache { get; }
        void ClearAll();
        CacheStatistics GetStatistics();
    }
    
    public readonly struct CacheStatistics
    {
        public readonly int glyphCacheHits;
        public readonly int glyphCacheMisses;
        public readonly int shapingCacheHits;
        public readonly int shapingCacheMisses;
        public readonly int layoutCacheHits;
        public readonly int layoutCacheMisses;
        public readonly long totalMemoryBytes;
        
        public float GlyphHitRate => glyphCacheHits / (float)(glyphCacheHits + glyphCacheMisses);
        public float ShapingHitRate => shapingCacheHits / (float)(shapingCacheHits + shapingCacheMisses);
        public float LayoutHitRate => layoutCacheHits / (float)(layoutCacheHits + layoutCacheMisses);
    }
    
    public sealed class LRUCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, LinkedListNode<CacheEntry>> _cache;
        private readonly LinkedList<CacheEntry> _lruList;
        private readonly int _maxSize;
        
        public int Hits { get; private set; }
        public int Misses { get; private set; }
        
        public bool TryGet(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out var node))
            {
                _lruList.Remove(node);
                _lruList.AddFirst(node);
                value = node.value.value;
                Hits++;
                return true;
            }
            
            value = default;
            Misses++;
            return false;
        }
        
        public void Add(TKey key, TValue value)
        {
            if (_cache.TryGetValue(key, out var existing))
            {
                _lruList.Remove(existing);
                _cache.Remove(key);
            }
            
            var node = _lruList.AddFirst(new CacheEntry { key = key, value = value });
            _cache[key] = node;
            
            while (_cache.Count > _maxSize)
            {
                var last = _lruList.Last;
                _cache.Remove(last.value.key);
                _lruList.RemoveLast();
            }
        }
        
        private struct CacheEntry
        {
            public TKey key;
            public TValue value;
        }
    }
}
```

---

## 8. High-Level API

### 8.1 TextBlock

```csharp
namespace UniText
{
    public sealed class TextBlock : IDisposable, IHitTestable, ITextNavigator
    {
        private string _text;
        private readonly AttributeStore _attributes;
        private readonly ITextProcessor _processor;
        private LayoutResult _cachedLayout;
        private bool _layoutDirty = true;
        
        // Properties
        public string Text
        {
            get => _text;
            set { _text = value; InvalidateLayout(); }
        }
        
        public float MaxWidth { get; set; } = float.MaxValue;
        public float MaxHeight { get; set; } = float.MaxValue;
        public TextDirection BaseDirection { get; set; } = TextDirection.Auto;
        public TextAlignment Alignment { get; set; } = TextAlignment.Leading;
        public TextWrapping Wrapping { get; set; } = TextWrapping.Wrap;
        public WritingMode WritingMode { get; set; } = WritingMode.HorizontalTB;
        public bool EnableRichText { get; set; } = true;
        
        // Font settings
        public IFontProvider FontProvider { get; set; }
        public int DefaultFontId { get; set; }
        public float DefaultFontSize { get; set; } = 16f;
        
        // Computed properties (lazy)
        public float Width => GetLayout().Width;
        public float Height => GetLayout().Height;
        public int LineCount => GetLayout().LineCount;
        
        public TextBlock(ITextProcessor processor = null)
        {
            _processor = processor ?? TextProcessor.CreateDefault();
            _attributes = new AttributeStore();
        }
        
        public void SetAttribute<T>(TextRange range, T attribute) where T : ITextAttribute
        {
            _attributes.Add(attribute);
            
            // Smart invalidation
            if (attribute is not IRenderAttribute)
                InvalidateLayout();
        }
        
        public LayoutResult GetLayout()
        {
            if (_layoutDirty)
            {
                _cachedLayout = _processor.Process(_text.AsSpan(), new TextProcessOptions
                {
                    MaxWidth = MaxWidth,
                    MaxHeight = MaxHeight,
                    BaseDirection = BaseDirection,
                    Alignment = Alignment,
                    Wrapping = Wrapping,
                    FontProvider = FontProvider,
                    EnableRichText = EnableRichText,
                    WritingMode = WritingMode
                });
                _layoutDirty = false;
            }
            return _cachedLayout;
        }
        
        public void InvalidateLayout() => _layoutDirty = true;
        
        // IHitTestable implementation
        public HitTestResult HitTest(Vector2 point) { /* ... */ }
        public CaretInfo GetCaretInfo(int index) { /* ... */ }
        public Rect GetCharacterRect(int index) { /* ... */ }
        public void GetRangeRects(TextRange range, IList<Rect> rects) { /* ... */ }
        public int GetLineFromCharacter(int index) { /* ... */ }
        public int GetLineStart(int lineIndex) { /* ... */ }
        public int GetLineEnd(int lineIndex) { /* ... */ }
        
        // ITextNavigator implementation
        public int Navigate(int pos, NavigationDirection dir, NavigationUnit unit) { /* ... */ }
        public TextRange GetWordBoundary(int pos) { /* ... */ }
        public TextRange GetLineBoundary(int pos) { /* ... */ }
        
        public void Dispose() => _cachedLayout = null;
    }
}
```

### 8.2 Unity Component

```csharp
namespace UniText.Unity
{
    [AddComponentMenu("UI/UniText Text")]
    [RequireComponent(typeof(CanvasRenderer))]
    public class UniTextText : MaskableGraphic, ILayoutElement
    {
        [SerializeField] private string _text = "";
        [SerializeField] private UniTextFontAsset _fontAsset;
        [SerializeField] private float _fontSize = 16f;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private TextAlignmentOptions _alignment;
        [SerializeField] private bool _richText = true;
        [SerializeField] private TextOverflowModes _overflow;
        [SerializeField] private TextWrappingModes _wrapping;
        
        private TextBlock _textBlock;
        private Mesh _mesh;
        private bool _meshDirty = true;
        
        public string text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    if (_textBlock != null) _textBlock.Text = value;
                    SetMeshDirty();
                }
            }
        }
        
        public float fontSize
        {
            get => _fontSize;
            set { _fontSize = value; SetMeshDirty(); }
        }
        
        protected override void Awake()
        {
            base.Awake();
            _textBlock = new TextBlock
            {
                Text = _text,
                EnableRichText = _richText,
                DefaultFontSize = _fontSize
            };
        }
        
        protected override void UpdateGeometry()
        {
            if (_meshDirty)
            {
                RebuildMesh();
                _meshDirty = false;
            }
            canvasRenderer.SetMesh(_mesh);
        }
        
        private void RebuildMesh()
        {
            var rect = rectTransform.rect;
            _textBlock.MaxWidth = rect.width;
            _textBlock.MaxHeight = rect.height;
            
            var layout = _textBlock.GetLayout();
            var builder = new UnityMeshBuilder();
            var renderer = new UniTextRenderer();
            
            renderer.Render(layout, _textBlock.Attributes, 
                new RenderOptions { Color = (Color32)_color }, builder);
            
            _mesh = builder.Build();
        }
        
        // ILayoutElement
        public float preferredWidth => _textBlock?.Width ?? 0;
        public float preferredHeight => _textBlock?.Height ?? 0;
    }
}
```

---

## 9. Implementation Plan

### Phase 1: Foundation (6 weeks)

| Week | Tasks | Deliverable |
|------|-------|-------------|
| 1-2 | Pipeline refactoring, interface definitions | Core architecture |
| 3-4 | HarfBuzz integration (Windows) | Working shaping |
| 5-6 | Basic pipeline, end-to-end test | Latin/Arabic rendering |

### Phase 2: Font System (4 weeks)

| Week | Tasks | Deliverable |
|------|-------|-------------|
| 7-8 | Font loading, IFontFace implementation | Font infrastructure |
| 9-10 | Fallback chain, Noto fonts | All scripts support |

### Phase 3: Interaction (4 weeks)

| Week | Tasks | Deliverable |
|------|-------|-------------|
| 11-12 | Hit testing, character rects | Click-to-position |
| 13-14 | Navigation, selection | Full editing support |

### Phase 4: Optimization (4 weeks)

| Week | Tasks | Deliverable |
|------|-------|-------------|
| 15-16 | Glyph atlas, SDF rendering | Cached glyphs |
| 17-18 | Layout cache, profiling | Production performance |

### Phase 5: Advanced Features (6+ weeks)

| Week | Tasks | Deliverable |
|------|-------|-------------|
| 19-20 | Vertical text layout | CJK vertical |
| 21-22 | COLR/CPAL parsing | Color emoji |
| 23-24 | Variable fonts | Full OpenType |

### Phase 6: Unity Integration (3 weeks)

| Week | Tasks | Deliverable |
|------|-------|-------------|
| 25-26 | UniTextText component, editor UI | Unity package |
| 27 | Documentation, examples, polish | Release |

---

## 10. Performance Targets

| Operation | Target | Notes |
|-----------|--------|-------|
| Initial layout (1000 chars) | < 5ms | Cold cache |
| Re-layout (width change) | < 2ms | Warm cache |
| Incremental update (typing) | < 1ms | Single char |
| Hit testing | < 0.1ms | Per call |
| Glyph rendering (cached) | < 0.5ms | Per 1000 glyphs |
| Cache hit rate (shaping) | > 80% | Typical UI |
| Cache hit rate (glyphs) | > 95% | After warmup |
| Memory (glyph atlas) | < 10MB | Per font/size |

---

## 11. Testing Strategy

### 11.1 Conformance Tests

Using official Unicode test files:
- `BidiCharacterTest.txt` - BiDi algorithm
- `LineBreakTest.txt` - Line breaking
- `GraphemeBreakTest.txt` - Grapheme clusters

### 11.2 Script Coverage Tests

Tier 1 (must pass):
- Latin, Cyrillic, Greek
- Arabic, Hebrew
- CJK (Chinese, Japanese, Korean)
- Emoji (including ZWJ sequences)

Tier 2 (should pass):
- Devanagari, Tamil, Telugu, Bengali
- Thai, Khmer, Myanmar

### 11.3 Performance Tests

- Initial layout benchmarks
- Cache hit rate validation
- Memory usage profiling
- Stress tests (10000+ chars)

---

## 12. Required Dependencies

### Native Libraries

| Platform | Library | Size |
|----------|---------|------|
| Windows | harfbuzz.dll | ~2MB |
| macOS | libharfbuzz.dylib | ~2MB |
| Linux | libharfbuzz.so | ~2MB |
| Android | System (API 21+) | 0 |
| iOS | CoreText fallback | 0 |
| WebGL | harfbuzz.wasm | ~1MB |

### Font Stack (Noto)

| Font | Coverage | Size |
|------|----------|------|
| Noto Sans | Latin, Cyrillic, Greek | ~500KB |
| Noto Sans Arabic | Arabic | ~200KB |
| Noto Sans Hebrew | Hebrew | ~100KB |
| Noto Sans CJK SC | Chinese | ~15MB |
| Noto Sans CJK JP | Japanese | ~15MB |
| Noto Sans CJK KR | Korean | ~15MB |
| Noto Sans Devanagari | Hindi | ~200KB |
| Noto Color Emoji | Emoji | ~10MB |

**Total (all fonts):** ~60MB  
**Minimal set:** ~5MB

---

## 13. Appendix: OpenType Features

| Tag | Name | Usage |
|-----|------|-------|
| liga | Ligatures | fi, fl combinations |
| kern | Kerning | Pair adjustments |
| calt | Contextual | Context-dependent forms |
| smcp | Small Caps | Lowercase → small caps |
| onum | Oldstyle Figures | Non-lining numbers |
| tnum | Tabular Figures | Monospace numbers |
| ss01-ss20 | Stylistic Sets | Alternative designs |
| init | Initial Forms | Arabic initial |
| medi | Medial Forms | Arabic medial |
| fina | Final Forms | Arabic final |
| isol | Isolated Forms | Arabic isolated |

---

## 14. References

- [UAX #9: BiDi Algorithm](https://unicode.org/reports/tr9/)
- [UAX #14: Line Breaking](https://unicode.org/reports/tr14/)
- [UAX #24: Script Property](https://unicode.org/reports/tr24/)
- [UAX #29: Text Segmentation](https://unicode.org/reports/tr29/)
- [HarfBuzz Documentation](https://harfbuzz.github.io/)
- [OpenType Specification](https://docs.microsoft.com/en-us/typography/opentype/spec/)

---

*Версия 1.0 — Декабрь 2024*
