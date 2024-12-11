package com.lscore.emoji;

import android.app.Application;
import androidx.emoji2.text.EmojiCompat;
import androidx.emoji2.bundled.BundledEmojiCompatConfig;

public class App extends Application {
    @Override
    public void onCreate() {
        super.onCreate();
        EmojiCompat.Config config = new BundledEmojiCompatConfig(this)
                .setReplaceAll(true);
        EmojiCompat.init(config);
        
        API.initCache(this);
    }
}
