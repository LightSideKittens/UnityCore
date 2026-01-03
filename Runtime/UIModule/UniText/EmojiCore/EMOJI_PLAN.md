# План: Система Emoji для UniText (v3 - финальная)

## Цель
Реализовать поддержку color emoji с **максимальным переиспользованием** существующей логики UniTextFont.

## Ключевые решения
- **Шрифты**: Системные + bundled (напр. Twemoji)
- **Color модификатор**: Emoji игнорируют color атрибут
- **ZWJ/Skin tone**: HarfBuzz автоматически обрабатывает (EmojiSequenceResolver НЕ нужен!)
- **Архитектура**: Emoji как специальный UniTextFont в font cascade

---

## Главная идея: Emoji = специальный UniTextFont

Вместо отдельной системы атласов, используем **существующую** инфраструктуру:

1. `EmojiFont` наследует `UniTextFont`
2. Переопределяет только рендеринг (FreeType вместо Unity FontEngine)
3. Встраивается в `UniTextFonts` как fallback шрифт
4. MeshGenerator рендерит emoji как обычные глифы!

**Преимущества**:
- MultiAtlas уже работает в UniTextFont
- HarfBuzz **уже** умеет шейпить emoji (ZWJ, skin tone → автоматически!)
- Генерация mesh уже работает
- Минимум нового кода

---

## Важно: HarfBuzz и ZWJ sequences

HarfBuzz автоматически обрабатывает ZWJ sequences в `Shape()`:
- Если шрифт поддерживает 👨‍👩‍👧 как единый глиф → возвращает 1 glyphId
- Если НЕ поддерживает → возвращает 3 отдельных glyphId (👨, 👩, 👧)

**Никакой дополнительной логики не нужно!**

---

## Этап 1: FreeTypeWrapper (managed обёртка)

**Файл**: `EmojiCore/FreeType/FreeTypeWrapper.cs`

```csharp
public static class FreeTypeWrapper
{
    // Структуры
    public struct FaceInfo { hasColor, hasFixedSizes, availableSizes[], unitsPerEm }
    public struct RenderedGlyph { width, height, bearingX/Y, advance, byte[] rgba, pitch }

    // API
    static bool LoadFontFromPath(string path);
    static bool LoadFontFromData(byte[] data);
    static FaceInfo GetFaceInfo();
    static uint GetGlyphIndex(uint codepoint);
    static bool TryRenderGlyph(uint glyphIndex, int targetSize, out RenderedGlyph result);
    static bool HasGlyph(uint codepoint);  // для проверки ZWJ sequences
    static void Dispose();
}
```

**Ключевое**:
- CBDT/CBLC (Noto, Samsung): `FT_Select_Size()` → `FT_LOAD_COLOR | FT_LOAD_RENDER`
- COLR/CPAL (Segoe): `FT_Set_Pixel_Sizes()` → `FT_LOAD_COLOR` → `FT_Render_Glyph()`
- Конвертация BGRA → RGBA

---

## Этап 2: Модификация UniTextFont (виртуализация)

**Файл**: `FontCore/UniTextFont.cs` (модификация)

Текущие методы `TryAddGlyphToAtlasByIndex` и т.д. являются **private**. Нужно:

```csharp
// Было:
private bool TryAddGlyphToAtlasByIndex(uint glyphIndex) { ... }

// Стало:
protected virtual bool TryAddGlyphToAtlasByIndex(uint glyphIndex) { ... }
```

Это позволит `EmojiFont` переопределить логику рендеринга.

---

## Этап 3: EmojiFont (наследник UniTextFont)

**Файл**: `EmojiCore/EmojiFont.cs`

```csharp
public class EmojiFont : UniTextFont
{
    private static EmojiFont instance;
    public static EmojiFont Instance => instance ??= CreateEmojiFont();

    // Переопределяем метод добавления глифа
    protected override bool TryAddGlyphToAtlasByIndex(uint glyphIndex)
    {
        // 1. Рендерим через FreeTypeWrapper (не Unity FontEngine!)
        if (!FreeTypeWrapper.TryRenderGlyph(glyphIndex, emojiSize, out var rendered))
            return false;

        // 2. Bin packing - ищем место в атласе
        //    Используем существующие freeGlyphRects/usedGlyphRects
        var rect = FindRectForGlyph(rendered.width, rendered.height);
        if (rect == null)
        {
            // Атлас полон → создаём новый (существующая логика MultiAtlas)
            CreateNewAtlasTexture();
            rect = FindRectForGlyph(rendered.width, rendered.height);
        }

        // 3. Копируем RGBA пиксели в атлас
        CopyPixelsToAtlas(rendered.rgba, rect.Value);

        // 4. Создаём Glyph с метриками
        var glyph = CreateGlyph(glyphIndex, rendered, rect.Value);
        glyphTable.Add(glyph);
        glyphLookupDictionary[glyphIndex] = glyph;

        atlasTextures[atlasTextureIndex].Apply(false, false);
        return true;
    }

    private static EmojiFont CreateEmojiFont()
    {
        var fontPath = SystemEmojiFont.GetDefaultEmojiFont();
        if (fontPath == null) return null;

        FreeTypeWrapper.LoadFontFromPath(fontPath);
        var info = FreeTypeWrapper.GetFaceInfo();

        var font = CreateInstance<EmojiFont>();
        font.name = "System Emoji";
        font.atlasRenderMode = GlyphRenderMode.COLOR;  // RGBA32 текстуры
        font.atlasWidth = 1024;
        font.atlasHeight = 1024;
        return font;
    }
}
```

**Ключевое**:
- Вся логика MultiAtlas (freeGlyphRects, usedGlyphRects, atlasTextures[]) **наследуется**
- glyphLookupDictionary, characterLookupDictionary **наследуются**
- Только рендеринг заменён на FreeType

---

## Этап 4: SystemEmojiFont (улучшения)

**Файл**: `EmojiCore/SystemEmojiFont.cs` (модификация)

```csharp
public static class SystemEmojiFont
{
    public static string GetDefaultEmojiFont()
    {
        var path = GetPlatformEmojiFont();
        if (path == null) return null;

        // НОВОЕ: Валидация
        if (!ValidateEmojiFont(path)) return null;
        return path;
    }

    private static bool ValidateEmojiFont(string path)
    {
        FreeTypeWrapper.LoadFontFromPath(path);
        var info = FreeTypeWrapper.GetFaceInfo();

        // Должен иметь color support
        if (!info.hasColor) return false;

        // Проверяем наличие базового emoji (😀 = U+1F600)
        if (!FreeTypeWrapper.HasGlyph(0x1F600)) return false;

        return true;
    }
}
```

---

## Этап 5: Интеграция с Font Cascade

**Файл**: `FontCore/UniTextFonts.cs` (модификация)

```csharp
public class UniTextFonts : ScriptableObject
{
    // Существующие шрифты
    public UniTextFont[] fonts;

    // НОВОЕ: Emoji как автоматический fallback
    public UniTextFont FindFontForCodepoint(uint codepoint, out int fontId)
    {
        // 1. Сначала ищем в обычных шрифтах
        foreach (var font in fonts)
            if (font.HasGlyph(codepoint))
                return font;

        // 2. Fallback на emoji шрифт
        if (IsEmojiCodepoint(codepoint) && EmojiFont.Instance != null)
            return EmojiFont.Instance;

        return null;
    }
}
```

---

## Этап 5.5: HarfBuzz Shaping для Emoji

### Как это работает (без изменений в HarfBuzz!)

HarfBuzz **уже** умеет шейпить emoji через GSUB таблицы шрифта:

**Пример для 👨‍👩‍👧 (U+1F468 + ZWJ + U+1F469 + ZWJ + U+1F467):**
```
Вход: [0x1F468, 0x200D, 0x1F469, 0x200D, 0x1F467] (5 codepoints)

HarfBuzz.Shape():
  1. Читает GSUB таблицу emoji шрифта
  2. Находит ligature: "1F468+200D+1F469+200D+1F467 → glyphId 12345"
  3. Применяет замену

Выход: [12345] (1 glyphId) — шрифт поддерживает
   или: [100, 200, 300] (3 glyphIds) — fallback
```

### Почему изменения в HarfBuzzShapingEngine.cs НЕ нужны

Текущий код уже работает для любого UniTextFont:

```csharp
// GetOrCreateCacheByInstanceId() - строка 101
var fontData = font.FontData;  // EmojiFont наследует это!
fontEntry = new HarfBuzzFontCache(fontData);

// Shape() - строка 243
fontEntry.font.Shape(buffer);  // HarfBuzz читает GSUB из emoji шрифта
```

### Что нужно: регистрация EmojiFont в UniTextFontProvider

**Файл**: `FontCore/UniTextFontProvider.cs` (модификация)

```csharp
// В GetFontAsset или отдельном методе
public UniTextFont GetFontAsset(int fontId)
{
    // ... существующая логика ...

    // Если fontId == специальный emoji ID
    if (fontId == EmojiFont.FontId)
        return EmojiFont.Instance;

    // ...
}
```

EmojiFont наследует `FontData`, `HasFontData`, `SetFontData()` — HarfBuzz подхватывает автоматически!

---

## Этап 6: ColorModifier - пропуск emoji

**Файл**: `ModCore/ColorModifier.cs` (модификация)

```csharp
private static void OnGlyph()
{
    var gen = UniTextMeshGenerator.Current;
    var fontId = gen.currentFontId;

    // Пропускаем emoji (у них свой цвет)
    if (fontId == EmojiFont.Instance?.GetCachedInstanceId())
        return;

    // ... существующая логика ...
}
```

---

## Структура файлов

```
EmojiCore/
├── FreeType/
│   └── FreeTypeWrapper.cs        # НОВЫЙ - managed обёртка
│
├── EmojiFont.cs                   # НОВЫЙ - наследник UniTextFont
├── SystemEmojiFont.cs             # МОДИФИКАЦИЯ - добавить валидацию
│
└── Test/
    └── EmojiTest.cs               # УДАЛИТЬ после
```

## Файлы для модификации

1. `FontCore/UniTextFonts.cs` - добавить emoji fallback в FindFontForCodepoint
2. `FontCore/UniTextFont.cs` - сделать TryAddGlyphToAtlas virtual (если не virtual)
3. `FontCore/UniTextFontProvider.cs` - добавить GetFontAsset для EmojiFont
4. `ModCore/ColorModifier.cs` - пропуск emoji
5. `LSCore.UniText.asmdef` - reference на FreeTypeSharp

---

## Порядок реализации

1. **FreeTypeWrapper** - базовая обёртка, тесты рендеринга
2. **EmojiFont** - наследник UniTextFont с FreeType рендерингом
3. **SystemEmojiFont** - добавить валидацию
4. **UniTextFonts + UniTextFontProvider** - интеграция emoji в font cascade
5. **ColorModifier** - пропуск emoji

---

## Преимущества упрощённой архитектуры

| Аспект | Старый план | Новый план |
|--------|-------------|------------|
| MultiAtlas | Своя система | Наследуется от UniTextFont |
| Mesh generation | Отдельная логика | Существующая GenerateMeshDataForFont |
| Shaping | Отдельный детектор | HarfBuzz (уже работает) |
| Font fallback | Ручная замена | Встроен в cascade |
| Новых файлов | ~8 | **2** |
| Модификаций | ~4 | ~5 |

---

## Тестовые случаи

1. Простой emoji: "Hello 😀 World"
2. ZWJ sequence (поддерживается): "👨‍👩‍👧‍👦"
3. ZWJ sequence (fallback): на старой ОС → 👨👩👧👦
4. Skin tone: "👋🏻👋🏿"
5. Emoji + markup: "<b>Test 🎉</b>" (bold игнорируется для emoji)
6. RTL с emoji: "שלום 😊 עולם"
7. MultiAtlas: много уникальных emoji
8. Color игнорируется: "<color=red>😀</color>" остаётся жёлтым
