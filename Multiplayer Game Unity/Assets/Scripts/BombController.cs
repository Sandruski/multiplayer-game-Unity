using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class BombController : NetworkBehaviour
{
    #region Public
    public float secondsToExplode;
    [HideInInspector]
    public Collider2D myCollider;
    [HideInInspector]
    public Player owner;
    #endregion

    #region Private
    private float timer = 0.0f;

    private CustomNetworkManager networkManager;
    private DynamicGridManager dynamicGridManager;
    #endregion

    void Start()
    {
        NetworkManager mng = NetworkManager.singleton;
        networkManager = mng.GetComponent<CustomNetworkManager>();
        dynamicGridManager = GameObject.Find("DynamicGridManager").GetComponent<DynamicGridManager>();
        myCollider = GetComponent<Collider2D>();

        List<GameObject> playersOnTop = dynamicGridManager.GetPlayersOnTile(transform.position);
        foreach (GameObject playerOnTop in playersOnTop)
        {
            Physics2D.IgnoreCollision(myCollider, playerOnTop.GetComponent<Collider2D>(), true);
        }
    }

    void Update()
    {
        if (!isServer)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= secondsToExplode)
        {
            dynamicGridManager.SpawnExplosions(owner, transform.position);

            Die();
            Kill();
        }
    }

    void Die()
    {
        owner.RemoveConcurrentBombs(1);
    }

    public void Kill()
    {
        networkManager.RemoveObject(gameObject);
    }
}
