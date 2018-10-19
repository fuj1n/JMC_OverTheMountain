using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public const string TILES_PATH = "BackgroundTiles";

    private static readonly HashSet<string> tokens = new HashSet<string>();
    private static readonly Dictionary<string, Sprite[]> sprites = new Dictionary<string, Sprite[]>();
    private static readonly Dictionary<string, float> frameRates = new Dictionary<string, float>();
    private static readonly Dictionary<string, Sprite[]> spawnables = new Dictionary<string, Sprite[]>();
    private static readonly Dictionary<string, float> rotations = new Dictionary<string, float>();

    public Vector2Int tilesCount = new Vector2Int(4, 6);
    public Vector2 tilesGap = new Vector2(10, 10);

    public float scrollSpeed = 5F;
    public float resetsAt = 20F;

    private Transform[,] tiles;

    private Transform anchor;

    private float scrollValue = 0F;

    private void Awake()
    {
        anchor = new GameObject("Anchor").transform;
        anchor.SetParent(transform, false);

        tiles = new Transform[tilesCount.x, tilesCount.y];

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                GameObject tile = new GameObject("Tile at X:" + x + " number " + y);
                tile.transform.SetParent(anchor);
                tile.AddComponent<SpriteAnimator>().Sprites = sprites.First().Value;

                tile.transform.localPosition = new Vector2(tilesGap.x * x, tilesGap.y * y);
            }
        }
    }

    private void Update()
    {
        scrollValue += scrollSpeed * Time.deltaTime;

        if (scrollValue >= resetsAt)
            scrollValue = 0;

        anchor.SetAxisPosition(Utility.Axis.Y, -scrollValue, true);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        BasicTile[] tiles = Resources.LoadAll<BasicTile>(TILES_PATH);

        foreach (BasicTile tile in tiles)
        {
            if (tokens.Contains(tile.name))
            {
                Debug.LogWarning(tile.name + " already exists, skipping...");
                continue;
            }

            tokens.Add(tile.name);

            int spriteWidth = (int)tile.sprite.rect.width;
            int spriteHeight = (int)tile.sprite.rect.height;

            if (!ValidateSprite(tile.sprite))
                continue;

            sprites[tile.name] = ChopSprite(tile.sprite);
            frameRates[tile.name] = tile.fps;

            if (tile is ConnectedTile)
            {
                ConnectedTile ct = (ConnectedTile)tile;

                Sprite[] u, d, l, r;
                if (ValidateSprite(ct.up))
                {
                    u = ChopSprite(ct.up);

                    if (ct.useRotation)
                    {
                        d = l = r = u;

                        rotations[tile.name + "$u"] = ct.rotationUp;
                        rotations[tile.name + "$d"] = ct.rotationDown;
                        rotations[tile.name + "$l"] = ct.rotationLeft;
                        rotations[tile.name + "$r"] = ct.rotationRight;
                    }
                    else
                    {
                        if (ValidateSprite(ct.down, "using UP...")) d = ChopSprite(ct.down);
                        else d = u;

                        if (ValidateSprite(ct.left, "using UP...")) l = ChopSprite(ct.left);
                        else l = u;

                        if (ValidateSprite(ct.right, "using UP...")) r = ChopSprite(ct.right);
                        else r = u;
                    }

                    sprites[tile.name + "$u"] = u;
                    sprites[tile.name + "$d"] = d;
                    sprites[tile.name + "$l"] = l;
                    sprites[tile.name + "$r"] = r;
                }
            }

            if (tile.modifiers.spawnables != null)
            {
                List<Sprite> sb = new List<Sprite>();

                foreach (BasicTile.Modifier.Spawnable spawnable in tile.modifiers.spawnables)
                {
                    if (spawnable.chop)
                    {
                        spriteWidth = (int)spawnable.sprite.rect.width;
                        spriteHeight = (int)spawnable.sprite.rect.height;
                        float cellSize = spawnable.cellSize;

                        if (spriteWidth % spawnable.cellSize != 0 || spriteHeight % spawnable.cellSize != 0)
                        {
                            Debug.LogError("Spawnable " + spawnable.name + " sprite size not evenly divisible by cell size, skipping...");
                            continue;
                        }

                        for (int x = 0; x < spriteWidth / spawnable.cellSize; x++)
                        {
                            for (int y = 0; y < spriteHeight / spawnable.cellSize; y++)
                            {
                                sb.Add(Sprite.Create(spawnable.sprite.texture, new Rect(x * cellSize, y * cellSize, cellSize, cellSize), Vector2.one / 0.5F, spawnable.sprite.pixelsPerUnit));
                                sb.Last().name = spawnable.sprite.name + " cutout " + x * spriteWidth + y;
                            }
                        }
                    }
                    else
                    {
                        sb.Add(spawnable.sprite);
                    }
                }

                spawnables[tile.name] = sb.ToArray();
            }
        }
    }

    private static Sprite[] ChopSprite(Sprite s)
    {
        int spriteWidth = (int)s.rect.width;
        int spriteHeight = (int)s.rect.height;

        Sprite[] sprites = new Sprite[spriteWidth / spriteHeight];

        // Chops up the sprite into smaller sprites for animation
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i] = Sprite.Create(s.texture, new Rect(spriteHeight * i, 0, spriteHeight, spriteHeight), Vector2.zero, s.pixelsPerUnit);
            sprites[i].name = s.name + " frame " + i;
        }

        return sprites;
    }

    private static bool ValidateSprite(Sprite s, string errorSuffix = "skipping...")
    {
        int spriteWidth = (int)s.rect.width;
        int spriteHeight = (int)s.rect.height;

        if (spriteWidth < spriteHeight)
        {
            Debug.LogWarning("Sprite " + s.name + " is higher than it is wide, " + errorSuffix);
            return false;
        }

        if (spriteWidth % spriteHeight != 0)
        {
            Debug.LogWarning("Sprite " + s.name + ": height cannot be evenly divided into width, " + errorSuffix);
            return false;
        }

        return true;
    }
}
