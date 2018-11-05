using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour, IDamageReceiver
{
    public static Player instance;

    public float speed = 10F;

    [Header("Firing")]
    public GameObject bullet;
    public float fireSpeed = .5F;
    public float bulletSpeed = 10F;

    [Header("Dash")]
    public float dashSpeed = 25F;
    public float dashTime = 5F;

    private float fireCooldown;

    private Rigidbody2D r2d;

    private Bounds worldBounds;

    private float dashTimer;
    private Vector2 dashDirection;

    private void Awake()
    {
        if (instance)
            Destroy(instance);

        instance = this;

        r2d = GetComponent<Rigidbody2D>();

        EventBus.Register(this);
    }

    private void Update()
    {
        if (fireCooldown > 0F)
            fireCooldown -= Time.deltaTime;
        if (Input.GetButton("Shoot") && fireCooldown <= 0F)
        {
            fireCooldown = fireSpeed;
            GameObject bullet = Instantiate(this.bullet, transform.position, this.bullet.transform.rotation);

            Bullet b = bullet.GetComponent<Bullet>();
            if (!b)
                b = bullet.AddComponent<Bullet>();

            b.direction = Vector3.up;
            b.speed = bulletSpeed;
            b.worldBounds = worldBounds;
            b.hitTarget = Target.ENEMY;
        }
    }

    private void LateUpdate()
    {
        if (dashTimer > 0F)
        {
            dashTimer -= Time.deltaTime;

            r2d.velocity = dashDirection * dashSpeed;

            return;
        }


        float up = Input.GetAxis("Vertical");
        float right = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Dash") && right != 0F)
        {
            dashTimer = dashTime;
            dashDirection = right > 0F ? Vector2.right : Vector2.left;
            return;
        }

        r2d.velocity = new Vector2(right, up) * speed;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log("Test " + collision.collider.tag);
        if (collision.collider.CompareTag("Bound"))
            dashTimer = 0F;
    }

    public bool OnDamage(Target target)
    {
        if (dashTimer > 0F || target != Target.PLAYER)
            return false;

        SceneManager.LoadScene(0);
        return true;
    }

    [SubscribeEvent]
    public void SetWorldBounds(EventSetWorldBounds e)
    {
        worldBounds = e.worldBounds;
    }
}
