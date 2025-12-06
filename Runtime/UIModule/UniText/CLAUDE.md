# UniText — Unicode Text Engine for Unity

Добавляй сюда новые общие правила, которые посчитаешь нужным, что они будут общими на весь проект.
---

## ⚠️ ГЛАВНАЯ ЦЕЛЬ ПРОЕКТА — ЧИТАЙ ПЕРВЫМ ⚠️

**UniText должен быть МАКСИМАЛЬНО ПРАВИЛЬНЫМ текстовым компонентом по ВСЕМ Unicode стандартам.**
**Создать промышленный текстовый движок для Unity, соответствующий всем стандартам Unicode и лучшим практикам индустрии (Pango, DirectWrite, Core Text).**

Это означает:
- **100% compliance** со всеми UAX стандартами (BiDi, Line Breaking, Script, Grapheme)
- **Полная поддержка ВСЕХ скриптов** — латиница, кириллица, арабский, иврит, деванагари, тамильский, тайский, CJK, и т.д.
- **HarfBuzz интеграция ОБЯЗАТЕЛЬНА** — это не "nice to have", это **must have** для правильного рендеринга сложных скриптов
- **Никаких компромиссов** — если что-то не работает правильно, это баг, который нужно исправить

**НЕ ГОВОРИ пользователю:**
- "HarfBuzz низкий приоритет"
- "Это можно сделать позже"
- "Для большинства случаев достаточно"

**ГОВОРИ:**
- "HarfBuzz необходим для полной поддержки Unicode"
- "Без HarfBuzz арабский/деванагари/тайский не будут работать правильно"
- "Это обязательная часть проекта"

---

## Документация

**📋 Текущий статус:** [PROJECT_STATE.md](./PROJECT_STATE.md)

---

## Conventions

### Naming
- **Поля (fields)**: `camelCase` — `public int glyphId;`
- **Всё остальное**: `PascalCase` — свойства, методы, типы, enums

### Namespace
Без namespace (global)

### Принципы
- Минимум интерфейсов — только `IShapingEngine` для HarfBuzz
- Unified buffers в `TextProcessor` — без лишних копирований
- Компоненты используются напрямую, без обёрток

---

## Ключевые файлы

| Файл | Описание |
|------|----------|
| `Core/TextProcessor.cs` | Главный координатор с unified buffers |
| `Core/IPipeline.cs` | `IShapingEngine` интерфейс |
| `Core/TagSystem.cs` | Расширяемая система тегов |
| `Core/UniTextFontAsset.cs` | Font asset (TMP-free, использует Unity TextCore) |
| `Core/UniTextFontProvider.cs` | Font provider (TMP-free) |
| `Core/UniTextMeshGenerator.cs` | Генератор mesh для рендеринга |

---

## Архитектура (TMP-Free)

```
TextProcessor (координатор)
    ├── RichTextParser          ← парсинг rich text
    ├── BidiEngine              ← BiDi (UAX #9)
    ├── ScriptAnalyzer          ← скрипты (UAX #24)
    ├── [inline itemization]    ← разбиение на runs
    ├── IShapingEngine          ← shaping (единственный интерфейс)
    │       ├── UniTextShapingEngine   ← базовый shaping
    │       └── HarfBuzzShapingEngine  ← OpenType shaping
    ├── LineBreaker             ← перенос (UAX #14)
    └── TextLayout              ← позиционирование

UniTextFontProvider             ← работа с UniTextFontAsset
UniTextMeshGenerator            ← генерация mesh для рендеринга
```

---

## Важно

1. **Поля camelCase** — проверяй перед коммитом
2. **Используй `UnicodeData.Provider`** для Unicode данных, не хардкодь codepoints
3. **Единственный интерфейс** — `IShapingEngine` для подключения HarfBuzz

---

## Что НЕ является проблемой

### Unicode алгоритмы корректны
- **BidiEngine (UAX #9)** — проходит 100% conformance тестов Unicode
- **LineBreakAlgorithm (UAX #14)** — проходит 100% conformance тестов
- **ScriptAnalyzer (UAX #24)** — проходит conformance тесты
- **GraphemeBreaker (UAX #29)** — проходит conformance тесты

ICU нельзя использовать в Unity, поэтому алгоритмы реализованы с нуля. Это НЕ велосипед — это необходимость.

### RTL позиционирование корректно
Логика в `TextLayout.cs` правильная:
```csharp
float runEndX = x + run.width;
for (int g = 0; g < glyphLen; g++)
{
    runEndX -= glyph.advanceX;
    result[glyphCount++] = new PositionedGlyph { x = runEndX + glyph.offsetX };
}
```
Глифы размещаются справа налево в пределах run — это корректно для RTL.

### Shaping — известное ограничение
`UniTextShapingEngine` не выполняет OpenType shaping (GSUB/GPOS). Это known limitation, не баг. HarfBuzz интеграция реализована через `HarfBuzzShapingEngine` (требует UNITEXT_HARFBUZZ define и HarfBuzzSharp).

---

## Известные TODO

### КРИТИЧЕСКИ ВАЖНО (без этого проект не считается завершённым)
- [x] **UniTextFontAsset с raw bytes** — реализовано, готово для HarfBuzz
- [ ] **HarfBuzz интеграция** — использовать `UniTextFontAsset.FontData` для shaping
- [ ] **Применение атрибутов** (color, size) в рендеринге — rich text должен работать полностью

### Важно
- [ ] Emoji система (нативные платформенные эмодзи)
- [ ] Text effects (outline, shadow)
- [ ] Text input (caret, selection, IME)
- [ ] Kerning support

---

## Архитектурные решения

### Emoji система (планируется)
Вместо хранения огромного атласа эмодзи в билде:
1. На каждой платформе находим предустановленные системные эмодзи
2. Рендерим их на нативной стороне (iOS/Android/Windows)
3. Записываем в runtime-атлас
4. Возвращаем `EmojiSprite`
5. В тексте делаем пустой отступ на месте эмодзи
6. Вставляем картинку с эмодзи в этот отступ

**Преимущества:**
- Размер билда не увеличивается
- Эмодзи в стиле платформы (Apple/Google/Windows)
- Автоматическая поддержка новых эмодзи

### UniTextFontAsset (реализовано)
Собственный font asset, полностью независимый от TMP_FontAsset:
- **Raw font bytes** — сериализуются в ассет для использования HarfBuzz
- **В Editor**: Назначается `sourceFont`, автоматически извлекаются байты
- **В Runtime**: Только `byte[] fontData` + атлас + метрики
- **FontEngine** используется для динамической загрузки глифов
- **Fallback fonts** поддерживаются через `FallbackFontAssetTable`

```csharp
// Создание из raw bytes (для HarfBuzz):
var fontAsset = UniTextFontAsset.CreateFontAsset(fontBytes);

// Доступ к raw bytes для HarfBuzz:
byte[] fontData = fontAsset.FontData;
```

### Static UnicodeData
Unicode данные загружаются один раз через `UnicodeData.Provider`. Все компоненты используют статический синглтон.

### Shared Static Resources (реализовано)
Для минимизации аллокаций используются статические shared ресурсы в `SharedPools.cs`:
- `SharedTextBuffers` — буферы TextProcessor
- `SharedFontCache` — кеш font fallback lookup
- `SharedMeshPool` — пул Mesh объектов
