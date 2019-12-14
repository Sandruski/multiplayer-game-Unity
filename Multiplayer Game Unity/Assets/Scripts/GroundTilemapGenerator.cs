using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GroundTilemapGenerator : MonoBehaviour
{
    public Tilemap collidableGroundTilemap;
    public Tilemap nonCollidableGroundTilemap;

    public Tile bricks;
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
                Vector3Int position = new Vector3Int(x, y, 0);
                if (!collidableGroundTilemap.HasTile(position))
                {
                    bool isSpawnTile =
                        position == topLeftSpawnTile ||
                        position == topRightSpawnTile ||
                        position == bottomLeftSpawnTile ||
                        position == bottomRightSpawnTile ||
                        position == topLeftSpawnTileBottomTile ||
                        position == topLeftSpawnTileRightTile ||
                        position == topRightSpawnTileBottomTile ||
                        position == topRightSpawnTileLeftTile ||
                        position == bottomLeftSpawnTileTopTile ||
                        position == bottomLeftSpawnTileRightTile ||
                        position == bottomRightSpawnTileTopTile ||
                        position == bottomRightSpawnTileLeftTile;
                    if (!isSpawnTile && Random.value <= bricksProbability)
                    {
                        collidableGroundTilemap.SetTile(position, bricks);
                    }
                    else if (collidableGroundTilemap.HasTile(new Vector3Int(x, y + 1, 0)))
                    {
                        nonCollidableGroundTilemap.SetTile(position, grassWithShadow);
                    }
                    else
                    {
                        nonCollidableGroundTilemap.SetTile(position, grass);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
