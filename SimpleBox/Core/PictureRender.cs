using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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

            Task.Factory.StartNew(Render,
                new List<KeyValuePair<string, Mallow>>
                {
                    new KeyValuePair<string, Mallow>(fileName, mallow)
                });
        }

        public static void RenderMultiple(Mallow[] mallows)
        {
            string folderName = SelectSaveFolderPath();
            if (string.IsNullOrEmpty(folderName)) return;

            Task.Factory.StartNew(Render, mallows.Select((t, i) =>
                new KeyValuePair<string, Mallow>(Path.Combine(folderName, $"SimpleBoxExport_{i:D3}.png"), t)).ToList());
        }

        #endregion

        #region Core

        private static async Task Render(object state)
        {
            if (!(state is List<KeyValuePair<string, Mallow>> mallows)) return;

            ProgressDialog dialog = new ProgressDialog
            {
                Text = "准备导出…",
                MinimizeBox = false,
                ShowCancelButton = false,
                ShowTimeRemaining = true,
                WindowTitle = DialogTitle
            };

            dialog.ShowDialog();

            dialog.ReportProgress(
                0,
                "准备导出…",
                "加载渲染组件");

            PictureRenderCore renderCore = PictureRenderCore.CreateRenderCore();

            for (int index = 0; index < mallows.Count; index++)
            {
                dialog.ReportProgress(
                    (int) Math.Floor(index * 100 / (double) mallows.Count),
                    "正在导出图片…",
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

            dialog.ReportProgress(
                100,
                "正在导出图片…",
                "正在清理");

            renderCore.Dispose();

            dialog.Dispose();
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

        private ChromiumWebBrowser Browser;

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
                Browser = new ChromiumWebBrowser(
                    Config.Current.PictureRenderAddress,
                    browserSettings,
                    new RequestContext())
            };

            return core;
        }

        public async Task<Bitmap> Capture()
        {
            Browser.Load(Config.Current.PictureRenderAddress);

            await Task.Delay(TimeSpan.FromSeconds(6));

            return await Browser.ScreenshotAsync();
        }

        #endregion

        public void Dispose() => Browser.Dispose();
    }
}
