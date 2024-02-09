#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine.U2D;

namespace BeatHeroes
{
    public partial class SpriteAtlasSequencer
    {
        [Button("Configure")]
        void Configure()
        {
            forward.spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>($"Assets/{animName}/F/{animName}_F.spriteatlas");
            forwardLeft.spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>($"Assets/{animName}/FL/{animName}_FL.spriteatlas");
            forwardRight.spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>($"Assets/{animName}/FR/{animName}_FR.spriteatlas");
            back.spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>($"Assets/{animName}/B/{animName}_B.spriteatlas");
            backLeft.spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>($"Assets/{animName}/BL/{animName}_BL.spriteatlas");
            backRight.spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>($"Assets/{animName}/BR/{animName}_BR.spriteatlas");
            left.spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>($"Assets/{animName}/L/{animName}_L.spriteatlas");
            right.spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>($"Assets/{animName}/R/{animName}_R.spriteatlas");
        }
    }
}
#endif
