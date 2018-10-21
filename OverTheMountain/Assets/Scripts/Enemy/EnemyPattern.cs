using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Pattern", menuName = AssetGlobals.CREATE_MENU_ROOT + "Enemy Pattern")]
public class EnemyPattern : ScriptableObject
{
    [Range(0, 25)]
    public int patternWeight = 1;
    public Enemy[] enemies;

    [System.Serializable]
    public struct Enemy
    {
        public GameObject enemyType;

        [Space]
        [Tooltip("The offset from the center that this starts at where -1 is far left and 1 is far right")]
        public Vector2 startOffset;
        public Movement[] movements;

        [System.Serializable]
        public struct Movement
        {
            public Vector2 pos;
            public float await;
        }
    }
}
