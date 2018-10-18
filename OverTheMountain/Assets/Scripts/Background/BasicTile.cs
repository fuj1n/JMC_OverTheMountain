using UnityEngine;

[CreateAssetMenu(fileName = "New Basic Tile", menuName = AssetGlobals.CREATE_MENU_TILES + "Basic Tile", order = 0)]
public class BasicTile : ScriptableObject
{
    [Header("Basic Tile")]
    public Sprite sprite;
    public float fps = 60F;

    [Header("Modifiers", order = int.MaxValue)]
    public Modifier modifiers;

    [System.Serializable]
    public struct Modifier
    {
        public Spawnable[] spawnables;

        [System.Serializable]
        public struct Spawnable
        {
            public string name;
            public Sprite sprite;

            public bool chop;
            [ConditionalHide(true, ConditionalSourceField = "chop")]
            public int cellSize;
        }
    }
}
