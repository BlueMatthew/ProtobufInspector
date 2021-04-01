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

        public virtual bool AssignSession(Session session)
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
            return /*null == session || */null == headers || null == rawBody;
        }
       
        public abstract string GetName();
        public abstract HTTPHeaders GetHeaders(Session session);

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
        public RequestInspectorContext() : base()
        {
        }

        public override HTTPHeaders GetHeaders(Session session)
        {
            return session.RequestHeaders;
        }

        public override bool AssignSession(Session session)
        {
            bool changed = false;
            if (session.state >= SessionStates.ReadingResponse)
            {
                if (session.RequestHeaders != this.headers)
                {
                    this.headers = session.RequestHeaders;
                    changed = true;
                }
                if (session.RequestBody != this.rawBody)
                {
                    this.rawBody = session.RequestBody;
                    changed = true;
                }
            }
            else
            {
                if (null == this.headers)
                {
                    this.headers = session.RequestHeaders;
                    changed = true;
                }
                if (null == this.rawBody)
                {
                    this.rawBody = session.RequestBody;
                    changed = true;
                }
            }
            
            return base.AssignSession(session) && changed;
        }

        /*
        public override byte[] GetBody()
        {
            return null == session ? null : session.RequestBody;
        }
        public override HTTPHeaders GetHeaders()
        {
            return null == session ? null : session.RequestHeaders;
        }
        */
        public override string GetName()
        {
            return "Request";
        }

    }

    public class ResponseInspectorContext : InspectorContext
    {
        public ResponseInspectorContext() : base()
        {
        }

        public override HTTPHeaders GetHeaders(Session session)
        {
            return session.ResponseHeaders;
        }

        public override bool AssignSession(Session session)
        {
            if (!session.bHasResponse)
            {
                this.rawBody = null;
                this.headers = null;
            }
            else
            {
                this.rawBody = session.ResponseBody;
                this.headers = session.ResponseHeaders;
            }
            
            return base.AssignSession(session);
        }
        
        public override string GetName()
        {
            return "Response";
        }
    }
}
