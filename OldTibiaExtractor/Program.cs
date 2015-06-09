using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace OldTibiaExtractor
{
    class Program
    {
        static System.Drawing.Imaging.ColorPalette colorPalette;

        static void ExtractPIC(string filename)
        {
            using (System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead(filename))) {
                ushort width = (ushort)(reader.ReadUInt16() + 2);
                ushort height = (ushort)(reader.ReadUInt16() + 2);
                Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                bmp.Palette = colorPalette;
                System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                byte[] byteArray = new byte[width * height];

                for (int x = 1; x < (height - 1); x++) {
                    for (int y = 1; y < (width - 1); y++) {
                        byteArray[(width * x) + y] = reader.ReadByte();
                    }
                }

                System.Runtime.InteropServices.Marshal.Copy(byteArray, 0, bmpData.Scan0, width * height);
                bmp.UnlockBits(bmpData);
                bmp.Save(filename.Substring(0, filename.IndexOf(".")) + ".bmp");
                bmp.Dispose();
            }
        }

        static void ExtractSPR(string filename)
        {
            using (System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead("TIBIA.SPR"))) {
                ushort count = reader.ReadUInt16();
                reader.BaseStream.Seek(6, System.IO.SeekOrigin.Current);

                for (int a = 1; a < count; a++) {
                    Bitmap bmp = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    bmp.Palette = colorPalette;
                    System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, 32, 32), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                    byte[] byteArray = new byte[1024];

                    for (int b = 0; b < 1024; b++) {
                        byteArray[b] = 0xFF;
                    }

                    long size = reader.ReadUInt16();
                    size += reader.BaseStream.Position - 1;
                    int i = 0;

                    while (reader.BaseStream.Position <= size) {
                        if (reader.BaseStream.Position >= reader.BaseStream.Length) {
                            break;
                        }

                        ushort tPixels = reader.ReadUInt16();
                        i += tPixels;

                        if (reader.BaseStream.Position > size) {
                            break;
                        }

                        byte cPixels = reader.ReadByte();

                        for (int c = 0; c < cPixels; c++) {
                            byte color = reader.ReadByte();
                            byteArray[i] = color;
                            i++;
                        }
                    }

                    System.Runtime.InteropServices.Marshal.Copy(byteArray, 0, bmpData.Scan0, 1024);
                    bmp.UnlockBits(bmpData);
                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                    if (!System.IO.Directory.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filename), "Sprites\\"))) {
                        System.IO.Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filename), "Sprites\\"));
                    }

                    bmp.Save(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filename), "Sprites\\" + a.ToString() + ".bmp"));
                    bmp.Dispose();
                }
            }
        }

        static void SetPalette(string filename)
        {
            using (System.IO.BinaryReader palette = new System.IO.BinaryReader(System.IO.File.OpenRead(filename))) {
                for (int p = 0; p < 256; p++) {
                    colorPalette.Entries[p] = Color.FromArgb(palette.ReadByte(), palette.ReadByte(), palette.ReadByte());
                }
            }
        }

        static void Main(string[] args)
        {
            if (!System.IO.File.Exists("Palette.dat")) {
                Console.WriteLine("Palette.dat could not be found. Place Palette.dat in the same folder as OldTibiaExtractor and try again.");
                Console.ReadKey();
                return;
            }

            Bitmap bmp = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            colorPalette = bmp.Palette;
            SetPalette("Palette.dat");

            foreach (string filename in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))) {
                if (filename.Contains(".PIC")) {
                    ExtractPIC(filename);
                }

                if (filename.Contains(".SPR")) {
                    ExtractSPR(filename);
                }
            }

            bmp.Dispose();
        }
    }
}
