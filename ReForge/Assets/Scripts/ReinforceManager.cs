using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private Transform reset;

    private Unit unit;
    private Goods goods;
    private TempUpgrade tempUpgrade;
    private PermUpgrade permUpgrade;
    private Work work;

    private UnitInfo curUnit;

    private int totalPoint;
    private bool resetOpen;

    private KeyValuePair<int, TempUpgradeData> posTemp;
    private KeyValuePair<int, PermUpgradeData> posPerm;
    private KeyValuePair<int, TempUpgradeData> workTemp;
    private KeyValuePair<int, PermUpgradeData> workPerm;
    private KeyValuePair<int, PermUpgradeData> pointGain;

    async void Start()
    {
        Setting();
        upgradeBtn.onClick.AddListener(() => TryUpgrade());
        outsourcingBtn.onClick.AddListener(() => PlaceOutsourcing());
        projectBtn.onClick.AddListener(() => PlaceProject());

        await DataManger.Instance.WaitForLoadingUnitData();
        Reset();
        reset.GetChild(3).GetComponent<Button>().onClick.AddListener(() => TryReset());

        posTemp = DataManger.Instance.tempUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.ReinforcePosibility);

        posPerm = DataManger.Instance.permUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.ReinforcePosibility);

        workTemp = DataManger.Instance.tempUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.UnitPower);

        workPerm = DataManger.Instance.permUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.UnitPower);

        pointGain = DataManger.Instance.permUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.UpPointGain);
    }

    void Update()
    {
        int point = goods.gold / 10000;
        totalPoint = Mathf.RoundToInt(point * (1 + (permUpgrade.complete.Contains(pointGain.Key) ? pointGain.Value.value : 0)));
        if (point > 0 && !resetOpen)
        {
            reset.gameObject.SetActive(true);
            resetOpen = true;
        }

        if (resetOpen)
        {
            reset.GetChild(2).GetComponent<TMP_Text>().text = totalPoint.ToString();
        }
    }

    private void OnEnable()
    {
        AutoManager.OnBuyUnit += HandleAutoBuy;
        AutoManager.OnUpgradeUnit += (unit) => HandleAutoUpgrade(unit);
    }

    private void OnDisable()
    {
        AutoManager.OnBuyUnit -= HandleAutoBuy;
        AutoManager.OnUpgradeUnit -= (unit) => HandleAutoUpgrade(unit);
    }

    private void Setting()
    {
        unit = DataManger.Instance.unit;
        goods = DataManger.Instance.goods;
        tempUpgrade = DataManger.Instance.tempUpgrade;
        permUpgrade = DataManger.Instance.permUpgrade;
        work = DataManger.Instance.work;
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
        unit.units.Sort((a, b) =>
        {
            int cmp = b.id.CompareTo(a.id);
            if (cmp != 0) return cmp;

            return b.upgrade.CompareTo(a.upgrade);
        });

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

        float totalPosibility = Mathf.Clamp(unitData.posibility + (tempUpgrade.upgrade[posTemp.Key] * posTemp.Value.value * (1 + (permUpgrade.complete.Contains(posPerm.Key) ? posPerm.Value.value : 0))
                                                                - unitData.id * 5 * curUnit.upgrade), 0f, 100f);
        if (totalPosibility >= random)
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

        Reset();
    }

    private void TryUpgrade(UnitInfo unitInfo)
    {
        int id = unitInfo.id;
        float random = Random.Range(0, 100f);
        Debug.Log(random);
        UnitData unitData = DataManger.Instance.GetUnitData(id);

        float totalPosibility = Mathf.Clamp(unitData.posibility + (tempUpgrade.upgrade[posTemp.Key] * posTemp.Value.value * (1 + (permUpgrade.complete.Contains(posPerm.Key) ? posPerm.Value.value : 0))
                                                                - unitData.id * 5 * unitInfo.upgrade), 0f, 100f);
        if (totalPosibility >= random)
        {
            if (unitInfo.upgrade == 2)
            {
                // 승급
                unitInfo.id += 1;
                unitInfo.upgrade = 0;
            }
            else
            {
                // 강화
                unitInfo.upgrade += 1;
            }
        }
        else
        {
            unit.units.Remove(unitInfo);
        }
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
        int totalWork = Mathf.RoundToInt(unitData.power * (1 + tempUpgrade.upgrade[workTemp.Key] * workTemp.Value.value)
                                        * (1 + (permUpgrade.complete.Contains(workPerm.Key) ? workPerm.Value.value : 0))
                                        * (1 + (unitInfo.upgrade * 0.1f)));
        unitDetail.GetChild(0).GetComponent<TMP_Text>().text = unitData.dataName;
        unitDetail.GetChild(1).GetComponent<TMP_Text>().text = totalWork + " 작업";
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
        unitUpgrade.GetChild(1).GetComponent<TMP_Text>().text = "확률 : " + Mathf.Clamp(unitData.posibility + (tempUpgrade.upgrade[posTemp.Key] * posTemp.Value.value* (1 + (permUpgrade.complete.Contains(posPerm.Key) ? posPerm.Value.value : 0))
                                                                                                            - unitData.id * 5 * unitInfo.upgrade), 0f, 100f);;
    }

    private void PlaceOutsourcing()
    {
        if (work.curOut >= work.outsourcingMax) return;
        work.curOut++;
        curUnit.place = 1;
        Reset();
    }

    private void PlaceProject()
    {
        if (work.curProject >= 4) return;
        work.curProject++;
        curUnit.place = 2;
        Reset();
    }

    private void HandleAutoBuy()
    {
        SetUnitWindow();
    }

    private void HandleAutoUpgrade(UnitInfo unitInfo)
    {
        TryUpgrade(unitInfo);
        SetUnitWindow();
    }

    public void TryReset()
    {
        DataManger.Instance.ResetData();
        permUpgrade.upPoint += totalPoint;
        Reset();
        reset.gameObject.SetActive(false);
        resetOpen = false;
    }
}