using UnityEngine;

[CreateAssetMenu(fileName = "OutsourcingData", menuName = "OutsourcingData", order = 0)]
public class OutsourcingData : ScriptableObject
{
    public int id;
    public string dataName;
    public float max;
    public int reward;
    public GameObject prefab;
}