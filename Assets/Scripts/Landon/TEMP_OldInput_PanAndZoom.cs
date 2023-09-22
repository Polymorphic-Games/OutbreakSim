//By Carson Rueber


using UnityEngine;

//Attach to a camera and let the panning and zooming wow you!
public class PanAndZoom : MonoBehaviour {
	private bool dragging = false;
	private Vector3 offset;
	//Drag start
	public void DragStart() {
		offset = getWorldMouseCoords();
		dragging = true;
	}

	//Drag end
	public void DragEnd() {
		dragging = false;
	}

	//Zoom
	public void OnScroll() {
		//Scale factor
		float y = Input.mouseScrollDelta.y;
		float factor = (0.05f * -y) + 1.0f;

		Vector2 worldPosBefore, worldPosAfter;

		//Scale
		worldPosBefore = getWorldMouseCoords();
		transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z * factor);
		worldPosAfter = getWorldMouseCoords();

		//Move according to the mosue position, keeping it at the same worldPosition as it was before
		Vector2 diff = worldPosAfter - worldPosBefore;
		transform.position = transform.position - (new Vector3(diff.x, diff.y));
	}

	//Update position if dragging
	public void Update() {

		if (Input.GetMouseButtonDown(2)) {
			DragStart();
		}
		if (Input.GetMouseButtonUp(2)) {
			DragEnd();
		}
		if (
			Input.mouseScrollDelta.y != 0 && //Mouse wheel scrolled
			!(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || //Mouse was on screen
			Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y)
		)
			OnScroll();

		//Update position if dragging
		if (dragging) {
			transform.position -= getWorldMouseCoords() - this.offset;
			offset = getWorldMouseCoords();
		}
	}

	Vector3 lastCoords = Vector3.zero;
	Vector3 getWorldMouseCoords() {
		RaycastHit hit;
		//Didn't hit anything, but why do we return last coords?
		if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
			return Vector3.zero;

		
		Renderer rend = hit.transform.GetComponent<Renderer>();
		MeshCollider meshCollider = hit.collider as MeshCollider;
		lastCoords = hit.point;
		return hit.point;
	}
}