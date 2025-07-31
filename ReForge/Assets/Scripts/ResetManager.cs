using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResetManager : MonoBehaviour, IWindow
{
    [SerializeField] private Button[] up;
    [SerializeField] private TMP_Text point;

    private PermUpgrade permUpgrade;
    async void Start()
    {
        permUpgrade = DataManger.Instance.permUpgrade;
        SetPointText();

        await DataManger.Instance.WaitForLoadingPermUpgradeData();

        for (int i = 0; i < up.Length; i++)
        {
            int index = i;
            up[index].onClick.AddListener(() => BuyUpgrade(index));
            up[index].GetComponent<Tooltip>().SetUpgrade(index);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Reset()
    {

    }

    private void BuyUpgrade(int index)
    {
        PermUpgradeData permUpgradeData = DataManger.Instance.GetPermUpgradeData(index);
        if (permUpgrade.upPoint >= permUpgradeData.price)
        {
            permUpgrade.complete.Add(index);
            permUpgrade.upPoint -= permUpgradeData.price;
            up[index].enabled = false;
            up[index].GetComponent<Image>().color = Color.gray;
            SetPointText();
        }
    }

    private void SetPointText()
    {
        point.text = permUpgrade.upPoint + " 포인트";
    }

    // 버튼 더블 클릭으로 업그레이드 구매 (UP포인트 비교)
}
