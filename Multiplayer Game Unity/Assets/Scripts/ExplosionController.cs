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

        isAlive = true;
        gameObject.SetActive(true);

        switch (orientation)
        {
            case Orientation.top:
                animator.SetBool("top", true);
                animator.SetBool("bottom", false);
                animator.SetBool("left", false);
                animator.SetBool("right", false);
                animator.SetBool("vertical", false);
                animator.SetBool("horizontal", false);
                break;
            case Orientation.bottom:
                animator.SetBool("top", false);
                animator.SetBool("bottom", true);
                animator.SetBool("left", false);
                animator.SetBool("right", false);
                animator.SetBool("vertical", false);
                animator.SetBool("horizontal", false);
                break;
            case Orientation.left:
                animator.SetBool("top", false);
                animator.SetBool("bottom", false);
                animator.SetBool("left", true);
                animator.SetBool("right", false);
                animator.SetBool("vertical", false);
                animator.SetBool("horizontal", false);
                break;
            case Orientation.right:
                animator.SetBool("top", false);
                animator.SetBool("bottom", false);
                animator.SetBool("left", false);
                animator.SetBool("right", true);
                animator.SetBool("vertical", false);
                animator.SetBool("horizontal", false);
                break;
            case Orientation.vertical:
                animator.SetBool("top", false);
                animator.SetBool("bottom", false);
                animator.SetBool("left", false);
                animator.SetBool("right", false);
                animator.SetBool("vertical", true);
                animator.SetBool("horizontal", false);
                break;
            case Orientation.horizontal:
                animator.SetBool("top", false);
                animator.SetBool("bottom", false);
                animator.SetBool("left", false);
                animator.SetBool("right", false);
                animator.SetBool("vertical", false);
                animator.SetBool("horizontal", true);
                break;
            default:
                break;
        }
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
