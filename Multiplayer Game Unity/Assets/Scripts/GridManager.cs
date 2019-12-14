using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public List<GameObject> players;

    public Tilemap collidableGroundTilemap;
    public Tilemap nonCollidableGroundTilemap;

    public Tile bricks;
    public Tile grass;
    public Tile grassWithShadow;

    public int width;
    public int height;

    public float bricksProbability;

    private enum TileType { Block, Bricks, Grass };

    private TileType[,] tileTypes;

    private Vector3Int topLeftSpawnTile;
    private Vector3Int topRightSpawnTile;
    private Vector3Int bottomLeftSpawnTile;
    private Vector3Int bottomRightSpawnTile;

    private int explosionsPoolSize;
    private int explodingBricksPoolSize;
    private List<ExplosionController> explosionsPool;
    private List<ExplodingBrickController> explodingBricksPool;

    void Start()
    {
        explodingBricksPoolSize = explosionsPoolSize = width * height;

        Object explosionPrefab = Resources.Load("Explosion");
        explosionsPool = new List<ExplosionController>();
        for (uint i = 0u; i < explosionsPoolSize; ++i)
        {
            GameObject explosion = (GameObject)Instantiate(explosionPrefab);
            explosion.SetActive(false);
            ExplosionController explosionController = explosion.GetComponent<ExplosionController>();
            explosionsPool.Add(explosionController);
        }

        Object explodingBricksPrefab = Resources.Load("ExplodingBricks");
        explodingBricksPool = new List<ExplodingBrickController>();
        for (uint i = 0u; i < explodingBricksPoolSize; ++i)
        {
            GameObject explodingBrick = (GameObject)Instantiate(explodingBricksPrefab);
            explodingBrick.SetActive(false);
            ExplodingBrickController explodingBrickController = explodingBrick.GetComponent<ExplodingBrickController>();
            explodingBricksPool.Add(explodingBrickController);
        }

        topLeftSpawnTile = new Vector3Int(2, height - 1 - 1, 0);
        topRightSpawnTile = new Vector3Int(width - 2 - 1, height - 1 - 1, 0);
        bottomLeftSpawnTile = new Vector3Int(2, 1, 0);
        bottomRightSpawnTile = new Vector3Int(width - 2 - 1, 1, 0);

        Vector3Int topLeftSpawnTileBottomTile = topLeftSpawnTile + Vector3Int.down;
        Vector3Int topLeftSpawnTileRightTile = topLeftSpawnTile + Vector3Int.right;
        Vector3Int topRightSpawnTileBottomTile = topRightSpawnTile + Vector3Int.down;
        Vector3Int topRightSpawnTileLeftTile = topRightSpawnTile + Vector3Int.left;
        Vector3Int bottomLeftSpawnTileTopTile = bottomLeftSpawnTile + Vector3Int.up;
        Vector3Int bottomLeftSpawnTileRightTile = bottomLeftSpawnTile + Vector3Int.right;
        Vector3Int bottomRightSpawnTileTopTile = bottomRightSpawnTile + Vector3Int.up;
        Vector3Int bottomRightSpawnTileLeftTile = bottomRightSpawnTile + Vector3Int.left;

        tileTypes = new TileType[width, height];

        for (int x = 0; x < width; ++x)
        {
            for (int y = height - 1; y >= 0; --y)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (!collidableGroundTilemap.HasTile(cellPosition))
                {
                    bool isSpawnTile =
                        cellPosition == topLeftSpawnTile 
                        || cellPosition == topRightSpawnTile
                        || cellPosition == bottomLeftSpawnTile
                        || cellPosition == bottomRightSpawnTile
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
                        collidableGroundTilemap.SetTile(cellPosition, bricks);
                        tileTypes[x, y] = TileType.Bricks;
                    }
                    else
                    {
                        if (collidableGroundTilemap.HasTile(new Vector3Int(x, y + 1, 0)))
                        {
                            nonCollidableGroundTilemap.SetTile(cellPosition, grassWithShadow);
                        }
                        else
                        {
                            nonCollidableGroundTilemap.SetTile(cellPosition, grass);
                        }

                        tileTypes[x, y] = TileType.Grass;
                    }
                }
                else
                {
                    tileTypes[x, y] = TileType.Block;
                }
            }
        }
    }

    public Vector3 GetCellCenterPosition(Vector3 position)
    {
        Vector3Int cellPosition = nonCollidableGroundTilemap.WorldToCell(position);
        return nonCollidableGroundTilemap.GetCellCenterWorld(cellPosition);
    }

    public List<GameObject> GetPlayersOnTile(Vector3 position)
    {
        List<GameObject> playersOnTile = new List<GameObject>();
        Vector3Int cellPosition = nonCollidableGroundTilemap.WorldToCell(position);

        foreach (GameObject player in players)
        {
            Vector3Int playerCellPosition = nonCollidableGroundTilemap.WorldToCell(player.transform.position);
            if (cellPosition == playerCellPosition)
            {
                playersOnTile.Add(player);
            }
        }

        return playersOnTile;
    }

    public void SpawnExplosions(Player player, Vector3 position)
    {
        Vector3Int cellPosition = nonCollidableGroundTilemap.WorldToCell(position);

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
                    SpawnExplosion(player, nonCollidableGroundTilemap.GetCellCenterWorld(nextCellPosition));
                }
            }
        }
    }

    private bool SpawnExplosion(Player player, Vector3 position)
    {
        for (int i = 0; i < explosionsPoolSize; ++i)
        {
            ExplosionController explosion = explosionsPool[i];
            if (!explosion.isAlive)
            {
                explosion.SetOwner(player);
                explosion.Spawn(position);
                return true;
            }
        }

        return false;
    }

    private bool SpawnExplodingBrick(Vector3 position)
    {
        for (int i = 0; i < explodingBricksPoolSize; ++i)
        {
            ExplodingBrickController explodingBrick = explodingBricksPool[i];
            if (!explodingBrick.isAlive)
            {
                explodingBrick.Spawn(position);
                return true;
            }
        }

        return false;
    }
}
