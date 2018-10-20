using UnityEngine;

public sealed class EnemySpawner : MonoBehaviour
{
    public const string PATTERNS_PATH = "EnemyPatterns";

    private static EnemyPattern[] patterns;
    private static int weightSum;

    private static EnemyPattern GetRandom()
    {
        if (weightSum == 0)
        {
            Debug.LogWarning("Weights sum to 0, cannot pick a random number");
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

        foreach (EnemyPattern pattern in patterns)
        {
            weightSum += pattern.patternWeight;
        }
    }
}
