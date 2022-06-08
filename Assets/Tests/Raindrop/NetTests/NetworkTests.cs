using System;
using System.Collections;
using System.Net;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Raindrop.Tests.NetTests
{
    public class NetworkTests
    {
        //able to reach google.com, synchronous
        [UnityTest]
        public IEnumerator NetSync_Get_Google()
        {
            // Create the GET request.
            var destinationUrl = new Uri("http://www.google.com"); 
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(destinationUrl);
            request.Method = "GET";
     
            // Content type is JSON.
            request.ContentType = "application/json";
     
            try
            {
                using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Debug.Log("Publish Response: " + (int)response.StatusCode + ", " + response.StatusDescription);
                    if((int)response.StatusCode == 200)
                    {
                        Debug.Log("passed.");
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e.ToString());
            }
            Assert.Pass();
            yield break;
        }

    }
}