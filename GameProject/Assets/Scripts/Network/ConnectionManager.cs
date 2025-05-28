using Agones;
#if UNITY_EDITOR
using ParrelSync;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionManager : MonoBehaviour
{
    [Header("Player settings")]
    [SerializeField] private TMPro.TMP_InputField inputName;

    [Space]
    [Header("Manual connection")]
    [SerializeField] Button button_Host;
    [SerializeField] Button button_Server;
    [SerializeField] Button button_Client;
    [SerializeField] TMPro.TMP_InputField ip_address;

    [Space]
    [Header("Connection settings")]
    [SerializeField] bool startServerAuto = false;
    [SerializeField] bool startHostAuto = false;
    [SerializeField] bool startClientAuto = false;
    [Header("Local")]
    [SerializeField] private bool localTest = false;
    [Header("Parrel Sync")]
    [SerializeField] private bool ParrelStart;

    [Space]
    [Header("Components")]
    [SerializeField] GameObject connexionMenu;
    [SerializeField] SpawnManager spawnManager;
    [SerializeField] PlayerManager playerManager;
    private AgonesAlphaSdk agones;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private UNetTransport transport;
    Ping ping;
    List<int> pingsArray = new List<int>();



    //Dictionary<ulong, NetworkObject> dict = new Dictionary<ulong, NetworkObject>();

    [SerializeField]
    private ConnectionString ConnectionString
    {
        get
        {
            var co = new ConnectionString { Id = new Guid(), Name = PlayerSettings.Name, Licensed = true };
            return co;
        }
    }
    public int Ping
    {
        get
        {
            if (pingsArray.Count == 0)
                return 0;
            else
                return pingsArray.Sum() / pingsArray.Count;
        }
    }

    public PlayerManager PlayerManager { get { if (playerManager == null) playerManager = FindObjectOfType<PlayerManager>(); return playerManager; } set => playerManager = value; }

    private void Start()
    {
        ip_address.onSubmit.AddListener(delegate { transport.ConnectAddress = ip_address.text; });



        button_Host.onClick.AddListener(Host);
        button_Server.onClick.AddListener(Server);
        button_Client.onClick.AddListener(Client);

        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;

        if (ClonesManager.IsClone())
        {
            string customArgument = ClonesManager.GetArgument();
            inputName.text = customArgument;
            PlayerSettings.Name = customArgument;
            if(ParrelStart)
            Client();
        }
        else
        {
            inputName.text = "Bogoss";
            PlayerSettings.Name = "Bogoss";
            if (ParrelStart)
                Host();
        }
#else
  //Debug.unityLogger.logEnabled = false;
#endif

        if (startServerAuto)
            Server();
        if (startHostAuto)
            Host();
        if (startClientAuto)
            Client();
        /*else
            PlayerSettings.CharacterId = -1;*/
        inputName.onValueChanged.AddListener(delegate { PlayerSettings.Name = inputName.text; });
    }
    void OnGUI()
    {
        /*GUILayout.BeginArea(new Rect(10, 70, 300, 300));
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
            StatusLabels(Ping);

        GUILayout.EndArea();*/
    }

    async void TryConnectToAgonesAsync()
    {

        agones = GetComponent<Agones.AgonesAlphaSdk>();
        agones.enabled = true;
        bool ok = await agones.Connect();
        if (ok)
        {
            Debug.Log(("Server - Connected"));
        }
        else
        {
            Debug.Log(("Server - Failed to connect, exiting"));
            Application.Quit(1);
        }
        try
        {

            ok = await agones.Ready();
            if (ok)
            {
                Debug.Log($"Server - Ready");
                //await agones.SetPlayerCapacity(20);
            }
            else
            {
                Debug.Log($"Server - Ready failed");
            }
        }
        catch (Exception e)
        {
            Debug.Log(($"Server - Exception : {e.Message}"));
            Application.Quit(1);
        }
    }

    private void OnDestroy()
    {
        // Prevent error in the editor
        if (NetworkManager.Singleton == null) { return; }

        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;

    }
    private void Update()
    {
        if (NetworkManager.Singleton.IsClient && ping != null)
        {
            if (ping.isDone)
            {
                pingsArray.Add(ping.time);
                if (pingsArray.Count > 10) pingsArray.RemoveAt(0);
                ping = new Ping(transport.ConnectAddress);
            }
        }
    }

    public void Host()
    {
        if (string.IsNullOrWhiteSpace(PlayerSettings.Name)) return;
        // Hook up password approval check
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(JsonUtility.ToJson(ConnectionString));
        NetworkManager.Singleton.StartHost();
    }
    void Server()
    {
        if (!localTest)
            TryConnectToAgonesAsync();

        //if (string.IsNullOrWhiteSpace(PlayerSettings.Name)) return;
        // Hook up password approval check
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(JsonUtility.ToJson(ConnectionString));
        NetworkManager.Singleton.StartServer();
    }
    void Client()
    {
        if (!localTest)
            CheckForServers();
        else
        {
            // Set password ready to send to the server to validate
            transport.ConnectAddress = "127.0.0.1";

            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(JsonUtility.ToJson(ConnectionString));
            NetworkManager.Singleton.StartClient();
        }
    }

    // Update is called once per frame
    void CheckForServers()
    {
        PlayerGameInfo playerGameInfo = new PlayerGameInfo() { id = new System.Guid().ToString(), elo = 200, gameVersion = Application.version, name = PlayerSettings.Name };
        var http = gameObject.AddComponent<HttpRequestHelper>();
        CoroutineWithData cd = new CoroutineWithData(this, http.GetServer(playerGameInfo));
        StartCoroutine(WaitForGameServers(cd));
    }
    private IEnumerator WaitForGameServers(CoroutineWithData corout)
    {
        //wait
        while (!(corout.result is string) || corout.result == null)
        {
            Debug.Log("EditorUI, WaitForGameServers : data is null");
            yield return false;
        }
        //do stuff
        var gs = JsonUtility.FromJson<GameServer>((string)corout.result);
        //unetTransport.ConnectAddress = 

        Debug.Log(gs);

        transport.ConnectAddress = gs.ip;
        transport.ConnectPort = gs.port;

        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(JsonUtility.ToJson(ConnectionString));
        NetworkManager.Singleton.StartClient();
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!localTest)
            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            {
                _ = AddPlayerAndAllocate(clientId);
            }

        // Are we the client that is connecting?
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            //ping = new Ping(transport.ConnectAddress);
            connexionMenu.SetActive(false);
        }
    }

    private async Task AddPlayerAndAllocate(ulong clientId)
    {
        var nb = await agones.GetPlayerCount();

        if (nb == 0) _ = agones.Allocate();

        _ = agones.PlayerConnect(clientId.ToString());
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            playerManager.RemovePlayer(clientId);
            if (!localTest)
                _ = AgonesDisconectPlayerAsync(clientId);
        }

        // Are we the client that is disconnecting?
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            connexionMenu.SetActive(false);
            //passwordEntryUI.SetActive(true);
            //leaveButton.SetActive(false);
        }
    }

    private async Task AgonesDisconectPlayerAsync(ulong clientId)
    {
        var ok = await agones.PlayerDisconnect(clientId.ToString());
        var nb = await agones.GetPlayerCount();
        if (nb == 0)
            await agones.Shutdown();
    }

    private void HandleServerStarted()
    {
        connexionMenu.SetActive(false);
        // Temporary workaround to treat host as client
        if (NetworkManager.Singleton.IsHost)
        {
            spawnManager.Init();
            //HandleClientConnected(NetworkManager.ServerClientId);
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // The client identifier to be authenticated
        var clientId = request.ClientNetworkId;

        // Additional connection data defined by user code
        var connectionData = request.Payload;


        var connectionString = JsonUtility.FromJson<ConnectionString>(Encoding.Default.GetString(connectionData));
        var gameReturnStatus = GetConnectStatus(connectionString);

        // Your approval logic determines the following values
        response.Approved = gameReturnStatus == ConnectStatus.Success ? true : false;

        response.CreatePlayerObject = true;

        // The prefab hash value of the NetworkPrefab, if null the default NetworkManager player prefab is used
        response.PlayerPrefabHash = null;

        // Position to spawn the player object (if null it uses default of Vector3.zero)
        response.Position = Vector3.zero;

        // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
        response.Rotation = Quaternion.identity;

        response.Reason = gameReturnStatus.ToString();

        // If additional approval steps are needed, set this to true until the additional steps are complete
        // once it transitions from true to false the connection approval response will be processed.
        response.Pending = false;

        if (response.Approved) PlayerManager.AddPlayerById(clientId, connectionString.Name);
    }

    public async Task ShutdownServer()
    {
        SetCanvasActiveClientRpc();

        NetworkManager.Singleton.ConnectedClientsList.ToList().ForEach(c =>
        {
            if (c.PlayerObject.IsSpawned)
                c.PlayerObject.Despawn();
            NetworkManager.Singleton.DisconnectClient(c.ClientId);
        });

        var shut = await agones.Shutdown();
    }

    [ClientRpc]
    private void SetCanvasActiveClientRpc()
    {
        connexionMenu.SetActive(true);
    }

    enum ConnectStatus
    {
        Success,
        LoggedInAgain,
        ServerFull,
        IncompatibleBuildType,
        NoLicense
    }
    ConnectStatus GetConnectStatus(ConnectionString connectionPayload)
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= 20)
            return ConnectStatus.ServerFull;

        if (!connectionPayload.Licensed)
            return ConnectStatus.NoLicense;

        /*if (connectionPayload.isDebug != Debug.isDebugBuild)
        {
            return ConnectStatus.IncompatibleBuildType;
        }*/

        /*return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(connectionPayload.playerId) ?
            ConnectStatus.LoggedInAgain : ConnectStatus.Success;*/
        return ConnectStatus.Success;
    }

    static void StatusLabels(int ping)
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Mode: " + mode);
        if (mode == "Client")
            GUILayout.Label("Ping: " + ping);
    }


}

