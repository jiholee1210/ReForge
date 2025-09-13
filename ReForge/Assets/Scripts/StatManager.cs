using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class StatManager : MonoBehaviour, IWindow
{
    [SerializeField] private StatText[] statTexts;

    private PermUpgrade permUpgrade;
    private TempUpgrade tempUpgrade;

    private Dictionary<UpgradeType, KeyValuePair<int, TempUpgradeData>> tempUpgrades;
    private Dictionary<UpgradeType, KeyValuePair<int, PermUpgradeData>> permUpgrades;

    async void Start()
    {
        permUpgrade = DataManger.Instance.permUpgrade;
        tempUpgrade = DataManger.Instance.tempUpgrade;

        await DataManger.Instance.WaitForLoadingPermUpgradeData();
        await DataManger.Instance.WaitForLoadingTempUpgradeData();

        tempUpgrades = Enum.GetValues(typeof(UpgradeType))
        .Cast<UpgradeType>()
        .ToDictionary(
            type => type,
            type => DataManger.Instance.tempUpgradeDataDict.FirstOrDefault(pair => pair.Value.upgradeType == type)
        );

        permUpgrades = Enum.GetValues(typeof(UpgradeType))
        .Cast<UpgradeType>()
        .ToDictionary(
            type => type,
            type => DataManger.Instance.permUpgradeDataDict.FirstOrDefault(pair => pair.Value.upgradeType == type)
        );
    }

    void OnEnable()
    {
        DataManger.OnTryReset += Reset;
    }

    void OnDisable()
    {
        DataManger.OnTryReset -= Reset;
    }

    private void SetStat()
    {
        for (int i = 0; i < statTexts.Length; i++)
        {
            CalValue(statTexts[i].type, statTexts[i].text);
        }
    }

    private void CalValue(UpgradeType type, TMP_Text text)
    {
        int value;
        switch (type)
        {
            case UpgradeType.PlusReinforce:
                value = Mathf.RoundToInt(permUpgrade.complete.Contains(permUpgrades[type].Key) ? permUpgrades[type].Value.value : 0);
                text.text = value + "%";
                break;
            case UpgradeType.ReinforcePosibility:
                value = Mathf.RoundToInt(tempUpgrades[type].Value.value * tempUpgrade.upgrade[tempUpgrades[type].Key] * (1 + (permUpgrade.complete.Contains(permUpgrades[type].Key) ? permUpgrades[type].Value.value : 0)));                
                text.text = value + "%";
                break;
            case UpgradeType.UpgradeDiscount:
            case UpgradeType.UnitDiscount:
                value = Mathf.RoundToInt((permUpgrade.complete.Contains(permUpgrades[type].Key) ? permUpgrades[type].Value.value : 0) * 100);
                text.text = value + "%";
                break;
            case UpgradeType.BaseGold:
                value = Mathf.RoundToInt(permUpgrade.complete.Contains(permUpgrades[type].Key) ? permUpgrades[type].Value.value : 0);
                text.text = value + " 골드";
                break;
            case UpgradeType.UpPointGain:
                value = Mathf.RoundToInt((1 + (permUpgrade.complete.Contains(permUpgrades[type].Key) ? permUpgrades[type].Value.value : 0)) * 100);
                text.text = value + "%";
                break;
            default:
                value = Mathf.RoundToInt((1 + tempUpgrades[type].Value.value * tempUpgrade.upgrade[tempUpgrades[type].Key]) * (1 + (permUpgrade.complete.Contains(permUpgrades[type].Key) ? permUpgrades[type].Value.value : 0)) * 100);
                text.text = value + "%";
                break;
        }
    }


    public void Leave()
    {

    }

    public void Reset()
    {
        SetStat(); 
    }
}

[Serializable]
public class StatText
{
    public UpgradeType type;
    public TMP_Text text;
}