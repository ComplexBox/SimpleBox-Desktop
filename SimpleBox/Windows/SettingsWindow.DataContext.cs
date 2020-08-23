using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBox.Windows
{
    public partial class SettingsWindow
    {
        #region About

        public string AppVersion => $"版本 {Assembly.GetExecutingAssembly().GetName().Version}";

        #endregion
    }
}
