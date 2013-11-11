using UnityEngine;
using System.Collections;

public class PDA : MonoBehaviour
{
	void Start ()
	{
		PDANetwork.instance.joinRandomRoom();
	}
	
	void Update ()
	{
	
	}
}
