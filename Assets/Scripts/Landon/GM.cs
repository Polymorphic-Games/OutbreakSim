using UnityEngine;

public class GM : MonoBehaviour {

	public static Controls controls;

	void Awake() {
		controls = new Controls();
		controls.CameraMap.Enable();
	}
}