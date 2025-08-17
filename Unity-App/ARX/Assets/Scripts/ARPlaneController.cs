using UnityEngine;

public class ARPlaneController : MonoBehaviour
{
    public GameObject planePrefab;
    private GameObject currentPlane;

    public void SpawnOrMovePlane(Vector3 position)
    {
        if (currentPlane != null)
        {
            Destroy(currentPlane);
        }
        currentPlane = Instantiate(planePrefab, position, Quaternion.identity);
        currentPlane.SetActive(true);
    }

    public void HidePlane()
    {
        if (currentPlane != null)
        {
            Destroy(currentPlane);
            currentPlane = null;
        }
    }
}
