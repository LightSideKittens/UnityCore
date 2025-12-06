using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SetColor : BaseChange
{
    [SerializeReference] 
    [LabelText("🖼️ Image")] public IKeyGet<Image> image;
    [LabelText("🎨 Color")] public Color color;

    public override string Key => $"{base.Key}_{image.Key}";

    public override void Do(Action onComplete)
    {
        image.Data.color = color;
        base.Do(onComplete);
    }
}

[Serializable]
public class SetText : BaseChange
{
    [SerializeReference] public IKeyGet<TMP_Text> text;
    public string textString;

    public override void Do(Action onComplete)
    {
        text.Data.text = textString;
        base.Do(onComplete);
    }
}