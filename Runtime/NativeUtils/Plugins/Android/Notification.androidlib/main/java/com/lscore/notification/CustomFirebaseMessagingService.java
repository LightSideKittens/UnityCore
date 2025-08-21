package com.lscore.notification;

import android.util.Log;

import com.google.firebase.messaging.FirebaseMessagingService;
import com.google.firebase.messaging.RemoteMessage;

import org.json.JSONObject;

import java.util.Map;

public class CustomFirebaseMessagingService extends FirebaseMessagingService {

    private static final String TAG = "CustomFirebaseMsgService";

    @Override
    public void onMessageReceived(RemoteMessage remoteMessage) {
        Log.d(TAG, "onMessageReceived, data=" + remoteMessage.getData() + 
                ", notif=" + (remoteMessage.getNotification()!=null));
        if (remoteMessage.getData().size() > 0) {
            Map<String, String> data = remoteMessage.getData();
            Log.d(TAG, "Data payload: " + data.toString());

            String jsonString = new JSONObject(data).toString();

            String title = data.get("title");
            String message = data.get("message");

            int notificationId = 100;

            NotificationHelper.showNotification(
                    getApplicationContext(),
                    notificationId,
                    title,
                    message,
                    null,
                    jsonString,
                    null,
                    null,
                    true
            );
        }
    }

    @Override
    public void onNewToken(String token) {
        super.onNewToken(token);
        Log.d(TAG, "Refreshed token: " + token);
    }
}
