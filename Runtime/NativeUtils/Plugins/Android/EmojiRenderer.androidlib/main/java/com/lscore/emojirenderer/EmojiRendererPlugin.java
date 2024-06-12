package com.lscore.emojirenderer;

import android.util.Base64;

public class EmojiRendererPlugin {
    public static String renderEmoji(String text, float textSize) {
        byte[] byteArray = EmojiRenderer.renderEmojiToByteArray(text, textSize);
        return Base64.encodeToString(byteArray, Base64.DEFAULT);
    }
}
