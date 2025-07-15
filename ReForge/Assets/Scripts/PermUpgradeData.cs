using UnityEngine;

[CreateAssetMenu(fileName = "PermUpgradeData", menuName = "PermUpgradeData", order = 0)]
public class PermUpgradeData : ScriptableObject
{
    public int id;
    public string dataName;
    public UpgradeType upgradeType;
    public float value;
    public int price;
}