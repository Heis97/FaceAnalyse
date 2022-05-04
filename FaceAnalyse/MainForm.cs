using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
           // GL1.addGLMesh(model.mesh, PrimitiveType.Triangles);
            //GL1.addSTL(model.mesh, PrimitiveType.Triangles, new Point3d_GL(0, 0, 0), new Point3d_GL(0, 0, 0), 0.01f);
            //GL1.addSTL(cube2.mesh, PrimitiveType.Triangles, new Point3d_GL(0, 0, 0), new Point3d_GL(0, 0, 0), 0.1f);
            //GL1.addSTL(cube2.mesh,PrimitiveType.Triangles,new Point3d_GL(0,0,0),new Point3d_GL(0,0,0));
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(10, 0, 0), new Point3d_GL(0, 10, 0), new Point3d_GL(0, 0, 10));
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(-10, 0, 0), new Point3d_GL(0, -10, 0), new Point3d_GL(0, 0, -10));
        }


        #region gl_control
        private void glControl1_ContextCreated(object sender, GlControlEventArgs e)
        {
            var pict = new Mat(@"faces/Model.jpg");
            CvInvoke.Resize(pict, pict, new Size(800, 800));

            GL1.pict = pict;
            //detectingFace(pict);
            imageBox1.Image = pict;
            GL1.glControl_ContextCreated(sender, e);
            var send = (Control)sender;
            var w = send.Width;
            var h = send.Height;
            GL1.addMonitor(new Rectangle(0, 0, w, h), 0);
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
            Pen pen1 = new Pen(Color.Black);
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

        private void imageBox1_Move(object sender, MouseEventArgs e)
        {
            var cont = (Control)sender;
            GL1.MouseLoc.x = (float)e.X/(float)cont.Width;
            GL1.MouseLoc.y = 1 - (float)e.Y / (float)cont.Height;
        }

        void detectingFace(Mat mat_face)
        {

            //var recog = FaceRecognitionDotNet.FaceRecognition.Create(_face);
            var imface = faceImageFromMat(mat_face);

            var param = new ModelParameter();
            var fr = FaceRecognition.Create(@"dlib_models");
            var locs =  fr.FaceLocations(imface).ToArray();

            var lands = fr.FaceLandmark(imface).ToArray();
            for(int i=0; i<locs.Length;i++)
            {
                //locs[i].
                  CvInvoke.Rectangle(mat_face, new Rectangle(locs[i].Left, locs[i].Top, locs[i].Right- locs[i].Left, locs[i].Bottom), new MCvScalar(255, 0, 0),2);
                
               
                foreach (FacePart face_tp in Enum.GetValues(typeof(FacePart)))
                {
                    var points = new List<FacePoint>();
                    var en = points.AsEnumerable();
                    lands[i].TryGetValue(face_tp, out en);
                    if(en != null)
                    {
                        var ps = en.ToArray();
                        for (int j = 0; j < ps.Length; j++)
                        {
                            CvInvoke.Circle(mat_face, new System.Drawing.Point(ps[j].Point.X, ps[j].Point.Y), 2, new MCvScalar(0, 0, 255), 2);
                        }
                    }
                    
                }
                    
            }

            CvInvoke.Imshow("sdf", mat_face);

        }
        FaceRecognitionDotNet.Image faceImageFromMat(Mat mat)
        {
            return FaceRecognition.LoadImage(mat.ToBitmap());
            /*if (mat.NumberOfChannels==1)
            {
                var matData = (byte[,])mat.GetData();
                var imData = new byte[matData.GetLength(0) * matData.GetLength(1)];
                int w = matData.GetLength(0);
                int h = matData.GetLength(1);
                for (int j=0; j< h;j++)
                {
                    for (int i = 0; i < w; i++)
                    {
                        imData[j * w + i] = matData[i, j];
                    }
                }
                
            }
            else if (mat.NumberOfChannels == 3)
            {
                var matData = (byte[,,])mat.GetData();
                var imData = new byte[matData.GetLength(0) * matData.GetLength(1) * 3];
                int w = matData.GetLength(0);
                int h = matData.GetLength(1);
                for (int j = 0; j < h; j++)
                {
                    for (int i = 0; i < w; i++)
                    {

                        imData[j * w + 3*i] = matData[i, j,0];
                        imData[j * w + 3 * i + 1] = matData[i, j, 1];
                        imData[j * w + 3 * i + 2] = matData[i, j, 2];
                    }
                }
                return FaceRecognitionDotNet.FaceRecognition.LoadImage(imData, h, w, 3);
            }
            else
            {
                return null;
            }*/
        }

        private void glControl1_ContextDestroying(object sender, GlControlEventArgs e)
        {
            GL1.glControl_ContextDestroying(sender, e);
        }
    }
}
