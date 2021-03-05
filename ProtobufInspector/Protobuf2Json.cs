using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Net;
using Fiddler;

namespace Google.Protobuf.FiddlerInspector
{
    class Protobuf2Json
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct PB2JSON_PROTOS_SRC_INFO
        {
            [MarshalAs(UnmanagedType.LPStr)] public string protoPath;
            [MarshalAs(UnmanagedType.U4)] public UInt32 numberOfProtoFileNames;
            public IntPtr protoFileNames;
            [MarshalAs(UnmanagedType.LPStr)] public string messageTypeName;
            public IntPtr messageData;
            [MarshalAs(UnmanagedType.U4)] public UInt32 lengthOfMessageData;
            [MarshalAs(UnmanagedType.U4)] public UInt32 options;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PB2JSON_DESCRIPTOR_SET_SRC_INFO
        {
            [MarshalAs(UnmanagedType.LPStr)] public string descriptorSetFileName;
            [MarshalAs(UnmanagedType.LPStr)] public string messageTypeName;
            public IntPtr messageData;
            [MarshalAs(UnmanagedType.U4)] public UInt32 lengthOfMessageData;
            [MarshalAs(UnmanagedType.U4)] public UInt32 options;
        }

        [DllImport("Protobuf2Json.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int ConvertMessageWithProtoFilesToJson(
            [MarshalAs(UnmanagedType.Struct)] ref PB2JSON_PROTOS_SRC_INFO src,
            ref IntPtr outputString,
            [MarshalAs(UnmanagedType.U4)] ref UInt32 lengthOfOutputString
        );

        [DllImport("Protobuf2Json.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int ConvertMessageWithDescriptorSetToJson(
            [MarshalAs(UnmanagedType.Struct)] ref PB2JSON_DESCRIPTOR_SET_SRC_INFO src,
            ref IntPtr outputString,
            [MarshalAs(UnmanagedType.U4)] ref UInt32 lengthOfOutputString
        );

        [DllImport("Protobuf2Json.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int FreeOutputString(
            IntPtr outputString,
            [MarshalAs(UnmanagedType.U4)] UInt32 lengthOfOutputString
        );

        public static string convertAndFreeStrPtr(IntPtr outputStringPtr, UInt32 outputStringLength)
        {
            string outputString = "";
            if (outputStringPtr != IntPtr.Zero)
            {
                byte[] buffer = new byte[outputStringLength];
                Marshal.Copy(outputStringPtr, buffer, 0, (int)outputStringLength);
                FreeOutputString(outputStringPtr, outputStringLength);

                outputString = Encoding.UTF8.GetString(buffer);
            }

            return outputString;
        }

        public static string ConvertToJson(string protoPath, string[] protoFiles, string descriptorSetUrl, string messageTypeName, bool printEnumAsInteger, bool printPrimitiveFields, bool isReq, byte[] data)
        {
#if DEBUG
            FiddlerApplication.Log.LogString("Start Decoding");
#endif
            if (null == messageTypeName)
            {
#if DEBUG
                FiddlerApplication.Log.LogString("Decoding:messageTypeName is null");
#endif
                // return null;
            }
            string retval = string.Empty;
        
            int ret = 0;
            string outputString = "";
            IntPtr outputStringPtr = IntPtr.Zero;
            UInt32 outputStringLength = 0;

            bool descriptorSetFileExisted = false;
            if (null != descriptorSetUrl)
            {
                string descriptorSetFileName = "";
                descriptorSetFileExisted = DownloadDescriptorSetFile(descriptorSetUrl, out descriptorSetFileName);

                if (descriptorSetFileExisted)
                {
                    PB2JSON_DESCRIPTOR_SET_SRC_INFO src = new PB2JSON_DESCRIPTOR_SET_SRC_INFO();

                    src.options = 4;
                    if (printEnumAsInteger) src.options |= 1;
                    if (printPrimitiveFields) src.options |= 2;

                    src.messageTypeName = messageTypeName;
                    src.descriptorSetFileName = descriptorSetFileName;

                    src.messageData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * data.Length);
                    src.lengthOfMessageData = (UInt32)data.Length;
                    try
                    {
                        Marshal.Copy(data, 0, src.messageData, data.Length);
                        ret = ConvertMessageWithDescriptorSetToJson(ref src, ref outputStringPtr, ref outputStringLength);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(src.messageData);
                    }

                    if (File.Exists(descriptorSetFileName)) File.Delete(descriptorSetFileName);
                }
            }
            
            if (!descriptorSetFileExisted)
            {

                int totalLength = 0;
                for (int idx = 0; idx < protoFiles.Length; idx++)
                {
                    totalLength += System.Text.Encoding.UTF8.GetByteCount(protoFiles[idx]) + 1;
                }

                byte[] protoFilesPtr = new byte[totalLength + 1];
                protoFilesPtr[totalLength] = 0;
                int offset = 0;
                for (int idx = 0; idx < protoFiles.Length; idx++)
                {
                    byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(protoFiles[idx]);

                    utf8Bytes.CopyTo(protoFilesPtr, offset);
                    offset += utf8Bytes.Length;
                    protoFilesPtr[offset] = 0;
                    offset++;
                }
      
                PB2JSON_PROTOS_SRC_INFO src = new PB2JSON_PROTOS_SRC_INFO();

                src.options = 4;
                if (printEnumAsInteger) src.options |= 1;
                if (printPrimitiveFields) src.options |= 2;

                src.protoPath = protoPath;
                // src.protoFileNames = protoFiles;
                src.numberOfProtoFileNames = (UInt32)protoFiles.Length;
                src.lengthOfMessageData = (UInt32)data.Length;
                src.messageTypeName = messageTypeName;

                src.protoFileNames = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * protoFilesPtr.Length);
                src.messageData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * data.Length);
                try
                {
                    Marshal.Copy(data, 0, src.messageData, data.Length);
                    Marshal.Copy(protoFilesPtr, 0, src.protoFileNames, protoFilesPtr.Length);
                    
                    ret = ConvertMessageWithProtoFilesToJson(ref src, ref outputStringPtr, ref outputStringLength);
                }
                finally
                {
                    Marshal.FreeHGlobal(src.messageData);
                    Marshal.FreeHGlobal(src.protoFileNames);
                }
            }

            if (ret == 1)
            {
                try
                {
                    outputString = convertAndFreeStrPtr(outputStringPtr, outputStringLength);
                }
                catch (Exception ex)
                {
                    FiddlerApplication.Log.LogString("Decoding exception: " + ex.Message);
                }
            }
            else
            {
                string errorStr = convertAndFreeStrPtr(outputStringPtr, outputStringLength);
                if (errorStr != null && errorStr.Length > 0)
                {
                    FiddlerApplication.Log.LogString("Protobuf Decoding Failed:" + errorStr);
                }
            }

            return outputString;
        }

        protected static bool DownloadDescriptorSetFile(string descriptorSetUrl, out string descriptorSetFileName)
        {
            descriptorSetFileName = "";

            bool ret = false;
            Stream stm = null;
            FileStream fs = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(descriptorSetUrl);
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
                request.AllowAutoRedirect = true;
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.Credentials = CredentialCache.DefaultCredentials;
                WebResponse response = request.GetResponse();

                HttpWebResponse httpWebResponse = (HttpWebResponse)response;
                if (HttpStatusCode.OK == httpWebResponse.StatusCode)
                {
                    stm = httpWebResponse.GetResponseStream();

                    descriptorSetFileName = Path.GetTempFileName();

                    fs = File.OpenWrite(descriptorSetFileName);
                    stm.CopyTo(fs);
                    
                    ret = true;
                }
            }
            catch (Exception ex)
            {
                if (File.Exists(descriptorSetFileName)) File.Delete(descriptorSetFileName);
                FiddlerApplication.Log.LogString("Exception on decoding: " + ex.Message);
            }
            finally
            {
                // File.Delete(descriptorSetFileName);
                if (null != stm)
                {
                    stm.Close();
                }
                if (null != fs)
                {
                    fs.Close();
                }
            }

            if (!File.Exists(descriptorSetFileName))
            {
                ret = false;
            }

            return ret;
        }
        
    }
}
