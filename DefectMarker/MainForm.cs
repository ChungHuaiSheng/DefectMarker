using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DefectMarker
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        List<string> Files = new List<string>();
        Marker Marker = new Marker();
        int showPicSN = 0;
        bool isDrawing = false;
        Point PointStart = Point.Empty;
        Bitmap CurrentImage;
        string MarkerFilePath;

        public void RefreshPicBox(string file)
        {
            if (pbMain.Image != null) { pbMain.Image.Dispose(); }
            using (FileStream fs = File.OpenRead(file))
            {
                pbMain.Image = Image.FromStream(fs);
            }
            //Check FileName record exist if no create new file
            if (!Marker.FileNameExist(Path.GetFileName(file)))
            {
                Marker.CreateMarkerFile(Path.GetFileName(file));
            }
            //Set Marker File as Current Data
            Marker.SetCurrentMarkerFile(Path.GetFileName(file));

            //Draw History Marker
            DrawMarkerList(Path.GetFileName(file));
        }

        private void DrawMarkerList(string FileName)
        {
            //Draw History Marker
            var list = Marker.ReadMarkerList(Path.GetFileName(FileName));
            if (list.Count() > 0)
            {
                CurrentImage = new Bitmap(pbMain.Image);
                Bitmap bt = (Bitmap)CurrentImage.Clone();
                Graphics g = Graphics.FromImage(bt);
                for (int i = 0; i < list.Count(); i++)
                {
                    g.DrawRectangle(new Pen(Color.Red, 1), new Rectangle()
                    {
                        X = list[i]._Xs,
                        Y = list[i]._Ys,
                        Width = list[i]._Xe - list[i]._Xs,
                        Height = list[i]._Ye - list[i]._Ys
                    });
                }
                pbMain.Image = bt;
            }
        }

        private void preImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showPicSN - 1 >= 0)
            {
                showPicSN--;
                RefreshPicBox(Files[showPicSN]);
            }
            else
            {
                MessageBox.Show("已經是第一張圖片");
            }
        }

        private void nextImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showPicSN + 1 < Files.Count())
            {
                showPicSN++;
                RefreshPicBox(Files[showPicSN]);
            }
            else
            {
                MessageBox.Show("已經是最後一張圖片");
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyValue.ToString())
            {
                //PageUp
                case "33":
                    preImageToolStripMenuItem_Click(this, e);
                    break;
                //PageDown
                case "34":
                    nextImageToolStripMenuItem_Click(this, e);
                    break;
                default:
                    break;
            }
        }

        private void loadSelectRawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Clear File List
            Files.Clear();
            //Get File List
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = @"E:\Hitachi Qtz\algoSample";
            ofd.Filter = "Png files (*.png)|*.png|Jpg files (*.jpg)|*.jpg";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (var fileName in ofd.FileNames)
                {
                    Files.Add(fileName);
                }
                showPicSN = 0;
                RefreshPicBox(Files[showPicSN]);
            }
        }

        private void pbMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (pbMain.Image != null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    isDrawing = true;
                    PointStart = new Point()
                    {
                        X = ContainerPosn2ImagePosition(new Point() { X = e.X, Y = e.Y }).X,
                        Y = ContainerPosn2ImagePosition(new Point() { X = e.X, Y = e.Y }).Y
                    };
                    //Save Current Image for Drawing
                    CurrentImage = new Bitmap(pbMain.Image);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    //Delete Last Marker List data
                    Marker.DeleteLastMarkerData(Path.GetFileName(Files[showPicSN]));
                    //Refresh PictureBox to match current Marker List
                    RefreshPicBox(Files[showPicSN]);
                }
            }
        }

        private void pbMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                Bitmap bt = (Bitmap)CurrentImage.Clone();
                Graphics g = Graphics.FromImage(bt);
                g.DrawRectangle(new Pen(Color.Red, 1), new Rectangle()
                {
                    X = PointStart.X,
                    Y = PointStart.Y,
                    Width = ContainerPosn2ImagePosition(new Point() { X = e.X, Y = e.Y }).X - PointStart.X,
                    Height = ContainerPosn2ImagePosition(new Point() { X = e.X, Y = e.Y }).Y - PointStart.Y
                });
                pbMain.Image = bt;
            }
        }

        private void pbMain_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isDrawing == true)
            {
                isDrawing = false;
                Marker.WriteMarkerData(Path.GetFileName(Files[showPicSN]),
                    PointStart.X,
                    PointStart.Y,
                    ContainerPosn2ImagePosition(new Point() { X = e.X, Y = e.Y }).X,
                    ContainerPosn2ImagePosition(new Point() { X = e.X, Y = e.Y }).Y);
            }
        }

        private void pbMain_Paint(object sender, PaintEventArgs e)
        {

        }

        //Transfer the Panel Position to Image Position
        private Point ContainerPosn2ImagePosition(Point p)
        {
            double[] Scale = new double[] { Convert.ToDouble(pbMain.Width) / Convert.ToDouble(pbMain.Image.Width), Convert.ToDouble(pbMain.Height) / Convert.ToDouble(pbMain.Image.Height) };
            double IW = pbMain.Image.Width * Scale.Min();
            double IH = pbMain.Image.Height * Scale.Min();

            Point r = new Point();
            if (IH < IW)
            {
                r.X = Convert.ToInt16(Math.Round((p.X - (pbMain.Width - IW) / 2) / Scale.Min()));
                r.Y = Convert.ToInt16(Math.Round(p.Y / Scale.Min()));
            }
            else if (IH < IW)
            {
                r.X = Convert.ToInt16(Math.Round(p.X / Scale.Min()));
                r.Y = Convert.ToInt16(Math.Round((p.Y - (pbMain.Height - IH) / 2) / Scale.Min()));
            }
            else if (IH == IW)
            {
                r.X = Convert.ToInt16(Math.Round(p.X / Scale.Min()));
                r.Y = Convert.ToInt16(Math.Round(p.Y / Scale.Min()));
            }
            return r;
        }

        private void saveMarkerFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "csv（*.csv）|*.csv";
            sfd.RestoreDirectory = true;
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                MarkerFilePath = sfd.FileName.ToString(); //獲得檔案路徑
                Marker.SaveMarkerFiles(MarkerFilePath);
            }
        }

        private void loadImageToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //Get File List
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = @"E:\Hitachi Qtz\algoSample";
            ofd.Filter = "csv (*.csv)|*.csv";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                MarkerFilePath = ofd.FileName.ToString();
                Marker.LoadMarkerFiles(MarkerFilePath);
                if (Files.Count > 0)
                {
                    RefreshPicBox(Files[showPicSN]);
                }
            }
        }
    }
}