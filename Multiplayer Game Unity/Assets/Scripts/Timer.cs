using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Timer : NetworkBehaviour
{
    [SyncVar(hook = "OnTimerUpdated")]
    private float timer = 120.0f;

    private Text text;

    void Start()
    {
        text = GetComponent<Text>();   
    }

    void Update()
    {
        if (!isServer)
        {
            return;
        }

        timer -= Time.deltaTime;
    }

    void OnTimerUpdated(float timer)
    {
        this.timer = timer;

        int time = (int)this.timer;
        text.text = time.ToString();
    }
}
