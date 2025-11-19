using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ARPlaneDragController : MonoBehaviour
{
    private Camera arCamera;
    private Vector3 offset;
    private bool dragging = false;

    void Start()
    {
        arCamera = Camera.main;
        if (arCamera == null)
            Debug.LogError("ARPlaneDragController: No main camera found!");
    }

    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        Ray ray = arCamera.ScreenPointToRay(touch.position);
        RaycastHit hit;

        switch (touch.phase)
        {
            case TouchPhase.Began:
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == transform)
                    {
                        dragging = true;
                        offset = transform.position - hit.point;
                    }
                }
                break;

            case TouchPhase.Moved:
                if (dragging)
                {
                    // Move along horizontal plane (Y = current object Y)
                    Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
                    if (groundPlane.Raycast(ray, out float enter))
                    {
                        Vector3 hitPoint = ray.GetPoint(enter);
                        transform.position = hitPoint + offset;
                    }
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                dragging = false;
                break;
        }
    }
}
