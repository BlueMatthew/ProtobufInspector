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

                    object jsonObject = Fiddler.WebFormats.JSON.JsonDecode(jsonString);
                    Fiddler.WebFormats.JSON.JSONParseResult jsonResult = null;
                    if (!(jsonObject is Fiddler.WebFormats.JSON.JSONParseResult))
                    {
                        tvJson.Nodes.Clear();
                        return;
                    }
                    
                    jsonResult = jsonObject as Fiddler.WebFormats.JSON.JSONParseResult;
                    tvJson.Tag = jsonString;
#if DEBUG || OUTPUT_PERF_LOG
                    FiddlerApplication.Log.LogString(inspectorContext.GetName() + " beginUpdate");
#endif
                    TreeNode rootNode = new TreeNode("Protobuf");

                    Queue<KeyValuePair<object, TreeNode>> queue = new Queue<KeyValuePair<object, TreeNode>>();
                    object jsonItem = jsonResult.JSONObject;
                    TreeNode parentNode = rootNode;
                    
                    while (true)
                    {
                        AddNode(jsonItem, parentNode, queue);
                        if (queue.Count == 0)
                        {
                            break;
                        }

                        KeyValuePair<object, TreeNode> kv = queue.Dequeue();
                        jsonItem = kv.Key;
                        parentNode = kv.Value;
                    }

                    rootNode.ExpandAll();

                    tvJson.BeginUpdate();
                    try
                    {   
                        if (tvJson.Nodes.Count > 0)
                        {
                            tvJson.Nodes.Clear();
                        }
                        
                        tvJson.Nodes.Add(rootNode);

                        // tvJson.ExpandAll();
                        // rootNode.EnsureVisible();
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

        private void AddNode(object token, TreeNode node, Queue<KeyValuePair<object, TreeNode>> queue)
        {
            if (token == null)
                return;

            if (token is Hashtable)
            {
                Hashtable hashTable = token as Hashtable;
                if (hashTable.Count > 0)
                {
                    List<TreeNode> childNodes = new List<TreeNode>(hashTable.Count);
                    
                    foreach (DictionaryEntry kv in hashTable)
                    {
                        TreeNode childNode = new TreeNode(kv.Key.ToString());
                        childNode.Tag = kv;
                        childNodes.Add(childNode);
                        
                        queue.Enqueue(new KeyValuePair<object, TreeNode>(kv.Value, childNode));
                        // AddNode(kv.Value, childNode, queue);
                    }
                    if (childNodes.Count > 0)
                    {
                        node.Nodes.AddRange(childNodes.ToArray());
                    }
                }
                
            }
            else if (token is ArrayList)
            {
                ArrayList arrayList = token as ArrayList;
                if (arrayList.Count > 0)
                {
                    List<TreeNode> childNodes = new List<TreeNode>(arrayList.Count);

                    for (int idx = 0; idx < arrayList.Count; idx++)
                    {
                        TreeNode middleNode = node;

                        if (arrayList[idx] is Hashtable)
                        {
                            middleNode = new TreeNode("{}");
                            childNodes.Add(middleNode);
                        }
                        else if (arrayList[idx] is ArrayList)
                        {
                            middleNode = new TreeNode("[]");
                            childNodes.Add(middleNode);
                        }

                        // TreeNode middleNode = AddMiddleNodeForArrayItem(inTreeNode, arrayList[idx]);
                        queue.Enqueue(new KeyValuePair<object, TreeNode>(arrayList[idx], middleNode));
                        // AddNode(arrayList[idx], middleNode, queue);
                    }

                    if (childNodes.Count > 0)
                    {
                        node.Nodes.AddRange(childNodes.ToArray());
                    }
                }
            }
            else
            {
                string text = node.Text;
                if (text.Length > 0)
                {
                    text += "=";
                }
                text += token.ToString();
                node.Text = text;
                node.Tag = token;
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

        
    }
}
