using System.ComponentModel;
using Scenes.Launcher.SingleServices;
using UnityEngine.Scripting;

public partial class SROptions
{
    [Category("Game")]
    [Preserve]
    public float MinTimeInterval
    {
        get => GameController.minTimeInterval;
        set =>  GameController.minTimeInterval = value;
    }
    
    [Category("Game")]
    [Preserve]
    public int StartMusicIndex
    {
        get => GameController.startMusicIndex;
        set =>  GameController.startMusicIndex = value;
    }
    
    [Category("Game")]
    [Preserve]
    public float CarSpeed
    {
        get => GameController.carSpeed;
        set =>  GameController.carSpeed = value;
    }
    
    [Category("Game")]
    [Preserve]
    public int MaxMissCount
    {
        get => GameController.lifes;
        set =>  GameController.lifes = value;
    }
}