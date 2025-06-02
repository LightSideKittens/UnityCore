package com.lscore.emoji;

import android.graphics.Paint;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Color;

import java.text.BreakIterator;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

import android.icu.lang.UCharacter;
import android.icu.lang.UProperty;
import android.os.Build;

public class API {

    private static final int TARGET_SIZE = 256;
    private static final Paint paint;
    private static final Bitmap bitmap;
    private static final Canvas canvas;
    private static final int[] pixelBuffer = new int[TARGET_SIZE * TARGET_SIZE];
    private static final byte[] rgbaBuffer = new byte[TARGET_SIZE * TARGET_SIZE * 4];

    static {
        paint = new Paint(Paint.ANTI_ALIAS_FLAG | Paint.FILTER_BITMAP_FLAG | Paint.DITHER_FLAG);
        paint.setTextAlign(Paint.Align.LEFT);
        paint.setTextSize(TARGET_SIZE);

        bitmap = Bitmap.createBitmap(TARGET_SIZE, TARGET_SIZE, Bitmap.Config.ARGB_8888);
        canvas = new Canvas(bitmap);
    }

    public static byte[] emojiToRGBA32(String emoji) {
        if (emoji == null || emoji.isEmpty()) {
            return new byte[0];
        }

        Paint.FontMetricsInt fmTemp = paint.getFontMetricsInt();
        float rawHeight = fmTemp.descent - fmTemp.ascent;
        float rawWidth  = paint.measureText(emoji);
        if (rawWidth <= 0 || rawHeight <= 0) {
            return new byte[0];
        }
        
        float scaleX = TARGET_SIZE / rawWidth;
        float scaleY = TARGET_SIZE / rawHeight;
        float scaleFactor = Math.min(scaleX, scaleY);
        float newTextSize = TARGET_SIZE * scaleFactor;

        paint.setTextSize(newTextSize);

        Paint.FontMetricsInt fm = paint.getFontMetricsInt();
        float textWidth  = paint.measureText(emoji);
        float textHeight = fm.descent - fm.ascent;

        bitmap.eraseColor(Color.TRANSPARENT);

        float x = (TARGET_SIZE - textWidth) / 2f;
        float y = (TARGET_SIZE - textHeight) / 2f - fm.ascent;
        canvas.drawText(emoji, x, y, paint);
        bitmap.getPixels(pixelBuffer, 0, TARGET_SIZE, 0, 0, TARGET_SIZE, TARGET_SIZE);

        final int width  = TARGET_SIZE;
        final int height = TARGET_SIZE;
        for (int dstY = 0, srcY = height - 1; dstY < height; dstY++, srcY--) {
            int srcRowOffset = srcY * width;
            int dstRowOffset = dstY * width;
            int pixelIndexSrc = srcRowOffset;
            int byteIndexDst = dstRowOffset * 4;
            
            for (int xCur = 0; xCur < width; xCur++, pixelIndexSrc++, byteIndexDst += 4) {
                int argb = pixelBuffer[pixelIndexSrc];
                byte r = (byte) ((argb >>> 16) & 0xFF);
                byte g = (byte) ((argb >>>  8) & 0xFF);
                byte b = (byte) ( argb         & 0xFF);
                byte a = (byte) ((argb >>> 24) & 0xFF);
                rgbaBuffer[byteIndexDst    ] = r;
                rgbaBuffer[byteIndexDst + 1] = g;
                rgbaBuffer[byteIndexDst + 2] = b;
                rgbaBuffer[byteIndexDst + 3] = a;
            }
        }

        return rgbaBuffer;
    }

    public static EmojiRange[] parseEmojis(String text) {
        if (text == null || text.isEmpty()) {
            return new EmojiRange[0];
        }
        
        List<EmojiRange> result = new ArrayList<>();
        Paint paint = new Paint(Paint.ANTI_ALIAS_FLAG);
        paint.setTextSize(64);
    
        BreakIterator bi = BreakIterator.getCharacterInstance(Locale.getDefault());
        bi.setText(text);
    
        int start = bi.first();
        for (int end = bi.next(); end != BreakIterator.DONE; start = end, end = bi.next()) {
            int length = end - start;
            String cluster = text.substring(start, end);
    
            if (isEmojiCluster(cluster)) {
                if (!paint.hasGlyph(cluster)) {
                    String zwj = "\u200D";
                    int localOffset = start;
                    String[] parts = cluster.split(zwj);
    
                    for (String part : parts) {
                        int partLen = part.length();
                        if (paint.hasGlyph(part)) {
                            result.add(new EmojiRange(localOffset, partLen, part));
                        } else {
                            int local = 0;
                            while (local < partLen) {
                                int cp = part.codePointAt(local);
                                int cpLen = Character.charCount(cp);
                                String cpStr = new String(Character.toChars(cp));
                                result.add(new EmojiRange(localOffset + local, cpLen, cpStr));
                                local += cpLen;
                            }
                        }
                        localOffset += partLen + zwj.length();
                    }
                } else {
                    result.add(new EmojiRange(start, length, cluster));
                }
            }
        }
    
        return result.toArray(new EmojiRange[result.size()]);
    }


    private static boolean isEmojiCluster(String cluster) {
        if (cluster == null || cluster.isEmpty()) {
            return false;
        }
        int offset = 0;
        int length = cluster.length();
        while (offset < length) {
            int cp = cluster.codePointAt(offset);
            offset += Character.charCount(cp);
            if (isEmojiCodepoint(cp)) {
                return true;
            }
        }
        return false;
    }

    private static boolean isEmojiCodepoint(int codePoint) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
            return UCharacter.hasBinaryProperty(codePoint, UProperty.EMOJI);
        } else {
            return (codePoint >= 0x1F600 && codePoint <= 0x1F64F)
                || (codePoint >= 0x1F300 && codePoint <= 0x1F5FF)
                || (codePoint >= 0x1F680 && codePoint <= 0x1F6FF)
                || (codePoint >= 0x1F900 && codePoint <= 0x1F9FF)
                || (codePoint >= 0x2600  && codePoint <= 0x26FF)
                || (codePoint >= 0x2700  && codePoint <= 0x27BF)
                || (codePoint >= 0x2B00  && codePoint <= 0x2BFF)
                || (codePoint >= 0x1F100 && codePoint <= 0x1F1FF)
                || (codePoint >= 0x1F200 && codePoint <= 0x1F2FF)
                || (codePoint >= 0x1FA70 && codePoint <= 0x1FAFF);
        }
    }

    public static class EmojiRange {
        public int index;
        public int length;
        public String emoji;

        public EmojiRange(int index, int length, String emoji) {
            this.index = index;
            this.length = length;
            this.emoji = emoji;
        }
    }
}