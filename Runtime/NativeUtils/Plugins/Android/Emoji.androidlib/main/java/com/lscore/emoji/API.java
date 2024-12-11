package com.lscore.emoji;

import android.content.Context;
import android.content.SharedPreferences;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.text.Spannable;
import android.text.style.ReplacementSpan;

import androidx.emoji2.text.EmojiCompat;
import androidx.emoji2.text.EmojiSpan;

import org.json.JSONArray;
import org.json.JSONObject;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.Map;

public class API {
    private static final int MAX_CACHE_SIZE = 100;

    // LRU-кэш. accessOrder=true означает, что каждое обращение к элементу переносит его в конец,
    // поддерживая порядок по времени последнего доступа.
    private static LinkedHashMap<String, String> emojiCache = new LinkedHashMap<String, String>(16, 0.75f, true) {
        @Override
        protected boolean removeEldestEntry(Map.Entry<String, String> eldest) {
            return false; // Удаляем вручную, когда превышаем лимит
        }
    };

    private static Context appContext;

    public static void initCache(Context context) {
        appContext = context.getApplicationContext();
        loadCacheFromDisk();
        // Дополнительно можно проверить, что все файлы в кэше существуют,
        // и удалить записи для несуществующих файлов.
        cleanupNonExistingFiles();
    }

    public static EmojiRange[] parseEmojis(String text, String saveDirPath) {
        if (!isEmojiCompatInitialized()) {
            return new EmojiRange[0];
        }

        CharSequence processed = EmojiCompat.get().process(text, 0, text.length());
        if (!(processed instanceof Spannable)) {
            return new EmojiRange[0];
        }

        Spannable spannable = (Spannable) processed;
        ReplacementSpan[] spans = spannable.getSpans(0, spannable.length(), ReplacementSpan.class);
        if (spans == null || spans.length == 0) {
            return new EmojiRange[0];
        }

        EmojiRange[] result = new EmojiRange[spans.length];
        int counter = 0;

        for (ReplacementSpan span : spans) {
            if (span instanceof EmojiSpan) {
                int start = spannable.getSpanStart(span);
                int end = spannable.getSpanEnd(span);
                int length = end - start;

                String emojiKey = spannable.subSequence(start, end).toString();

                String imagePath = getCachedEmojiPath(emojiKey);
                if (imagePath == null || imagePath.isEmpty() || !(new File(imagePath).exists())) {
                    // Эмодзи нет в кэше или файл пропал - создаём заново
                    Bitmap bitmap = createEmojiBitmap((EmojiSpan) span, spannable, start, end);
                    if (bitmap != null) {
                        String fileName = "emoji_" + System.nanoTime() + ".png";
                        imagePath = saveBitmapToPNG(bitmap, saveDirPath, fileName);

                        if (!imagePath.isEmpty()) {
                            addEmojiToCache(emojiKey, imagePath);
                        }
                    }
                }
                result[counter] = new EmojiRange(start, length, imagePath);
                counter++;
            }
        }

        return result;
    }

    private static boolean isEmojiCompatInitialized() {
        try {
            EmojiCompat.get();
            return true;
        } catch (IllegalStateException e) {
            return false;
        }
    }

    private static Bitmap createEmojiBitmap(EmojiSpan emojiSpan, CharSequence text, int start, int end) {
        Paint paint = new Paint(Paint.ANTI_ALIAS_FLAG);
        paint.setFilterBitmap(true);
        paint.setDither(true);
        paint.setTextSize(256);

        Paint.FontMetricsInt fm = new Paint.FontMetricsInt();
        int width = emojiSpan.getSize(paint, text, start, end, fm);
        int lineHeight = fm.descent - fm.ascent;

        Bitmap bitmap = Bitmap.createBitmap(width, lineHeight, Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmap);

        int baseline = -fm.ascent;
        int top = 0;
        int bottom = lineHeight;
        float x = 0f;

        emojiSpan.draw(canvas, text, start, end, x, top, baseline, bottom, paint);

        return bitmap;
    }

    private static String saveBitmapToPNG(Bitmap bitmap, String dirPath, String fileName) {
        File directory = new File(dirPath);
        if (!directory.exists()) {
            directory.mkdirs();
        }

        File file = new File(directory, fileName);
        try (FileOutputStream out = new FileOutputStream(file)) {
            bitmap.compress(Bitmap.CompressFormat.PNG, 100, out);
            return file.getAbsolutePath();
        } catch (IOException e) {
            e.printStackTrace();
            return "";
        }
    }

    private static synchronized String getCachedEmojiPath(String emoji) {
        return emojiCache.get(emoji);
    }

    private static synchronized void addEmojiToCache(String emoji, String path) {
        emojiCache.put(emoji, path);
        if (emojiCache.size() > MAX_CACHE_SIZE) {
            // Удаляем самый старый элемент
            Map.Entry<String, String> eldest = emojiCache.entrySet().iterator().next();
            String eldestKey = eldest.getKey();
            String eldestPath = eldest.getValue();
            emojiCache.remove(eldestKey);
            // Удаляем файл
            File oldFile = new File(eldestPath);
            if (oldFile.exists()) {
                oldFile.delete();
            }
        }
        saveCacheToDisk();
    }

    // Загрузка кэша из SharedPreferences (JSON)
    private static synchronized void loadCacheFromDisk() {
        if (appContext == null) return;
        SharedPreferences prefs = appContext.getSharedPreferences("emoji_cache_prefs", Context.MODE_PRIVATE);
        String jsonStr = prefs.getString("emoji_cache", null);
        if (jsonStr != null) {
            try {
                JSONObject json = new JSONObject(jsonStr);
                JSONArray keys = json.names();
                // Восстанавливаем кэш
                LinkedHashMap<String, String> restored = new LinkedHashMap<String, String>(16, 0.75f, true);
                for (int i = 0; i < keys.length(); i++) {
                    String key = keys.getString(i);
                    String val = json.getString(key);
                    restored.put(key, val);
                }
                emojiCache = restored;
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }

    // Сохранение кэша в SharedPreferences (JSON)
    private static synchronized void saveCacheToDisk() {
        if (appContext == null) return;
        try {
            JSONObject json = new JSONObject();
            for (Map.Entry<String, String> entry : emojiCache.entrySet()) {
                json.put(entry.getKey(), entry.getValue());
            }
            SharedPreferences prefs = appContext.getSharedPreferences("emoji_cache_prefs", Context.MODE_PRIVATE);
            prefs.edit().putString("emoji_cache", json.toString()).apply();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    // Проверяем файлы в кэше, удаляем из кэша записи для которых файлы отсутствуют
    private static synchronized void cleanupNonExistingFiles() {
        Iterator<Map.Entry<String, String>> iter = emojiCache.entrySet().iterator();
        boolean changed = false;
        while (iter.hasNext()) {
            Map.Entry<String, String> entry = iter.next();
            File f = new File(entry.getValue());
            if (!f.exists()) {
                iter.remove();
                changed = true;
            }
        }
        if (changed) {
            saveCacheToDisk();
        }
    }

    public static class EmojiRange {
        public int index;
        public int length;
        public String imagePath;

        public EmojiRange(int index, int length, String imagePath) {
            this.index = index;
            this.length = length;
            this.imagePath = imagePath;
        }
    }
}
