using UnityEngine;

/// <summary>
/// Preview-only camera for Env scenes.
/// Auto-disables when loaded additively with MainScene (main camera already exists).
/// </summary>
public class PreviewCamera : MonoBehaviour
{
    private void Start()
    {
        Camera[] cameras = FindObjectsOfType<Camera>();
        if (cameras.Length > 1)
        {
            gameObject.SetActive(false);
        }
    }
}
