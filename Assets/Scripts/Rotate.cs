using UnityEngine;

public class RotateViewTitleScreen : MonoBehaviour {

    public float speed = 1f;
    public bool z = false;

    void Update() {
        if(z)
            transform.rotation *= Quaternion.Euler(Vector3.forward * speed * Time.deltaTime);
        else
            transform.rotation *= Quaternion.Euler(Vector3.up * speed * Time.deltaTime);
    }
}