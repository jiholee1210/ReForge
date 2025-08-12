using UnityEngine;

public enum UpgradeType
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
    public UpgradeType upgradeType;
    public float value;
    public int price;
    public Sprite sprite;
}