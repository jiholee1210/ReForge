using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutsourcingManager : MonoBehaviour, IWindow
{
    [SerializeField] private Transform unitDetail;
    [SerializeField] private Transform outsourcingDetail;
    [SerializeField] private TMP_Text count;

    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform unitParent;
    [SerializeField] private GameObject outsourcingPrefab;
    [SerializeField] private Transform outsourcingParent;

    [SerializeField] private Button reinforceBtn;

    [SerializeField] private GameObject notice;

    private Unit unit;
    private Goods goods;
    private TempUpgrade tempUpgrade;
    private PermUpgrade permUpgrade;
    private Work work;

    private UnitInfo curUnit;

    private Coroutine coroutine;

    private KeyValuePair<int, TempUpgradeData> workSpeedTemp;
    private KeyValuePair<int, TempUpgradeData> unitPowerTemp;
    private KeyValuePair<int, TempUpgradeData> goldGainTemp;
    private KeyValuePair<int, PermUpgradeData> workSpeedPerm;
    private KeyValuePair<int, PermUpgradeData> unitPowerPerm;
    private KeyValuePair<int, PermUpgradeData> goldGainPerm;
    async void Start()
    {
        unit = DataManger.Instance.unit;
        goods = DataManger.Instance.goods;
        tempUpgrade = DataManger.Instance.tempUpgrade;
        permUpgrade = DataManger.Instance.permUpgrade;
        work = DataManger.Instance.work;

        reinforceBtn.onClick.AddListener(() => PlaceReinforce());

        await DataManger.Instance.WaitForLoadingOutsourcingData();
        await DataManger.Instance.WaitForLoadingPermUpgradeData();
        await DataManger.Instance.WaitForLoadingTempUpgradeData();
        SetOutsourcingList();

        workSpeedTemp = DataManger.Instance.tempUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.WorkSpeed);

        unitPowerTemp = DataManger.Instance.tempUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.UnitPower);

        goldGainTemp = DataManger.Instance.tempUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.GoldGain);

        workSpeedPerm = DataManger.Instance.permUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.WorkSpeed);

        unitPowerPerm = DataManger.Instance.permUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.UnitPower);

        goldGainPerm = DataManger.Instance.permUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.GoldGain);
            
        if (work.outsourcingID != -1)
        {
            SetOutsourcing(work.outsourcingID);
        }
    }

    void OnEnable()
    {
        DataManger.OnTryReset += TryReset;
    }

    void OnDisable()
    {
        DataManger.OnTryReset -= TryReset;
    }

    public void Reset()
    {
        unitDetail.gameObject.SetActive(false);
        curUnit = null;
        SetUnitList();
        StartCoroutine(WaitForAnimator(work.outsourcingID));
        if (work.outsourcingID != -1) notice.SetActive(false);
        else notice.SetActive(true);

        EditText();
    }

    private void SetUnitList()
    {
        foreach (Transform transform in unitParent)
        {
            Destroy(transform.gameObject);
        }

        SetCount();
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

    private void SetOutsourcingList()
    {
        foreach (Transform transform in outsourcingParent)
        {
            Destroy(transform.gameObject);
        }

        int count = DataManger.Instance.outsourcingDataDict.Count;

        for (int i = 0; i < count; i++)
        {
            int id = i;
            OutsourcingData outsourcingData = DataManger.Instance.GetOutsourcingData(id);

            GameObject item = Instantiate(outsourcingPrefab, outsourcingParent);
            item.transform.GetChild(0).GetComponent<TMP_Text>().text = outsourcingData.dataName;
            item.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => SetOutsourcing(id));
        }
    }

    private void ShowDetail(UnitInfo unitInfo)
    {
        int id = unitInfo.id;

        UnitData unitData = DataManger.Instance.GetUnitData(id);
        curUnit = unitInfo;

        unitDetail.gameObject.SetActive(true);

        unitDetail.GetChild(0).GetComponent<TMP_Text>().text = unitData.dataName;
        unitDetail.GetChild(1).GetComponent<TMP_Text>().text = Mathf.RoundToInt(unitData.power * (1 + tempUpgrade.upgrade[unitPowerTemp.Key] * unitPowerTemp.Value.value)
                                        * (1 + (permUpgrade.complete.Contains(unitPowerPerm.Key) ? unitPowerPerm.Value.value : 0))) + " 파워";
    }

    private void SetOutsourcing(int id)
    {
        outsourcingDetail.gameObject.SetActive(true);
        OutsourcingData outsourcingData = DataManger.Instance.GetOutsourcingData(id);
        work.outsourcingID = id;
        outsourcingDetail.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = outsourcingData.dataName;
        outsourcingDetail.GetChild(3).GetComponent<TMP_Text>().text = outsourcingData.max + " 노력치";
        outsourcingDetail.GetChild(4).GetComponent<TMP_Text>().text = Mathf.RoundToInt(outsourcingData.reward * (1 + tempUpgrade.upgrade[goldGainTemp.Key] * goldGainTemp.Value.value)
                                                            * (1 + (permUpgrade.complete.Contains(goldGainPerm.Key) ? goldGainPerm.Value.value : 0))) + " 골드";
        StartCoroutine(WaitForAnimator(id));

        if (work.outsourcingID != -1) notice.SetActive(false);
        else notice.SetActive(true);

        // place 1에 위치한 유닛 데이터를 기반으로 외주 코루틴 시작
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(StartOutsourcing(id));
    }

    private void EditText()
    {
        if (work.outsourcingID == -1) return;

        OutsourcingData outsourcingData = DataManger.Instance.GetOutsourcingData(work.outsourcingID);

        outsourcingDetail.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = outsourcingData.dataName;
        outsourcingDetail.GetChild(3).GetComponent<TMP_Text>().text = outsourcingData.max + " 노력치";
        outsourcingDetail.GetChild(4).GetComponent<TMP_Text>().text = Mathf.RoundToInt(outsourcingData.reward * (1 + tempUpgrade.upgrade[goldGainTemp.Key] * goldGainTemp.Value.value)
                                                            * (1 + (permUpgrade.complete.Contains(goldGainPerm.Key) ? goldGainPerm.Value.value : 0))) + " 골드";
    }

    private IEnumerator WaitForAnimator(int id)
    {
        Animator animator = outsourcingDetail.GetChild(0).GetComponent<Animator>();
        if (!animator.gameObject.activeInHierarchy) yield break;
        animator.SetInteger("id", id);
        Debug.Log("실행됨");
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName($"run{id}"))
        {
            yield return new WaitForEndOfFrame();
        }
        outsourcingDetail.GetChild(0).GetComponent<Image>().SetNativeSize();
    }

    private IEnumerator StartOutsourcing(int id)
    {
        Slider slider = outsourcingDetail.GetChild(2).GetComponent<Slider>();
        TMP_Text rateText = outsourcingDetail.GetChild(2).GetChild(2).GetComponent<TMP_Text>();

        while (true)
        {
            float power;
            // dps 기준으로  slider value 값 측정
            OutsourcingData outsourcingData = DataManger.Instance.GetOutsourcingData(id);
            float max = outsourcingData.max;
            float cur = outsourcingData.max;
            slider.maxValue = max;
            slider.value = max;
            rateText.text = "100.00%";

            while (cur > 0)
            {
                float time = 1f * (1 - tempUpgrade.upgrade[workSpeedTemp.Key] * workSpeedTemp.Value.value)
                            * (1 - (permUpgrade.complete.Contains(workSpeedPerm.Key) ? workSpeedPerm.Value.value : 0));
                yield return new WaitForSeconds(time);
                power = 0;
                foreach (UnitInfo unitInfo in unit.units)
                {
                    if (unitInfo.place != 1) continue;
                    UnitData unitData = DataManger.Instance.GetUnitData(unitInfo.id);
                    Debug.Log(unitData.power);
                    power += Mathf.RoundToInt(unitData.power * (1 + tempUpgrade.upgrade[unitPowerTemp.Key] * unitPowerTemp.Value.value)
                                        * (1 + (permUpgrade.complete.Contains(unitPowerPerm.Key) ? unitPowerPerm.Value.value : 0)));
                }

                cur -= power;
                cur = Mathf.Max(0, cur);
                float rate = cur / max * 100;
                Debug.Log(rate + " " + power);
                slider.value = cur;
                rateText.text = rate.ToString("F2") + "%";
            }
            goods.gold += Mathf.RoundToInt(outsourcingData.reward * (1 + tempUpgrade.upgrade[goldGainTemp.Key] * goldGainTemp.Value.value)
                                                            * (1 + (permUpgrade.complete.Contains(goldGainPerm.Key) ? goldGainPerm.Value.value : 0)));
            UIManager.Instance.SetGoldText();
        }
    }

    private void SetCount()
    {
        count.text = work.curOut + "/" + work.outsourcingMax;
    }

    private void PlaceReinforce()
    {
        curUnit.place = 0;
        SetUnitList();
        unitDetail.gameObject.SetActive(false);
    }

    private void TryReset()
    {
        StopAllCoroutines();
        coroutine = null;

        outsourcingDetail.gameObject.SetActive(false);
    }
}