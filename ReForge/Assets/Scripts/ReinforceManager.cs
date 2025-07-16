using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReinforceManager : MonoBehaviour, IWindow
{
    [SerializeField] private Button upgradeBtn;
    [SerializeField] private Button outsourcingBtn;
    [SerializeField] private Button projectBtn;

    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform unitParent;

    [SerializeField] private Transform unitDetail;
    [SerializeField] private Transform unitUpgrade;

    [SerializeField] private Transform starParent;
    [SerializeField] private GameObject starPrefab;

    private Unit unit;
    private Goods goods;
    private TempUpgrade tempUpgrade;
    private PermUpgrade permUpgrade;

    private UnitInfo curUnit;
    async void Start()
    {
        Setting();
        upgradeBtn.onClick.AddListener(() => TryUpgrade());
        outsourcingBtn.onClick.AddListener(() => PlaceOutsourcing());
        projectBtn.onClick.AddListener(() => PlaceProject());

        await DataManger.Instance.WaitForLoadingUnitData();
        Reset();
    }

    private void Setting()
    {
        unit = DataManger.Instance.unit;
        goods = DataManger.Instance.goods;
        tempUpgrade = DataManger.Instance.tempUpgrade;
        permUpgrade = DataManger.Instance.permUpgrade;
    }

    public void Reset()
    {
        unitDetail.gameObject.SetActive(false);
        unitUpgrade.gameObject.SetActive(false);
        curUnit = null;
        SetUnitWindow();
        Debug.Log("리스트 초기화");
    }

    private void SetUnitWindow()
    {
        foreach (Transform transform in unitParent)
        {
            Destroy(transform.gameObject);
        }

        List<GameObject> unitList = new();
        foreach (UnitInfo unitInfo in unit.units)
        {
            if (unitInfo.place != 0) continue;
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
                    ShowUpgrade(unitInfo);
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
                        ShowUpgrade(unitInfo);
                    });
                    unitList.Add(newUnit);
                }
            }
        }
    }

    private void TryUpgrade()
    {
        // 확률에 따라서 강화 실행
        // 강화 성공 시 기존 유닛의 upgrade 값 1증가
        // 실패 시 기존 유닛 리스트에서 제거
        // 강화 수치가 2 일때 승급 시도
        int id = curUnit.id;
        float random = Random.Range(0, 100f);
        Debug.Log(random);
        UnitData unitData = DataManger.Instance.GetUnitData(id);
        if (unitData.posibility >= random)
        {
            if (curUnit.upgrade == 2)
            {
                // 승급
                curUnit.id += 1;
                curUnit.upgrade = 0;
            }
            else
            {
                // 강화
                curUnit.upgrade += 1;
            }
        }
        else
        {
            unit.units.Remove(curUnit);
        }

        unit.units.Sort((a, b) =>
        {
            int cmp = a.id.CompareTo(b.id);
            if (cmp != 0) return cmp;

            return a.upgrade.CompareTo(b.upgrade);
        });

        Reset();
    }

    private void ShowDetail(UnitInfo unitInfo)
    {
        int id = unitInfo.id;
        int upgrade = unitInfo.upgrade;
        curUnit = unitInfo;
        UnitData unitData = DataManger.Instance.GetUnitData(id);

        unitDetail.gameObject.SetActive(true);

        foreach (Transform transform in starParent)
        {
            Destroy(transform.gameObject);
        }

        for (int i = 0; i < upgrade + 1; i++)
        {
            GameObject star = Instantiate(starPrefab, starParent);
            star.transform.localPosition += new Vector3(-5 * upgrade + 10 * i, 8, 0);
        }

        unitDetail.GetChild(0).GetComponent<TMP_Text>().text = unitData.dataName;
        unitDetail.GetChild(1).GetComponent<TMP_Text>().text = unitData.power + " 파워";
    }

    private void ShowUpgrade(UnitInfo unitInfo)
    {
        int id = unitInfo.id;
        int upgrade = unitInfo.upgrade;
        UnitData unitData = DataManger.Instance.GetUnitData(id);

        unitUpgrade.gameObject.SetActive(true);

        foreach (Transform transform in unitUpgrade.GetChild(3).GetChild(0))
        {
            Destroy(transform.gameObject);
        }

        foreach (Transform transform in unitUpgrade.GetChild(3).GetChild(1))
        {
            Destroy(transform.gameObject);
        }

        if (upgrade == 2)
        {
            unitUpgrade.GetChild(0).GetComponent<TMP_Text>().text = $"+{id} => +{id + 1}";
        }
        else
        {
            unitUpgrade.GetChild(0).GetComponent<TMP_Text>().text = "=>";
            // 별 생성
            for (int i = 0; i < upgrade + 1; i++)
            {
                GameObject star = Instantiate(starPrefab, unitUpgrade.GetChild(3).GetChild(0));
                star.transform.localPosition += new Vector3(-3.5f * upgrade + 7f * i, 0, 0);
            }

            for (int i = 0; i < upgrade + 2; i++)
            {
                GameObject star = Instantiate(starPrefab, unitUpgrade.GetChild(3).GetChild(1));

                star.transform.localPosition += new Vector3((-3.5f * (upgrade + 1)) + (7f * i), 0, 0);
            }
        }
        // 각 단계마다 1성~3성, 3성 강화 시 다음 단계로 성장
        unitUpgrade.GetChild(1).GetComponent<TMP_Text>().text = "확률 : " + unitData.posibility;
    }

    private void PlaceOutsourcing()
    {
        curUnit.place = 1;
        Reset();
    }

    private void PlaceProject()
    {
        curUnit.place = 2;
        Reset();
    }
}