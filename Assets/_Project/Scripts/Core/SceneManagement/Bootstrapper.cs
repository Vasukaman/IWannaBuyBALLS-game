// Filename: Bootstrapper.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private string _sceneToLoad = "GameScene"; // Or whatever your scene is named

    private void Awake()
    {
        // This is the crucial line. It tells Unity to not destroy the GameObject
        // this script is attached to (our [ProjectContext]) when a new scene loads.
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        // Load the main game scene after this bootstrap scene has initialized.
        SceneManager.LoadScene(_sceneToLoad, LoadSceneMode.Single);
    }
}   