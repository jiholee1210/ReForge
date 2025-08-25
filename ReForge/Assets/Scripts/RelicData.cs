using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "RelicData", menuName = "RelicData", order = 0)]
public class RelicData : ScriptableObject
{
    public int id;
    public string dataName;
    public UpgradeType upgradeType;
    public string desc;
    public float value;
    public Sprite sprite;
}