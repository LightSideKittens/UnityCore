package com.lscore.textrenderer;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Typeface;
import android.text.Layout;
import android.text.Spannable;
import android.text.SpannableString;
import android.text.StaticLayout;
import android.text.TextPaint;
import android.text.TextUtils;
import android.text.style.ReplacementSpan;

import androidx.core.content.res.ResourcesCompat;
import java.io.ByteArrayOutputStream;

public class TextRenderer
{
    private static Typeface customTypeface;
    private static float customTextSize = 16f; // Установка значения по умолчанию
    private static int customTextColor = 0xFF000000; // Черный цвет по умолчанию
    private static int width = 0;
    private static int height = 0;
    private static Layout.Alignment alignment = Layout.Alignment.ALIGN_NORMAL;
    private static boolean wrapText = true; // Установка значения по умолчанию
    private static TextUtils.TruncateAt overflow = null;
    private static float strokeWidth = 0f; // Значение по умолчанию
    private static int strokeColor = 0xFF000000; // Черный цвет по умолчанию
    private static float underlayOffsetX = 0f; // Значение по умолчанию
    private static float underlayOffsetY = 0f; // Значение по умолчанию
    private static int underlayColor = 0xFF000000; // Черный цвет по умолчанию
    private static float underlayDilate = 0f; // Значение по умолчанию
    private static float underlaySoftness = 0f; // Значение по умолчанию
    private static float faceDilate = 0f; // Значение по умолчанию

    public static void setCustomFont(Context context, String fontName) {
        int fontResId = context.getResources().getIdentifier(fontName, "font", context.getPackageName());
        customTypeface = ResourcesCompat.getFont(context, fontResId);
    }

    public static void setCustomTextSize(float textSize) {
        customTextSize = textSize;
    }

    public static void setCustomTextColor(int textColor) {
        customTextColor = textColor;
    }

    public static void setRect(int width, int height) {
        TextRenderer.width = width;
        TextRenderer.height = height;
    }

    public static void setAlignment(int align) {
        switch (align) {
            case 1:
                alignment = Layout.Alignment.ALIGN_OPPOSITE;
                break;
            case 2:
                alignment = Layout.Alignment.ALIGN_CENTER;
                break;
            default:
                alignment = Layout.Alignment.ALIGN_NORMAL;
                break;
        }
    }

    public static void setWrapText(boolean wrap) {
        wrapText = wrap;
    }

    public static void setOverflow(int overflowType) {
        switch (overflowType) {
            case 1:
                overflow = TextUtils.TruncateAt.END;
                break;
            case 2:
                overflow = TextUtils.TruncateAt.MARQUEE;
                break;
            case 3:
                overflow = TextUtils.TruncateAt.MIDDLE;
                break;
            case 4:
                overflow = TextUtils.TruncateAt.START;
                break;
            default:
                overflow = null;
                break;
        }
    }

    public static void setStroke(float width, int color) {
        strokeWidth = width;
        strokeColor = color;
    }

    public static void setUnderlay(int color, float offsetX, float offsetY, float dilate, float softness) {
        underlayColor = color;
        underlayOffsetX = offsetX;
        underlayOffsetY = offsetY;
        underlayDilate = dilate;
        underlaySoftness = softness;
    }

    public static void setFaceDilate(float dilate) {
        faceDilate = dilate;
    }

    public static Bitmap renderToBitmap(String text)
    {
       TextPaint paint = new TextPaint();
       paint.setTextSize(customTextSize);
       paint.setTypeface(customTypeface != null ? customTypeface : Typeface.DEFAULT);
       paint.setAntiAlias(true);

        if (faceDilate != 0) {
            paint.setFakeBoldText(true); // Используем жирный текст для увеличения толщины
            paint.setStyle(Paint.Style.FILL_AND_STROKE);
            paint.setStrokeWidth(faceDilate * customTextSize); // Пропорциональная обводка для Face Dilate
        }
        
       int adjustedWidth = (width > 0) ? width : (int) Math.ceil(Layout.getDesiredWidth(text, paint));

       // Рассчитываем максимальное количество строк, которые могут поместиться по высоте
       int maxLines = Integer.MAX_VALUE;

       StaticLayout.Builder builder = StaticLayout.Builder.obtain(text, 0, text.length(), paint, adjustedWidth)
               .setAlignment(alignment)
               .setLineSpacing(0, 1.0f)
               .setIncludePad(false)
               .setEllipsize(overflow);

       StaticLayout staticLayout = builder.build();
       
       if (height > 0) {
           int lineHeight = staticLayout.getHeight() / staticLayout.getLineCount();
           maxLines = height / lineHeight;
           builder.setMaxLines(maxLines);
           staticLayout = builder.build();
       }

       int adjustedHeight = staticLayout.getHeight();

       // Учитываем максимальное значение обводки и тени для увеличения размера Bitmap
       float maxPadding = Math.max(strokeWidth, Math.max(underlayOffsetX + underlayDilate, underlayOffsetY + underlayDilate));

       // Создаем Bitmap с большими размерами для предотвращения обрезки текста
       Bitmap bitmap = Bitmap.createBitmap(adjustedWidth + (int)maxPadding * 2, adjustedHeight + (int)maxPadding * 2, Bitmap.Config.ARGB_8888);
       Canvas canvas = new Canvas(bitmap);

       canvas.translate(maxPadding, maxPadding); // Перемещаем текст внутрь битмапа

       // Сначала рисуем обводку
       if (strokeWidth > 0) {
           paint.setStyle(Paint.Style.STROKE);
           paint.setStrokeWidth(strokeWidth * customTextSize); // Пропорциональная обводка
           paint.setColor(strokeColor);
           staticLayout.draw(canvas);
       }
       
       if (underlayOffsetX != 0 || underlayOffsetY != 0 || underlayDilate != 0 || underlaySoftness != 0) {
           paint.setStyle(Paint.Style.STROKE);
           paint.setStrokeWidth(strokeWidth * customTextSize); // Пропорциональная обводка
           paint.setColor(strokeColor);
           canvas.translate(underlayOffsetX * 5, underlayOffsetY * 5);
           staticLayout.draw(canvas);
       }

       // Затем рисуем основной текст
       paint.setStyle(Paint.Style.FILL);
       paint.setColor(customTextColor);
       staticLayout.draw(canvas);

       return bitmap;
    }

    public static byte[] render(String text)
    {
        Bitmap bitmap = renderToBitmap(text);
        ByteArrayOutputStream stream = new ByteArrayOutputStream();
        bitmap.compress(Bitmap.CompressFormat.PNG, 100, stream);
        return stream.toByteArray();
    }
}

