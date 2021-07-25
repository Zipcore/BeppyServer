#if LINUX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeppyServer.Native
{
    class LinuxConsole : SystemConsole
    {
        // We don't know what the object is yet.
        public override void Log(BeppyLogType level, string message)
        {
            SdtdConsole.print(message);
        }

        public override void Translate(BeppyLogType type, ref string message)
        {
            throw new NotImplementedException();
        }
    }
}
#endif