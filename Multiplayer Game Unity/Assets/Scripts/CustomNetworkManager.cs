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
    public short playerPrefabIndex;

    public string[] playerNames = new string[] { "White", "Black", "Red", "Blue" };

    // MatchMaking

    public List<MatchInfoSnapshot> matchList;
    string newMatchName;
    uint maxMatchSize = 4;

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
        base.OnServerAddPlayer(netMsg.conn, msg.controllerId);
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

    public void AddObject(int objIndex, Transform t)
    {
        GameObject newObject = Instantiate(
            spawnPrefabs[objIndex],
            t.position,
            Quaternion.identity);

        NetworkServer.Spawn(newObject);
    }

    public void OnCreateMatchClicked(string matchName)
    {
        Debug.Log("OnCreateMatchClicked" + matchName);
        NetworkManager.singleton.StartMatchMaker();
        NetworkManager.singleton.matchMaker.CreateMatch(matchName, maxMatchSize, true, "", "", "", 0, 0, OnMatchCreate);
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
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

    public void OnJoinMatchClicked(UnityEngine.Networking.Types.NetworkID networkID)
    {
        NetworkManager.singleton.StartMatchMaker();
        NetworkManager.singleton.matchMaker.JoinMatch(networkID, "", "", "", 0, 0, OnMatchJoin);
    }

    public void OnMatchJoin(bool success, string extendedInfo, MatchInfo matchInfo)
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

    void RequestMatches()
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
        }
        else
        {
            Debug.Log("OnMatchList failed");
        }
    }
}
