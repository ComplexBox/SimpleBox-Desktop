using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBox.Models
{
    public sealed class Config
    {
        #region Current

        public static Config Current { get; set; }

        #endregion

        #region Update

        private string updateServer = "https://simplebox.vbox.moe/stable";

        public string UpdateServer
        {
            get => updateServer;
            set => updateServer = value;
        }

        #endregion
    }
}
