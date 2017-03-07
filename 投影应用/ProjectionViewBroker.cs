using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace CustomMediaTransportControls
{
    //包含主页和新建页面之间的关系引用，实现主页面和新页面的通信
    internal class ProjectionViewBroker
    {
        public CoreDispatcher MainPageDispatcher;
        public ViewLifetimeControl ProjectionViewPageControl;
        public int MainViewId;
        public event EventHandler ProjectionStopping;
        public ProjectionViewPage ProjectedPage;
        public void NotifyProjectionStopping()
        {
            try
            {
                if (ProjectionStopping != null)
                    ProjectionStopping(this, EventArgs.Empty);
            }
            catch { }
        }

    }
}
