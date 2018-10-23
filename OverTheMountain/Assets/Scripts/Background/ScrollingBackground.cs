using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public const string TILES_PATH = "BackgroundTiles";
    public const string STARTING_TOKEN = "Water";
    public const float HARD_RESET = -2000F;

    private static readonly HashSet<string> tokens = new HashSet<string>();
    private static readonly Dictionary<string, Sprite[]> sprites = new Dictionary<string, Sprite[]>();
    private static readonly Dictionary<string, float> frameRates = new Dictionary<string, float>();
    private static readonly Dictionary<string, Sprite[]> spawnables = new Dictionary<string, Sprite[]>();
    private static readonly Dictionary<string, float> rotations = new Dictionary<string, float>();

    private static Sprite white;

    public bool enableDebugDraw = false;

    public Vector2Int tilesCount = new Vector2Int(4, 6);
    public Vector2 tilesGap = new Vector2(10, 10);

    public float scrollSpeed = 5F;

    [Header("Generation")]
    [Range(0, 5)]
    public int vegetatePasses = 2;
    [Range(0, 100)]
    public int vegetateChance = 20;
    [Range(0, 100)]
    public int tokenSwitchChance = 50;
    [Range(1, 7)]
    public int smallestBiome = 3;

    private Transform[][] tiles;

    private readonly Dictionary<Transform, SpriteAnimator> animCache = new Dictionary<Transform, SpriteAnimator>();
    private readonly Dictionary<SpriteAnimator, string> tokensCache = new Dictionary<SpriteAnimator, string>();
    private Transform anchor;

    private float scrollValue = 0F;

    private Bounds tileBounds;

    private Queue<string[]> upcomingTokens = new Queue<string[]>();

    private string currentToken;
    private string[] currentTokens;

    private void Awake()
    {
        {
            // Weird boundary math cause I am bad at math xP
            float top = transform.position.y + Mathf.FloorToInt(tilesCount.y / 2F + (tilesCount.y % 2F == 0 ? 1 : 2)) * tilesGap.y;
            float bottom = top - (tilesCount.y + 1) * tilesGap.y;

            tileBounds = new Bounds
            {
                center = new Vector2(transform.position.x + tilesGap.x * tilesCount.x / 2F, (top + bottom) / 2F),
                size = new Vector3(tilesCount.x * tilesGap.x, tilesGap.y * (tilesCount.y + 1))
            };
        }

        currentToken = tokens.Contains(STARTING_TOKEN) ? STARTING_TOKEN : RandomToken();
        currentTokens = Enumerable.Repeat(currentToken, tilesCount.x).ToArray();
        AddQueue(false);
        AddQueue(false);

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

                // Create overlay to show the player where the starting (no generation) zone is
                GameObject tileStartOverlay = new GameObject("Start Overlay");
                tileStartOverlay.transform.SetParent(tile.transform, false);
                SpriteRenderer srSo = tileStartOverlay.AddComponent<SpriteRenderer>();
                srSo.sprite = white;
                srSo.color = new Color(1F, 0F, 0F, 0.2F);
                tileStartOverlay.transform.localPosition += Vector3.back * 2F;

                tile.transform.localPosition = new Vector2(tilesGap.x * x, tilesGap.y * y);
            }
        }
    }

    private void Update()
    {
        scrollValue += scrollSpeed * Time.deltaTime;

        bool postEvent = false;

        if (scrollValue >= Mathf.FloorToInt(tilesCount.y / 2F) * tilesGap.y)
        {
            scrollValue -= tilesGap.y;

            if (upcomingTokens.Count == 0)
            {
                AddQueue();
                AddQueue();
            }

            currentTokens = upcomingTokens.Dequeue();
            AddQueue();

            for (int x = 0; x < tilesCount.x; x++)
            {
                Transform newTop = tiles[x][0];
                newTop.localPosition += Vector3.up * tilesCount.y * tilesGap.y;

                for (int y = 1; y < tilesCount.y; y++)
                    tiles[x][y - 1] = tiles[x][y];

                tiles[x][tilesCount.y - 1] = newTop;

                ConfigureTile(x, tilesCount.y - 1, currentTokens[x]);
            }

            postEvent = true;
        }

        anchor.localPosition += Vector3.down * scrollSpeed * Time.deltaTime;

        // Occasionally reset the offset to avoid floating point errors
        if (anchor.localPosition.y <= HARD_RESET)
        {
            anchor.localPosition += Vector3.down * HARD_RESET;

            foreach (Transform t in anchor)
                t.localPosition += Vector3.up * HARD_RESET;
        }

        // Post event after everything so that the reset doesn't mess with things
        if (postEvent)
        {
            Bounds bounds = new Bounds
            {
                center = new Vector2(tileBounds.center.x, transform.position.y + (tilesCount.y / 2F + 1) * tilesGap.y),
                size = new Vector3(tileBounds.size.x, tilesGap.y)
            };

            EventBus.Post(new EventTilesSpawned(bounds, tileBounds, scrollSpeed));
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

        // Select random variation
        var variations = sprites.Select(s => s.Key).Where(s => s.Equals(token) || s.StartsWith(token + "#"));
        ConfigureAnimator(san, token, variations.ElementAt(Random.Range(0, variations.Count())));

        // Connected textures
        // Up
        if (upcomingTokens.Count > 0 && y == tilesCount.y - 1 && sprites.ContainsKey(token + "$u"))
            ConnectTextures(token, 'u', tile, upcomingTokens.Peek()[x]);

        // Down
        if (y > 0 && sprites.ContainsKey(token + "$d"))
            ConnectTextures(token, 'd', tile, tokensCache[animCache[tiles[x][y - 1]]]);

        // Left
        if (x == 0)
            ConnectTextures(token, 'l', tile, "\u00A7");
        else if (y == tilesCount.y - 1) // Only connects if at the top row
            ConnectTextures(token, 'l', tile, currentTokens[x - 1]);

        // Right
        if (x == tilesCount.x - 1)
            ConnectTextures(token, 'r', tile, "\u00A7");
        else if (y == tilesCount.y - 1) // Only connects if at the top row
            ConnectTextures(token, 'r', tile, currentTokens[x + 1]);

        // Vegetation (spawnables)
        if (vegetate && spawnables.ContainsKey(token))
        {
            for (int pass = 0; pass < vegetatePasses; pass++)
            {
                if (Random.Range(0, 100) > vegetateChance)
                    continue;

                GameObject spawnable = new GameObject("Spawnable");
                spawnable.transform.SetParent(tile, false);

                Sprite[] spawnables = ScrollingBackground.spawnables[token];
                Sprite sprite = spawnables[Random.Range(0, spawnables.Length)];
                float width = sprite.bounds.size.x;
                float height = sprite.bounds.size.y;

                spawnable.transform.localPosition = new Vector3(Random.Range(0F, tilesGap.x - width), Random.Range(0F, tilesGap.y - height), -3F + Random.value);
                // Disabled cause it doesn't make sense with the sprites I am using, upside down tree anyone?
                //spawnable.transform.eulerAngles = Vector3.forward * Random.Range(0, 360);
                spawnable.AddComponent<SpriteRenderer>().sprite = sprite;
            }
        }
    }

    private void ConnectTextures(string token, char direction, Transform tile, string other)
    {
        if (other != token)
        {
            GameObject connect = new GameObject("ct_" + direction);
            connect.transform.SetParent(tile, false);

            connect.transform.localPosition += (Vector3.up * tilesGap.y + Vector3.right * tilesGap.x) / 2F + Vector3.back;

            SpriteAnimator sanCt = connect.AddComponent<SpriteAnimator>();
            ConfigureAnimator(sanCt, token, token + "$" + direction);
        }
    }

    private void ConfigureAnimator(SpriteAnimator san, string token, string literalToken)
    {
        if (string.IsNullOrWhiteSpace(literalToken))
            literalToken = token;

        san.Sprites = sprites[literalToken];
        if (frameRates.ContainsKey(literalToken))
            san.fps = frameRates[literalToken];
        else if (frameRates.ContainsKey(token))
            san.fps = frameRates[token];
        if (rotations.ContainsKey(literalToken))
            san.transform.eulerAngles = Vector3.forward * rotations[literalToken];
        else if (rotations.ContainsKey(token))
            san.transform.eulerAngles = Vector3.forward * rotations[token];
    }

    private string RandomToken()
    {
        return tokens.ElementAt(Random.Range(0, tokens.Count));
    }

    // World generator
    private void AddQueue(bool rng = true)
    {
        if (currentTokens == null)
            currentTokens = Enumerable.Repeat(currentToken, tilesCount.x).ToArray();

        // Avoid leaking by having the queue grow larger over time
        if (upcomingTokens.Count > 2)
            return;

        if (rng && Random.Range(0, 100) < tokenSwitchChance)
        {
            string oldToken = currentToken;
            currentToken = RandomToken();

            string[] tokens = new string[tilesCount.x];
            for (int i = 0; i < tokens.Length; i++)
                tokens[i] = Random.Range(0, 2) == 0 ? oldToken : currentToken;

            upcomingTokens.Enqueue(tokens);

            for (int i = 0; i < smallestBiome; i++)
            {
                upcomingTokens.Enqueue(Enumerable.Repeat(currentToken, tilesCount.x).ToArray());
            }
        }
        else
        {
            upcomingTokens.Enqueue(Enumerable.Repeat(currentToken, tilesCount.x).ToArray());
        }
    }

    // Run when the game engine is initializing
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Generate a purely white 1x1 texture and convert it to a sprite
        Texture2D rend = new Texture2D(1, 1);
        rend.SetPixel(0, 0, Color.white);
        white = Sprite.Create(rend, new Rect(0, 0, 1, 1), Vector2.zero, .1F);
        white.name = "White";

        // Load all the tiles from "Resources/${TILES_PATH}"
        BasicTile[] tiles = Resources.LoadAll<BasicTile>(TILES_PATH);

        foreach (BasicTile tile in tiles)
        {
            if (tokens.Contains(tile.name))
            {
                Debug.LogWarning(tile.name + " already exists, skipping...");
                continue;
            }

            int spriteWidth = (int)tile.sprite.rect.width;
            int spriteHeight = (int)tile.sprite.rect.height;

            if (!ValidateSprite(tile.sprite))
                continue;

            tokens.Add(tile.name);

            sprites[tile.name] = ChopSprite(tile.sprite);
            frameRates[tile.name] = tile.fps;

            if (tile.spriteVariations != null)
            {
                for (int i = 0; i < tile.spriteVariations.Length; i++)
                {
                    if (!ValidateSprite(tile.spriteVariations[i], errorPrefix: "Sprite Variation"))
                        continue;
                    sprites[tile.name + "#" + i] = ChopSprite(tile.spriteVariations[i]);
                }
            }

            if (tile is ConnectedTile)
            {
                ConnectedTile ct = (ConnectedTile)tile;

                Sprite[] u, d, l, r;
                if (ValidateSprite(ct.up))
                {
                    u = ChopSprite(ct.up, 0.5F);

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
                        if (ValidateSprite(ct.down, errorSuffix: "using UP...")) d = ChopSprite(ct.down, 0.5F);
                        else d = u;

                        if (ValidateSprite(ct.left, errorSuffix: "using UP...")) l = ChopSprite(ct.left, 0.5F);
                        else l = u;

                        if (ValidateSprite(ct.right, errorSuffix: "using UP...")) r = ChopSprite(ct.right, 0.5F);
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

    private static Sprite[] ChopSprite(Sprite s, float pivot = 0)
    {
        int spriteWidth = (int)s.rect.width;
        int spriteHeight = (int)s.rect.height;

        Sprite[] sprites = new Sprite[spriteWidth / spriteHeight];

        // Chops up the sprite into smaller sprites for animation
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i] = Sprite.Create(s.texture, new Rect(spriteHeight * i, 0, spriteHeight, spriteHeight), Vector2.one * pivot, s.pixelsPerUnit);
            sprites[i].name = s.name + " frame " + i;
        }

        return sprites;
    }

    private static bool ValidateSprite(Sprite s, string errorPrefix = "Sprite", string errorSuffix = "skipping...")
    {
        int spriteWidth = (int)s.rect.width;
        int spriteHeight = (int)s.rect.height;

        if (spriteWidth < spriteHeight)
        {
            Debug.LogWarning(errorPrefix + s.name + " is higher than it is wide, " + errorSuffix);
            return false;
        }

        if (spriteWidth % spriteHeight != 0)
        {
            Debug.LogWarning(errorPrefix + " " + s.name + ": height cannot be evenly divided into width, " + errorSuffix);
            return false;
        }

        return true;
    }

    // Debug bounds
    private void OnDrawGizmosSelected()
    {
        // Draw the whole boundary in translucent green
        Gizmos.color = new Color(0F, 1F, 0F, 0.5F);
        Gizmos.DrawCube(tileBounds.center, tileBounds.size);
        // Draw the center in red
        Gizmos.color = Color.red;
        Gizmos.DrawCube(tileBounds.center, Vector3.up * .5F + Vector3.right * tileBounds.size.x);
    }
}
