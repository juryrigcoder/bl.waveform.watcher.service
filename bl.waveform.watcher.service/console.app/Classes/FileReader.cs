using console.app.Interfaces;
using System;
using System.IO;

namespace console.app.Classes
{
    class FileReader : IFileReader
    {
        public string[] ReadFile<T>(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            string[] result = new string[0];
            try
            {
                using (StreamReader inputFile = new StreamReader(path))
                {
                    string content = inputFile.ReadToEnd();
                    result = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                }

            }
            catch (Exception ex)
            {

            }
            return result;
        }
    }
}
