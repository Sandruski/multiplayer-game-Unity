using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WinnerText : NetworkBehaviour
{
    #region Public
    [SyncVar(hook = "OnWinnerSet")]
    public string winner;
    public float seconds;
    #endregion

    #region Private
    private Text text;
    private CustomNetworkManager networkManager;

    private float timer = 0.0f;
    #endregion

    void Start()
    {
        text = GetComponent<Text>();
        NetworkManager mng = NetworkManager.singleton;
        networkManager = mng.GetComponent<CustomNetworkManager>();
    }

    void Update()
    {
        if (!isServer)
        {
            return;
        }

        if (winner != "")
        {
            timer += Time.deltaTime;
            if (timer >= seconds)
            {
                networkManager.StopHost();
            }
        }
    }

    void OnWinnerSet(string winner)
    {
        this.winner = winner;

        if (text != null)
        {
            text.text = winner;
        }
    }
}
