using Mirror;
using Open.Nat;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class HUD : MonoBehaviour
{
    NetworkManager Manager;
    public bool Show = true;
    public int Width;
    public int Height;
    public string Hostname = "pong.froogo.co.uk";
    public int Port = 32729;

    private string Name = "";
    private string Password = "";

    private string Status = "No Status";

    private IPEndPoint IP;

    private string ConnectingIP;
    private int ConnectingPort;

    private IgnoranceTransport Transport;

    public float SendDelay = 5f;
    private float NextSend = 0f;

    public float AttemptTimeout = 10f;
    private float Attempt = 0f;

    private Lobby[] Lobbies;

    private Lobby Room;

    public enum Types
    {
        Create,
        Alive,
        List,
        Request,
        Port
    };

    public enum States
    {
        Wait,
        Create,
        Created,
        List,
        Listed,
        Lobby,
        Request,
        Requested,
        Connected
    };

    private States State = States.Wait;

    private UdpClient Connection;

    void Awake()
    {
        Manager = GetComponent<NetworkManager>();

        Transport = GetComponent<IgnoranceTransport>();

        Connection = new UdpClient();
        Connection.BeginReceive(OnReceive, null);
    }

    async void OnReceive(IAsyncResult ar)
    {
        // Receive our data and the IP it came from.
        IPEndPoint ip = null;
        var data = Connection.EndReceive(ar, ref ip);

        // Convert our data bytes into a string.
        var message = Encoding.UTF8.GetString(data);

        // Deserialise our string into a response via JSON.
        var response = JsonUtility.FromJson<Response>(message);

        // Switch our response:
        switch (response.Type)
        {
            // If our room was successfully created:
            case Response.Types.Created:
                // Set our state to created.
                State = States.Created;

                // Update our status.
                Status = "Room succesfully created!";

                var discoverer = new NatDiscoverer();
                var cts = new CancellationTokenSource(10000);
                var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

                await device.CreatePortMapAsync(new Mapping(Protocol.Udp, Transport.port, Transport.port, "Pong UDP UPnP"));

                break;

            // If we have succesfully received a lobby listing:
            case Response.Types.Listed:
                // Set our state to listed.
                State = States.Listed;

                // Set our lobbies to our new listing.
                Lobbies = response.Listing;

                // Update our status.
                Status = "Listing received!";

                break;

            // If we have succesfully received lobby information:
            case Response.Types.Requested:
                // Set our state to requested.
                State = States.Requested;

                // Set our IP and port to connect to.
                ConnectingIP = response.IP;
                ConnectingPort = response.Port;

                Attempt = 0f;

                // Update our status.
                Status = "Lobby information received!";

                break;

            // If our room name has already been taken:
            case Response.Types.Taken:
                // Set our state to wait.
                State = States.Wait;

                // Update our status.
                Status = "Room name already taken!";

                break;

            case Response.Types.PasswordMismatch:
                // Set our state to wait.
                State = States.Lobby;

                // Update our status.
                Status = "Password incorrect.";

                break;
        }

        // Don't wait for our delay for the next send.
        NextSend = 0f;

        // Setup the next receive.
        Connection.BeginReceive(OnReceive, null);
    }

    void Update()
    {
        if (IP == null)
            SetIP();

        if (IP == null || NextSend > Time.time)
            return;

        switch (State)
        {
            case States.Create:
                SendCreate();
                break;

            case States.Created:
                // If we're not already hosting:
                if (!Manager.IsClientConnected() && !NetworkServer.active)
                {
                    // Start hosting our server.
                    Manager.StartHost();
                }

                SendAlive();
                break;

            case States.List:
                SendList();
                break;

            case States.Request:
                SendRequest();
                break;

            case States.Requested:
                if (Manager.IsClientConnected())
                    break;

                if (Attempt == 0f)
                {
                    Manager.networkAddress = ConnectingIP;
                    Transport.port = (ushort)ConnectingPort;

                    Status = "Connecting to " + ConnectingIP + ":" + ConnectingPort.ToString();

                    Manager.StartClient();

                    Attempt = Time.time;
                }
                else if (Attempt + AttemptTimeout < Time.time)
                {
                    Manager.StopClient();

                    State = States.Wait;

                    Status = "Connection timed out...";
                }

                break;
        }
    }

    void OnGUI()
    {
        if (!Show)
            return;

        var rect = new Rect((Screen.width - Width) / 2, (Screen.height - Height) / 2, Width, Height);

        GUILayout.BeginArea(rect);

        GUILayout.Label("State: " + State.ToString());
        GUILayout.Label(Status);

        if (!Manager.IsClientConnected() && !NetworkServer.active)
        {
            switch (State)
            {
                case States.Wait:
                    if (GUILayout.Button("Host"))
                        State = States.Create;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Name:");
                    Name = GUILayout.TextField(Name);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Password:");
                    Password = GUILayout.PasswordField(Password, '*');
                    GUILayout.EndHorizontal();

                    GUILayout.Label("");

                    if (GUILayout.Button("Connect"))
                    {
                        Status = "Waiting for listing...";
                        State = States.List;
                    }

                    break;

                case States.Listed:
                    if (GUILayout.Button("Back"))
                        State = States.Wait;

                    if (Lobbies != null)
                    {
                        GUILayout.Label("");

                        foreach (var lobby in Lobbies)
                        {
                            if (GUILayout.Button(lobby.Name))
                            {
                                Room = lobby;
                                State = States.Lobby;
                            }
                        }
                    }

                    break;

                case States.Lobby:
                    if (GUILayout.Button("Back"))
                        State = States.Listed;

                    GUILayout.Label("");

                    GUILayout.Label("Lobby: " + Room.Name);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Password:");
                    Password = GUILayout.PasswordField(Password, '*');
                    GUILayout.EndHorizontal();

                    GUILayout.Label("");

                    if (GUILayout.Button("Connect"))
                    {
                        Status = "Requesting lobby information...";
                        State = States.Request;
                    }

                    break;
            }
        }

        GUILayout.EndArea();
    }

    private void SetIP()
    {
        Status = "Resolving IP...";

        var addresses = Dns.GetHostAddresses(Hostname);

        if (addresses.Length == 0)
            return;

        Status = "IP resolved!";

        IP = new IPEndPoint(addresses[0], Port);
    }

    private void SendCreate()
    {
        var message = new MessageCreate()
        {
            Type = Types.Create,
            Name = Name,
            Password = Password
        };

        Send(message);
    }

    private void SendList()
    {
        var message = new MessageList()
        {
            Type = Types.List
        };

        Send(message);
    }

    private void SendRequest()
    {
        var message = new MessageRequest()
        {
            Type = Types.Request,
            ID = Room.ID,
            Password = Password
        };

        Send(message);
    }

    private void SendAlive()
    {
        var message = new MessageAlive()
        {
            Type = Types.Alive
        };

        Send(message);
    }

    private void Send(object message)
    {
        NextSend = Time.time + SendDelay;

        var json = JsonUtility.ToJson(message);

        var bytes = Encoding.ASCII.GetBytes(json);

        Connection.SendAsync(bytes, bytes.Length, IP);
    }
}

[Serializable]
public class Response
{
    public enum Types
    {
        Created,
        Listed,
        Requested,
        Taken,
        PasswordMismatch,
        Dead,
        Connection
    }

    public Types Type;
    public Lobby[] Listing;
    public string IP;
    public int Port;
    public int LocalPort;
}

[Serializable]
public class Lobby
{
    public int ID;
    public string Name;
    public int Players;
}

[Serializable]
public class MessageCreate
{
    public HUD.Types Type;
    public string Name;
    public string Password;
}

[Serializable]
public class MessageList
{
    public HUD.Types Type;
}

[Serializable]
public class MessageRequest
{
    public HUD.Types Type;
    public int ID;
    public string Password;
}

[Serializable]
public class MessageAlive
{
    public HUD.Types Type;
}