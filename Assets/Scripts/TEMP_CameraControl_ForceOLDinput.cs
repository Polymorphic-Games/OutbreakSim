using UnityEngine;

public class TEMP_CameraControl_Force : MonoBehaviour {
	
	Transform cam;
	
	float zoomInput = 0f;
	Vector2 rotationInput = Vector2.zero;
	Vector2 sunRotationInput = Vector2.zero;

	Vector2 zoomRange = new Vector2(-155f, -350f);
	Vector2 zoomSpeedRange = new Vector2(0.1f, 1f);
	float zoomPercent = 1f;
	float zoom = -280f;
	float zoomDamping = 5f;
	Vector2 zoomAngleRange = new Vector2(-50f, 0f);
	float zoomAngleCurveSharpness = 5f;

	Vector3 eulerRotation = Vector3.zero;
	Vector2 rotationSpeedRange = new Vector2(200f,5000f);
	float rotationDamping = 3f;

	public Rigidbody sun;

	Vector2 lastMousePos;
	

	void Start() {
		cam = Camera.main.transform;
	}
	void Update() {
		UpdateInputs();

		Zoom();
		Rotation();
		//NorthUpAuto();
		NorthUp();
	}
	private void FixedUpdate() {
		SunRotation();
	}

	void UpdateInputs() {
		Vector2 mouseDelta = lastMousePos - (Vector2)Input.mousePosition;
		lastMousePos = Input.mousePosition;
		
		zoomInput = Input.mouseScrollDelta.y * 20f;
		zoomInput += Input.GetMouseButton(2) ? mouseDelta.y / 5f : 0;
		float KeyZoom = Input.GetKey(KeyCode.Z) ? -1f : Input.GetKey(KeyCode.X) ? 1f : 0f;
		zoomInput += KeyZoom;

		rotationInput = Input.GetMouseButton(0) ? mouseDelta : Vector2.zero;
		sunRotationInput = Input.GetMouseButton(1) ? mouseDelta / -5f : Vector2.zero;
	}
	void Zoom() {
		float zoomSpeed = Mathf.Lerp(zoomSpeedRange.x, zoomSpeedRange.y, zoomPercent);
		zoomPercent = Mathf.Clamp01(zoomPercent + zoomInput * zoomSpeed * Time.deltaTime);
		zoom = Mathf.Lerp(zoom, Mathf.Lerp(zoomRange.x, zoomRange.y, zoomPercent), zoomDamping * Time.deltaTime);
		cam.localPosition = new Vector3(0, 0, zoom);

		ZoomRotation();
	}
	void ZoomRotation() {
		float anglePercent = (-1 / (1 + zoomAngleCurveSharpness * zoomPercent) + 1) * (zoomAngleCurveSharpness + 1) / zoomAngleCurveSharpness;
		float angle = Mathf.Lerp(zoomAngleRange.x, zoomAngleRange.y, anglePercent);
		cam.localRotation = Quaternion.Euler(angle, 0f, 0f);
	}
	void Rotation() {
		float rotInputX = rotationInput.x;
		float rotInputY = rotationInput.y;
		if (rotInputX == 0 && rotInputY == 0) return;

		float rotationSpeed = Mathf.Lerp(rotationSpeedRange.x, rotationSpeedRange.y, zoomPercent);

		//raw rotation - rotation relative to camera controller (this) based on input;
		Vector3 rot = (rotInputY * transform.right - rotInputX * transform.up);
		//rotation correction - move towards the plane of rotation;
		Vector3 idealForward = Vector3.ProjectOnPlane(transform.forward, rot);
		rot += Vector3.Cross(transform.forward, idealForward);
		rot *= rotationSpeed;
		GetComponent<Rigidbody>().AddTorque(rot);
	}
	void SunRotation() {
		float sunInputX = sunRotationInput.x;
		float sunInputY = sunRotationInput.y;
		if (sunInputX == 0 && sunInputY == 0) return;
		
		//raw sun rotation - rotation relative to camera controller (this) based on input;
		Vector3 sunRot = (sunInputY * transform.right - sunInputX * transform.up);
		//correct sun roation - move the sun towards the plane of its rotation;
		Vector3 sunIdealForward = Vector3.ProjectOnPlane(sun.transform.forward, sunRot);
		sunRot += Vector3.Cross(sun.transform.forward, sunIdealForward);
		sun.AddTorque(sunRot);
	}
	void NorthUp() {
		if (!Input.GetMouseButton(0) || !Input.GetMouseButton(1)) return;
		
		Vector3 rot = Vector3.zero;
		Vector3 idealNorth = Vector3.ProjectOnPlane(Vector3.up, transform.forward);
		rot += Vector3.Cross(transform.up, idealNorth);
		rot *= 20000;
		GetComponent<Rigidbody>().AddTorque(rot);
	}
	void NorthUpAuto() {
		Vector3 rot = Vector3.zero;
		Vector3 idealNorth = Vector3.ProjectOnPlane(Vector3.up, transform.forward);
		rot += Vector3.Cross(transform.up, idealNorth);
		rot *= 1000;
		GetComponent<Rigidbody>().AddTorque(rot);
	}
}

/*	void Rotation() {
		// Euler fixed rotation
		float rotationSpeed = Mathf.Lerp(rotationSpeedRange.x, rotationSpeedRange.y, zoomPercent);
		eulerRotation.x += rotationInput.y * rotationSpeed * Time.deltaTime;
		eulerRotation.y -= rotationInput.x * rotationSpeed * Time.deltaTime;
		transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0f);
	}
*/