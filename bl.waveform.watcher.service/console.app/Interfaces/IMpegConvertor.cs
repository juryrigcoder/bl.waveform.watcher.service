using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace console.app.Interfaces
{
    interface IMpegConvertor
    {
        IEnumerable GetMpegFilesToConvert(string path);
    }
}
