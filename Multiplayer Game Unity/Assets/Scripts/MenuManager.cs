using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OnHostButton()
    {
        SceneManager.LoadScene("MainScene");
    }
}
