using UnityEngine;

public static class VectorUtils
{
    // Vector3 확장
    public static Vector3 ModifiedX(this Vector3 original, float targetFloat)
    {
        return new Vector3(targetFloat, original.y, original.z);
    }

    public static Vector3 ModifiedY(this Vector3 original, float targetFloat)
    {
        return new Vector3(original.x, targetFloat, original.z);
    }

    public static Vector3 ModifiedZ(this Vector3 original, float targetFloat)
    {
        return new Vector3(original.x, original.y, targetFloat);
    }

    // Vector2 확장
    public static Vector2 ModifiedX(this Vector2 original, float targetFloat)
    {
        return new Vector2(targetFloat, original.y);
    }

    public static Vector2 ModifiedY(this Vector2 original, float targetFloat)
    {
        return new Vector2(original.x, targetFloat);
    }
}
