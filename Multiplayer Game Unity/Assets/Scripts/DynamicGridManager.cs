using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Networking;

public class DynamicGridManager : NetworkBehaviour
{
    #region Public
    public float bricksProbability;
    #endregion

    #region Private     
    private TileDef[,] tileDefs;

    private List<GameObject> players;
    
    private StaticGridManager staticGridManager;
    private CustomNetworkManager networkManager;
    #endregion

    void Start()
    {
        staticGridManager = GameObject.Find("StaticGridManager").GetComponent<StaticGridManager>();
        NetworkManager mng = NetworkManager.singleton;
        networkManager = mng.GetComponent<CustomNetworkManager>();

        players = new List<GameObject>();
        tileDefs = new TileDef[staticGridManager.width, staticGridManager.height];
    }

    public void GenerateMap()
    {
        Vector3Int topLeftSpawnTileBottomTile = staticGridManager.topLeftSpawnTile + Vector3Int.down;
        Vector3Int topLeftSpawnTileRightTile = staticGridManager.topLeftSpawnTile + Vector3Int.right;
        Vector3Int topRightSpawnTileBottomTile = staticGridManager.topRightSpawnTile + Vector3Int.down;
        Vector3Int topRightSpawnTileLeftTile = staticGridManager.topRightSpawnTile + Vector3Int.left;
        Vector3Int bottomLeftSpawnTileTopTile = staticGridManager.bottomLeftSpawnTile + Vector3Int.up;
        Vector3Int bottomLeftSpawnTileRightTile = staticGridManager.bottomLeftSpawnTile + Vector3Int.right;
        Vector3Int bottomRightSpawnTileTopTile = staticGridManager.bottomRightSpawnTile + Vector3Int.up;
        Vector3Int bottomRightSpawnTileLeftTile = staticGridManager.bottomRightSpawnTile + Vector3Int.left;

        for (int x = 0; x < staticGridManager.width; ++x)
        {
            for (int y = staticGridManager.height - 1; y >= 0; --y)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (!staticGridManager.collidableGroundTilemap.HasTile(cellPosition))
                {
                    bool isSpawnTile =
                        cellPosition == staticGridManager.topLeftSpawnTile
                        || cellPosition == staticGridManager.topRightSpawnTile
                        || cellPosition == staticGridManager.bottomLeftSpawnTile
                        || cellPosition == staticGridManager.bottomRightSpawnTile
                        || cellPosition == topLeftSpawnTileBottomTile
                        || cellPosition == topLeftSpawnTileRightTile
                        || cellPosition == topRightSpawnTileBottomTile
                        || cellPosition == topRightSpawnTileLeftTile
                        || cellPosition == bottomLeftSpawnTileTopTile
                        || cellPosition == bottomLeftSpawnTileRightTile
                        || cellPosition == bottomRightSpawnTileTopTile
                        || cellPosition == bottomRightSpawnTileLeftTile;

                    Vector3 cellWorldPosition = staticGridManager.nonCollidableGroundTilemap.GetCellCenterWorld(cellPosition);

                    if (!isSpawnTile && Random.value <= bricksProbability)
                    {
                        networkManager.AddObject((int)CustomNetworkManager.SpawnPrefabs.BricksTile, cellWorldPosition);
                    }
                    else
                    {
                        if (staticGridManager.collidableGroundTilemap.HasTile(new Vector3Int(x, y + 1, 0)))
                        {
                            networkManager.AddObject((int)CustomNetworkManager.SpawnPrefabs.GrassWithShadowTile, cellWorldPosition);
                        }
                        else
                        {
                            networkManager.AddObject((int)CustomNetworkManager.SpawnPrefabs.GrassTile, cellWorldPosition);
                        }
                    }
                }
            }
        }
    }

    public void AddPlayer(GameObject player)
    {
        players.Add(player);
    }

    public void RemovePlayer(GameObject player)
    {
        players.Remove(player);
    }

    public List<GameObject> GetPlayersOnTile(Vector3 position)
    {
        List<GameObject> playersOnTile = new List<GameObject>();
        Vector3Int cellPosition = staticGridManager.nonCollidableGroundTilemap.WorldToCell(position);

        foreach (GameObject player in players)
        {
            Vector3Int playerCellPosition = staticGridManager.nonCollidableGroundTilemap.WorldToCell(player.transform.position);
            if (cellPosition == playerCellPosition)
            {
                playersOnTile.Add(player);
            }
        }

        return playersOnTile;
    }

    public Player GetPlayer(Player.PlayerColor playerColor)
    {
        foreach (GameObject gameObject in players)
        {
            Player player = gameObject.GetComponent<Player>();
            if (player.color == playerColor)
            {
                return player;
            }
        }

        return null;
    }



    public void SpawnExplosions(Player player, Vector3 position)
    {
        /*
        Vector3Int cellPosition = nonCollidableGroundTilemap.WorldToCell(position);
        SpawnExplosion(nonCollidableGroundTilemap.GetCellCenterWorld(cellPosition), ExplosionController.Orientation.center);

        Vector3Int direction = Vector3Int.left;
        for (int i = 0; i < 4; ++i)
        {
            switch (i)
            {
                case 1:
                    direction = Vector3Int.right;
                    break;
                case 2:
                    direction = Vector3Int.up;
                    break;
                case 3:
                    direction = Vector3Int.down;
                    break;
                default:
                    break;
            }

            for (int j = 1; j < player.sizeBombs + 1; ++j)
            {
                Vector3Int nextCellPosition = cellPosition + direction * j;
                TileType nextTileType = tileTypes[nextCellPosition.x, nextCellPosition.y];
                if (nextTileType == TileType.Block)
                {
                    break;
                }
                else if (nextTileType == TileType.Bricks)
                {
                    collidableGroundTilemap.SetTile(nextCellPosition, null);

                    Vector3Int topNextCellPosition = nextCellPosition + Vector3Int.up;
                    if (tileTypes[topNextCellPosition.x, topNextCellPosition.y] != TileType.Grass)
                    {
                        nonCollidableGroundTilemap.SetTile(nextCellPosition, grassWithShadow);
                    }
                    else
                    {
                        nonCollidableGroundTilemap.SetTile(nextCellPosition, grass);
                    }

                    Vector3Int bottomNextCellPosition = nextCellPosition + Vector3Int.down;
                    if (tileTypes[bottomNextCellPosition.x, bottomNextCellPosition.y] == TileType.Grass)
                    {
                        nonCollidableGroundTilemap.SetTile(bottomNextCellPosition, grass);
                    }

                    tileTypes[nextCellPosition.x, nextCellPosition.y] = TileType.Grass;

                    SpawnExplodingBrick(nonCollidableGroundTilemap.GetCellCenterWorld(nextCellPosition));

                    break;
                }
                else if (nextTileType == TileType.Grass)
                {
                    ExplosionController.Orientation orientation = ExplosionController.Orientation.center;
                    Vector3Int nextNextCellPosition = nextCellPosition + direction;
                    TileType nextNextTileType = tileTypes[nextNextCellPosition.x, nextNextCellPosition.y];

                    switch (i)
                    {
                        case 0:
                            if (nextNextTileType != TileType.Grass || j == player.sizeBombs)
                            {
                                orientation = ExplosionController.Orientation.left;
                            }
                            else
                            {
                                orientation = ExplosionController.Orientation.horizontal;
                            }
                            break;
                        case 1:
                            if (nextNextTileType != TileType.Grass || j == player.sizeBombs)
                            {
                                orientation = ExplosionController.Orientation.right;
                            }
                            else
                            {
                                orientation = ExplosionController.Orientation.horizontal;
                            }
                            break;
                        case 2:
                            if (nextNextTileType != TileType.Grass || j == player.sizeBombs)
                            {
                                orientation = ExplosionController.Orientation.top;
                            }
                            else
                            {
                                orientation = ExplosionController.Orientation.vertical;
                            }
                            break;
                        case 3:
                            if (nextNextTileType != TileType.Grass || j == player.sizeBombs)
                            {
                                orientation = ExplosionController.Orientation.bottom;
                            }
                            else
                            {
                                orientation = ExplosionController.Orientation.vertical;
                            }
                            break;
                        default:
                            break;
                    }

                    SpawnExplosion(nonCollidableGroundTilemap.GetCellCenterWorld(nextCellPosition), orientation);
                }
            }
        }*/
    }

    [ClientRpc]
    public void RpcSyncTile(GameObject gameObject)
    {
        Vector3Int cellPosition = staticGridManager.nonCollidableGroundTilemap.WorldToCell(gameObject.transform.position);
        tileDefs[cellPosition.x, cellPosition.y] = gameObject.GetComponent<TileDef>();
    }
}

