using TMPro;
using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set;}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private RectTransform goldUI;

    private Goods goods;
    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        goods = DataManger.Instance.goods;
        SetGoldText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGoldText()
    {
        goldText.text = goods.gold.ToString();

        goldUI.localScale = Vector3.one;

        goldUI.DOKill();
        goldUI.DOPunchScale(Vector3.one * 0.5f, 0.3f, 1, 1f); 
    }
}
