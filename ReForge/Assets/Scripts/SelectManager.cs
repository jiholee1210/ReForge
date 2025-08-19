using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SelectManager : MonoBehaviour
{
    [SerializeField] private Button[] button;
    [SerializeField] private GameObject[] selects;
    [SerializeField] private GameObject[] managers;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int prevWindow;
    private bool change = false;
    void Start()
    {
        prevWindow = 0;
        button[prevWindow].GetComponent<Image>().color = Color.green;
        for (int i = 0; i < button.Length; i++)
        {
            int index = i;
            button[index].onClick.AddListener(() => StartCoroutine(OpenWindow(index)));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator OpenWindow(int index)
    {
        if (index == prevWindow || change) yield break;

        change = true;
        button[index].GetComponent<Image>().color = Color.green;
        button[prevWindow].GetComponent<Image>().color = Color.white;

        selects[index].SetActive(true);
        managers[index].GetComponent<IWindow>().Reset();

        bool isLeft = index > prevWindow;
        yield return StartCoroutine(WaitForAnimation(isLeft, index));

        selects[prevWindow].SetActive(false);
        managers[prevWindow].GetComponent<IWindow>().Leave();
        prevWindow = index;
        change = false;
    }

    private IEnumerator WaitForAnimation(bool left, int cur)
    {
        Animator prevAnimator = selects[prevWindow].GetComponent<Animator>();
        Animator curAnimator = selects[cur].GetComponent<Animator>();

        if (left)
        {
            prevAnimator.SetTrigger("left_out");
            curAnimator.SetTrigger("right_in");
        }
        else
        {
            prevAnimator.SetTrigger("right_out");
            curAnimator.SetTrigger("left_in");
        }

        yield return new WaitForSeconds(0.2f);
    }
}
