using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CustomNetwork : MonoBehaviour
{
	#region CLASS MEMBERS
	
	//EVENTS
	public Action onCreatedRoom;
	public Action onServerDisconnected;
	public Action onConnectionFailed;
	public Action onJoinRoomFailed;
	
	private string serverIpAddress = "";
	private int serverConnectionPort = 25001;
	
	private static CustomNetwork _instance;
	
	private string serverName;
	
	private static CustomUDPClient _customUDPClient;
	private static CustomUDPServer _customUDPServer;
	
	private static CustomNetworkView _customNetworkView;
	
	private static List<CustomNetworkPlayer> _playerList = new List<CustomNetworkPlayer>();
	
	#endregion
	
	#region UNITY BUILT-IN FUNCTIONS
	
	void Update()
	{
		//Check if we have already the server IP address (it is on Update because we can't connect from a different thread)
		if (serverIpAddress != "")
		{
			Network.Connect(serverIpAddress, serverConnectionPort);
			serverIpAddress = "";
		}
	}
	
	#endregion
	
	#region UNITY CALLBACKS
	
	void OnServerInitialized()
	{
		string name = "Player" + UnityEngine.Random.Range(0, 100).ToString();
		_customNetworkPlayer = new CustomNetworkPlayer(name, Network.player);
		
		_playerList.Add(_customNetworkPlayer);
		
		if (onCreatedRoom != null)
		{
			onCreatedRoom();
		}
		
		//Add player in the clients
		_customNetworkView.sendRPC("addPlayerLocal", CustomNetworkRPCMode.OthersBuffered, name, Network.player);
		
		//Set the server name
		serverName = name;
		//Set server in the clients
		_customNetworkView.sendRPC("setServerName", CustomNetworkRPCMode.OthersBuffered, name);
	}
	
	void OnPlayerConnected (NetworkPlayer player)
	{
		//Only the server can add new players
		if (isServer)
		{
			string name = "Player" + UnityEngine.Random.Range(0, 100).ToString();
			CustomNetworkPlayer customNetworkPlayer = new CustomNetworkPlayer(name, player);
			
			_playerList.Add(customNetworkPlayer);
			
			if (onCustomPlayerConnected != null)
			{
				onCustomPlayerConnected(customNetworkPlayer);
			}
			
			//Add player in the clients
			_customNetworkView.sendRPC("addPlayerLocal", CustomNetworkRPCMode.OthersBuffered, name, player);
		}
	}
	
	void OnPlayerDisconnected (NetworkPlayer player)
	{
		CustomNetworkPlayer customNetworkPlayer = getCustomNetworkPlayerByNetworkPlayer(player);
		_playerList.Remove(customNetworkPlayer);
		
		if (onCustomPlayerDisconnected != null)
		{
			onCustomPlayerDisconnected(customNetworkPlayer);
		}
		
		//Remove player in the clients
		_customNetworkView.sendRPC("removePlayer", CustomNetworkRPCMode.OthersBuffered, customNetworkPlayer.name);
	}
	
	#endregion
	
	#region PROPERTIES
	
	public static CustomNetwork instance
    {
        get
        {
			if (_instance == null)
			{
				//Find game object
				GameObject customNetwork = GameObject.Find("CustomNetwork");
				
				if (customNetwork == null)
				{
					Debug.LogError("Couldn't find CustomNetwork instance");				
				}
				
				//Get Custom Network component
				_instance = customNetwork.GetComponent<CustomNetwork>();
				
				//Get UDP client and server components
				_customUDPClient = customNetwork.GetComponent<CustomUDPClient>();
				_customUDPServer = customNetwork.GetComponent<CustomUDPServer>();
			}
            
			return _instance;
        }
    }
	
	public static double time
    {
        get
        {
			return Network.time;
        }
    }
	
	public static bool isServer
    {
        get
        {
			return Network.isServer;
        }
    }
	
	public static bool inRoom
    {
        get
        {
			if (isLocalNetwork)
			{
				return Network.isServer || Network.isClient;
			}
			else
			{
				return PhotonNetwork.room != null;
			}
        }
    }
	
	public static CustomConnectionState connectionState
    {
        get
        {
			if (isLocalNetwork)
			{
				return getNetworkConnectionState();
			}
			else
			{
				return getPhotonNetworkConnectionState();
			}
        }
    }
	
	public static CustomNetworkPlayer player
    {
        get
        {
			return _customNetworkPlayer;
        }
    }
	
	#endregion
	
	#region PUBLIC FUNCTIONS
	
	public void connect()
	{
		if (!isLocalNetwork)
		{
			PhotonNetwork.ConnectUsingSettings("v1.0");
		}
	}
	
	public void disconnect()
	{
		if (isLocalNetwork)
		{
			Network.Disconnect();
		}
		else
		{
			PhotonNetwork.Disconnect();
		}
	}
	
	public void initServer()
	{
		//Initialize network server
		Network.InitializeServer(32, serverConnectionPort, false);
		
		//Start broadcasting UDP messages with the server IP
		_customUDPServer.init();
	}
	
	public void joinRandomRoom()
	{
		//Start listening for UDP messages with the server IP
		_customUDPClient.init();
		
		//Add listeners
		_customUDPClient.serverIpReceivedEvent += serverIpReceived;
	}
	
	public void leaveCurrentRoom()
	{
		Network.Disconnect();
	}
	
	/// <summary>
	/// Closes the access to the current room, so no more players can connect to it
	/// </summary>
	public void closeCurrentRoom()
	{
		_customUDPServer.stop();
	}
	
	#endregion
	
	#region PRIVATE FUNCTIONS
	
	private static CustomNetworkPlayer getCustomNetworkPlayerByPhotonPlayer(PhotonPlayer photonPlayer)
	{
		foreach(CustomNetworkPlayer player in _playerList)
		{
			if (player.photonPlayer == photonPlayer)
			{
				return player;
			}
		}
		
		return null;
	}
	
	private static CustomNetworkPlayer getCustomNetworkPlayerByNetworkPlayer(NetworkPlayer networkPlayer)
	{
		foreach(CustomNetworkPlayer player in _playerList)
		{
			if (player.networkPlayer == networkPlayer)
			{
				return player;
			}
		}
		
		return null;
	}
	
	private static CustomConnectionState getNetworkConnectionState()
	{
		return CustomConnectionState.Connected;
	}
	
	private static CustomConnectionState getPhotonNetworkConnectionState()
	{
		switch (PhotonNetwork.connectionState)
		{
			case ConnectionState.Connecting:
				return CustomConnectionState.Connecting;
			case ConnectionState.Connected:
				return CustomConnectionState.Connected;
			case ConnectionState.Disconnected:
				return CustomConnectionState.Disconnected;
		}
		
		return CustomConnectionState.Disconnected;
	}
	
	private void serverIpReceived(string ipAddress)
	{
		//Save server IP address
		serverIpAddress = ipAddress;
		
		//Remove listeners
		_customUDPClient.serverIpReceivedEvent -= serverIpReceived;
	}
	
	#endregion
	
	#region RPC FUNCTIONS
	
	/// <summary>
	/// Adds a player connected with a local connection
	/// </summary>
	/// <param name='playerName'>
	/// Player name
	/// </param>
	/// <param name='player'>
	/// Network player
	/// </param>
	[RPC]
	void addPlayerLocal(string playerName, NetworkPlayer player)
    {
        CustomNetworkPlayer customNetworkPlayer = new CustomNetworkPlayer(playerName, player);
			
		_playerList.Add(customNetworkPlayer);
		
		//If we are the player added, save the reference
		if (Network.player == player)
		{
			_customNetworkPlayer = customNetworkPlayer;
		}
    }
	
	/// <summary>
	/// Removes a player disconnected from the room
	/// </summary>
	/// <param name='playerName'>
	/// Player name
	[RPC]
	void removePlayer(string playerName)
    {
        CustomNetworkPlayer customNetworkPlayer = getCustomNetworkPlayerByName(playerName);
		_playerList.Remove(customNetworkPlayer);
    }
	
	[RPC]
	void setServerName(string serverName)
	{
		this.serverName = serverName;
	}
	
	#endregion
}