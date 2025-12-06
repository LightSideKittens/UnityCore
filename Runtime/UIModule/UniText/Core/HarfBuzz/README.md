# HarfBuzz Integration for UniText

## Зачем нужен HarfBuzz?

HarfBuzz необходим для правильного рендеринга сложных скриптов:
- **Арабский** — лигатуры, контекстные формы (начальная/средняя/конечная/изолированная)
- **Деванагари** — матры, полуформы, конъюнкты
- **Тайский** — комбинирование диакритиков
- **Хинди, Тамильский, Бенгальский** и другие индийские скрипты

Без HarfBuzz эти скрипты будут отображаться **некорректно**.

## Установка HarfBuzzSharp

### Вариант 1: SkiaForUnity (рекомендуется)

1. Откройте Package Manager
2. Добавьте package by git URL:
   ```
   https://github.com/ammariqais/SkiaForUnity.git?path=SkiaUnity/Assets/SkiaSharp
   ```
3. Добавьте `UNITEXT_HARFBUZZ` в **Player Settings → Scripting Define Symbols**

### Вариант 2: NuGet packages (ручная настройка)

1. Установите через NuGetForUnity:
   - `HarfBuzzSharp` (managed DLL)

2. Скачайте native библиотеки для каждой платформы:
   - Windows: `HarfBuzzSharp.NativeAssets.Win32`
   - Android: `HarfBuzzSharp.NativeAssets.Android`
   - iOS: `HarfBuzzSharp.NativeAssets.iOS`
   - macOS: `HarfBuzzSharp.NativeAssets.macOS`

3. Разместите native библиотеки в `Plugins/`:
   ```
   Plugins/
   ├── Windows/
   │   └── libHarfBuzzSharp.dll
   ├── Android/
   │   ├── arm64-v8a/
   │   │   └── libHarfBuzzSharp.so
   │   └── armeabi-v7a/
   │       └── libHarfBuzzSharp.so
   ├── iOS/
   │   └── libHarfBuzzSharp.a
   └── macOS/
       └── libHarfBuzzSharp.dylib
   ```

4. Добавьте `UNITEXT_HARFBUZZ` в **Player Settings → Scripting Define Symbols**

## Использование

### 1. Создайте UniTextFontAsset

1. **Assets → Create → UniText → Font Asset**
2. Назначьте TTF/OTF шрифт в поле **Source Font**
3. Нажмите **Generate TMP Font Asset**

### 2. Используйте HarfBuzzShapingEngine

```csharp
// В TextProcessor или UniText

var harfBuzzEngine = new HarfBuzzShapingEngine();

// Регистрируем шрифт
harfBuzzEngine.RegisterFont(fontId, uniTextFontAsset.FontData);

// Используем вместо TMPShapingEngine
processor.SetShapingEngine(harfBuzzEngine);
```

### 3. Автоматическое переключение

UniText автоматически использует HarfBuzz для сложных скриптов если:
- HarfBuzzSharp установлен (`UNITEXT_HARFBUZZ` определён)
- Шрифт зарегистрирован через `UniTextFontAsset`

## Поддерживаемые скрипты

| Скрипт | Без HarfBuzz | С HarfBuzz |
|--------|--------------|------------|
| Latin, Cyrillic | ✅ | ✅ |
| Hebrew | ⚠️ RTL OK | ✅ |
| Arabic | ❌ | ✅ |
| Devanagari | ❌ | ✅ |
| Thai | ⚠️ | ✅ |
| Tamil, Bengali | ❌ | ✅ |
| CJK | ✅ | ✅ |

## Troubleshooting

### "HarfBuzzSharp not installed"
Убедитесь что:
1. HarfBuzzSharp NuGet package установлен
2. Native библиотеки в правильных папках
3. `UNITEXT_HARFBUZZ` добавлен в Scripting Define Symbols

### "Font not registered"
Вызовите `RegisterFont()` перед первым использованием:
```csharp
harfBuzzEngine.RegisterFont(fontId, fontAsset.FontData);
```

### Глифы отображаются как квадраты
Шрифт не содержит глифы для этих символов. Используйте шрифт с поддержкой нужных скриптов (например, Noto Sans).
