# UniText — Unicode Text Engine for Unity

## Документация

**📖 Полная спецификация:** [UniText_Architecture.md](./UniText_Architecture.md)
Содержит детальное описание всех компонентов, интерфейсов, примеры кода и план реализации.

**📋 Текущий статус:** [PROJECT_STATE.md](./PROJECT_STATE.md)

---

## Conventions

### Naming
- **Поля (fields)**: `camelCase` — `public int glyphId;`
- **Всё остальное**: `PascalCase` — свойства, методы, типы, enums

### Namespace
`UniText` или `UniText.*`

### Принципы
- Расширяемость через интерфейсы и регистрацию
- Переиспользование буферов (без аллокаций)
- Не реализуем объёмную и повторяющуюся конкретику, например, теги — только инфраструктуру

---

## Ключевые файлы

| Файл | Описание |


d




|------|----------|
| `Core/Pipeline.cs` | Все интерфейсы пайплайна |
| `Core/Attributes.cs` | Система атрибутов |
| `Parsing/TagSystem.cs` | Расширяемая система тегов |

---

## Существующий код (адаптируем)

```
../Unicode/
├── BidiEngine.cs              → IBidiAnalyzer
├── Analysis/ScriptAnalyzer.cs → IScriptAnalyzer
├── Layout/LineBreakAlgorithm.cs → ILineBreakAnalyzer
└── Grapheme/GraphemeBreaker.cs  → IGraphemeAnalyzer
```

---

## Важно

1. **Читай UniText_Architecture.md** для деталей реализации
2. **Используй существующий код** из Unicode/ — не переписывай
3. **Поля camelCase** — проверяй перед коммитом
