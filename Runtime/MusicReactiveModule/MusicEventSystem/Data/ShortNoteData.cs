using System;

[Serializable]
public struct ShortNoteData
{
    public float startTime;

    public ShortNoteData(float startTime)
    {
        this.startTime = startTime;
    }
}