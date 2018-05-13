using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GenCert
{
    // NAPOMENA:
    // Koriste se ikone sa sajta https://materialdesignicons.com/ i tu se pokupi:
    // Data = Geometry.Parse("M4,3C2.89,3 2,3.89 2,5V15A2,2 0 0,0 4,17H12V22L15,19L18,22V17H20A2,2 0 0,0 22,15V8L22,6V5A2,2 0 0,0 20,3H16V3H4M12,5L15,7L18,5V8.5L21,10L18,11.5V15L15,13L12,15V11.5L9,10L12,8.5V5M4,5H9V7H4V5M4,9H7V11H4V9M4,13H9V15H4V13Z"),
    // odatle se snimi export iste ikone kao .png
    // onda se koristi sajt http://icoconvert.com/ gde se uradi upload .png fajla i odatle snimi .ico fajl koji se koristi u programu tj veze se za application project
    // Propreties->application->Icon and manifest
    // CodePlex -> https://www.codeproject.com/Tips/1089489/Vector-Icons-in-WPF
    // Material Design Icons    ->https://materialdesignicons.com/
    // Fontastic                ->http://fontastic.me/
    // Streamline Icons         ->http://www.streamlineicons.com/free-icons.html
    // Kameleon                 ->http://www.kameleon.pics/free-icons-pack.html
    // Icomoon                  ->https://icomoon.io/
    //
    //Programmatically change Taskbar icon in WPF
    //http://www.dignaj.com/change-taskbar-icon-in-wpf/
    //
    public class IconMaker
    {
        public static BitmapImage Icon(System.Windows.Media.Color color)
        {
            /*
            <?xml version="1.0" encoding="utf-8"?>
            <Canvas xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" x:Name="appbar_graph_bar" Width="76" Height="76" Clip="F1 M 0,0L 76,0L 76,76L 0,76L 0,0">
	            <Path Width="42" Height="38" Canvas.Left="17" Canvas.Top="19" Stretch="Fill" Fill="#FF000000" Data="F1 M 22,52L 22,35L 30,35L 30,52L 22,52 Z M 32,52L 32,22L 39,22L 39,52L 32,52 Z M 41,52L 41,41L 49,41L 49,52L 41,52 Z M 51,52L 51,29L 59,29L 59,52L 51,52 Z M 17,19L 20,19L 20,54L 59,54L 59,57L 17,57L 17,19 Z "/>
            </Canvas>


            http://modernuiicons.com/

            <?xml version="1.0" encoding="utf-8"?>
            <Canvas xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" x:Name="appbar_social_aws" Width="76" Height="76" Clip="F1 M 0,0L 76,0L 76,76L 0,76L 0,0">
	            <Path Width="37.9996" Height="36.7333" Canvas.Left="19.025" Canvas.Top="20.0616" Stretch="Fill" Fill="#FF000000" Data="F1 M 38.0081,29.8002L 53.5133,24.7558L 38.0081,20.0616L 22.1469,24.7558M 40.077,56.795L 57.0245,51.0045L 57.0245,27.2965L 40.1327,32.5388M 19.025,27.1398L 35.9134,32.617L 35.9134,56.7162L 19.1348,51.0045"/>
            </Canvas>

            <?xml version="1.0" encoding="utf-8"?>
            <Canvas xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" x:Name="appbar_tiles_add" Width="76" Height="76" Clip="F1 M 0,0L 76,0L 76,76L 0,76L 0,0">
	            <Path Width="40" Height="40" Canvas.Left="18" Canvas.Top="18" Stretch="Fill" Fill="#FF000000" Data="F1 M 18,40L 36,40L 36,58L 18,58L 18,40 Z M 40,58L 40,40L 58,40L 58,58L 40,58 Z M 40,36L 40,18L 58,18L 58,36L 40,36 Z M 29,36L 25,36L 25,29L 18,29L 18,25L 25,25L 25,18L 29,18L 29,25L 36,25L 36,29L 29,29L 29,36 Z "/>
            </Canvas>
            */

            var canvas = new Canvas
            {
                Width = 36,
                Height = 36,
                Background = new SolidColorBrush(Colors.Transparent)
            };
            //var canvas = new Canvas
            //{
            //    Width = 24,
            //    Height = 24,
            //    Background = new SolidColorBrush(Colors.Transparent)
            //};

            //var path = new System.Windows.Shapes.Path()
            //{
            //    //Data = Geometry.Parse("F1 M 22,52L 22,35L 30,35L 30,52L 22,52 Z M 32,52L 32,22L 39,22L 39,52L 32,52 Z M 41,52L 41,41L 49,41L 49,52L 41,52 Z M 51,52L 51,29L 59,29L 59,52L 51,52 Z M 17,19L 20,19L 20,54L 59,54L 59,57L 17,57L 17,19 Z "),
            //    //Data = Geometry.Parse("F1 M 38.0081,29.8002L 53.5133,24.7558L 38.0081,20.0616L 22.1469,24.7558M 40.077,56.795L 57.0245,51.0045L 57.0245,27.2965L 40.1327,32.5388M 19.025,27.1398L 35.9134,32.617L 35.9134,56.7162L 19.1348,51.0045"),
            //    Data = Geometry.Parse("F1 M 18,40L 36,40L 36,58L 18,58L 18,40 Z M 40,58L 40,40L 58,40L 58,58L 40,58 Z M 40,36L 40,18L 58,18L 58,36L 40,36 Z M 29,36L 25,36L 25,29L 18,29L 18,25L 25,25L 25,18L 29,18L 29,25L 36,25L 36,29L 29,29L 29,36 Z "),
            //    Stretch = Stretch.Fill,
            //    Fill = new SolidColorBrush(color),
            //    Width = 36,
            //    Height = 36,
            //};
            var path = new System.Windows.Shapes.Path()
            {
                //Data = Geometry.Parse("F1 M 22,52L 22,35L 30,35L 30,52L 22,52 Z M 32,52L 32,22L 39,22L 39,52L 32,52 Z M 41,52L 41,41L 49,41L 49,52L 41,52 Z M 51,52L 51,29L 59,29L 59,52L 51,52 Z M 17,19L 20,19L 20,54L 59,54L 59,57L 17,57L 17,19 Z "),
                //Data = Geometry.Parse("F1 M 38.0081,29.8002L 53.5133,24.7558L 38.0081,20.0616L 22.1469,24.7558M 40.077,56.795L 57.0245,51.0045L 57.0245,27.2965L 40.1327,32.5388M 19.025,27.1398L 35.9134,32.617L 35.9134,56.7162L 19.1348,51.0045"),
                //Data = Geometry.Parse("F1 M 18,40L 36,40L 36,58L 18,58L 18,40 Z M 40,58L 40,40L 58,40L 58,58L 40,58 Z M 40,36L 40,18L 58,18L 58,36L 40,36 Z M 29,36L 25,36L 25,29L 18,29L 18,25L 25,25L 25,18L 29,18L 29,25L 36,25L 36,29L 29,29L 29,36 Z "),
                Data = Geometry.Parse("M4,3C2.89,3 2,3.89 2,5V15A2,2 0 0,0 4,17H12V22L15,19L18,22V17H20A2,2 0 0,0 22,15V8L22,6V5A2,2 0 0,0 20,3H16V3H4M12,5L15,7L18,5V8.5L21,10L18,11.5V15L15,13L12,15V11.5L9,10L12,8.5V5M4,5H9V7H4V5M4,9H7V11H4V9M4,13H9V15H4V13Z"),
                Stretch = Stretch.Fill,
                Fill = new SolidColorBrush(color),
                Width = 36,
                Height = 36,
            };
            canvas.Children.Add(path);

            var size = new System.Windows.Size(36, 36);
            canvas.Measure(size);
            canvas.Arrange(new Rect(size));

            var rtb = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(canvas);

            var png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb));

            using (var memory = new MemoryStream())
            {
                png.Save(memory);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}
