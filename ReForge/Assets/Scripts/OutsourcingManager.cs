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
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text workText;

    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform unitParent;
    [SerializeField] private GameObject outsourcingPrefab;
    [SerializeField] private Transform outsourcingParent;
    [SerializeField] private Image radial;

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

    private bool inWindow = false;
    private List<GameObject> objectList = new();
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

    void Update()
    {
        if (inWindow && curUnit != null)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                PlaceReinforce();
            }
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
        inWindow = true;
    }

    private void SetUnitList()
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

        SetCount();
        SetWork();
        objectList.Clear();
        foreach (UnitInfo unitInfo in unit.units)
        {
            if (unitInfo.place != 1) continue;
            int id = unitInfo.id;
            int upgrade = unitInfo.upgrade;
            UnitData unitData = DataManger.Instance.GetUnitData(id);

            bool found = false;
            if (objectList.Count == 0)
            {
                GameObject newUnit = Instantiate(unitPrefab, unitParent);
                newUnit.GetComponent<UnitStat>().SetStat(id, upgrade);
                newUnit.transform.GetChild(0).GetComponent<Image>().sprite = unitData.sprite;
                newUnit.transform.GetChild(1).GetComponent<Text>().text = "1";
                newUnit.transform.GetComponent<Button>().onClick.AddListener(() =>
                {
                    ShowDetail(unitInfo);
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
                    objectList.Add(newUnit);
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
                                        * (1 + (permUpgrade.complete.Contains(unitPowerPerm.Key) ? unitPowerPerm.Value.value : 0))
                                        * (1 + (unitInfo.upgrade * 0.1f))) + " 작업";
    }

    private void SetOutsourcing(int id)
    {
        if(work.outsourcingID != -1) outsourcingParent.GetChild(work.outsourcingID).GetChild(1).GetComponent<Image>().color = Color.green;
        outsourcingDetail.gameObject.SetActive(true);
        OutsourcingData outsourcingData = DataManger.Instance.GetOutsourcingData(id);
        work.outsourcingID = id;
        outsourcingParent.GetChild(work.outsourcingID).GetChild(1).GetComponent<Image>().color = Color.yellow;
        outsourcingDetail.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = outsourcingData.dataName;
        outsourcingDetail.GetChild(2).GetChild(1).GetComponent<TMP_Text>().text = outsourcingData.max + " 작업량";
        outsourcingDetail.GetChild(2).GetChild(2).GetComponent<TMP_Text>().text = Mathf.RoundToInt(outsourcingData.reward * (1 + tempUpgrade.upgrade[goldGainTemp.Key] * goldGainTemp.Value.value)
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
        outsourcingDetail.GetChild(2).GetChild(1).GetComponent<TMP_Text>().text = outsourcingData.max + " 작업량";
        outsourcingDetail.GetChild(2).GetChild(2).GetComponent<TMP_Text>().text = Mathf.RoundToInt(outsourcingData.reward * (1 + tempUpgrade.upgrade[goldGainTemp.Key] * goldGainTemp.Value.value)
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
        Slider slider = outsourcingDetail.GetChild(2).GetChild(0).GetComponent<Slider>();
        TMP_Text rateText = outsourcingDetail.GetChild(2).GetChild(0).GetChild(3).GetComponent<TMP_Text>();

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
                 
                float t = 0f;
                radial.fillAmount = 0f;
                while (t < time)
                {
                    t += Time.deltaTime;
                    radial.fillAmount = Mathf.Clamp01(t / time);
                    yield return null;
                }

                power = 0;
                foreach (UnitInfo unitInfo in unit.units)
                {
                    if (unitInfo.place != 1) continue;
                    UnitData unitData = DataManger.Instance.GetUnitData(unitInfo.id);
                    Debug.Log(unitData.power);
                    power += Mathf.RoundToInt(unitData.power * (1 + tempUpgrade.upgrade[unitPowerTemp.Key] * unitPowerTemp.Value.value)
                                        * (1 + (permUpgrade.complete.Contains(unitPowerPerm.Key) ? unitPowerPerm.Value.value : 0))
                                        * (1 + (unitInfo.upgrade * 0.1f)));
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
            UIManager.Instance.GoldEffect();
        }
    }
    private void SetCount()
    {
        countText.text = work.curOut + "/" + work.outsourcingMax;
    }

    private void SetWork()
    {
        float workPower = 0;
        foreach (UnitInfo unit in unit.units) {
            if (unit.place == 1)
            {
                workPower += DataManger.Instance.GetUnitData(unit.id).power * (1 + (unit.upgrade * 0.1f));
            }
        }
        int totalWork = Mathf.RoundToInt(workPower * (1 + tempUpgrade.upgrade[unitPowerTemp.Key] * unitPowerTemp.Value.value)
                                                    * (1 + (permUpgrade.complete.Contains(unitPowerPerm.Key) ? unitPowerPerm.Value.value : 0)));
        workText.text = totalWork + " 작업";
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
            unitDetail.gameObject.SetActive(false);
        }
    }

    private void PlaceReinforce()
    {
        if (curUnit == null) return;
        curUnit.place = 0;
        work.curOut--;
        SetUnitList();
        CheckCount();
        curUnit = unit.units.FirstOrDefault(unit => unit.id == curUnit.id && unit.upgrade == curUnit.upgrade && unit.place == 1);
    }

    private void TryReset()
    {
        StopAllCoroutines();
        coroutine = null;
        outsourcingDetail.gameObject.SetActive(false);
    }

    public void Leave()
    {
        inWindow = false;
    }
}