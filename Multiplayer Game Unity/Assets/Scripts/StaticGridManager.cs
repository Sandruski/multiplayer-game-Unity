using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StaticGridManager : MonoBehaviour
{
    #region Public
    public Tilemap collidableGroundTilemap;
    public Tilemap nonCollidableGroundTilemap;

    public int width;
    public int height;
    #endregion

    #region PublicNoInspector
    [HideInInspector]
    public Vector3Int topLeftSpawnTile;
    [HideInInspector]
    public Vector3Int topRightSpawnTile;
    [HideInInspector]
    public Vector3Int bottomLeftSpawnTile;
    [HideInInspector]
    public Vector3Int bottomRightSpawnTile;
    #endregion

    void Start()
    {
        topLeftSpawnTile = new Vector3Int(2, height - 1 - 1, 0);
        topRightSpawnTile = new Vector3Int(width - 2 - 1, height - 1 - 1, 0);
        bottomLeftSpawnTile = new Vector3Int(2, 1, 0);
        bottomRightSpawnTile = new Vector3Int(width - 2 - 1, 1, 0);

        CenterCamera();
    }

    public Vector3 GetCellCenterWorldPosition(Vector3 position)
    {
        Vector3Int cellPosition = nonCollidableGroundTilemap.WorldToCell(position);
        return nonCollidableGroundTilemap.GetCellCenterWorld(cellPosition);
    }

    public Vector3 GetPlayerSpawnPosition(Player.PlayerColor playerColor)
    {
        Vector3Int cellPosition = Vector3Int.zero;
        switch (playerColor)
        {
            case Player.PlayerColor.white:
                {
                    cellPosition = topLeftSpawnTile;
                    break;
                }
            case Player.PlayerColor.black:
                {
                    cellPosition = bottomRightSpawnTile;
                    break;
                }
            case Player.PlayerColor.red:
                {
                    cellPosition = bottomLeftSpawnTile;
                    break;
                }
            case Player.PlayerColor.blue:
                {
                    cellPosition = topRightSpawnTile;
                    break;
                }
            default:
                {
                    break;
                }
        }

        return nonCollidableGroundTilemap.GetCellCenterWorld(cellPosition);
    }

    private void CenterCamera()
    {
        Vector3Int cellPosition = new Vector3Int((width - 1) / 2, (height - 1) / 2, 0);
        Vector3 cellCenterWorldPosition = nonCollidableGroundTilemap.GetCellCenterWorld(cellPosition);
        Camera.main.transform.position = new Vector3(cellCenterWorldPosition.x, cellCenterWorldPosition.y, Camera.main.transform.position.z);
    }
}
