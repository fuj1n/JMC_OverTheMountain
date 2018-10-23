using System.Linq;
using UnityEngine;
using static EnemyPattern;

public sealed class EnemySpawner : MonoBehaviour
{
    public const string PATTERNS_PATH = "EnemyPatterns";

    [Tooltip("Enemy spawn rate by number of tiles")]
    public float spawnRate = 5F;
    [Tooltip("How much the spawn rate increases by every spawn")]
    public float spawnRateDelta = 0.02F;

    private Transform anchor;

    private static EnemyPattern[] patterns;
    private static int weightSum;

    private int currentSpawnCount = 0;

    private void Awake()
    {
        anchor = new GameObject("Enemies Anchor").transform;

        EventBus.Register(this);
    }

    [SubscribeEvent]
    public void TilesSpawned(EventTilesSpawned e)
    {
        //Debug.LogFormat("Event received {0}", e.position);

        currentSpawnCount++;
        if (currentSpawnCount >= spawnRate)
        {
            spawnRate -= spawnRateDelta;
            currentSpawnCount = 0;

            Enemy[] enemies = GetRandom().enemies;

            foreach (Enemy enemy in enemies)
            {
                GameObject go = Instantiate(enemy.enemyType, anchor);

                EnemyAI ai = go.GetComponent<EnemyAI>();
                if (!ai)
                    ai = go.AddComponent<EnemyAI>();

                ai.scrollSpeed = e.scrollSpeed;
                ai.tileBounds = e.totalBounds;
                ai.movements = enemy.movements;

                // Scale position so that (-1, -1) is the bottom-left of the leftmost tile and (1, 1) is the top-right of the rightmost tile
                // Yeah the maths ^_^
                go.transform.localPosition = (Vector2)e.spawnBounds.center + e.spawnBounds.extents * enemy.startOffset;
            }
        }
    }

    private static EnemyPattern GetRandom()
    {
        if (weightSum == 0)
        {
            Debug.LogError("Weights sum to 0, cannot pick a random number");
            return null;
        }

        int weight = Random.Range(1, weightSum);

        foreach (EnemyPattern pattern in patterns)
        {
            weight -= pattern.patternWeight;

            if (weight <= 0)
                return pattern;
        }

        return null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        patterns = Resources.LoadAll<EnemyPattern>(PATTERNS_PATH);

        // Calculate sum of all weights
        weightSum = patterns.Aggregate(0, (aggr, patt) => aggr += patt.patternWeight);
    }
}
