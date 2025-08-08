using UnityEngine;

/// <summary>
/// 보관 그리드에 저장된 오브젝트를 게임 페이즈에 따라 보이거나 숨김
/// - Day 페이즈일 때만 보관된 오브젝트 활성화
/// - 그 외 페이즈에서는 비활성화
/// </summary>
public class StorageGridCell : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Register(GameEventType.GamePhaseChanged, OnGamePhaseChanged);
        UpdateVisibility(GameManager.Instance.CurrentPhase); // 초기 상태 동기화
    }

    private void OnDisable()
    {
        EventBus.Unregister(GameEventType.GamePhaseChanged, OnGamePhaseChanged);
    }

    /// <summary>
    /// 게임 페이즈 변경 시 호출되는 콜백
    /// </summary>
    private void OnGamePhaseChanged(object phaseObj)
    {
        if (phaseObj is GamePhase phase)
        {
            UpdateVisibility(phase);
        }
    }

    /// <summary>
    /// 현재 보관 중인 자식 오브젝트의 활성화 상태를 변경
    /// </summary>
    private void UpdateVisibility(GamePhase phase)
    {
        Transform stored = GetStoredObject();
        if (stored == null) return;

        bool pausedReturn = phase == GamePhase.Paused;
        return;
    }

    private Transform GetStoredObject()
    {
        // Station만 대상으로 찾음
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Station"))
                return child;
        }

        return null;
    }
}
