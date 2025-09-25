
using UnityEngine;

public static class TransformUtils
{
/// <summary>
    /// ✅ 특정 Transform의 모든 자식 오브젝트 삭제
    /// </summary>
    public static void DestroyAllChildren(this Transform parent)
    {
        foreach (Transform child in parent)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}