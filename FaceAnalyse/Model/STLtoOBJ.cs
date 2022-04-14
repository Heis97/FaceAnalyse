using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace FaceAnalyse
{
    
    public class STLtoOBJ
    {
        public STLtoOBJ()
        {

        }
        private double parseE(string num)
        {
            if (num.Contains("e"))
            {
                var splnum = num.Split(new char[] { 'e' });
                return Convert.ToDouble(splnum[0]) * Math.Pow(10, Convert.ToInt32(splnum[1]));
            }
            else
            {
                return Convert.ToDouble(num);
            }
            
        }

        public void stlToObj(string path)
        {

            string file1;
            using (StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII))
            {
                file1 = sr.ReadToEnd();
            }
            string[] lines = file1.Split(new char[] { '\n' });
            Vertex3f[] vertex = new Vertex3f[3*lines.Length / 7];
            Vertex3f[] normal = new Vertex3f[lines.Length / 7];

            int i_v = 0;
            int i_n = 0;
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });

                if (vert.Length > 3)
                {
                    if (vert[0].Contains("ert"))
                    {
                        vertex[i_v].x = (float)parseE(vert[1]); 
                        vertex[i_v].y = (float)parseE(vert[2]); 
                        vertex[i_v].z = (float)parseE(vert[3]); i_v++;
                    }
                    if (vert[1].Contains("orm"))
                    {
                        normal[i_n].x = (float)parseE(vert[2]);
                        normal[i_n].y = (float)parseE(vert[3]);
                        normal[i_n].z = (float)parseE(vert[4]); i_n++;
                    }

                }
            }
            if (path.Contains("."))
            {
                var pathspl = path.Split('.');
                path = pathspl[0];
            }
            path += ".obj";
            using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
            {
                for (int i =0; i<vertex.Length; i++)
                {
                    string text = "v " + vertex[i].x.ToString() + " " + vertex[i].y.ToString() + " " + vertex[i].z.ToString();
                    sw.WriteLine(text);
                }
                for (int i = 0; i < normal.Length; i++)
                {
                    string text = "vn " + normal[i].x.ToString() + " " + normal[i].y.ToString() + " " + normal[i].z.ToString();
                    sw.WriteLine(text);
                }
                sw.WriteLine("g "+path);
                sw.WriteLine("s 1");
                for (int i = 1; i < normal.Length; i++)
                {
                    string text = "f " + " " + (3 * i-2).ToString() + "//" + i.ToString() + " " + (3 * i-1).ToString() + "//" + i.ToString() + " "+(3*i).ToString()+"//"+ i.ToString();
                    sw.WriteLine(text);
                }





            }
        }
    }
}
