using System.Diagnostics;
using UnityEngine;

public class PerformanceTest : MonoBehaviour
{
    private Stopwatch stopwatch;
    private long ticks;
    private int frames = 1;

    public long AverageTicks => ticks / frames;
    private long currentTicks;
    private long maxTicks;
    private long minTicks = long.MaxValue;

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 40), "Average Ticks: " + AverageTicks);
        GUI.Label(new Rect(10, 30, 300, 40), "Current Ticks: " + currentTicks);
        GUI.Label(new Rect(10, 50, 300, 40), "Max Ticks: " + maxTicks);
        GUI.Label(new Rect(10, 70, 300, 40), "Min Ticks: " + minTicks);
    }

    private void Update()
    {
        if (stopwatch == null)
        {
            stopwatch = new Stopwatch(); 
            stopwatch.Start();
        }
        else
        {
            stopwatch.Stop();
            
            currentTicks = stopwatch.ElapsedTicks;
            ticks += currentTicks;
            if (currentTicks > maxTicks)
            {
                maxTicks = currentTicks;
            }

            if (currentTicks < minTicks)
            {
                minTicks = currentTicks;
            }
            
            stopwatch.Reset();
            stopwatch.Start();
        }

        frames++;
    }
}