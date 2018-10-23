using UnityEngine;

public struct EventSetWorldBounds : IEventBase
{
    public Bounds worldBounds;

    public EventSetWorldBounds(Bounds worldBounds)
    {
        this.worldBounds = worldBounds;
    }
}
