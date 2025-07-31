using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private PermUpgradeData permUpgradeData;
    private Vector2 pos;
    void Start()
    {
        pos = GetComponent<RectTransform>().anchoredPosition;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManager.Instance.ShowTooltip(permUpgradeData, pos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.HideTooltip();
    }

    public void SetUpgrade(int id)
    {
        permUpgradeData = DataManger.Instance.GetPermUpgradeData(id);
    }
}
