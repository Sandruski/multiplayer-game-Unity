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

    private static DynamicGridManager singleton;
    #endregion

    public static DynamicGridManager GetSingleton()
    {
        return singleton;
    }

    void Start()
    {
        if (singleton != null)
        {
            Destroy(gameObject);
            return;
        }

        singleton = this;

        NetworkManager mng = NetworkManager.singleton;
        networkManager = mng.GetComponent<CustomNetworkManager>();

        tileDefs = new TileDef[StaticGridManager.GetSingleton().width, StaticGridManager.GetSingleton().height];
        playerGameObjects = new List<GameObject>();
    }

    public void GenerateMap()
    {
        Vector3Int topLeftSpawnTileBottomTile = StaticGridManager.GetSingleton().topLeftSpawnTile + Vector3Int.down;
        Vector3Int topLeftSpawnTileRightTile = StaticGridManager.GetSingleton().topLeftSpawnTile + Vector3Int.right;
        Vector3Int topRightSpawnTileBottomTile = StaticGridManager.GetSingleton().topRightSpawnTile + Vector3Int.down;
        Vector3Int topRightSpawnTileLeftTile = StaticGridManager.GetSingleton().topRightSpawnTile + Vector3Int.left;
        Vector3Int bottomLeftSpawnTileTopTile = StaticGridManager.GetSingleton().bottomLeftSpawnTile + Vector3Int.up;
        Vector3Int bottomLeftSpawnTileRightTile = StaticGridManager.GetSingleton().bottomLeftSpawnTile + Vector3Int.right;
        Vector3Int bottomRightSpawnTileTopTile = StaticGridManager.GetSingleton().bottomRightSpawnTile + Vector3Int.up;
        Vector3Int bottomRightSpawnTileLeftTile = StaticGridManager.GetSingleton().bottomRightSpawnTile + Vector3Int.left;

        TileDef.TileType[,] tileTypes = new TileDef.TileType[StaticGridManager.GetSingleton().width, StaticGridManager.GetSingleton().height];

        for (int x = 0; x < StaticGridManager.GetSingleton().width; ++x)
        {
            for (int y = StaticGridManager.GetSingleton().height - 1; y >= 0; --y)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (!StaticGridManager.GetSingleton().collidableGroundTilemap.HasTile(cellPosition))
                {
                    Vector3 cellWorldPosition = StaticGridManager.GetSingleton().nonCollidableGroundTilemap.GetCellCenterWorld(cellPosition);

                    bool isSpawnTile =
                        cellPosition == StaticGridManager.GetSingleton().topLeftSpawnTile
                        || cellPosition == StaticGridManager.GetSingleton().topRightSpawnTile
                        || cellPosition == StaticGridManager.GetSingleton().bottomLeftSpawnTile
                        || cellPosition == StaticGridManager.GetSingleton().bottomRightSpawnTile
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

        if (isServer)
        {
            if (playerGameObjects.Count == 0)
            {
                GameObject.Find("Winner").GetComponent<WinnerText>().winner = "YOU WIN";
                GameObject.Find("Countdown").GetComponent<Timer>().isStopped = true;
            }
            else if (playerGameObjects.Count == 1)
            {
                switch (playerGameObjects[0].GetComponent<Player>().color)
                {
                    case Player.PlayerColor.white:
                        {
                            GameObject.Find("Winner").GetComponent<WinnerText>().winner = "WHITE WINS";
                            break;
                        }
                    case Player.PlayerColor.black:
                        {
                            GameObject.Find("Winner").GetComponent<WinnerText>().winner = "BLACK WINS";
                            break;
                        }
                    case Player.PlayerColor.red:
                        {
                            GameObject.Find("Winner").GetComponent<WinnerText>().winner = "RED WINS";
                            break;
                        }
                    case Player.PlayerColor.blue:
                        {
                            GameObject.Find("Winner").GetComponent<WinnerText>().winner = "BLUE WINS";
                            break;
                        }
                }

                GameObject.Find("Countdown").GetComponent<Timer>().isStopped = true;
            }
        }
    }

    public void UpdateTile(GameObject tileGameObject)
    {
        Vector3Int cellPosition = StaticGridManager.GetSingleton().nonCollidableGroundTilemap.WorldToCell(tileGameObject.transform.position);
        tileDefs[cellPosition.x, cellPosition.y] = tileGameObject.GetComponent<TileDef>();
    }

    public List<GameObject> GetPlayersOnTile(Vector3 position)
    {
        List<GameObject> playersOnTile = new List<GameObject>();
        Vector3Int cellPosition = StaticGridManager.GetSingleton().nonCollidableGroundTilemap.WorldToCell(position);

        foreach (GameObject player in playerGameObjects)
        {
            Vector3Int playerCellPosition = StaticGridManager.GetSingleton().nonCollidableGroundTilemap.WorldToCell(player.transform.position);
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
        Vector3Int cellPosition = StaticGridManager.GetSingleton().nonCollidableGroundTilemap.WorldToCell(position);
        Vector3 cellCenterWorldPosition = StaticGridManager.GetSingleton().nonCollidableGroundTilemap.GetCellCenterWorld(cellPosition);
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
                Vector3 nextCellWorldPosition = StaticGridManager.GetSingleton().nonCollidableGroundTilemap.GetCellCenterWorld(nextCellPosition);
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
        Vector3Int cellPosition = StaticGridManager.GetSingleton().nonCollidableGroundTilemap.WorldToCell(explodingBricksTileGameObject.transform.position);
        Vector3Int bottomNextCellPosition = cellPosition + Vector3Int.down;
        Vector3 bottomNextCellWorldPosition = StaticGridManager.GetSingleton().nonCollidableGroundTilemap.GetCellCenterWorld(bottomNextCellPosition);
        TileDef bottomNextTileDef = tileDefs[bottomNextCellPosition.x, bottomNextCellPosition.y];
        if (bottomNextTileDef != null && bottomNextTileDef.tileType == TileDef.TileType.Grass)
        {
            networkManager.RemoveObject(bottomNextTileDef.gameObject);
            networkManager.AddGridTile((int)CustomNetworkManager.SpawnPrefabs.GrassTile, bottomNextCellWorldPosition);
        }

        networkManager.RemoveObject(explodingBricksTileGameObject);
    }
}

