using System.Linq;
using UnityEngine;

/// <summary>
/// 게임 페이즈가 Opening이 되었을 때, 해당 그리드 셀에 있는 자식 오브젝트(Station)를 삭제
/// </summary>
public class DeleteGridCell : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Register(GameEventType.GamePhaseChanged, OnGamePhaseChanged);
    }

    private void OnDisable()
    {
        EventBus.Unregister(GameEventType.GamePhaseChanged, OnGamePhaseChanged);
    }

    private void OnGamePhaseChanged(object phaseObj)
    {
        if (phaseObj is not GamePhase phase) return;

        if (phase == GamePhase.Opening)
        {
            DeleteChildStations();
        }
    }

    /// <summary>
    /// 자식 중 Station 태그를 가진 오브젝트를 모두 삭제
    /// </summary>
    private void DeleteChildStations()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Station"))
            {
                Destroy(child.gameObject);
#if UNITY_EDITOR
                Debug.Log($"[DeleteGridCell] Station 삭제됨: {child.name}");
#endif
            }
        }
    }
    // 외부에서 확인 가능한 검사용 메서드 추가
    public bool HasStationToBeDeleted()
    {
        return GetComponentsInChildren<Transform>()
            .Any(t => t.CompareTag("Station") || t.GetComponent<IMovableStation>() != null);
    }
}
