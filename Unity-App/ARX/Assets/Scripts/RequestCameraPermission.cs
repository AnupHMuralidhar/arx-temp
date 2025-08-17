using UnityEngine;
using UnityEngine.Android;

public class RequestCameraPermission : MonoBehaviour
{
    void Start()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
#endif
    }
}
