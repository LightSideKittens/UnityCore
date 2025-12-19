using System;


[Serializable]
public abstract class BaseModifier
{
    protected UniText uniText;
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
        if (isInitialized)
        {
            Unsubscribe();
            ReleaseBuffers();
            isInitialized = false;
        }

        uniText = null;
    }

    public void Reset()
    {
        if (isInitialized)
            ClearBuffers();
    }

    public virtual void Destroy()
    {
    }


    protected abstract void CreateBuffers();


    protected abstract void Subscribe();


    protected abstract void Unsubscribe();


    protected abstract void ReleaseBuffers();


    protected abstract void ClearBuffers();


    protected abstract void ApplyModifier(int start, int end, string parameter);
}