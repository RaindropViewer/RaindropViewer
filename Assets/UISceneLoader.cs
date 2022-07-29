using UnityEngine;
using UnityEngine.SceneManagement;

public class UISceneLoader : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene(
            "Raindrop/Bootstrap/MainScene",
            LoadSceneMode.Single);
    }
}
