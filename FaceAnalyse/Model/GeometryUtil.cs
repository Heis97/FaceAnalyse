using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry;

namespace Model
{
    public struct VertexGl
    {
        public Point3d_GL p;
        public Point3d_GL n;
        public Geometry.PointF t;
        public VertexGl(Point3d_GL _p, Point3d_GL _n, PointF _t)
        {
            p = _p;
            n = _n;
            t = _t;
        }
    }
    public struct TriangleGl
    {
        VertexGl v1;
        VertexGl v2;
        VertexGl v3;
        public TriangleGl(VertexGl _v1, VertexGl _v2, VertexGl _v3)
        {
            v1 = _v1;
            v2 = _v2;
            v3 = _v3;
        }
        public bool affilationPoint(VertexGl _p)
        {
            float m, l;
            var p = _p;
            var P = _p.t - v1.t;
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
    }
}
