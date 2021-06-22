using console.app.Interfaces;
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

namespace console.app
{
    public class MpegConvertor : IMpegConvertor
    {
        private readonly IConfiguration _config;

        public MpegConvertor(IConfiguration config)
        {
            _config = config;
        }

        public async Task ConvertMpeg()
        {
            ConcurrentQueue<FileInfo> MP4FilesToConvert = new ConcurrentQueue<FileInfo>((IEnumerable<FileInfo>)GetMpegFilesToConvert(_config.GetValue<string>("DirectoryStructures:InputFolder")));
            FFmpeg.SetExecutablesPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
            await RunMpegConversion(MP4FilesToConvert);
        }

        public async Task ConvertMpeg(string fileName)
        {
            string output = Path.ChangeExtension($"{_config.GetValue<string>("DirectoryStructures:OutputFolder")}{fileName}", "mp3");
            FFmpeg.SetExecutablesPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
            try
            {
                if (!File.Exists(output))
                {
                    var conversion = await FFmpeg.Conversions.FromSnippet.ExtractAudio($"{_config.GetValue<string>("DirectoryStructures:InputFolder")}{fileName}", output);
                    await conversion.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception at convert to mpeg from folder watcher: {ex.Message}" + Environment.NewLine);
            }
        }

        public IEnumerable GetMpegFilesToConvert(string path)
        {
            return new DirectoryInfo(path).GetFiles().Where(x => x.Extension == ".mp4");
        }

        private async Task RunMpegConversion(ConcurrentQueue<FileInfo> mp4FilesToConvert)
        {
            while (mp4FilesToConvert.TryDequeue(out FileInfo fileToConvert))
            {
                string output = Path.ChangeExtension($"{_config.GetValue<string>("DirectoryStructures:OutputFolder")}{fileToConvert.Name}", "mp3");
                Console.WriteLine(output);
                var conversion = await FFmpeg.Conversions.FromSnippet.ExtractAudio(fileToConvert.FullName, output);
                await conversion.Start();
            }
        }
    }
}