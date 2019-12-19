using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TileDef : NetworkBehaviour
{
    public enum TileType { Block, Bricks, Grass };
    public TileType tileType = TileType.Block;
}
