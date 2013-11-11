using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour
{
	public Map m_map;
	
	void Start ()
	{
		CustomNetwork.instance.initServer();
	}
}
