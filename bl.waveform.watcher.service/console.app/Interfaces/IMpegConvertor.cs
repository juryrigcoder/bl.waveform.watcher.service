using System.Collections;

namespace console.app.Interfaces
{
    interface IMpegConvertor
    {
        IEnumerable GetMpegFilesToConvert(string path);
    }
}
