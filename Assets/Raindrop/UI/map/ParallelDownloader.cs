using OpenMetaverse.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Raindrop.Network
{
    // why is the code so repeated!!!!

    public class ParallelDownloader : IDisposable
    {
        Queue<QueuedItem> queue = new Queue<QueuedItem>();
        List<HttpWebRequest> activeDownloads = new List<HttpWebRequest>();

        public int ParallelDownloads { get; set; } = 15;

        public X509Certificate2 ClientCert { get; set; }

        public virtual void Dispose()
        {
            lock (activeDownloads)
            {
                foreach (var download in activeDownloads)
                {
                    try
                    {
                        download.Abort();
                    }
                    catch { }
                }
            }
        }

        protected virtual HttpWebRequest SetupRequest(Uri address, string acceptHeader)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
            request.Method = "GET";

            if (!string.IsNullOrEmpty(acceptHeader))
                request.Accept = acceptHeader;

            // Add the client certificate to the request if one was given
            if (ClientCert != null)
                request.ClientCertificates.Add(ClientCert);

            // Leave idle connections to this endpoint open for up to 60 seconds
            request.ServicePoint.MaxIdleTime = 0;
            // Disable stupid Expect-100: Continue header
            request.ServicePoint.Expect100Continue = false;
            // Crank up the max number of connections per endpoint (default is 2!)
            request.ServicePoint.ConnectionLimit = Math.Max(request.ServicePoint.ConnectionLimit, 128);
            // Caps requests are never sent as trickles of data, so Nagle's
            // coalescing algorithm won't help us
            request.ServicePoint.UseNagleAlgorithm = false;

            return request;
        }

        private void EnqueuePending()
        {
            lock (queue)
            {
                if (queue.Count > 0)
                {
                    int nr = 0;
                    lock (activeDownloads) nr = activeDownloads.Count;

                    for (int i = nr; i < ParallelDownloads && queue.Count > 0; i++)
                    {
                        QueuedItem item = queue.Dequeue();
                        Debug.Log("Requesting " + item.address);
                        HttpWebRequest req = SetupRequest(item.address, item.contentType);
                        CapsBase.DownloadDataAsync(
                            req,
                            item.millisecondsTimeout,
                            item.downloadProgressCallback,
                            (request, response, responseData, error) =>
                            {
                                lock (activeDownloads) activeDownloads.Remove(request);
                                item.completedCallback(request, response, responseData, error);
                                EnqueuePending();
                            }
                        );

                        lock (activeDownloads) activeDownloads.Add(req);
                    }
                }
            }
        }

        // Call this to request content which runs a desired callback on completion.
        public void QueueDownlad(Uri address, int millisecondsTimeout,
            string contentType,
            CapsBase.DownloadProgressEventHandler downloadProgressCallback,
            CapsBase.RequestCompletedEventHandler completedCallback)
        {
            lock (queue)
            {
                queue.Enqueue(new QueuedItem(
                    address,
                    millisecondsTimeout,
                    contentType,
                    downloadProgressCallback,
                    completedCallback
                    ));
            }
            EnqueuePending();
        }

        public class QueuedItem
        {
            public Uri address;
            public int millisecondsTimeout;
            public CapsBase.DownloadProgressEventHandler downloadProgressCallback;
            public CapsBase.RequestCompletedEventHandler completedCallback;
            public string contentType;

            public QueuedItem(Uri address, int millisecondsTimeout,
                string contentType,
                CapsBase.DownloadProgressEventHandler downloadProgressCallback,
                CapsBase.RequestCompletedEventHandler completedCallback)
            {
                this.address = address;
                this.millisecondsTimeout = millisecondsTimeout;
                this.downloadProgressCallback = downloadProgressCallback;
                this.completedCallback = completedCallback;
                this.contentType = contentType;
            }
        }

    }

}
