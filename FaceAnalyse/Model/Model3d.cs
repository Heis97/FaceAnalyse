using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry;

namespace Model
{
    public struct Model3d
    {
        public string path;
        public float[] mesh;
        public float[] texture;
        public float[] normale;
        public TriangleGl[] triangles;
        public Point3d_GL center;
        public Model3d(string _path, bool centering = true)
        {
            path = _path;
            var name_list = path.Split('.');
            var format = name_list[name_list.Length - 1].ToLower();
            var center1 = new Point3d_GL(0, 0, 0);
            if(format == "obj")
            {
                var triang = new TriangleGl[0];

                var arrays = parsingObj(path,out triang,out center1);
                mesh = arrays[0];
                texture = arrays[1];
                normale = arrays[2];
                triangles = triang;
                center = center1;
                

            }
            else if(format == "stl")
            {
                mesh = parsingStl_GL4(path, out center1);
                texture = new float[0];
                normale = new float[0];
                triangles = null;
                center = center1;
            }
            else
            {
                mesh = new float[0];
                texture = new float[0];
                normale = new float[0];
                triangles = null;
                center = center1;
            }
            if (centering)
            {
                this.FrameToCenter();
            }
            
        }
        public void FrameToCenter()
        {
            for(int i=0; i<mesh.Length;i+=3)
            {
                mesh[i] -= (float)center.x;
                mesh[i+1] -= (float)center.y;
                mesh[i+2] -= (float)center.z;
            }
        }
        public float[] parsingTxt_Tab(string path)
        {
            
            var text = File.ReadAllText(path);
            var lines = text.Split('\n');
            var mesh = new float[(lines.Length - 2) * 3];
            var ind = 0;
            for(int i=1; i<lines.Length-1;i++)
            {
                var p = lines[i].Split('\t');
                mesh[ind] = (float)parseE(p[0]); ind++;
                mesh[ind] = (float)parseE(p[1])-30; ind++;
                mesh[ind] = (float)parseE(p[2]); ind++;

            }
            for(int i=0; i<41-1;i++)
            {
                for (int j = 0; j < 91-1; j++)
                {
                    //Console.WriteLine(mesh[3 * (i * 91 + j+1) ] - mesh[3 * (i * 91 + j) ]);
                    //mesh[3*(i * 91 + j) +2] += j;    
                    
                }
            }
            var text1 = text.Replace('\t', ';');
           // Console.WriteLine("Len = "+((double)lines.Length-2)/91);
            return mesh;
        }

        static public double parseE(string num)
        {
            if (num.Contains("e"))
            {
                var splnum = num.Split(new char[] { 'e' });
                return Convert.ToDouble(splnum[0]) * Math.Pow(10, Convert.ToInt32(splnum[1]));
            }
            else if (num.Contains("E"))
            {
                var splnum = num.Split(new char[] { 'E' });
                return Convert.ToDouble(splnum[0]) * Math.Pow(10, Convert.ToInt32(splnum[1]));
            }
            else
            {
                return Convert.ToDouble(num);
            }

        }

        static public int[] parseFace(string num)
        {
            var splnum  =num.Split('/');
            var splnum_int = new int[splnum.Length];
            for (int i=0; i<splnum.Length;i++)
            {
                splnum_int[i] = Convert.ToInt32(splnum[i]);
            }
            return splnum_int;

        }

        static float[] minCompar(float[] val, float[] min)
        {
            if (val == null || min == null)
            {
                return min;
            }
            if (val.Length != min.Length)
            {
                return min;
            }
            for (int i = 0; i < val.Length; i++)
            {
                if (min[i] > val[i])
                {
                    min[i] = val[i];
                }
            }
            return min;
        }

        static float[] maxCompar(float[] val, float[] max)
        {
            if (val == null || max == null)
            {
                return max;
            }
            if (val.Length != max.Length)
            {
                return max;
            }
            for (int i = 0; i < val.Length; i++)
            {
                if (max[i] < val[i])
                {
                    max[i] = val[i];
                }
            }
            return max;
        }
        static public float[] parsingStl_GL4(string path, out Point3d_GL _center)
        {
            float[] min_v = new float[] { float.MaxValue, float.MaxValue, float.MaxValue };
            float[] max_v = new float[] { float.MinValue, float.MinValue, float.MinValue };
            string file1;
            using (StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII))
            {
                file1 = sr.ReadToEnd();
            }
            string[] lines = file1.Split(new char[] { '\n' });
            int len = 0;
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });

                if (vert.Length > 3)
                {
                    if (vert[0].Contains("ert"))
                    {
                        len += 3;
                    }

                }
            }
            float[] ret1 = new float[len];
            Console.WriteLine("Len Stl " + len);
            int i2 = 0;
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });

                if (vert.Length > 3)
                {
                    if (vert[0].Contains("ert"))
                    {
                        ret1[i2] = (float)parseE(vert[1]); i2++;
                        ret1[i2] = (float)parseE(vert[2]); i2++;
                        ret1[i2] = (float)parseE(vert[3]); i2++;
                        min_v = minCompar(new float[] { ret1[i2 - 3], ret1[i2 - 2], ret1[i2-1] }, min_v);
                        max_v = maxCompar(new float[] { ret1[i2 - 3], ret1[i2 - 2], ret1[i2-1] }, max_v);
                    }
                    
                }
            }
            float x_sr = (max_v[0] - min_v[0]) / 2 + min_v[0];
            float y_sr = (max_v[1] - min_v[1]) / 2 + min_v[1];
            float z_sr = (max_v[2] - min_v[2]) / 2 + min_v[2];
            _center = new Point3d_GL( x_sr, y_sr, z_sr );
            return ret1;
        }
        static public float[][] parsingObj(string path, out TriangleGl[] triangleGl, out Point3d_GL _center)
        {
            var ret = new List<float[]>();
            string file1;
            using (StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII))
            {
                file1 = sr.ReadToEnd();
            }
            string[] lines = file1.Split(new char[] { '\n' });
            int v_len = 0;
            int vt_len = 0;
            int vn_len = 0;
            int f_len = 0;
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });

                if (vert.Length > 2)
                {
                    if (vert[0] == "v")
                    {
                        v_len++;
                    }
                    if (vert[0] == "vt")
                    {
                        vt_len++;
                    }
                    if (vert[0] == "vn")
                    {
                        vn_len++;
                    }
                    if (vert[0] == "f")
                    {
                        f_len++;
                    }
                }
            }
            float[][] vertex = new float[v_len][];
            float[][] texture = new float[vt_len][];
            float[][] normale = new float[vn_len][];
            int[][] face_v = new int[f_len][];
            int[][] face_vt = new int[f_len][];
            int[][] face_vn = new int[f_len][];
            triangleGl = new TriangleGl[f_len];
            Console.WriteLine("Len v " + v_len);
            Console.WriteLine("Len vt " + vt_len);
            Console.WriteLine("Len vn " + vn_len);
            int i_v = 0;
            int i_vt = 0;
            int i_vn = 0;
            int i_f = 0;
            float[] min_v = new float[3];
            min_v[0] = float.MaxValue;
            min_v[1] = float.MaxValue;
            min_v[2] = float.MaxValue;
            float[] max_v = new float[3];
            max_v[0] = float.MinValue;
            max_v[1] = float.MinValue;
            max_v[2] = float.MinValue;
            foreach (string str in lines)
            {
                string line = str.Trim();
                string[] subline = line.Split(new char[] { ' ' });

                if (subline.Length > 2)
                {
                    if (subline[0] == "v")
                    {
                        //Console.WriteLine
                        vertex[i_v] = new float[3];
                        vertex[i_v][0] = (float)parseE(subline[1]);
                        vertex[i_v][1] = (float)parseE(subline[2]);
                        vertex[i_v][2] = (float)parseE(subline[3]);
                        max_v = maxCompar(vertex[i_v], max_v);
                        min_v = minCompar(vertex[i_v], min_v);

                        i_v++;
                    }
                    if (subline[0] == "vn")
                    {
                        //Console.WriteLine
                        normale[i_vn] = new float[3];
                        normale[i_vn][0] = (float)parseE(subline[1]);
                        normale[i_vn][1] = (float)parseE(subline[2]);
                        normale[i_vn][2] = (float)parseE(subline[3]);
                        i_vn++;
                    }
                    if (subline[0] == "vt")
                    {
                        texture[i_vt] = new float[2];
                        texture[i_vt][0] = (float)parseE(subline[1]);
                        texture[i_vt][1] = (float)parseE(subline[2]);
                        i_vt++;
                    }
                    if (subline[0] == "f")
                    {
                        face_v[i_f] = new int[3];
                        face_v[i_f][0] = parseFace(subline[1])[0];
                        face_v[i_f][1] = parseFace(subline[2])[0];
                        face_v[i_f][2] = parseFace(subline[3])[0];

                        face_vt[i_f] = new int[3];
                        face_vt[i_f][0] = parseFace(subline[1])[1];
                        face_vt[i_f][1] = parseFace(subline[2])[1];
                        face_vt[i_f][2] = parseFace(subline[3])[1];


                        face_vn[i_f] = new int[3];
                        face_vn[i_f][0] = parseFace(subline[1])[2];
                        face_vn[i_f][1] = parseFace(subline[2])[2];
                        face_vn[i_f][2] = parseFace(subline[3])[2];

                        i_f++;
                    }
                }

            }

            var vertexdata = new float[f_len * 9];
            var normaldata = new float[f_len * 9];
            var textureldata = new float[f_len * 6];
            i_v = 0;
            i_vn = 0;
            i_vt = 0;
            for (int i = 0; i < f_len; i++)
            {
                vertexdata[i_v] = vertex[face_v[i][0] - 1][0]; i_v++;
                vertexdata[i_v] = vertex[face_v[i][0] - 1][1]; i_v++;
                vertexdata[i_v] = vertex[face_v[i][0] - 1][2]; i_v++;

                normaldata[i_vn] = normale[face_vn[i][0] - 1][0]; i_vn++;
                normaldata[i_vn] = normale[face_vn[i][0] - 1][1]; i_vn++;
                normaldata[i_vn] = normale[face_vn[i][0] - 1][2]; i_vn++;

                textureldata[i_vt] = texture[face_vt[i][0] - 1][0]; i_vt++;
                textureldata[i_vt] = texture[face_vt[i][0] - 1][1]; i_vt++;

                var v1 = new VertexGl(
                    new Point3d_GL(vertexdata[i_v - 3], vertexdata[i_v - 2], vertexdata[i_v - 1]),
                    new Point3d_GL(normaldata[i_vn - 3], normaldata[i_vn - 2], normaldata[i_vn - 1]),
                    new PointF(textureldata[i_vt - 2], textureldata[i_vt - 1]));
                //--------------------------------------------------------------

                vertexdata[i_v] = vertex[face_v[i][1] - 1][0]; i_v++;
                vertexdata[i_v] = vertex[face_v[i][1] - 1][1]; i_v++;
                vertexdata[i_v] = vertex[face_v[i][1] - 1][2]; i_v++;

                normaldata[i_vn] = normale[face_vn[i][1] - 1][0]; i_vn++;
                normaldata[i_vn] = normale[face_vn[i][1] - 1][1]; i_vn++;
                normaldata[i_vn] = normale[face_vn[i][1] - 1][2]; i_vn++;

                textureldata[i_vt] = texture[face_vt[i][1] - 1][0]; i_vt++;
                textureldata[i_vt] = texture[face_vt[i][1] - 1][1]; i_vt++;

                var v2 = new VertexGl(
                    new Point3d_GL(vertexdata[i_v - 3], vertexdata[i_v - 2], vertexdata[i_v - 1]),
                    new Point3d_GL(normaldata[i_vn - 3], normaldata[i_vn - 2], normaldata[i_vn - 1]),
                    new PointF(textureldata[i_vt - 2], textureldata[i_vt - 1]));
                //--------------------------------------------------------------

                vertexdata[i_v] = vertex[face_v[i][2] - 1][0]; i_v++;
                vertexdata[i_v] = vertex[face_v[i][2] - 1][1]; i_v++;
                vertexdata[i_v] = vertex[face_v[i][2] - 1][2]; i_v++;

                normaldata[i_vn] = normale[face_vn[i][2] - 1][0]; i_vn++;
                normaldata[i_vn] = normale[face_vn[i][2] - 1][1]; i_vn++;
                normaldata[i_vn] = normale[face_vn[i][2] - 1][2]; i_vn++;

                textureldata[i_vt] = texture[face_vt[i][2] - 1][0]; i_vt++;
                textureldata[i_vt] = texture[face_vt[i][2] - 1][1]; i_vt++;

                var v3 = new VertexGl(new Point3d_GL(vertexdata[i_v - 3], vertexdata[i_v - 2], vertexdata[i_v - 1]),
                    new Point3d_GL(normaldata[i_vn - 3], normaldata[i_vn - 2], normaldata[i_vn - 1]),
                    new PointF(textureldata[i_vt - 2], textureldata[i_vt - 1]));

                triangleGl[i] = new TriangleGl(v1, v2, v3);
            }
            ret.Add(vertexdata);
            ret.Add(textureldata);
            ret.Add(normaldata);

            float x_sr = (max_v[0] - min_v[0]) / 2 + min_v[0];
            float y_sr = (max_v[1] - min_v[1]) / 2 + min_v[1];
            float z_sr = (max_v[2] - min_v[2]) / 2 + min_v[2];
            _center = new Point3d_GL(x_sr, y_sr, z_sr);
            return ret.ToArray();
        }
        public List<double[,]> parsingStl_GL2(string path)
        {
            int i2 = 0;
            string file1;
            List<double[,]> ret1 = new List<double[,]>();
            using (StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII))
            {
                file1 = sr.ReadToEnd();
            }
            string[] lines = file1.Split(new char[] { '\n' });
            double[,] norm = new double[(int)(lines.Length / 7), 3];
            double[,] p1 = new double[(int)(lines.Length / 7), 3];
            double[,] p2 = new double[(int)(lines.Length / 7), 3];
            double[,] p3 = new double[(int)(lines.Length / 7), 3];
            Console.WriteLine((int)(lines.Length / 7));
            Console.WriteLine("-------------------");
            int i3 = 0;
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });
                if (vert.Length > 3)
                {
                    if (vert[1].Contains("orma"))
                    {
                        norm[i2, 0] = parseE(vert[2]);
                        norm[i2, 1] = parseE(vert[3]);
                        norm[i2, 2] = parseE(vert[4]);

                        i3 = 0;
                    }
                    else if (vert[0].Contains("ert") && i3 == 0)
                    {
                        p1[i2, 0] = parseE(vert[1]);
                        p1[i2, 1] = parseE(vert[2]);
                        p1[i2, 2] = parseE(vert[3]);
                        i3++;
                    }
                    else if (vert[0].Contains("ert") && i3 == 1)
                    {
                        p2[i2, 0] = parseE(vert[1]);
                        p2[i2, 1] = parseE(vert[2]);
                        p2[i2, 2] = parseE(vert[3]);
                        i3++;
                    }
                    else if (vert[0].Contains("ert") && i3 == 2)
                    {
                        p3[i2, 0] = parseE(vert[1]);
                        p3[i2, 1] = parseE(vert[2]);
                        p3[i2, 2] = parseE(vert[3]);
                        i2++;
                    }
                }
            }

            Console.WriteLine("-------------------");
            Console.WriteLine(i2);
            ret1.Add(norm);
            ret1.Add(p1);
            ret1.Add(p2);
            ret1.Add(p3);

            return ret1;
        }

        public Point3d_GL take3dfrom2d(PointF point)
        {
            if(triangles!=null)
            {
                for(int i=0; i<triangles.Length; i++)
                {
                    if(triangles[i].affilationPoint(point))
                    {
                        return triangles[i].v1.p;
                    }
                }
            }
            return new Point3d_GL(0, 0, 0);
        }
    }

}
