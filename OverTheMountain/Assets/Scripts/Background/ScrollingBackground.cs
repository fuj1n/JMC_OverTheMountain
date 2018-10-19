using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public const string TILES_PATH = "BackgroundTiles";
    public const string STARTING_TOKEN = "water";
    public const float HARD_RESET = -2000F;

    private static readonly HashSet<string> tokens = new HashSet<string>();
    private static readonly Dictionary<string, Sprite[]> sprites = new Dictionary<string, Sprite[]>();
    private static readonly Dictionary<string, float> frameRates = new Dictionary<string, float>();
    private static readonly Dictionary<string, Sprite[]> spawnables = new Dictionary<string, Sprite[]>();
    private static readonly Dictionary<string, float> rotations = new Dictionary<string, float>();

    public bool enableDebugDraw = false;

    public Vector2Int tilesCount = new Vector2Int(4, 6);
    public Vector2 tilesGap = new Vector2(10, 10);

    public float scrollSpeed = 5F;

    private Transform[][] tiles;

    private Dictionary<Transform, SpriteAnimator> animCache = new Dictionary<Transform, SpriteAnimator>();
    private Dictionary<SpriteAnimator, string> tokensCache = new Dictionary<SpriteAnimator, string>();
    private Transform anchor;

    private float scrollValue = 0F;

    private string currentToken;

    private void Awake()
    {
        currentToken = tokens.Contains(STARTING_TOKEN) ? STARTING_TOKEN : RandomToken();

        anchor = new GameObject("Anchor").transform;
        anchor.SetParent(transform, false);

        tiles = new Transform[tilesCount.x][];

        for (int x = 0; x < tilesCount.x; x++)
        {
            tiles[x] = new Transform[tilesCount.y];

            for (int y = 0; y < tilesCount.y; y++)
            {
                GameObject tile = new GameObject("Tile at X:" + x + " number " + y);
                tile.transform.SetParent(anchor);
                tiles[x][y] = tile.transform;

                animCache[tile.transform] = tile.AddComponent<SpriteAnimator>();
                ConfigureTile(x, y, currentToken, false);

                // Begin test labels
                if (enableDebugDraw)
                {
                    GameObject test = new GameObject("Test");
                    test.transform.SetParent(tile.transform, false);
                    TextMeshPro testtmp = test.AddComponent<TextMeshPro>();
                    testtmp.text = x + "," + y;
                    testtmp.alignment = TextAlignmentOptions.Center;
                    testtmp.transform.SetAxisPosition(Utility.Axis.Z, -5);
                    testtmp.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0F, tilesGap.x);
                    testtmp.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0F, tilesGap.x);
                }
                // End test labels

                tile.transform.localPosition = new Vector2(tilesGap.x * x, tilesGap.y * y);
            }
        }
    }

    private void Update()
    {
        scrollValue += scrollSpeed * Time.deltaTime;

        if (scrollValue >= Mathf.FloorToInt(tilesCount.y / 2F) * tilesGap.y)
        {
            scrollValue -= tilesGap.y;

            for (int x = 0; x < tilesCount.x; x++)
            {
                Transform newTop = tiles[x][0];
                newTop.localPosition += Vector3.up * tilesCount.y * tilesGap.y;

                for (int y = 1; y < tilesCount.y; y++)
                    tiles[x][y - 1] = tiles[x][y];

                tiles[x][tilesCount.y - 1] = newTop;

                ConfigureTile(x, tilesCount.y - 1, currentToken);
                //TODO generate
            }
        }

        anchor.localPosition += Vector3.down * scrollSpeed * Time.deltaTime;

        // Occasionally reset the offset to avoid floating point errors
        if (anchor.localPosition.y <= HARD_RESET)
        {
            anchor.localPosition += Vector3.down * HARD_RESET;

            foreach (Transform t in anchor)
                t.localPosition += Vector3.up * HARD_RESET;
        }
    }

    private void ConfigureTile(int x, int y, string token, bool vegetate = true)
    {
        Transform tile = tiles[x][y];
        SpriteAnimator san = animCache[tile];

        if (!sprites.ContainsKey(token))
        {
            Debug.LogError("No such sprite " + token);
            return;
        }

        // Kill all children of tile that are not named "Test"
        foreach (Transform child in tile)
            if (child.name != "Test")
                Destroy(child.gameObject);

        tokensCache[san] = token;

        san.Sprites = sprites[token];
        if (frameRates.ContainsKey(token))
            san.fps = frameRates[token];
        if (rotations.ContainsKey(token))
            tile.eulerAngles = Vector3.forward * rotations[token];

        //TODO connected textures

        if (vegetate && spawnables.ContainsKey(token))
        {
            for (int pass = 0; pass < 3; pass++)
            {
                if (Random.Range(0, 100) <= 1)
                    continue;

                GameObject spawnable = new GameObject("Spawnable");
                spawnable.transform.SetParent(tile, false);

                Sprite[] spawnables = ScrollingBackground.spawnables[token];
                Sprite sprite = spawnables[Random.Range(0, spawnables.Length)];
                float width = sprite.bounds.size.x;
                float height = sprite.bounds.size.y;

                spawnable.transform.localPosition = new Vector3(Random.Range(0F, tilesGap.x - width), Random.Range(0F, tilesGap.y - height), -3F);
                //spawnable.transform.eulerAngles = Vector3.forward * Random.Range(0, 360);
                spawnable.AddComponent<SpriteRenderer>().sprite = sprite;
            }
        }
    }

    private string RandomToken()
    {
        return tokens.ElementAt(Random.Range(0, tokens.Count));
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
                                sb.Add(Sprite.Create(spawnable.sprite.texture, new Rect(x * cellSize, y * cellSize, cellSize, cellSize), Vector2.zero, spawnable.sprite.pixelsPerUnit));
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
