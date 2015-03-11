using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour 
{
	public float rotateY;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate( Vector3.up, rotateY * Time.deltaTime);
	}
}
