using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerArea : MonoBehaviour
{
    public virtual float getSurfaceArea()
    {
        Debug.LogError("getSurfaceArea is not implemented");
        return float.NaN;
    }

    public virtual Vector2 getRandomPositionWithin(float seed)
    {
        Debug.LogError("getRandomPositionWithin is not implemented");
        return Vector2.zero;
    }
}
