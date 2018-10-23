using UnityEngine;

public struct EventTilesSpawned : IEventBase
{
    public float scrollSpeed;

    public Bounds spawnBounds;

    public EventTilesSpawned(Bounds bounds, float scrollSpeed)
    {
        spawnBounds = bounds;
        this.scrollSpeed = scrollSpeed;
    }
}
