package com.lscore.emojirenderer;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Typeface;
import android.text.Layout;
import android.text.StaticLayout;
import android.text.TextPaint;
import java.io.ByteArrayOutputStream;
import java.text.BreakIterator;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

public class EmojiRenderer {
    public static Bitmap renderEmojiToBitmap(String text, float textSize, int textColor) {
        TextPaint paint = new TextPaint();
        paint.setTextSize(textSize);
        paint.setTypeface(Typeface.DEFAULT);
        paint.setAntiAlias(true);
        paint.setColor(textColor);

        // Определяем ширину текста
        int width = (int) Math.ceil(Layout.getDesiredWidth(text, paint));

        // Создаем StaticLayout для многострочного текста
        StaticLayout staticLayout = new StaticLayout(text, paint, width, Layout.Alignment.ALIGN_NORMAL, 1.0f, 0.0f, false);

        // Определяем высоту текста
        int height = staticLayout.getHeight();

        // Создаем Bitmap с учетом высоты и ширины текста
        Bitmap bitmap = Bitmap.createBitmap(width, height, Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmap);

        // Отрисовываем текст на Canvas
        staticLayout.draw(canvas);

        return bitmap;
    }

    public static byte[] renderEmoji(String text, float textSize, int textColor) {
        Bitmap bitmap = renderEmojiToBitmap(text, textSize, textColor);
        ByteArrayOutputStream stream = new ByteArrayOutputStream();
        bitmap.compress(Bitmap.CompressFormat.PNG, 100, stream);
        return stream.toByteArray();
    }

    private static final String TAG = "GraphemeClusterBreaker";

    public static List<String> getGraphemeClusters(String text) {
        List<String> graphemeClusters = new ArrayList<>();
        BreakIterator iterator = BreakIterator.getCharacterInstance(Locale.getDefault());
        iterator.setText(text);
        int start = iterator.first();
        for (int end = iterator.next(); end != BreakIterator.DONE; start = end, end = iterator.next()) {
            graphemeClusters.add(text.substring(start, end));
        }
        return graphemeClusters;
    }
}
