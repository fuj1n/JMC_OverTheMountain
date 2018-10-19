using System;
using UnityEngine;

public static class Utility
{
    /// <summary>
    /// Sets the position on specified <paramref name="axis"/> of the <paramref name="t"/>
    /// </summary>
    /// <param name="t">The transform</param>
    /// <param name="axis">The axis to set</param>
    /// <param name="value">The value to set the axis to</param>
    /// <param name="local">Whether to do this to local position</param>
    public static void SetAxisPosition(this Transform t, Axis axis, float value, bool local = false)
    {
        Vector3 pos = local ? t.localPosition : t.position;
        if (axis.HasFlag(Axis.X))
            pos.x = value;
        if (axis.HasFlag(Axis.Y))
            pos.y = value;
        if (axis.HasFlag(Axis.Z))
            pos.z = value;

        if (local)
            t.localPosition = pos;
        else
            t.position = pos;
    }

    /// <summary>
    /// Isolates the given <paramref name="axis"/>
    /// </summary>
    /// <param name="source">Source Vector</param>
    /// <param name="axis">The axis to keep</param>
    /// <returns><paramref name="source"/>, but where the unspecified axis are removed</returns>
    public static Vector3 Isolate(this Vector3 source, Axis axis)
    {
        if (!axis.HasFlag(Axis.X))
            source.x = 0;
        if (!axis.HasFlag(Axis.Y))
            source.y = 0;
        if (!axis.HasFlag(Axis.Z))
            source.z = 0;

        return source;
    }

    /// <summary>
    /// Calls <see cref="Isolate(Vector3, Axis)"/> with an inverted <paramref name="axis"/>
    /// </summary>
    /// <param name="source">Source Vector</param>
    /// <param name="axis">The axis to remove</param>
    /// <returns><paramref name="source"/>, but where the specified axis are removed</returns>
    public static Vector3 Remove(this Vector3 source, Axis axis)
    {
        return Isolate(source, ~axis);
    }

    [Flags]
    public enum Axis
    {
        NONE = 0,
        X = 1,
        Y = 2,
        Z = 4
    }
}
