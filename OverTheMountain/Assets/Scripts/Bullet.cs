using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector]
    public Vector3 direction;
    [HideInInspector]
    public float speed;
    [HideInInspector]
    public Bounds worldBounds;
    [HideInInspector]
    public Target hitTarget;

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        // If we leave the world bounds, destroy
        if (!worldBounds.Contains(transform.position))
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        string tag = hitTarget == Target.PLAYER ? "Player" : "Enemy";

        if (!other.CompareTag(tag))
            return;

        //TODO deal damage

        Destroy(gameObject);
    }

    public enum Target
    {
        PLAYER,
        ENEMY
    }
}
