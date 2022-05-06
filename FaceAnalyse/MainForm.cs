using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Graphic;
using Model;
using Geometry;
using FaceRecognitionDotNet;

namespace FaceAnalyse
{
    public partial class MainForm : Form
    {
        private GraphicGL GL1 = new GraphicGL();
        
        public MainForm()
        {
            InitializeComponent();
            GL1.lightXscroll(trackBar_X_L.Value);
            GL1.lightYscroll(trackBar_Y_L.Value);
            GL1.lightZscroll(trackBar_Z_L.Value);
            glControl1.MouseWheel += GL1.Form1_mousewheel; 
        }
        void Init()
        {
            var model = new Model3d(@"faces/Model.obj");
            var cube2 = new Model3d(@"faces/cube2.obj");
            GL1.addOBJ(model.mesh, model.normale, model.texture,0.01f);

            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(10, 0, 0), new Point3d_GL(0, 10, 0), new Point3d_GL(0, 0, 10));
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(-10, 0, 0), new Point3d_GL(0, -10, 0), new Point3d_GL(0, 0, -10));
        }

        #region gl_control
        private void glControl1_ContextCreated(object sender, GlControlEventArgs e)
        {
            var pict = new Mat(@"faces/Model.jpg");
            //CvInvoke.Resize(pict, pict, new Size(3900, 3400));
            CvInvoke.Resize(pict, pict, new System.Drawing.Size(800, 800));
            GL1.pict = pict;
            detectingFace(pict);
            imageBox1.Image = pict;
            GL1.glControl_ContextCreated(sender, e);
            var send = (Control)sender;
            var w = send.Width;
            var h = send.Height;
            GL1.addMonitor(new System.Drawing.Rectangle(0, 0, w, h), 0);
            Init();
            GL1.SortObj();
            // pictureBox1.Image = GL1.bmp;
            //GL1.printDebug(richTextBox1);
        }

        private void glControl1_Render(object sender, GlControlEventArgs e)
        {
            GL1.glControl_Render(sender, e);           
        }

        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            GL1.glControl_MouseDown(sender, e);
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {

            GL1.glControl_MouseMove(sender, e);

        }
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {

            var g = glControl1.CreateGraphics();
            System.Drawing.Pen pen1 = new System.Drawing.Pen(System.Drawing.Color.Black);
            pen1.Width = 2;

            //g.Clear(Color.White);
            g.DrawRectangle(pen1, 0, 0, 200, 100);

            glControl1.Update();
        }
        #endregion

        #region track
        private void trackBar_X_L_Scroll(object sender, EventArgs e)
        {
            GL1.lightXscroll(trackBar_X_L.Value);
        }

        private void trackBar_Y_L_Scroll(object sender, EventArgs e)
        {
            GL1.lightYscroll(trackBar_Y_L.Value);
        }

        private void trackBar_Z_L_Scroll(object sender, EventArgs e)
        {
            GL1.lightZscroll(trackBar_Z_L.Value);
        }
        #endregion

        private void but_textureVision_Click(object sender, EventArgs e)
        {
            if(GL1.textureVis == 0)
            {
                GL1.textureVis = 1;
                but_textureVision.Text = "Убрать текстуру";
            }
            else if (GL1.textureVis == 1)
            {
                GL1.textureVis = 0;
                but_textureVision.Text = "Отобразить текстуру";
            }
        }

        private void but_light_Vision_Click(object sender, EventArgs e)
        {
            if (GL1.lightVis == 0)
            {
                GL1.lightVis = 1;
                but_light.Text = "Убрать освещение";
            }
            else if (GL1.lightVis == 1)
            {
                GL1.lightVis = 0;
                but_light.Text = "Отобразить освещение";
            }
        }

        private void imageBox1_Move(object sender, MouseEventArgs e)
        {
            var cont = (Control)sender;
            GL1.MouseLoc.x = (float)e.X/(float)cont.Width;
            GL1.MouseLoc.y = 1 - (float)e.Y / (float)cont.Height;
        }

        Point3d_GL[][] find3DPointsFromTex(PointF[][] points, Model3d model)
        {
            var landm = new List<Point3d_GL[]>();
            for(int i=0; i<points.Length; i++)
            {
                var p3d = new List<Point3d_GL>();
                for (int j = 0; j < points[i].Length; j++)
                {
                    p3d.Add(model.take3dfrom2d(points[i][j]));
                }
                landm.Add(p3d.ToArray());
            }
            return landm.ToArray();
        }

        PointF[][] detectingFace(Mat mat_face)
        {
            var imface = faceImageFromMat(mat_face);
            var fr = FaceRecognition.Create(@"dlib_models");
            var locs =  fr.FaceLocations(imface).ToArray();            
            var lands = fr.FaceLandmark(imface).ToArray();
            var landm = new List<PointF[]>();
            for(int i=0; i<locs.Length;i++)
            {
                CvInvoke.Rectangle(mat_face, new System.Drawing.Rectangle(locs[i].Left, locs[i].Top, locs[i].Right- locs[i].Left, locs[i].Bottom), new MCvScalar(255, 0, 0),2);                
                
                foreach (FacePart face_tp in Enum.GetValues(typeof(FacePart)))
                {
                    var psLand = new List<PointF>();
                    var color = new MCvScalar(0, 0, 255);
                    if(face_tp == FacePart.LeftEye)
                    {
                        color = new MCvScalar(255, 0, 0);
                    }
                    var points = new List<FacePoint>();
                    var en = points.AsEnumerable();
                    lands[i].TryGetValue(face_tp, out en);
                    if(en != null)
                    {
                        var ps = en.ToArray();
                        for (int j = 0; j < ps.Length; j++)
                        {
                            var pf = new System.Drawing.Point(ps[j].Point.X, ps[j].Point.Y);
                            CvInvoke.Circle(mat_face, pf, 2, color, 2);
                            psLand.Add(new PointF(ps[j].Point.X, ps[j].Point.Y));
                        }
                        landm.Add(psLand.ToArray());
                    }                    
                }                    
            }
            return landm.ToArray();
        }
        FaceRecognitionDotNet.Image faceImageFromMat(Mat mat)
        {
            return FaceRecognition.LoadImage(mat.ToBitmap());
        }

        private void glControl1_ContextDestroying(object sender, GlControlEventArgs e)
        {
            GL1.glControl_ContextDestroying(sender, e);
        }
    }
}
