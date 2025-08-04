using UnityEngine;

[CreateAssetMenu(fileName = "ProjectData", menuName = "ProjectData", order = 0)]
public class ProjectData : ScriptableObject
{
    public int id;
    public string dataName;
    public float max;
    public int rewardGold;
    public int unitMax;
}