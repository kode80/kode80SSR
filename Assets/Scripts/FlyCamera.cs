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

public class FlyCamera : MonoBehaviour {
	
	public float lookSpeed = 15.0f;
	public float moveSpeed = 15.0f;
	
	private float _rotationX = 0.0f;
	private float _rotationY = 0.0f;
	private Vector3 _targetPosition;

	void Start () 
	{
		_targetPosition = transform.position;
		_rotationX = transform.localEulerAngles.y;
		_rotationY = transform.localEulerAngles.x;
	}
	
	void Update ()
	{
		if( Input.GetMouseButton( 0))
		{
			_rotationX += Input.GetAxis("Mouse X") * Time.deltaTime * lookSpeed;
			_rotationY += Input.GetAxis("Mouse Y") * Time.deltaTime * lookSpeed;
			_rotationY = Mathf.Clamp( _rotationY, -90, 90);
			
			transform.localRotation = Quaternion.AngleAxis( _rotationX, Vector3.up);
			transform.localRotation *= Quaternion.AngleAxis( _rotationY, Vector3.left);
			
			_targetPosition += transform.forward * moveSpeed * Time.deltaTime * Input.GetAxis("Vertical");
			_targetPosition += transform.right * moveSpeed * Time.deltaTime * Input.GetAxis("Horizontal");

			_targetPosition += Vector3.up * moveSpeed * Time.deltaTime * (Input.GetButton( "Up") ? 1.0f : 0.0f);
			_targetPosition -= Vector3.up * moveSpeed * Time.deltaTime * (Input.GetButton( "Down") ? 1.0f : 0.0f);
		}

		transform.position = Vector3.Lerp( transform.position, _targetPosition, 0.5f); 
	}
}
