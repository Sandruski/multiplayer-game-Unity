using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0414
public class Player : MonoBehaviour
{
    #region Inspector
    [SerializeField]
    uint poolSize = 4;
    [SerializeField]
    uint maxBombs = 1;
    [SerializeField]
    uint maxConcurrentBombs = 1;
    [SerializeField]
    uint sizeBombs = 1;
    [SerializeField]
    float speed = 1.0f;
    [SerializeField]
    float animSpeed = 0.25f;
    #endregion

    #region NoInspector
    List<BombController> bombsPool = new List<BombController>();
    [HideInInspector]
    public uint concurrentBombs = 0u;
    Rigidbody2D rb;
    Animator animator;
    #endregion

    void Start()
    {
        if (maxBombs > poolSize)
        {
            Debug.LogWarning("Num of MaxBombs: " + maxBombs + "is higher than the size of the pool:" + poolSize);
            poolSize = maxBombs;
        }

        Object bombPrefab = Resources.Load("Bomb");
        for (uint i = 0u; i < poolSize; ++i)
        {
            GameObject bomb = (GameObject)Instantiate(bombPrefab);
            bomb.SetActive(false);
            BombController bombController = bomb.GetComponent<BombController>();
            bombController.SetOwner(this);
            bombsPool.Add(bombController);
        }

        rb = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        animator.SetFloat("Speed", animSpeed);
    }

    void FixedUpdate()
    {
        Vector2 velocity = Vector2.zero;
        if (Input.GetKeyDown("up"))
        {
            velocity.y += speed;
            animator.SetBool("Down", false);
            animator.SetBool("Left", false);
            animator.SetBool("Right", false);
            animator.SetBool("Up", true);
        }
        else if (Input.GetKeyUp("up"))
        {
            animator.SetBool("Up", false);
        }

        if (Input.GetKeyDown("down"))
        {
            velocity.y -= speed;
            animator.SetBool("Up", false);
            animator.SetBool("Left", false);
            animator.SetBool("Right", false);
            animator.SetBool("Down", true);
        }
        else if (Input.GetKeyUp("down"))
        {
            animator.SetBool("Down", false);
        }

        if (Input.GetKeyDown("left"))
        {
            velocity.x -= speed;
            animator.SetBool("Up", false);
            animator.SetBool("Down", false);
            animator.SetBool("Right", false);
            animator.SetBool("Left", true);
        }
        else if (Input.GetKeyUp("left"))
        {
            animator.SetBool("Left", false);
        }

        if (Input.GetKeyDown("right"))
        {
            velocity.x += speed;
            animator.SetBool("Up", false);
            animator.SetBool("Down", false);
            animator.SetBool("Left", false);
            animator.SetBool("Right", true);
        }
        else if (Input.GetKeyUp("left"))
        {
            animator.SetBool("Right", false);
        }
        rb.velocity = velocity;

        if (Input.GetKeyDown("space"))
        {
            if (concurrentBombs < maxConcurrentBombs)
            {
                SpawnBomb();
            }
        }
    }

    bool SpawnBomb()
    {
        for (int i = 0; i < maxBombs; ++i)
        {
            BombController bomb = bombsPool[i];
            if (!bomb.isAlive)
            {
                bomb.Spawn();
                return true;
            }
        }
        return false;
    }

    void IncreaseMaxBombs()
    {
        if (maxBombs > poolSize)
        {
            Debug.LogWarning("Num of MaxBombs: " + maxBombs + "is higher than the size of the pool:" + poolSize);
            return;
        }
        maxBombs += 1;
    }
}
