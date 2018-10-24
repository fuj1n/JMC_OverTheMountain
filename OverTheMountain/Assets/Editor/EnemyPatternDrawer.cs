using UnityEditor;
using UnityEngine;
using static EnemyPattern;

[CustomPropertyDrawer(typeof(PreviewRender))]
public class EnemyPatternDrawer : PropertyDrawer
{
    public const float PREVIEW_WIDTH = .85F;
    public static readonly Color PREVIEW_BACKGROUND = new Color(0F, 1F, 0F, .1F);
    public static readonly Color PREVIEW_DOT_COLOR = Color.red;
    public const float DOT_SIZE = 8F;
    public const int LINES = 10;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PrefixLabel(position, label);

        position.y += EditorGUIUtility.singleLineHeight;

        Rect rect = new Rect()
        {
            size = new Vector2(position.width *= PREVIEW_WIDTH, EditorGUIUtility.singleLineHeight * LINES),
            position = new Vector2(position.position.x + position.width * (1 - PREVIEW_WIDTH) / 2, position.position.y)
        };

        EditorGUI.DrawRect(rect, PREVIEW_BACKGROUND);

        SerializedProperty enemiesArray = property.serializedObject.FindProperty("enemies");

        for (int i = 0; i < enemiesArray.arraySize; i++)
        {
            Vector2 enemyPos = enemiesArray.GetArrayElementAtIndex(i).FindPropertyRelative("startOffset").vector2Value;
            enemyPos.y *= -1;

            Rect enemyRect = new Rect()
            {
                size = Vector2.one * DOT_SIZE,
                center = rect.center + (rect.size / 2F) * enemyPos
            };
            EditorGUI.DrawRect(enemyRect, PREVIEW_DOT_COLOR);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight * LINES;
    }
}
