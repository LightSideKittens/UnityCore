using System;
using Newtonsoft.Json;

[Serializable]
public class LongNoteTrackData : BaseTrackData<LongNoteData>
{
    public event Action Playing;
    public event Action NoteOut;

    public LongNoteTrackData(string name, LongNoteData[] notes) : base(name, notes) { }

    [JsonIgnore] public LongNoteData CurrentLongNote => notes[nextNote];


    protected override void CheckNoteIn(in float currentTime)
    {
        if (currentTime > GetStartTime(nextNote))
        {
            OnNoteIn();
            updater = CheckNoteOut;
        }
    }

    protected override float GetStartTime(in int index)
    {
        return notes[index].startTime;
    }

    private void CheckNoteOut(in float currentTime)
    {
        if (currentTime < notes[nextNote].endTime)
        {
            Playing?.Invoke();
        }
        else
        {
            NoteOut?.Invoke();
            CheckComplete(currentTime);
        }
    }
}