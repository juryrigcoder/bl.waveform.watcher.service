using console.app.Domain;
using Microsoft.Extensions.Configuration;
using myoddweb.directorywatcher;
using System;
using System.IO;
using System.Threading.Tasks;

namespace console.app
{
    class App
    {
        private readonly IConfiguration _config;
        private readonly DatConvertor _dat;
        private readonly MpegConvertor _mpeg;

        public App(IConfiguration config, DatConvertor dat, MpegConvertor mpeg)
        {
            _config = config;
            _dat = dat;
            _mpeg = mpeg;
        }

        public async Task RunAsync(string[] args)
        {
            if (_config.GetValue<bool>("Ingest:Full") == true)
            {
                _mpeg.ConvertMpeg().Wait();
                await _dat.ConvertDat();
            }

            using (var watch = new Watcher())
            {
                watch.Add(new Request($"{_config.GetValue<string>("DirectoryStructures:InputFolder")}", false));
                watch.Add(new Request($"{_config.GetValue<string>("DirectoryStructures:OutputFolder")}", false));

                watch.OnAddedAsync += async (f, t) =>
                {
                    Console.Write(f.Name);

                    //await Task.WhenAll(_mpeg.ConvertMpeg(f.Name), _dat.ConvertDat(f.FullName));

                    if (f.FileSystemInfo.Exists)
                        _mpeg.ConvertMpeg(f.Name).Wait();


                    if (!Utility.IsFileLocked(new FileInfo(f.FullName)))
                        await _dat.ConvertDat(f.FullName);

                };
                watch.Start();

                Console.WriteLine("Press Ctrl+C to stop to exit.");
                Utility.WaitForCtrlC();

                watch.Stop();
            }

        }
    }
}
