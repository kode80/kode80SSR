using UnityEngine;
using System.Collections;

public class Water : MonoBehaviour 
{
	private Vector2 _uvOffset = Vector2.zero;
	private Renderer _renderer;

	void Start () 
	{
		_renderer = GetComponent<Renderer>();
	}

	void Update () 
	{
		_uvOffset += new Vector2( 0.051f, 0.091f) * Time.deltaTime;
		_renderer.materials[ 0].SetTextureOffset( "_MainTex", _uvOffset);
	}
}
