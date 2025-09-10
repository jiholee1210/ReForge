
using System.Collections;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RelicManager : MonoBehaviour, IWindow
{
    [SerializeField] private GameObject relicPrefab;
    [SerializeField] private Transform relicParent;
    [SerializeField] private Transform detail;
    [SerializeField] private Button pick;

    [SerializeField] private GameObject roulette;
    [SerializeField] private RectTransform content;
    [SerializeField] private Animator rouletteCon;

    [SerializeField] private TMP_Text fragText;
    // 유물 목록 생성
    private PermUpgrade permUpgrade;
    private Goods goods;
    private Relic relic;

    private float itemWidth = 20f;

    private int count;
    private int select = 7;
    private bool isSpinning = false;

    private int reqFrag = 0;
    private Vector2 prevPos;
    async void Start()
    {
        permUpgrade = DataManger.Instance.permUpgrade;
        goods = DataManger.Instance.goods;
        relic = DataManger.Instance.relic;

        pick.onClick.AddListener(() => RandomPick());

        await DataManger.Instance.WaitForLoadingRelicData();
        InitSetting();
        prevPos = pick.GetComponent<RectTransform>().anchoredPosition;
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
        if (!relic.relics.Contains(0) || isSpinning) return;

        if (goods.frag < reqFrag)
        {
            RectTransform target = pick.GetComponent<RectTransform>();
    
            target.DOKill(true);
            target.anchoredPosition = prevPos;

            Sequence seq = DOTween.Sequence();
            seq.Append(target.DOAnchorPosX(target.anchoredPosition.x + 1.5f, 0.05f));
            seq.Append(target.DOAnchorPosX(target.anchoredPosition.x - 1.5f, 0.05f));
            seq.SetLoops(2, LoopType.Yoyo).OnComplete(() => target.anchoredPosition = prevPos);
            return;
        }
        goods.frag -= reqFrag;
        UIManager.Instance.SetFragText();
        isSpinning = true;
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
        relic.count++;
        SpinRelic(random);
        Debug.Log(random + "번 유물 획득");
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
        reqFrag = Mathf.RoundToInt(Mathf.Pow(1000, relic.count + 1));
        fragText.text = reqFrag >= 10000000 ? reqFrag.ToString("e2") : reqFrag.ToString();
    }

    private void SpinRelic(int id)
    {
        roulette.SetActive(true);
        rouletteCon.SetTrigger("open");
        content.anchoredPosition = Vector2.zero;
        count = 0;
        for (int i = 0; i < content.childCount; i++)
        {
            int index = i;
            int random = Random.Range(0, 16);
            content.GetChild(index).GetComponent<RectTransform>().anchoredPosition = new Vector2(-60 + index * itemWidth, 0);
            content.GetChild(index).GetComponent<Image>().sprite = DataManger.Instance.GetRelicData(random).sprite;
            if (index == select)
            {
                content.GetChild(select).GetComponent<Image>().sprite = DataManger.Instance.GetRelicData(id).sprite;
            }
            content.GetChild(index).GetComponent<Image>().SetNativeSize();
        }

        DOTween.To(
            () => content.anchoredPosition.x,
            x => content.anchoredPosition = new Vector2(x, 0),
            -480f, 2.5f)
            .SetEase(Ease.OutCubic)
            .OnUpdate(() =>
            {
                RepositionItem();
            })
            .OnComplete(() =>
            {
                Highlight();
            });
    }

    private void RepositionItem()
    {
        int index = count % 10;
        if (content.anchoredPosition.x < -72 - (itemWidth * count))
        {
            content.GetChild(index).GetComponent<RectTransform>().anchoredPosition += new Vector2(200f, 0);
            count++;
        }
    }

    private void Highlight()
    {
        RectTransform target = content.GetChild(select).GetComponent<RectTransform>();

        target.DOScale(1.2f, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(1f, () =>
                {
                    rouletteCon.SetTrigger("close");
                    float length = rouletteCon.GetCurrentAnimatorStateInfo(0).length;
                    DOVirtual.DelayedCall(length, () =>
                    {
                        roulette.SetActive(false);
                        target.localScale = new Vector3(1f, 1f, 1f);
                        isSpinning = false;
                        RenewRelic();
                    });
                });
            }); 
    }            
}