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


        public ProtobufInspectorView()
        {
            this.inspectorContext = inspectorContext;
        }

        public HTTPHeaders GetHeaders()
        {
            return inspectorContext.GetHeaders();
            
        }

        // IBaseInspector2.body
        public byte[] GetBody()
        {
            return inspectorContext.GetBody();
        }


        public void AssignSession(Session session)
        {
            inspectorContext.AssignSession(session);
        }
        
        public ProtobufInspectorView(InspectorContext inspectorContext)
        {
            this.inspectorContext = inspectorContext;
            InitializeComponent();

            this.txtDirectory.Text = FiddlerApp.GetProtoPath(inspectorContext.GetName());

            UpdateMessageTypes(FiddlerApp.GetRecentMessageTypes(inspectorContext.GetName()));
        }

        
        public void UpdateData()
        {
            ClearView();
            
            HTTPHeaders headers = inspectorContext.Headers;
            if (!FiddlerApp.IsProtobufPacket(headers))
            {
                return;
            }

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

            if (inspectorContext.IsInvalidSession())
            {
                return;
            }

            
            if (!FiddlerApp.ParseMessageTypeNameAndDescriptorSetUrl(headers, out messageTypeName, out descriptorSetUrl))
            {
                messageTypeName = this.cmbMessageType.Text;
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
                        return;
                    }
                    jsonResult = jsonObject as Fiddler.WebFormats.JSON.JSONParseResult;
                    tvJson.Tag = jsonString;
                    tvJson.BeginUpdate();
                    try
                    {
                        tvJson.Nodes.Clear();
                        TreeNode rootNode = tvJson.Nodes.Add("JSON");
                        AddNode(jsonResult.JSONObject, rootNode);
                        tvJson.ExpandAll();
                        rootNode.EnsureVisible();
                    }
                    finally
                    {
                        tvJson.EndUpdate();
                    }

                }
            }
            catch (Exception ex)
            {
                FiddlerApplication.Log.LogString(ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
            }
        }

        /*
        public void UpdateData(Session session)
        {
            this.session = session;

            ClearView();

            string messageTypeName = "";
            string descriptorSetUrl = "";

            if (null != session && FiddlerApp.ParseMessageTypeNameAndDescriptorSetUrl(session, out messageTypeName, out descriptorSetUrl))
            {
                this.cmbMessageType.Text = messageTypeName == null ? "" : messageTypeName;
                this.cmbMessageType.Enabled = (messageTypeName == null || messageTypeName.Length == 0);
            }
            else
            {
                this.cmbMessageType.Enabled = true;
            }

            UpdateData();
        }
        */
        
        private void AddNode(object token, TreeNode inTreeNode)
        {
            if (token == null)
                return;

            if (token is Hashtable)
            {
                TreeNode parentNode = inTreeNode;

                Hashtable hashTable = token as Hashtable;
                foreach (DictionaryEntry kv in hashTable)
                {
                    TreeNode childNode = parentNode.Nodes.Add(kv.Key.ToString());
                    childNode.Tag = kv;
                    AddNode(kv.Value, childNode);
                }
            }
            else if (token is ArrayList)
            {
                ArrayList arrayList = token as ArrayList;
                for (int idx = 0; idx < arrayList.Count; idx++)
                {
                    TreeNode middleNode = AddMiddleNodeForArrayItem(inTreeNode, arrayList[idx]);
                    AddNode(arrayList[idx], middleNode);
                }
            }
            else
            {
                string text = inTreeNode.Text;
                if (text.Length > 0)
                {
                    text += "=";
                }
                text += token.ToString();
                inTreeNode.Text = text;
                inTreeNode.Tag = token;
            }
        }

        private TreeNode AddMiddleNodeForArrayItem(TreeNode inTreeNode, object arrayItem)
        {
            TreeNode middleNode = inTreeNode;

            if (arrayItem is Hashtable)
            {
                middleNode = inTreeNode.Nodes.Add("{}");
            }
            else if (arrayItem is ArrayList)
            {
                middleNode = inTreeNode.Nodes.Add("[]");
            }

            return middleNode;
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

                    UpdateData();
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
                UpdateData();
            }
        }

        private void chkboxOptions_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cmbMessageType.Text != null && this.cmbMessageType.Text.Length > 0)
            {
                UpdateData();
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
