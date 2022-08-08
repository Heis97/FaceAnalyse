using Emgu.CV;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry
{ 
    public struct Point3d_GL
    {
        public double x;
        public double y;
        public double z;
        public bool exist;
        public Point3d_GL(double _x = 0, double _y = 0, double _z = 0)
        {
            x = _x;
            y = _y;
            z = _z;
            exist = true;
        }

        public Point3d_GL(Point p, double _z)
        {
            x = p.X;
            y = p.Y;
            z = _z;
            exist = true;
        }
        public Point3d_GL(PointF p, double _z)
        {
            x = p.X;
            y = p.Y;
            z = _z;
            exist = true;
        }
        public Point3d_GL(Point3d_GL p1, Point3d_GL p2)
        {
            x = p1.x + (p2.x - p1.x) / 2;
            y = p1.y + (p2.y - p1.y) / 2;
            z = p1.z + (p2.z - p1.z) / 2;
            exist = true;
        }
        public Point3d_GL(double[,] cor)
        {
            x = cor[0, 0];
            y = cor[1, 0];
            z = cor[2, 0];
            exist = true;
        }
        public Point3d_GL normalize()
        {
            var norm = Math.Sqrt(x * x + y * y + z * z);
            if (norm != 0)
            {
                x /= norm;
                y /= norm;
                z /= norm;
            }
            else
            {
                x = 0;
                y = 0;
                z = 0;
            }

            return new Point3d_GL(x, y, z);
        }
        public double magnitude()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }
        public static float[] toMesh(Point3d_GL[] point3Ds)
        {
            var mesh = new float[point3Ds.Length * 3];
            for(int i=0; i<point3Ds.Length;i++)
            {
                mesh[3 * i] =  (float)point3Ds[i].x;
                mesh[3 * i +1] = (float)point3Ds[i].y;
                mesh[3 * i +2] = (float)point3Ds[i].z;
            }
            return mesh;
        }
        public static Point3d_GL notExistP()
        {
            var p = new Point3d_GL();
            p.exist = false;
            return p;
        }
        public double[,] ToDouble()
        {
            return new double[,] { { x }, { y }, { z }, { 1 } };
        }

        public static Point3d_GL[] multMatr(Point3d_GL[] ps,Matrix<double> matrix)
        {
            var ps_ret = new Point3d_GL[ps.Length];
            for(int i=0; i<ps_ret.Length; i++)
            {
                ps_ret[i] = matrix * ps[i];
            }
            return ps_ret;
        }
        public static Point3d_GL operator *(Point3d_GL p, double k)
        {
            return new Point3d_GL(p.x * k, p.y * k, p.z * k);
        }
        public static Point3d_GL operator *(double k,Point3d_GL p)
        {
            return new Point3d_GL(p.x * k, p.y * k, p.z * k);
        }
        public static Point3d_GL operator *(double[,] matrixA, Point3d_GL p)
        {
            double[,] matrixB = new double[1, 1];
            if (matrixA.GetLength(0) == 4)
            {
                matrixB = new double[,] { { p.x }, { p.y }, { p.z }, { 1 } };
            }
            else if (matrixA.GetLength(0) == 3)
            {
                matrixB = new double[,] { { p.x }, { p.y }, { p.z } };
            }
            else
            {
                return Point3d_GL.notExistP();
            }

            if (matrixA.GetLength(1) != matrixB.GetLength(0))
            {
                return Point3d_GL.notExistP();
            }
            var matrixC = new double[matrixA.GetLength(0), matrixB.GetLength(1)];
            for (var i = 0; i < matrixA.GetLength(0); i++)
            {
                for (var j = 0; j < matrixB.GetLength(1); j++)
                {
                    matrixC[i, j] = 0;
                    for (var k = 0; k < matrixB.GetLength(0); k++)
                    {
                        matrixC[i, j] += matrixA[i, k] * matrixB[k, j];
                    }
                }
            }
            return new Point3d_GL(matrixC);
        }
        static double[,] Matrix4x4ToDouble(Matrix4x4f matrixA)
        {
            var ret = new double[4, 4];
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    ret[i, j] = (double)matrixA[(uint)i, (uint)j];

                }
            }
            return ret;
        }

        static double[,] Matrix4x4ToDouble(Matrix<double> matrixA)
        {

            var ret = new double[matrixA.Cols, matrixA.Rows];
            for (var i = 0; i < ret.GetLength(0); i++)
            {
                for (var j = 0; j < ret.GetLength(1); j++)
                {
                    ret[i, j] = (double)matrixA[i, j];

                }
            }
            return ret;
        }


        public static Point3d_GL operator *(Matrix4x4f matrixA, Point3d_GL p)
        {
            var matrix = Matrix4x4ToDouble(matrixA);
            return matrix * p;
        }

        public static Point3d_GL operator *(Matrix<double> matrixA, Point3d_GL p)
        {
            var matrix = Matrix4x4ToDouble(matrixA);
            return matrix * p;
        }
        public static float[] toData(Point3d_GL[] ps)
        {
            var data = new float[ps.Length * 4];
            for(int i=0; i<ps.Length; i++)
            {
                data[4 * i] = (float)ps[i].x;
                data[4 * i + 1] = (float)ps[i].y;
                data[4 * i + 2] = (float)ps[i].z;
            }
            return data;
        }

        public static Point3d_GL[] dataToPoints(float[] data)
        {
            var ps = new Point3d_GL[data.Length / 4];
            for (int i = 0; i < ps.Length; i++)
            {
                ps[i].x = data[4 * i];
                ps[i].y = data[4 * i + 1];
                ps[i].z = data[4 * i + 2];
            }
            return ps;
        }

        public static Point3d_GL[] dataToLines(float[] data)
        {
            var ps = new Point3d_GL[data.Length / 4];
            for (int i = 0; i < ps.Length; i++)
            {
                ps[i].x = data[4 * i];
                ps[i].y = data[4 * i + 1];
                ps[i].z = data[4 * i + 2];

                i++;

                ps[i].x = data[4 * i];
                ps[i].y = data[4 * i + 1];
                ps[i].z = data[4 * i + 2];
            }
            return ps;
        }

        public static Point3d_GL[] dataToLines_2(float[] data)
        {
            var ps = new Point3d_GL[data.Length / 4];
            int i_c = 0;
            for (int i = 0; i < ps.Length/2; i++)
            {
                ps[i_c].x = data[4 * i];
                ps[i_c].y = data[4 * i + 1];
                ps[i_c].z = data[4 * i + 2];
                i_c++;
                var i1 = i + ps.Length / 2;

                ps[i_c].x = data[4 * i1];
                ps[i_c].y = data[4 * i1 + 1];
                ps[i_c].z = data[4 * i1 + 2];
                i_c++;
            }
            return ps;
        }

        public static Point3d_GL[][] dataToLines_3(float[] data,int lines_count)
        {
            var ps = new Point3d_GL[lines_count][];
            int len_line = ps.Length / (lines_count * 4);
            for (int i = 0; i < lines_count; i++)
            {
                ps[i] = new Point3d_GL[len_line];
                for(int j = 0; j < len_line; j++)
                {
                    var ind = (j+i*len_line )* 4;
                    ps[i][j].x = data[ind];
                    ps[i][j].y = data[ind + 1];
                    ps[i][j].z = data[ind + 2];
                }
            }
            return ps;
        }

        public static Point3d_GL[] connect_points(Point3d_GL[] ps)
        {
            var ps_con = new List<Point3d_GL>();
            var ps_l = ps.ToList();

            ps_con.Add(ps[0]);
            ps_l.RemoveAt(0);
            for (int i=0;i< ps_l.Count; i++)
            {
                var ind_i = get_nearest_point(ps_con[ps_con.Count - 1], ps_l);
                var dist = (ps_con[ps_con.Count - 1] - ps_l[ind_i]).magnitude();
                if (dist > 0.001 && dist < 0.1) ps_con.Add(ps_l[ind_i]);
                ps_l.RemoveAt(ind_i);
            }
            return ps_con.ToArray();
        }

        public static int get_nearest_point(Point3d_GL p, List<Point3d_GL> ps)
        {
            int i_min = 0;
            double dist_min = double.MaxValue;
            for (int i = 0; i < ps.Count; i++)
            {
                var dist = (p - ps[i]).magnitude();
                if (dist < dist_min)
                {
                    i_min = i;
                    dist_min = dist;
                }
            }
            return i_min;
        }

        public static Point3d_GL operator +(Point3d_GL p, Vector3d_GL v1)
        {
            return new Point3d_GL(p.x + v1.x, p.y + v1.y, p.z + v1.z);
        }
        public static double operator *(Point3d_GL p, Vector3d_GL v1)
        {
            return p.x * v1.x+ p.y * v1.y+p.z * v1.z;
        }
        public static Point3d_GL operator +(Point3d_GL p1, Point3d_GL p2)
        {
            return new Point3d_GL(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
        }

        public static Point3d_GL operator -(Point3d_GL p1, Point3d_GL p2)
        {
            return new Point3d_GL(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
        }
        public static Point3d_GL operator -(Point3d_GL p1)
        {
            return new Point3d_GL(-p1.x,  - p1.y,- p1.z);
        }
        public  Point3d_GL Clone()
        {
            return new Point3d_GL(x, y, z);
        }
        public override string ToString()
        {

            return Math.Round(x, 4).ToString() + " " + Math.Round(y, 4).ToString() + " " + Math.Round(z, 4).ToString();
        }
    }

}
