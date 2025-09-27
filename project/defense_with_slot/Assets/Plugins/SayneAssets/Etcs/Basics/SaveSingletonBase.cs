// SaveSingletonMono.cs
using System;
using UnityEngine;
// ✅ (1) 마커용 비제네릭 베이스 클래스 이름 변경
/// <summary>
/// SaveData를 바인딩/언바인딩할 수 있는 매니저의 최소 인터페이스.
/// SaveSingletonMono<T>가 기본 구현을 제공.
/// </summary>
public abstract class SaveSingletonBase : MonoBehaviour
{
    public abstract void BindSaveData(SaveData data);
    public abstract void UnbindSaveData();
}
