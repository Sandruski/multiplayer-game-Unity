using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    [HideInInspector]
    public bool isAlive;

    Player owner;

    void Awake()
    {
        isAlive = false;
    }

    public void SetOwner(Player owner)
    {
        this.owner = owner;
    }

    public void Spawn(Vector3 position)
    {
        transform.position = position;
        isAlive = true;
        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        isAlive = false;
        gameObject.SetActive(false);
    }
}
