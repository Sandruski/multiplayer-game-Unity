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
    public AnimatedTile explodingBricks;
    public Tile grass;
    public Tile grassWithShadow;

    public int width;
    public int height;

    public float bricksProbability;

    private Vector3Int topLeftSpawnTile;
    private Vector3Int topRightSpawnTile;
    private Vector3Int bottomLeftSpawnTile;
    private Vector3Int bottomRightSpawnTile;

    // Start is called before the first frame update
    void Start()
    {
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
                    }
                    else if (collidableGroundTilemap.HasTile(new Vector3Int(x, y + 1, 0)))
                    {
                        nonCollidableGroundTilemap.SetTile(cellPosition, grassWithShadow);
                    }
                    else
                    {
                        nonCollidableGroundTilemap.SetTile(cellPosition, grass);
                    }
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
}
