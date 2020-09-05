using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using SimpleBox.Models;

namespace SimpleBox.Core
{
    public interface IImporter
    {
        public string Name { get; }
        public string DialogDisplayName { get; }
        public string ExtensionName { get; }
        public Mallow[] Parse(string rawData);
    }

    public static class ImportHelper
    {
        public static void Import(IImporter importer)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Title = $"选择{importer.DialogDisplayName}",
                DefaultDirectory = Environment.CurrentDirectory,
                IsFolderPicker = false,
                DefaultExtension = importer.ExtensionName,
                Filters =
                {
                    new CommonFileDialogFilter(importer.DialogDisplayName, importer.ExtensionName)
                },
                EnsureFileExists = true,
                EnsurePathExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            string fileName = dialog.FileName;

            Mallow[] mallows;

            try
            {
                // mallows = JsonConvert.DeserializeObject<List<Mallow>>(File.ReadAllText(fileName));
                mallows = importer.Parse(File.ReadAllText(fileName));
                if (mallows is null) throw new Exception("未解析到数据。");
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"导入数据时发生错误：{exception.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK);

                return;
            }

            MallowGroup group = new MallowGroup
            {
                Name = $"{importer.Name}导入"
            };
            MallowSource.CurrentSource.Data.Insert(0, group);
            MallowSource.CurrentSource.Current = group;

            for (int i = 0; i < mallows.Length; i++)
                MallowSource.CurrentSource.Current.Mallows.Insert(i, mallows[i]);
        }
    }

    public sealed class ManagerImporter : IImporter
    {
        public string Name => "Manager";
        public string DialogDisplayName => "Manager导出文件";
        public string ExtensionName => ".json";
        public Mallow[] Parse(string rawData)
        {
            return JsonConvert.DeserializeObject<Mallow[]>(rawData);
        }
    }
}
