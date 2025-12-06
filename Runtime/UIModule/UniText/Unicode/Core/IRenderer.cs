using System;
using UnityEngine;

namespace Tekst
{
    /// <summary>
    /// Данные для рендеринга одного глифа
    /// </summary>
    public struct GlyphRenderData
    {
        public int glyphId;
        public int fontId;
        
        // Позиция
        public float x;
        public float y;
        
        // Размеры
        public float width;
        public float height;
        
        // UV координаты в атласе
        public float uvX;
        public float uvY;
        public float uvWidth;
        public float uvHeight;
        
        // Атрибуты рендеринга
        public Color32 color;
        public float scale;
    }
    
    /// <summary>
    /// Результат рендеринга
    /// </summary>
    public interface IRenderResult
    {
        /// <summary>
        /// Mesh vertices
        /// </summary>
        ReadOnlySpan<Vector3> Vertices { get; }
        
        /// <summary>
        /// UV coordinates
        /// </summary>
        ReadOnlySpan<Vector2> UVs { get; }
        
        /// <summary>
        /// Vertex colors
        /// </summary>
        ReadOnlySpan<Color32> Colors { get; }
        
        /// <summary>
        /// Triangle indices
        /// </summary>
        ReadOnlySpan<int> Triangles { get; }
        
        /// <summary>
        /// Применить к Unity Mesh
        /// </summary>
        void ApplyToMesh(Mesh mesh);
    }
    
    /// <summary>
    /// Text renderer - генерирует mesh
    /// </summary>
    public interface ITextRenderer
    {
        /// <summary>
        /// Рендерить глифы
        /// </summary>
        void Render(
            ReadOnlySpan<PositionedGlyph> glyphs,
            IFontProvider fontProvider,
            IAttributeResolver attributeResolver,
            IRenderResult result);
    }
    
    /// <summary>
    /// Резолвер атрибутов - получает атрибуты по snapshot ID
    /// </summary>
    public interface IAttributeResolver
    {
        /// <summary>
        /// Получить цвет для snapshot
        /// </summary>
        Color32 GetColor(int snapshotId);
        
        /// <summary>
        /// Получить scale для snapshot
        /// </summary>
        float GetScale(int snapshotId);
        
        /// <summary>
        /// Проверить наличие underline
        /// </summary>
        bool HasUnderline(int snapshotId, out Color32 color);
        
        /// <summary>
        /// Проверить наличие strikethrough
        /// </summary>
        bool HasStrikethrough(int snapshotId, out Color32 color);
    }
}
