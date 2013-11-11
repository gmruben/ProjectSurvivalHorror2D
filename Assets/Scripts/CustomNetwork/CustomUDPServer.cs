using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
 
public class CustomUDPServer : MonoBehaviour
{
	private const float newMessageTime = 1;
	private float nextMessageTime = 0;
	
	private UdpClient udpclient;
	private IPEndPoint remoteep;
	
	private Byte[] buffer;
	
	private bool isInit = false;
	
	void Update()
	{
		if (isInit && Time.time > nextMessageTime)
		{
			sendUDPMessage(Network.player.ipAddress.ToString());
			
			nextMessageTime = Time.time + newMessageTime;
		}
	}
	
	public void init()
	{
	   	udpclient = new UdpClient();
 
        IPAddress multicastaddress = IPAddress.Parse("239.0.0.222"); 
        udpclient.JoinMulticastGroup(multicastaddress);
    	remoteep = new IPEndPoint(multicastaddress, 2222);
 
    	buffer = null;
		isInit = true;
	}
	
	/// <summary>
	/// Stop sending messages
	/// </summary>
	public void stop()
	{
		isInit = false;
	}
	
	private void sendUDPMessage(string message)
	{
		buffer = Encoding.ASCII.GetBytes(message);
     	udpclient.Send(buffer, buffer.Length, remoteep);
	}
}