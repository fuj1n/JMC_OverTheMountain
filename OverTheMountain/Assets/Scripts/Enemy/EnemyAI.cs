using UnityEngine;
using static EnemyPattern.Enemy;

public class EnemyAI : MonoBehaviour
{
    [HideInInspector]
    public float scrollSpeed;
    [HideInInspector]
    public Movement[] movements;

    private void Update()
    {
        transform.localPosition += Vector3.down * scrollSpeed * Time.deltaTime;
    }
}
