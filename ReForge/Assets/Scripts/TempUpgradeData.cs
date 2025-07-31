using System.Collections.Generic;
using UnityEngine;

public enum TempUpgradeType
{
    None,
    UnitPower,
    WorkSpeed,
    ReinforcePosibility,
    GoldGain
}

[CreateAssetMenu(fileName = "TempUpgradeData", menuName = "TempUpgradeData", order = 0)]
public class TempUpgradeData : ScriptableObject
{
    public int id;
    public string dataName;
    public TempUpgradeType upgradeType;
    public float value;
    public int price;
    public Sprite sprite;
}