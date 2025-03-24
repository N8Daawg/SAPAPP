using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPAPP.Scripts
{
    internal abstract class Script
    {

        protected string workingDirectory;
        protected const bool testing = true;

        public Script()
        {
            workingDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        }

        public abstract void Download();

    }
}
