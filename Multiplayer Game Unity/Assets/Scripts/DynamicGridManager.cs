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
    private List<GameObject> playerGameObjects;
    
    private CustomNetworkManager networkManager;
    private StaticGridManager staticGridManager;
    #endregion

    void Start()
    {
        NetworkManager mng = NetworkManager.singleton;
        networkManager = mng.GetComponent<CustomNetworkManager>();
        staticGridManager = GameObject.Find("StaticGridManager").GetComponent<StaticGridManager>();

        tileDefs = new TileDef[staticGridManager.width, staticGridManager.height];
        playerGameObjects = new List<GameObject>();
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

        TileDef.TileType[,] tileTypes = new TileDef.TileType[staticGridManager.width, staticGridManager.height];

        for (int x = 0; x < staticGridManager.width; ++x)
        {
            for (int y = staticGridManager.height - 1; y >= 0; --y)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (!staticGridManager.collidableGroundTilemap.HasTile(cellPosition))
                {
                    Vector3 cellWorldPosition = staticGridManager.nonCollidableGroundTilemap.GetCellCenterWorld(cellPosition);

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
                    if (!isSpawnTile && Random.value <= bricksProbability)
                    {
                        networkManager.AddObject((int)CustomNetworkManager.SpawnPrefabs.BricksTile, cellWorldPosition);
                    }
                    else
                    {
                        Vector3Int topCellPosition = new Vector3Int(x, y + 1, 0);
                        TileDef.TileType topTileType = tileTypes[topCellPosition.x, topCellPosition.y];

                        if (topTileType == TileDef.TileType.Bricks)
                        {
                            networkManager.AddObject((int)CustomNetworkManager.SpawnPrefabs.GrassWithShadowTile, cellWorldPosition);
                        }
                        else
                        {
                            networkManager.AddObject((int)CustomNetworkManager.SpawnPrefabs.GrassTile, cellWorldPosition);
                        }

                        tileTypes[x, y] = TileDef.TileType.Grass;
                    }
                }
            }
        }
    }

    public void AddPlayer(GameObject playerGameObject)
    {
        playerGameObjects.Add(playerGameObject);
    }

    public void RemovePlayer(GameObject playerGameObject)
    {
        playerGameObjects.Remove(playerGameObject);
    }

    public void UpdateTile(GameObject tileGameObject)
    {
        Vector3Int cellPosition = staticGridManager.nonCollidableGroundTilemap.WorldToCell(tileGameObject.transform.position);
        tileDefs[cellPosition.x, cellPosition.y] = tileGameObject.GetComponent<TileDef>();
    }

    public List<GameObject> GetPlayersOnTile(Vector3 position)
    {
        List<GameObject> playersOnTile = new List<GameObject>();
        Vector3Int cellPosition = staticGridManager.nonCollidableGroundTilemap.WorldToCell(position);

        foreach (GameObject player in playerGameObjects)
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
        foreach (GameObject gameObject in playerGameObjects)
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
        Vector3Int cellPosition = staticGridManager.nonCollidableGroundTilemap.WorldToCell(position);
        Vector3 cellCenterWorldPosition = staticGridManager.nonCollidableGroundTilemap.GetCellCenterWorld(cellPosition);
        networkManager.AddExplosion(position, ExplosionController.Orientation.center);

        Vector3Int direction = Vector3Int.left;
        for (int i = 0; i < 4; ++i)
        {
            switch (i)
            {
                case 1:
                    {
                        direction = Vector3Int.right;
                        break;
                    }
                case 2:
                    {
                        direction = Vector3Int.up;
                        break;
                    }
                case 3:
                    {
                        direction = Vector3Int.down;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            for (int j = 1; j < player.sizeBombs + 1; ++j)
            {
                Vector3Int nextCellPosition = cellPosition + direction * j;
                Vector3 nextCellWorldPosition = staticGridManager.nonCollidableGroundTilemap.GetCellCenterWorld(nextCellPosition);
                TileDef nextTileDef = tileDefs[nextCellPosition.x, nextCellPosition.y];
                if (nextTileDef != null)
                {
                    if (nextTileDef.tileType == TileDef.TileType.Bricks)
                    {
                        networkManager.RemoveObject(nextTileDef.gameObject);
                        networkManager.AddGridTile((int)CustomNetworkManager.SpawnPrefabs.ExplodingBricksTile, nextCellWorldPosition);

                        Vector3Int topNextCellPosition = nextCellPosition + Vector3Int.up;
                        TileDef topNextTileDef = tileDefs[topNextCellPosition.x, topNextCellPosition.y];
                        if (topNextTileDef == null || topNextTileDef.tileType == TileDef.TileType.Bricks)
                        {
                            networkManager.AddGridTile((int)CustomNetworkManager.SpawnPrefabs.GrassWithShadowTile, nextCellWorldPosition);
                        }
                        else
                        {
                            networkManager.AddGridTile((int)CustomNetworkManager.SpawnPrefabs.GrassTile, nextCellWorldPosition);
                        }

                        break;
                    }
                    else if (nextTileDef.tileType == TileDef.TileType.Grass)
                    {
                        ExplosionController.Orientation orientation = ExplosionController.Orientation.center;
                        Vector3Int nextNextCellPosition = nextCellPosition + direction;
                        TileDef nextNextTileDef = tileDefs[nextNextCellPosition.x, nextNextCellPosition.y];

                        switch (i)
                        {
                            case 0:
                                {
                                    if (nextNextTileDef == null || nextNextTileDef.tileType == TileDef.TileType.Bricks || j == player.sizeBombs)
                                    {
                                        orientation = ExplosionController.Orientation.left;
                                    }
                                    else
                                    {
                                        orientation = ExplosionController.Orientation.horizontal;
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    if (nextNextTileDef == null || nextNextTileDef.tileType == TileDef.TileType.Bricks || j == player.sizeBombs)
                                    {
                                        orientation = ExplosionController.Orientation.right;
                                    }
                                    else
                                    {
                                        orientation = ExplosionController.Orientation.horizontal;
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    if (nextNextTileDef == null || nextNextTileDef.tileType == TileDef.TileType.Bricks || j == player.sizeBombs)
                                    {
                                        orientation = ExplosionController.Orientation.top;
                                    }
                                    else
                                    {
                                        orientation = ExplosionController.Orientation.vertical;
                                    }
                                    break;
                                }
                            case 3:
                                {
                                    if (nextNextTileDef == null || nextNextTileDef.tileType == TileDef.TileType.Bricks || j == player.sizeBombs)
                                    {
                                        orientation = ExplosionController.Orientation.bottom;
                                    }
                                    else
                                    {
                                        orientation = ExplosionController.Orientation.vertical;
                                    }
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }

                        networkManager.AddExplosion(nextCellWorldPosition, orientation);
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    public void RemoveExplodingBricksTile(GameObject explodingBricksTileGameObject)
    {
        Vector3Int cellPosition = staticGridManager.nonCollidableGroundTilemap.WorldToCell(explodingBricksTileGameObject.transform.position);
        Vector3Int bottomNextCellPosition = cellPosition + Vector3Int.down;
        Vector3 bottomNextCellWorldPosition = staticGridManager.nonCollidableGroundTilemap.GetCellCenterWorld(bottomNextCellPosition);
        TileDef bottomNextTileDef = tileDefs[bottomNextCellPosition.x, bottomNextCellPosition.y];
        if (bottomNextTileDef != null && bottomNextTileDef.tileType == TileDef.TileType.Grass)
        {
            networkManager.RemoveObject(bottomNextTileDef.gameObject);
            networkManager.AddGridTile((int)CustomNetworkManager.SpawnPrefabs.GrassTile, bottomNextCellWorldPosition);
        }

        networkManager.RemoveObject(explodingBricksTileGameObject);
    }

    /*
    [ClientRpc]
    public void RpcSyncTile(GameObject gameObject)
    {
        Vector3Int cellPosition = staticGridManager.nonCollidableGroundTilemap.WorldToCell(gameObject.transform.position);
        tileDefs[cellPosition.x, cellPosition.y] = gameObject.GetComponent<TileDef>();
    }
    */
}

