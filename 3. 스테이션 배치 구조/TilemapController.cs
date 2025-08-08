using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 에디터에서 직접 배치한 GridCell들을 자동으로 탐색하고 관리
/// 자식 오브젝트 중 GridCell 태그를 가진 것만 인식
/// 처음에는 셀을 숨기고, 버튼 클릭 시 보이게 설정 가능
/// </summary>
public class TilemapController : MonoBehaviour
{
    public List<GameObject> gridCells = new List<GameObject>();

    [SerializeField] protected bool showDebugInfo;

    [SerializeField] private Material baseMaterial;
    [SerializeField] private Material placeableMaterial;
    [SerializeField] private Material notPlaceableMaterial;
    [SerializeField] private Material selectMaterial;

    private GameObject selectedCell;

    private void Awake()
    {
        FindGridCells();
        HideAllCells();                               // 시작 시 셀 숨김
    }



    private void Update()
    {
        UpdateGridCellStates();
    }

    /// <summary>
    /// 자식 오브젝트 중 GridCell 태그를 가진 오브젝트를 모두 수집
    /// </summary>
    public void FindGridCells()
    {
        gridCells.Clear();

        foreach (Transform child in GetComponentsInChildren<Transform>(true)) // true = 비활성화 포함
        {
            if (child.CompareTag("GridCell"))
            {
                gridCells.Add(child.gameObject);
            }
        }

        if (showDebugInfo) Debug.Log($"[TilemapController] GridCell {gridCells.Count}개를 찾았습니다.");
    }

    private void Start()
    {
        {
            StartCoroutine(WaitAndConnect());
        }
    }

    private IEnumerator WaitAndConnect()
    {
        while (StationManager.Instance == null)
        {
            yield return null; // 1프레임 대기
        }

        StationManager.Instance.SetTilemapController(this);
        if (showDebugInfo) Debug.Log("[TilemapController] StationManager 연결 완료!");
    }

    /// <summary>
    /// 모든 셀의 SpriteRenderer를 비활성화하여 셀을 숨김
    /// </summary>
    public void HideAllCells()
    {
        foreach (var cell in gridCells)
        {
            // SpriteRenderer 비활성화
            var sr = cell.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;

                // baseMaterial로 원복
                if (baseMaterial != null)
                    sr.material = baseMaterial;
            }
        }
    }


    /// <summary>
    /// 모든 셀의 SpriteRenderer를 활성화하고 상태에 따른 머티리얼을 설정
    /// 선택된 셀은 selectMaterial을 적용
    /// </summary>
    public void ShowAllCells()
    {
        foreach (var cell in gridCells)
        {
            // 렌더링 활성화
            var sr = cell.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = true;

            // 셀 상태 업데이트
            var status = cell.GetComponent<GridCellStatus>();
            if (status != null)
            {
                // 머티리얼 설정
                status.SetMaterials(placeableMaterial, notPlaceableMaterial, selectMaterial);

                // 선택 여부 반영
                bool isCurrentSelection = (cell == selectedCell);
                status.SetSelected(isCurrentSelection);
            }
        }
    }

    /// <summary>
    /// 모든 GridCell의 자식 중에 스테이션이 있으면 하단 중심이 겹치도록 정렬
    /// </summary>
    public void AlignShelvesToGridCells()
    {
        foreach (var cell in gridCells)
        {
            foreach (Transform child in cell.transform)
            {
                if (child.CompareTag("Station"))
                {
                    child.localPosition = Vector3.zero;

                    if (showDebugInfo)
                    {
                        Debug.Log($"Shelf 위치 정렬됨: {child.name} @ {cell.name}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 모든 GridCell의 상태를 업데이트하여 배치 가능 여부에 따라 Material을 변경, GridCellStatus 컴포넌트가 있는 경우에만 적용
    /// </summary>
    public void UpdateGridCellStates()
    {
        foreach (var cell in gridCells)
        {
            var status = cell.GetComponent<GridCellStatus>();
            if (status != null)
            {
                bool isCurrentSelection = (cell == selectedCell);
                status.SetSelected(isCurrentSelection);
            }
        }
    }

    /// <summary>
    /// 사용자가 선택한 셀을 강조 표시하고, 전체 셀의 상태를 갱신
    /// 선택된 셀은 활성화(true) 상태로 설정되며, 나머지 셀들은 비활성화(false)
    /// 디버그 모드에서는 선택된 셀의 이름을 출력
    /// </summary>
    /// <param name="newSelection">새로 선택된 셀 GameObject</param>
    public void HighlightSelectedCell(GameObject newSelection)
    {
        selectedCell = newSelection;

        // 셀 상태 업데이트: 선택 여부 반영
        UpdateGridCellStates();

        // 디버그 정보 출력
        if (showDebugInfo && selectedCell != null)
            Debug.Log($"[TilemapController] 선택된 셀: {selectedCell.name}");
    }

    /// <summary>
    /// 선택된 셀의 하이라이트를 제거하고 선택 상태를 해제
    /// </summary>
    public void ClearSelection()
    {
        selectedCell = null;
        UpdateGridCellStates();
    }
}