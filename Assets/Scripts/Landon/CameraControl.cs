using UnityEngine;
using UnityEngine.InputSystem;

using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(Rigidbody))]
public class CameraControl : MonoBehaviour {

	Transform cam;
	Controls.CameraMapActions input;

	float inputZoom = 0f;
	Vector2 inputCamRot = Vector2.zero;
	Vector2 inputSunRot = Vector2.zero;

	Vector2 zoomRange = new Vector2(-155f, -350f);
	Vector2 zoomSpeedRange = new Vector2(0.1f, 1f);
	float zoomPercent = 1f;
	float zoom = -280f;
	Vector2 zoomAngleRange = new Vector2(-50f, 0f);

	Vector3 eulerRotation = Vector3.zero;
	Vector2 rotationSpeedRange = new Vector2(1f,20f);
	float rotationDamping = 3f;

	bool correctNorth = false;

	public Rigidbody sun;

	void Start() {
		cam = Camera.main.transform;
		input = GM.controls.CameraMap;
	}
	
	// movement is done in FixedUpdate to match physics updates
	void FixedUpdate() {
		UpdateInputs();
		Zoom();
		Rotation();
		SunRotation();
		NorthUp();
	}

	void UpdateInputs() {
		inputZoom = input.Zoom.ReadValue<float>();
		// button
		inputCamRot = input.Rotate.ReadValue<Vector2>();
		// mouse
		if (Mouse.current.leftButton.IsPressed()) inputCamRot = -Mouse.current.delta.ReadValue() / 10f;
		inputSunRot = input.SunRotation.ReadValue<Vector2>();
		correctNorth = input.NorthUp.ReadValue<float>() > 0.1f;
	}
	void Zoom() {
		// zoom faster while further out
		float zoomSpeed = Mathf.Lerp(zoomSpeedRange.x, zoomSpeedRange.y, zoomPercent);
		// apply inputZoom and normalize to give a range
		zoomPercent = Mathf.Clamp01(zoomPercent + inputZoom * zoomSpeed * Time.fixedDeltaTime);
		//calculate zoomGoal based on zoomPercent between min and max zoom amount
		float zoomGoal = Mathf.Lerp(zoomRange.x, zoomRange.y, zoomPercent);
		// amount of damping to apply to zoom motion (higher number = more smoothing)
		float zoomDamping = 5f;
		// get a zoom between the previous value and zoomGoal based on damping
		zoom = Mathf.Lerp(zoom, zoomGoal, zoomDamping * Time.fixedDeltaTime);
		// apply zoom to camera position
		cam.localPosition = new Vector3(0, 0, zoom);
		// calculate the current actualZoomPercent based on zoom after damping
		float actualZoomPercent = (zoom - zoomRange.x) / (zoomRange.y - zoomRange.x);
		// rotate the camera based on actualZoomPercent
		RotateCamBasedOnZoom(actualZoomPercent);
	}
	void RotateCamBasedOnZoom(float zoomRange) {
		// larger = sharper curve (less angled camera until zoomPercent is close to 0)
		float rotSharpness = 5f;
		// gives a range between 0 and 1 to rotate the camera from a curve based on zoomRange and rotSharpness
		float anglePercent = (-1 / (1 + rotSharpness * zoomRange) + 1) * (rotSharpness + 1) / rotSharpness;
		// calculate angle based on anglePercent between min and max zoom angle
		float angle = Mathf.Lerp(zoomAngleRange.x, zoomAngleRange.y, anglePercent);
		// apply the angle as a rotation to the camera
		cam.localRotation = Quaternion.Euler(angle, 0f, 0f);
	}
	void Rotation() {
		// if there's no input, don't bother with calculations 
		if (inputCamRot == Vector2.zero) return;
		// raw rotation: relative to camera controller (this) based on input;
		Vector3 rot = (inputCamRot.y * transform.right - inputCamRot.x * transform.up);
		// ideal camera direction vector based on the plane of the rotation
		Vector3 idealForward = Vector3.ProjectOnPlane(transform.forward, rot);
		// correct rotation by adding change toward ideal
		rot += Vector3.Cross(transform.forward, idealForward);
		// scale rotation based on zoom: rotate faster when zoomed out
		rot *= Mathf.Lerp(rotationSpeedRange.x, rotationSpeedRange.y, zoomPercent);
		// apply the rotation as a torque
		GetComponent<Rigidbody>().AddTorque(rot);
	}
	void SunRotation() {
		// if there's no input, don't bother with calculations 
		if (inputSunRot == Vector2.zero) return;
		// raw rotation: relative to camera controller (this) based on input
		Vector3 sunRot = (inputSunRot.y * transform.right - inputSunRot.x * transform.up);
		// ideal sun direction vector based on the plane of the rotation
		Vector3 sunIdealForward = Vector3.ProjectOnPlane(sun.transform.forward, sunRot);
		// correct rotation by adding change toward ideal
		sunRot += Vector3.Cross(sun.transform.forward, sunIdealForward);
		// apply the rotation as a torque
		sun.AddTorque(sunRot);
	}
	void NorthUp() {
		// check for input
		if (!correctNorth) return;
		// earth's north (Vector3.up) relative to camera plane gives ideal camera controller rotation
		Vector3 idealNorth = Vector3.ProjectOnPlane(Vector3.up, transform.forward).normalized;
		// dot is 1 when parallel -1 when antiparallel.  this gives 0 to 2 for correction needed
		float dist = 1 - Vector3.Dot(transform.up, idealNorth);
		//don't rotate if camera controller north is already close enough to idealNorth
		if (dist < 0.1f) return;
		// direction towards idealNorth from camera controller
		Vector3 dir = Vector3.Cross(transform.up, idealNorth).normalized;
		// rotation vector: direction scaled based on distance
		Vector3 rot = dir * dist * 20f;
		// apply the rotation as a torque
		GetComponent<Rigidbody>().AddTorque(rot);
	}
}







/* example of option to use input as events instead of contantly checking for inpouts in update
 // in start:
	controls.CameraMap.Zoom.performed += ZoomPerformed;
 // calls this on event:
	void ZoomPerformed(InputAction.CallbackContext context) { Debug.Log(context.ReadValue<float>()); }
*/

/*
	float pinchZoom;
	void PinchZoom(Touch touch0, Touch touch1) {
		if (Touch.activeFingers.Count <= 1) return;

			//1
			if (firstTouch.phase == TouchPhase.Began ||
			  secondTouch.phase == TouchPhase.Began) {
				lastMultiTouchDistance = Vector2.Distance(firstTouch.screenPosition,
				  secondTouch.screenPosition);
			}
			//2
			if (firstTouch.phase != TouchPhase.Moved ||
			  secondTouch.phase != TouchPhase.Moved) {
				return;
			}
			//3
			float newMultiTouchDistance = Vector2.Distance(firstTouch.screenPosition,
			  secondTouch.screenPosition);
			//4
			CameraController.Instance?.Zoom(newMultiTouchDistance <
			  lastMultiTouchDistance);
			//5
			lastMultiTouchDistance = newMultiTouchDistance;
	}
*/
/* not force based
void Rotation() {
	float rotationSpeed = Mathf.Lerp(rotationSpeedRange.x, rotationSpeedRange.y, zoomPercent);
	eulerRotation.x += inputCamRot.y * rotationSpeed * Time.deltaTime;
	eulerRotation.y -= inputCamRot.x * rotationSpeed * Time.deltaTime;
	transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0f);
}
*/

/*  alternate method
	void SunRotation() {
		float inputX = sunRotationInput.x;// + sunJoystick.input.x;
		float inputY = sunRotationInput.y;// + sunJoystick.input.y;
		if (inputX == 0 && inputY == 0) return;
		// Get the world vector space for cameraController (this)
		Vector3 relativeUp = transform.TransformDirection(Vector3.up);
		Vector3 relativeRight = transform.TransformDirection(Vector3.right);
		// convert relative vectors from world to object local space
		Vector3 sunLocalUp = sun.transform.InverseTransformDirection(relativeUp);
		Vector3 sunLocalRight = sun.transform.InverseTransformDirection(relativeRight);
		Quaternion rotateBy = Quaternion.AngleAxis(-inputX / gameObject.transform.localScale.x, sunLocalUp)
			 * Quaternion.AngleAxis(inputY / gameObject.transform.localScale.x, sunLocalRight);
		sun.localRotation *= rotateBy;
	}
*/