package com.lscore.notification;

import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Build;

import androidx.annotation.Nullable;
import androidx.core.app.NotificationCompat;
import androidx.core.app.NotificationManagerCompat;

import java.io.File;
import java.util.List;

public class NotificationHelper {

    public static final String CHANNEL_ID = "file_monitor_channel";
    public static final String ACTION_BUTTON_CLICK = "com.lscore.notification.ACTION_BUTTON_CLICK";

    public static final String EXTRA_NOTIFICATION_ID = "notification_id";
    public static final String EXTRA_ACTION_TYPE = "action_type";
    public static final String EXTRA_JSON_DATA = "json_data";

    public static final int BUTTON_TYPE_DISMISS = 0;
    public static final int BUTTON_TYPE_OPEN_APP = 1;

    public static void createNotificationChannel(Context context) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            CharSequence name = "Default Channel";
            int importance = NotificationManager.IMPORTANCE_HIGH;
            NotificationChannel channel = new NotificationChannel(CHANNEL_ID, name, importance);

            NotificationManager manager = context.getSystemService(NotificationManager.class);
            if (manager != null) {
                manager.createNotificationChannel(channel);
            }
        }
    }


    public static void showNotification(
            Context context,
            int notificationId,
            String title,
            @Nullable String description,
            @Nullable List<NotificationButton> buttons,
            @Nullable String jsonData,
            @Nullable String bigImagePath,
            @Nullable String smallImagePath
    ) {
        showNotification(context, notificationId, title, description, buttons, jsonData, bigImagePath, smallImagePath, false);
    }


    public static void showNotification(
            Context context,
            int notificationId,
            String title,
            @Nullable String description,
            @Nullable List<NotificationButton> buttons,
            @Nullable String jsonData,
            @Nullable String bigImagePath,
            @Nullable String smallImagePath,
            boolean openAppOnNotificationClick
    ) {
        createNotificationChannel(context);
    
        NotificationCompat.Builder builder = new NotificationCompat.Builder(context, CHANNEL_ID)
                .setSmallIcon(R.drawable.ic_notification)
                .setContentTitle(title)
                .setPriority(NotificationCompat.PRIORITY_HIGH)
                .setDefaults(NotificationCompat.DEFAULT_ALL)
                .setCategory(NotificationCompat.CATEGORY_MESSAGE)
                .setVisibility(NotificationCompat.VISIBILITY_PUBLIC)
                .setAutoCancel(true);
    
        if (description != null && !description.isEmpty()) {
            builder.setContentText(description);
        }
        
        if (smallImagePath != null && !smallImagePath.isEmpty()) {
            File smallImageFile = new File(context.getExternalFilesDir(null), smallImagePath);
            if (smallImageFile.exists()) {
                Bitmap smallBitmap = BitmapFactory.decodeFile(smallImageFile.getAbsolutePath());
                if (smallBitmap != null) {
                    builder.setLargeIcon(smallBitmap);
                }
            }
        }
    
        if (bigImagePath != null && !bigImagePath.isEmpty()) {
            File bigImageFile = new File(context.getExternalFilesDir(null), bigImagePath);
            if (bigImageFile.exists()) {
                Bitmap bigBitmap = BitmapFactory.decodeFile(bigImageFile.getAbsolutePath());
                if (bigBitmap != null) {
                    builder.setStyle(new NotificationCompat.BigPictureStyle()
                            .bigPicture(bigBitmap)
                            .setBigContentTitle(title)
                            .setSummaryText(description));
                }
            }
        }
    
        if (openAppOnNotificationClick) {
            Intent launchIntent = new Intent();
            launchIntent.setClassName(context.getPackageName(), "com.unity3d.player.UnityPlayerActivity");
            launchIntent.setAction(Intent.ACTION_MAIN);
            launchIntent.addCategory(Intent.CATEGORY_LAUNCHER);
            launchIntent.setFlags(
                    Intent.FLAG_ACTIVITY_NEW_TASK
                  | Intent.FLAG_ACTIVITY_CLEAR_TOP
                  | Intent.FLAG_ACTIVITY_SINGLE_TOP
            );
            if (jsonData != null) {
                launchIntent.putExtra(EXTRA_JSON_DATA, jsonData);
            }
    
            PendingIntent contentPendingIntent = PendingIntent.getActivity(
                    context,
                    notificationId,
                    launchIntent,
                    PendingIntent.FLAG_IMMUTABLE | PendingIntent.FLAG_UPDATE_CURRENT
            );
            builder.setContentIntent(contentPendingIntent);
        }
    
        if (buttons != null) {
            for (int i = 0; i < buttons.size(); i++) {
                NotificationButton button = buttons.get(i);
    
                if (button.type == BUTTON_TYPE_OPEN_APP) {
    
                    Intent launchIntent = new Intent();
                    launchIntent.setClassName(context.getPackageName(), "com.unity3d.player.UnityPlayerActivity");
                    launchIntent.setAction(Intent.ACTION_MAIN);
                    launchIntent.addCategory(Intent.CATEGORY_LAUNCHER);
                    launchIntent.putExtra(EXTRA_NOTIFICATION_ID, notificationId);
                    launchIntent.setFlags(
                            Intent.FLAG_ACTIVITY_NEW_TASK
                                    | Intent.FLAG_ACTIVITY_CLEAR_TOP
                                    | Intent.FLAG_ACTIVITY_SINGLE_TOP
                    );
    
                    if (jsonData != null) {
                        launchIntent.putExtra(EXTRA_JSON_DATA, jsonData);
                    }
    
                    PendingIntent openAppPendingIntent = PendingIntent.getActivity(
                            context,
                            notificationId * 100 + i,
                            launchIntent,
                            PendingIntent.FLAG_IMMUTABLE | PendingIntent.FLAG_UPDATE_CURRENT
                    );
    
                    builder.addAction(
                            new NotificationCompat.Action(
                                    0,
                                    button.label,
                                    openAppPendingIntent
                            )
                    );
    
                } else {
                    Intent actionIntent = new Intent(context, NotificationActionReceiver.class);
                    actionIntent.setAction(ACTION_BUTTON_CLICK);
                    actionIntent.putExtra(EXTRA_NOTIFICATION_ID, notificationId);
                    actionIntent.putExtra(EXTRA_ACTION_TYPE, button.type);
                    
                    if (button.type != BUTTON_TYPE_OPEN_APP && jsonData != null) {
                        actionIntent.putExtra(EXTRA_JSON_DATA, jsonData);
                    }
    
                    PendingIntent actionPendingIntent = PendingIntent.getBroadcast(
                            context,
                            notificationId * 100 + i,
                            actionIntent,
                            PendingIntent.FLAG_IMMUTABLE
                    );
    
                    builder.addAction(
                            new NotificationCompat.Action(
                                    0,
                                    button.label,
                                    actionPendingIntent
                            )
                    );
                }
            }
        }
    
        NotificationManagerCompat notificationManager = NotificationManagerCompat.from(context);
        notificationManager.notify(notificationId, builder.build());
    }
}
