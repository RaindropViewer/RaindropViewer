using System.Collections;
using NUnit.Framework;
using Raindrop;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Raindrop.RaindropIntegrationTests.ChatTests
{
    [TestFixture()]
    public class LocalChatWindowTests : MonoBehaviour
    {
        [UnityTest]
        //test the local chat UI, send and receive functions.
        public IEnumerator AbleOpenLocalChatUI()
        {
            Debug.Log("test is not implemented.");
            // SceneManager.LoadScene("LocalChatWindowScene");
            //
            yield break;
        }
    }
}
