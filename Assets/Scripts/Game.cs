using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour
{
	void Start ()
	{
		PDANetwork.instance.startSession();
		
		//Get network view
		//customNetworkView = GetComponent<CustomNetworkView>();
		
		//Add listeners
		PDANetwork.instance.OnPDAConnected += OnPDAConnected;
	}
	
	private void OnPDAConnected()
	{
		//PDANetwork.instance.initPDA();
	}
}
