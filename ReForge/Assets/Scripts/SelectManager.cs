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
        selects[prevWindow].SetActive(false);
        selects[index].SetActive(true);
        managers[index].GetComponent<IWindow>().Reset();
        prevWindow = index;
    }
}
