using UnityEngine.SceneManagement;

namespace Raindrop.Bootstrap
{
    // call these methods to load the game.
    public static class RaindropLoader
    {
        public static void Load()
        {
            SceneManager.LoadScene(
                "Raindrop/Bootstrap/BootstrapScene",
                LoadSceneMode.Single);
        }

        public static void Unload()
        {
            // calling with LoadSceneMode.Single will call Destroy() on
            // RaindropBootstrapper, causing a cascade of teardown,
            // similar to what happens in the actual device
            var emptyScene = "Scenes/empty";
            SceneManager.LoadScene(emptyScene, LoadSceneMode.Single);
        }
    }
}