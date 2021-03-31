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
    public abstract class InspectorContext
    {
        public InspectorContext()
        {
            Clear();
        }

        public bool AssignSession(Session session)
        {
            bool changed = (this.session != session);
            this.session = session;
            return changed;
        }

        public void Clear()
        {
            session = null;
            headers = null;
            rawBody = null;
        }

        public bool IsInvalidSession()
        {
            return null == session || null == headers || null == rawBody;
        }
        public abstract byte[] GetBody();
        public abstract HTTPHeaders GetHeaders();
        public abstract string GetName();

        public HTTPHeaders Headers
        {
            get { return headers; }
            set { headers = value; }
        }

        public byte[] RawBody
        {
            get { return rawBody; }
            set { rawBody = value; }
        }


        protected Session session;
        protected HTTPHeaders headers;
        protected byte[] rawBody;
    }
    
    public class RequestInspectorContext : InspectorContext
    {
        public RequestInspectorContext()
        {
        }
        
        public override byte[] GetBody()
        {
            return null == session ? null : session.RequestBody;
        }
        public override HTTPHeaders GetHeaders()
        {
            return null == session ? null : session.RequestHeaders;
        }
        public override string GetName()
        {
            return "Request";
        }

    }

    public class ResponseInspectorContext : InspectorContext
    {
        public ResponseInspectorContext()
        {
        }

        public override byte[] GetBody()
        {
            return null == session ? null : session.ResponseBody;
        }
        public override HTTPHeaders GetHeaders()
        {
            return null == session ? null : session.ResponseHeaders;
        }
        public override string GetName()
        {
            return "Response";
        }
    }
}
