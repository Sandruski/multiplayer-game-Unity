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
    #endregion

    void Start()
    {
        NetworkManager mng = NetworkManager.singleton;
        networkManager = mng.GetComponent<CustomNetworkManager>();
        myCollider = GetComponent<Collider2D>();

        List<GameObject> playersOnTop = DynamicGridManager.GetSingleton().GetPlayersOnTile(transform.position);
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
            DynamicGridManager.GetSingleton().SpawnExplosions(owner, transform.position);

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
