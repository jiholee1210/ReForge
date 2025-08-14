using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResetManager : MonoBehaviour, IWindow
{
    [SerializeField] private Button[] up;
    [SerializeField] private TMP_Text point;
    [SerializeField] private GameObject[] windows;

    private PermUpgrade permUpgrade;
    async void Start()
    {
        permUpgrade = DataManger.Instance.permUpgrade;
        await DataManger.Instance.WaitForLoadingPermUpgradeData();

        for (int i = 0; i < up.Length; i++)
        {
            int index = i;
            PermUpgradeData permUpgradeData = DataManger.Instance.GetPermUpgradeData(index);

            up[index].onClick.AddListener(() => BuyUpgrade(index));
            up[index].GetComponent<Tooltip>().SetUpgrade(index);
            up[index].transform.GetChild(0).GetComponent<Image>().sprite = permUpgradeData.sprite;
        }

        for (int i = 0; i < permUpgrade.open; i++)
        {
            windows[i].SetActive(false);
        }
        foreach (int i in permUpgrade.complete)
        {
            up[i].enabled = true;
            up[i].GetComponent<Image>().color = Color.gray;
        }
    }

    public void Reset()
    {
        SetPointText();
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
            SetWindow();
            DataManger.Instance.SaveAll();
        }
    }

    private void SetPointText()
    {
        point.text = permUpgrade.upPoint + " 포인트";
    }

    private void SetWindow()
    {
        Debug.Log("완료 업그레이드 수 : " + permUpgrade.complete.Count);
        switch (permUpgrade.complete.Count)
        {
            case 1:
                windows[0].SetActive(false);
                permUpgrade.open = 1;
                break;
            case 4:
                windows[1].SetActive(false);
                permUpgrade.open = 2;
                break;
            case 7:
                windows[2].SetActive(false);
                permUpgrade.open = 3;
                break;
            case 8:
                windows[3].SetActive(false);
                permUpgrade.open = 4;
                break;
            case 10:
                windows[4].SetActive(false);
                permUpgrade.open = 5;
                break;
        }
    }

    public void Leave()
    {
    }
}
