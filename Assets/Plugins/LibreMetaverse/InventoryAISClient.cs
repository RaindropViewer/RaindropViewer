using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace LibreMetaverse
{
    public class InventoryAISClient
    {
        public const string INVENTORY_CAP_NAME = "InventoryAPIv3";
        public const string LIBRARY_CAP_NAME = "LibraryAPIv3";

        [NonSerialized]
        private readonly GridClient Client;

        private static readonly HttpClient httpClient = new HttpClient();

        public InventoryAISClient(GridClient client)
        {
            Client = client;
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "LibreMetaverse AIS Client");
        }

        public bool IsAvailable => (Client.Network.CurrentSim.Caps != null &&
                                    Client.Network.CurrentSim.Caps.CapabilityURI(INVENTORY_CAP_NAME) != null);

        public async Task CreateInventory(UUID parentUuid, OSD newInventory, bool createLink, InventoryManager.ItemCreatedCallback callback)
        {
            var cap = getInventoryCap();
            if (cap == null)
            {
                Logger.Log("No AIS3 Capability!", Helpers.LogLevel.Warning, Client);
                return;
            }

            var success = false;
            InventoryItem item = null;

            try
            {
                var tid = UUID.Random();

                var url = $"{cap}/category/{parentUuid}?tid={tid}";

                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    success = false;

                    return;
                }

                var payload = OSDParser.SerializeLLSDXmlString(newInventory);

                using (var content = new StringContent(payload, Encoding.UTF8, "application/llsd+xml"))
                {
                    using (var reply = await httpClient.PostAsync(uri, content))
                    {
                        success = reply.IsSuccessStatusCode;

                        if (!success)
                        {
                            Logger.Log("Could not create inventory: " + reply.ReasonPhrase, Helpers.LogLevel.Warning);

                            return;
                        }

                        var data = await reply.Content.ReadAsStringAsync();

                        if (OSDParser.Deserialize(data) is OSDMap map && map["_embedded"] is OSDMap embedded)
                        {
                            var items = !createLink ?
                                parseItemsFromResponse((OSDMap)embedded["items"]) :
                                parseLinksFromResponse((OSDMap)embedded["links"]);

                            item = items.First();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Warning);
            }
            finally
            {
                callback(success, item);
            }
        }

        public async Task SlamFolder(UUID folderUuid, OSD newInventory, Action<bool> callback)
        {
            var cap = getInventoryCap();
            if (cap == null)
            {
                Logger.Log("No AIS3 Capability!", Helpers.LogLevel.Warning, Client);
                return;
            }

            var success = false;

            try
            {
                var tid = UUID.Random();
                var url = $"{cap}/category/{folderUuid}/links?tid={tid}";

                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    success = false;

                    return;
                }

                var payload = OSDParser.SerializeLLSDXmlString(newInventory);

                using (var content = new StringContent(payload, Encoding.UTF8, "application/llsd+xml"))
                {
                    using (var reply = await httpClient.PutAsync(uri, content))
                    {
                        success = reply.IsSuccessStatusCode;

                        if (!success)
                        {
                            Logger.Log("Could not slam folder: " + reply.ReasonPhrase, Helpers.LogLevel.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Warning);
            }
            finally
            {
                callback?.Invoke(success);
            }
        }

        public async Task RemoveCategory(UUID categoryUuid, Action<bool> callback)
        {
            var cap = getInventoryCap();
            if (cap == null)
            {
                Logger.Log("No AIS3 Capability!", Helpers.LogLevel.Warning, Client);
                return;
            }

            var success = false;

            try
            {
                var url = $"{cap}/category/{categoryUuid}";

                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    success = false;

                    return;
                }

                using (var reply = await httpClient.DeleteAsync(uri))
                {
                    success = reply.IsSuccessStatusCode;

                    if (!success)
                    {
                        Logger.Log("Could not remove folder: " + reply.ReasonPhrase, Helpers.LogLevel.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Warning);
            }
            finally
            {
                callback?.Invoke(success);
            }
        }

        public async Task RemoveItem(UUID itemUuid, Action<bool> callback)
        {
            var cap = getInventoryCap();
            if (cap == null)
            {
                Logger.Log("No AIS3 Capability!", Helpers.LogLevel.Warning, Client);
                return;
            }

            var success = false;

            try
            {
                var url = $"{cap}/item/{itemUuid}";

                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    success = false;

                    return;
                }

                using (var reply = await httpClient.DeleteAsync(uri))
                {
                    success = reply.IsSuccessStatusCode;

                    if (!success)
                    {
                        Logger.Log("Could not remove item: " + itemUuid + " " + reply.ReasonPhrase,
                            Helpers.LogLevel.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Warning);
            }
            finally
            {
                callback?.Invoke(success);
            }
        }

        public async Task CopyLibraryCategory(UUID sourceUuid, UUID destUuid, bool copySubfolders, Action<bool> callback)
        {
            var cap = getLibraryCap();
            if (cap == null)
            {
                Logger.Log("No AIS3 Capability!", Helpers.LogLevel.Warning, Client);
                return;
            }

            var success = false;

            try
            {
                UUID tid = UUID.Random();

                var url = new StringBuilder($"{cap}/category/{sourceUuid}?tid={tid}");

                if (copySubfolders)
                    url.Append(",depth=0");

                if (!Uri.TryCreate(url.ToString(), UriKind.Absolute, out var uri))
                {
                    success = false;

                    return;
                }

                using (var message = new HttpRequestMessage())
                {
                    message.RequestUri = uri;
                    message.Method = new HttpMethod("COPY");
                    message.Headers.Add("Destination", destUuid.ToString());

                    using (var reply = await httpClient.SendAsync(message))
                    {
                        success = reply.IsSuccessStatusCode;

                        if (!success)
                        {
                            Logger.Log("Could not copy library folder: " + reply.ReasonPhrase, Helpers.LogLevel.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Warning);
            }
            finally
            {
                callback?.Invoke(success);
            }
        }

        public async Task PurgeDescendents(UUID categoryUuid, Action<bool, UUID> callback)
        {
            var cap = getInventoryCap();
            if (cap == null)
            {
                Logger.Log("No AIS3 Capability!", Helpers.LogLevel.Warning, Client);
                return;
            }

            var success = false;

            try
            {
                var url = $"{cap}/category/{categoryUuid}/children";

                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    success = false;

                    return;
                }

                using (var reply = await httpClient.DeleteAsync(uri))
                {
                    success = reply.IsSuccessStatusCode;

                    if (!success)
                    {
                        Logger.Log("Could not purge descendents: " + reply.ReasonPhrase, Helpers.LogLevel.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Warning);
            }
            finally
            {
                callback?.Invoke(success, categoryUuid);
            }
        }

        public async Task UpdateCategory(UUID categoryUuid, OSD updates, Action<bool> callback)
        {
            var cap = getInventoryCap();
            if (cap == null)
            {
                Logger.Log("No AIS3 Capability!", Helpers.LogLevel.Warning, Client);
                return;
            }

            var success = false;

            try
            {
                var url = $"{cap}/category/{categoryUuid}";

                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    success = false;

                    return;
                }

                var method = new HttpMethod("PATCH");

                using (var message = new HttpRequestMessage(method, uri))
                {
                    var payload = OSDParser.SerializeLLSDXmlString(updates);

                    using (var content = new StringContent(payload, Encoding.UTF8, "application/llsd+xml"))
                    {
                        message.Content = content;

                        using (var reply = await httpClient.SendAsync(message))
                        {
                            success = reply.IsSuccessStatusCode;

                            if (!success)
                            {
                                Logger.Log("Could not update folder: " + reply.ReasonPhrase,
                                    Helpers.LogLevel.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Warning);
            }
            finally
            {
                callback?.Invoke(success);
            }
        }

        public async Task UpdateItem(UUID itemUuid, OSD updates, Action<bool> callback)
        {
            var cap = getInventoryCap();
            if (cap == null)
            {
                Logger.Log("No AIS3 Capability!", Helpers.LogLevel.Warning, Client);
                return;
            }

            var success = false;

            try
            {
                var url = $"{cap}/item/{itemUuid}";

                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    success = false;

                    return;
                }

                var method = new HttpMethod("PATCH");

                using (var message = new HttpRequestMessage(method, uri))
                {
                    var payload = OSDParser.SerializeLLSDXmlString(updates);

                    using (var content = new StringContent(payload, Encoding.UTF8, "application/llsd+xml"))
                    {
                        message.Content = content;

                        using (var reply = await httpClient.SendAsync(message))
                        {
                            success = reply.IsSuccessStatusCode;

                            if (!success)
                            {
                                Logger.Log("Could not update item: " + reply.ReasonPhrase,
                                    Helpers.LogLevel.Warning);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Warning);
            }
            finally
            {
                callback?.Invoke(success);
            }
        }

        private List<InventoryItem> parseItemsFromResponse(OSDMap itemsOsd)
        {
            List<InventoryItem> ret = new List<InventoryItem>();

            foreach (KeyValuePair<string, OSD> o in itemsOsd)
            {
                var item = (OSDMap)o.Value;
                ret.Add(InventoryItem.FromOSD(item));
            }
            return ret;
        }

        private List<InventoryItem> parseLinksFromResponse(OSDMap linksOsd)
        {
            List<InventoryItem> ret = new List<InventoryItem>();

            foreach (KeyValuePair<string, OSD> o in linksOsd)
            {
                var link = (OSDMap)o.Value;
                InventoryType type = (InventoryType)link["inv_type"].AsInteger();
                if (type == InventoryType.Texture && (AssetType)link["type"].AsInteger() == AssetType.Object)
                {
                    type = InventoryType.Attachment;
                }
                InventoryItem item = InventoryManager.CreateInventoryItem(type, link["item_id"]);

                item.ParentUUID = link["parent_id"];
                item.Name = link["name"];
                item.Description = link["desc"];
                item.OwnerID = link["agent_id"];
                item.ParentUUID = link["parent_id"];
                item.AssetUUID = link["linked_id"];
                item.AssetType = AssetType.Link;
                item.CreationDate = Utils.UnixTimeToDateTime(link["created_at"]);

                item.CreatorID = link["agent_id"]; // hack
                item.LastOwnerID = link["agent_id"]; // hack
                item.Permissions = Permissions.NoPermissions;
                item.GroupOwned = false;
                item.GroupID = UUID.Zero;

                item.SalePrice = 0;
                item.SaleType = SaleType.Not;

                ret.Add(item);
            }

            return ret;
        }

        private Uri getInventoryCap()
        {
            Uri cap = null;
            if (Client.Network.CurrentSim.Caps != null)
            {
                cap = Client.Network.CurrentSim.Caps.CapabilityURI(INVENTORY_CAP_NAME);
            }
            return cap;
        }

        private Uri getLibraryCap()
        {
            Uri cap = null;
            if (Client.Network.CurrentSim.Caps != null)
            {
                cap = Client.Network.CurrentSim.Caps.CapabilityURI(LIBRARY_CAP_NAME);
            }
            return cap;
        }
    }
}
