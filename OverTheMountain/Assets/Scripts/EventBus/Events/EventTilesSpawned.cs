using UnityEngine;

public struct EventTilesSpawned : IEventBase
{
    public Vector3 position;

    public EventTilesSpawned(Vector3 position)
    {
        this.position = position;
    }
}
