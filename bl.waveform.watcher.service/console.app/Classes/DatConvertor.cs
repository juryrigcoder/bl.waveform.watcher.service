using CliWrap;
using console.app.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace console.app
{
    public class DatConvertor : IDatConvertor
    {
        private readonly IConfiguration _config;

        public DatConvertor(IConfiguration config)
        {
            _config = config;
        }

        public async Task ConvertDat()
        {
            ConcurrentQueue<FileInfo> MP3ToConvert = new ConcurrentQueue<FileInfo>((IEnumerable<FileInfo>)GetDatFilesToConvert(_config.GetValue<string>("DirectoryStructures:OutputFolder")));
            await ConvertToDat(MP3ToConvert);
        }

        public async Task ConvertDat(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            try
            {
                if (!File.Exists($"{_config.GetValue<string>("DirectoryStructures:DataFolder")}{fileName}.dat"))
                {
                    await Cli.Wrap(@".\bbc\audiowaveform.exe")
                .WithArguments(new[] { "-i", $"{_config.GetValue<string>("DirectoryStructures:OutputFolder")}{fileName}.mp3", "-o", $"{_config.GetValue<string>("DirectoryStructures:DataFolder")}{fileName}.dat", "-b", "8" })
                .WithWorkingDirectory($"{_config.GetValue<string>("DirectoryStructures:OutputFolder")}")
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex + Environment.NewLine);
            }
        }

        public async Task ConvertToDat(ConcurrentQueue<FileInfo> mp3FilesToConvert)
        {
            /*
             * https://github.com/bbc/audiowaveform#usage
             * audiowaveform -i filesource -o fileoutput -b 8 --pixels-per-second 100
             */
            while (mp3FilesToConvert.TryDequeue(out FileInfo fileToConvert))
            {
                try
                {
                    await Cli.Wrap(@".\bbc\audiowaveform.exe")
                    .WithArguments(new[] { "-i", $"{fileToConvert}", "-o", $"{_config.GetValue<string>("DirectoryStructures:DataFolder")}/{fileToConvert.Name}.dat", "-b", "8" })
                    .WithWorkingDirectory($"{_config.GetValue<string>("DirectoryStructures:OutputFolder")}")
                    .ExecuteAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public IEnumerable GetDatFilesToConvert(string path)
        {
            return new DirectoryInfo(path).GetFiles().Where(x => x.Extension == ".mp3");
        }

    }
}