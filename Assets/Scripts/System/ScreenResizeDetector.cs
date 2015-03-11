using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]
public class ScreenResizeDetector : MonoBehaviour
{
	public delegate void ScreenDidResize();
	static public ScreenDidResize screenDidResizeDelegate;

	private int _screenWidth;
	private int _screenHeight;

	public void Awake()
	{
		_screenWidth = Screen.width;
		_screenHeight = Screen.height;
	}

	public void Update()
	{
		if( _screenWidth != Screen.width || _screenHeight != Screen.height)
		{
			_screenWidth = Screen.width;
			_screenHeight = Screen.height;

			if( screenDidResizeDelegate != null)
			{
				screenDidResizeDelegate();
			}
		}
	}
}
