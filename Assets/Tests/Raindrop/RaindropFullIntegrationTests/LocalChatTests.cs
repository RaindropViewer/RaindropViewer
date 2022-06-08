using NUnit.Framework;
using Plugins.CommonDependencies;
using Raindrop.Netcom;
using UnityEngine.SceneManagement;

namespace Raindrop.Tests.RaindropFullIntegrationTests
{
    /*
     * UI-intensive tests for the chat functionality. the main scene will be loaded.
     * - chat window.
     * - self-disappearing local chat in main game screen. 
     */
    [TestFixture()]
    public partial class LocalChatTests
    {
        
        private RaindropNetcom netcom { get { return instance.Netcom; } }
        private RaindropInstance instance { get { return ServiceLocator.Instance.Get<RaindropInstance>(); } }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            //load the main scene.
            SceneManager.LoadScene("Tests/HeadlessBootstrapScene"); 
        }

    }

}
