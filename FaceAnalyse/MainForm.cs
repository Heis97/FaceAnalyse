using Emgu.CV;
using Emgu.CV.UI;
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
            var model = new Model3d("Model.obj");
            GL1.addOBJ(model.mesh, model.normale, model.texture,0.01f);
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(10, 0, 0), new Point3d_GL(0, 10, 0), new Point3d_GL(0, 0, 10));
        }

        #region gl_control
        private void glControl1_ContextCreated(object sender, GlControlEventArgs e)
        {
            var pict = new Mat("Model.png");
            CvInvoke.Resize(pict, pict, new Size(800, 800));
            GL1.pict = pict;
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

        }

        private void glControl1_ContextDestroying(object sender, GlControlEventArgs e)
        {
            GL1.glControl_ContextDestroying(sender, e);
        }
    }
}
