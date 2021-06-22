using System.Collections;

namespace console.app.Interfaces
{
    interface IDatConvertor
    {
        IEnumerable GetDatFilesToConvert(string path);
    }
}
