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
        jObj["maskInteraction"] = (int)sr.maskInteraction;
        jObj["drawMode"] = (int)sr.drawMode;
        jObj["size"] = sr.size.ToJObject();
        jObj["sprite"] = SerializeReference(sprite);
        jObj["material"] = SerializeReference(material);
    }

    public override void Populate(JToken jObj, object obj)
    {
        SpriteRenderer sr = (SpriteRenderer)obj;
        sr.color = jObj["color"].ToColor();
        sr.flipX = jObj["flipX"].ToBool();
        sr.flipY = jObj["flipY"].ToBool();
        sr.sortingLayerID = jObj["sortingLayerID"].ToInt();
        sr.sortingOrder = jObj["sortingOrder"].ToInt();
        sr.maskInteraction = (SpriteMaskInteraction)jObj["maskInteraction"].ToInt();
        sr.drawMode = (SpriteDrawMode)jObj["drawMode"].ToInt();
        sr.size = jObj["size"].ToVector2();
        sr.sprite = DeserializeReference<Sprite>(jObj["sprite"], sr.sprite);
        sr.material = DeserializeReference<Material>(jObj["material"], sr.material);
    }
}
