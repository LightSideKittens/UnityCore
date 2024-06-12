package com.lscore.emojirenderer;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Typeface;
import android.text.TextPaint;
import java.io.ByteArrayOutputStream;

public class EmojiRenderer {
    public static Bitmap renderEmoji(String text, float textSize) {
        TextPaint paint = new TextPaint();
        paint.setTextSize(textSize);
        paint.setTypeface(Typeface.DEFAULT);
        paint.setAntiAlias(true);

        int width = (int) paint.measureText(text);
        int height = (int) (paint.descent() - paint.ascent());

        Bitmap bitmap = Bitmap.createBitmap(width, height, Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmap);
        canvas.drawText(text, 0, -paint.ascent(), paint);

        return bitmap;
    }

    public static byte[] renderEmojiToByteArray(String text, float textSize) {
        Bitmap bitmap = renderEmoji(text, textSize);
        ByteArrayOutputStream stream = new ByteArrayOutputStream();
        bitmap.compress(Bitmap.CompressFormat.PNG, 100, stream);
        return stream.toByteArray();
    }
}
