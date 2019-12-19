using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System.Collections.Generic;

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
    #region Public
    [HideInInspector]
    public short playerPrefabIndex;

    public string[] playerNames = new string[] { "White", "Black", "Red", "Blue" };
    
    public enum SpawnPrefabs
    {
        WhitePlayer, BlackPlayer, RedPlayer, BluePlayer,
        Bomb,
        GrassTile, GrassWithShadowTile,
        BricksTile, ExplodingBricksTile,
        ExplosionTile,
        DynamicGridManager
    }

    // MatchMaking
    public MenuManager menuManager;
    public List<MatchInfoSnapshot> matchList;
    #endregion

    #region Private
    // MatchMaking
    private string newMatchName;
    private uint maxMatchSize = 4;
    #endregion

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
        Player.PlayerColor playerColor = (Player.PlayerColor)msg.prefabIndex;
        Vector3 playerPosition = StaticGridManager.GetSingleton().GetPlayerSpawnPosition(playerColor);
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

    public void AddBomb(Vector3 position, Player owner)
    {
        GameObject newObject = Instantiate(
            spawnPrefabs[(int)SpawnPrefabs.Bomb],
            position,
            Quaternion.identity);

        newObject.GetComponent<BombController>().owner = owner;

        NetworkServer.Spawn(newObject);
    }

    public void AddExplosion(Vector3 position, ExplosionController.Orientation orientation)
    {
        GameObject newObject = Instantiate(
            spawnPrefabs[(int)SpawnPrefabs.ExplosionTile],
            position,
            Quaternion.identity);

        ExplosionController explosionController = newObject.GetComponent<ExplosionController>();
        explosionController.orientation = orientation;

        NetworkServer.Spawn(newObject);
    }

    public void AddGridTile(int objIndex, Vector3 position)
    {
        GameObject newObject = Instantiate(
            spawnPrefabs[objIndex],
            position,
            Quaternion.identity);

        NetworkServer.Spawn(newObject);

        //GameObject.Find("DynamicGridManager").GetComponent<DynamicGridManager>().RpcSyncTile(newObject);
    }

    public void RemoveObject(GameObject gameObject)
    {
        NetworkServer.Destroy(gameObject);
    }

    public void OnCreateMatch(string matchName)
    {
        Debug.Log("OnCreateMatchClicked" + matchName);
        NetworkManager.singleton.StartMatchMaker();
        NetworkManager.singleton.matchMaker.CreateMatch(matchName, maxMatchSize, true, "", "", "", 0, 0, OnMatchCreate);
    }

    public void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        NetworkManager.singleton.StopMatchMaker();

        if (success)
        {
            NetworkManager.singleton.StartHost(matchInfo);
        }
        else
        {
            Debug.Log("OnMatchCreate failed");
        }
    }

    public bool OnJoinMatch(string room)
    {
        foreach(MatchInfoSnapshot match in matchList)
        {
            if (match.name == room)
            {
                NetworkManager.singleton.StartMatchMaker();
                NetworkManager.singleton.matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, OnMatchJoin);
                return true;
            }
        }
        return false;
    }

    void OnMatchJoin(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        NetworkManager.singleton.StopMatchMaker();

        if (success)
        {
            NetworkManager.singleton.StartClient(matchInfo);
        }
        else
        {
            Debug.Log("OnMatchJoin failed");
        }
    }

    public void RequestMatches()
    {
        NetworkManager.singleton.StartMatchMaker();
        NetworkManager.singleton.matchMaker.ListMatches(0, 10, "", true, 0, 0, OnMatchList);
    }

    public void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        NetworkManager.singleton.StopMatchMaker();

        if (success)
        {
            matchList = matches;
            if (menuManager != null)
            {
                menuManager.OnUpdateDropbox();
            }
        }
        else
        {
            Debug.Log("OnMatchList failed");
        }
    }
}
