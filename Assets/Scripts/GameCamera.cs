using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour
{
	public Transform m_player;
	
	void Update ()
	{
		transform.position = new Vector3(m_player.transform.position.x, m_player.transform.position.y + 1.0f, m_player.transform.position.z - 5.0f);
	}
}
