using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0414
public class Player : NetworkBehaviour
{
    public enum PlayerColor { white, black, red, blue };
    [SyncVar]
    public PlayerColor color = PlayerColor.white;

    #region Inspector
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
    Rigidbody2D rb;
    Animator animator;
    CustomNetworkManager networkManager;
    StaticGridManager staticGridManager;
    DynamicGridManager dynamicGridManager;
    #endregion

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
        animator.SetFloat("Speed", animSpeed);

        staticGridManager = GameObject.Find("StaticGridManager").GetComponent<StaticGridManager>();
        dynamicGridManager = GameObject.Find("DynamicGridManager").GetComponent<DynamicGridManager>();

        NetworkManager mng = NetworkManager.singleton;
        networkManager = mng.GetComponent<CustomNetworkManager>();

        dynamicGridManager.AddPlayer(gameObject);

        if (isServer && isLocalPlayer)
        {
            dynamicGridManager.GenerateMap();
        }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
    }

    void OnDestroy()
    {
        dynamicGridManager.RemovePlayer(gameObject);
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
                    AddBomb();
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
        networkManager.RemoveObject(gameObject);
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

    // Bombs sync
    [Command]
    void CmdAddBomb()
    {
        Vector3 bombPosition = staticGridManager.GetCellCenterWorldPosition(transform.position);
        networkManager.AddBomb(bombPosition, this);
    }

    void AddBomb()
    {
        CmdAddConcurrentBombs(1);
        CmdAddBomb();
    }

    // Concurrent bombs sync
    [SyncVar(hook = "OnConcurrentBombsChanged")]
    uint concurrentBombs;

    [Command]
    void CmdAddConcurrentBombs(uint concurrentBombs)
    {
        this.concurrentBombs += concurrentBombs;
    }

    [Command]
    void CmdRemoveConcurrentBombs(uint concurrentBombs)
    {
        this.concurrentBombs -= concurrentBombs;
    }

    void OnConcurrentBombsChanged(uint concurrentBombs)
    {
        this.concurrentBombs = concurrentBombs;
    }

    public void AddConcurrentBombs(uint concurrentBombs)
    {
        CmdAddConcurrentBombs(concurrentBombs);
    }

    public void RemoveConcurrentBombs(uint concurrentBombs)
    {
        CmdRemoveConcurrentBombs(concurrentBombs);
    }
}
