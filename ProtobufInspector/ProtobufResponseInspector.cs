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
    public class ProtobufResponseInspector : ProtobufInspector, IResponseInspector2
    {
        /*
        static ProtobufResponseInspector()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            FiddlerApp.LogString("ProtobufInspector is loaded, (version:" + fvi.FileVersion + ").");
        }
        */
        
        public ProtobufResponseInspector() : base()
        {
        }

        protected override InspectorContext CreateInspectorContext()
        {
            return new ResponseInspectorContext();
        }

        // IResponseInspector2.headers
        public HTTPResponseHeaders headers
        {
            get { return inspectorContext.Headers as HTTPResponseHeaders; }
            set
            {
#if DEBUG
                FiddlerApplication.Log.LogString("Res Headers Changed");
#endif
                inspectorContext.Headers = value;
                inspectorView.UpdateData();
            }
        }

        // IBaseInspector2.body
        public byte[] body
        {
            get { return inspectorContext.RawBody; }
            set
            {
#if DEBUG
                FiddlerApplication.Log.LogString("Res Body Changed");
#endif
                inspectorContext.RawBody = value;
                inspectorView.UpdateData();
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

        // IBaseInspector2.Clear
        public void Clear()
        {
#if DEBUG
            FiddlerApplication.Log.LogString("Res Clear");
#endif
            ClearInspector();
        }
    }
}
