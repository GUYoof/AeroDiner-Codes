using System.Linq;
using UnityEngine;

/// <summary>
/// 자동 조리 스테이션: 재료를 놓으면 자동으로 조리되고, 완료 시 결과물이 생성됨
/// </summary>
public class AutomaticStation : BaseStation
{
    private void Update()
    {
        if (!timer.IsRunning) return;

        // 재료가 부족해졌다면 요리 중단
        if (!cookedIngredient ||
            !cookedIngredient.ingredients.All(id => currentIngredients.Contains(id)))
        {
            if (showDebugInfo) Debug.Log("[AutomaticStation] 조리 도중 재료 부족 → 요리 취소");
            timer.Stop();
            timerController?.gameObject.SetActive(false);
            return;
        }

        timer.Update(Time.deltaTime);                                          // 시간 감소

        timerController?.UpdateTimer(timer.Remaining, timer.Duration);         // UI 갱신

        if (timer.Remaining <= 0f)
        {
            ProcessCookingResult();
            ResetStation();
        }
    }
}