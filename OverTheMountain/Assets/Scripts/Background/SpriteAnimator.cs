using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    public Sprite[] Sprites
    {
        get
        {
            return sprites;
        }
        set
        {
            sprites = value;
            frame = 0;

            UpdateSprite();
        }
    }

    public float fps = 15F;

    public int frame = 0;

    [SerializeField]
    private Sprite[] sprites;

    private new SpriteRenderer renderer;

    private float seconds = 0F;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        if (!renderer)
            renderer = gameObject.AddComponent<SpriteRenderer>();

        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (sprites == null || sprites.Length == 0)
        {
            renderer.sprite = null;
            return;
        }

        if (frame >= sprites.Length)
            frame = 0;

        renderer.sprite = sprites[frame];
    }

    private void Update()
    {
        if (fps == 0)
            return;

        seconds += Time.deltaTime;

        if (seconds >= 1 / fps)
        {
            seconds = 0;
            frame++;
            UpdateSprite();
        }
    }
}
