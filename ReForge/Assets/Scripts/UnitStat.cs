using UnityEngine;
using UnityEngine.UI;

public class UnitStat : MonoBehaviour
{
    private int id;
    private int upgrade;
    public int count;

    public void SetStat(int unitId, int unitUpgrade)
    {
        id = unitId;
        upgrade = unitUpgrade;
        count = 1;

        for (int i = 0; i < upgrade + 1; i++) {
            int index = i;
            transform.GetChild(2).GetChild(index).gameObject.SetActive(true);
        }
        
    }

    public bool CheckStat(int unitId, int unitUpgrade)
    {
        if (id == unitId && upgrade == unitUpgrade)
        {
            return true;
        }

        return false;
    }

    public void PlusCount()
    {
        count++;
        transform.GetChild(1).GetComponent<Text>().text = count.ToString();
    }
}