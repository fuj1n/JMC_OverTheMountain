using UnityEngine;

public struct EventTilesSpawned : IEventBase
{
    public float scrollSpeed;

    public Bounds spawnBounds;

    public EventTilesSpawned(Bounds spawnBounds, float scrollSpeed)
    {
        this.spawnBounds = spawnBounds;
        this.scrollSpeed = scrollSpeed;
    }
}
