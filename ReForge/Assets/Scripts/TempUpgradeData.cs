using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    None,
    GoldGain,
    UpPointGain,
    UnitPower,
    WorkSpeed,
    ReinforcePosibility,
    PriceDiscount
}
[CreateAssetMenu(fileName = "TempUpgradeData", menuName = "TempUpgradeData", order = 0)]
public class TempUpgradeData : ScriptableObject
{
    public int id;
    public string dataName;
    public UpgradeType upgradeType;
    public float value;
    public int price;
    public Sprite sprite;
}