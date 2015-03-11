//  Copyright (c) 2015, Ben Hopkins (kode80)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
//  
//  1. Redistributions of source code must retain the above copyright notice, 
//     this list of conditions and the following disclaimer.
//  
//  2. Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//  
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
//  MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
//  THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT 
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
//  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
//  EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


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
