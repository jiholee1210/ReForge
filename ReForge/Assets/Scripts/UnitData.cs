using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "UnitData", order = 0)]
public class UnitData : ScriptableObject
{
    public int id;
    public string dataName;
    public float power;
    public float posibility;
    public int price;
    public int nextUnit;
    public Sprite sprite;
}