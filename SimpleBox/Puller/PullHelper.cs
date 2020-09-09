using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleBox.Core;
using SimpleBox.Models;
using SimpleBox.Utils.State;
using SimpleBox.Windows;

namespace SimpleBox.Puller
{
    public static class PullHelper
    {
        public static void Pull(MallowPuller puller)
        {
            EventWaitHandle handle = new AutoResetEvent(true);
            PullWindow pullWindow = new PullWindow(puller, handle);
            Task.Factory.StartNew(PullCore, pullWindow);

            pullWindow.Show();
        }

        private static void PullCore(object obj)
        {
            if (!(obj is PullWindow pullWindow)) return;
            pullWindow.Handle.WaitOne();

            MallowPuller puller = pullWindow.Puller;
            bool isCreateMode = pullWindow.IsCreateMode;
            MallowGroup selectedMallowGroup = pullWindow.SelectedGroup;
            string createGroupName = pullWindow.CreateGroupName;
        }
    }
}
