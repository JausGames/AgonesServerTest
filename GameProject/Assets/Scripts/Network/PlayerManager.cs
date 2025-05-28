using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : NetworkBehaviour
{
    public NetworkObject myPrefab;
    [SerializeField]
    private Vector2 center;

    private List<Player> players = new List<Player>();
    private UnityEvent onLobbyFull = new UnityEvent();
    private int maxPlayer = 3;
    private List<Vector2> playerPositions;

    public UnityEvent OnLobbyFull { get => onLobbyFull; set => onLobbyFull = value; }

    internal Vector3 GetPlayerPosition(ulong target, ulong from)
    {
        if(target == from) return playerPositions[0];
        var targetId = players.FindIndex(p => p.OwnerClientId == target);
        var fromId = players.FindIndex(p => p.OwnerClientId == from);

        var diff = (targetId - fromId + maxPlayer) % maxPlayer;

        return playerPositions[diff];
    }

    public List<Player> Players { get => players; set => players = value; }
    public Vector2 Center { get => center; }

    private void Start()
    {
        /*if (IsServer)
            NetworkManager.OnClientConnectedCallback += AddPlayerById;
        else
            NetworkManager.OnClientConnectedCallback += AddPlayerFromIdServerRpc;*/

        OnLobbyFull.AddListener(SetUpUiClientRpc);
    }

    [ClientRpc]
    public void SetUpUiClientRpc()
    {
    }


    public List<Vector2> GetPointsOnUnitCircle(int nbPoints)
    {
        List<Vector2> points = new List<Vector2>();

        Vector2 startPoint = new Vector2(0, -1f);
        points.Add(startPoint);

        float angleStep = 2 * Mathf.PI / nbPoints;

        for (int i = nbPoints - 1; i >= 0; i--)
        {
            float angle = i * angleStep;
            float x = Mathf.Sin(angle);
            float y = -Mathf.Cos(angle);

            points.Add(new Vector2(x, y));
        }

        return points;
    }

    internal void AddPlayerById(ulong clientId, string name)
    {
        if (players.Count == maxPlayer) return;
        if (!players.Any(p => p.OwnerClientId == clientId))
        {
            if (NetworkManager.ConnectedClients.TryGetValue(clientId, out var client))
            {
                var player = client.PlayerObject.GetComponent<Player>();
                player.Name = name;
                players.Add(player);
            }
            else StartCoroutine(WaitForClientToConnect(clientId, name));
        }

        if (players.Count == maxPlayer)
        {
            SyncPlayersClientRpc(ComposePlayersString(players));
            OnLobbyFull.Invoke();
        }
    }


    internal int NextId(ulong key)
    {
        var next = players.FindIndex(p => p.OwnerClientId == key) + 1;
        next = next >= players.Count ? 0 : next;
        return next;
    }
    internal Player Next(ulong key) => players[NextId(key)];

    private string ComposePlayersString(List<Player> players)
    {
        var res = "";
        players.ForEach(p => res += $"{p.NetworkObjectId}/#/{players.IndexOf(p)}/;/");
        return res;
    }

    [ClientRpc]
    private void SyncPlayersClientRpc(string playersString)
    {
        var split = playersString.Split("/;/").ToList();
        split.Remove(split.Last());     // gay
        var arr = new Player[split.Count()];
        split.ToList().ForEach(s =>
        {
            var ss = s.Split("/#/");
            var player = GetNetworkObject(ulong.Parse(ss[0])).GetComponent<Player>();
            arr[int.Parse(ss[1])] = player;
        });
        players = arr.ToList();
    }



    private IEnumerator WaitForClientToConnect(ulong clientId, string name)
    {
        while (!NetworkManager.ConnectedClients.TryGetValue(clientId, out var client))
            yield return new WaitForEndOfFrame();

        AddPlayerById(clientId, name);
    }

    internal void RemovePlayer(ulong clientId)
    {
        NetworkManager.ConnectedClients.TryGetValue(clientId, out var client);
        if (client != null)
        {
            var player = client.PlayerObject.GetComponent<Player>();
            players.Remove(player);
        }
    }
}
