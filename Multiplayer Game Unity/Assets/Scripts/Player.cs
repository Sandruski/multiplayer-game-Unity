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
    public uint sizeBombs = 1;
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
    bool up, down, left, right;
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

    void Update()
    {
        if (Input.GetKeyDown("up")) { up = true; }
        else if (Input.GetKeyUp("up")) { up = false; }

        if (Input.GetKeyDown("down")){  down = true; }
        else if (Input.GetKeyUp("down")) { down = false; }

        if (Input.GetKeyDown("left")) { left = true; }
        else if (Input.GetKeyUp("left")) { left = false; }

        if (Input.GetKeyDown("right")) { right = true; }
        else if (Input.GetKeyUp("right")) { right = false; }

        animator.SetBool("Down", down);
        animator.SetBool("Left", left);
        animator.SetBool("Right", right);
        animator.SetBool("Up", up);

        if (Input.GetKeyDown("space"))
        {
            if (concurrentBombs < maxConcurrentBombs)
            {
                SpawnBomb();
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 velocity = Vector2.zero;
        if (up)
        {
            velocity.y += speed;
        }
        if (down)
        {
            velocity.y -= speed;
        }
        if (left)
        {
            velocity.x -= speed;
        }
        if (right)
        {
            velocity.x += speed;
        }
        rb.velocity = velocity;
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
