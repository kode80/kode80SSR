using UnityEngine;
using System.Collections;

public class FPSCounter : MonoBehaviour 
{
	[Range( 1.0f, 60.0f)]
	public int updatesPerSecond = 10;

	private int _tickCount;
	private float _deltaTimeAccumulator;
	private float _fps;

	void Start () 
	{
		_tickCount = 0;
		_deltaTimeAccumulator = 0.0f;
		_fps = 0.0f;
	}
	
	void Update () 
	{
		_tickCount++;
		_deltaTimeAccumulator += Time.deltaTime;

		if( _deltaTimeAccumulator > 1.0f / updatesPerSecond)
		{
			_fps = _tickCount / _deltaTimeAccumulator;
			_tickCount = 0;
			_deltaTimeAccumulator -= 1.0f / updatesPerSecond;
		}
	}

	void OnGUI()
	{
		GUI.Label( new Rect( 10.0f, 10.0f, 100.0f, 20.0f), "FPS: " + _fps);
	}
}
