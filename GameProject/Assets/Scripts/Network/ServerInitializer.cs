using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Text;
using Unity.Netcode.Transports.UNET;
using TMPro;
using System.Threading.Tasks;
//using ParrelSync;

public class ServerInitializer : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] EnemySpawner spawner;


    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;

        Server();
    }

    async void TryConnectToAgonesAsync()
    {
        var agones = GetComponent<Agones.AgonesSdk>();
        bool connected = await agones.Connect();
        if (!connected)
        {
            Debug.Log("Agones: Connect() failed");
            return;
        }

        Debug.Log("Agones: .. connected");

        Debug.Log("Agones: Marking as ready...");
        bool readied = await agones.Ready();
    }

    private void OnDestroy()
    {
        // Prevent error in the editor
        if (NetworkManager.Singleton == null) { return; }

        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;

    }
    public void Server()
    {
        TryConnectToAgonesAsync();
        // Hook up password approval check
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        //NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes();
        NetworkManager.Singleton.StartServer();


    }

    private void HandleServerStarted()
    {
        StartCoroutine(spawner.SpawnStuff());
        // Temporary workaround to treat host as client
        if (NetworkManager.Singleton.IsHost)
        {
            //HandleClientConnected(NetworkManager.ServerClientId);
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        var connectionData = request.Payload;
        var playerName = Encoding.Default.GetString(connectionData);

        // Your approval logic determines the following values
        response.Approved = true;
        response.CreatePlayerObject = false;

        // The prefab hash value of the NetworkPrefab, if null the default NetworkManager player prefab is used
        response.PlayerPrefabHash = null;

        // Position to spawn the player object (if null it uses default of Vector3.zero)
        response.Position = Vector3.zero;

        // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
        response.Rotation = Quaternion.identity;

        // If additional approval steps are needed, set this to true until the additional steps are complete
        // once it transitions from true to false the connection approval response will be processed.
        response.Pending = false;
        Debug.Log("connection approval : name = " + playerName + ", id = " + clientId);

        GameObject go = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        var networkObject = go.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(clientId, false);
    }

}