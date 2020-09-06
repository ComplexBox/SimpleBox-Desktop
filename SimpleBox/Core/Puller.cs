using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBox.Models;

namespace SimpleBox.Core
{
    public interface IMallowPuller : IMallowProvider
    {
        public Mallow[] Pull(UserPass userPass, int mode);
    }

    public static class PullHelper
    {

    }
}
