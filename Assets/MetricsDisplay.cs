using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetricsDisplay : MonoBehaviour {
	private float deltaTime = 0.0f;
	private (string fps, string movementSpeed, string movementState) text;

	private void Update() {
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}

	private void OnGUI() {
		int fps = Mathf.RoundToInt(1.0f / deltaTime);
		float movementSpeed = Mathf.RoundToInt(new Vector2(PlayerMovement.Instance.velocity.x, PlayerMovement.Instance.velocity.z).magnitude);

		text.fps = $"FPS: {fps}";
		text.movementSpeed = $"MovementSpeed: {movementSpeed} {PlayerMovement.Instance.velocity}";
		text.movementState = $"MovementState: {PlayerMovement.Instance.SM.currState.name}";

		GUIStyle style = new GUIStyle();

		style.fontSize = 24;
		style.normal.textColor = Color.white;

		GUI.Label(new Rect(10, 10, 100, 20), text.fps, style);
		GUI.Label(new Rect(10, 40, 100, 20), text.movementSpeed, style);
		GUI.Label(new Rect(10, 70, 100, 20), text.movementState, style);
	}
}
