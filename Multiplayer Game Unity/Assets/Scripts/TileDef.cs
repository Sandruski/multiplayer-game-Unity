using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TileDef : NetworkBehaviour
{
    #region Public
    public enum TileType { Bricks, Grass };
    public TileType tileType = TileType.Bricks;
    #endregion

    void Start()
    {
        DynamicGridManager.GetSingleton().UpdateTile(gameObject);
    }
}
