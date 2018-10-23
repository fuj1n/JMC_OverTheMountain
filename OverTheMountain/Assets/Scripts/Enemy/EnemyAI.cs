using UnityEngine;
using static EnemyPattern.Enemy;

public class EnemyAI : MonoBehaviour
{
    [HideInInspector]
    public float scrollSpeed;
    [HideInInspector]
    public Movement[] movements;
    [HideInInspector]
    public Bounds worldBounds;

    [Header("Movement")]
    public float speed = 5F;

    [Header("Shooting")]
    public GameObject bullet;
    public float shootFrequency = 2F;
    public float bulletSpeed = 10F;
    public float bulletOrbit = 0F;
    public int bulletsCount = 1;
    public float radius = 1F;
    [Range(0F, 360F)]
    public float bulletsAngle = 0F;
    [Range(0F, 180F)]
    public float angleOffset = 0F;
    public bool fireTowardsPlayer = true;

    private int currentMove = -1;
    private Vector2 moveQuota;
    private Vector2 moveDirection;
    private float nextMoveCooldown = 0F;

    private float nextShot = 0F;
    private float currentOffset = 0F;

    private void Awake()
    {
        nextShot = shootFrequency;
    }

    private void Update()
    {
        transform.localPosition += Vector3.down * scrollSpeed * Time.deltaTime;

        nextShot -= Time.deltaTime;

        if (nextShot <= 0F)
        {
            nextShot = shootFrequency;

            float angle = currentOffset;
            if (fireTowardsPlayer) // Derive angle between the enemy and the player
                angle += Mathf.Atan2(Player.instance.transform.position.y - transform.position.y, Player.instance.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
            currentOffset += angleOffset;

            // Overflow offset
            while (currentOffset > 360)
                currentOffset -= 360;
            while (currentOffset < -360)
                currentOffset += 360;

            float increment = bulletsAngle / bulletsCount;

            for (int i = 0; i < bulletsCount; i++)
            {
                float currentAngle = (angle + increment * i) * Mathf.Deg2Rad;

                GameObject bullet = Instantiate(this.bullet);
                bullet.transform.position = transform.position + new Vector3(radius * Mathf.Cos(currentAngle), radius * Mathf.Sin(currentAngle));

                Bullet b = bullet.GetComponent<Bullet>();
                if (!b)
                    b = bullet.AddComponent<Bullet>();

                b.direction = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0F);
                b.speed = bulletSpeed;
                b.worldBounds = worldBounds;
                b.hitTarget = Bullet.Target.PLAYER;
            }
        }

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
            else if (!worldBounds.Contains(transform.position))
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1F, 0F, 0F, .2F);

        Gizmos.DrawSphere(transform.position, radius);

        Gizmos.color = Color.white;

        float increment = bulletsAngle / bulletsCount;

        for (int i = 0; i < bulletsCount; i++)
        {
            float currentAngle = (angleOffset + increment * i) * Mathf.Deg2Rad;

            Gizmos.DrawWireSphere(transform.position + new Vector3(radius * Mathf.Cos(currentAngle), radius * Mathf.Sin(currentAngle)), 0.2F);
        }
    }
}
