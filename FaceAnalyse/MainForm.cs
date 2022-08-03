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
using FaceAnalyse;
using System.Threading;

namespace FaceAnalyse
{
    public partial class MainForm : Form
    {
        private GraphicGL GL1 = new GraphicGL();
        Model3d model;
        Mat pict;
        int face_model;
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
            model = new Model3d(@"faces/Archive/25/25.0/Model.obj", false);
            pict = new Mat(@"faces/Archive/25/25.0/Model.jpg");

            //model = new Model3d(@"faces/Archive/RAW/SMALL2_model.obj", false);
            //pict = new Mat(@"faces/Archive/RAW/SMALL2_model.jpg");
            CvInvoke.Resize(pict, pict, new System.Drawing.Size(1500, 1500));           
            imageBox1.Image = pict;
            
            
            face_model = GL1.addOBJ(model.mesh,model.normale, model.texture, 1, 1, pict);
            //GL1.buffersGl.setTranspobj(face_model, 0.3f);

            
            GL1.buffersGl.setMatrobj(face_model, 0, trsc.toGLmatrix(model.matrix_norm));
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(1, 0, 0), new Point3d_GL(0, 1, 0), new Point3d_GL(0, 0, 1));
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(-1, 0, 0), new Point3d_GL(0, -1, 0), new Point3d_GL(0, 0, -1));
        }

        

        #region gl_control
        private void glControl1_ContextCreated(object sender, GlControlEventArgs e)
        {
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
            var mat1 = GL1.matFromMonitor(0);
            CvInvoke.Flip(mat1,mat1,FlipType.Vertical);
            imageBox1.Image = mat1;
            GL1.glControl_Render(sender, e);

            var data = GL1.isolines_data.getData();
            {
                if(data!=null)
                {
                    var ps3d = Point3d_GL.dataToPoints(data);
                    Console.WriteLine(GL1.toStringBuf(data, 4, 0, "det"));
                    GL1.addMeshWithoutNorm(Point3d_GL.toMesh(ps3d), PrimitiveType.Points,1,1,1);
                    
                }
            }
            

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

        Face3d setPointsModel(Face3d[] faces,Model3d model,bool from_gl = false)
        {
            var ps3d_l = new List<Point3d_GL>();
            for(int i = 0; i < faces.Length; i++)
            {
                faces[i].setPoints3dFromModel(model,  GL1.transRotZooms[0].zoom,from_gl);
                ps3d_l.AddRange(faces[i].getPoints3d());
            }
            return Face3d.joinFaces3d(faces);
        }

        Point3d_GL[][] find3DPointsFromTex(
            PointF[][] points,
            Model3d model
            )
        {
            var landm = new List<Point3d_GL[]>();
            for(int i=0; i<points.Length; i++)
            {
                var p3d = new List<Point3d_GL>();
                for (int j = 0; j < points[i].Length; j++)
                {
                    var p2d = new PointF(points[i][j].X, 1-points[i][j].Y);
                    var p3 = model.take3dfrom2d(p2d);
                    p3d.Add(p3);
                }
                landm.Add(p3d.ToArray());
            }
            return landm.ToArray();
        }

        Face3d[] detectingFace(Mat mat_face)
        {
            var imface = faceImageFromMat(mat_face);
            var fr = FaceRecognition.Create(@"dlib_models");
            var locs =  fr.FaceLocations(imface).ToArray();            
            var lands = fr.FaceLandmark(imface).ToArray();
            var faces = new List<Face3d>();
            for (int i=0; i<locs.Length;i++)
            {
                //CvInvoke.Rectangle(mat_face, new System.Drawing.Rectangle(locs[i].Left, locs[i].Top, locs[i].Right- locs[i].Left, locs[i].Bottom), new MCvScalar(255, 0, 0),2);                
                var parts = new List<FacePart3d>();
                foreach (FacePart face_tp in Enum.GetValues(typeof(FacePart)))
                {
                    var ps_norm = new List<PointF>();
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
                            //CvInvoke.Circle(mat_face, pf, 0, color, 2);
                            CvInvoke.DrawMarker(mat_face, pf, new MCvScalar(0, 255, 255), MarkerTypes.Cross,6);
                            CvInvoke.PutText(mat_face, j.ToString(),new System.Drawing.Point( pf.X+3, pf.Y - 3), FontFace.HersheyPlain, 1, new MCvScalar(0, 255, 255));
                            ps_norm.Add(new PointF(ps[j].Point.X/ (float)mat_face.Width, ps[j].Point.Y / (float)mat_face.Height));
                        }
                        parts.Add(new FacePart3d(face_tp, ps_norm.ToArray()));
                    }                    
                }
                faces.Add(new Face3d(parts.ToArray()));
            }
            return faces.ToArray();
        }

        PointF[] pointsFromFacePoins(FacePoint[] fps)
        {
            PointF[] points = new PointF[fps.Length];
            for(int i=0; i<points.Length;i++)
            {
                points[i] =new PointF(fps[i].Point.X, fps[i].Point.Y);
            }
            return points;
        }

        FaceRecognitionDotNet.Image faceImageFromMat(Mat mat)
        {
            return FaceRecognition.LoadImage(mat.ToBitmap());
        }

        private void glControl1_ContextDestroying(object sender, GlControlEventArgs e)
        {
            GL1.glControl_ContextDestroying(sender, e);
        }

        private void but_prog_type_Click(object sender, EventArgs e)
        {
            GL1.changeViewType(0);
        }

        private void but_xy_plane_Click(object sender, EventArgs e)
        {
            GL1.planeXY();
        }

        async void det_landmark()
        {
           
            var mat1 = (Mat)imageBox1.Image;
            var face = detectingFace(mat1)[0];
            imageBox2.Image = mat1;
            await Task.Delay(200);
            GL1.landmark2d_data.setData(face.getPointsData());
            //Console.WriteLine( GL1.toStringBuf(face.getPointsData(),4,0,"det"));
            GL1.comp_proj = 1;
            await Task.Delay(200);
            GL1.comp_proj = 0;

            face.setPointsFromData(Point3d_GL.dataToPoints(GL1.landmark3d_data.getData()));
            //Console.WriteLine(GL1.toStringBuf(GL1.landmark3d_data.getData(), 4, 0, "det"));
            var ps3d = face.getPoints3d();
            GL1.addMeshWithoutNorm(Point3d_GL.toMesh(ps3d), PrimitiveType.Points);
            GL1.addMeshWithoutNorm(Point3d_GL.toMesh(face.centerEye.ToArray()), PrimitiveType.Lines, 0.9f);
            var flatxy1 = GL1.addFlat3d_XY(0);
            GL1.buffersGl.setTranspobj(flatxy1, 0.3f);
            var flatxy2 = GL1.addFlat3d_XY(0.2);
            GL1.buffersGl.setTranspobj(flatxy2, 0.3f);

            var flatzy1 = GL1.addFlat3d_ZY(-0.2);
            GL1.buffersGl.setTranspobj(flatzy1, 0.3f);

            var flatzy2 = GL1.addFlat3d_ZY(0.2);
            GL1.buffersGl.setTranspobj(flatzy2, 0.3f);
            GL1.SortObj();
            
        }

       
        async void align_face()
        {
            prepare_face_gl();
            await Task.Delay(200);
            var mat1 = (Mat)imageBox1.Image;
            var faces = detectingFace(mat1);
            if(faces == null)
            {
                return;
            }
            var face = faces[0];
            imageBox2.Image = mat1;
            GL1.landmark2d_data.setData(face.getPointsData());
            //Console.WriteLine(GL1.toStringBuf(face.getPointsData(), 4, 0, "align"));
            GL1.comp_proj = 1;
            await Task.Delay(200);
            GL1.comp_proj = 0;

            face.setPointsFromData(Point3d_GL.dataToPoints(GL1.landmark3d_data.getData()));
           // Console.WriteLine(GL1.toStringBuf(GL1.landmark3d_data.getData(), 4, 0, "align"));
            face.getPoints3d();
            var matr = face.get_matrix_eye_center();
            GL1.buffersGl.addMatrobj(face_model, 0, trsc.toGLmatrix(matr));
            GL1.SortObj();
        }
        void prepare_face_gl()
        {
            GL1.lightVis = 1;
            GL1.textureVis = 1;
            GL1.transRotZooms[0].viewType_ = viewType.Ortho;
            GL1.transRotZooms[0].zoom = 1;
        }

        private void but_det_landmark_Click(object sender, EventArgs e)
        {
            //var GL1 = (GraphicGL)obj;
            det_landmark();

        }

        private void but_align_face_Click(object sender, EventArgs e)
        {
            align_face();
        }

        private void but_show_faces_Click(object sender, EventArgs e)
        {
            if (GL1.show_faces == 0)
            {
                GL1.show_faces = 1;
            }
            else
            {
                GL1.show_faces = 0;
            }
        }

        private void but_norm_inv_Click(object sender, EventArgs e)
        {
            if (GL1.inv_norm == 0)
            {
                GL1.inv_norm = 1;
            }
            else
            {
                GL1.inv_norm = 0;
            }
        }



        /* public void startdet_landmark()
        {
            try
            {
                Thread robot_thread = new Thread(det_landmark);
                robot_thread.Start();
            }
            catch
            {
            }
        }
        private void det_landmark(object obj)
        {        
        }*/
    }
}
