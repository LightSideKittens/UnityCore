package com.lscore.textrenderer;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Typeface;
import android.text.Layout;
import android.text.StaticLayout;
import android.text.TextPaint;
import androidx.core.content.res.ResourcesCompat;
import java.io.ByteArrayOutputStream;
import android.text.TextUtils;

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

    public static Bitmap renderToBitmap(String text)
    {
        TextPaint paint = new TextPaint();
        paint.setTextSize(customTextSize);
        paint.setTypeface(customTypeface != null ? customTypeface : Typeface.DEFAULT);
        paint.setAntiAlias(true);
        paint.setColor(customTextColor);

        int adjustedWidth = (width > 0) ? width : (int) Math.ceil(Layout.getDesiredWidth(text, paint));
        StaticLayout.Builder builder = StaticLayout.Builder.obtain(text, 0, text.length(), paint, adjustedWidth)
                .setAlignment(alignment)
                .setLineSpacing(0, 1.0f)
                .setIncludePad(false);

        if (!wrapText) {
            builder.setMaxLines(1).setEllipsize(overflow);
        } else if (overflow != null) {
            // Если wrapText включен, обрабатываем overflow вручную
            builder.setEllipsize(overflow).setMaxLines(2);
        }

        StaticLayout staticLayout = builder.build();
        int adjustedHeight = (height > 0) ? height : staticLayout.getHeight();

        Bitmap bitmap = Bitmap.createBitmap(adjustedWidth, adjustedHeight, Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmap);
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
