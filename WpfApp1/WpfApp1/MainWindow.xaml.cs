using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    public enum DrawMode
    {
        NONE,
        DRAW,
        GMODE
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DrawMode drawMode = DrawMode.NONE;

        public bool fileLoaded = false;
        private Scribe scribe = new Scribe();

        public MainWindow()
        {
            ComponentDispatcher.ThreadIdle += new System.EventHandler(ComponentDispatcher_ThreadIdle);

            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if(this.drawMode == DrawMode.DRAW)
                {
                    this.drawMode = DrawMode.NONE; //cancel drawing.
                    
                    //clean up
                }
            }
        }

        void ComponentDispatcher_ThreadIdle(object sender, EventArgs e)
        {
            Point pos = Mouse.GetPosition(this.Cnv);

            //update debug display
            this.lbl_MousePos.Content = "X: " + pos.X.ToString() + ", Y:" + pos.Y.ToString();

            //RENDER  AREA//

            if (this.drawMode == DrawMode.DRAW)
            {
                this.Cnv.Children.Clear();

                //paint all established lines for this draw
                scribe.DrawCurrentStrip(this.Cnv);

                //paint the temp. point-mouse cursor line
                scribe.DrawActiveLine(this.Cnv);

                //paint pre-existing work
                //scribe.DrawExistingStrips(this.Cnv);

                scribe.DrawAllPolygons(this.Cnv);
            }

            if (this.drawMode == DrawMode.NONE)
            {
                this.Cnv.Children.Clear();

                //scribe.DrawExistingStrips(this.Cnv);

                scribe.DrawAllPolygons(this.Cnv);
            }

        }

        public void cmd_OpenImageFile(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                Uri uri = new Uri(filename);
                BitmapImage bmp = new BitmapImage(uri);
                this.Overlay.Source = bmp;

            }
        }

        public void Clicked_grid_Main(object sender, MouseEventArgs e)
        {
            //TODO: disabled for testing
            //Enable this to force the user to load an image first
            //if(this.fileLoaded == false)
            //{
            //    return;
            //}

            //main activity switch
            switch (this.drawMode)
            {
                case DrawMode.NONE:

                    this.drawMode = DrawMode.DRAW;

                    scribe.PlaceInitialPoint();

                    break;

                case DrawMode.DRAW:

                    //if strip is done, finalize it.
                    //check to see if the current and the first are near:
                    if (scribe.CheckSnap())
                    {
                        scribe.PlaceFinalPoint(Cnv);

                        //stop drawing
                        this.drawMode = DrawMode.NONE;
                    }
                    else
                    {
                        //otherwise, add a new point
                        scribe.PlacePoint();
                    }

                    break;

                case DrawMode.GMODE:

                    Point point = e.GetPosition(this.Cnv);

                    scribe.CheckPolygons(point);

                    break;

            }
        }

        //update display and the scribe's registers
        private void grid_Main_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(this.Cnv);

            this.scribe.xRegister = (int)point.X;
            this.scribe.yRegister = (int)point.Y;
            this.lbl_MousePos.Content = "X: " + point.X.ToString() + ", Y:" + point.Y.ToString();
        }

        private void cmd_Exit(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void cmb_ClearAll(object sender, RoutedEventArgs e)
        {
            this.drawMode = DrawMode.NONE;
            scribe.ClearAllArtifacts();
        }

        private void cmd_ToggleBorders(object sender, RoutedEventArgs e)
        {
            scribe.ToggleBorders();
        }

        private void cmd_ForceBorders(object sender, RoutedEventArgs e)
        {
            scribe.ToggleForceBorders();

            if(scribe.ForceBorders)
            {
                this.lbl_ForceBorders.Visibility = Visibility.Visible;
            }
            else
            {
                this.lbl_ForceBorders.Visibility = Visibility.Hidden;
            }
        }

        private void cmd_GMode(object sender, RoutedEventArgs e)
        {
            if(this.drawMode == DrawMode.GMODE)
            {
                this.drawMode = DrawMode.NONE;
            }
            else
            {
                this.drawMode = DrawMode.GMODE;
            }
        }
    }
}
