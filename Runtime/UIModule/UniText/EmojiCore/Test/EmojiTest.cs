using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using FreeTypeSharp;
using static FreeTypeSharp.FT;

public class EmojiTest : MonoBehaviour
{
    public RawImage targetImage;
    public string fontPath = "C:/Windows/Fonts/seguiemj.ttf";
    
    public uint emojiCodepoint = 0x1F600;
    public uint pixelSize = 109;  // Segoe UI Emoji работает лучше на определённых размерах

    unsafe void Start()
    {
        FT_LibraryRec_* lib;
        FT_FaceRec_* face;

        var error = FT_Init_FreeType(&lib);
        if (error != FT_Error.FT_Err_Ok)
        {
            Debug.LogError($"FT_Init_FreeType failed: {error}");
            return;
        }

        int major, minor, patch;
        FT_Library_Version(lib, &major, &minor, &patch);
        Debug.Log($"FreeType version: {major}.{minor}.{patch}");

        var fontPathBytes = System.Text.Encoding.UTF8.GetBytes(fontPath + '\0');
        fixed (byte* pathPtr = fontPathBytes)
        {
            error = FT_New_Face(lib, pathPtr, (IntPtr)0, &face);
        }
        
        if (error != FT_Error.FT_Err_Ok)
        {
            Debug.LogError($"FT_New_Face failed: {error}");
            FT_Done_FreeType(lib);
            return;
        }

        Debug.Log($"Num glyphs: {face->num_glyphs}, Num fixed sizes: {face->num_fixed_sizes}");

        // Для COLR шрифтов (Segoe UI Emoji) используем обычный размер
        FT_Set_Pixel_Sizes(face, 0, pixelSize);

        var glyphIndex = FT_Get_Char_Index(face, (UIntPtr)emojiCodepoint);
        if (glyphIndex == 0)
        {
            Debug.LogError("Glyph not found in font");
            FT_Done_Face(face);
            FT_Done_FreeType(lib);
            return;
        }

        Debug.Log($"Glyph index: {glyphIndex}");

        // Только FT_LOAD_COLOR, без FT_LOAD_RENDER
        error = FT_Load_Glyph(face, glyphIndex, FreeTypeSharp.FT_LOAD.FT_LOAD_COLOR);
        if (error != FT_Error.FT_Err_Ok)
        {
            Debug.LogError($"FT_Load_Glyph failed: {error}");
            FT_Done_Face(face);
            FT_Done_FreeType(lib);
            return;
        }

        Debug.Log($"Glyph format: {face->glyph->format}");

        // Теперь рендерим
        error = FT_Render_Glyph(face->glyph, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);
        if (error != FT_Error.FT_Err_Ok)
        {
            Debug.LogError($"FT_Render_Glyph failed: {error}");
            FT_Done_Face(face);
            FT_Done_FreeType(lib);
            return;
        }

        var bitmap = face->glyph->bitmap;
        int width = (int)bitmap.width;
        int rows = (int)bitmap.rows;
        var pixelMode = bitmap.pixel_mode;

        Debug.Log($"Bitmap: {width}x{rows}, pixel_mode={pixelMode}, pitch={bitmap.pitch}");

        if (width == 0 || rows == 0 || width > 10000 || rows > 10000)
        {
            Debug.LogError($"Invalid bitmap dimensions: {width}x{rows}");
            FT_Done_Face(face);
            FT_Done_FreeType(lib);
            return;
        }

        var texture = new Texture2D(width, rows, TextureFormat.RGBA32, false);
        var pixels = new Color32[width * rows];

        byte* buffer = (byte*)bitmap.buffer;
        int pitch = bitmap.pitch;

        if (pixelMode == FreeTypeSharp.FT_Pixel_Mode_.FT_PIXEL_MODE_BGRA)
        {
            Debug.Log("Color emoji!");
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcIndex = y * pitch + x * 4;
                    int dstIndex = (rows - 1 - y) * width + x;
                    
                    pixels[dstIndex] = new Color32(
                        buffer[srcIndex + 2],  // R
                        buffer[srcIndex + 1],  // G
                        buffer[srcIndex + 0],  // B
                        buffer[srcIndex + 3]   // A
                    );
                }
            }
        }
        else if (pixelMode == FreeTypeSharp.FT_Pixel_Mode_.FT_PIXEL_MODE_GRAY)
        {
            Debug.Log("Grayscale glyph");
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcIndex = y * pitch + x;
                    int dstIndex = (rows - 1 - y) * width + x;
                    byte gray = buffer[srcIndex];
                    pixels[dstIndex] = new Color32(255, 255, 255, gray);
                }
            }
        }
        else
        {
            Debug.LogError($"Unsupported pixel mode: {pixelMode}");
            FT_Done_Face(face);
            FT_Done_FreeType(lib);
            return;
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        targetImage.texture = texture;
        targetImage.SetNativeSize();

        FT_Done_Face(face);
        FT_Done_FreeType(lib);

        Debug.Log("Done!");
    }
}