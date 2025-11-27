using System;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(MeshRenderer))]
public class VirtualMonitor : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoClip videoClip;       // Assign your imported VideoClip
    public Material monitorMaterial;  // Assign MonitorMaterial (Unlit/Texture, white)
    public string materialTextureName = "_MainTex"; // Usually "_MainTex"
    public float maxHeight = 0.6f;   // Maximum plane height in meters
    public float maxWidth = 1.2f;    // Maximum plane width in meters

    private VideoPlayer videoPlayer;

    void Start()
    {
        if (monitorMaterial == null || videoClip == null)
        {
            Debug.LogError("❌ Assign VideoClip and MonitorMaterial in Inspector!");
            return;
        }

        // Apply material to MeshRenderer
        var renderer = GetComponent<MeshRenderer>();
        renderer.material = monitorMaterial;
        monitorMaterial.color = Color.white;

        // Add VideoPlayer
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = videoClip;

        // Render directly to the material
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        videoPlayer.targetMaterialRenderer = renderer;
        videoPlayer.targetMaterialProperty = materialTextureName;

        // Prepare and play
        videoPlayer.prepareCompleted += (vp) =>
        {
            vp.Play();
            Debug.Log($"✅ Video playing: {vp.clip.name}, size: {vp.width}x{vp.height}");

            // Auto-scale plane to match video aspect ratio
            if (vp.width > 0 && vp.height > 0)
            {
                float videoAspect = (float)vp.width / vp.height;

                // Scale plane respecting max dimensions
                float height = Mathf.Min(maxHeight, maxWidth / videoAspect);
                float width = height * videoAspect;

                transform.localScale = new Vector3(width, height, 0.02f); // thin plane depth
                Debug.Log($"🔹 Plane auto-scaled to {width}m x {height}m");
            }
        };

        videoPlayer.errorReceived += (vp, msg) =>
        {
            Debug.LogError("❌ VideoPlayer error: " + msg);
        };

        videoPlayer.Prepare();

        // Face the plane toward the camera
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0f, 180f, 0f);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            Destroy(videoPlayer);
        }
    }

    public void PauseVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            Debug.Log("⏸ VirtualMonitor: Pausing Video");
            videoPlayer.Pause();
        }
    }

    public void PlayVideo()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            Debug.Log("▶ VirtualMonitor: Playing Video");
            videoPlayer.Play();
        }
    }
}
