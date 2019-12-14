using UnityEngine;
using System.Collections.Generic;

public class BombController : MonoBehaviour
{
    public float secondsToExplode;
    [HideInInspector]
    public bool isAlive;
    [HideInInspector]
    public Collider2D collider2D;

    Player owner;
    GridManager gridManager;
    float timer;

    void Awake()
    {
        isAlive = false;
        collider2D = GetComponent<Collider2D>();
        gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();      
    }

    void Update()
    {
        if (isAlive)
        {
            timer += Time.deltaTime;

            if (timer >= secondsToExplode)
            {
                Despawn();
            }
        }
    }

    public void SetOwner(Player owner)
    {
        this.owner = owner;
    }

    public void Spawn()
    {
        transform.position = gridManager.GetCellCenterPosition(owner.transform.position);
        List<GameObject> playersOnTop = gridManager.GetPlayersOnTile(transform.position);
        foreach (GameObject playerOnTop in playersOnTop)
        {
            Physics2D.IgnoreCollision(collider2D, playerOnTop.GetComponent<Collider2D>(), true);
        }

        isAlive = true;
        timer = 0.0f;
        owner.concurrentBombs += 1;
        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        gridManager.SpawnExplosions(owner, transform.position);

        isAlive = false;
        owner.concurrentBombs -= 1;
        gameObject.SetActive(false);
    }
}
