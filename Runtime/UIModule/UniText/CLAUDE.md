# UniText — Unicode Text Engine for Unity

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
| `Core/TMPFontAdapter.cs` | `TMPFontProvider` для работы с TMP |

---

## Архитектура

```
TextProcessor (координатор)
    ├── RichTextParser          ← парсинг rich text
    ├── BidiEngine              ← BiDi (UAX #9)
    ├── ScriptAnalyzer          ← скрипты (UAX #24)
    ├── [inline itemization]    ← разбиение на runs
    ├── IShapingEngine          ← shaping (единственный интерфейс)
    │       └── TMPShapingEngine
    ├── LineBreaker             ← перенос (UAX #14)
    └── TextLayout              ← позиционирование
```

---

## Важно

1. **Поля camelCase** — проверяй перед коммитом
2. **Используй `IUnicodeDataProvider`** для Unicode данных, не хардкодь codepoints
3. **Единственный интерфейс** — `IShapingEngine` для подключения HarfBuzz
