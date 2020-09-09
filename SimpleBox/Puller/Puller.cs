using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBox.Core;
using SimpleBox.Models;
using SimpleBox.Utils.State;

namespace SimpleBox.Puller
{
    public interface IMallowPuller : IMallowProvider
    {
        public Progress Progress { get; }
    }

    public static class PullHelper
    {
        public static void Pull(IMallowPuller puller)
        {

        }
    }
}
