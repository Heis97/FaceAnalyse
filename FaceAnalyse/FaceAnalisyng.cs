using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using Geometry;
using FaceRecognitionDotNet;
using Emgu.CV;

namespace FaceAnalyse
{
    public class Face3d
    {
        FacePart3d[] parts;
        Point3d_GL[] ps3d;
        public List<Point3d_GL> centerEye;

        public Face3d(FacePart3d[] _parts)
        {
            parts = _parts;
            centerEye = new List<Point3d_GL>();
        }

        static public Face3d joinFaces3d(Face3d[] faces)
        {
            if(faces.Length==1)
            {
                return faces[0];
            }
            var face_b = findBestFace(faces);
            face_b.centerEye = new List<Point3d_GL>();
           // var face_b = faces[3];
            for (int i = 0; i < face_b.parts.Length; i++)
            {
                for (int j = 0; j < face_b.parts[i].ps3d.Length; j++)
                {
                    Console.WriteLine("_________");
                    for (int i_another =0; i_another < faces.Length; i_another++)
                    {
                        var p3d = face_b.parts[i].ps3d[j];
                        
                        // Console.WriteLine("p3d " + p3d+ "; p3d.magnitude(): " + p3d.magnitude());
                        if (p3d.magnitude() < 0.01)
                        {
                            face_b.parts[i].ps3d[j] = faces[i_another].parts[i].ps3d[j];
                            
                        }
                        if(face_b.parts[i].part_type== FacePart.LeftEye)
                        {
                            //Console.WriteLine("face_b.parts["+i_another+"].ps3d[" +j+"] = " + faces[i_another].parts[i].ps3d[j]);
                        }

                    }
                   // Console.WriteLine("face_b.parts[i].ps3d[j] " + face_b.parts[i].ps3d[j]);
                }
            }
            return face_b;
        }

        static Face3d findBestFace(Face3d[] faces)
        {
            if(faces==null)
            {
                return null;
            }
            int[] inds = new int[faces.Length];
            int ind_max = 0;
            int val_max = 0;
            for (int i = 0; i < faces.Length; i++)
            {
                for(int j=0; j < faces[i].parts.Length;j++)
                {
                    for (int k = 0; k < faces[i].parts[j].ps3d.Length; k++)
                    {
                        var p3d = faces[i].parts[j].ps3d[k];
                        if (p3d.magnitude()>0.01)
                        {
                            inds[i]++;
                        }
                    }
                }
            }
            for (int i = 0; i < inds.Length; i++)
            {
                if(inds[i] > val_max)
                {
                    val_max = inds[i];
                    ind_max = i;                    
                }
            }
            Console.WriteLine("ind_max: " + ind_max);
            return faces[ind_max];
        }

        static Point3d_GL[] find3DPointsFromTex(PointF[] points, Model3d model)
        {
            var p3d = new List<Point3d_GL>();
            for (int j = 0; j < points.Length; j++)
            {
                var p2d = new PointF(points[j].X, 1 - points[j].Y);
                var p3 = model.take3dfrom2d(p2d);
                //Console.WriteLine(points[j] + " " + p2d + " " + p3);
                p3d.Add(p3);
            }
            return p3d.ToArray();
        }

        static Point3d_GL[] find3DPointsFromGl(PointF[] points, Model3d model,double zoom)
        {
            var p3d = new List<Point3d_GL>();
            for (int j = 0; j < points.Length; j++)
            {
                var p2d = new Point3d_GL(points[j].X, points[j].Y,0);
                var p3 = model.take3dfrom2d_gl(p2d, zoom);
                Console.WriteLine(points[j] + " " + p2d + " " + p3);
                p3d.Add(p3);
            }
            return p3d.ToArray();
        }


        public Point3d_GL[] getPoints3d()
        {
            var ps3d_list = new List<Point3d_GL>();
            for (int i = 0; i < parts.Length; i++)
            {
                ps3d_list.AddRange(parts[i].ps3d);
                var p_c = findCenterEye(parts[i]);
                if (p_c.exist)
                {
                    ps3d_list.Add(p_c);
                    centerEye.Add(p_c);
                }
            }
            return ps3d_list.ToArray();
        }
        static Point3d_GL norm_to2d_gl(PointF p)
        {
            var x = (p.X - 0.5) * 2;
            var y = (0.5 - p.Y) * 2;
            return new Point3d_GL(x,y,0);
        }

        public float[] getPointsData()
        {
            var ps3d_list = new List<Point3d_GL>();
            for (int i = 0; i < parts.Length; i++)
            {
                for (int j = 0; j < parts[i].ps2d.Length; j++)
                {
                    ps3d_list.Add(norm_to2d_gl(parts[i].ps2d[j]));
                }
            }
            return Point3d_GL.toData(ps3d_list.ToArray());
        }
        public void setPointsFromData(Point3d_GL[] data)
        {
            int data_i = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                var ps3d_part = new List<Point3d_GL>();
                for (int j = 0; j < parts[i].ps2d.Length; j++)
                {
                    ps3d_part.Add(data[data_i]);
                    data_i++;
                }
                parts[i].ps3d = ps3d_part.ToArray();
            }
        }
        public void setPointsFromData3dp(float[] data)
        {
            int data_i = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                var ps3d_part = new List<Point3d_GL>();
                for (int j = 0; j < parts[i].ps2d.Length; j++)
                {
                    ps3d_part.Add(new Point3d_GL(data[data_i], data[data_i + 1], data[data_i] + 2));
                    data_i += 4;
                }
                parts[i].ps3d = ps3d_part.ToArray();
            }
        }


        public void setPoints3dFromModel(Model3d model,double zoom,bool from_gl = false)
        {
            var ps3d_list = new List<Point3d_GL>();
            for(int i=0; i < parts.Length; i++)
            {

                var ps3d_cur = find3DPointsFromGl(parts[i].ps2d, model,zoom);
                
                parts[i].ps3d = ps3d_cur;
                ps3d_list.AddRange(ps3d_cur);
            }
            ps3d = ps3d_list.ToArray();
        }

        public Point3d_GL findCenterEye(FacePart3d facePart)
        {
            if(facePart.part_type==FacePart.LeftEye || facePart.part_type==FacePart.RightEye)
            {
                return new Point3d_GL(
                    new Point3d_GL(facePart.ps3d[1], facePart.ps3d[4]),
                    new Point3d_GL(facePart.ps3d[2], facePart.ps3d[5]));
            }
            return Point3d_GL.notExistP();
        }

        public Matrix<double> get_matrix_eye_center()
        {
            if(centerEye.Count>1)
            {
                return GetMatrixLineZ(centerEye[0], centerEye[1]);
            }
            else
            {
                return null;
            }
        }
        static Matrix<double> GetMatrixLineZ(Point3d_GL p1,Point3d_GL p2)
        {
            if(p2.x<p1.x)
            {
                var lm = p1.Clone();
                p1 = p2.Clone();
                p2 = lm.Clone();
            }
            var vec_x = new Vector3d_GL(p1, p2);
            vec_x.normalize();
            var vec_z = new Vector3d_GL(0, 0, -1);
            var vec_y = vec_z | vec_x;
            vec_y.normalize();
            vec_z = vec_x | vec_y;
            var rot_matr = new Matrix<double>(new double[,] {
                { -vec_x.x, -vec_x.y, -vec_x.z, 0 },
                { -vec_y.x, -vec_y.y, -vec_y.z, 0 },
                { -vec_z.x, -vec_z.y, -vec_z.z, 0 },
                { 0, 0, 0, 1} });
            var p_c = new Point3d_GL(rot_matr * p1, rot_matr * p2);
            rot_matr[0, 3] = -p_c.x;
            rot_matr[1, 3] = -p_c.y;
            rot_matr[2, 3] = -p_c.z;
            return rot_matr;
        }

    }

    public class FacePart3d
    {
        public FacePart part_type;
        public PointF[] ps2d;
        public Point3d_GL[] ps3d;
        
        public FacePart3d(FacePart _part_type,PointF[] _ps2d)
        {
            part_type = _part_type;
            ps2d = _ps2d;
        }     
    }
    public class FaceAnalisyng
    {
        public Model3d model;
        public FaceAnalisyng()
        {

        }
    }
}
