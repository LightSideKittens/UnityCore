// EmojiTest.cs
using UnityEngine;
using UnityEngine.UI;

public class EmojiTest : MonoBehaviour
{
    public RawImage targetImage;
    public int pixelSize = 64;

    [Header("Font")]
    public string fontPath = "C:/Windows/Fonts/seguiemj.ttf";

    [Header("Test")]
    public uint testEmoji = 0x1F600;

    [Header("Background")]
    public bool transparentBackground = true;
    public Color32 backgroundColor = new Color32(40, 40, 40, 255);

    void Start()
    {
        AnalyzeFont();
    }

    void AnalyzeFont()
    {
        if (string.IsNullOrEmpty(fontPath))
        {
            Debug.LogError("[EmojiTest] Font path is empty!");
            return;
        }

        Debug.Log($"[EmojiTest] ========== ANALYZING: {fontPath} ==========");

        int numFaces = FreeTypeWrapper.GetNumFaces(fontPath);
        Debug.Log($"[EmojiTest] Total faces in file: {numFaces}");

        if (numFaces == 0)
        {
            Debug.LogError("[EmojiTest] Could not read font file!");
            return;
        }

        int workingFaceIndex = -1;

        for (int faceIdx = 0; faceIdx < numFaces; faceIdx++)
        {
            Debug.Log($"\n[EmojiTest] ----- Face {faceIdx} -----");

            if (!FreeTypeWrapper.LoadFontFromPath(fontPath, faceIdx))
            {
                Debug.LogError($"[EmojiTest] Failed to load face {faceIdx}");
                continue;
            }

            var info = FreeTypeWrapper.GetFaceInfo();

            Debug.Log($"[EmojiTest] Family: \"{info.familyName}\" Style: \"{info.styleName}\"");
            Debug.Log($"[EmojiTest] Glyphs: {info.numGlyphs}, UnitsPerEm: {info.unitsPerEm}");
            
            Debug.Log($"[EmojiTest] Flags (hex): 0x{info.faceFlags:X8}");
            Debug.Log($"[EmojiTest] hasColor: {info.hasColor}");
            Debug.Log($"[EmojiTest] hasSVG: {info.hasSVG}");
            Debug.Log($"[EmojiTest] hasSbix: {info.hasSbix}");
            Debug.Log($"[EmojiTest] hasFixedSizes: {info.hasFixedSizes}");

            if (info.hasFixedSizes && info.availableSizes != null)
            {
                Debug.Log($"[EmojiTest] Available sizes: {string.Join(", ", info.availableSizes)}");
            }

            string format = FreeTypeWrapper.GetColorFormatDescription(info);
            Debug.Log($"[EmojiTest] <color=yellow>Detected format: {format}</color>");

            uint glyphIndex = FreeTypeWrapper.GetGlyphIndex(testEmoji);

            if (glyphIndex != 0)
            {
                if (FreeTypeWrapper.TryRenderGlyph(glyphIndex, pixelSize, out var glyph, out var failReason))
                {
                    Debug.Log($"[EmojiTest] <color=green>SUCCESS!</color> Rendered {glyph.width}x{glyph.height}");
                    workingFaceIndex = faceIdx;
                }
                else
                {
                    Debug.LogWarning($"[EmojiTest] <color=red>FAILED: {failReason}</color>");
                }
            }
        }

        if (workingFaceIndex >= 0)
        {
            Debug.Log($"[EmojiTest] <color=green>Working face index: {workingFaceIndex}</color>");
            FreeTypeWrapper.LoadFontFromPath(fontPath, workingFaceIndex);
            RenderEmojiGrid();
        }
        else
        {
            Debug.LogError("[EmojiTest] No working face found!");
        }
    }

    void RenderEmojiGrid()
    {
        if (targetImage == null) return;

        uint[] gridEmoji = {
            0x1F600, 0x1F60D, 0x1F4A9, 0x1F680,
            0x1F308, 0x1F44B, 0x2764, 0x1F525
        };

        // Determine actual cell size from first emoji
        int cellSize = pixelSize;
        if (FreeTypeWrapper.TryRenderCodepoint(gridEmoji[0], pixelSize, out var sample))
        {
            cellSize = Mathf.Max(sample.width, sample.height);
            Debug.Log($"[EmojiTest] Requested: {pixelSize}px, Actual: {cellSize}px");
        }

        int cols = 4;
        int rows = 2;
        int padding = 4;

        int texWidth = cols * (cellSize + padding) + padding;
        int texHeight = rows * (cellSize + padding) + padding;

        var texture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        var pixels = new Color32[texWidth * texHeight];

        // Background
        Color32 bgColor = transparentBackground ? new Color32(0, 0, 0, 0) : backgroundColor;
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = bgColor;

        int renderedCount = 0;

        for (int idx = 0; idx < gridEmoji.Length; idx++)
        {
            int row = idx / cols;
            int col = idx % cols;
            var cp = gridEmoji[idx];

            if (FreeTypeWrapper.TryRenderCodepoint(cp, pixelSize, out var glyph))
            {
                renderedCount++;

                int cellX = padding + col * (cellSize + padding);
                int cellY = texHeight - padding - (row + 1) * (cellSize + padding) + padding;

                // Center in cell
                int offsetX = (cellSize - glyph.width) / 2;
                int offsetY = (cellSize - glyph.height) / 2;

                for (int y = 0; y < glyph.height; y++)
                {
                    for (int x = 0; x < glyph.width; x++)
                    {
                        int dstY = cellY + offsetY + (glyph.height - 1 - y);
                        int dstX = cellX + offsetX + x;

                        if (dstX >= 0 && dstX < texWidth && dstY >= 0 && dstY < texHeight)
                        {
                            int srcIdx = (y * glyph.width + x) * 4;
                            int dstIdx = dstY * texWidth + dstX;

                            byte r = glyph.rgbaPixels[srcIdx + 0];
                            byte g = glyph.rgbaPixels[srcIdx + 1];
                            byte b = glyph.rgbaPixels[srcIdx + 2];
                            byte a = glyph.rgbaPixels[srcIdx + 3];

                            if (a > 0)
                            {
                                if (transparentBackground || pixels[dstIdx].a == 0)
                                {
                                    pixels[dstIdx] = new Color32(r, g, b, a);
                                }
                                else
                                {
                                    // Alpha blend
                                    var dst = pixels[dstIdx];
                                    float alpha = a / 255f;
                                    pixels[dstIdx] = new Color32(
                                        (byte)(r * alpha + dst.r * (1 - alpha)),
                                        (byte)(g * alpha + dst.g * (1 - alpha)),
                                        (byte)(b * alpha + dst.b * (1 - alpha)),
                                        255
                                    );
                                }
                            }
                        }
                    }
                }
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        targetImage.texture = texture;
        targetImage.SetNativeSize();

        Debug.Log($"[EmojiTest] Rendered {renderedCount}/{gridEmoji.Length} emoji, texture: {texWidth}x{texHeight}");
    }
}