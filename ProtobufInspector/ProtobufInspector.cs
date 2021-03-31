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
    public abstract class ProtobufInspector : Inspector2
    {   
        protected InspectorContext inspectorContext;
        protected ProtobufInspectorView inspectorView;

        static ProtobufInspector()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            FiddlerApp.LogString("ProtobufInspector is loaded, (version:" + fvi.FileVersion + ").");
        }

        protected abstract InspectorContext CreateInspectorContext();


        public ProtobufInspector()
        {
            inspectorContext = CreateInspectorContext();
            inspectorView = new ProtobufInspectorView(inspectorContext);
        }

        // Inspector2
        public override void AddToTab(TabPage o)
        {
            o.Text = @"Protobuf";
            o.Controls.Add(inspectorView);
            o.Controls[0].Dock = DockStyle.Fill;
            inspectorView.Dock = DockStyle.Fill;

            // _view.UpdateProtoDirectory(ProtobufHelper.ProtoDirectory);

        }

        // Inspector2
        public override void AssignSession(Session oS)
        {
            base.AssignSession(oS);

            if (inspectorContext.AssignSession(oS))
            {
#if DEBUG
                FiddlerApplication.Log.LogString("AssignSession:" + inspectorContext.GetName() + " " + oS.url);
#endif
                inspectorView.UpdateData();
            }
        }

        public override bool CommitAnyChanges(Session oS)
        {
            bool result = base.CommitAnyChanges(oS);

#if DEBUG
            FiddlerApplication.Log.LogString("CommitAnyChanges:" + inspectorContext.GetName() + " " + oS.url);
#endif
            // inspectorView.UpdateData();

            return result;
        }

        // Inspector2
        public override int GetOrder()
        {
            return 0;
        }

        public void ClearInspector()
        {
            inspectorContext.Clear();
            inspectorView.UpdateData();
        }

    }
}
