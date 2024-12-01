using System;
using System.Collections.Generic;
using BeatHeroes;
using DG.Tweening;
using LSCore;
using LSCore.Async;
using MusicEventSystem.Configs;
using UnityEngine;

public class Playlist : SingleService<Playlist>
{
    public const int MusicDelay = 1;
    public const float SilenceFix = 0.04f;
    
    public static event Action Started;
    public static event Action MusicChanged;
    public static float StartTimeOffset { get; set; }
    public static float Time { get; private set; }
    public static float NormalizedTime => Mathf.InverseLerp(0, MusicData.Config.realEndTime, Time);

    public static MusicData NextMusic
    {
        get
        {
            var data = musicsData[Instance.GetNextMusicIndex()];
            AudioClip clip = data.audioClip;
            var endTime = Mathf.Min(data.endTime, clip.length);

            if (!cachedMusic.TryGetValue(data, out var musicData))
            {
                musicData = MusicData.Load(data.configName, data.startTime, endTime);
                cachedMusic.Add(data, musicData);
            }

            return musicData;
        }
    }

    private static readonly Dictionary<MusicLevelData, MusicData> cachedMusic = new();
    private static List<MusicLevelData> musicsData;

    [SerializeField] private string configName;
    [SerializeField] private float startTime;
    [SerializeField] private float endTime = MusicData.DefaultEndTime;
    [SerializeField] private AudioSource source;

    private ShortNoteListener listener;
    private Tween playTimer;
    private int currentMusic;
    private int count;
    private bool isMainMusicStarted;

    public static void Create(List<MusicLevelData> data)
    {
        Resume();
        Instance.Internal_Create(data);
    }

    public static void Create(MusicLevelData data) => Create(new List<MusicLevelData> { data });
    public static void Add(MusicLevelData data) => musicsData.Add(data);
    public static void AddRange(IEnumerable<MusicLevelData> data) => musicsData.AddRange(data);
    public static void Remove(MusicLevelData data) => musicsData.Remove(data);
    public static void RemoveAt(int index) => musicsData.RemoveAt(index);
    public static void Insert(int index, MusicLevelData data) => musicsData.Insert(index, data);

    public static void Destroy()
    {
        Destroy(Instance.gameObject);
    }

    protected override void DeInit()
    {
        base.DeInit();
        playTimer?.Kill();
        listener?.Dispose();
    }

    public static void Pause()
    {
        Instance.enabled = false;
    }

    public static void Resume()
    {
        Instance.enabled = true;
    }

    private void Internal_Create(List<MusicLevelData> dataList)
    {
        musicsData = dataList;
        isMainMusicStarted = false;
        Set(currentMusic);
    }

    private void Set(int index)
    {
        currentMusic = index;
        var data = musicsData[index];

        startTime = data.startTime;
        endTime = data.endTime;
        configName = data.configName;
        source.clip = data.audioClip;

        var startTimeOffseted = startTime + StartTimeOffset;
        var endTimeClipped = Mathf.Min(endTime, source.clip.length);
        
        if (cachedMusic.TryGetValue(data, out var musicData))
        {
            MusicData.SetConfig(musicData, configName, startTimeOffseted, endTimeClipped);
        }
        else
        {
            MusicData.ConfigName = configName;
            MusicData.StartTime = startTimeOffseted;
            MusicData.EndTime = endTimeClipped;
        }

        listener = ShortNoteListener.Listen().OnComplete(PlayNext);
        StartTimeOffset = 0;
        Time = 0;
        source.time = startTimeOffseted + SilenceFix;
        source.Stop();
    }

    private int GetNextMusicIndex() => (currentMusic + 1) % musicsData.Count;

    private void SetNext() => Set(GetNextMusicIndex());
    private void PlayNext()
    {
        SetNext();
        PlaySource();
        cachedMusic.Remove(musicsData[currentMusic]);
        MusicChanged?.Invoke();
    }


    private void PlaySource() => playTimer = Wait.Delay(MusicDelay, source.Play);

    public static void Play() => Instance.Internal_Play();
    
    private void Internal_Play()
    {
        if (isMainMusicStarted) return;
        
        isMainMusicStarted = true;
        PlaySource();

        Started?.Invoke();
        cachedMusic.Remove(musicsData[currentMusic]);
    }
    
    public void Update()
    {
        if (LSInput.UIExcluded.IsTouchDown)
        {
            Internal_Play();
        }
        
        if (isMainMusicStarted)
        {
            MusicData.Update(Time);
            Time += UnityEngine.Time.deltaTime;
        }
    }
}
