using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_TextMeshPromm;
    [SerializeField] TextMeshProUGUI m_TextMeshProe;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (m_TextMeshPromm != null)
        {
            float x = MaskManager.GetBestTime();
            m_TextMeshPromm.text = "Best time: " + x;
        }
        if (m_TextMeshProe != null)
        {
            float x = MaskManager.GetLastTime();
            m_TextMeshProe.text = "Last time: " + x;

        }

    }
    public void StartGame()
    {
        SceneManager.LoadScene("FirstLevel");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
