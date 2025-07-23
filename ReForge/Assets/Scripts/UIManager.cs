using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set;}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private TMP_Text goldText;

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
    }
}
