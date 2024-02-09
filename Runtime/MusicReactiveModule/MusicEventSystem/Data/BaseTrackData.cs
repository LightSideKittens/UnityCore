using System;

[Serializable]
public abstract class BaseTrackData
{
    public event Action NoteIn;
    public event Action Completed;

    protected void Complete()
    {
        Completed?.Invoke();
    }

    protected void OnNoteIn()
    {
        NoteIn?.Invoke();
    }

    public abstract string Name { get; }
    internal abstract void Update(in float currentTime);
}

[Serializable]
public abstract class BaseTrackData<T> : BaseTrackData where T : struct
{
    public T[] notes;
    protected int nextNote;
    protected InAction<float> updater;
    public int NextNote => nextNote;
    public override string Name { get; }

    public BaseTrackData(string name, T[] notes)
    {
        Name = name;
        this.notes = notes;
        if (notes.Length == 0)
        {
            updater = UpdaterPlug;
        }
        else
        {
            updater = CheckNoteIn;
        }
    }

    protected abstract void CheckNoteIn(in float currentTime);

    protected void UpdaterPlug(in float currentTime) => Complete();
    
    protected void CheckComplete(in float currentTime)
    {
        nextNote++;

        if (nextNote < notes.Length)
        {
            updater = CheckNoteIn;
            CheckNoteIn(currentTime);
        }
        else
        {
            nextNote--;
            Complete();
        }
    }

    protected abstract float GetStartTime(in int index);

    internal override void Update(in float currentTime)
    {
        updater(currentTime);
    }
}