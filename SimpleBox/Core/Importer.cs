﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleBox.Models;

namespace SimpleBox.Core
{
    public interface IMallowProvider
    {
        public string Name { get; }
    }

    public interface IMallowImporter : IMallowProvider
    {
        public string ExtensionName { get; }
        public Mallow[] Parse(string rawData);
    }

    public static class ImportExportHelper
    {
        public static void Import(IMallowImporter importer)
        {
            if (importer is null) return;

            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Title = $"选择{importer.Name}导出文件",
                DefaultDirectory = Environment.CurrentDirectory,
                IsFolderPicker = false,
                DefaultExtension = importer.ExtensionName,
                Filters =
                {
                    new CommonFileDialogFilter($"{importer.Name}文件", importer.ExtensionName)
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

            Import(importer, mallows);
        }

        public static void Import(IMallowProvider mallowProvider, Mallow[] mallows)
        {
            MallowGroup group = new MallowGroup
            {
                Name = $"{mallowProvider.Name}导入"
            };
            MallowSource.CurrentSource.Data.Insert(0, group);
            MallowSource.CurrentSource.Current = group;

            for (int i = 0; i < mallows.Length; i++)
                MallowSource.CurrentSource.Current.Mallows.Insert(i, mallows[i]);
        }

        public static void Export(List<Mallow> mallows)
        {
            CommonSaveFileDialog dialog = new CommonSaveFileDialog
            {
                Title = "导出",
                DefaultDirectory = Environment.CurrentDirectory,
                IsExpandedMode = true,
                DefaultExtension = ".json",
                Filters =
                {
                    new CommonFileDialogFilter("SimpleBox导出文件", ".json")
                },
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureValidNames = true,
                DefaultFileName = "SimpleBox导出.json"
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            string fileName = dialog.FileName;

            string rawData = JsonConvert.SerializeObject(mallows);

            File.WriteAllText(fileName, rawData);

        }
    }

    public sealed class SimpleBoxImporter : IMallowImporter
    {
        public string Name => "SimpleBox";
        public string ExtensionName => ".json";
        public Mallow[] Parse(string rawData) => JsonConvert.DeserializeObject<Mallow[]>(rawData);
    }

    public sealed class MarsherImporter : IMallowImporter
    {
        public string Name => "Marsher";
        public string ExtensionName => ".marsher";
        public Mallow[] Parse(string rawData)
        {
            JArray data = (JArray) JObject.Parse(rawData)["items"];
            if (data is null) return Array.Empty<Mallow>();

            List<Mallow> mallows = new List<Mallow>();
            foreach (JToken rawItem in data)
            {
                Mallow mallow = new Mallow
                {
                    OriginalMessage = (string) rawItem["content"]
                };
                mallow.SetValuesOnDeserialized(new StreamingContext());
                mallows.Add(mallow);
            }

            return mallows.ToArray();
        }
    }
}
