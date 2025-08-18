using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour, IWindow
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject upgradePrefab;
    [SerializeField] private Transform unitParent;
    [SerializeField] private Transform upgradeParent;

    private Unit unit;
    private Goods goods;
    private TempUpgrade tempUpgrade;
    private PermUpgrade permUpgrade;
    private ShopUnit shopUnit;

    private KeyValuePair<int, PermUpgradeData> unitDiscount;
    private KeyValuePair<int, PermUpgradeData> upDiscount;
    private KeyValuePair<int, PermUpgradeData> unitPowerPerm;
    private KeyValuePair<int, TempUpgradeData> unitPowerTemp;

    async void Start()
    {
        unit = DataManger.Instance.unit;
        goods = DataManger.Instance.goods;
        tempUpgrade = DataManger.Instance.tempUpgrade;
        permUpgrade = DataManger.Instance.permUpgrade;
        shopUnit = DataManger.Instance.shopUnit;

        await DataManger.Instance.WaitForLoadingPermUpgradeData();
        await DataManger.Instance.WaitForLoadingTempUpgradeData();

        unitDiscount = DataManger.Instance.permUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.UnitDiscount);

        upDiscount = DataManger.Instance.permUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.UpgradeDiscount);

        unitPowerPerm = DataManger.Instance.permUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.UnitPower);

        unitPowerTemp = DataManger.Instance.tempUpgradeDataDict
            .FirstOrDefault(pair => pair.Value.upgradeType == UpgradeType.UnitPower);
    }

    public void Reset()
    {
        ResetUnit();
        ResetUpgrade();
    }

    private void ResetUnit()
    {
        foreach (Transform transform in unitParent)
        {
            Destroy(transform.gameObject);
        }

        shopUnit.canBuy.Sort();
        foreach (int id in shopUnit.canBuy)
        {
            UnitData unitData = DataManger.Instance.GetUnitData(id);

            GameObject unitItem = Instantiate(unitPrefab, unitParent);
            unitItem.transform.GetChild(0).GetComponent<Image>().sprite = unitData.sprite;
            unitItem.transform.GetChild(0).GetComponent<Image>().SetNativeSize();
            unitItem.transform.GetChild(1).GetComponent<TMP_Text>().text = unitData.dataName;
            unitItem.transform.GetChild(2).GetComponent<TMP_Text>().text = Mathf.RoundToInt(unitData.power * (1 + (tempUpgrade.upgrade[unitPowerTemp.Key] * unitPowerTemp.Value.value)) * (1 + (permUpgrade.complete.Contains(unitPowerPerm.Key) ? unitPowerPerm.Value.value : 0))) + " 작업량";
            unitItem.transform.GetChild(3).GetComponent<TMP_Text>().text = Mathf.RoundToInt(unitData.price * (1 - (permUpgrade.complete.Contains(unitDiscount.Key) ? unitDiscount.Value.value : 0))) + " 골드";
            unitItem.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => BuyUnit(id));
        }
    }

    private void ResetUpgrade()
    {
        foreach (Transform transform in upgradeParent)
        {
            Destroy(transform.gameObject);
        }

        for (int i = 0; i < tempUpgrade.upgrade.Length; i++)
        {
            int id = i;
            TempUpgradeData tempUpgradeData = DataManger.Instance.GetTempUpgradeData(id);

            GameObject upgradeItem = Instantiate(upgradePrefab, upgradeParent);
            upgradeItem.transform.GetChild(0).GetComponent<Image>().sprite = tempUpgradeData.sprite;
            upgradeItem.transform.GetChild(1).GetComponent<TMP_Text>().text = tempUpgradeData.dataName;
            if (tempUpgradeData.max != -1)
            {
                upgradeItem.transform.GetChild(2).GetComponent<TMP_Text>().text = Mathf.RoundToInt(tempUpgradeData.valueList[tempUpgrade.upgrade[id]]) + " 골드";
                if (tempUpgradeData.max <= tempUpgrade.upgrade[id])
                {
                    upgradeItem.transform.GetChild(4).GetComponent<TMP_Text>().text = "LV.MAX";
                    upgradeItem.transform.GetChild(5).gameObject.SetActive(true);
                }
                else
                {
                    upgradeItem.transform.GetChild(4).GetComponent<TMP_Text>().text = "LV." + tempUpgrade.upgrade[id];
                    upgradeItem.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => BuyUpgrade(id));
                }
            }
            else
            {
                upgradeItem.transform.GetChild(2).GetComponent<TMP_Text>().text = Mathf.RoundToInt(tempUpgradeData.price * (1 - (permUpgrade.complete.Contains(upDiscount.Key) ? upDiscount.Value.value : 0))
                                                                                                                        * MathF.Pow(1.5f, tempUpgrade.upgrade[id])) + " 골드";

                upgradeItem.transform.GetChild(4).GetComponent<TMP_Text>().text = "LV." + tempUpgrade.upgrade[id];
                upgradeItem.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => BuyUpgrade(id));
            }
        }
    }

    private void BuyUnit(int id)
    {
        UnitData unitData = DataManger.Instance.GetUnitData(id);
        int finalPrice = Mathf.RoundToInt(unitData.price * (1 - (permUpgrade.complete.Contains(unitDiscount.Key) ? unitDiscount.Value.value : 0)));
        if (goods.gold >= finalPrice)
        {
            goods.gold -= finalPrice;
            UnitInfo unitInfo = new UnitInfo
            {
                id = id,
                upgrade = 0,
                place = 0
            };
            unit.units.Add(unitInfo);
            unit.units.Sort((a, b) =>
            {
                int cmp = a.id.CompareTo(b.id);
                if (cmp != 0) return cmp;

                return a.upgrade.CompareTo(b.upgrade);
            });
            // 골드 텍스트 수정 추가
            UIManager.Instance.SetGoldText();
            if (!shopUnit.canBuy.Contains(unitData.nextUnit))
            {
                shopUnit.canBuy.Add(unitData.nextUnit);
            }
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
        }

        ResetUnit();
        DataManger.Instance.SaveAll();
    }

    private void BuyUpgrade(int id)
    {
        TempUpgradeData tempUpgradeData = DataManger.Instance.GetTempUpgradeData(id);

        int totalPrice = tempUpgradeData.max == -1
                        ? Mathf.RoundToInt(tempUpgradeData.price * (1 - (permUpgrade.complete.Contains(upDiscount.Key) ? upDiscount.Value.value : 0)) * MathF.Pow(1.5f, tempUpgrade.upgrade[id]))
                        : Mathf.RoundToInt(tempUpgradeData.valueList[tempUpgrade.upgrade[id]]);
        if (goods.gold >= totalPrice)
        {
            goods.gold -= totalPrice;
            tempUpgrade.upgrade[id]++;
            // 골드 텍스트 수정 추가
            UIManager.Instance.SetGoldText();
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
        }
        Reset();
    }

    public void Leave()
    {
    }
}
