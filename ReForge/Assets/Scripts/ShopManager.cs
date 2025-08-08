using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour, IWindow
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject upgradePrefab;
    [SerializeField] private Transform unitParent;
    [SerializeField] private Transform upgradeParent;

    public Unit unit;
    public Goods goods;
    public TempUpgrade tempUpgrade;
    public PermUpgrade permUpgrade;
    public ShopUnit shopUnit;

    void Start()
    {
        unit = DataManger.Instance.unit;
        goods = DataManger.Instance.goods;
        tempUpgrade = DataManger.Instance.tempUpgrade;
        permUpgrade = DataManger.Instance.permUpgrade;
        shopUnit = DataManger.Instance.shopUnit;

        InitSetting();
    }

    private void InitSetting()
    {
        if (shopUnit.canBuy.Count == 0)
        {
            shopUnit.canBuy.Add(0);
            Debug.Log("초기 리스트 추가");
        }
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
            unitItem.transform.GetChild(2).GetComponent<TMP_Text>().text = unitData.price + " 골드";
            unitItem.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => BuyUnit(id));
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
            upgradeItem.transform.GetChild(2).GetComponent<TMP_Text>().text = tempUpgradeData.price + " 골드";
            upgradeItem.transform.GetChild(4).GetComponent<TMP_Text>().text = "LV." + tempUpgrade.upgrade[id];
            upgradeItem.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => BuyUpgrade(id));
        }
    }

    private void BuyUnit(int id)
    {
        UnitData unitData = DataManger.Instance.GetUnitData(id);
        int finalPrice = unitData.price;
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

        if (goods.gold >= tempUpgradeData.price)
        {
            goods.gold -= tempUpgradeData.price;
            tempUpgrade.upgrade[id]++;
            // 골드 텍스트 수정 추가
            UIManager.Instance.SetGoldText();
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
        }
        ResetUpgrade();
        ResetUnit();
    }
}
