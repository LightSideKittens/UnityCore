using System;


[Serializable]
public abstract class BaseModifier
{
    [NonSerialized] public UniText uniText;
    protected UniTextBuffers buffers;
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
        this.uniText = uniText;
        buffers = uniText.Buffers;
    }

    public void Deinitialize()
    {
        if (!isInitialized) return;
        Unsubscribe();
        ReleaseBuffers();
        isInitialized = false;
    }

    public void Reset()
    {
        if (isInitialized)
            ClearBuffers();
    }

    protected abstract void CreateBuffers();


    protected abstract void Subscribe();


    protected abstract void Unsubscribe();


    protected abstract void ReleaseBuffers();


    protected abstract void ClearBuffers();


    protected abstract void ApplyModifier(int start, int end, string parameter);
}