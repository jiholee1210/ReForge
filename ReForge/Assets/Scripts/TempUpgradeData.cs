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
    public UpgradeType upgradeType;
    public float value;
    public float[] valueList;
    public int price;
    public int max;
    public Sprite sprite;
}