using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;
using Microsoft.WindowsAPICodePack.Dialogs;
using Ookii.Dialogs.Wpf;
using SimpleBox.Models;

namespace SimpleBox.Core
{
    public static class PictureRender
    {
        #region Interface

        public static void RenderSingle(Mallow mallow)
        {
            string fileName = SelectSaveFilePath();
            if (string.IsNullOrEmpty(fileName)) return;

            Render(new List<KeyValuePair<string, Mallow>>
            {
                new KeyValuePair<string, Mallow>(fileName, mallow)
            });
        }

        public static void RenderMultiple(Mallow[] mallows)
        {
            string folderName = SelectSaveFolderPath();
            if (string.IsNullOrEmpty(folderName)) return;

            Render(mallows.Select((t, i) =>
                new KeyValuePair<string, Mallow>(Path.Combine(folderName, $"SimpleBoxExport_{i:D3}.png"), t)).ToList());
        }

        #endregion

        #region Core

        private static void Render(List<KeyValuePair<string, Mallow>> mallows)
        {
            ProgressDialog dialog = new ProgressDialog
            {
                Text = "准备导出…",
                MinimizeBox = false,
                ShowCancelButton = true,
                ShowTimeRemaining = true,
                WindowTitle = DialogTitle
            };

            dialog.DoWork += RenderDoWork;

            dialog.Show(mallows);
        }

        static async void RenderDoWork(object sender, DoWorkEventArgs args)
        {
            if (!(sender is ProgressDialog dialog)) return;

            if (!(args.Argument is List<KeyValuePair<string, Mallow>> mallows)) return;

            if (dialog.CancellationPending) return;

            dialog.ReportProgress(0, "准备导出…", "加载渲染组件");

            PictureRenderCore renderCore = PictureRenderCore.CreateRenderCore();

            if (dialog.CancellationPending)
            {
                renderCore.Dispose();
                return;
            }

            for (int index = 0; index < mallows.Count; index++)
            {
                if (dialog.CancellationPending)
                {
                    renderCore.Dispose();
                    return;
                }

                dialog.ReportProgress((int) Math.Floor(index * 100 / (double) mallows.Count), "正在导出图片…",
                    $"第{index}个，共{mallows.Count}个");

                string fileName = mallows[index].Key;
                Mallow mallow = mallows[index].Value;

                bool pushed = false;
                WebPush.Current.PushMallow(mallow, () => pushed = true);

                while (true)
                    if (pushed)
                        break;

                Bitmap result = await renderCore.Capture();
                result.Save(fileName);
            }

            dialog.ReportProgress(100, "正在导出图片…", "正在清理");

            renderCore.Dispose();

            //dialog.Dispose();
        }

        #endregion

        #region Save Dialog

        private const string DialogTitle = "导出为图片";

        private static string SelectSaveFilePath()
        {
            CommonSaveFileDialog dialog = new CommonSaveFileDialog
            {
                AlwaysAppendDefaultExtension = true,
                DefaultDirectory = Environment.CurrentDirectory,
                DefaultExtension = ".png",
                Filters =
                {
                    new CommonFileDialogFilter("PNG 图片", ".png")
                },
                EnsurePathExists = true,
                EnsureFileExists = true,
                IsExpandedMode = true,
                Title = DialogTitle
            };

            return dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : string.Empty;
        }

        private static string SelectSaveFolderPath()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                DefaultDirectory = Environment.CurrentDirectory,
                IsFolderPicker = true,
                EnsurePathExists = true,
                Title = DialogTitle
            };

            return dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : string.Empty;
        }

        #endregion
    }

    internal class PictureRenderCore : IDisposable
    {
        #region Browser Object

        private ChromiumWebBrowser _browser;

        private readonly ManualResetEvent _resetFlag = new ManualResetEvent(false);

        #endregion

        private PictureRenderCore()
        {
        }

        #region Interface

        public static PictureRenderCore CreateRenderCore()
        {
            BrowserSettings browserSettings = new BrowserSettings
            {
                WindowlessFrameRate = 1
            };

            PictureRenderCore core = new PictureRenderCore
            {
                _browser = new ChromiumWebBrowser(
                    Config.Current.PictureRenderAddress,
                    browserSettings,
                    new RequestContext())
            };

            core._browser.BrowserInitialized += (sender, args) => core._resetFlag.Set();

            return core;
        }

        public async Task<Bitmap> Capture()
        {
            _resetFlag.WaitOne();

            _browser.Load(Config.Current.PictureRenderAddress);

            await Task.Delay(TimeSpan.FromSeconds(6));

            return await _browser.ScreenshotAsync();
        }

        #endregion

        public void Dispose() => _browser.Dispose();
    }
}
