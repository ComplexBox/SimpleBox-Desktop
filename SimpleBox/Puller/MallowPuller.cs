using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBox.Core;
using SimpleBox.Utils.State;

namespace SimpleBox.Puller
{
    public abstract class MallowPuller : IMallowProvider
    {
        #region User Data

        public string Name { get; } = "";

        public Progress Progress { get; } = new Progress();

        #endregion
    }
}
