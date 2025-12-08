using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
// MODIFIER INTERFACES — Different types for different pipeline stages
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Base interface for all modifiers.
/// </summary>
public interface IModifier
{
    /// <summary>
    /// Human-readable name for debugging.
    /// </summary>
    string Name { get; }
}

/// <summary>
/// Modifier applied during Itemization stage.
/// Affects font selection, text runs, shaping.
/// Examples: Bold, Italic, FontSize, FontFamily
/// </summary>
public interface IItemizeModifier : IModifier
{
    // TODO: Implement when applying modifiers
    // bool TryParseValue(ReadOnlySpan<char> rawValue, out AttributeValue value);
    // void Apply(in AttributeValue value, ref ItemizeState state);
}

/// <summary>
/// Modifier applied during Layout stage.
/// Affects glyph positioning.
/// Examples: Superscript, Subscript, BaselineOffset
/// </summary>
public interface ILayoutModifier : IModifier
{
    // TODO: Implement when applying modifiers
    // bool TryParseValue(ReadOnlySpan<char> rawValue, out AttributeValue value);
    // void Apply(in AttributeValue value, ref GlyphPosition position, in LayoutContext context);
}

/// <summary>
/// Modifier applied during Render stage.
/// Affects visual appearance only.
/// Examples: Color, Underline, Strikethrough, Outline
/// </summary>
public interface IRenderModifier : IModifier
{
    // TODO: Implement when applying modifiers
    // bool TryParseValue(ReadOnlySpan<char> rawValue, out AttributeValue value);
    // void Apply(in AttributeValue value, ref GlyphRenderData renderData);
}

// ═══════════════════════════════════════════════════════════════════════════
// STUB MODIFIERS — Placeholders that don't do anything yet
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Stub modifier for testing parsing without actual modification.
/// </summary>
public sealed class StubItemizeModifier : IItemizeModifier
{
    public string Name { get; }

    public StubItemizeModifier(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Stub modifier for testing parsing without actual modification.
/// </summary>
public sealed class StubLayoutModifier : ILayoutModifier
{
    public string Name { get; }

    public StubLayoutModifier(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Stub modifier for testing parsing without actual modification.
/// </summary>
public sealed class StubRenderModifier : IRenderModifier
{
    public string Name { get; }

    public StubRenderModifier(string name)
    {
        Name = name;
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// MODIFIER REGISTRY — Central storage for all modifiers
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Registry for all modifiers.
/// Each modifier gets a unique ID that parsers reference.
/// </summary>
public sealed class ModifierRegistry
{
    private readonly List<IModifier> modifiers = new();
    private readonly Dictionary<int, IItemizeModifier> itemizeModifiers = new();
    private readonly Dictionary<int, ILayoutModifier> layoutModifiers = new();
    private readonly Dictionary<int, IRenderModifier> renderModifiers = new();

    /// <summary>
    /// Enable debug logging.
    /// </summary>
    public bool DebugLogging { get; set; }

    /// <summary>
    /// Total number of registered modifiers.
    /// </summary>
    public int Count => modifiers.Count;

    /// <summary>
    /// Register an Itemize modifier and return its ID.
    /// </summary>
    public int Register(IItemizeModifier modifier)
    {
        int id = modifiers.Count;
        modifiers.Add(modifier);
        itemizeModifiers[id] = modifier;

        if (DebugLogging)
            Debug.Log($"[ModifierRegistry] Registered Itemize modifier '{modifier.Name}' with ID {id}");

        return id;
    }

    /// <summary>
    /// Register a Layout modifier and return its ID.
    /// </summary>
    public int Register(ILayoutModifier modifier)
    {
        int id = modifiers.Count;
        modifiers.Add(modifier);
        layoutModifiers[id] = modifier;

        if (DebugLogging)
            Debug.Log($"[ModifierRegistry] Registered Layout modifier '{modifier.Name}' with ID {id}");

        return id;
    }

    /// <summary>
    /// Register a Render modifier and return its ID.
    /// </summary>
    public int Register(IRenderModifier modifier)
    {
        int id = modifiers.Count;
        modifiers.Add(modifier);
        renderModifiers[id] = modifier;

        if (DebugLogging)
            Debug.Log($"[ModifierRegistry] Registered Render modifier '{modifier.Name}' with ID {id}");

        return id;
    }

    /// <summary>
    /// Get modifier by ID.
    /// </summary>
    public IModifier Get(int id)
    {
        if (id < 0 || id >= modifiers.Count)
            return null;
        return modifiers[id];
    }

    /// <summary>
    /// Try get Itemize modifier by ID.
    /// </summary>
    public bool TryGetItemize(int id, out IItemizeModifier modifier)
        => itemizeModifiers.TryGetValue(id, out modifier);

    /// <summary>
    /// Try get Layout modifier by ID.
    /// </summary>
    public bool TryGetLayout(int id, out ILayoutModifier modifier)
        => layoutModifiers.TryGetValue(id, out modifier);

    /// <summary>
    /// Try get Render modifier by ID.
    /// </summary>
    public bool TryGetRender(int id, out IRenderModifier modifier)
        => renderModifiers.TryGetValue(id, out modifier);

    /// <summary>
    /// Check if ID corresponds to an Itemize modifier.
    /// </summary>
    public bool IsItemizeModifier(int id) => itemizeModifiers.ContainsKey(id);

    /// <summary>
    /// Check if ID corresponds to a Layout modifier.
    /// </summary>
    public bool IsLayoutModifier(int id) => layoutModifiers.ContainsKey(id);

    /// <summary>
    /// Check if ID corresponds to a Render modifier.
    /// </summary>
    public bool IsRenderModifier(int id) => renderModifiers.ContainsKey(id);

    /// <summary>
    /// Get modifier stage as string for debugging.
    /// </summary>
    public string GetModifierStage(int id)
    {
        if (itemizeModifiers.ContainsKey(id)) return "Itemize";
        if (layoutModifiers.ContainsKey(id)) return "Layout";
        if (renderModifiers.ContainsKey(id)) return "Render";
        return "Unknown";
    }

    /// <summary>
    /// Clear all registered modifiers.
    /// </summary>
    public void Clear()
    {
        modifiers.Clear();
        itemizeModifiers.Clear();
        layoutModifiers.Clear();
        renderModifiers.Clear();
    }

    /// <summary>
    /// Log all registered modifiers.
    /// </summary>
    public void LogAllModifiers()
    {
        Debug.Log($"[ModifierRegistry] Total modifiers: {modifiers.Count}");
        for (int i = 0; i < modifiers.Count; i++)
        {
            var mod = modifiers[i];
            string stage = GetModifierStage(i);
            Debug.Log($"  [{i}] {stage}: {mod.Name}");
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// ATTRIBUTE VALUE — Union type for modifier values (for future use)
// ═══════════════════════════════════════════════════════════════════════════

/// <summary>
/// Union-like struct for attribute values. Zero-boxing.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct AttributeValue
{
    [FieldOffset(0)] public int intValue;
    [FieldOffset(0)] public float floatValue;
    [FieldOffset(0)] public Color32 colorValue;
    [FieldOffset(0)] public bool boolValue;
    [FieldOffset(4)] public float floatValue2;  // For vec2 if needed

    public static AttributeValue FromBool(bool value) => new() { boolValue = value };
    public static AttributeValue FromInt(int value) => new() { intValue = value };
    public static AttributeValue FromFloat(float value) => new() { floatValue = value };
    public static AttributeValue FromColor(Color32 value) => new() { colorValue = value };
}
