using UnityEngine;

public struct EventTilesSpawned : IEventBase
{
    public Vector2 position;
    public float scrollSpeed;

    public EventTilesSpawned(Vector2 position, float scrollSpeed)
    {
        this.position = position;
        this.scrollSpeed = scrollSpeed;
    }
}
