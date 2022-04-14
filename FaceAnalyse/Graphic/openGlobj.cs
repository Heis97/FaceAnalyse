using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceAnalyse
{

    public struct openGlobj
    {
        public enum AnimType { Static, Dynamic }
        public float[] vertex_buffer_data;
        public float[] color_buffer_data;
        public float[] normal_buffer_data;
        public float[] texture_buffer_data;
        public int vert_len;
        public PrimitiveType tp;
        public AnimType animType;
        public int id;
        public bool visible;
        public uint buff_pos;
        public uint buff_normal;
        public uint buff_color;
        public uint buff_UV;
        public Point3d_GL transl;


        public openGlobj(float[] v_buf, float[] c_buf, float[] n_buf, float[] t_buf, PrimitiveType type, int _id= -1)
        {
            vertex_buffer_data = new float[v_buf.Length];
            normal_buffer_data = new float[n_buf.Length];
            transl = new Point3d_GL(0, 0, 0);
            buff_pos = 0; buff_normal = 0; buff_color = 0; buff_UV = 0;
            if (t_buf == null)
            {
                texture_buffer_data = new float[v_buf.Length];
            }
            else
            {
                texture_buffer_data = new float[t_buf.Length];
                t_buf.CopyTo(texture_buffer_data, 0);
            }

            if (c_buf == null)
            {
                color_buffer_data = new float[v_buf.Length];
                
            }
            else
            {
                color_buffer_data = new float[c_buf.Length];
                c_buf.CopyTo(color_buffer_data, 0);
            }
            vert_len =(int) v_buf.Length / 3;
            v_buf.CopyTo(vertex_buffer_data, 0);       
            n_buf.CopyTo(normal_buffer_data, 0);
            
            tp = type;
            visible = true;
            id = _id;
            if ( _id == -1)
            {
                animType = AnimType.Static;
            }
            else
            {
                animType = AnimType.Dynamic;
            }
        }

        public openGlobj setBuffers(uint _buff_pos = 0, uint _buff_normal = 0, uint _buff_color = 0, uint _buff_UV = 0)
        {
            buff_pos = _buff_pos;
            buff_normal = _buff_normal;
            buff_color = _buff_color;
            buff_UV = _buff_UV;
            return this;
        }

    }

}
