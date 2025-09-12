using TMPro;
using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text fragText;

    [SerializeField] private RectTransform goldUI;
    [SerializeField] private RectTransform fragUI;
    [SerializeField] private GameObject relic;

    private Goods goods;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        goods = DataManger.Instance.goods;
        SetGoldText();
        SetFragText();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetGoldText()
    {
        if (goods.gold >= 10000000)
        {
            goldText.text = goods.gold.ToString("e2");
        }
        else
        {
            goldText.text = goods.gold.ToString();
        }
        
    }

    public void SetFragText()
    {
        Debug.Log(goods.frag);
        if (goods.frag >= 10000000)
        {
            fragText.text = goods.frag.ToString("e2");
        }
        else
        {
            fragText.text = goods.frag.ToString();
        }
    }

    public void GoldEffect()
    {
        goldUI.localScale = Vector3.one;

        goldUI.DOKill();
        goldUI.DOPunchScale(Vector3.one * 0.3f, 0.1f, 1, 1f);
    }

    public void FragEffect()
    {
        fragUI.localScale = Vector3.one;

        fragUI.DOKill();
        fragUI.DOPunchScale(Vector3.one * 0.3f, 0.1f, 1, 1f);
    }

    public void ActiveRelic()
    {
        relic.SetActive(true);
    }
}
