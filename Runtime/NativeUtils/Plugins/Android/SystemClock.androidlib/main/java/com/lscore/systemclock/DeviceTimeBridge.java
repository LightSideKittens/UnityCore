package com.lscore.systemclock;

public class DeviceTimeBridge extends android.app.Activity {
    public static long getElapsedRealtime() {
        return android.os.SystemClock.elapsedRealtime();
    }

    public static long getUptimeMillis() {
        return android.os.SystemClock.uptimeMillis();
    }
}
