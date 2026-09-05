using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Fiddler;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace Google.Protobuf.FiddlerInspector
{
    public partial class ProtobufInspectorView : UserControl
    {
        protected InspectorContext inspectorContext;
        
        private const uint BM_CLICK = 0x00F5;

        [DllImport("User32.Dll", EntryPoint = "PostMessageA", SetLastError = true)]
        public static extern int PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        
        public ProtobufInspectorView(InspectorContext inspectorContext)
        {
            this.inspectorContext = inspectorContext;
            InitializeComponent();

#if DEBUG || OUTPUT_PERF_LOG
            FiddlerApp.LogString("New " + inspectorContext.GetName() + " View");
#endif
            this.txtDirectory.Text = FiddlerApp.GetProtoPath(inspectorContext.GetName());

            UpdateMessageTypes(FiddlerApp.GetRecentMessageTypes(inspectorContext.GetName()));
        }

#if DEBUG || OUTPUT_PERF_LOG
        public void UpdateData(string reason)
#else
        public void UpdateData()
#endif
        {
#if DEBUG || OUTPUT_PERF_LOG
            FiddlerApplication.Log.LogString(inspectorContext.GetName() + " UpdateData: " + reason);
#else
            FiddlerApp.LogString(inspectorContext.GetName() + " UpdateData");
#endif

            if (inspectorContext.IsInvalidSession())
            {
#if DEBUG || OUTPUT_PERF_LOG
                FiddlerApp.LogString("UpdateData exits for invalidated session");
#endif
                ClearView();
                return;
            }

            HTTPHeaders headers = inspectorContext.Headers;
            if (!FiddlerApp.IsProtobufPacket(headers))
            {
#if DEBUG || OUTPUT_PERF_LOG
                FiddlerApp.LogString("UpdateData exits for non-protobuf session");
#endif
                ClearView();
                return;
            }

            ClearView(false);
            string messageTypeName = "";
            string descriptorSetUrl = "";
            if (null != headers && FiddlerApp.ParseMessageTypeNameAndDescriptorSetUrl(headers, out messageTypeName, out descriptorSetUrl))
            {
                this.cmbMessageType.Text = messageTypeName == null ? "" : messageTypeName;
                this.cmbMessageType.Enabled = (messageTypeName == null || messageTypeName.Length == 0);
            }
            else
            {
                this.cmbMessageType.Enabled = true;
            }
            
            string protoPath = this.txtDirectory.Text;
            bool printEnumAsInteger = this.chkboxEnumValue.Checked;
            bool printPrimitiveFields = this.chkboxPrintPrimitiveFields.Checked;

            try
            {
                string jsonString = null;
                byte[] body = FiddlerApp.DecodeContent(inspectorContext.RawBody, headers);

                if (null != body)
                {
                    string[] protoFiles = FiddlerApp.LoadProtos(protoPath);

                    jsonString = Protobuf2Json.ConvertToJson(protoPath, protoFiles, descriptorSetUrl, messageTypeName, printEnumAsInteger, printPrimitiveFields, false, body);
                    object jsonObject = JsonParser.ParseJson(jsonString);
                    if (jsonObject == null)
                    {
                        tvJson.Nodes.Clear();
                        return;
                    }
                    
                    tvJson.Tag = jsonString;
#if DEBUG || OUTPUT_PERF_LOG
                    FiddlerApplication.Log.LogString(inspectorContext.GetName() + " beginUpdate");
#endif
                    TreeNode rootNode = new TreeNode("Protobuf");

                    AddNode(jsonObject, rootNode);

                    tvJson.BeginUpdate();
                    try
                    {   
                        if (tvJson.Nodes.Count > 0)
                        {
                            tvJson.Nodes.Clear();
                        }

                        tvJson.Nodes.Add(rootNode);
                        rootNode.ExpandAll();
                    }
                    finally
                    {
                        tvJson.EndUpdate();
#if DEBUG || OUTPUT_PERF_LOG
                        FiddlerApp.LogString(inspectorContext.GetName() + " EndUpdate: " + tvJson.GetNodeCount(true).ToString());
#endif
                    }

                }
            }
            catch (Exception ex)
            {
                FiddlerApp.LogString(ex.Message);
            }
        }

        private void AddNode(object token, TreeNode node)
        {
            const int maxValueLength = 4096;

            IDictionary dictionary = token as IDictionary;
            if (dictionary != null)
            {
                foreach (DictionaryEntry item in dictionary)
                {
                    AddNode(item.Value, node.Nodes.Add(item.Key.ToString()));
                }
                return;
            }

            IList list = token as IList;
            if (list != null)
            {
                foreach (object item in list)
                {
                    AddNode(item, item is IDictionary ? node.Nodes.Add("{}") : item is IList ? node.Nodes.Add("[]") : node);
                }
                return;
            }

            if (token != null)
            {
                string value = token.ToString();
                bool isBinary = token is string && value.Take(maxValueLength).Any(c => char.IsControl(c) && c != '\t' && c != '\r' && c != '\n');
                node.Tag = token;
                if (isBinary)
                {
                    node.Text += "=<binary data, " + value.Length + " bytes>";
                    return;
                }

                bool truncated = value.Length > maxValueLength;
                if (token is string)
                {
                    value = value.Substring(0, Math.Min(value.Length, maxValueLength));
                    value = new JavaScriptSerializer().Serialize(value);
                    value = value.Substring(1, value.Length - 2);
                }

                node.Text += "=" + value.Substring(0, Math.Min(value.Length, maxValueLength)) + (truncated || value.Length > maxValueLength ? "..." : "");
            }
        }

        private void cmbMsgType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMessageType.SelectedIndex == cmbMessageType.Items.Count - 1)
            {
                cmbMessageType.Items.Clear();
                FiddlerApp.CleanRecentMessageTypes(inspectorContext.GetName());
                return;
            }
            PostMessage(this.btnReload.Handle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = false;
                dialog.SelectedPath = this.txtDirectory.Text;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    FiddlerApp.SetProtoPath(dialog.SelectedPath, inspectorContext.GetName());
                    this.txtDirectory.Text = dialog.SelectedPath;

#if DEBUG || OUTPUT_PERF_LOG
                    UpdateData("ProtoPath Changed");
#else
                    UpdateData();
#endif
                }
            }
        }

        private void cmbMessageType_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                PostMessage(this.btnReload.Handle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            if (this.cmbMessageType.Text != null && this.cmbMessageType.Text.Length > 0)
            {
#if DEBUG || OUTPUT_PERF_LOG
                UpdateData("Reload Clicked");
#else
                UpdateData();
#endif
            }
        }

        private void chkboxOptions_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cmbMessageType.Text != null && this.cmbMessageType.Text.Length > 0)
            {
#if DEBUG || OUTPUT_PERF_LOG
                UpdateData("Options Changed");
#else
                UpdateData();
#endif
            }
        }

        private void txtSearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvJson.SelectedNode != null)
            {
                Clipboard.SetText(tvJson.SelectedNode.Text);
            }
        }

        private void copyValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvJson.SelectedNode != null)
            {
                object value = tvJson.SelectedNode.Tag;
                if (value != null)
                {
                    string text = value.ToString();
                    if (value is string)
                    {
                        text = new JavaScriptSerializer { MaxJsonLength = int.MaxValue }.Serialize(text);
                        text = text.Substring(1, text.Length - 2);
                    }

                    Clipboard.SetText(text);
                    return;
                }

                String val = tvJson.SelectedNode.Text;
                int pos = val.IndexOf('=');
                if (pos == -1)
                    Clipboard.SetText(val);
                else
                    Clipboard.SetText(val.Substring(pos + 1));
            }
        }

        private void copyAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string jsonString = this.tvJson.Tag as string;
                if (null == jsonString)
                {
                    return;
                }
                
                Clipboard.SetText(jsonString);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public void UpdateMessageTypes(List<string> messageTypes)
        {
            string text = cmbMessageType.Text;
            this.cmbMessageType.SelectedIndexChanged -= cmbMsgType_SelectedIndexChanged;
            cmbMessageType.BeginUpdate();
            cmbMessageType.Items.Clear();
            cmbMessageType.Items.AddRange(messageTypes.ToArray());
            if (messageTypes.Count > 0)
            {
                cmbMessageType.Items.Add("Clean History Data...");
            }
            cmbMessageType.Text = text;
            cmbMessageType.EndUpdate();
            this.cmbMessageType.SelectedIndexChanged += cmbMsgType_SelectedIndexChanged;
        }


        public static class JsonParser
        {
            /// <summary>
            /// Parse JSON string efficiently, returning ArrayList for arrays and Hashtable for objects
            /// </summary>
            /// <param name="jsonString">JSON string to parse</param>
            /// <returns>Parsing result, could be Hashtable or ArrayList type</returns>
            public static object ParseJson(string jsonString)
            {
                if (string.IsNullOrEmpty(jsonString))
                    return null;

                try
                {
                    return new JavaScriptSerializer { MaxJsonLength = int.MaxValue }.DeserializeObject(jsonString);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

    }
}
