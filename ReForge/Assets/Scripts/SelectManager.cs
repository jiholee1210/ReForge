using UnityEngine;
using UnityEngine.UI;

public class SelectManager : MonoBehaviour
{
    [SerializeField] private Button[] button;
    [SerializeField] private GameObject[] selects;
    [SerializeField] private GameObject[] managers;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int prevWindow;
    void Start()
    {
        prevWindow = 0;
        button[prevWindow].GetComponent<Image>().color = Color.green;
        for (int i = 0; i < button.Length; i++)
        {
            int index = i;
            button[index].onClick.AddListener(() => OpenWindow(index));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OpenWindow(int index)
    {
        button[index].GetComponent<Image>().color = Color.green;
        button[prevWindow].GetComponent<Image>().color = Color.white;
        
        selects[index].SetActive(true);
        selects[prevWindow].SetActive(false);

        managers[index].GetComponent<IWindow>().Reset();
        managers[prevWindow].GetComponent<IWindow>().Leave();
        prevWindow = index;
    }
}
