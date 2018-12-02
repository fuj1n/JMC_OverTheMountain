using UnityEditor;
using UnityEngine;
using static EnemyPattern;

[CustomPropertyDrawer(typeof(PreviewRender))]
public class EnemyPatternDrawer : PropertyDrawer
{
    public const float PREVIEW_RATIO = 16F / 9F;
    public const float PREVIEW_WIDTH = .85F;
    public const float Y_OFFSET = 45F;
    public static readonly Color PREVIEW_BACKGROUND = new Color(0F, 1F, 0F, .1F);
    public static readonly Color PREVIEW_DOT_COLOR = Color.red;
    public static readonly Color PATH_COLOR = Color.white;

    public const float DOT_SIZE = 8F;

    private float cachedWidth;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        System.Random random = new System.Random(25565);

        EditorGUI.PrefixLabel(position, label);

        position.y += EditorGUIUtility.singleLineHeight;

        cachedWidth = position.x + position.width * (1 - PREVIEW_WIDTH) / 2;

        if (Event.current.type != EventType.Repaint)
            return;

        position.height = position.width / PREVIEW_RATIO;

        Rect rect = new Rect()
        {
            size = new Vector2(position.width *= PREVIEW_WIDTH, position.width / PREVIEW_RATIO),
            position = new Vector2(position.x += position.width * (1 - PREVIEW_WIDTH) / 2, position.position.y + Y_OFFSET)
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

            SerializedProperty movementsArray = enemiesArray.GetArrayElementAtIndex(i).FindPropertyRelative("movements");

            if (movementsArray.arraySize == 0)
                continue;

            Vector2 last = enemyRect.center;

            GL.Begin(GL.LINE_STRIP);
            GL.Color(new Color(random.Next(50, 255) / 255F, random.Next(50, 255) / 255F, random.Next(50, 255) / 255F));
            GL.Vertex(enemyRect.center);

            for (int j = 0; j < movementsArray.arraySize; j++)
            {
                Vector2 move = movementsArray.GetArrayElementAtIndex(j).FindPropertyRelative("move").vector2Value;
                move *= rect.width * .025F;
                move.y *= -1;
                Vector2 newPos = last + move;

                GL.Vertex(newPos);

                last = newPos;
            }
            GL.End();

        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + cachedWidth / PREVIEW_RATIO + Y_OFFSET * 4;
    }
}
