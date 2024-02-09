using System;

[Serializable]
public class ShortNoteTrackData : BaseTrackData<ShortNoteData>
{
    public ShortNoteTrackData(string name, ShortNoteData[] notes) : base(name, notes) { }

    protected override void CheckNoteIn(in float currentTime)
    {
        if (currentTime > GetStartTime(nextNote))
        {
            OnNoteIn();
            CheckComplete(currentTime);
        }
    }

    protected override float GetStartTime(in int index)
    {
        return notes[index].startTime;
    }
}