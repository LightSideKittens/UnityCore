using System;

/// <summary>
/// Базовый класс для модификаторов с ленивой инициализацией.
/// Буферы и подписки создаются только при первом Apply.
/// </summary>
[Serializable]
public abstract class BaseModifier
{
    protected UniText cachedUniText;
    protected bool isInitialized;

    public void Apply(int start, int end, string parameter)
    {
        if (!isInitialized)
        {
            CreateBuffers();
            Subscribe();
            isInitialized = true;
        }
        ApplyModifier(start, end, parameter);
    }

    public void Initialize(UniText uniText)
    {
        cachedUniText = uniText;
    }

    public void Deinitialize()
    {
        if (isInitialized)
        {
            Unsubscribe();
            ReleaseBuffers();
            isInitialized = false;
        }
        cachedUniText = null;
    }

    public void Reset()
    {
        if (isInitialized)
            ClearBuffers();
    }
    
    public virtual void Destroy() { }
    
    /// <summary>Создать буферы и установить static указатели.</summary>
    protected abstract void CreateBuffers();

    /// <summary>Подписаться на события UniText/MeshGenerator.</summary>
    protected abstract void Subscribe();

    /// <summary>Отписаться от событий.</summary>
    protected abstract void Unsubscribe();

    /// <summary>Освободить буферы (вернуть в пул).</summary>
    protected abstract void ReleaseBuffers();

    /// <summary>Очистить буферы (перед новым Rebuild).</summary>
    protected abstract void ClearBuffers();

    /// <summary>Логика Apply модификатора.</summary>
    protected abstract void ApplyModifier(int start, int end, string parameter);
}
