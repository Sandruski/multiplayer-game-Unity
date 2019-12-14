using UnityEngine;
using UnityEngine.Tilemaps;

public class BombController : MonoBehaviour
{
    [HideInInspector]
    public bool isAlive;

    Player owner;

    BombController()
    {
        isAlive = false;
    }

    void Update()
    {
        
    }

    public void SetOwner(Player owner)
    {
        this.owner = owner;
    }

    public void GetSpawned()
    {
        isAlive = true;
        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        isAlive = false;
        gameObject.SetActive(false);
    }
}
