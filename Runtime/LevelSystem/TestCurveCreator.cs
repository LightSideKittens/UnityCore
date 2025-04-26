using LSCore.LevelSystem;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class TestCurveCreator : MonoBehaviour
    {
        public Sprite sprite;
        public SpriteRenderer spriteRenderer;
        public TestCurveCreator curveCreator;
        public GameObject curvePrefab;
        public Id id;
        public LevelsManager manager;
        public bool useParallel;
    }
}