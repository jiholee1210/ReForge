using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [SerializeField] private GameObject tooltip;

    void Start()
    {
        Instance = this;
    }

    public void ShowTooltip(PermUpgradeData permUpgradeData, Vector3 pos)
    {
        //null 인 경우 생각
        tooltip.SetActive(true);
        tooltip.transform.GetChild(0).GetComponent<TMP_Text>().text = permUpgradeData.dataName;
        tooltip.transform.GetChild(1).GetComponent<TMP_Text>().text = permUpgradeData.desc;

        tooltip.GetComponent<RectTransform>().anchoredPosition = pos + new Vector3(40f, 0, 0);
    }

    public void HideTooltip()
    {
        tooltip.SetActive(false);
    }
}