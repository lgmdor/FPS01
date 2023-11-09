using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMouseInput : MonoBehaviour {
	[SerializeField] private float mouseSens = 0.8f;
	[SerializeField] private Camera playerCamera;

	private float verticalRotation = 0;

	private void Update() {
		Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSens;

		transform.Rotate(Vector3.up * mouseInput.x);

		verticalRotation -= mouseInput.y;
		verticalRotation = Mathf.Clamp(verticalRotation, -90, 90);

		playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
	}
}
