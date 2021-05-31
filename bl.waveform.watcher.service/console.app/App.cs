using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using CliWrap;
using myoddweb.directorywatcher;

namespace console.app
{
    class App
    {
        private readonly IConfiguration _config;

        public App(IConfiguration config)
        {
            _config = config;

        }

        public void Run(string[] args)
        {
            if(_config.GetValue<bool>("Ingest:Full") == true)
            {
            ConvertMpeg().Wait();
            ConvertDat().Wait();
            }

            using (var watch = new Watcher())
            {
                watch.Add(new Request($"{_config.GetValue<string>("DirectoryStructures:InsputFolder")}", true));
                watch.OnAddedAsync += async (f, t) =>
                {
                    await ConvertMpeg();
                };
            }
        }

        private async Task ConvertMpeg()
        {
            ConcurrentQueue<FileInfo> MP4FilesToConvert = new ConcurrentQueue<FileInfo>((IEnumerable<FileInfo>)GetMpegFilesToConvert("path to files"));
            FFmpeg.SetExecutablesPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
            await RunMpegConversion(MP4FilesToConvert);
        }

        private async Task ConvertDat() {
            ConcurrentQueue<FileInfo> MP3ToConvert = new ConcurrentQueue<FileInfo>((IEnumerable<FileInfo>)GetDatFilesToConvert("path to files"));

            await ConvertToDat(MP3ToConvert);
        }

        private IEnumerable GetMpegFilesToConvert(string path)
        {
            return new DirectoryInfo(path).GetFiles().Where(x => x.Extension == "mp4");
        }

        private IEnumerable GetDatFilesToConvert(string path)
        {
            return new DirectoryInfo(path).GetFiles().Where(x => x.Extension == "mp3");
        }

        private async Task RunMpegConversion(ConcurrentQueue<FileInfo> mp4FilesToConvert)
        {
            while (mp4FilesToConvert.TryDequeue(out FileInfo fileToConvert))
            {
                string output = Path.ChangeExtension(Path.GetTempFileName(), ".mp3");
                var conversion = await FFmpeg.Conversions.FromSnippet.ExtractAudio(fileToConvert.FullName, output);
                await conversion.Start();
            }
        }

        private async Task ConvertToDat(ConcurrentQueue<FileInfo> mp3FilesToConvert)
        {
            /*
             * https://github.com/bbc/audiowaveform#usage
             * audiowaveform -i filesource -o fileoutput -b 8 --pixels-per-second 100
             */
            while (mp3FilesToConvert.TryDequeue(out FileInfo fileToConvert))
            {
                await Cli.Wrap("path/to/exe")
                .WithArguments(new[] { "-i", $"{_config.GetValue<string>("DirectoryStructures:OutputFolder")}/{fileToConvert.FullName}", "-o", $"{_config.GetValue<string>("DirectoryStructures:OutputFolder")}/{fileToConvert.FullName}.dat", "-b", "8" })
                .WithWorkingDirectory($"{_config.GetValue<string>("DirectoryStructures:OutputFolder")}")
                .ExecuteAsync();
            }
        }
    }
}
