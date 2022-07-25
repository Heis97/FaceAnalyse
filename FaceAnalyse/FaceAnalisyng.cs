using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using Geometry;
using FaceRecognitionDotNet;

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

        static Point3d_GL[] find3DPointsFromGl(PointF[] points, Model3d model)
        {
            var p3d = new List<Point3d_GL>();
            for (int j = 0; j < points.Length; j++)
            {
                var p2d = new PointF(points[j].X, points[j].Y);
                var p3 = model.take3dfrom2d(p2d);
                //Console.WriteLine(points[j] + " " + p2d + " " + p3);
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


        public void setPoints3dFromModel(Model3d model,bool from_gl = false)
        {
            var ps3d_list = new List<Point3d_GL>();
            for(int i=0; i < parts.Length; i++)
            {

                var ps3d_cur = find3DPointsFromTex(parts[i].ps2d, model);

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
               // Console.WriteLine(facePart.ps3d[1]+"; "+ facePart.ps3d[4]);
               // return new Point3d_GL(facePart.ps3d[1], facePart.ps3d[4]);
            }
            return Point3d_GL.notExistP();
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
