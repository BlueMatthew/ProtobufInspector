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
        public bool bReadOnly
        {
            get { return true; }
            set {}
        }

    }
}
