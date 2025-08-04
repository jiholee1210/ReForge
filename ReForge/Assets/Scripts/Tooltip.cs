using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private PermUpgradeData permUpgradeData;
    private bool isPointerOver = false;

    void Update()
    {
        if (isPointerOver)
        {
            TooltipManager.Instance.ResetPos(transform.position);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        TooltipManager.Instance.ShowTooltip(permUpgradeData, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        TooltipManager.Instance.HideTooltip();
    }

    public void SetUpgrade(int id)
    {
        permUpgradeData = DataManger.Instance.GetPermUpgradeData(id);
    }
}
