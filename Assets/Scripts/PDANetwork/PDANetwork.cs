using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PDANetwork : MonoBehaviour
{
	#region CLASS MEMBERS
	
	//EVENTS
	public Action onCreatedRoom;
	public Action onServerDisconnected;
	public Action onConnectionFailed;
	public Action onJoinRoomFailed;
	
	public Action OnPDAConnected;
	public Action OnPDADisconnected;
	
	private string serverIpAddress = "";
	private int serverConnectionPort = 25001;
	
	private static PDANetwork _instance;
	
	private string serverName;
	
	private static PDAUDPClient _pdaUDPClient;
	private static PDADPServer _pdaUDPServer;
	
	private NetworkView _pdaNetworkView;
	
	private NetworkPlayer _pdaNetworkPlayer;
	
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
		//_customNetworkPlayer = new CustomNetworkPlayer(name, Network.player);
		
		if (onCreatedRoom != null)
		{
			onCreatedRoom();
		}
		
		//Add player in the clients
		//_customNetworkView.sendRPC("addPlayerLocal", CustomNetworkRPCMode.OthersBuffered, name, Network.player);
		
		//Set the server name
		//serverName = name;
		//Set server in the clients
		//_customNetworkView.sendRPC("setServerName", CustomNetworkRPCMode.OthersBuffered, name);
	}
	
	void OnPlayerConnected (NetworkPlayer player)
	{
		//Only the server can add new players
		if (isServer)
		{
			//CustomNetworkPlayer customNetworkPlayer = new CustomNetworkPlayer(name, player);
			
			if (OnPDAConnected != null)
			{
				OnPDAConnected();
			}
			
			//Add player in the clients
			//_networkView.sendRPC("addPlayerLocal", CustomNetworkRPCMode.OthersBuffered, name, player);
		}
	}
	
	void OnPlayerDisconnected (NetworkPlayer player)
	{
		if (OnPDADisconnected != null)
		{
			OnPDADisconnected();
		}
	}
	
	#endregion
	
	#region PROPERTIES
	
	public static PDANetwork instance
    {
        get
        {
			if (_instance == null)
			{
				//Find game object
				GameObject pdaNetwork = GameObject.Find("PDANetwork");
				
				if (pdaNetwork == null)
				{
					Debug.LogError("Couldn't find PDANetwork instance");				
				}
				
				//Get Custom Network component
				_instance = pdaNetwork.GetComponent<PDANetwork>();
				
				//Get UDP client and server components
				_pdaNetworkView = pdaNetwork.GetComponent<NetworkView>();
				_pdaUDPClient = pdaNetwork.GetComponent<PDAUDPClient>();
				_pdaUDPServer = pdaNetwork.GetComponent<PDADPServer>();
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
			return Network.isServer || Network.isClient;
        }
    }
	
	#endregion
	
	#region PUBLIC FUNCTIONS
	
	public void startSession()
	{
		//Initialize network server
		Network.InitializeServer(32, serverConnectionPort, false);
		
		//Start broadcasting UDP messages with the server IP
		_pdaUDPServer.init();
	}
	
	public void joinRandomRoom()
	{
		//Start listening for UDP messages with the server IP
		_pdaUDPClient.init();
		
		//Add listeners
		_pdaUDPClient.serverIpReceivedEvent += serverIpReceived;
	}
	
	public void endSession()
	{
		Network.Disconnect();
	}
	
	/// <summary>
	/// Closes the access to the current room, so no more players can connect to it
	/// </summary>
	public void closeCurrentRoom()
	{
		_pdaUDPServer.stop();
	}
	
	#endregion
	
	#region PRIVATE FUNCTIONS
	
	private void serverIpReceived(string ipAddress)
	{
		//Save server IP address
		serverIpAddress = ipAddress;
		
		//Remove listeners
		_pdaUDPClient.serverIpReceivedEvent -= serverIpReceived;
	}
	
	#endregion
	
	private void initPDA()
	{
		//_pdaNetworkView("InitPDA", _pdaNetworkPlayer);
	}
}