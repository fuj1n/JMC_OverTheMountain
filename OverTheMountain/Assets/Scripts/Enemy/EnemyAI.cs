using UnityEngine;
using static EnemyPattern.Enemy;

public class EnemyAI : MonoBehaviour
{
    [HideInInspector]
    public float scrollSpeed;
    [HideInInspector]
    public Movement[] movements;
    [HideInInspector]
    public Bounds tileBounds;

    public float speed = 5F;

    private int currentMove = -1;
    private Vector2 moveQuota;
    private Vector2 moveDirection;
    private float nextMoveCooldown = 0F;

    private void Update()
    {
        transform.localPosition += Vector3.down * scrollSpeed * Time.deltaTime;

        if (nextMoveCooldown > 0F)
        {
            nextMoveCooldown -= Time.deltaTime;
            return;
        }

        if ((currentMove == -1 || (moveQuota.x <= 0F && moveQuota.y <= 0F)))
        {
            currentMove++;
            if (currentMove < movements.Length)
            {
                moveQuota = movements[currentMove].move;
                moveDirection = new Vector2(movements[currentMove].move.x > 0 ? 1 : -1, movements[currentMove].move.y > 0 ? 1 : -1);
                nextMoveCooldown = movements[currentMove].await;
            }
            else if (!tileBounds.Contains(transform.position))
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Vector3 move = new Vector3(moveQuota.x > 0 ? moveDirection.x : 0, moveQuota.y > 0 ? moveDirection.y : 0) * speed * Time.deltaTime;
            transform.localPosition += move;
            moveQuota.x -= Mathf.Abs(move.x);
            moveQuota.y -= Mathf.Abs(move.y);
        }
    }
}
