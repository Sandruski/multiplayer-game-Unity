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

    #region Private
    private DynamicGridManager dynamicGridManager;
    #endregion

    void Start()
    {
        dynamicGridManager = GameObject.Find("DynamicGridManager").GetComponent<DynamicGridManager>();
        dynamicGridManager.UpdateTile(gameObject);
    }
}
