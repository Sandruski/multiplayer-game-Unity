using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618
public class MsgTypes
{
    public const short PlayerPrefabSelect = MsgType.Highest + 1;
    public class PlayerPrefabMsg : MessageBase
    {
        public short controllerId;
        public short prefabIndex;
    }
}

public class CustomNetworkManager : NetworkManager
{
    public short playerPrefabIndex;

    public string[] playerNames = new string[] { "White", "Black", "Red", "Blue" };

    public enum SpawnPrefabs { WhitePlayer, BlackPlayer, RedPlayer, BluePlayer, Bomb }

    private GridManager gridManager;

    private void OnGUI()
    {
        if (!isNetworkActive)
        {
            playerPrefabIndex = (short)GUI.SelectionGrid(
                new Rect(Screen.width - 200, 10, 200, 50),
                playerPrefabIndex,
                playerNames,
                3);
        }
    }

    private void Start()
    {
        gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
    }

    // 1) Executed in the server
    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler(MsgTypes.PlayerPrefabSelect, OnPrefabResponse);
        base.OnStartServer();
    }

    // 2) Executed in the client
    public override void OnClientConnect(NetworkConnection conn)
    {
        client.RegisterHandler(MsgTypes.PlayerPrefabSelect, OnPrefabRequest);
        base.OnClientConnect(conn);
    }

    // 3) Executed in the server
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        MsgTypes.PlayerPrefabMsg msg = new MsgTypes.PlayerPrefabMsg();
        msg.controllerId = playerControllerId;
        NetworkServer.SendToClient(conn.connectionId, MsgTypes.PlayerPrefabSelect, msg);
    }

    // 4) Prefab requested in the client
    private void OnPrefabRequest(NetworkMessage netMsg)
    {
        MsgTypes.PlayerPrefabMsg msg = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>();
        msg.prefabIndex = playerPrefabIndex;
        client.Send(MsgTypes.PlayerPrefabSelect, msg);
    }

    // 5) Prefab communicated to the server
    private void OnPrefabResponse(NetworkMessage netMsg)
    {
        MsgTypes.PlayerPrefabMsg msg = netMsg.ReadMessage<MsgTypes.PlayerPrefabMsg>();
        playerPrefab = spawnPrefabs[msg.prefabIndex];
        Debug.Log("Prefab index is " + playerPrefabIndex);
        Player.PlayerColor playerColor = (Player.PlayerColor)msg.prefabIndex;
        Vector3 playerPosition = gridManager.GetPlayerSpawnPosition(playerColor);
        GameObject player = (GameObject)Instantiate(playerPrefab, playerPosition, Quaternion.identity);
        player.GetComponent<Player>().color = playerColor;
        NetworkServer.AddPlayerForConnection(netMsg.conn, player, msg.controllerId);
    }

    public void ChangePlayerPrefab(PlayerController currentPlayer, int prefabIndex)
    {
        // Instantiate a new GameObject where the previous one was
        GameObject newPlayer = Instantiate(spawnPrefabs[prefabIndex],
            currentPlayer.gameObject.transform.position,
            currentPlayer.gameObject.transform.rotation);

        // Destroy the previous player GameObject
        NetworkServer.Destroy(currentPlayer.gameObject);

        // Replace the connected player GameObject
        NetworkServer.ReplacePlayerForConnection(
            currentPlayer.connectionToClient, newPlayer, 0);
    }

    public void AddObject(int objIndex, Vector3 position)
    {
        GameObject newObject = Instantiate(
            spawnPrefabs[objIndex],
            position,
            Quaternion.identity);

        NetworkServer.Spawn(newObject);
    }

    public void AddBomb(Vector3 position, Player.PlayerColor playerColor)
    {
        GameObject newObject = Instantiate(
            spawnPrefabs[(int)SpawnPrefabs.Bomb],
            position,
            Quaternion.identity);

        BombController bombController = newObject.GetComponent<BombController>();
        bombController.playerColor = playerColor;

        NetworkServer.Spawn(newObject);
    }
}
