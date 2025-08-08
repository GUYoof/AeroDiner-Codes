# 스테이션 배치 구조
- 그리드 셀 및 스테이션 오브젝트의 배치, 상태 관리, 삭제, 표시 제어를 담당하는 네 개의 주요 스크립트를 설명 합니다
- 각 스크립트는 게임 페이즈, 플레이어 상호작용, 셀 상태 변화에 따라 동작합니다.

## 📂 폴더 구조
```
Scripts
├── DeleteGridCell.cs        # 게임 페이즈 이벤트를 받아 GridCell 자식의 Station 태그 오브젝트를 삭제.
├── GridCellStatus.cs        # 셀의 SpriteRenderer·Outline을 제어해 배치 가능/불가/선택 상태를 표시.
├── StorageGridCell.cs       # 게임 페이즈에 따라 보관된 스테이션을 보이거나 숨김. 초기화 시 현재 페이즈와 동기화.
└── TilemapController.cs     # 씬에 에디터로 배치된 GridCell들을 수집·숨김·보임·정렬·선택 관리. StationManager와 연결하여 전체 흐름을 제어.
```

## 🎯 핵심 기능의 설계 특징
### 1. DeleteGridCell.cs
- OnEnable / OnDisable
  - GameEventType.GamePhaseChanged 이벤트를 등록/해제.

- OnGamePhaseChanged
  - 게임 페이즈가 Opening일 때 DeleteChildStations() 실행.

- DeleteChildStations()
  - 현재 오브젝트의 자식 중 Tag가 "Station" 인 오브젝트를 모두 삭제.

- HasStationToBeDeleted()
  - 삭제 대상 스테이션 존재 여부를 외부에서 검사 가능.
 
### 2. GridCellStatus.cs
- Awake
  - SpriteRenderer, OutlineShaderController 참조 획득.
  - 자식 중 IMovableStation 구현체를 탐색해 placementStation 설정.

- SetMaterials()
  - 배치 가능 / 불가 / 선택 시 머티리얼을 할당.

- SetSelected(bool)
  - 선택 상태 변경 후 UpdateStatus() 호출.

- UpdateStatus()
  - 선택 상태 또는 배치 가능 여부에 따라 머티리얼 변경.
  - 아웃라인 항상 활성화.

- HasStation()
  - 자식 중 IMovableStation 존재 여부 반환.

- IInteractable 인터페이스 구현
  - OnHoverEnter → 아웃라인 표시
  - OnHoverExit → 아웃라인 숨김

### 3. StorageGridCell.cs
- OnEnable / OnDisable
  - GameEventType.GamePhaseChanged 이벤트 등록/해제.
  - 초기 상태는 현재 게임 페이즈에 맞게 동기화.

- OnGamePhaseChanged
  - 게임 페이즈 변경 시 UpdateVisibility() 실행.

- UpdateVisibility(GamePhase)
  - 자식 중 Tag == "Station"인 오브젝트를 찾아 활성/비활성 처리.

- GetStoredObject()
  - 보관된 스테이션 오브젝트 반환.

### 4. TilemapController.cs
- 그리드 셀 탐색 & 초기화
  - FindGridCells() → 모든 자식 중 Tag == "GridCell"인 오브젝트를 수집.
  - HideAllCells() → 모든 셀의 SpriteRenderer 비활성화 및 기본 머티리얼 적용.

- 게임 시작 시 연결
  - Start()에서 StationManager와 연결 시도 (WaitAndConnect 코루틴).

- 셀 표시 & 상태 반영
  - ShowAllCells() → 모든 셀 활성화, GridCellStatus에 머티리얼과 선택 상태 반영.
  - UpdateGridCellStates() → 선택 셀 강조 표시.

정렬 기능
  - AlignShelvesToGridCells() → 스테이션의 위치를 셀 중앙으로 정렬.

셀 선택 제어
  - HighlightSelectedCell() → 특정 셀을 선택하고 상태 업데이트.
  - ClearSelection() → 선택 해제.

## 📌 핵심 코드 예시
### 1) 스테이션 삭제 로직 (DeleteGridCell.cs)
```
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
```
- GamePhase.Opening 등 특정 페이즈가 될 때 GridCell의 모든 자식 트랜스폼을 검사해 Tag == "Station"인 경우 Destroy로 삭제.

### 2) 그리드 셀 상태 업데이트 (GridCellStatus.cs)
```
public void UpdateStatus()
{
    if (sr == null) return;

    if (isSelected)
    {
        sr.sharedMaterial = selectMaterial;
    }
    else
    {
        bool isPlaceable = !HasStation();
        sr.sharedMaterial = isPlaceable ? placeableMaterial : notPlaceableMaterial;
    }
    outline?.EnableOutline();
}
```
- 선택 상태(isSelected)가 true면 selectMaterial을 적용하고, 아니면 자식 스테이션 유무로 배치 가능/불가 머티리얼을 선택해 SpriteRenderer에 적용.
- 마지막에 아웃라인을 활성화함.
- sharedMaterial은 머티리얼 애셋을 직접 변경(모든 인스턴스에 영향)을 의미하고, material은 인스턴스화를 통해 해당 렌더러에만 적용.

### 3) 스테이션 정렬 기능 (TilemapController.cs)
```
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
                    Debug.Log($"Shelf 위치 정렬됨: {child.name} @ {cell.name}");
            }
        }
    }
}
```
- 각 GridCell의 자식 중 Station 태그를 가진 오브젝트의 localPosition을 (0,0,0)으로 설정해 셀의 중심(하위 피벗 기준)으로 정렬.
-  프리팹의 위치가 그리드셀과 일치하도록 일괄 정리하기 위해 사용.
