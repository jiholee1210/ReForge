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
    private KeyValuePair<int, PermUpgradeData> addPos;
    private KeyValuePair<int, PermUpgradeData> fragPos;
    private KeyValuePair<int, PermUpgradeData> fragValue;

    private bool inWindow = false;

    private List<GameObject> objectList = new();

    async void Start()
    {
        Setting();
        upgradeBtn.onClick.AddListener(() => TryUpgrade());
        outsourcingBtn.onClick.AddListener(() => PlaceOutsourcing());
        projectBtn.onClick.AddListener(() => PlaceProject());

        await DataManger.Instance.WaitForLoadingUnitData();
        await DataManger.Instance.WaitForLoadingPermUpgradeData();
        await DataManger.Instance.WaitForLoadingTempUpgradeData();

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

        addPos = DataManger.Instance.permUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.PlusReinforce);

        fragPos = DataManger.Instance.permUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.DestroyFrag);

        fragValue = DataManger.Instance.permUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.DestroyGain);
    }

    void Update()
    {
        int point = goods.gold / 10000;

        if (pointGain.Value != null)
        {
            totalPoint = Mathf.RoundToInt(point * (1 + (permUpgrade.complete.Contains(pointGain.Key) ? pointGain.Value.value : 0)));
        }
        
        if (point > 0 && !resetOpen)
        {
            reset.gameObject.SetActive(true);
            resetOpen = true;
        }

        if (resetOpen)
        {
            reset.GetChild(2).GetComponent<TMP_Text>().text = totalPoint.ToString();
        }

        if (inWindow && curUnit != null)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                // 외주
                PlaceOutsourcing();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                // 프로젝트
                PlaceProject();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                // 강화
                TryUpgrade();
            }
        }
    }

    private void OnEnable()
    {
        AutoManager.OnBuyUnit += HandleAutoBuy;
        AutoManager.OnUpgradeUnit += (unitList) => HandleAutoUpgrade(unitList);
        DataManger.OnTryReset += Reset;
    }

    private void OnDisable()
    {
        AutoManager.OnBuyUnit -= HandleAutoBuy;
        AutoManager.OnUpgradeUnit -= (unitList) => HandleAutoUpgrade(unitList);
        DataManger.OnTryReset -= Reset;
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
        inWindow = true;
        SetUnitWindow();
    }

    public void Leave()
    {
        inWindow = false;
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
        objectList.Clear();

        foreach (UnitInfo unitInfo in unit.units)
        {
            if (unitInfo.place != 0) continue;
            int id = unitInfo.id;
            int upgrade = unitInfo.upgrade;
            UnitData unitData = DataManger.Instance.GetUnitData(id);

            bool foundObject = false;
            if (objectList.Count == 0)
            {
                GameObject newUnit = Instantiate(unitPrefab, unitParent);
                objectList.Add(newUnit);
                newUnit.GetComponent<UnitStat>().SetStat(id, upgrade);
                newUnit.transform.GetChild(0).GetComponent<Image>().sprite = unitData.sprite;
                newUnit.transform.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ShowDetail(unitInfo);
                    ShowUpgrade(unitInfo);
                });
                objectList.Add(newUnit);
            }
            else
            {
                for (int i = 0; i < objectList.Count; i++)
                {
                    if (objectList[i].GetComponent<UnitStat>().CheckStat(id, upgrade))
                    {
                        objectList[i].GetComponent<UnitStat>().PlusCount();
                        foundObject = true;
                        break;
                    }
                }

                if (!foundObject)
                {
                    GameObject newUnit = Instantiate(unitPrefab, unitParent);
                    objectList.Add(newUnit);
                    newUnit.GetComponent<UnitStat>().SetStat(id, upgrade);
                    newUnit.transform.GetChild(0).GetComponent<Image>().sprite = unitData.sprite;
                    newUnit.transform.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ShowDetail(unitInfo);
                        ShowUpgrade(unitInfo);
                    });
                    objectList.Add(newUnit);
                }
            }
        }
        CheckCount();
    }

    private void CheckCount()
    {
        bool found = false;
        foreach (GameObject unit in objectList)
        {
            if (curUnit != null && unit.GetComponent<UnitStat>().CheckStat(curUnit.id, curUnit.upgrade))
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            curUnit = null;
            unitDetail.gameObject.SetActive(false);
            unitUpgrade.gameObject.SetActive(false);
        }
    }

    private void TryUpgrade()
    {
        int id = curUnit.id;
        int upgrade = curUnit.upgrade;
        float random1 = Random.Range(0, 100f);
        UnitData unitData = DataManger.Instance.GetUnitData(id);

        float totalPosibility = Mathf.Clamp(unitData.posibility + (tempUpgrade.upgrade[posTemp.Key] * posTemp.Value.value * (1 + (permUpgrade.complete.Contains(posPerm.Key) ? posPerm.Value.value : 0))
                                                                - (unitData.id + 1) * 5 * curUnit.upgrade), 0f, 100f);
        if (totalPosibility >= random1)
        {
            int up;
            float random2 = Random.Range(0, 100f);
            float additional = permUpgrade.complete.Contains(addPos.Key) ? addPos.Value.value : 0f;
            if (additional >= random2)
            {
                up = 2;
            }
            else
            {
                up = 1;
            }
            curUnit.id += (curUnit.upgrade + up) / 3;
            curUnit.upgrade = (curUnit.upgrade + up) % 3;
        }
        else
        {
            unit.units.Remove(curUnit);
            float random = Random.Range(0, 100f);
            float fragment = 10f + (permUpgrade.complete.Contains(fragPos.Key) ? fragPos.Value.value : 0);
            if (random <= fragment)
            {
                goods.frag += Mathf.RoundToInt(Mathf.Pow(10, curUnit.id) * (curUnit.upgrade + 1) * (1 + (permUpgrade.complete.Contains(fragValue.Key) ? fragValue.Value.value : 0)));
                Debug.Log(Mathf.Pow(10, curUnit.id) + " " + (curUnit.upgrade + 1) + " " + (1 + (permUpgrade.complete.Contains(fragValue.Key) ? fragValue.Value.value : 0)));    
                UIManager.Instance.SetFragText();
                UIManager.Instance.FragEffect();
            }
        }
        curUnit = unit.units.FirstOrDefault(unit => unit.id == id && unit.upgrade == upgrade && unit.place == 0);
        SetUnitWindow();
    }

    private void TryUpgrade(List<UnitInfo> unitList)
    {
        for (int i = 0; i < unitList.Count; i++)
        {
            int id = unitList[i].id;
            int upgrade = unitList[i].upgrade;
            float random1 = Random.Range(0, 100f);
            UnitData unitData = DataManger.Instance.GetUnitData(id);
            if (posPerm.Equals(default(KeyValuePair<int, PermUpgradeData>))) return;

            float totalPosibility = Mathf.Clamp(unitData.posibility + (tempUpgrade.upgrade[posTemp.Key] * posTemp.Value.value * (1 + (permUpgrade.complete.Contains(posPerm.Key) ? posPerm.Value.value : 0))
                                                                    - (unitData.id + 1) * 5 * upgrade), 0f, 100f);
            if (totalPosibility >= random1)
            {
                int up;
                float random2 = Random.Range(0, 100f);
                float additional = permUpgrade.complete.Contains(addPos.Key) ? addPos.Value.value : 0f;
                if (additional >= random2)
                {
                    up = 2;
                }
                else
                {
                    up = 1;
                }
                unitList[i].id += (unitList[i].upgrade + up) / 3;
                unitList[i].upgrade = (unitList[i].upgrade + up) % 3;
            }
            else
            {
                unit.units.Remove(unitList[i]);
                float random = Random.Range(0, 100f);
                float fragment = 10f + (permUpgrade.complete.Contains(fragPos.Key) ? fragPos.Value.value : 0);
                if (random <= fragment)
                {
                    goods.frag += Mathf.RoundToInt(Mathf.Pow(10, curUnit.id) * (curUnit.upgrade + 1) * (1 + (permUpgrade.complete.Contains(fragValue.Key) ? fragValue.Value.value : 0)));
                    UIManager.Instance.SetFragText();
                    UIManager.Instance.FragEffect();
                }
            }

            if (curUnit != null && curUnit.Equals(unitList[i]))
            {
                curUnit = unit.units.FirstOrDefault(unit => unit.id == id && unit.upgrade == upgrade && unit.place == 0);
            }
        }
        SetUnitWindow();
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
            star.transform.localPosition += new Vector3(-5 * upgrade + 10 * i, 0, 0);
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
        unitUpgrade.GetChild(1).GetComponent<TMP_Text>().text = "확률 : " + Mathf.Clamp(unitData.posibility + (tempUpgrade.upgrade[posTemp.Key] * posTemp.Value.value * (1 + (permUpgrade.complete.Contains(posPerm.Key) ? posPerm.Value.value : 0))
                                                                                                            - (unitData.id + 1) * 5 * unitInfo.upgrade), 0f, 100f);
    }

    private void PlaceOutsourcing()
    {
        if (work.curOut >= work.outsourcingMax || curUnit == null) return;
        work.curOut++;
        curUnit.place = 1;
        curUnit = unit.units.FirstOrDefault(unit => unit.id == curUnit.id && unit.upgrade == curUnit.upgrade && unit.place == 0);
        SetUnitWindow();
    }

    private void PlaceProject()
    {
        if (work.curProject >= 4 || curUnit == null) return;
        work.curProject++;
        curUnit.place = 2;
        curUnit = unit.units.FirstOrDefault(unit => unit.id == curUnit.id && unit.upgrade == curUnit.upgrade && unit.place == 0);
        SetUnitWindow();
    }

    private void HandleAutoBuy()
    {
        SetUnitWindow();
    }

    private void HandleAutoUpgrade(List<UnitInfo> unitList)
    {
        TryUpgrade(unitList);
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