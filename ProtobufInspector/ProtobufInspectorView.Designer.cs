using System.Windows.Forms;

namespace Google.Protobuf.FiddlerInspector
{
    partial class ProtobufInspectorView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private SplitContainer splitContainer;

        private TextBox txtDirectory;
        private Label lblProtoDirectory;
        private Button btnBrowse;
        private Button btnReload;

        private Label lblMessageType;
        private ComboBox cmbMessageType;
        private ContextMenuStrip nodeMenuStrip;
        
        private ToolStripMenuItem menuItemCopy;
        private ToolStripMenuItem menuItemCopyValue;
        private ToolStripMenuItem menuItemCopyAll;

        private CheckBox chkboxEnumValue;
        private Label lblOptions;
        private CheckBox chkboxPrintPrimitiveFields;

        private TreeView tvJson;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.chkboxPrintPrimitiveFields = new System.Windows.Forms.CheckBox();
            this.chkboxEnumValue = new System.Windows.Forms.CheckBox();
            this.lblOptions = new System.Windows.Forms.Label();
            this.cmbMessageType = new System.Windows.Forms.ComboBox();
            this.lblMessageType = new System.Windows.Forms.Label();
            this.btnReload = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtDirectory = new System.Windows.Forms.TextBox();
            this.lblProtoDirectory = new System.Windows.Forms.Label();
            this.tvJson = new System.Windows.Forms.TreeView();
            this.nodeMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemCopyValue = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemCopyAll = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.nodeMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.IsSplitterFixed = true;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer1";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.chkboxPrintPrimitiveFields);
            this.splitContainer.Panel1.Controls.Add(this.chkboxEnumValue);
            this.splitContainer.Panel1.Controls.Add(this.lblOptions);
            this.splitContainer.Panel1.Controls.Add(this.cmbMessageType);
            this.splitContainer.Panel1.Controls.Add(this.lblMessageType);
            this.splitContainer.Panel1.Controls.Add(this.btnReload);
            this.splitContainer.Panel1.Controls.Add(this.btnBrowse);
            this.splitContainer.Panel1.Controls.Add(this.txtDirectory);
            this.splitContainer.Panel1.Controls.Add(this.lblProtoDirectory);
            this.splitContainer.Panel1MinSize = 80;
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.tvJson);
            this.splitContainer.Size = new System.Drawing.Size(995, 709);
            this.splitContainer.SplitterDistance = 80;
            this.splitContainer.TabIndex = 0;
            // 
            // chkboxPrintPrimitiveFields
            // 
            this.chkboxPrintPrimitiveFields.AutoSize = true;
            this.chkboxPrintPrimitiveFields.Location = new System.Drawing.Point(293, 56);
            this.chkboxPrintPrimitiveFields.Name = "chkboxPrintPrimitiveFields";
            this.chkboxPrintPrimitiveFields.Size = new System.Drawing.Size(156, 16);
            this.chkboxPrintPrimitiveFields.TabIndex = 12;
            this.chkboxPrintPrimitiveFields.Text = "Print Primitive Fields";
            this.chkboxPrintPrimitiveFields.UseVisualStyleBackColor = true;
            this.chkboxPrintPrimitiveFields.CheckedChanged += new System.EventHandler(this.chkboxOptions_CheckedChanged);
            // 
            // chkboxEnumValue
            // 
            this.chkboxEnumValue.AutoSize = true;
            this.chkboxEnumValue.Location = new System.Drawing.Point(116, 56);
            this.chkboxEnumValue.Name = "chkboxEnumValue";
            this.chkboxEnumValue.Size = new System.Drawing.Size(150, 16);
            this.chkboxEnumValue.TabIndex = 11;
            this.chkboxEnumValue.Text = "Print Enum As Integer";
            this.chkboxEnumValue.UseVisualStyleBackColor = true;
            this.chkboxEnumValue.CheckedChanged += new System.EventHandler(this.chkboxOptions_CheckedChanged);
            // 
            // lblOptions
            // 
            this.lblOptions.AutoSize = true;
            this.lblOptions.Location = new System.Drawing.Point(3, 58);
            this.lblOptions.Name = "label3";
            this.lblOptions.Size = new System.Drawing.Size(83, 12);
            this.lblOptions.TabIndex = 10;
            this.lblOptions.Text = "JSON Options:";
            // 
            // cmbMessageType
            // 
            this.cmbMessageType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbMessageType.FormattingEnabled = true;
            this.cmbMessageType.Location = new System.Drawing.Point(116, 32);
            this.cmbMessageType.Name = "cmbMessageType";
            this.cmbMessageType.Size = new System.Drawing.Size(783, 20);
            this.cmbMessageType.FlatStyle = FlatStyle.Flat;
            this.cmbMessageType.TabIndex = 9;
            this.cmbMessageType.SelectedIndexChanged += new System.EventHandler(this.cmbMsgType_SelectedIndexChanged);
            this.cmbMessageType.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cmbMessageType_KeyPress);
            // 
            // lblMessageType
            // 
            this.lblMessageType.AutoSize = true;
            this.lblMessageType.Location = new System.Drawing.Point(3, 34);
            this.lblMessageType.Name = "label2";
            this.lblMessageType.Size = new System.Drawing.Size(83, 12);
            this.lblMessageType.TabIndex = 7;
            this.lblMessageType.Text = "Message Type:";
            // 
            // btnReload
            // 
            this.btnReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReload.Location = new System.Drawing.Point(911, 29);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(75, 23);
            this.btnReload.TabIndex = 6;
            this.btnReload.Text = "Reload";
            this.btnReload.FlatStyle = FlatStyle.Flat;
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(911, 5);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 4;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.FlatStyle = FlatStyle.Flat;
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtDirectory
            // 
            this.txtDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDirectory.Location = new System.Drawing.Point(116, 7);
            this.txtDirectory.Name = "txtDirectory";
            this.txtDirectory.ReadOnly = true;
            this.txtDirectory.Size = new System.Drawing.Size(783, 21);
            this.txtDirectory.TabIndex = 1;
            // 
            // lblProtoDirectory
            // 
            this.lblProtoDirectory.AutoSize = true;
            this.lblProtoDirectory.Location = new System.Drawing.Point(3, 11);
            this.lblProtoDirectory.Name = "label1";
            this.lblProtoDirectory.Size = new System.Drawing.Size(107, 12);
            this.lblProtoDirectory.TabIndex = 0;
            this.lblProtoDirectory.Text = "Protos Folder:";
            // 
            // tvJson
            // 
            this.tvJson.BackColor = System.Drawing.Color.Azure;
            this.tvJson.ContextMenuStrip = this.nodeMenuStrip;
            this.tvJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvJson.Location = new System.Drawing.Point(0, 0);
            this.tvJson.Margin = new System.Windows.Forms.Padding(4);
            this.tvJson.Name = "tvJson";
            this.tvJson.ShowNodeToolTips = true;
            this.tvJson.FullRowSelect = true;
            this.tvJson.Size = new System.Drawing.Size(995, 625);
            this.tvJson.TabIndex = 0;
            // 
            // nodeMenuStrip
            // 
            this.nodeMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemCopy,
            this.menuItemCopyValue,
            this.menuItemCopyAll});
            this.nodeMenuStrip.Name = "nodeMenuStrip";
            this.nodeMenuStrip.Size = new System.Drawing.Size(153, 70);
            // 
            // menuItemCopy
            // 
            this.menuItemCopy.Name = "copyToolStripMenuItem";
            this.menuItemCopy.Size = new System.Drawing.Size(152, 22);
            this.menuItemCopy.Text = "Copy Raw Text";
            this.menuItemCopy.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // menuItemCopyValue
            // 
            this.menuItemCopyValue.Name = "copyValueToolStripMenuItem";
            this.menuItemCopyValue.Size = new System.Drawing.Size(152, 22);
            this.menuItemCopyValue.Text = "Copy Value";
            this.menuItemCopyValue.Click += new System.EventHandler(this.copyValueToolStripMenuItem_Click);
            // 
            // menuItemCopyAll
            // 
            this.menuItemCopyAll.Name = "copyAllToolStripMenuItem";
            this.menuItemCopyAll.Size = new System.Drawing.Size(152, 22);
            this.menuItemCopyAll.Text = "Copy All";
            this.menuItemCopyAll.Click += new System.EventHandler(this.copyAllToolStripMenuItem_Click);
            // 
            // ProtobufView
            // 
            this.Controls.Add(this.splitContainer);
            this.Name = "ProtobufView";
            this.Size = new System.Drawing.Size(995, 709);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.nodeMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        protected void ClearView(bool includingJson = true)
        {
            if (includingJson && this.tvJson.Nodes.Count > 0)
            {
                this.tvJson.Nodes.Clear();
                this.tvJson.Tag = null;
            }
            
            if (this.cmbMessageType.Text.Length > 0)
            {
                this.cmbMessageType.Text = "";
            }
        }
    }
}
