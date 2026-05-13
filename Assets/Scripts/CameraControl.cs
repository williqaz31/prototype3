using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float zoomSpeed = 5f;
    public float minSize = 2f;
    public float maxSize = 50f;
    public float dragSpeed = 2f;

    private Camera cam;
    private Vector3 dragOrigin;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleZoom();
        HandlePan();
    }

    private void HandleZoom()
    {
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Ajuste la taille orthographique
            var newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minSize, maxSize);
        }
    }

    private void HandlePan()
    {
        // Permet de déplacer la caméra avec le clic droit ou molette cliquée
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            var difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position += difference;
        }
    }
}