using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject mainSection;
    public GameObject onlineSection;
    public GameObject lanSection;
    public void OnBackButton()
    {
        mainSection.SetActive(true);
        lanSection.SetActive(false);
        onlineSection.SetActive(false);
    }

    public void OnLanButton()
    {
        mainSection.SetActive(false);
        lanSection.SetActive(true);
    }

    public void OnOnlineButton()
    {
        mainSection.SetActive(false);
        onlineSection.SetActive(true);
    }
}
