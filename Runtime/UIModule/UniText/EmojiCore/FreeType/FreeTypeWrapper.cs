// FreeTypeWrapper.cs
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using FreeTypeSharp;
using static FreeTypeSharp.FT;

public static unsafe class FreeTypeWrapper
{
    // Face flags from FreeType (freetype.h)
    private const long FT_FACE_FLAG_SCALABLE         = 1L << 0;
    private const long FT_FACE_FLAG_FIXED_SIZES      = 1L << 1;
    private const long FT_FACE_FLAG_FIXED_WIDTH      = 1L << 2;
    private const long FT_FACE_FLAG_SFNT             = 1L << 3;
    private const long FT_FACE_FLAG_HORIZONTAL       = 1L << 4;
    private const long FT_FACE_FLAG_VERTICAL         = 1L << 5;
    private const long FT_FACE_FLAG_KERNING          = 1L << 6;
    private const long FT_FACE_FLAG_FAST_GLYPHS      = 1L << 7;
    private const long FT_FACE_FLAG_MULTIPLE_MASTERS = 1L << 8;
    private const long FT_FACE_FLAG_GLYPH_NAMES      = 1L << 9;
    private const long FT_FACE_FLAG_EXTERNAL_STREAM  = 1L << 10;
    private const long FT_FACE_FLAG_HINTER           = 1L << 11;
    private const long FT_FACE_FLAG_CID_KEYED        = 1L << 12;
    private const long FT_FACE_FLAG_TRICKY           = 1L << 13;
    private const long FT_FACE_FLAG_COLOR            = 1L << 14;
    private const long FT_FACE_FLAG_VARIATION        = 1L << 15;
    private const long FT_FACE_FLAG_SVG              = 1L << 16;
    private const long FT_FACE_FLAG_SBIX             = 1L << 17;
    private const long FT_FACE_FLAG_SBIX_OVERLAY     = 1L << 18;

    public struct FaceInfo
    {
        public bool isValid;
        
        // Capabilities
        public bool hasColor;
        public bool hasFixedSizes;
        public bool hasSVG;
        public bool hasSbix;
        public bool hasSbixOverlay;
        public bool isScalable;
        public bool isSfnt;
        
        // Sizes
        public int numFixedSizes;
        public int[] availableSizes;
        
        // General info
        public int unitsPerEm;
        public int numGlyphs;
        public int numFaces;
        public int faceIndex;
        public string familyName;
        public string styleName;
        
        // Raw flags for debugging
        public long faceFlags;
    }

    public struct RenderedGlyph
    {
        public bool isValid;
        public int width;
        public int height;
        public int bearingX;
        public int bearingY;
        public float advanceX;
        public float advanceY;
        public byte[] rgbaPixels;
    }

    private static FT_LibraryRec_* library;
    private static FT_FaceRec_* currentFace;
    private static bool isInitialized;
    private static byte[] currentFontData;
    private static GCHandle fontDataHandle;
    private static int currentPixelSize;
    private static int currentFaceIndex;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        Dispose();
    }

    public static bool Initialize()
    {
        if (isInitialized) return true;

        FT_LibraryRec_* lib;
        var error = FT_Init_FreeType(&lib);
        if (error != FT_Error.FT_Err_Ok)
        {
            Debug.LogError($"[FreeTypeWrapper] FT_Init_FreeType failed: {error}");
            return false;
        }

        library = lib;
        isInitialized = true;

        int major, minor, patch;
        FT_Library_Version(library, &major, &minor, &patch);
        Debug.Log($"[FreeTypeWrapper] FreeType {major}.{minor}.{patch} initialized");

        return true;
    }

    public static int GetNumFaces(string fontPath)
    {
        if (!Initialize()) return 0;
        if (!File.Exists(fontPath)) return 0;

        FT_FaceRec_* face;
        var pathBytes = System.Text.Encoding.UTF8.GetBytes(fontPath + '\0');

        fixed (byte* pathPtr = pathBytes)
        {
            var error = FT_New_Face(library, pathPtr, (IntPtr)0, &face);
            if (error != FT_Error.FT_Err_Ok) return 0;
        }

        int numFaces = (int)face->num_faces;
        FT_Done_Face(face);
        return numFaces;
    }

    public static bool LoadFontFromPath(string fontPath, int faceIndex = 0)
    {
        if (!Initialize()) return false;
        DisposeFace();

        if (!File.Exists(fontPath))
        {
            Debug.LogError($"[FreeTypeWrapper] Font file not found: {fontPath}");
            return false;
        }

        FT_FaceRec_* face;
        var pathBytes = System.Text.Encoding.UTF8.GetBytes(fontPath + '\0');

        fixed (byte* pathPtr = pathBytes)
        {
            var error = FT_New_Face(library, pathPtr, (IntPtr)faceIndex, &face);
            if (error != FT_Error.FT_Err_Ok)
            {
                Debug.LogError($"[FreeTypeWrapper] FT_New_Face failed for face {faceIndex}: {error}");
                return false;
            }
        }

        currentFace = face;
        currentFaceIndex = faceIndex;
        currentPixelSize = 0;

        var familyName = Marshal.PtrToStringAnsi((IntPtr)face->family_name) ?? "Unknown";
        var styleName = Marshal.PtrToStringAnsi((IntPtr)face->style_name) ?? "";
        Debug.Log($"[FreeTypeWrapper] Loaded face {faceIndex}/{face->num_faces}: \"{familyName}\" {styleName}");

        return true;
    }

    public static bool LoadFontFromData(byte[] fontData, int faceIndex = 0)
    {
        if (!Initialize()) return false;
        DisposeFace();

        if (fontData == null || fontData.Length == 0)
        {
            Debug.LogError("[FreeTypeWrapper] Font data is empty");
            return false;
        }

        currentFontData = fontData;
        fontDataHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);

        FT_FaceRec_* face;
        var dataPtr = fontDataHandle.AddrOfPinnedObject();

        var error = FT_New_Memory_Face(library, (byte*)dataPtr, (IntPtr)fontData.Length, (IntPtr)faceIndex, &face);
        if (error != FT_Error.FT_Err_Ok)
        {
            fontDataHandle.Free();
            currentFontData = null;
            Debug.LogError($"[FreeTypeWrapper] FT_New_Memory_Face failed: {error}");
            return false;
        }

        currentFace = face;
        currentFaceIndex = faceIndex;
        currentPixelSize = 0;
        return true;
    }

    public static FaceInfo GetFaceInfo()
    {
        var info = new FaceInfo { isValid = false };
        if (currentFace == null) return info;

        info.isValid = true;
        info.numGlyphs = (int)currentFace->num_glyphs;
        info.numFaces = (int)currentFace->num_faces;
        info.faceIndex = currentFaceIndex;
        info.unitsPerEm = currentFace->units_per_EM > 0 ? currentFace->units_per_EM : 1000;

        info.familyName = Marshal.PtrToStringAnsi((IntPtr)currentFace->family_name) ?? "Unknown";
        info.styleName = Marshal.PtrToStringAnsi((IntPtr)currentFace->style_name) ?? "";

        info.numFixedSizes = currentFace->num_fixed_sizes;
        info.hasFixedSizes = info.numFixedSizes > 0;

        if (info.hasFixedSizes)
        {
            info.availableSizes = new int[info.numFixedSizes];
            for (int i = 0; i < info.numFixedSizes; i++)
            {
                info.availableSizes[i] = currentFace->available_sizes[i].height;
            }
        }

        // Get raw flags
        info.faceFlags = (long)currentFace->face_flags;
        
        // Parse flags
        info.hasColor      = (info.faceFlags & FT_FACE_FLAG_COLOR) != 0;
        info.hasSVG        = (info.faceFlags & FT_FACE_FLAG_SVG) != 0;
        info.hasSbix       = (info.faceFlags & FT_FACE_FLAG_SBIX) != 0;
        info.hasSbixOverlay = (info.faceFlags & FT_FACE_FLAG_SBIX_OVERLAY) != 0;
        info.isScalable    = (info.faceFlags & FT_FACE_FLAG_SCALABLE) != 0;
        info.isSfnt        = (info.faceFlags & FT_FACE_FLAG_SFNT) != 0;

        return info;
    }

    /// <summary>
    /// Determines the color format type of the font
    /// </summary>
    public static string GetColorFormatDescription(FaceInfo info)
    {
        var formats = new System.Collections.Generic.List<string>();
        
        if (info.hasSVG)
            formats.Add("SVG (requires external renderer)");
        
        if (info.hasSbix)
            formats.Add("SBIX (Apple PNG bitmaps - requires libpng!)");
        
        if (info.hasColor && !info.hasSVG && !info.hasSbix)
        {
            // If hasColor but not SVG or SBIX, it's likely COLR or CBDT
            if (info.hasFixedSizes)
                formats.Add("CBDT/CBLC (Google PNG bitmaps)");
            else
                formats.Add("COLR/CPAL (vector layers)");
        }
        
        if (formats.Count == 0)
        {
            if (info.hasFixedSizes)
                formats.Add("Bitmap (no color)");
            else
                formats.Add("Outline only (no color)");
        }
        
        return string.Join(" + ", formats);
    }

    public static uint GetGlyphIndex(uint codepoint)
    {
        if (currentFace == null) return 0;
        return FT_Get_Char_Index(currentFace, (UIntPtr)codepoint);
    }

    public static bool HasGlyph(uint codepoint)
    {
        return GetGlyphIndex(codepoint) != 0;
    }

    public static bool SetPixelSize(int size)
    {
        if (currentFace == null) return false;
        if (currentPixelSize == size) return true;

        var info = GetFaceInfo();

        if (info.hasFixedSizes && info.availableSizes != null && info.availableSizes.Length > 0)
        {
            int bestIndex = 0;
            int bestDiff = int.MaxValue;

            for (int i = 0; i < info.numFixedSizes; i++)
            {
                int diff = Math.Abs(info.availableSizes[i] - size);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    bestIndex = i;
                }
            }

            var error = FT_Select_Size(currentFace, bestIndex);
            if (error != FT_Error.FT_Err_Ok)
            {
                Debug.LogError($"[FreeTypeWrapper] FT_Select_Size failed: {error}");
                return false;
            }

            currentPixelSize = info.availableSizes[bestIndex];
        }
        else
        {
            var error = FT_Set_Pixel_Sizes(currentFace, 0, (uint)size);
            if (error != FT_Error.FT_Err_Ok)
            {
                Debug.LogError($"[FreeTypeWrapper] FT_Set_Pixel_Sizes failed: {error}");
                return false;
            }

            currentPixelSize = size;
        }

        return true;
    }

    public static int GetCurrentPixelSize() => currentPixelSize;

    public static bool TryRenderGlyph(uint glyphIndex, int targetSize, out RenderedGlyph result)
    {
        return TryRenderGlyph(glyphIndex, targetSize, out result, out _);
    }

    public static bool TryRenderGlyph(uint glyphIndex, int targetSize, out RenderedGlyph result, out string failReason)
    {
        result = new RenderedGlyph { isValid = false };
        failReason = null;

        if (currentFace == null)
        {
            failReason = "No face loaded";
            return false;
        }

        if (glyphIndex == 0)
        {
            failReason = "Glyph index is 0";
            return false;
        }

        if (!SetPixelSize(targetSize))
        {
            failReason = "Failed to set pixel size";
            return false;
        }

        var info = GetFaceInfo();

        // Try loading strategies in order
        FT_Error loadError = FT_Error.FT_Err_Ok;
        bool loaded = false;

        // Strategy 1: COLOR + RENDER (best for bitmap color fonts)
        if (!loaded)
        {
            loadError = FT_Load_Glyph(currentFace, glyphIndex,
                FT_LOAD.FT_LOAD_COLOR | FT_LOAD.FT_LOAD_RENDER);
            loaded = (loadError == FT_Error.FT_Err_Ok);
        }

        // Strategy 2: COLOR only (for COLR vector fonts)
        if (!loaded)
        {
            loadError = FT_Load_Glyph(currentFace, glyphIndex, FT_LOAD.FT_LOAD_COLOR);
            loaded = (loadError == FT_Error.FT_Err_Ok);
        }

        // Strategy 3: RENDER only
        if (!loaded)
        {
            loadError = FT_Load_Glyph(currentFace, glyphIndex, FT_LOAD.FT_LOAD_RENDER);
            loaded = (loadError == FT_Error.FT_Err_Ok);
        }

        // Strategy 4: DEFAULT
        if (!loaded)
        {
            loadError = FT_Load_Glyph(currentFace, glyphIndex, FT_LOAD.FT_LOAD_DEFAULT);
            loaded = (loadError == FT_Error.FT_Err_Ok);
        }

        if (!loaded)
        {
            failReason = $"FT_Load_Glyph failed: {loadError}";

            // Provide detailed hint for common errors
            if (loadError == FT_Error.FT_Err_Unimplemented_Feature)
            {
                if (info.hasSbix)
                    failReason += "\n  → SBIX format detected! FreeType needs libpng to render sbix.";
                else if (info.hasSVG)
                    failReason += "\n  → SVG format detected. FreeType needs external SVG renderer.";
                else
                    failReason += "\n  → Unsupported color format.";
                
                failReason += "\n  → Solution: Rebuild FreeType with libpng support (see BUILD_INSTRUCTIONS.md)";
            }

            return false;
        }

        // Render if needed (for outline formats)
        if (currentFace->glyph->format == FT_Glyph_Format_.FT_GLYPH_FORMAT_OUTLINE)
        {
            var renderError = FT_Render_Glyph(currentFace->glyph, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);
            if (renderError != FT_Error.FT_Err_Ok)
            {
                failReason = $"FT_Render_Glyph failed: {renderError}";
                return false;
            }
        }

        var glyph = currentFace->glyph;
        var bitmap = glyph->bitmap;

        int width = (int)bitmap.width;
        int height = (int)bitmap.rows;

        if (width <= 0 || height <= 0)
        {
            failReason = $"Empty bitmap: {width}x{height}, format={glyph->format}, pixel_mode={bitmap.pixel_mode}";
            return false;
        }

        if (width > 4096 || height > 4096)
        {
            failReason = $"Bitmap too large: {width}x{height}";
            return false;
        }

        result.width = width;
        result.height = height;
        result.bearingX = glyph->bitmap_left;
        result.bearingY = glyph->bitmap_top;
        result.advanceX = (long)glyph->advance.x / 64f;
        result.advanceY = (long)glyph->advance.y / 64f;

        int pitch = bitmap.pitch;
        byte* buffer = (byte*)bitmap.buffer;

        result.rgbaPixels = new byte[width * height * 4];

        var pixelMode = bitmap.pixel_mode;

        if (pixelMode == FT_Pixel_Mode_.FT_PIXEL_MODE_BGRA)
        {
            // Color emoji - BGRA to RGBA
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcIndex = y * pitch + x * 4;
                    int dstIndex = (y * width + x) * 4;

                    result.rgbaPixels[dstIndex + 0] = buffer[srcIndex + 2]; // R
                    result.rgbaPixels[dstIndex + 1] = buffer[srcIndex + 1]; // G
                    result.rgbaPixels[dstIndex + 2] = buffer[srcIndex + 0]; // B
                    result.rgbaPixels[dstIndex + 3] = buffer[srcIndex + 3]; // A
                }
            }
        }
        else if (pixelMode == FT_Pixel_Mode_.FT_PIXEL_MODE_GRAY)
        {
            // Grayscale
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcIndex = y * pitch + x;
                    int dstIndex = (y * width + x) * 4;
                    byte gray = buffer[srcIndex];

                    result.rgbaPixels[dstIndex + 0] = 255;
                    result.rgbaPixels[dstIndex + 1] = 255;
                    result.rgbaPixels[dstIndex + 2] = 255;
                    result.rgbaPixels[dstIndex + 3] = gray;
                }
            }
        }
        else
        {
            failReason = $"Unsupported pixel mode: {pixelMode}";
            return false;
        }

        result.isValid = true;
        return true;
    }

    public static bool TryRenderCodepoint(uint codepoint, int targetSize, out RenderedGlyph result)
    {
        result = new RenderedGlyph { isValid = false };

        var glyphIndex = GetGlyphIndex(codepoint);
        if (glyphIndex == 0) return false;

        return TryRenderGlyph(glyphIndex, targetSize, out result);
    }

    private static void DisposeFace()
    {
        if (currentFace != null)
        {
            FT_Done_Face(currentFace);
            currentFace = null;
        }

        if (fontDataHandle.IsAllocated)
        {
            fontDataHandle.Free();
        }
        currentFontData = null;
        currentPixelSize = 0;
        currentFaceIndex = 0;
    }

    public static void Dispose()
    {
        DisposeFace();

        if (isInitialized && library != null)
        {
            FT_Done_FreeType(library);
            library = null;
            isInitialized = false;
        }
    }
}