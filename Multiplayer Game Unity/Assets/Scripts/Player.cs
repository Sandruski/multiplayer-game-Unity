using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0414
public class Player : NetworkBehaviour
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
    #endregion

    void Awake()
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

        GameObject.Find("GridManager").GetComponent<GridManager>().AddPlayer(gameObject);
    }

    void OnDestroy()
    {
        GameObject gridManagerGameObject = GameObject.Find("GridManager");
        if (gridManagerGameObject != null)
        {
            GridManager gridManager = gridManagerGameObject.GetComponent<GridManager>();
            gridManager.RemovePlayer(gameObject);
        }
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown("up")) { up = true; }
            else if (Input.GetKeyUp("up")) { up = false; }

            if (Input.GetKeyDown("down")) { down = true; }
            else if (Input.GetKeyUp("down")) { down = false; }

            if (Input.GetKeyDown("left")) { left = true; }
            else if (Input.GetKeyUp("left")) { left = false; }

            if (Input.GetKeyDown("right")) { right = true; }
            else if (Input.GetKeyUp("right")) { right = false; }

            SetAnimation();

            if (Input.GetKeyDown("space"))
            {
                if (concurrentBombs < maxConcurrentBombs)
                {
                    SpawnBomb();
                }
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

    void IncreaseConcurrentBombs(uint amount)
    {
        concurrentBombs += amount;
    }

    void IncreaseSpeed(uint amount)
    {
        speed += amount;
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    // Animation sync
    [SyncVar(hook = "OnUp")]
    bool up;
    [SyncVar(hook = "OnDown")]
    bool down;
    [SyncVar(hook = "OnLeft")]
    bool left;
    [SyncVar(hook = "OnRight")]
    bool right;

    [Command]
    void CmdSetAnimation(bool up, bool down, bool left, bool right)
    {
        this.up = up;
        this.down = down;
        this.left = left;
        this.right = right;
    }

    void OnUp(bool up)
    {
        this.up = up;
        animator.SetBool("Up", this.up);
    }

    void OnDown(bool down)
    {
        this.down = down;
        animator.SetBool("Down", this.down);
    }

    void OnLeft(bool left)
    {
        this.left = left;
        animator.SetBool("Left", this.left);
    }

    void OnRight(bool right)
    {
        this.right = right;
        animator.SetBool("Right", this.right);
    }

    void SetAnimation()
    {
        OnUp(up);
        OnDown(down);
        OnLeft(left);
        OnRight(right);
        CmdSetAnimation(up, down, left, right);
    }
}
