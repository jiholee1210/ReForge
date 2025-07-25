using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoManager : MonoBehaviour, IWindow
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private TMP_Dropdown buyLevel;
    [SerializeField] private TMP_Dropdown upgradeLevel;
    [SerializeField] private TMP_Dropdown upgradeGrade;
    [SerializeField] private Toggle autoBuyCheck;
    [SerializeField] private Toggle autoUpgradeCheck;

    public static event Action OnBuyUnit;
    public static event Action<UnitInfo> OnUpgradeUnit;

    private Unit unit;
    private Goods goods;
    private TempUpgrade tempUpgrade;
    private PermUpgrade permUpgrade;
    private Auto auto;

    private WaitForSeconds waitForOneSecond = new WaitForSeconds(0.2f);

    private Coroutine autoBuy;
    private Coroutine autoUpgrade;
    void Start()
    {
        unit = DataManger.Instance.unit;
        goods = DataManger.Instance.goods;
        tempUpgrade = DataManger.Instance.tempUpgrade;
        permUpgrade = DataManger.Instance.permUpgrade;
        auto = DataManger.Instance.auto;

        DefaultSetting();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void DefaultSetting()
    {
        buyLevel.value = auto.buyLevel;
        upgradeLevel.value = auto.upgradeLevel;
        upgradeGrade.value = auto.upgradeGrade;
        autoBuyCheck.isOn = auto.autoBuyOn;
        autoUpgradeCheck.isOn = auto.autoUpgradeOn;

        if (auto.autoBuyOn) autoBuy = StartCoroutine(AutoBuyUnit());
        if (auto.autoUpgradeOn) autoUpgrade = StartCoroutine(AutoUpgradeUnit());
    }

    public void Reset()
    {
        // auto 수치 기반으로 드롭다운 value 값 초기화
    }

    public void SwitchAutoBuy(bool value)
    {
        Debug.Log(value);
        if (value)
        {
            Debug.Log("활성화");
            autoBuy = StartCoroutine(AutoBuyUnit());
        }
        else
        {
            if (autoBuy != null)
            {
                Debug.Log("비활성화");
                StopCoroutine(autoBuy);
                autoBuy = null;
            }
        }
        auto.autoBuyOn = value;
    }

    public void SwitchAutoUpgrade(bool value)
    {
        if (value)
        {
            autoUpgrade = StartCoroutine(AutoUpgradeUnit());
        }
        else
        {
            if (autoUpgrade != null)
            {
                StopCoroutine(autoUpgrade);
                autoUpgrade = null;
            }
        }
        auto.autoUpgradeOn = value;
    }

    public void SetAutoBuy(int value)
    {
        auto.buyLevel = value;
        if (autoBuyCheck.isOn)
        {
            if (autoBuy != null)
            {
                StopCoroutine(autoBuy);
                autoBuy = null;
            }
            autoBuy = StartCoroutine(AutoBuyUnit());
        }
    }

    public void SetAutoUpgradeLevel(int value)
    {
        auto.upgradeLevel = value;
        if (autoUpgradeCheck.isOn)
        {
            if (autoUpgrade != null)
            {
                StopCoroutine(autoUpgrade);
                autoUpgrade = null;
            }
            autoUpgrade = StartCoroutine(AutoUpgradeUnit());
        }
        
    }

    public void SetAutoUpgradeGrade(int value)
    {
        auto.upgradeGrade = value;
        if (autoUpgradeCheck.isOn)
        {
            if (autoUpgrade != null)
            {
                StopCoroutine(autoUpgrade);
                autoUpgrade = null;
            }
            autoUpgrade = StartCoroutine(AutoUpgradeUnit());
        }
    }

    private IEnumerator AutoBuyUnit()
    {
        // 매 초 목표 유닛 자동 구매
        int unitId = auto.buyLevel;
        
        UnitData unitData = DataManger.Instance.GetUnitData(unitId);
        while (true)
        {
            UnitInfo unitInfo = new UnitInfo
            {
                id = unitId,
                upgrade = 0,
                place = 0
            };
            // 실시간 골드 출력 수정
            // 실시간 유닛 목록 초기화
            unit.units.Add(unitInfo);
            Debug.Log("유닛 자동 구매 : " + unitInfo.id + " " + auto.buyLevel);
            goods.gold -= Mathf.RoundToInt(unitData.price * (1 - tempUpgrade.upgrade[5] * DataManger.Instance.GetTempUpgradeData(5).value));
            OnBuyUnit?.Invoke();
            UIManager.Instance.SetGoldText();
            yield return waitForOneSecond;
        }
    }

    private IEnumerator AutoUpgradeUnit()
    {
        // 매 초 목표 유닛보다 낮은 단계의 유닛 자동 강화
        while (true)
        {
            for (int i = 0; i < unit.units.Count; i++)
            {
                if (unit.units[i].place != 0) continue;
                if (unit.units[i].id < auto.upgradeLevel || (unit.units[i].id == auto.upgradeLevel && unit.units[i].upgrade < auto.upgradeGrade))
                {
                    OnUpgradeUnit?.Invoke(unit.units[i]);
                }
                }
            yield return waitForOneSecond;
        }
    }
}
