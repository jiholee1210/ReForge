using UnityEngine;

public enum PermUpgradeType
{
    PlusReinforce,
    UpgradeDiscount,
    UnitDiscount,
    BaseGold,
    Auto,
    UpPointGain,
    UnitPower,
    WorkSpeed,
    ReinforcePosibility,
    GoldGain,
}

[CreateAssetMenu(fileName = "PermUpgradeData", menuName = "PermUpgradeData", order = 0)]
public class PermUpgradeData : ScriptableObject
{
    public int id;
    public string dataName;
    public string desc;
    public PermUpgradeType upgradeType;
    public float value;
    public int price;
}