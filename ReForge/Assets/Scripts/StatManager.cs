using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class StatManager : MonoBehaviour, IWindow
{
    [SerializeField] private TMP_Text[] unit;
    [SerializeField] private TMP_Text[] other;

    private PermUpgrade permUpgrade;
    private TempUpgrade tempUpgrade;

    private Dictionary<UpgradeType, TempUpgradeData> tempUpgrades;
    private Dictionary<UpgradeType, PermUpgradeData> permUpgrades;

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
            type => DataManger.Instance.tempUpgradeDataDict.FirstOrDefault(pair => pair.Value.upgradeType == type).Value
        );

        permUpgrades = Enum.GetValues(typeof(UpgradeType))
        .Cast<UpgradeType>()
        .ToDictionary(
            type => type,
            type => DataManger.Instance.permUpgradeDataDict.FirstOrDefault(pair => pair.Value.upgradeType == type).Value
        );
    }

    public void Leave()
    {
        
    }

    public void Reset()
    {
        
    }
}