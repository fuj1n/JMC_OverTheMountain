using UnityEngine;

[CreateAssetMenu(fileName = "New Connected Tile", menuName = AssetGlobals.CREATE_MENU_TILES + "Connected Tile", order = 1)]
public class ConnectedTile : BasicTile
{
    [Header("Connected Tile")]
    public bool useRotation = true;

    [ConditionalRename(ConditionalSourceField = "useRotation", Name = "Connected Sprite")]
    public Sprite up;
    [ConditionalHide(HideInInspector = true, ConditionalSourceField = "useRotation", Inverse = true)]
    public Sprite down;
    [ConditionalHide(HideInInspector = true, ConditionalSourceField = "useRotation", Inverse = true)]
    public Sprite left;
    [ConditionalHide(HideInInspector = true, ConditionalSourceField = "useRotation", Inverse = true)]
    public Sprite right;

    [ConditionalHide(HideInInspector = true, ConditionalSourceField = "useRotation")]
    public float rotationUp = 0F;
    [ConditionalHide(HideInInspector = true, ConditionalSourceField = "useRotation")]
    public float rotationDown = 180F;
    [ConditionalHide(HideInInspector = true, ConditionalSourceField = "useRotation")]
    public float rotationLeft = 90F;
    [ConditionalHide(HideInInspector = true, ConditionalSourceField = "useRotation")]
    public float rotationRight = 270F;
}
