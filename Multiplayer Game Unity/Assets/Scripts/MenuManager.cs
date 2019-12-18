using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject mainSection;
    public GameObject onlineSection;
    public GameObject lanSection;

    public Dropdown dropdown;
    public InputField roomName;
    public CustomNetworkManager netManager;

    private void Start()
    {
        netManager.RequestMatches();
    }

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

    public void OnRefreshButton()
    {
        netManager.RequestMatches();
        dropdown.ClearOptions();
    }

    public void OnUpdateDropbox()
    {
        foreach (MatchInfoSnapshot match in netManager.matchList)
        {
            dropdown.options.Add(new Dropdown.OptionData() { text = match.name });
        }
    }

    public void OnJoinButton()
    {
        if (dropdown.value >= 0)
        {
            string selected = dropdown.options[dropdown.value].text;
            netManager.OnJoinMatch(selected);
        }
    }

    public void OnCreateButton()
    {
        netManager.OnCreateMatch(roomName.text);
    }
}
