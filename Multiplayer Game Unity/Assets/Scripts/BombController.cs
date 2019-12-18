using UnityEngine;
using System.Collections.Generic;

public class BombController : MonoBehaviour
{
    public float secondsToExplode;
    [HideInInspector]
    public Collider2D collider2D;
    public Player.PlayerColor playerColor;

    private Player owner;
    private GridManager gridManager;
    private float timer = 0.0f;

    void Awake()
    {
        gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
        collider2D = GetComponent<Collider2D>();
        owner = gridManager.GetPlayer(playerColor);

        List<GameObject> playersOnTop = gridManager.GetPlayersOnTile(transform.position);
        foreach (GameObject playerOnTop in playersOnTop)
        {
            Physics2D.IgnoreCollision(collider2D, playerOnTop.GetComponent<Collider2D>(), true);
        }
        
        owner.concurrentBombs += 1;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= secondsToExplode)
        {
            Die();
            Destroy(gameObject);
        }
    }

    void Die()
    {
        gridManager.SpawnExplosions(owner, transform.position);
        
        owner.concurrentBombs -= 1;
    }
}
