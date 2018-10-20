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
        public Vector2 start;
        public Movement[] movements;

        [System.Serializable]
        public struct Movement
        {
            public Vector2 pos;
            public float await;
        }
    }
}
