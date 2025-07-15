using TMPro;
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

        if (tempUpgrade.canUpgrade.Count == 0 && tempUpgrade.complete.Count == 0)
        {
            //tempUpgrade.canUpgrade.Add(0);
        }
    }

    public void Reset()
    {
        foreach (Transform transform in unitParent)
        {
            Destroy(transform.gameObject);
        }

        foreach (Transform transform in upgradeParent)
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
            unitItem.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => BuyUnit(id));
        }

        tempUpgrade.canUpgrade.Sort();
        foreach (int id in tempUpgrade.canUpgrade)
        {
            TempUpgradeData tempUpgradeData = DataManger.Instance.GetTempUpgradeData(id);

            GameObject upgradeItem = Instantiate(upgradePrefab, upgradeParent);
            upgradeItem.transform.GetChild(1).GetComponent<TMP_Text>().text = tempUpgradeData.dataName;
            upgradeItem.transform.GetChild(2).GetComponent<TMP_Text>().text = tempUpgradeData.price + " 골드";
            upgradeItem.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => BuyUpgrade(id));
        }
    }

    private void BuyUnit(int id)
    {
        UnitData unitData = DataManger.Instance.GetUnitData(id);

        if (goods.gold >= unitData.price)
        {
            goods.gold -= unitData.price;
            UnitInfo unitInfo = new UnitInfo
            {
                id = id,
                upgrade = 0,
                place = 0
            };
            unit.units.Add(unitInfo);
            // 골드 텍스트 수정 추가
            if (!shopUnit.canBuy.Contains(unitData.nextUnit))
            {
                shopUnit.canBuy.Add(unitData.nextUnit);
            }
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
        }

        Reset();
    }

    private void BuyUpgrade(int id)
    {
        TempUpgradeData tempUpgradeData = DataManger.Instance.GetTempUpgradeData(id);

        if (goods.gold >= tempUpgradeData.price)
        {
            goods.gold -= tempUpgradeData.price;
            tempUpgrade.complete.Add(id);
            tempUpgrade.canUpgrade.Remove(id);
            foreach (int next in tempUpgradeData.nextUpgrade)
            {
                tempUpgrade.canUpgrade.Add(next);
            }
            // 골드 텍스트 수정 추가
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
        }
        Reset();
    }
}
