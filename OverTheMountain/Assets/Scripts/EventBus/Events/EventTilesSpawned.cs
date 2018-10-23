using UnityEngine;

public struct EventTilesSpawned : IEventBase
{
    public float scrollSpeed;

    public Bounds spawnBounds;
    public Bounds totalBounds;

    public EventTilesSpawned(Bounds spawnBounds, Bounds totalBounds, float scrollSpeed)
    {
        this.spawnBounds = spawnBounds;
        this.totalBounds = totalBounds;
        this.scrollSpeed = scrollSpeed;
    }
}
