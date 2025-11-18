using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class DeviceTime
{
    public static bool enableTimeCheats
#if DEBUG
    = true
#endif
        ;
    
    private long ticks;
    private long clockTicks;
    private int bootId;
    
    public event Action Changed;
    
    public long Ticks
    {
        get
        {
            var bootIdChanged = bootId != CurrentBootId;
#if DEBUG
            bootIdChanged |= enableTimeCheats;   
#endif
            if (bootIdChanged)
            {
                ticks = CurrentTicks + (clockTicks - DateTime.UtcNow.Ticks);
                bootId = CurrentBootId;
                Changed?.Invoke();
            }
            return ticks;
        }
        set => ticks = value;
    }
    
    public long ClockTicks => clockTicks;
    public int BootId => bootId;

    public TimeSpan Time
    {
        get => new(Ticks);
        set
        {
            var lts = ticks;
            bool wasChanged = lts != Ticks;
            
            var ts = value.Ticks;
            ticks = ts;
            clockTicks = DateTime.UtcNow.Ticks + (ts - CurrentTicks);
            
            if (!wasChanged)
            { 
                Changed?.Invoke();
            }
        }
    }

    private static DeviceTime now;
    public static DeviceTime Now
    {
        get
        {
            if (now == null) now = new DeviceTime(CurrentTicks);
            else now.ticks = CurrentTicks;
            now.clockTicks = DateTime.UtcNow.Ticks;
            return now;
        }
    }
    
    public static bool operator >(DeviceTime a, DeviceTime b) => a.Ticks > b.Ticks;

    public static bool operator <(DeviceTime a, DeviceTime b) => a.Ticks < b.Ticks;

    public static TimeSpan operator -(DeviceTime a, DeviceTime b) => new(a.Ticks - b.Ticks);
    public static TimeSpan operator +(DeviceTime a, TimeSpan b) => new(a.Ticks + b.Ticks);

    public DeviceTime(long ticks, long clockTicks, int bootId)
    {
        this.ticks = ticks;
        this.clockTicks = clockTicks;
        this.bootId = bootId;
    }
    
    public DeviceTime(long ticks)
    {
        bootId = CurrentBootId;
        this.Time = new(ticks);
    }

    private static AndroidJavaClass cls;
    private static AndroidJavaClass Cls => cls ??= new AndroidJavaClass("com.lscore.systemclock.DeviceTimeBridge");
    
    public static long CurrentTicks
    {
        get
        {
#if UNITY_EDITOR
            return (long)(EditorApplication.timeSinceStartup * 1000) * TimeSpan.TicksPerMillisecond;
#elif UNITY_ANDROID
            return Cls.CallStatic<long>("getElapsedRealtime") * TimeSpan.TicksPerMillisecond;
#elif UNITY_IOS
            return DeviceGetUptimeTicks();
#endif
        }
    }
    
    public static int CurrentBootId
    {
        get
        {
#if UNITY_EDITOR
            return EditorWorld.BootId;
#elif UNITY_ANDROID
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            using (var resolver = context.Call<AndroidJavaObject>("getContentResolver"))
            using (var settingsGlobal = new AndroidJavaClass("android.provider.Settings$Global"))
            {
                string key = settingsGlobal.GetStatic<string>("BOOT_COUNT");
                int defaultValue = -1;
                return settingsGlobal.CallStatic<int>("getInt", resolver, key, defaultValue);
            }
#elif UNITY_IOS
           return DeviceGetBootSessionId();
#endif
        }
    }
    
    [DllImport("__Internal")]
    private static extern ulong DeviceGetUptimeTicks();
    
    [DllImport("__Internal")]
    private static extern long DeviceGetBootSessionId();
}

public class DeviceTimeJObject
{
    public DeviceTime Value;
    public JObject jObject;

    public DeviceTimeJObject(JObject jObject, string key)
    {
        Value = jObject.AsDeviceTime(key);
        Value.Changed += OnChanged;
        this.jObject = (JObject)jObject[key];
    }

    private void OnChanged() => Value.SyncJObject(jObject);
}
    
public static class DeviceTimeExtensions
{
    public static JObject ToJObject(this DeviceTime time)
    {
        return time.SyncJObject(new JObject());
    }
        
    public static JObject SyncJObject(this DeviceTime time, JObject jObject)
    {
        var last = DeviceTime.enableTimeCheats;
        DeviceTime.enableTimeCheats = false;
        var ticks = time.Ticks;
        DeviceTime.enableTimeCheats = last;
        jObject["ticks"] = ticks;
        jObject["clockTicks"] = time.ClockTicks;
        jObject["bootId"] = time.BootId;
        return jObject;
    }

    public static DeviceTime ToDeviceTime(this JToken jObject)
    {
        var time = new DeviceTime(
            jObject["ticks"].ToObject<long>(),
            jObject["clockTicks"].ToObject<long>(),
            jObject["bootId"].ToObject<int>());
            
        return time;
    }
        
    public static DeviceTime AsDeviceTime(this JToken token, string key)
    {
        if (token[key] != null)
        {
            return token[key].ToDeviceTime();
        }
            
        var device = new DeviceTime(0);
        token[key] = device.ToJObject();
        return device;
    }
}