using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ExplosionController : NetworkBehaviour
{
    #region Public
    public enum Orientation { center, top, bottom, left, right, vertical, horizontal };
    [SyncVar]
    public Orientation orientation = Orientation.center;
    #endregion

    #region Private
    private CustomNetworkManager networkManager;
    private Animator animator;
    #endregion

    void Start()
    {
        NetworkManager mng = NetworkManager.singleton;
        networkManager = mng.GetComponent<CustomNetworkManager>();
        animator = GetComponent<Animator>();

        switch (orientation)
        {
            case Orientation.top:
                {
                    animator.SetBool("top", true);
                    animator.SetBool("bottom", false);
                    animator.SetBool("left", false);
                    animator.SetBool("right", false);
                    animator.SetBool("vertical", false);
                    animator.SetBool("horizontal", false);
                    break;
                }
            case Orientation.bottom:
                {
                    animator.SetBool("top", false);
                    animator.SetBool("bottom", true);
                    animator.SetBool("left", false);
                    animator.SetBool("right", false);
                    animator.SetBool("vertical", false);
                    animator.SetBool("horizontal", false);
                    break;
                }
            case Orientation.left:
                {
                    animator.SetBool("top", false);
                    animator.SetBool("bottom", false);
                    animator.SetBool("left", true);
                    animator.SetBool("right", false);
                    animator.SetBool("vertical", false);
                    animator.SetBool("horizontal", false);
                    break;
                }
            case Orientation.right:
                {
                    animator.SetBool("top", false);
                    animator.SetBool("bottom", false);
                    animator.SetBool("left", false);
                    animator.SetBool("right", true);
                    animator.SetBool("vertical", false);
                    animator.SetBool("horizontal", false);
                    break;
                }
            case Orientation.vertical:
                {
                    animator.SetBool("top", false);
                    animator.SetBool("bottom", false);
                    animator.SetBool("left", false);
                    animator.SetBool("right", false);
                    animator.SetBool("vertical", true);
                    animator.SetBool("horizontal", false);
                    break;
                }
            case Orientation.horizontal:
                {
                    animator.SetBool("top", false);
                    animator.SetBool("bottom", false);
                    animator.SetBool("left", false);
                    animator.SetBool("right", false);
                    animator.SetBool("vertical", false);
                    animator.SetBool("horizontal", true);
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    void Update()
    {
        if (!isServer)
        {
            return;
        }

        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            Die();
            Kill();
        }
    }

    public void Die()
    {
        List<GameObject> playersOnTop = DynamicGridManager.GetSingleton().GetPlayersOnTile(transform.position);
        foreach (GameObject playerOnTop in playersOnTop)
        {
            playerOnTop.GetComponent<Player>().Kill();
        }
    }

    public void Kill()
    {
        networkManager.RemoveObject(gameObject);
    }
}
