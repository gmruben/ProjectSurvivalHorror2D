using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class CustomUDPClient : MonoBehaviour
{
	private UdpClient client;
	private IPEndPoint localEp;
	
	public Action<string> serverIpReceivedEvent;
	
	public void init()
	{
		client = new UdpClient();
 
      	client.ExclusiveAddressUse = false;
        localEp = new IPEndPoint(IPAddress.Any, 2222);

        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        client.ExclusiveAddressUse = false;

        client.Client.Bind(localEp);

        IPAddress multicastaddress = IPAddress.Parse("239.0.0.222");
        client.JoinMulticastGroup(multicastaddress);
		
		client.BeginReceive(new AsyncCallback(receiveCallback), null);
	}
	
	public void receiveCallback(IAsyncResult ar)
	{
	  	Byte[] receiveBytes = client.EndReceive(ar, ref localEp);
	  	string serverIpAddress = Encoding.ASCII.GetString(receiveBytes);
		
		client.BeginReceive(new AsyncCallback(receiveCallback), null);
		 
		if (serverIpReceivedEvent != null)
		{
			serverIpReceivedEvent(serverIpAddress);
		}
	}
}