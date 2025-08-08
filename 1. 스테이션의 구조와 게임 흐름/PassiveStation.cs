using UnityEngine;
using System.Linq;

/// <summary>
/// 플레이어가 상호작용하면 재료를 가공하여 가공된 재료를 생성하는 스테이션
/// 주요 기능:
/// - Interact(): J 키를 누르는 동안 조리 타이머가 감소하며, 타이머가 다 되면 가공 처리
/// </summary>
public class PassiveStation : BaseStation, IInteractable
{
    /// <summary>
    /// 플레이어가 J 키 등으로 상호작용할 때 호출
    /// InteractionType.Use: 타이머 감소 및 조리 진행
    /// InteractionType.Stop: 조리 중단 및 사운드 정지
    /// </summary>
    public new void Interact(PlayerInventory playerInventory, InteractionType interactionType)
    {
        if (interactionType == InteractionType.Use)
        {
            // 조리 조건 검사
            if (!CanStartCooking())
            {
                if (showDebugInfo) Debug.Log("[PassiveStation] 조리 조건 불충분");

                // 인스턴스 없으면 생성, 있으면 리셋
                if (timer == null)
                    timer = new CookingTimer(cookedIngredient);
                else
                    timer.Reset(); // 기존 타이머 유지하면서 리셋

                timerController?.UpdateTimer(timer.Remaining, timer.Duration);
                timerController?.gameObject.SetActive(false);
                return;
            }

            // 조리 시작 처리
            if (!isCooking)
            {
                isCooking = true;

                // 타이머가 멈춰 있다면 이어서 시작
                if (!timer.IsRunning)
                {
                    timer.Start(timer.Remaining); // 남은 시간 기준
                    if (showDebugInfo) Debug.Log($"[PassiveStation] 조리 이어서 시작 / 남은 시간: {timer.Remaining:F2}s");
                }

                StartCooking(); // 사운드 및 UI 처리
            }

            // 조리 진행 처리
            timer.Update(Time.deltaTime);
            timerController?.UpdateTimer(timer.Remaining, timer.Duration);

            // 조리 완료 시 결과 처리
            if (timer.Remaining <= 0f)
            {
                ProcessCookingResult();
                ResetStation(); // 결과 생성 후 초기화 처리
            }
        }
        else if (interactionType == InteractionType.Stop)
        {
            // 조리 중단 처리
            isCooking = false;
            timer?.Stop(); // 타이머 멈춤

            var sfx = StationSFXResolver.GetSFXFromStationData(StationData);
            EventBus.StopLoopSFX(sfx);

            if (showDebugInfo) Debug.Log($"[PassiveStation] 조리 중단됨 / 저장된 시간: {timer?.Remaining:F2}s");
        }
    }

    /// <summary>
    /// 현재 재료 목록이 조리 가능한 완전한 레시피를 충족하는지 확인
    /// </summary>
    /// <returns>조리 가능 여부</returns>
    private bool CanStartCooking()
    {
        return cookedIngredient != null &&
               cookedIngredient.ingredients.All(id => currentIngredients.Contains(id));
    }

    /// <summary>
    /// 필요 시 타이머를 완전히 초기화하는 함수 (조리 조건 미충족 시 등)
    /// </summary>
    private void ResetCookingTimer()
    {
        if (timer == null)
            timer = new CookingTimer(cookedIngredient);
        else
            timer.Reset(); // 인스턴스 유지하며 초기화

        timerController?.UpdateTimer(timer.Remaining, timer.Duration);
        timerController?.gameObject.SetActive(false);
    }
}