using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Fiddler;

namespace Google.Protobuf.FiddlerInspector
{
    public class ProtobufResponseInspector : Inspector2, IResponseInspector2
    {
        private byte[] responseBody;
        protected HTTPHeaders responseHeaders;
        protected Session session;
        protected ProtobufInspectorView inspectorView;

        // protected string jsonString;

        static ProtobufResponseInspector()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            FiddlerApp.LogString("ProtobufInspector is loaded, (version:" + fvi.FileVersion + ").");
        }

        public ProtobufResponseInspector()
        {
            inspectorView = new ProtobufInspectorView();
        }

        public override void AddToTab(TabPage o)
        {
            o.Text = @"Protobuf";
            o.Controls.Add(inspectorView);
            o.Controls[0].Dock = DockStyle.Fill;
            inspectorView.Dock = DockStyle.Fill;

            // _view.UpdateProtoDirectory(ProtobufHelper.ProtoDirectory);

        }

        public override void AssignSession(Session oS)
        {
#if DEBUG
            FiddlerApplication.Log.LogString("AssignSession:" + oS.url);
#endif
            base.AssignSession(oS);
            
            session = oS;
            
            UpdateView();
        }

        public override int GetOrder()
        {
            return 0;
        }

        // IBaseInspector2.Clear
        public void Clear()
        {
            session = null;
            responseBody = null;
            responseHeaders = null;

            UpdateView();

    }

        // IBaseInspector2.headers
        public HTTPResponseHeaders headers
        {
            get { return responseHeaders as HTTPResponseHeaders; }
            set
            {
                responseHeaders = value;
                // UpdateView();
            }
        }

        // IBaseInspector2.body
        public byte[] body
        {
            get { return responseBody; }
            set
            {
                responseBody = value;
                // UpdateView();
            }
        }

        // IBaseInspector2.bDirty
        public bool bDirty
        {
            get { return false; }
        }

        // IBaseInspector2.bReadOnly
        public bool bReadOnly {
            get { return true; }
            set {}
        }

        private bool IsProtobufPacket()
        {
            if (null == session)
            {
                return false;
            }

            HTTPResponseHeaders headers = session.ResponseHeaders;
            return null != headers && headers.ExistsAndContains("Content-Type", "application/x-protobuf");
        }

        protected void UpdateView()
        {
            inspectorView.UpdateData(IsProtobufPacket() ? session : null);
        }

    }
}
