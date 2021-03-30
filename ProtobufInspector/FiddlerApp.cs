using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Fiddler;

namespace Google.Protobuf.FiddlerInspector
{
    internal class FiddlerApp
    {
        public static void LogString(string log)
        {
            FiddlerApplication.Log.LogString(log);
        }

        public static bool IsProtobufPacket(HTTPHeaders headers)
        {
            if (null == headers)
            {
                return false;
            }
            
            return null != headers && (headers.ExistsAndContains("Content-Type", "application/x-protobuf") || headers.ExistsAndContains("Content-Type", "application/x-google-protobuf"));
        }
        
        public static byte[] DecodeContent(byte[] body, Fiddler.HTTPHeaders headers)
        {
            if (headers.Exists("Content-Encoding"))
            {
                List<HTTPHeaderItem> headerItems = headers.FindAll("Content-Encoding");
                string encoding = null;
                if (headerItems != null)
                {
                    for (int idx = 0; idx < headerItems.Count; idx++)
                    {
                        encoding = headerItems[idx].Value;
                    }
                }

                if (encoding == null) encoding = "";

                if (encoding.Equals("gzip"))
                {
                    body = Fiddler.Utilities.GzipExpand(body);
                }
                else if (encoding.Equals("deflate"))
                {
                    body = Fiddler.Utilities.DeflaterExpand(body);
                }
                else if (encoding.Equals("br"))
                {
                    body = Fiddler.Utilities.BrotliExpand(body);
                }
                else if (encoding.Equals("identity"))
                {
                    // body = session.ResponseBody;
                }
            }

            return body;
        }

        // Use the same rule like Charles to parse message type and decriptorset url
        // Refer to: https://www.charlesproxy.com/documentation/using-charles/protocol-buffers/
        public static bool ParseMessageTypeNameAndDescriptorSetUrl(HTTPHeaders headers, out string messageTypeName, out string descriptorSetUrl)
        {
            messageTypeName = "";
            descriptorSetUrl = "";

            if (null == headers || !headers.Exists("Content-Type"))
            {
                return false;
            }

            messageTypeName = headers.GetTokenValue("Content-Type", "messageType");
            if (null == messageTypeName)
            {
                messageTypeName = headers.GetTokenValue("Content-Type", "MessageType");
            }

            descriptorSetUrl = headers.GetTokenValue("Content-Type", "Desc");
            if (null == descriptorSetUrl)
            {
                descriptorSetUrl = headers.GetTokenValue("Content-Type", "desc");
            }

            return true;
        }

        public static string[] LoadProtos(string protoPath)
        {
            List<string> protoFiles = new List<string>(8);

            if (protoPath != null && protoPath.Length > 0)
            {
                if (Directory.Exists(protoPath))
                {
                    int rootPathLength = protoPath.Length;
                    if (protoPath[rootPathLength - 1] != Path.DirectorySeparatorChar) rootPathLength++;

                    List<string> files = new List<string>(Directory.EnumerateFiles(protoPath, "*.proto", SearchOption.AllDirectories));
                    foreach (string file in files)
                    {
                        protoFiles.Add(file.Substring(rootPathLength).Replace(Path.DirectorySeparatorChar, '/'));
                    }
                }
            }

            return protoFiles.ToArray();
        }

        public static string GetProtoPath(string configKey)
        {
            return FiddlerApplication.Prefs.GetStringPref(configKey + "ProtoPath", "");
        }

        public static void SetProtoPath(string value, string configKey)
        {
            FiddlerApplication.Prefs.SetStringPref(configKey + "ProtoPath", value);
        }

        public static List<string> GetRecentMessageTypes(string configKey)
        {
            List<string> array = new List<string>(8);
                string recentMessageTypes = FiddlerApplication.Prefs.GetStringPref(configKey + "RecentMessageTypes", "");
                if (recentMessageTypes != null && recentMessageTypes.Length > 0)
                {
                    string[] recentMessageTypesArray = recentMessageTypes.Split('\t');
                    for (int idx = 0; idx<recentMessageTypesArray.Length; idx++)
                    {
                        array.Add(recentMessageTypesArray[idx]);
                    }
                }

                return array;
        }

        public static void SetRecentMessageTypes(List<string> value, string configKey)
        {
           FiddlerApplication.Prefs.SetStringPref(configKey + "RecentMessageTypes", (null == value || value.Count == 0) ? "" : String.Join("\t", value));
        }

        public static void CleanRecentMessageTypes(string configKey)
        {
            FiddlerApplication.Prefs.SetStringPref(configKey + "RecentMessageTypes", "");
        }

        public static void UpdateRecentMessageType(string recentMessageType, string configKey)
        {
            if (null == recentMessageType || recentMessageType.Length == 0)
            {
                return;
            }

            List<string> recentMessageTypes = FiddlerApp.GetRecentMessageTypes(configKey);
            if (null == recentMessageTypes)
            {
                recentMessageTypes = new List<string>(1);
            }
            recentMessageTypes.Insert(0, recentMessageType);

            for (int idx = 1; idx < recentMessageTypes.Count(); idx++)
            {
                if (recentMessageType.Equals(recentMessageTypes[idx]))
                {
                    recentMessageTypes.RemoveAt(idx);
                    break;
                }
            }

            FiddlerApp.SetRecentMessageTypes(recentMessageTypes, configKey);
        }

    }
}
