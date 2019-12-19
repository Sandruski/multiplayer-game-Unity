using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ExplodingBrickController : NetworkBehaviour
{
    #region Private
    Animator animator;
    CustomNetworkManager networkManager;
    #endregion

    void Awake()
    {
        animator = GetComponent<Animator>();

        NetworkManager mng = NetworkManager.singleton;
        networkManager = mng.GetComponent<CustomNetworkManager>();
    }

    void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            && isServer)
        {
            Kill();
        }
    }

    public void Kill()
    {
        networkManager.RemoveObject(gameObject);
    }
}
