﻿using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geometry;

namespace Graphic
{

    public class BuffersGl
    {
        public int countObj = 0;
        List<openGlobj> objs;
        public List<openGlobj> objs_dynamic;
        public List<openGlobj> objs_static;
        public BuffersGl()
        {
            objs = new List<openGlobj>();
            objs_dynamic = new List<openGlobj>();
            objs_static = new List<openGlobj>();
        }
        public int add_obj(openGlobj opgl_obj)
        {
            if (opgl_obj.animType == openGlobj.AnimType.Dynamic)
            {
                objs_dynamic.Add(opgl_obj);
                countObj++;
                return countObj - 1;
            }
            else
            {
                objs.Add(opgl_obj);
                
                return -1;
            }               
        }
        public void sortObj()
        {
            objs_static = new List<openGlobj>();
            foreach (PrimitiveType val_tp in Enum.GetValues(typeof(PrimitiveType)))
            {
                var vertex_buffer_data = new List<float>();
                var color_buffer_data = new List<float>();
                var normal_buffer_data = new List<float>();
                var texture_buffer_data = new List<float>();

                for (int i = 0; i < objs.Count; i++)
                {
                    if (objs[i].tp == val_tp && objs[i].animType == openGlobj.AnimType.Static)
                    {
                        vertex_buffer_data.AddRange(objs[i].vertex_buffer_data);
                        color_buffer_data.AddRange(objs[i].color_buffer_data);
                        normal_buffer_data.AddRange(objs[i].normal_buffer_data);
                        texture_buffer_data.AddRange(objs[i].texture_buffer_data);

                    }
                }

                if (vertex_buffer_data.Count > 2)
                {

                    objs_static.Add(new openGlobj(vertex_buffer_data.ToArray(), color_buffer_data.ToArray(), normal_buffer_data.ToArray(), texture_buffer_data.ToArray(), val_tp));
                }

            }
        }

        public void removeObj(int id)
        {
            objs_dynamic[id] = new openGlobj();
        }

        #region setters
        public void setScale(int id, int i, float _scale)
        {
            objs_dynamic[id] = objs_dynamic[id].setScale(i, _scale);
        }
        public void setTransfObj(int id, int i, Point3d_GL _transl, Point3d_GL _rotate)
        {
            objs_dynamic[id] = objs_dynamic[id].setTransf(i, _transl, _rotate);
        }
        public void setXobj(int id, int i, double x)
        {
            objs_dynamic[id] = objs_dynamic[id].setX(i, x);
        }
        public void setYobj(int id, int i, double y)
        {
            objs_dynamic[id] = objs_dynamic[id].setY(i, y);
        }
        public void setZobj(int id, int i, double z)
        {
            objs_dynamic[id] = objs_dynamic[id].setZ(i, z);
        }

        public void setRotXobj(int id, int i, double x)
        {
            objs_dynamic[id] = objs_dynamic[id].setRotX(i, x);
        }
        public void setRotYobj(int id, int i, double y)
        {
            objs_dynamic[id] = objs_dynamic[id].setRotY(i, y);
        }
        public void setRotZobj(int id, int i, double z)
        {
            objs_dynamic[id] = objs_dynamic[id].setRotZ(i,z);
        }
        public void setMatrobj(int id, int i, Matrix4x4f matr)
        {
            objs_dynamic[id] = objs_dynamic[id].setMatr(i, matr);
        }
        public void addMatrobj(int id, int i, Matrix4x4f matr)
        {
            objs_dynamic[id] = objs_dynamic[id].addMatr(i, matr);
        }
        public void setTranspobj(int id,  float transp)
        {
            objs_dynamic[id] = objs_dynamic[id].setTrasp(transp);
        }
        #endregion


    }


}
