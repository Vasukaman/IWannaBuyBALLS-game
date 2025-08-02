// Filename: EditorBootstrapLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// An editor-only helper that ensures the Bootstrap scene is loaded when
/// starting from a different scene, allowing services to be injected.
/// This script is automatically stripped from final builds.
/// </summary>
public class EditorBootstrapLoader : MonoBehaviour
{
    public static bool IsBootstrapInitiated = false;

    [SerializeField] private string _bootstrapSceneName = "Bootstrap";

#if UNITY_EDITOR
    private void Awake()
    {
        // If the bootstrap scene is NOT loaded, it means we started from this scene.
        if (!SceneManager.GetSceneByName(_bootstrapSceneName).isLoaded && !IsBootstrapInitiated)
        {
            // 1. Set the flag to true. This is our "secret message".
            IsBootstrapInitiated = true;

            // 2. Load the bootstrap scene additively.
            Debug.Log($"<color=yellow>EditorBootstrapLoader:</color> Bootstrap scene not found. Loading it additively.");
            SceneManager.LoadScene(_bootstrapSceneName, LoadSceneMode.Single);
        }
    }
#endif
}