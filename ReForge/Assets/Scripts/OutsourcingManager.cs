using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutsourcingManager : MonoBehaviour, IWindow
{
    [SerializeField] private Transform unitDetail;
    [SerializeField] private Transform outsourcingDetail;

    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform unitParent;

    [SerializeField] private Button reinforceBtn;

    private Unit unit;
    private Goods goods;
    private TempUpgrade tempUpgrade;
    private PermUpgrade permUpgrade;

    private UnitInfo curUnit;

    void Start()
    {
        unit = DataManger.Instance.unit;
        goods = DataManger.Instance.goods;
        tempUpgrade = DataManger.Instance.tempUpgrade;
        permUpgrade = DataManger.Instance.permUpgrade;

        reinforceBtn.onClick.AddListener(() => PlaceReinforce());
    }

    public void Reset()
    {
        unitDetail.gameObject.SetActive(false);
        outsourcingDetail.gameObject.SetActive(false);
        curUnit = null;
        SetUnitList();
    }

    private void SetUnitList()
    {
        foreach (Transform transform in unitParent)
        {
            Destroy(transform.gameObject);
        }

        List<GameObject> unitList = new();
        foreach (UnitInfo unitInfo in unit.units)
        {
            if (unitInfo.place != 1) continue;
            int id = unitInfo.id;
            int upgrade = unitInfo.upgrade;
            UnitData unitData = DataManger.Instance.GetUnitData(id);

            bool found = false;
            if (unitList.Count == 0)
            {
                GameObject newUnit = Instantiate(unitPrefab, unitParent);
                newUnit.GetComponent<UnitStat>().SetStat(id, upgrade);
                newUnit.transform.GetChild(0).GetComponent<Image>().sprite = unitData.sprite;
                newUnit.transform.GetChild(1).GetComponent<Text>().text = "1";
                newUnit.transform.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ShowDetail(unitInfo);
                });
                unitList.Add(newUnit);
            }
            else
            {
                for (int i = 0; i < unitList.Count; i++)
                {
                    if (unitList[i].GetComponent<UnitStat>().CheckStat(id, upgrade))
                    {
                        unitList[i].GetComponent<UnitStat>().PlusCount();
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    GameObject newUnit = Instantiate(unitPrefab, unitParent);
                    newUnit.GetComponent<UnitStat>().SetStat(id, upgrade);
                    newUnit.transform.GetChild(0).GetComponent<Image>().sprite = unitData.sprite;
                    newUnit.transform.GetChild(1).GetComponent<Text>().text = "1";
                    newUnit.transform.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ShowDetail(unitInfo);
                    });
                    unitList.Add(newUnit);
                }
            }
        }
    }

    private void ShowDetail(UnitInfo unitInfo)
    {
        int id = unitInfo.id;

        UnitData unitData = DataManger.Instance.GetUnitData(id);
        curUnit = unitInfo;

        unitDetail.gameObject.SetActive(true);

        unitDetail.GetChild(0).GetComponent<TMP_Text>().text = unitData.dataName;
        unitDetail.GetChild(1).GetComponent<TMP_Text>().text = unitData.power + " 파워";
    }

    private void PlaceReinforce()
    {
        curUnit.place = 0;
        Reset();
    }
}