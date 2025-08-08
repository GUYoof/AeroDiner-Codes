using UnityEngine;
using System.Collections.Generic;

public class PrintCurrentIngredients : MonoBehaviour
{
    [Header("확인하려는 Station ID")]
    public string stationId;

    [Header("StationManager 연결")]
    public StationManager stationManager;

    [Header("확인할 재료 목록 (선택 사항)")]
    public string[] ingredientsToCheck;

    [Header("디버깅용 현재 재료 리스트")]
    public List<string> currentIngredients = new();

    [Header("재료별 상태 설정 결과")]
    public List<string> checkedResults = new();

    [Header("재료 확인 결과 (Inspector 표시용)")]
    [TextArea]
    public string resultLog;

    /// <summary>
    /// 현재 Station의 재료 리스트를 Inspector에 출력하고 StationManager 디버깅 호출
    /// </summary>
    public void UpdateCurrentIngredients()
    {
        currentIngredients.Clear();

        if (!stationManager) return;

        currentIngredients = stationManager.GetCurrentIngredients(stationId);

        if (ingredientsToCheck != null && ingredientsToCheck.Length > 0)
        {
            stationManager.CheckIngredients(stationId, ingredientsToCheck);
        }
    }

    /// <summary>
    /// StationManager의 CheckIngredients 호출하고 Inspector에서 확인 가능하게 결과 요약 저장
    /// </summary>
    public void TestIngredientPresence()
    {
        resultLog = ""; // 이전 결과 초기화

        if (stationManager == null || string.IsNullOrEmpty(stationId))
        {
            resultLog = " StationManager 연결 또는 Station ID 누락";
            return;
        }

        if (ingredientsToCheck == null || ingredientsToCheck.Length == 0)
        {
            resultLog = " 검사할 재료가 없습니다";
            return;
        }

        // 콘솔 로그 출력은 StationManager가 담당
        stationManager.CheckIngredients(stationId, ingredientsToCheck);

        // Inspector용 요약 메시지 추가
        resultLog = $" 재료 상태 확인 요청 완료\n→ Station ID: '{stationId}'\n→ 검사 재료 수: {ingredientsToCheck.Length}";
    }

}
