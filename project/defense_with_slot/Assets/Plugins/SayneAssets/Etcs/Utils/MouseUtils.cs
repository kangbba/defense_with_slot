using UnityEngine;
using UnityEngine.EventSystems;

public static class MouseUtils
{
    /// <summary>
    /// 현재 포인터가 UI 위에 있는지 체크 (마우스/터치 지원)
    /// </summary>
    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null) 
            return false;

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return false;
#else
        return EventSystem.current.IsPointerOverGameObject();
#endif
    }

    /// <summary>
    /// 현재 마우스/터치의 월드 좌표 반환
    /// </summary>
    public static Vector2 GetPointerWorldPosition(Camera cam = null)
    {
        cam ??= Camera.main;
        return cam.ScreenToWorldPoint(Input.mousePosition);
    }
}
