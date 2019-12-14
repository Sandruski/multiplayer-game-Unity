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
    uint sizeBombs = 1;
    [SerializeField]
    float speed = 1.0f;
    #endregion

    #region Private
    List<BombController> bombsPool = new List<BombController>();
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
    }

    void Update()
    {
        
    }

    bool SpawnBomb()
    {
        for (int i = 0; i < maxBombs; ++i)
        {
            BombController bomb = bombsPool[i];
            if (!bomb.isAlive)
            {
                bomb.GetSpawned();
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
