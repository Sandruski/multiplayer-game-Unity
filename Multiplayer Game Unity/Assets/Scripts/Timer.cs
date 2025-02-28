﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Timer : NetworkBehaviour
{
    #region Public
    [SyncVar]
    public bool isStopped = false;
    #endregion

    #region Private
    [SyncVar(hook = "OnTimerUpdated")]
    private float timer = 120.0f;

    private Text text;
    private CustomNetworkManager networkManager;
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

        if (!isStopped)
        {
            timer -= Time.deltaTime;
            if (timer <= 0.0f)
            {
                timer = 0.0f;
                networkManager.StopHost();
            }
        }
    }

    void OnTimerUpdated(float timer)
    {
        this.timer = timer;

        if (text != null)
        {
            int time = (int)this.timer;
            text.text = time.ToString();
        }
    }
}
