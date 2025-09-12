using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    // 모든 업그레이드 수치를 저장하는 클래스
    // 여러 클래스에서 직접 데이터를 검색해서 수치를 입력할 필요없이 현재 클래스에 접근해서 사용하면 편리

    // 각 temp 업그레이드 데이터 
    // 각 perm 업그레이드 데이터
    // 각 relic 데이터

    private Dictionary<UpgradeType, TempUpgradeData> tempUpgradeDict = new();
    private Dictionary<UpgradeType, PermUpgradeData> permUpgradeDict = new();
    private Dictionary<UpgradeType, RelicData> relicDict = new();

    private PermUpgrade permUpgrade;
    private TempUpgrade tempUpgrade;
    private Relic relic;

    async void Start()
    {
        await DataManger.Instance.WaitForLoadingTempUpgradeData();
        await DataManger.Instance.WaitForLoadingPermUpgradeData();

        for (int i = 0; i < DataManger.Instance.tempUpgradeDataDict.Count; i++)
        {
            TempUpgradeData temp = DataManger.Instance.tempUpgradeDataDict[i];
            tempUpgradeDict.Add(temp.upgradeType, temp);
        }

        for (int i = 0; i < DataManger.Instance.permUpgradeDataDict.Count; i++)
        {
            PermUpgradeData perm = DataManger.Instance.permUpgradeDataDict[i];
            permUpgradeDict.Add(perm.upgradeType, perm);
        }

        for (int i = 0; i < DataManger.Instance.relicDataDict.Count; i++)
        {
            RelicData relic = DataManger.Instance.relicDataDict[i];
            relicDict.Add(relic.upgradeType, relic);
        }

        permUpgrade = DataManger.Instance.permUpgrade;
        tempUpgrade = DataManger.Instance.tempUpgrade;
        relic = DataManger.Instance.relic;
    }

    public float GetPlusReinforce()
    {
        PermUpgradeData plusReinforce = permUpgradeDict[UpgradeType.PlusReinforce];
        RelicData relicData = relicDict[UpgradeType.PlusReinforce];
        float value = permUpgrade.complete.Contains(plusReinforce.id) ? plusReinforce.value : 0
            + relic.relics[relicData.id] * relicData.value;
        return value;
    }

    public float GetUpgradeDis()
    {
        PermUpgradeData upDis = permUpgradeDict[UpgradeType.UpgradeDiscount];
        RelicData relicData = relicDict[UpgradeType.UpgradeDiscount];
        float value = (1 - (permUpgrade.complete.Contains(upDis.id) ? upDis.value : 0))
            * (1 - (relic.relics[relicData.id] * relicData.value));
        return value;
    }

    public float GetUnitDis()
    {
        PermUpgradeData unitDis = permUpgradeDict[UpgradeType.UnitDiscount];
        RelicData relicData = relicDict[UpgradeType.UnitDiscount];
        float value = (1 - (permUpgrade.complete.Contains(unitDis.id) ? unitDis.value : 0))
            * (1 - (relic.relics[relicData.id] * relicData.value));
        return value;
    }

    public float GetBaseGold()
    {
        PermUpgradeData baseGold = permUpgradeDict[UpgradeType.BaseGold];
        RelicData relicData = relicDict[UpgradeType.BaseGold];
        float value = permUpgrade.complete.Contains(baseGold.id) ? baseGold.value : 0
            + relic.relics[relicData.id] * relicData.value;
        return value;
    }

    public float GetUpPoint()
    {
        PermUpgradeData upPoint = permUpgradeDict[UpgradeType.UpPointGain];
        RelicData relicData = relicDict[UpgradeType.UpPointGain];
        float value = (1 + (permUpgrade.complete.Contains(upPoint.id) ? upPoint.value : 0))
            * (1 + (relic.relics[relicData.id] * relicData.value));
        return value;
    }

    public float GetUnitPower()
    {
        TempUpgradeData temp = tempUpgradeDict[UpgradeType.UnitPower];
        PermUpgradeData perm = permUpgradeDict[UpgradeType.UnitPower];
        RelicData relicData = relicDict[UpgradeType.UnitPower];
        float value = (1 + tempUpgrade.upgrade[temp.id] * temp.value)
            * (1 + (permUpgrade.complete.Contains(perm.id) ? perm.value : 0))
            * (1 + (relic.relics[relicData.id] * relicData.value));
        return value;
    }

    public float GetWorkSpeed()
    {
        TempUpgradeData temp = tempUpgradeDict[UpgradeType.WorkSpeed];
        PermUpgradeData perm = permUpgradeDict[UpgradeType.WorkSpeed];
        RelicData relicData = relicDict[UpgradeType.WorkSpeed];
        float value = (1 - tempUpgrade.upgrade[temp.id] * temp.value)
            * (1 - (permUpgrade.complete.Contains(perm.id) ? perm.value : 0))
            * (1 - (relic.relics[relicData.id] * relicData.value));
        return value;
    }

    public float GetReinforcePos()
    {
        TempUpgradeData temp = tempUpgradeDict[UpgradeType.ReinforcePosibility];
        PermUpgradeData perm = permUpgradeDict[UpgradeType.ReinforcePosibility];
        RelicData relicData = relicDict[UpgradeType.ReinforcePosibility];
        float value = tempUpgrade.upgrade[temp.id] * temp.value
            * (1 + (permUpgrade.complete.Contains(perm.id) ? perm.value : 0))
            * (1 + (relic.relics[relicData.id] * relicData.value));
        return value;
    }

    public float GetGoldGain()
    {
        TempUpgradeData temp = tempUpgradeDict[UpgradeType.GoldGain];
        PermUpgradeData perm = permUpgradeDict[UpgradeType.GoldGain];
        RelicData relicData = relicDict[UpgradeType.GoldGain];
        float value = (1 + tempUpgrade.upgrade[temp.id] * temp.value)
            * (1 + (permUpgrade.complete.Contains(perm.id) ? perm.value : 0))
            * (1 + (relic.relics[relicData.id] * relicData.value));
        return value;
    }

    public float GetCritPos()
    {
        PermUpgradeData perm = permUpgradeDict[UpgradeType.CritPos];
        RelicData relicData = relicDict[UpgradeType.CritPos];
        float value = permUpgrade.complete.Contains(perm.id) ? perm.value : 0
            + relic.relics[relicData.id] * relicData.value;
        return value;
    }

    public float GetCritValue()
    {
        PermUpgradeData perm = permUpgradeDict[UpgradeType.CritValue];
        RelicData relicData = relicDict[UpgradeType.CritValue];
        float value = 1 + (permUpgrade.complete.Contains(perm.id) ? perm.value : 0)
            * (1 + (relic.relics[relicData.id] * relicData.value));
        return value;
    }

    public float GetGoldPerTime()
    {
        PermUpgradeData perm = permUpgradeDict[UpgradeType.GoldPerTime];
        RelicData relicData = relicDict[UpgradeType.GoldPerTime];
        float value = permUpgrade.complete.Contains(perm.id) ? perm.value : 0
            + relic.relics[relicData.id] * relicData.value;
        return value;
    }

    public float GetRelicDis()
    {
        PermUpgradeData perm = permUpgradeDict[UpgradeType.RelicDiscount];
        float value = 1 - (permUpgrade.complete.Contains(perm.id) ? perm.value : 0);
        return value;
    }

    public float GetRelicValue()
    {
        PermUpgradeData perm = permUpgradeDict[UpgradeType.RelicValue];
        RelicData relicData = relicDict[UpgradeType.RelicValue];
        float value = 1 + (permUpgrade.complete.Contains(perm.id) ? perm.value : 0)
            * (1 + (relic.relics[relicData.id] * relicData.value));
        return value;
    }

    public float GetDestroyFrag()
    {
        PermUpgradeData perm = permUpgradeDict[UpgradeType.DestroyFrag];
        RelicData relicData = relicDict[UpgradeType.DestroyFrag];
        float value = 1 + (permUpgrade.complete.Contains(perm.id) ? perm.value : 0)
            * (1 + (relic.relics[relicData.id] * relicData.value));
        return value;
    }

    public float GetDestroyGain()
    {
        PermUpgradeData perm = permUpgradeDict[UpgradeType.DestroyGain];
        RelicData relicData = relicDict[UpgradeType.DestroyGain];
        float value = 1 + (permUpgrade.complete.Contains(perm.id) ? perm.value : 0)
            * (1 + (relic.relics[relicData.id] * relicData.value));
        return value;
    }

    public bool GetAuto()
    {
        PermUpgradeData perm = permUpgradeDict[UpgradeType.Auto];
        bool value = permUpgrade.complete.Contains(perm.id) ? true : false;
        return value;
    }

    public bool GetRelic()
    {
        PermUpgradeData perm = permUpgradeDict[UpgradeType.Relic];
        bool value = permUpgrade.complete.Contains(perm.id) ? true : false;
        return value;
    }

    public bool GetFinal()
    {
        PermUpgradeData perm = permUpgradeDict[UpgradeType.Final];
        bool value = permUpgrade.complete.Contains(perm.id) ? true : false;
        return value;
    }
}