using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using File = TagLib.File;

namespace musicCollectionConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    encoder = "oggenc2.exe";
                    break;
                case PlatformID.Unix:
                    encoder = "oggenc";
                    break;
                default:
                    Console.WriteLine("I'm sorry, but I don't support {0}", Environment.OSVersion.Platform);
                    return;
            }

            if (args.Length != 2)
            {
                Console.WriteLine("usage: musicCollectionConverter.exe indir outdir");
                return;
            }

            pathSeperator = Path.DirectorySeparatorChar.ToString();
            EnsureDirectoryExists(new DirectoryInfo(args[1]));
            HandleDirectory(new DirectoryInfo(args[0]), new DirectoryInfo(args[1]));
        }

        private static string encoder = "oggenc.exe";
        private static string pathSeperator;

        static void HandleDirectory(DirectoryInfo indir,DirectoryInfo outdir)
        {
            foreach (DirectoryInfo subdir in indir.GetDirectories())
            {
                //if (!subdir.Name.StartsWith("0-"))
                {
                    HandleDirectory(subdir, outdir);
                }
            }
            foreach (FileInfo fi in indir.GetFiles("*.flac"))
            {
                string fullname = fi.FullName;
                if (fullname.Length > 260)
                {
                    Console.WriteLine("Filename too long: " + fullname);
                    continue;
                }
                File f = File.Create(fi.FullName);
                if (string.IsNullOrEmpty(f.Tag.Album))
                    continue;

                string ogAlbum = f.Tag.Album;
                string ogTitle = f.Tag.Title;
                string album = ogAlbum.Replace(':', '\uFF1A').Replace('\"', '\uFF02').Replace('/', '\uFF0F').Replace('?', '\uFF1F').Replace('|','\uFF5C').Replace('/', '\uFF0F').Trim();
                string title = ogTitle.Replace(':', '\uFF1A').Replace('\"', '\uFF02').Replace('/', '\uFF0F').Replace('?', '\uFF1F').Replace('|','\uFF5C').Replace('/', '\uFF0F').Trim();
                uint disc = f.Tag.Disc;
                uint trackNo = f.Tag.Track;

                if (album.StartsWith(".")) 
                    album = album.Replace('.', '．');

                DirectoryInfo albumOutDir = new DirectoryInfo(Path.Combine(outdir.FullName, album + pathSeperator));
                EnsureDirectoryExists(albumOutDir);
                FileInfo coverOutFileInfo = new FileInfo(Path.Combine(albumOutDir.FullName, "folder.jpg"));

                if (!coverOutFileInfo.Exists)
                {
                    var pictures = f.Tag.Pictures;
                    if (pictures.Length > 0)
                    {
                        Console.WriteLine("Writing album art for: {0}", album);
                        var picture = pictures[0];
                        MemoryStream ms = new MemoryStream(picture.Data.Data);
                        Bitmap bmp = new Bitmap(ms);
                        bmp.Save(coverOutFileInfo.FullName, ImageFormat.Jpeg);
                        bmp.Dispose();
                        ms.Dispose();
                    }
                }
                f.Dispose();

                string outFileName = string.Format("{0}{1}{2:00} {3}.ogg", albumOutDir,disc != 0 ? disc.ToString() + "." : "", trackNo, title);
                outFileName = outFileName.Replace('\"', '\uFF02');
                outFileName = outFileName.Replace('?',  '\uFF1F');
                outFileName = outFileName.Replace('<',  '\uFF1C');
                outFileName = outFileName.Replace('>',  '\uFF1E');
                outFileName = outFileName.Replace('*',  '\uFF0A');

                FileInfo outFileInfo = new FileInfo(outFileName);
                if (!outFileInfo.Exists)
                {
                    if (!outFileInfo.Directory.Exists)
                    {
                        EnsureDirectoryExists(outFileInfo.Directory);
                    }

                    string oggencSwitches = null;
                    string oggencSwitchesFilename = Path.Combine(indir.FullName, "oggenc2.conf");
                    if (System.IO.File.Exists(oggencSwitchesFilename))
                    {
                        oggencSwitches = System.IO.File.ReadAllText(oggencSwitchesFilename);
                    }
                    Console.WriteLine("Encoding:" + outFileInfo.Name);
                    Process p = new Process();
                    while (outFileName.Length > 255)
                    {
                        outFileName = title.Substring(0, title.Length - 1);
                        outFileName = string.Format("{0}{1}{2:00} {3}.ogg", albumOutDir, disc != 0 ? disc.ToString() + "." : "", trackNo, title);
                    }
                    p.StartInfo.Arguments = string.Format("{2} \"{0}\" -o \"{1}\"", fi.FullName, outFileName, oggencSwitches);
                    p.StartInfo.FileName = encoder;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.Start();
                    p.WaitForExit();
                    outFileInfo.Refresh();
                    if (!outFileInfo.Exists)
                    {
                        title = string.Format("Track {0}", trackNo);
                        outFileName = string.Format("{0}{1}{2:00} {3}.ogg", albumOutDir, disc != 0 ? disc.ToString() + "." : "", trackNo, title);
                        if (!new FileInfo(outFileName).Exists)
                        {
                            p.StartInfo.Arguments = string.Format("{2} \"{0}\" -o \"{1}\"", fi.FullName, outFileName,oggencSwitches);
                            p.Start();
                            p.WaitForExit();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Skipping:" + outFileInfo.Name);
                }
            }
        }

        static void EnsureDirectoryExists(DirectoryInfo di)
        {
            if (!di.Exists)
            {
                EnsureDirectoryExists(di.Parent);
                Console.WriteLine("Creating: {0}", di.Name);
                di.Create();
            }
        }
    }
}
