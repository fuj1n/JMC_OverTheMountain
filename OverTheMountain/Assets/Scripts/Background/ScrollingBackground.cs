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

    [RuntimeInitializeOnLoadMethod]
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

            if (spriteWidth < spriteHeight)
            {
                Debug.LogWarning("Sprite " + tile.name + " is higher than it is wide, skipping...");
                continue;
            }

            if (spriteWidth % spriteHeight != 0)
            {
                Debug.LogWarning("Sprite " + tile.name + " height cannot be evenly divided into width, skipping...");
                continue;
            }

            sprites[tile.name] = ChopSprite(tile.sprite);
            frameRates[tile.name] = tile.fps;

            if (tile is ConnectedTile)
            {
                ConnectedTile ct = (ConnectedTile)tile;

                Sprite[] u, d, l, r;
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
                    d = ChopSprite(ct.down);
                    l = ChopSprite(ct.left);
                    r = ChopSprite(ct.right);
                }

                sprites[tile.name + "$u"] = u;
                sprites[tile.name + "$d"] = d;
                sprites[tile.name + "$l"] = l;
                sprites[tile.name + "$r"] = r;
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

        //for (int i = 0; i < sprites.First().Value.Length; i++)
        //{
        //    GameObject g = new GameObject("Test" + i);
        //    g.AddComponent<SpriteRenderer>().sprite = sprites.First().Value[i];
        //    g.transform.Translate(Vector3.right * i * 10 + Vector3.forward * 10);
        //}

        new GameObject("Test").AddComponent<SpriteAnimator>().Sprites = sprites.First().Value;
    }

    private static Sprite[] ChopSprite(Sprite s)
    {
        int spriteWidth = (int)s.rect.width;
        int spriteHeight = (int)s.rect.height;

        Sprite[] sprites = new Sprite[spriteWidth / spriteHeight];

        // Chops up the sprite into smaller sprites for animation
        for (int i = 0; i < sprites.Length; i++)
            sprites[i] = Sprite.Create(s.texture, new Rect(spriteHeight * i, 0, spriteHeight, spriteHeight), Vector3.one * 0.5F, s.pixelsPerUnit);

        return sprites;
    }
}
