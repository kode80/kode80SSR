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
