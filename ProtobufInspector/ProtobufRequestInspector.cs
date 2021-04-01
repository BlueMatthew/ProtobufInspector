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
    public class ProtobufRequestInspector : ProtobufInspector, IRequestInspector2
    {   
        public ProtobufRequestInspector() : base()
        {
        }

        protected override InspectorContext CreateInspectorContext()
        {
            return new RequestInspectorContext();
        }

        // IRequestInspector2.headers
        public HTTPRequestHeaders headers
        {
            get { return inspectorContext.Headers as HTTPRequestHeaders; }
            set
            {
                if (inspectorContext.Headers != value)
                {
#if DEBUG || OUTPUT_PERF_LOG
                    FiddlerApp.LogString("Req Headers Changed");
#endif
                    inspectorContext.Headers = value;
                }
            }
        }

        // IBaseInspector2.body
        public byte[] body
        {
            get { return inspectorContext.RawBody; }
            set
            {
                if (inspectorContext.RawBody != value)
                {
#if DEBUG || OUTPUT_PERF_LOG
                    FiddlerApp.LogString("Req Body Changed");
#endif
                    inspectorContext.RawBody = value;
#if DEBUG || OUTPUT_PERF_LOG
                    inspectorView.UpdateData("Req Body Changed");
#else
                    inspectorView.UpdateData();
#endif

                }
            }
        }

        // IBaseInspector2.bDirty
        public bool bDirty
        {
            get { return false; }
        }

        // IBaseInspector2.bReadOnly
        public bool bReadOnly
        {
            get { return true; }
            set {}
        }

        // IBaseInspector2.Clear
        public void Clear()
        {
#if DEBUG || OUTPUT_PERF_LOG
            FiddlerApp.LogString("Req Clear");
#endif
            ClearInspector();
        }

    }
}
