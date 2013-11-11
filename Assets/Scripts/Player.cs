using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
	private const float c_speed = 5.0f;
	
	void Update ()
	{
		float deltaX = Input.GetAxis("Horizontal") * c_speed * Time.deltaTime;
		float deltaZ = Input.GetAxis("Vertical") * c_speed * Time.deltaTime;
		
		transform.position += new Vector3(deltaX, 0, deltaZ);
	}
}
