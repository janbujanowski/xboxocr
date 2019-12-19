using IronOcr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace XboxOcr
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"C:\Users\jan.bujanowski\Downloads\lol1contr2.jpg";
            string url = @"";
            bool rotate = false;
            // acceptable ironocr
            var Ocr = new AdvancedOcr()
            {
                AcceptedOcrCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.:;?!@$%&+-,-()\"'0123456789",
                CleanBackgroundNoise = false,
                ColorDepth = 0,
                ColorSpace = AdvancedOcr.OcrColorSpace.Color,
                EnhanceContrast = false,
                EnhanceResolution = false,
                Language = IronOcr.Languages.English.OcrLanguagePack,
                Strategy = IronOcr.AdvancedOcr.OcrStrategy.Advanced,
                DetectWhiteTextOnDarkBackgrounds = false,
                InputImageType = AdvancedOcr.InputTypes.Snippet,
                RotateAndStraighten = true,
                ReadBarCodes = false
            };
            List<Rectangle> captchaElementAreas = new List<Rectangle>();
            var stream = WebRequest.Create(url).GetResponse().GetResponseStream();
            var bitmap = new Bitmap(stream);
            bitmap = SetContrast(bitmap, 30);
            StringBuilder sb = new StringBuilder();
            if (rotate)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                captchaElementAreas.Add(new Rectangle(0, 0, 420, 210));
                captchaElementAreas.Add(new Rectangle(620, 0, 360, 240));
                captchaElementAreas.Add(new Rectangle(320, 230, 380, 170));
                captchaElementAreas.Add(new Rectangle(0, 380, 380, 200));
                captchaElementAreas.Add(new Rectangle(690, 410, 390, 190));
            }
            else
            {
                var startY = 450;
                var width = 460;
                var height = 200;
                captchaElementAreas.Add(new Rectangle(30, startY, width, height));
                captchaElementAreas.Add(new Rectangle(620, startY, width, height));
                captchaElementAreas.Add(new Rectangle(320, 670, width, height));
                captchaElementAreas.Add(new Rectangle(0, 870, width, height));
                captchaElementAreas.Add(new Rectangle(690, 880, width - 20, height - 20));
            }
          
            for (int i = 0; i < 5; i++)
            {
                
                    var Result = Ocr.Read(bitmap, captchaElementAreas[i]);
                    sb.Append(Result.Text +"-");
                    Console.WriteLine(Result.Text);
                
            }
            File.WriteAllText(@"C:\REPOS\XboxOcr\captchainput.txt", sb.ToString());
            Console.WriteLine("koniec");
            Console.ReadLine();
        }
        public static Bitmap SetContrast(Bitmap _currentBitmap, double contrast)
        {
            Bitmap temp = (Bitmap)_currentBitmap;
            Bitmap bmap = (Bitmap)temp.Clone();
            if (contrast < -100) contrast = -100;
            if (contrast > 100) contrast = 100;
            contrast = (100.0 + contrast) / 100.0;
            contrast *= contrast;
            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    double pR = c.R / 255.0;
                    pR -= 0.5;
                    pR *= contrast;
                    pR += 0.5;
                    pR *= 255;
                    if (pR < 0) pR = 0;
                    if (pR > 255) pR = 255;

                    double pG = c.G / 255.0;
                    pG -= 0.5;
                    pG *= contrast;
                    pG += 0.5;
                    pG *= 255;
                    if (pG < 0) pG = 0;
                    if (pG > 255) pG = 255;

                    double pB = c.B / 255.0;
                    pB -= 0.5;
                    pB *= contrast;
                    pB += 0.5;
                    pB *= 255;
                    if (pB < 0) pB = 0;
                    if (pB > 255) pB = 255;

                    bmap.SetPixel(i, j,
        Color.FromArgb((byte)pR, (byte)pG, (byte)pB));
                }
            }

            return (Bitmap)bmap.Clone();
        }
        public class TesseractService
        {
            private readonly string _tesseractExePath;
            private readonly string _language;

            /// <summary>
            /// Initializes a new instance of the <see cref="TesseractService"/> class.
            /// </summary>
            /// <param name="tesseractDir">The path for the Tesseract4 installation folder (C:\Program Files\Tesseract-OCR).</param>
            /// <param name="language">The language used to extract text from images (eng, por, etc)</param>
            /// <param name="dataDir">The data with the trained models (tessdata). Download the models from https://github.com/tesseract-ocr/tessdata_fast</param>
            public TesseractService(string tesseractDir, string language = "en", string dataDir = null)
            {
                // Tesseract configs.
                _tesseractExePath = Path.Combine(tesseractDir, "tesseract.exe");
                _language = language;

                if (String.IsNullOrEmpty(dataDir))
                    dataDir = Path.Combine(tesseractDir, "tessdata");

                Environment.SetEnvironmentVariable("TESSDATA_PREFIX", dataDir);
            }

            /// <summary>
            /// Read text from the images streams.
            /// </summary>
            /// <param name="images">The images streams.</param>
            /// <returns>The images text.</returns>
            public string GetText(params Stream[] images)
            {
                var output = string.Empty;

                if (images.Any())
                {
                    var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempPath);
                    var tempInputFile = NewTempFileName(tempPath);
                    var tempOutputFile = NewTempFileName(tempPath);

                    try
                    {
                        WriteInputFiles(images, tempPath, tempInputFile);

                        var info = new ProcessStartInfo
                        {
                            FileName = _tesseractExePath,
                            Arguments = $"{tempInputFile} {tempOutputFile} --oem 3 -l {_language}",
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true,
                            UseShellExecute = false
                        };

                        using (var ps = Process.Start(info))
                        {
                            ps.WaitForExit();

                            var exitCode = ps.ExitCode;

                            if (exitCode == 0)
                            {
                                output = File.ReadAllText(tempOutputFile + ".txt");
                            }
                            else
                            {
                                var stderr = ps.StandardError.ReadToEnd();
                                throw new InvalidOperationException(stderr);
                            }
                        }
                    }
                    finally
                    {
                        Directory.Delete(tempPath, true);
                    }
                }

                return output;
            }

            private static void WriteInputFiles(Stream[] inputStreams, string tempPath, string tempInputFile)
            {
                // If there is more thant one image file, so build the list file using the images as input files.
                if (inputStreams.Length > 1)
                {
                    var imagesListFileContent = new StringBuilder();

                    foreach (var inputStream in inputStreams)
                    {
                        var imageFile = NewTempFileName(tempPath);

                        using (var tempStream = File.OpenWrite(imageFile))
                        {
                            CopyStream(inputStream, tempStream);
                        }

                        imagesListFileContent.AppendLine(imageFile);
                    }

                    File.WriteAllText(tempInputFile, imagesListFileContent.ToString());
                }
                else
                {
                    // If is only one image file, than use the image file as input file.
                    using (var tempStream = File.OpenWrite(tempInputFile))
                    {
                        CopyStream(inputStreams.First(), tempStream);
                    }
                }
            }

            private static void CopyStream(Stream input, Stream output)
            {
                if (input.CanSeek)
                    input.Seek(0, SeekOrigin.Begin);

                input.CopyTo(output);
                input.Close();
            }

            private static string NewTempFileName(string tempPath)
            {
                return Path.Combine(tempPath, Guid.NewGuid().ToString());
            }
        }
    }
}
