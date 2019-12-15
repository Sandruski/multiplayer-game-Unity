using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    [HideInInspector]
    public bool isAlive;

    public enum Orientation { center, top, bottom, left, right, vertical, horizontal };

    Player owner;
    GridManager gridManager;
    Animator animator;

    void Awake()
    {
        isAlive = false;
        gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isAlive)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                Despawn();
            }
        }
    }

    public void SetOwner(Player owner)
    {
        this.owner = owner;
    }

    public void Spawn(Vector3 position, Orientation orientation)
    {
        transform.position = position;
        switch (orientation)
        {
            case Orientation.top:
                animator.SetBool("top", true);
                break;
            case Orientation.bottom:
                animator.SetBool("bottom", true);
                break;
            case Orientation.left:
                animator.SetBool("left", true);
                break;
            case Orientation.right:
                animator.SetBool("right", true);
                break;
            case Orientation.vertical:
                animator.SetBool("vertical", true);
                break;
            case Orientation.horizontal:
                animator.SetBool("horizontal", true);
                break;
            default:
                break;
        }

        isAlive = true;
        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        List<GameObject> playersOnTop = gridManager.GetPlayersOnTile(transform.position);
        foreach (GameObject playerOnTop in playersOnTop)
        {
            if (playerOnTop != owner)
            {
                playerOnTop.GetComponent<Player>().Kill();
            }
        }

        isAlive = false;
        gameObject.SetActive(false);
    }
}
