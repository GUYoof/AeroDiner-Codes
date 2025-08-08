using UnityEngine;

public class CookingTimer
{
    public float Duration { get; private set; }  // 전체 요리 시간
    public float Remaining { get; private set; } // 남은 시간
    public bool IsRunning { get; private set; }  // 동작 여부

    private FoodData foodData;

    public CookingTimer(FoodData foodData)
    {
        this.foodData = foodData;
        Duration = foodData != null ? foodData.cookTime : 0f;
        Remaining = 0f;
        IsRunning = false;
    }

    // 처음 시작하거나, 중단 후 이어서 재개할 때 사용
    public void Start(float resumeFrom = -1f)
    {
        Remaining = resumeFrom >= 0f ? resumeFrom : Duration;
        IsRunning = true;
    }

    public void Update(float deltaTime)
    {
        if (!IsRunning) return;
        Remaining = Mathf.Max(Remaining - deltaTime, 0f);
        if (Remaining <= 0f) Stop();  // 자동 정지
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public void Reset()
    {
        Remaining = Duration;
        IsRunning = false;
    }
}