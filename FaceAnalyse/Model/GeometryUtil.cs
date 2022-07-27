using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry;
using Emgu.CV;

namespace Model
{
    public struct VertexGl
    {
        public Point3d_GL p;
        public Point3d_GL n;
        public PointF t;
        public VertexGl(Point3d_GL _p, Point3d_GL _n, PointF _t)
        {
            p = _p;
            n = _n;
            t = _t;
        }
    }
    public struct TriangleGl
    {
        public VertexGl v1;
        VertexGl v2;
        VertexGl v3;
        Vector3d_GL n;
        public TriangleGl(VertexGl _v1, VertexGl _v2, VertexGl _v3)
        {
            v1 = _v1;
            v2 = _v2;
            v3 = _v3;
            var vec1 = new Vector3d_GL(v1.p, v2.p);
            var vec2 = new Vector3d_GL(v1.p, v3.p);
            var vec3 = vec1 | vec2;//vector multiply
            vec3.normalize();
            n = vec3;
        }
        public bool affilationPoint(PointF _p)
        {
            float m, l;
            //var p = _p;
            var P = _p - v1.t;
            var B = v2.t - v1.t;
            var C = v3.t - v1.t;
            m = (P.X * B.Y - B.X * P.Y) / (C.X * B.Y - B.X * C.Y);
            if(m>=0 && m<=1)
            {
                l = (P.X - m * C.X) / B.X;
                if (l >= 0 && (m+l) <= 1)
                {
                    return true;
                }
            }
            return false;
        }

        public bool affilationPoint_xy(Point3d_GL _p)
        {
            double m, l;
            var P = _p - v1.p;
            var B = v2.p - v1.p;
            var C = v3.p - v1.p;
            m = (P.x * B.y - B.x * P.y) / (C.x * B.y - B.x * C.y);
            if (m >= 0 && m <= 1)
            {
                l = (P.x - m * C.x) / B.x;
                if (l >= 0 && (m + l) <= 1)
                {
                    return true;
                }
            }
            return false;
        }

        public Point3d_GL project_point_xy(Point3d_GL p)
        {
            if(n.z==0)
            {
                return new Point3d_GL(p.x, p.y, 0);
            }
            
            var z = (-(v1.p * n) - n.x * p.x - n.y * p.y) / n.z;
            return new Point3d_GL(p.x,p.y,0);
        }
        public static void multiply_matr(TriangleGl[] triangles,Matrix<double> matrix)
        {            
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i].v1.p = matrix * triangles[i].v1.p;
                triangles[i].v2.p = matrix * triangles[i].v2.p;
                triangles[i].v3.p = matrix * triangles[i].v3.p;
            }
        }

        public static float[] get_mesh(TriangleGl[] triangles)
        {
            float[] mesh = new float[triangles.Length*9];
            for(int i=0; i<triangles.Length;i++)
            {
                mesh[9*i] = (float)triangles[i].v1.p.x;
                mesh[9 * i+1] = (float)triangles[i].v1.p.y;
                mesh[9 * i+2] = (float)triangles[i].v1.p.z;

                mesh[9 * i + 3] = (float)triangles[i].v2.p.x;
                mesh[9 * i + 4] = (float)triangles[i].v2.p.y;
                mesh[9 * i + 5] = (float)triangles[i].v2.p.z;

                mesh[9 * i + 6] = (float)triangles[i].v3.p.x;
                mesh[9 * i + 7] = (float)triangles[i].v3.p.y;
                mesh[9 * i + 8] = (float)triangles[i].v3.p.z;
            }
            return mesh;
        }
    }
}
