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
#if DEBUG || OUTPUT_PERF_LOG
              
                FiddlerApplication.Log.LogString((value == null) ? "Res Headers Changed with null" : "Res Headers Changed");
#endif
                inspectorContext.Headers = value;
            }
        }

        // IBaseInspector2.body
        public byte[] body
        {
            get { return inspectorContext.RawBody; }
            set
            {
#if DEBUG || OUTPUT_PERF_LOG
                FiddlerApplication.Log.LogString((value == null) ? "Res Body Changed with null" : "Res Body Changed");
#endif
                inspectorContext.RawBody = value;
#if DEBUG || OUTPUT_PERF_LOG
                inspectorView.UpdateData("Res Body Changed");
#else
                inspectorView.UpdateData();
#endif
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
#if DEBUG || OUTPUT_PERF_LOG
            FiddlerApp.LogString("Res Clear");
#endif
            ClearInspector();
        }
    }
}
