using System;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class SpriteRendererConverter : BaseUnityObjectConverter
{
    public SpriteRendererConverter(UnityObjectReferenceConverter referenceConverter) : base(referenceConverter) { }

    protected override void OnSerialize(JToken jObj, object obj)
    {
        SpriteRenderer sr = (SpriteRenderer)obj;
        var sprite = sr.sprite;
        var material = sr.material;
        jObj["color"] = sr.color.ToJObject();
        jObj["flipX"] = sr.flipX;
        jObj["flipY"] = sr.flipY;
        jObj["sortingLayerID"] = sr.sortingLayerID;
        jObj["sortingOrder"] = sr.sortingOrder;
        jObj["maskInteraction"] = sr.maskInteraction.ToString();
        jObj["drawMode"] = sr.drawMode.ToString();
        jObj["size"] = sr.size.ToJObject();
        jObj["sprite"] = SerializeReference(sprite);
        jObj["material"] = SerializeReference(material);
    }

    public override void Populate(JToken jObj, object obj)
    {
        SpriteRenderer sr = (SpriteRenderer)obj;
        sr.color = jObj["color"].ToColor();
        sr.flipX = jObj["flipX"].ToObject<bool>();
        sr.flipY = jObj["flipY"].ToObject<bool>();
        sr.sortingLayerID = jObj["sortingLayerID"].ToObject<int>();
        sr.sortingOrder = jObj["sortingOrder"].ToObject<int>();
        sr.maskInteraction = Enum.Parse<SpriteMaskInteraction>(jObj["maskInteraction"].ToString());
        sr.drawMode = Enum.Parse<SpriteDrawMode>(jObj["drawMode"].ToString());
        sr.size = jObj["size"].ToVector2();
        sr.sprite = DeserializeReference<Sprite>(jObj["sprite"], sr.sprite);
        sr.material = DeserializeReference<Material>(jObj["material"], sr.material);
    }
}
