using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ExplodingBrickController : NetworkBehaviour
{
    #region Private
    private CustomNetworkManager networkManager;
    private Animator animator;
    #endregion

    void Start()
    {
        NetworkManager mng = NetworkManager.singleton;
        networkManager = mng.GetComponent<CustomNetworkManager>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isServer)
        {
            return;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            DynamicGridManager.GetSingleton().RemoveExplodingBricksTile(gameObject);
        }
    }
}
