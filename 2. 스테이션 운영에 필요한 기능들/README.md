# 스테이션 운영에 필요한 기능들
- 스테이션이 동작하면서 필요한 기능들을 따로 관리하는 스크립트들 입니다.

## 📂 폴더 구조
📂 Scripts
 ├── ChangeSprite.cs                // 설비의 시각(스프라이트) 상태 전환 담당
 ├── CookingTimer.cs                // 음식별 조리 시간(타이머) 로직 (비-MonoBehaviour 유틸)
 ├── FoodIconLibrary.cs             // FoodType → 아이콘 Sprite 매핑 제공하는 정적 라이브러리
 ├── FoodSlotIconDisplay.cs         // 설비 위 슬롯 아이콘 생성/배치/표시/숨김 관리
 ├── OutlineShaderController.cs     // SpriteRenderer의 쉐이더 외곽선(Outline) On/Off 제어
 ├── StationSFXResolver.cs          // StationData를 SFXType으로 변환하는 매핑 유틸
 └── StationTimerController.cs      // 남은 조리시간에 따라 타이머 UI 이미지 갱신 및 캔버스 정렬

## 🎯 핵심 기능의 설계 특징
### 1. 시각(Visual) 처리의 분리와 최소 오버헤드
- ChangeSprite: 시각(스프라이트 변경) 로직은 상태 판정(조리중/대기)과 분리되어, 단순한 조건 기반 스프라이트 교체만 수행. 상태는 BaseStation이 소유하고, 이 컴포넌트는 읽기 전용으로 동작 — 책임 분리(CSRP).
- OutlineShaderController: MaterialPropertyBlock 사용으로 마테리얼을 복제하지 않고도 per-renderer 프로퍼티를 바꿔서 메모리·드로우콜 오버헤드를 낮춤.

### 2. 타이머 관리의 경량화 & 소유자 호출 방식
- CookingTimer는 MonoBehaviour를 상속하지 않는 순수 클래스(POCO). 이는 인스턴스를 자유롭게 생성/재설정/직렬화 없이 관리하기 쉬움.
- 운영 방식: BaseStation(또는 담당 스크립트)이 CookingTimer.Update(Time.deltaTime)를 호출해 흐름을 제어한다. (타이머 자체는 프레임 루프에 직접 참여하지 않음)
- 장점: 테스트 용이성(Unit test), 여러 타이머 인스턴스(여러 요리) 동시 관리 간단.

### 3. 데이터 기반 매핑 및 확장성
- FoodIconLibrary: 정적 딕셔너리로 FoodType → Sprite를 제공. 초기화 시점(게임 초기, 리소스 로드 후)에 맵을 채워야 안전.
- StationSFXResolver: 코드 내부 switch 매핑을 사용. 간단하고 빠르지만 SFX 추가/변경이 잦다면 ScriptableObject나 JSON 기반 룩업으로 전환 권장.

### 4. 슬롯 UI의 순서성 보장 & 동적 배치
- FoodSlotIconDisplay는 슬롯 리스트의 순서를 기준으로 아이콘을 좌우 정렬(basePosition 기준 오프셋), 사용 시 해당 타입의 첫 번째 활성 아이콘을 숨김.
- 내부 구조는 Dictionary<FoodType, List<GameObject>> 형태로 관리하여 타입별 복수 슬롯(동일 타입 여러 개)도 처리 가능.
- 슬롯을 다시 구성할 때는 ResetAll()로 상태를 초기화하고 Initialize()로 재배치.

### 5. 타이머 UI 매핑 방식
- StationTimerController: sprites 리스트의 길이를 N으로 보고 총 시간을 N구간으로 분할한 뒤, 남은 시간에 맞는 인덱스를 선택해서 timerImg.sprite를 갱신.
- ShowPassiveCookingState()는 수동 조리(대기형)일 때 마지막 스프라이트를 별도 lastTimerImg에 보여주도록 설계.

### 6. 책임 분리(Separation of Concerns)
- UI(이미지, 외곽선), 사운드 결정, 로직(타이머), 데이터(아이콘 맵)는 서로 분리되어 있음.
  → 유지보수성, 테스트성, 역할별 변경 영향 최소화.

## 📌 핵심 코드 예시
### 1. 조리 상태에 따른 스프라이트 변경
```
// ChangeSprite.cs
if (baseStation.IsCookingOrWaiting)
{
    spriteRenderer.sprite = cookSprite;
}
else
{
    spriteRenderer.sprite = baseSprite;
}
```
- BaseStation의 상태를 읽어 조리/대기 중일 때 cookSprite로, 아니면 baseSprite로 바꿈.

### 2. 조리 타이머 진행 (남은 시간 감소)
```
// CookingTimer.cs
public void Update(float deltaTime)
{
    if (!IsRunning) return;
    Remaining = Mathf.Max(Remaining - deltaTime, 0f);
    if (Remaining <= 0f) Stop();  // 자동 정지
}
```
- 매 프레임 deltaTime만큼 남은 시간을 깎음. 0에 도달하면 자동으로 타이머를 정지시킴.
  
### 3. 음식 타입별 아이콘 반환
```
// FoodIconLibrary.cs
public static Sprite GetIcon(FoodType type)
{
    if (iconDict != null && iconDict.TryGetValue(type, out var sprite))
        return sprite;
    return null;
}
```
- 초기화된 딕셔너리에서 FoodType에 해당하는 Sprite를 반환.

### 4. 슬롯 UI 초기화 및 배치
```
// FoodSlotIconDisplay.cs
Vector3 pos = basePosition.position + new Vector3(i * spacing - totalWidth / 2f, 1f, 0);
GameObject icon = Instantiate(iconPrefab, pos, Quaternion.identity, transform);
```
- basePosition을 기준으로 spacing만큼 간격을 두고 아이콘을 배치. totalWidth로 중앙 정렬을 수행.

5. 외곽선(Outline) On/Off — MaterialPropertyBlock 사용
```
// OutlineShaderController.cs
block.SetFloat(OutlineEnabled, 1); // Enable
block.SetFloat(OutlineEnabled, 0); // Disable
sr.SetPropertyBlock(block);
```
- MaterialPropertyBlock을 사용해 렌더러에만 임시 속성을 주입.
- 머티리얼 인스턴스 생성을 피해 성능에 유리.

6. 남은 시간에 맞는 타이머 스프라이트 선택
```
// StationTimerController.cs
int index = GetSpriteIndex(currentCookingTime, totalTime);
timerImg.sprite = sprites[index];
```
- 남은 시간(currentCookingTime)에 대응하는 스프라이트 인덱스를 계산 후 이미지 갱신. 
- sprites 배열의 순서 규약을 맞춰야 의도한 표시가 됨.
