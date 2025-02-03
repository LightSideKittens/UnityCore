using System;
using LSCore.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public partial class UnityComponentConverter
{
    private static void WriteSpriteRenderer(JToken jObj, object obj, JsonSerializer serializer)
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
        jObj["sprite"] = SerializeUnityReference(sprite);
        jObj["material"] = SerializeUnityReference(material);
    }

    private static object ReadSpriteRenderer(JToken jObj, Type objectType, object existingValue, JsonSerializer serializer)
    {
        SpriteRenderer sr = (SpriteRenderer)currentComp;
        sr.color = jObj["color"].ToColor();
        sr.flipX = jObj["flipX"].ToObject<bool>();
        sr.flipY = jObj["flipY"].ToObject<bool>();
        sr.sortingLayerID = jObj["sortingLayerID"].ToObject<int>();
        sr.sortingOrder = jObj["sortingOrder"].ToObject<int>();
        sr.maskInteraction = Enum.Parse<SpriteMaskInteraction>(jObj["maskInteraction"].ToString());
        sr.drawMode = Enum.Parse<SpriteDrawMode>(jObj["drawMode"].ToString());
        sr.size = jObj["size"].ToVector2();
        sr.sprite = DeserializeUnityReference<Sprite>(jObj["sprite"], sr.sprite);
        sr.material = DeserializeUnityReference<Material>(jObj["material"], sr.material);
        return sr;
    }
}
