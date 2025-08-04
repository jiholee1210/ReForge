using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [SerializeField] private GameObject tooltip;

    private bool isPointerOver = false;
    private Vector3 newPos;

    void Start()
    {
        Instance = this;
    }

    void Update()
    {
        if (isPointerOver)
        {
            tooltip.transform.position = newPos;
            tooltip.GetComponent<RectTransform>().anchoredPosition += new Vector2(40f, 0);
        }
    }

    public void ShowTooltip(PermUpgradeData permUpgradeData, Vector3 pos)
    {
        //null 인 경우 생각
        isPointerOver = true;

        tooltip.SetActive(true);
        tooltip.transform.GetChild(0).GetComponent<TMP_Text>().text = permUpgradeData.dataName;
        tooltip.transform.GetChild(1).GetComponent<TMP_Text>().text = permUpgradeData.desc;
    }

    public void HideTooltip()
    {
        isPointerOver = false;
        tooltip.SetActive(false);
    }

    public void ResetPos(Vector3 pos)
    {
        newPos = pos;
    }
}