using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Map : MonoBehaviour
{
	private const float c_tileSize = 0.1f;
	
	public GameObject m_mapTilePrefab;
	
	public GameObject m_mapPlayerPrefab;
	public List<GameObject> m_roomList;
	
	public Transform m_player;
	private GameObject m_mapPlayer;
	
	void Awake()
	{
		for (int i = 0; i < m_roomList.Count; i++)
		{
			GameObject room = m_roomList[i];
			
			createRoom((int) room.collider.bounds.center.x, (int) room.collider.bounds.center.z, (int) room.collider.bounds.size.x, (int) room.collider.bounds.size.z);
		}
		
		//Instantiate player
		m_mapPlayer = GameObject.Instantiate(m_mapPlayerPrefab) as GameObject;
	}
	
	public void createRoom(int posX, int posZ, int sizeX, int sizeZ)
	{
		float startX = (posX - (sizeX / 2)) * c_tileSize;
		float startZ = (posZ - (sizeZ / 2)) * c_tileSize;
		
		for (int x = 0; x < sizeX; x++)
		{
			for (int z = 0; z < sizeZ; z++)
			{
				GameObject tile = GameObject.Instantiate(m_mapTilePrefab) as GameObject;
				tile.transform.position = new Vector3(startX + (x * c_tileSize), 0, startZ + (z * c_tileSize));
			}
		}
	}
	
	void Update()
	{
		float playerPosX = Mathf.FloorToInt(m_player.transform.position.x) * c_tileSize;
		float playerPosZ = Mathf.FloorToInt(m_player.transform.position.z) * c_tileSize;
		
		m_mapPlayer.transform.position = new Vector3(playerPosX, 0, playerPosZ);	
	}
}
