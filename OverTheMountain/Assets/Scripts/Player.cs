using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public static Player instance;

    public float speed = 10F;

    [Header("Firing")]
    public GameObject bullet;
    public float fireSpeed = .5F;
    public float bulletSpeed = 10F;

    private float fireCooldown;

    private Rigidbody2D r2d;

    private Bounds worldBounds;

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
            b.hitTarget = Bullet.Target.ENEMY;
        }
    }

    private void LateUpdate()
    {
        float up = Input.GetAxis("Vertical");
        float right = Input.GetAxis("Horizontal");

        r2d.velocity = new Vector2(right * speed, up * speed);
    }

    private void OnDestroy()
    {
        //SceneManager.LoadScene(0);
    }

    [SubscribeEvent]
    public void SetWorldBounds(EventSetWorldBounds e)
    {
        worldBounds = e.worldBounds;
    }
}
