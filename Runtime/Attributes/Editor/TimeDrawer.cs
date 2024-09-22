#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using LSCore.Attributes;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Internal;
using static LSCore.Attributes.TimeAttribute;

public abstract class TimeDrawer<T> : OdinAttributeDrawer<T, long> where T : TimeAttribute
{
    protected int year, month, day, hour, minute, second, millisecond;

    protected List<(Options, RenderDateTime)> dateMap;
    protected List<(Options, RenderDateTime)> timeMap;
    
    protected Options options;
    
    protected override void Initialize()
    {
        base.Initialize();
        options = Attribute.options;
        
        dateMap = new ()
        {
            (Options.Year, DrawYear),
            (Options.Month, DrawMonth),
            (Options.Day, DrawDay),
        };
        
        timeMap = new ()
        {
            (Options.Hour, DrawHour),
            (Options.Minute, DrawMinute),
            (Options.Second, DrawSecond),
            (Options.Millisecond, DrawMillisecond)
        };
    }

    protected override void DrawPropertyLayout(GUIContent label)
    { 
        BeforeDraw();
        EditorUtils.DrawInBoxFoldout(label, Draw, this, false);
        ValueEntry.SmartValue = AfterDraw();
    }

    protected virtual void Draw()
    {
        EditorGUILayout.BeginHorizontal();
        
        var dateActions = GetRenderActions(options, dateMap);
        var timeActions = GetRenderActions(options, timeMap);

        DrawBlock(dateActions);
        DrawBlock(timeActions);
        
        EditorGUILayout.EndHorizontal();
    }

    protected abstract void BeforeDraw();
    protected abstract long AfterDraw();

    protected static void DrawBlock(List<RenderDateTime> actions)
    {
        if (actions.Count > 0)
        {
            EditorGUILayout.BeginVertical();
            foreach (var action in actions)
            {
                action();
            }
            EditorGUILayout.EndVertical();
        }
    }
    
    public delegate void RenderDateTime();

    private static int Int(string label, int value)
    {
        var old = EditorGUIUtility.labelWidth; 
        EditorGUIUtility.labelWidth = 70f;
        
        value = EditorGUILayout.IntField(label, value);

        EditorGUIUtility.labelWidth = old;
        return value;
    }
    
    protected void DrawYear() => year = Int("Year", year);
    protected void DrawMonth() => month = Int("Month", month);
    protected void DrawDay() => day = Int("Day", day);
    protected void DrawHour() => hour = Int("Hour", hour);
    protected void DrawMinute() => minute = Int("Minute", minute);
    protected void DrawSecond() => second = Int("Second", second);
    protected void DrawMillisecond() => millisecond = Int("Millisecond", millisecond);

    private static List<RenderDateTime> GetRenderActions(Options options, List<(Options options, RenderDateTime action)> map)
    {
        var actions = new List<RenderDateTime>();
        
        foreach (var entry in map)
        {
            if (options.HasFlag(entry.options))
            {
                actions.Add(entry.action);
            }
        }

        return actions;
    }
}

public class DateTimeDrawer : TimeDrawer<DateTimeAttribute>
{
    private DateTime dateTime;

    protected override void Initialize()
    {
        base.Initialize();

        ValueEntry.SmartValue = Math.Clamp(ValueEntry.SmartValue, DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks);
        
        dateTime = ValueEntry.SmartValue == 0 
            ? Attribute.defaultValue 
            : new DateTime(ValueEntry.SmartValue);

        ValueEntry.SmartValue = dateTime.Ticks;
        UpdateData(dateTime);
    }

    protected override void BeforeDraw()
    {
        dateTime = new DateTime(ValueEntry.SmartValue);
    }

    protected override void Draw()
    {
        base.Draw();
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal();
            
        bool now = GUILayout.Button("NOW");
        bool utcNow = GUILayout.Button("UTC NOW");
        bool today = GUILayout.Button("TODAY");
            
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();

        bool max = GUILayout.Button("MAX");
        bool min = GUILayout.Button("MIN");
                
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        if (now) dateTime = DateTime.Now;
        else if (utcNow) dateTime = DateTime.UtcNow;
        else if (today) dateTime = DateTime.Today;
        else if (max) dateTime = DateTime.MaxValue;
        else if (min) dateTime = DateTime.MinValue;
        else dateTime = CreateDateTime(year, month, day, hour, minute, second, millisecond);
    }
    
    
    protected override long AfterDraw()
    {
        UpdateData(dateTime);
        return dateTime.Ticks;
    }
    
    private void UpdateData(DateTime dateTime)
    {
        year = dateTime.Year;
        month = dateTime.Month;
        day = dateTime.Day;
        hour = dateTime.Hour;
        minute = dateTime.Minute;
        second = dateTime.Second;
        millisecond = dateTime.Millisecond;
    }
    
    private DateTime CreateDateTime(
        int year, 
        int month, 
        int day, 
        int hour = 0, 
        int minute = 0, 
        int second = 0, 
        int millisecond = 0)
    {
        year = Math.Max(1, Math.Min(9999, year));
        DateTime baseDate = new DateTime(year, 1, 1);

        try
        {
            baseDate = baseDate.AddMonths(month - 1);
            baseDate = baseDate.AddDays(day - 1);
        
            baseDate = baseDate.AddHours(hour);
            baseDate = baseDate.AddMinutes(minute);
            baseDate = baseDate.AddSeconds(second);
            baseDate = baseDate.AddMilliseconds(millisecond);
        }
        catch { }
        
        return baseDate;
    }
}

public class TimeSpanDrawer : TimeDrawer<TimeSpanAttribute>
{
    private TimeSpan minSpan;
    private TimeSpan maxSpan;
    private TimeSpan span;

    protected override void Initialize()
    {
        base.Initialize();
        minSpan = Property.GetAttribute<MinTimeSpanAttribute>()?.value ?? TimeSpan.MinValue;
        maxSpan = Property.GetAttribute<MaxTimeSpanAttribute>()?.value ?? TimeSpan.MaxValue;
        
        var abs = Math.Abs(ValueEntry.SmartValue);
        
        span = abs == 0
            ? Attribute.defaultValue 
            : new TimeSpan(ValueEntry.SmartValue);

        if (span < minSpan) span = minSpan;
        if (span > maxSpan) span = maxSpan;
        ValueEntry.SmartValue = span.Ticks;
        UpdateData(span);
    }

    protected override void BeforeDraw()
    {
        span = new TimeSpan(ValueEntry.SmartValue);
    }

    protected override long AfterDraw()
    {
        span = new TimeSpan(day, hour, minute, second, millisecond);
        if (span < minSpan) span = minSpan;
        if (span > maxSpan) span = maxSpan;
        
        UpdateData(span);
        var tick = span.Ticks;
        return tick == 0 ? 1 : tick;
    }

    private void UpdateData(TimeSpan span)
    {
        day = span.Days;
        hour = span.Hours;
        minute = span.Minutes;
        second = span.Seconds;
        millisecond = span.Milliseconds;
    }
}
#endif