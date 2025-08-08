using System;
using UnityEngine;

[Serializable]
public class Station
{
    [Header("기본 데이터")]
    public StationData stationData;

    [Header("플레이어 진행 상황")]
    public bool isUnlocked; // 스테이션을 가지고있는지 (인벤토리에서 해금)

    public Station(StationData data, bool unlocked = false, bool selected = false)
    {
        stationData = data;
        isUnlocked = unlocked;
    }

    public string StationName => stationData?.displayName ?? "Unknown";
}
