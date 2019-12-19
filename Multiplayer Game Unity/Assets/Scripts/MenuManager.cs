using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MenuManager : MonoBehaviour
{
    public GameObject mainSection;
    public GameObject onlineSection;
    public GameObject lanSection;
    public Dropdown dropdownCharacter;

    public Dropdown dropdownMatches;
    public InputField roomName;

    public InputField ip;
    public InputField port;

    private CustomNetworkManager netManager;

    void Start()
    {
        NetworkManager mng = NetworkManager.singleton;
        netManager = mng.GetComponent<CustomNetworkManager>();

        netManager.RequestMatches();

        foreach (string player in netManager.playerNames)
        {
            dropdownCharacter.options.Add(new Dropdown.OptionData() { text = player });
        }
        dropdownCharacter.value = 0;
        dropdownCharacter.RefreshShownValue();

        ip.text = "localhost";
        port.text = "7777";
    }

    public void OnQuit()
    {
        Application.Quit();
    }

    public void OnCharacterDropdown()
    {
        netManager.playerPrefabIndex = (short)dropdownCharacter.value;
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

    // Lan
    public void OnHostButton()
    {
        netManager.networkPort = int.Parse(port.text);
        netManager.StartHost();
    }

    public void OnClientButton()
    {
        netManager.networkAddress = ip.text;
        netManager.networkPort = int.Parse(port.text);
        netManager.StartClient();
    }

    // Online
    public void OnRefreshButton()
    {
        netManager.RequestMatches();
        dropdownMatches.ClearOptions();
    }

    public void OnUpdateDropbox()
    {
        foreach (MatchInfoSnapshot match in netManager.matchList)
        {
            dropdownMatches.options.Add(new Dropdown.OptionData() { text = match.name });
        }
    }

    public void OnJoinButton()
    {
        if (dropdownMatches.value >= 0 && dropdownMatches.value < dropdownMatches.options.Count)
        {
            string selected = dropdownMatches.options[dropdownMatches.value].text;
            netManager.OnJoinMatch(selected);
        }
    }

    public void OnCreateButton()
    {
        netManager.OnCreateMatch(roomName.text);
    }
}
