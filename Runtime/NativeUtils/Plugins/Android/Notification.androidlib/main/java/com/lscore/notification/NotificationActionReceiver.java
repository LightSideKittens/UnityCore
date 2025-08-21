package com.lscore.notification;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Environment;
import android.util.Log;

import androidx.core.app.NotificationManagerCompat;

import java.io.File;
import java.io.FileOutputStream;

public class NotificationActionReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        int notificationId = intent.getIntExtra(NotificationHelper.EXTRA_NOTIFICATION_ID, -1);
        int actionType = intent.getIntExtra(NotificationHelper.EXTRA_ACTION_TYPE, -1);

        if (actionType == NotificationHelper.BUTTON_TYPE_DISMISS) {
            NotificationManagerCompat.from(context).cancel(notificationId);
            Log.d("NotificationAction", "Dismiss action pressed. Notification " + notificationId + " canceled.");
        }
    }
}
