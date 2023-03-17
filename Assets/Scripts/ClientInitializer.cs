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

public class ClientInitializer : MonoBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] TMPro.TMP_InputField inputName;


    Ping ping;
    List<int> pingsArray = new List<int>();
    UNetTransport unetTransform;
    int pingCount = 49;

    //string ipAdress = "172.30.114.48";
    string ipAdress = "20.19.185.71";


    // Start is called before the first frame update
    void Start()
    {

        unetTransform = GetComponent<UNetTransport>();

        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

        //Client();

        //StartCoroutine(StartAgoneServer());
        /*#if UNITY_EDITOR
            //Is this unity editor instance opening a clone project?
            if (ClonesManager.IsClone())
            {
                string customArgument = ClonesManager.GetArgument();
                inputName.text = customArgument;
            }
            else
            {
                inputName.text = "bogoss";
            }
        #endif*/
    }

    IEnumerator StartAgoneServer()
    {
        var connected = TryConnectToAgonesAsync();

        while (!connected.IsCompleted)
        {
            yield return new WaitForEndOfFrame();
        }

        if (connected.Result)
        {
            var ready = IsAgonesServerReadyAsync();
            while (!ready.IsCompleted)
            {
                yield return new WaitForEndOfFrame();
            }

            if (ready.Result)
            {
                Debug.Log("____ Agones server ready ____");
            }
            else
            {
                Debug.Log("____ Agones server reached but not ready ____");
            }
        }
        else
        {
            Debug.Log("____ Agones server unreached ____");
        }
    }

    async Task<bool> TryConnectToAgonesAsync()
    {
        var agones = GetComponent<Agones.AgonesSdk>();
        return await agones.Connect();
    }

    async Task<bool> IsAgonesServerReadyAsync()
    {

        var agones = GetComponent<Agones.AgonesSdk>(); 
        return await agones.Ready();
    }
    private void OnDestroy()
    {
        // Prevent error in the editor
        if (NetworkManager.Singleton == null) { return; }

        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }
    public void Server()
    {
        // Hook up password approval check
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        //NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes();
        NetworkManager.Singleton.StartServer();
    }
    public void Host()
    {
        //if (inputName.text == "") return;
        // Hook up password approval check
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes("player01");
        NetworkManager.Singleton.StartHost();
    }

    public void Client()
    {
        //if (inputName.text == "") return;
        // Set password ready to send to the server to validate
        //NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(inputName.text);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes("player01");
        NetworkManager.Singleton.StartClient();
    }
    public void Leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            //MLAPI
            //NetworkManager.Singleton.StopHost();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            //MLAPI
            //NetworkManager.Singleton.StopClient();
        }

        //passwordEntryUI.SetActive(true);
        //leaveButton.SetActive(false);
    }

    private void HandleServerStarted()
    {
        // Temporary workaround to treat host as client
        if (NetworkManager.Singleton.IsHost)
        {
            //HandleClientConnected(NetworkManager.ServerClientId);
        }
    }
    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log("MainMenu, HandleClientConnected : clientid = " + clientId);
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {

        }

        // Are we the client that is connecting?
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {

        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        // Are we the client that is disconnecting?
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            //passwordEntryUI.SetActive(true);
            //leaveButton.SetActive(false);
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        //var connectionData = request.Payload;
        //var playerName = Encoding.Default.GetString(connectionData);

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
        //Debug.Log("connection approval : name = " + playerName +", id = " + clientId);

        GameObject go = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        var networkObject = go.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(clientId, false);
    }

}
