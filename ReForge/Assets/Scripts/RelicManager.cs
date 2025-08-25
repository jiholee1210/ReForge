
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicManager : MonoBehaviour, IWindow
{
    [SerializeField] private GameObject relicPrefab;
    [SerializeField] private Transform relicParent;
    [SerializeField] private Transform detail;
    [SerializeField] private Button pick;
    // 유물 목록 생성
    private PermUpgrade permUpgrade;
    private Goods goods;
    private Relic relic;

    async void Start()
    {
        permUpgrade = DataManger.Instance.permUpgrade;
        goods = DataManger.Instance.goods;
        relic = DataManger.Instance.relic;

        pick.onClick.AddListener(() => RandomPick());

        await DataManger.Instance.WaitForLoadingRelicData();
        InitSetting();
    }

    public void Reset()
    {
        RenewRelic();
        detail.gameObject.SetActive(false);
    }

    public void Leave()
    {
        
    }

    private void RandomPick()
    {
        // 유물 랜덤 뽑기
        // relics 리스트에 존재하지 않는 id의 유물만 획득하도록 구현
        // 현재 소지 유물 갯수에 비례해서 뽑기 비용 상승
        int random = Random.Range(0, relic.relics.Length);

        int loop = 0;
        while (relic.relics[random] == 1)
        {
            random = Random.Range(0, relic.relics.Length);
            if (loop++ > 1000)
            {
                throw new System.Exception("무한 루프");
            }
        }

        relic.relics[random] = 1;
        Debug.Log(random + "번 유물 획득");
        RenewRelic();
        // 비용 부분 구현 필요
    }

    private void InitSetting()
    {
        for (int i = 0; i < relic.relics.Length; i++)
        {
            int id = i;
            GameObject item = Instantiate(relicPrefab, relicParent);
            item.transform.GetChild(0).GetComponent<Image>().sprite = DataManger.Instance.GetRelicData(id).sprite;
            item.transform.GetChild(0).GetComponent<Image>().SetNativeSize();
            item.GetComponent<Button>().onClick.AddListener(() => ShowDetail(id));
        }
    }

    private void ShowDetail(int id)
    {
        // 세부 데이터 표시
        RelicData relicData = DataManger.Instance.GetRelicData(id);

        detail.gameObject.SetActive(true);
        detail.GetChild(0).GetComponent<TMP_Text>().text = relicData.dataName;
        detail.GetChild(1).GetComponent<TMP_Text>().text = relicData.desc;
    }

    private void RenewRelic()
    {
        // 미획득 유물은 ? 표시
        // 획득 유물은 이미지 출력 및 클릭 시 데이터 표시
        // relic 리스트 불러오기 => 존재하는 id 값으로 소지 유무 판단

        for (int i = 0; i < relic.relics.Length; i++)
        {
            if (relic.relics[i] == 1)
            {
                relicParent.GetChild(i).GetChild(1).gameObject.SetActive(false);
                relicParent.GetChild(i).GetComponent<Button>().enabled = true;
            }
            else
            {
                relicParent.GetChild(i).GetChild(1).gameObject.SetActive(true);
                relicParent.GetChild(i).GetComponent<Button>().enabled = false;
            }
        }
    }

    
}