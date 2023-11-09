using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
	[SerializeField] private Camera playerCamera;

	private float range = 100f;
	private float shootTimer = 0f;

  private LayerMask hitLayer;
	private LineRenderer lineRenderer;
	
  private const float shootRate = 1f / 10f;
	
	private void Awake() {
		lineRenderer = GetComponent<LineRenderer>();

		lineRenderer.startWidth = 0.1f;
		lineRenderer.endWidth = 0.05f;
	}

	private void Update() {
		if(Input.GetButton("Fire1") && shootTimer >= shootRate) {
			Shoot();
		}
		shootTimer += Time.deltaTime;
	}

	private void Shoot() {
		Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, range)) {
			if(hit.transform.GetComponent<Enemy>() != null) {
				Debug.Log("hit enemy");
			}

			CreateBulletTracer(transform.position, hit.point);
		}

		shootTimer = 0f;
	}

	private void CreateBulletTracer(Vector3 start, Vector3 end) {
		lineRenderer.enabled = true;

    lineRenderer.SetPosition(0, start);
    lineRenderer.SetPosition(1, end);

		StartCoroutine(DisableBulletTracerAfterDuration());
    // You can also set other LineRenderer properties like color, width, and materials here.
	}

	IEnumerator DisableBulletTracerAfterDuration() {
    yield return new WaitForSeconds(0.01f);
    lineRenderer.enabled = false;
  }
}
