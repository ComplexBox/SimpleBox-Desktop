using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SimpleBox.Core;
using SimpleBox.Helpers;
using SimpleBox.Models;
using SimpleBox.Utils.State;
using SimpleBox.Windows;

namespace SimpleBox.Puller
{
    public static class PullHelper
    {
        public static void Pull(MallowPuller puller)
        {
            if (puller is null) return;
            PullWindow pullWindow = new PullWindow(puller);
            pullWindow.Show();
        }

        public static async Task PullCore(object obj)
        {
            if (!(obj is PullWindow pullWindow)) return;

            MallowPuller puller = pullWindow.Puller;
            bool isCreateMode = pullWindow.IsCreateMode;
            MallowGroup selectedMallowGroup = pullWindow.SelectedGroup;
            string createGroupName = pullWindow.CreateGroupName;

            if (!isCreateMode && selectedMallowGroup is null)
                return;

            puller.Progress.IsIndeterminate = true;
            puller.Progress.Text = $"初始化{puller.Name}服务……";

            Mallow[] mallows = await puller.VerifyAndPull(isCreateMode ? Array.Empty<Mallow>() : selectedMallowGroup.Mallows.ToArray());

            puller.Progress.IsIndeterminate = true;
            puller.Progress.Text = "保存用户信息……";

            CookieStorageHelper.SaveData();

            Application.Current.Dispatcher.Invoke(() => pullWindow.Close());

            if (mallows is null)
                return;

            if (mallows.Length == 0)
            {
                MessageBox.Show(
                    "没有要拉取的项目。",
                    "成功",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    MessageBoxResult.OK);

                return;
            }

            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                MallowGroup resultGroup = isCreateMode
                    ? new MallowGroup
                    {
                        Name = createGroupName
                    }
                    : selectedMallowGroup;
                if (resultGroup is null) return;

                for (int i = 0; i < mallows.Length; i++)
                    resultGroup.Mallows.Insert(i, mallows[i]);

                MallowSource.CurrentSource.Data.Insert(0, resultGroup);
                MallowSource.CurrentSource.Current = resultGroup;
            });
        }
    }
}
