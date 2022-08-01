using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.Util;
using Geometry;

namespace Graphic
{   
    public enum modeGL { Paint, View}
    public enum viewType { Perspective, Ortho }
    public class TextureGL
    {
        
        public uint id;
        public int binding;
        public int w, h, ch;
        public PixelFormat pixelFormat;
        InternalFormat internalFormat;
        public float[] data;
        public TextureGL()
        {
        }
        public TextureGL(int _binding, int _w, int _h = 1, PixelFormat _pixelFormat = PixelFormat.Red, float[] _data = null)
        {
            Console.WriteLine("genTexture");
            if (_data != null)
            {
                data = (float[])_data.Clone();
            }
            else
            {
                data = null;
            }
            var buff = genTexture(_binding, _w, _h, _pixelFormat, data);
            id = buff;
            binding = _binding;
            w = _w;
            h = _h;
            pixelFormat = _pixelFormat;
            Console.WriteLine("bind " + binding + "; w " + w + " h " + h + " ch " + ch + "; " + pixelFormat);
        }
        public float[] getData()
        {
            Gl.BindTexture(TextureTarget.Texture2d, id);
            float[] dataf = new float[w * h * ch];
            Gl.GetTexImage(TextureTarget.Texture2d, 0, pixelFormat, PixelType.Float, dataf);
            //Console.WriteLine(w+" "+h+" "+ch+" "+ dataf.Length);
            return dataf;
        }
        public void setData(float[] data)
        {
            Gl.BindTexture(TextureTarget.Texture2d, id);
            Gl.TexImage2D(TextureTarget.Texture2d, 0, internalFormat, w, h, 0, pixelFormat, PixelType.Float, data);
        }
        uint genTexture(int binding, int w, int h = 1, PixelFormat pixelFormat = PixelFormat.Red, float[] data = null)
        {
            if (pixelFormat == PixelFormat.Red)
            {
                ch = 1;
            }
            else if (pixelFormat == PixelFormat.Rg)
            {
                ch = 2;
            }
            else if (pixelFormat == PixelFormat.Rgb)
            {
                ch = 3;
            }
            else if (pixelFormat == PixelFormat.Rgba)
            {
                ch = 4;
            }
            else
            {
                ch = 1;
            }
            var buff_texture = Gl.GenTexture();
            Gl.ActiveTexture(TextureUnit.Texture0 + binding);
            Gl.BindTexture(TextureTarget.Texture2d, buff_texture);

            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.NEAREST);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.NEAREST);
            internalFormat = InternalFormat.R32f;
            if (pixelFormat == PixelFormat.Rgb || pixelFormat == PixelFormat.Rgba)
            {
                internalFormat = InternalFormat.Rgba32f;
            }
            Gl.TexImage2D(TextureTarget.Texture2d, 0, internalFormat, w, h, 0, pixelFormat, PixelType.Float, data);

            Gl.BindImageTexture((uint)binding, buff_texture, 0, false, 0, BufferAccess.ReadWrite, internalFormat);
            return buff_texture;
        }

        private void useTexture()
        {
            Gl.ActiveTexture(TextureUnit.Texture0 + binding);
            Gl.BindTexture(TextureTarget.Texture2d, id);
        }
    }
    public  class IDs
    {
        public uint buff_tex;
        public uint buff_tex1;
        public uint programID;
        public uint vert;
        public int[] LocationVPs = new int[4];
        public int[] LocationVs = new int[4];
        public int[] LocationPs = new int[4];
        public int LocationM;
        public int LightID;
        public int textureVisID;
        public int lightVisID;
        public int LightPowerID;
        public int MaterialDiffuseID;
        public int MaterialAmbientID;
        public int currentMonitor = 1;
        public int MaterialSpecularID;
        public int TextureID;
        public int MouseLocID;
        public int MouseLocGLID;
        public int translMeshID;
        public int comp_proj_ID;
        public int render_count_ID;
        public int show_faces_ID;
        public int transparency_ID;
        public int inv_norm_ID;
        
    }
    public class GraphicGL
    {
        #region vars
        static float PI = 3.1415926535f;
        public int inv_norm = 0;
        public int show_faces = 0;
        public int startGen = 0;
        public int saveImagesLen = 0;
        public int render_delim = 150000;
        public int render_count = 0;
        public viewType typeProj = viewType.Perspective;
        Size sizeControl;
        Point lastPos;
        public int comp_proj = 0;
        int currentMonitor = 1;

        public int lightVis = 0;
        public int textureVis = 0;
        float LightPower = 500000.0f;
        Label Label_cor;
        Label Label_cor_cur;
        Label Label_trz_cur;
        RichTextBox debug_box;
        public BuffersGl buffersGl = new BuffersGl();
        public Matrix4x4f[] VPs;
        public Matrix4x4f[] Vs;
        public Matrix4x4f[] Ps;
        public Vertex2f MouseLoc;
        public Vertex2f MouseLocGL;
        Vertex3f translMesh = new Vertex3f(0.0f, 0.0f, 0.0f);
        Vertex3f lightPos = new Vertex3f(4.0f, 0.0f, 35.0f);
        Vertex3f MaterialDiffuse = new Vertex3f(0.5f, 0.5f, 0.5f);
        Vertex3f MaterialAmbient = new Vertex3f(0.1f, 0.1f, 0.1f);
        Vertex3f MaterialSpecular = new Vertex3f(0.1f, 0.1f, 0.1f);
        
        public modeGL modeGL = modeGL.View;
        List<Point3d_GL> pointsPaint = new List<Point3d_GL>();
        Point3d_GL curPointPaint = new Point3d_GL(0, 0, 0);
        public List<TransRotZoom> transRotZooms = new List<TransRotZoom>();
        
        public List<TransRotZoom[]> trzForSave;
        public int[] monitorsForGenerate;
        public string pathForSave;
        public ImageBox[] imageBoxesForSave;
        public Size size = new Size(1,1);
        public Mat pict = new Mat();
        byte[] textureB;
        Size textureSize;
        public Bitmap bmp;
        IDs idsPs = new IDs();
        IDs idsLs = new IDs();
        IDs idsTs = new IDs();
        IDs idsTsOne = new IDs();
        IDs idsPsOne = new IDs();
        IDs idsLsOne = new IDs();
        IDs idsCs = new IDs();
        public int landmark_len = 100;

        public TextureGL landmark2d_data, landmark3d_data;
        public List<float[]> dataComputeShader = new List<float[]>();
        bool initComputeShader = false;
        public float[] resultComputeShader;

        #endregion 

        public void glControl_Render(object sender, GlControlEventArgs e)
        {

            VPs = new Matrix4x4f[4];
            Vs = new Matrix4x4f[4];
            Ps = new Matrix4x4f[4];
            var txt = "";
            for (int i = 0; i < transRotZooms.Count; i++)
            {
                Gl.ViewportIndexed((uint)i,
                    transRotZooms[i].rect.X,
                    transRotZooms[i].rect.Y,
                    transRotZooms[i].rect.Width,
                    transRotZooms[i].rect.Height);
                var retM = transRotZooms[i].getVPmatrix();               
                VPs[i] = retM[2];
                Vs[i] = retM[1];
                Ps[i] = retM[0];
              /*  Console.WriteLine("Vs[i]");
                Console.WriteLine(Vs[i]);
                Console.WriteLine(Ps[i]);*/
                txt += "TRZ " + i + ": "+transRotZooms[i].getInfo(transRotZooms.ToArray()).ToString()+"\n";
            }

            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (buffersGl.objs_static!=null)
            {
                if (buffersGl.objs_static.Count!=0)
                {
                    foreach(var opglObj in buffersGl.objs_static)
                    {
                        renderGlobj(opglObj);   
                    }
                }
            }

            if (buffersGl.objs_dynamic != null)
            {
                if (buffersGl.objs_dynamic.Count != 0)
                {
                    foreach (var opglObj in buffersGl.objs_dynamic)
                    {
                        renderGlobj(opglObj);
                    }
                }
            }
            
            render_count++;
            if(render_count%render_delim==0)
            {
                render_count = 0;
            }
        }

        void renderGlobj(openGlobj opgl_obj)
        {
            if(opgl_obj.visible)
            {
                try
                {
                    var ids = new IDs();
                    if (opgl_obj.tp == PrimitiveType.Points)
                    {
                        ids = idsPs;
                        if (opgl_obj.count == 1)
                        {
                            ids = idsPsOne;
                        }
                    }
                    else if (opgl_obj.tp == PrimitiveType.Triangles)
                    {
                        ids = idsTs;
                        if(opgl_obj.count==1)
                        {
                            ids = idsTsOne;
                        }
                    }
                    else if (opgl_obj.tp == PrimitiveType.Lines)
                    {
                        ids = idsLs;
                        if (opgl_obj.count == 1)
                        {
                            ids = idsLsOne;
                        }
                    }
                    load_vars_gl(ids, opgl_obj);
                    opgl_obj.useBuffers();
                    if(opgl_obj.count > 1)
                    {
                        opgl_obj.loadModels();
                        Gl.DrawArraysInstanced(opgl_obj.tp, 0, opgl_obj.vert_len, opgl_obj.count);
                    }
                    else
                    {
                        Gl.DrawArrays(opgl_obj.tp, 0, opgl_obj.vert_len);                       
                    }
                }
                catch
                {
                }
            }
        }

        public void glControl_ContextDestroying(object sender, GlControlEventArgs e)
        {
        }

        public void glControl_ContextCreated(object sender, GlControlEventArgs e)
        {
            sizeControl = ((Control)sender).Size;
            Gl.Initialize();
            Gl.Enable(EnableCap.Multisample);
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha,BlendingFactor.OneMinusSrcAlpha);
            Gl.ClearColor(0.9f, 0.9f, 0.95f, 0.0f);
            Gl.PointSize(5f);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            var VertexSourceGL = assembCode(new string[] { @"Graphic\Shaders\Vert\VertexSh_Models.glsl" });
            var VertexOneSourceGL = assembCode(new string[] { @"Graphic\Shaders\Vert\VertexSh_ModelsOne.glsl" });

            var FragmentSourceGL = assembCode(new string[] { @"Graphic\Shaders\Frag\FragmSh.glsl" });
            var FragmentSimpleSourceGL = assembCode(new string[] { @"Graphic\Shaders\Frag\FragmSh_Simple.glsl" });

            var GeometryShaderPointsGL = assembCode(new string[]    { @"Graphic\Shaders\Geom\GeomSh_Points.glsl"});
            var GeometryShaderLinesGL = assembCode(new string[]     { @"Graphic\Shaders\Geom\GeomSh_Lines.glsl"});
            var GeometryShaderTrianglesGL = assembCode(new string[] { @"Graphic\Shaders\Geom\GeomSh_Triangles.glsl"});


            idsLs.programID = createShader(VertexSourceGL, GeometryShaderLinesGL, FragmentSimpleSourceGL);
            idsLsOne.programID = createShader(VertexOneSourceGL, GeometryShaderLinesGL, FragmentSimpleSourceGL);

            idsPs.programID = createShader(VertexSourceGL, GeometryShaderPointsGL, FragmentSimpleSourceGL);
            idsPsOne.programID = createShader(VertexOneSourceGL, GeometryShaderPointsGL, FragmentSimpleSourceGL);

            idsTs.programID = createShader(VertexSourceGL, GeometryShaderTrianglesGL , FragmentSourceGL);
            idsTsOne.programID = createShader(VertexOneSourceGL, GeometryShaderTrianglesGL, FragmentSourceGL);

            //idsCs.programID = createShaderCompute(ComputeSourceGL);
            
            init_vars_gl(idsLs);
            init_vars_gl(idsPs);
            init_vars_gl(idsTs);
            init_vars_gl(idsTsOne);
            init_vars_gl(idsPsOne);
            init_vars_gl(idsLsOne);

            //initComputeShader = init_textures(dataComputeShader);
            landmark2d_data = new TextureGL(1, landmark_len, 1, PixelFormat.Rgba);
            landmark3d_data = new TextureGL(2, landmark_len, 1, PixelFormat.Rgba);
            //Gl.Enable(EnableCap.CullFace);
           Gl.Enable(EnableCap.DepthTest);
        }

        #region addBuffer

        void bufferToCompute(float[] data, int locat)
        {
            var dat_buff = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ShaderStorageBuffer, dat_buff);
            Gl.BufferData(BufferTarget.ShaderStorageBuffer, (uint)(4 * data.Length), data, BufferUsage.StaticDraw);
            Gl.BindBufferBase(BufferTarget.ShaderStorageBuffer, (uint)locat, dat_buff);
            // Gl.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }
        public void SortObj()
        {
            buffersGl.sortObj();
            if (buffersGl.objs_static != null)
            {
                if (buffersGl.objs_static.Count != 0)
                {
                    for(int i=0; i< buffersGl.objs_static.Count;i++)
                    {
                        buffersGl.objs_static[i] = buffersGl.objs_static[i].setBuffers();
                    }
                }
            }
        }
        #endregion

        private void init_vars_gl(IDs ids)
        {
            Gl.UseProgram(ids.programID);

            for (int i = 0; i < 4; i++)
            {
                ids.LocationVPs[i] = Gl.GetUniformLocation(ids.programID, "VPs[" + i + "]");
                ids.LocationVs[i] = Gl.GetUniformLocation(ids.programID, "Vs[" + i + "]");
                ids.LocationPs[i] = Gl.GetUniformLocation(ids.programID, "Ps[" + i + "]");
            }
            ids.LocationM = Gl.GetUniformLocation(ids.programID, "ModelMatrix");
            ids.TextureID  = Gl.GetUniformLocation(ids.programID, "textureSample");
            ids.MaterialDiffuseID = Gl.GetUniformLocation(ids.programID, "MaterialDiffuse");
            ids.MaterialAmbientID = Gl.GetUniformLocation(ids.programID, "MaterialAmbient");
            ids.MaterialSpecularID = Gl.GetUniformLocation(ids.programID, "MaterialSpecular");
            ids.LightID = Gl.GetUniformLocation(ids.programID, "LightPosition_world");
            ids.LightPowerID = Gl.GetUniformLocation(ids.programID, "lightPower");

            ids.textureVisID = Gl.GetUniformLocation(ids.programID, "textureVis");
            ids.lightVisID = Gl.GetUniformLocation(ids.programID, "lightVis");

            ids.MouseLocID = Gl.GetUniformLocation(ids.programID, "MouseLoc");
            ids.MouseLocGLID = Gl.GetUniformLocation(ids.programID, "MouseLocGL");

            ids.comp_proj_ID = Gl.GetUniformLocation(ids.programID, "comp_proj");
            ids.render_count_ID = Gl.GetUniformLocation(ids.programID, "render_count");
            ids.show_faces_ID = Gl.GetUniformLocation(ids.programID, "show_faces");
            ids.transparency_ID = Gl.GetUniformLocation(ids.programID, "transparency");
            ids.inv_norm_ID = Gl.GetUniformLocation(ids.programID, "inv_norm");
        }
        private void load_vars_gl(IDs ids, openGlobj openGlobj)
        {

            Gl.UseProgram(ids.programID);
            if(openGlobj.count==1)
            {
                var ModelMatr = openGlobj.trsc[0].getModelMatrix();
                Gl.UniformMatrix4f(ids.LocationM, 1, false, ModelMatr);
            }
            
            //Console.WriteLine(ModelMatr);

            for (int i = 0; i < 4; i++)
            {
                Gl.UniformMatrix4f(ids.LocationVPs[i], 1, false, VPs[i]);
                Gl.UniformMatrix4f(ids.LocationVs[i], 1, false, Vs[i]);
                Gl.UniformMatrix4f(ids.LocationPs[i], 1, false, Ps[i]);
            }
            
            Gl.Uniform3f(ids.MaterialDiffuseID, 1, MaterialDiffuse);
            Gl.Uniform3f(ids.MaterialAmbientID, 1, MaterialAmbient);
            Gl.Uniform3f(ids.MaterialSpecularID, 1, MaterialSpecular);
            Gl.Uniform3f(ids.LightID, 1, lightPos);
            Gl.Uniform1f(ids.LightPowerID, 1, LightPower);
            Gl.Uniform2f(ids.MouseLocID, 1, MouseLoc);
            Gl.Uniform2f(ids.MouseLocGLID, 1, MouseLocGL);
            Gl.Uniform2f(ids.MouseLocGLID, 1, MouseLocGL);

            if(openGlobj.Textureid>0)
            {
                useTexture((uint)openGlobj.Textureid);
            }
            

            Gl.Uniform1i(ids.textureVisID, 1, textureVis);
            Gl.Uniform1i(ids.lightVisID, 1, lightVis);
            Gl.Uniform1i(ids.comp_proj_ID, 1, comp_proj);
            Gl.Uniform1i(ids.render_count_ID, 1, render_count);
            Gl.Uniform1i(ids.show_faces_ID, 1, show_faces);
            Gl.Uniform1f(ids.transparency_ID, 1, openGlobj.transparency);
            Gl.Uniform1i(ids.inv_norm_ID, 1, inv_norm);

        }

        #region texture
        private void useTexture(uint buff_texture)
        {
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2d, buff_texture);
        }
        Bitmap byteToBitmap(byte[] arr, Size size)
        {
            var bmp = new Bitmap(size.Width, size.Height);
            for (int i = 0; i < size.Width; i++)
            {
                for (int j = 0; j < size.Height; j++)
                {
                    // Console.WriteLine(3 * (j * size.Width + j));
                    var color = Color.FromArgb(
                        arr[3 * (j * size.Width + i)],
                        arr[3 * (j * size.Width + i) + 1],
                        arr[3 * (j * size.Width + i) + 2]
                        );
                    bmp.SetPixel(i, j, color);
                }
            }
            return bmp;
        }
        byte[] textureLoad(Mat mat)
        {

            var bytearr = (byte[,,])mat.GetData();
            Console.WriteLine(mat.Rows + " " + mat.Cols + " " + mat.NumberOfChannels + " ");
            var bytetext = new byte[bytearr.GetLength(0) * bytearr.GetLength(1) * bytearr.GetLength(2)];
            Console.WriteLine(bytearr.GetLength(0));
            Console.WriteLine(bytearr.GetLength(1));
            Console.WriteLine(bytearr.GetLength(0) * bytearr.GetLength(1) * 3);
            Console.WriteLine("___");
            int ind = 0;

            for (int i = 0; i < bytearr.GetLength(0); i++)
            {
                for (int j = 0; j < bytearr.GetLength(1); j++)
                {
                    bytetext[ind] = bytearr[bytearr.GetLength(0) - i - 1, j, 0]; ind++;
                    bytetext[ind] = bytearr[bytearr.GetLength(0) - i - 1, j, 1]; ind++;
                    bytetext[ind] = bytearr[bytearr.GetLength(0) - i - 1, j, 2]; ind++;
                }
            }
            Console.WriteLine(ind);
            textureSize = new Size(mat.Width, mat.Height);
            return bytetext;
        }
        private uint bindTexture(byte[] arrB)
        {
            var buff_texture = Gl.GenTexture();
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2d, buff_texture);
            // Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, Gl.REPEAT);

            Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgb, textureSize.Width, textureSize.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, arrB);
            Gl.GenerateMipmap(TextureTarget.Texture2d);
            return buff_texture;
        }

        #endregion

        #region util
        public string toStringBuf(float[] buff, int strip, int substrip, string name)
        {
            if (buff == null)
                return name + " null ";
            string txt = name + " " + buff.Length;
            for (int i = 0; i < buff.Length / strip; i++)
            {
                txt += "  | \n";
                for (int j = 0; j < strip; j++)
                {
                    
                    if (substrip != 0)
                    {
                        if (j % substrip == 0)
                        {
                            txt += "  | ";
                        }
                        txt += buff[i * strip + j].ToString() + ", ";
                    }
                    else
                    {
                        txt += buff[i * strip + j].ToString() + ", ";
                    }

                }
            }
            txt += " |\n--------------------------------\n";
            return txt;

        }
        public void SaveToFolder(string folder,int id)
        {
            var bitmap = matFromMonitor(id);
            var invVm = Vs[id].Inverse;
            var trz_in = transRotZooms[selectTRZ_id(id)];
            var trz = trz_in.getInfo(transRotZooms.ToArray());
            //Gl.depth
            var path_gl = Path.Combine(folder, "monitor_" + id.ToString());
            Directory.CreateDirectory(path_gl);
            bitmap.Save(path_gl + "/" + trz.ToString() + ".png");
        }
        public Mat matFromMonitor(int id)
        {
            var selecTrz = selectTRZ_id(id);
            if(selecTrz<0)
            {
                return null;
            }
            var trz = transRotZooms[selectTRZ_id(id)];
            var recTRZ = trz.rect;
            var data = new Mat(recTRZ.Width, recTRZ.Height, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
            Gl.ReadPixels(recTRZ.X, recTRZ.Y, recTRZ.Width, recTRZ.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.DataPointer);
            //CvInvoke.Rotate(data, data, Emgu.CV.CvEnum.RotateFlags.Rotate180);
            //CvInvoke.Flip(data, data, Emgu.CV.CvEnum.FlipType.Vertical);
            return data;
        }
        
        Geometry.PointF toGLcord(Geometry.PointF pf)
        {
            var sizeView = transRotZooms[0].rect;

            var x = (sizeView.Width / 2) * pf.X + sizeView.Width / 2;
            var y = -((sizeView.Width / 2) * pf.Y) + sizeView.Height / 2;
            return new Geometry.PointF(x, y);
        }
        Geometry.PointF toTRZcord(Geometry.PointF pf)
        {
            var sizeView = transRotZooms[0].rect;

            var x = (sizeView.Width / 2) * pf.X + sizeView.Width / 2;
            var y = (sizeView.Width / 2) * pf.Y + sizeView.Height / 2;
            return new Geometry.PointF(x, y);
        }
        int selectTRZ_id(int id)
        {
            int ind = 0;
            foreach (var trz in transRotZooms)
            {
                if (trz.id == id)
                {
                    return ind;
                }
                ind++;
            }
            return -1;
        }

        
        /// <summary>
        /// 3dGL->2dIm
        /// </summary>
        /// <param name="point"></param>
        /// <param name="mvp"></param>
        /// <returns></returns>
        public Geometry.PointF calcPixel(Vertex4f point, int id)
        {
            var p2 = VPs[id] * point;
            p2.Normalize();
            var p3 = toTRZcord(new Geometry.PointF(p2.x, p2.y));
            
            //Console.WriteLine("v: " + p3.X + " " + p3.Y + " " + p2.z + " " + p2.w + " mvp_len: " + MVPs[0].ToString());
            return p3;
        }

        public Geometry.PointF calcPixelInv(Vertex4f point, Matrix4x4f mvp)
        {
            var p2 = mvp.Inverse * point;
            var p4 = p2 / p2.w;
            var p3 = toGLcord(new Geometry.PointF(p4.x, p4.y));
            //Console.WriteLine("v: " + p3.X + " " + p3.Y + " " + p2.z + " " + p2.w + " mvp_len: " + MVPs[0].ToString());
            return p3;
        }

        public void printDebug(RichTextBox box)
        {
            string txt = "";
            txt += "\n______STATIC_________\n";
            foreach (var ob in buffersGl.objs_static)
            {
                txt += toStringBuf(ob.vertex_buffer_data, 3, "vert");
                txt += toStringBuf(ob.color_buffer_data, 3, "color");
                txt += toStringBuf(ob.normal_buffer_data, 3, "normal");
                txt += toStringBuf(ob.texture_buffer_data, 2, "textUV");
                txt += "\n________________________\n";
            }
            txt += "\n______DYNAMIC_________\n";
            foreach (var ob in buffersGl.objs_dynamic)
            {
                txt += toStringBuf(ob.vertex_buffer_data, 3, "vert");
                txt += toStringBuf(ob.color_buffer_data, 3, "color");
                txt += toStringBuf(ob.normal_buffer_data, 3, "normal");
                txt += toStringBuf(ob.texture_buffer_data, 2, "textUV");
                txt += "\n________________________\n";
            }
            
            box.Text = txt;
        }

        string toStringBuf(float[] buff, int strip,string name)
        {
            if (buff == null)
                return name + " null ";
            string txt = name +" "+buff.Length;
            for (int i = 0; i < buff.Length / strip; i++)
            {
                txt += "  | ";
                for(int j=0; j<strip; j++)
                {
                    txt += buff[i * strip+j].ToString() + ", ";
                }
            }
            txt += " |\n--------------------------------\n";
            return txt;
       
        }
        #endregion

       
        #region mouse
        public void add_Label(Label label_list, Label label_cur, Label label_trz)
        {
            Label_trz_cur = label_trz;
            Label_cor = label_list;
            Label_cor_cur = label_cur;
            if (Label_cor == null || Label_cor_cur==null || Label_trz_cur==null) 
            {
                Console.WriteLine("null_start");
            }
            
        }
        public void add_TextBox(RichTextBox richTextBox)
        {
            debug_box = richTextBox;
        }
        public void addMonitor(Rectangle rect,int id)
        {
            transRotZooms.Add(new TransRotZoom(rect,id));
        }
        public void addMonitor(Rectangle rect, int id, Vertex3d rotVer, Vertex3d transVer, int _idMast)
        {
            transRotZooms.Add(new TransRotZoom(rect, id, rotVer, transVer, _idMast));
        }
        int selectTRZ(MouseEventArgs e)
        {
            int ind = 0;
            foreach(var trz in transRotZooms)
            {
                var recGL = new Rectangle(trz.rect.X, sizeControl.Height - trz.rect.Y - trz.rect.Height, trz.rect.Width, sizeControl.Height - trz.rect.Y);
                if(recGL.Contains(e.Location))
                {
                    return ind;
                }
                ind++;
            }
            return -1;
        }


        public void glControl_MouseDown(object sender, MouseEventArgs e)
        {           
            switch(modeGL)
            {
                case modeGL.View:
                    lastPos = e.Location;
                    break;
                case modeGL.Paint:
                    if (e.Button == MouseButtons.Left)
                    {
                        pointsPaint.Add(curPointPaint);
                        
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        pointsPaint.Clear();
                    }
                    break;
            }
        }

        public void glControl_MouseMove(object sender, MouseEventArgs e)
        {

            var cont = (Control)sender;
            int sel_trz = selectTRZ(e);
            if(sel_trz < 0)
            {
                return;
            }
            var trz = transRotZooms[sel_trz];
            if(Label_cor_cur!=null)
            {
                Label_cor_cur.Text = e.X + " " + e.Y;
            }
            MouseLocGL = new Vertex2f(
                (float)trz.zoom*((float)e.X / (0.5f * (float)cont.Width) - 1f),
                -((float)trz.zoom * ((float)e.Y / (0.5f * (float)cont.Height) - 1f)));

            int w = trz.rect.Width;
            int h = trz.rect.Height;
            switch (modeGL)
            {
                case modeGL.View:
                    
                    var dx = e.X - lastPos.X;
                    var dy = e.Y - lastPos.Y;
                    double dyx = lastPos.Y - w / 2;
                    double dxy = lastPos.X - h / 2;
                    var delim = (Math.Sqrt(dy * dy + dx * dx) * Math.Sqrt(dxy * dxy + dyx * dyx));
                    double dz = 0;
                    if (delim != 0)
                    {
                        dz = (dy * dxy + dx * dyx) / delim;

                    }
                    else
                    {
                        dz = 0;
                    }
                    if (e.Button == MouseButtons.Left)
                    {
                        trz.xRot += dy;
                        trz.yRot += dx;
                        //trz.zRot += dz;
                        
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        var flow_mul = 0.1;
                        trz.off_x += flow_mul* Convert.ToDouble(dx);
                        trz.off_y += flow_mul * Convert.ToDouble(dy);
                    }
                    lastPos = e.Location;
                    break;
                case modeGL.Paint:
                    var p_XY = new Point3d_GL((double)e.Location.X/ (0.5*(double)w), (double)e.Location.Y/(0.5* (double)h), 0);
                   // var p_YZ = new Point3d_GL(0,(double)e.Location.X / (0.5 * (double)w), (double)e.Location.Y / (0.5 * (double)h));
                   // var p_ZX = new Point3d_GL((double)e.Location.X / (0.5 * (double)w),0, (double)e.Location.Y / (0.5 * (double)h));
                    try
                    {
                        var invM = Ps[0].Inverse;
                        //var invM = MVPs[0].Inverse;
                        if (Label_cor != null)
                        {
                            curPointPaint = invM * p_XY;
                            Label_cor.Text = curPointPaint.ToString() + "\n" + "\n";//;// + (invM * p_YZ).ToString() + "\n" + (invM * p_ZX).ToString();
                            if(pointsPaint.Count!=0)
                            {
                                foreach(var p in pointsPaint)
                                {
                                    Label_cor.Text += p.ToString() + "\n";
                                }
                            }

                            if (pointsPaint.Count > 1)
                            {
                                var dis = (pointsPaint[pointsPaint.Count - 1] - pointsPaint[pointsPaint.Count - 2]).magnitude();
                                Label_cor.Text +="dist = "+ Math.Round(dis,4).ToString() + "\n";
                            }
                        }
                    }
                    catch
                    {

                    }
                    
                    break;
            }
            transRotZooms[sel_trz] = trz;
        }
        public void Form1_mousewheel(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("P m = " + Pm);
            //Console.WriteLine("V m = " + Vm);
            // var invVm = Vm.Inverse;
            //Console.WriteLine("invV m = " + invVm);
            int sel_trz = selectTRZ(e);
            if (sel_trz < 0)
            {
                return;
            }
            var trz = transRotZooms[sel_trz];
            var angle = e.Delta;
            if (angle > 0)
            {
                
                    trz.zoom = 0.7 * trz.zoom;
                    //trz.zoom = Math.Round(trz.zoom, 4);
                
            }
            else
            {
                trz.zoom = 1.3 * trz.zoom;
                //trz.zoom = Math.Round(trz.zoom, 4);
            }
            transRotZooms[sel_trz] = trz;
        }


        public void lightPowerScroll(int value)
        {
            var f = (float)value*100;
            LightPower = f;
        }
        public void diffuseScroll(int value)
        {
            var f = (float)value / 10;
            MaterialDiffuse.x = f;
            MaterialDiffuse.y = f;
            MaterialDiffuse.z = f;
        }
        public void ambientScroll(int value)
        {
            var f = (float)value / 10;
            MaterialAmbient.x = f;
            MaterialAmbient.y = f;
            MaterialAmbient.z = f;
        }
        public void specularScroll(int value)
        {

            var f = (float)value / 10;
            MaterialSpecular.x = f;
            MaterialSpecular.y = f;
            MaterialSpecular.z = f;
        }
        
        public void lightXscroll(int value)
        {
            lightPos.x = (float)value * 10;
        }
        public void lightYscroll(int value)
        {
            lightPos.y = (float)value * 10;

        }
        public void lightZscroll(int value)
        {
            lightPos.z = (float)value * 10;
        }
        public void orientXscroll(int value)
        {
            var trz = transRotZooms[currentMonitor];
            trz.setxRot(value);
            transRotZooms[currentMonitor] = trz;
        }
        public void orientYscroll(int value)
        {
            var trz = transRotZooms[currentMonitor];
            trz.setyRot(value);
            transRotZooms[currentMonitor] = trz;
        }
        public void orientZscroll(int value)
        {
            var trz = transRotZooms[currentMonitor];
            trz.setzRot(value);
            transRotZooms[currentMonitor] = trz;
        }

        public void planeXY()
        {
            currentMonitor = 0;
            var trz = transRotZooms[currentMonitor];
            trz.setRot(0, 0, 0);
            transRotZooms[currentMonitor] = trz;
        }
        public void planeYZ()
        {
            var trz = transRotZooms[currentMonitor];
            trz.setRot(0, 90, 0);
            transRotZooms[currentMonitor] = trz;
        }
        public void planeZX()
        {
            var trz = transRotZooms[currentMonitor];
            trz.setRot(90, 0, 0);
            transRotZooms[currentMonitor] = trz;
        }
        public void changeViewType(int ind)
        {
            if (ind >= 0 && ind<transRotZooms.Count)
            {
                var trz = transRotZooms[ind];
                if(trz.viewType_==viewType.Ortho)
                {
                    trz.viewType_ = viewType.Perspective;
                    transRotZooms[ind] = trz;
                }
                else if (trz.viewType_ == viewType.Perspective)
                {
                    trz.viewType_ = viewType.Ortho;
                    transRotZooms[ind] = trz;
                }

            }
        }

        public void changeVisible(int ind)
        {
            if (ind >= 0 && ind < transRotZooms.Count)
            {
                var trz = transRotZooms[ind];
                if (trz.visible == true)
                {
                    trz.visible = false;
                    transRotZooms[ind] = trz;
                }
                else if (trz.visible == false)
                {
                    trz.visible = true;
                    transRotZooms[ind] = trz;
                }

            }
        }
        public void setMode(modeGL mode)
        {
            modeGL = mode;
        }
        #endregion
        #region mesh
        public int addFlat3d_XY(double z = 0)
        {
            double x = 1;
            double y = 1;

            var p0 = new Point3d_GL(x ,y, z);
            var p1 = new Point3d_GL(x, -y, z);
            var p2 = new Point3d_GL(-x, -y, z);
            var p3 = new Point3d_GL(-x, y, z);

            var ps = new Point3d_GL[]
            {
                p0,p1,p2,
                p0,p2,p3
            };
            //addMesh(Point3d_GL.toMesh(ps), PrimitiveType.Triangles);
            var mesh = Point3d_GL.toMesh(ps);
            return add_buff_gl_dyn_col(mesh, computeNormals(mesh), new Vertex3f(0, 0, 1), PrimitiveType.Triangles);
        }

        public int addFlat3d_ZY(double x = 0)
        {
            double z = 1;
            double y = 1;

            var p0 = new Point3d_GL(x, y, z);
            var p1 = new Point3d_GL(x, y, -z);
            var p2 = new Point3d_GL(x, -y, -z);
            var p3 = new Point3d_GL(x, -y, z);

            var ps = new Point3d_GL[]
            {
                p0,p1,p2,
                p0,p2,p3
            };
            //addMesh(Point3d_GL.toMesh(ps), PrimitiveType.Triangles);
            var mesh = Point3d_GL.toMesh(ps);
            return add_buff_gl_dyn_col(mesh, computeNormals(mesh), new Vertex3f(1, 0, 0), PrimitiveType.Triangles);
        }
        public int addFlat3d_XY_zero(double z = 0)
        {
            Flat3d_GL flat3D_GL = new Flat3d_GL(new Point3d_GL(10, 0, z), new Point3d_GL(10, 10, z), new Point3d_GL(0, 10, z));
            var p0 = (new Line3d_GL(new Vector3d_GL(0, 0, 10), new Point3d_GL(-50, -10, 0))).calcCrossFlat(flat3D_GL);
            var p1 = (new Line3d_GL(new Vector3d_GL(0, 0, 10), new Point3d_GL(50, -10, 0))).calcCrossFlat(flat3D_GL);

            var p2 = (new Line3d_GL(new Vector3d_GL(0, 0, 10), new Point3d_GL(-50, 100, 0))).calcCrossFlat(flat3D_GL);
            var p3 = (new Line3d_GL(new Vector3d_GL(0, 0, 10), new Point3d_GL(50, 100, 0))).calcCrossFlat(flat3D_GL);

            var ps = new Point3d_GL[]
            {
                p1,p3,p2,
                p2,p0,p1
            };
            //addMesh(Point3d_GL.toMesh(ps), PrimitiveType.Triangles);
            var mesh = Point3d_GL.toMesh(ps);
            return add_buff_gl_dyn_col(mesh, computeNormals(mesh),new Vertex3f(1,0,0), PrimitiveType.Triangles);
        }

        float[] toFloat(Point3d_GL[] points)
        {
            var fl = new float[points.Length * 3];
            for(int i=0; i< points.Length; i++)
            {
                fl[3 * i] = (float)points[i].x;
                fl[3 * i+1] = (float)points[i].y;
                fl[3 * i+2] = (float)points[i].z;
            }
            return fl;
        }
        float[] toFloat(Vertex4f[] points)
        {
            var fl = new float[points.Length * 3];
            for (int i = 0; i < points.Length; i++)
            {
                fl[3 * i] = points[i].x;
                fl[3 * i + 1] = points[i].y;
                fl[3 * i + 2] = points[i].z;
            }
            return fl;
        }

        public void add_buff_gl_obj(float[] data_v, float[] data_t, float[] data_n, PrimitiveType tp)
        {
            buffersGl.add_obj(new openGlobj(data_v, null, data_n, data_t, tp));
        }
        public int add_buff_gl_dyn(float[] data_v, float[] data_n, PrimitiveType tp)
        {
            return buffersGl.add_obj(new openGlobj(data_v, null, data_n, null, tp));
        }
        static float[] color_buf(Vertex3f color, int len)
        {
            var color_buf = new float[len];
            for(int i=0; i<len/3;i++)
            {
                color_buf[3 * i] = color.x;
                color_buf[3 * i+1] = color.y;
                color_buf[3 * i+2] = color.z;
            }
            //Console.WriteLine("col_buf");
            //Console.WriteLine(color_buf[0] + " " + color_buf[1] + " " + color_buf[2]);
            return color_buf;
        }
        public int add_buff_gl_dyn_col(float[] data_v, float[] data_n, Vertex3f color, PrimitiveType tp)
        {

            return buffersGl.add_obj(new openGlobj(data_v, color_buf(color,data_v.Length), data_n, null, tp, 0));
        }
        public void add_buff_gl(float[] data_v, float[] data_c, float[] data_n, PrimitiveType tp)
        {            
            buffersGl.add_obj(new openGlobj(data_v, data_c, data_n,null,  tp));
        }
        public int addSTL(float[] data_v, PrimitiveType tp, Point3d_GL trans, Point3d_GL rotate, float scale = 1,int count =1)
        {
            var data_n = computeNormals(data_v);
            var glObj = new openGlobj(data_v, null, data_n, null, tp,1,count);

            glObj.trsc[0].scale = scale;
            glObj.trsc[0].transl = trans;
            glObj.trsc[0].rotate = rotate;       
            return buffersGl.add_obj(glObj);
        }

        public int addOBJ(float[] data_v, float[] data_n, float[] data_t, float scale = 1, int count = 1, Mat pic = null)
        {
            var texid = -1;
            if(pic!=null)
            {
                var textureB = textureLoad(pic);
                texid = (int)bindTexture(textureB);
            }
            var glObj = new openGlobj(data_v, null, data_n, data_t, PrimitiveType.Triangles, 1, count, texid);
            
            //glObj.trsc[0].scale = scale;
            //glObj.trsc[0].transl = new Point3d_GL(0,0,0);
           // glObj.trsc[0].rotate = new Point3d_GL(0, 0, 0);
            glObj.trsc[0] = new trsc(Matrix4x4f.Identity);
            return buffersGl.add_obj(glObj);
        }
        void add_buff_gl_id(float[] data_v, float[] data_c, float[] data_n, PrimitiveType tp,int id)
        {

            buffersGl.add_obj(new openGlobj(data_v, data_c, data_n, null,tp,id));
        }

        void remove_buff_gl_id(int id)
        {
            buffersGl.removeObj(id);
        }
        public void addFrame(Point3d_GL pos, Point3d_GL x, Point3d_GL y, Point3d_GL z)
        {
            addLineMesh(new Point3d_GL[] { pos, x }, 1.0f, 0, 0);
            addLineMesh(new Point3d_GL[] { pos, y }, 0, 1.0f, 0);
            addLineMesh(new Point3d_GL[] { pos, z }, 0, 0, 1.0f);
        }

      
        public void addGLMesh(float[] _mesh, PrimitiveType primitiveType, float x = 0, float y = 0, float z = 0, float r = 0.1f, float g = 0.1f, float b = 0.1f, float scale = 1f)
        {
            // addMesh(cube_buf, PrimitiveType.Points);
            if (x == 0 && y == 0 && z == 0)
            {
                addMesh(_mesh, primitiveType, r, g, b);
            }
            else
            {
                addMesh(translateMesh(scaleMesh(_mesh, scale), x, y, z), primitiveType, r, g, b);
            }

        }
        public float[] translateMesh(float[] _mesh, float x=0, float y=0, float z=0)
        {
            var mesh = new float[_mesh.Length];
            for (int i = 0; i < mesh.Length; i += 3)
            {
                mesh[i] = _mesh[i] + x;
                mesh[i + 1] = _mesh[i + 1] + y;
                mesh[i + 2] = _mesh[i + 2] + z;
            }
            return mesh;
        }

        static public float[] translateMesh(float[] _mesh, Matrix<double> matrix)
        {
            var mesh = new float[_mesh.Length];
            for (int i = 0; i < mesh.Length; i += 3)
            {
                var p = matrix* new Point3d_GL(_mesh[i], _mesh[i + 1], _mesh[i + 2]);

                mesh[i] = (float)p.x;
                mesh[i + 1] = (float)p.y;
                mesh[i + 2] = (float)p.z;
            }
            return mesh;
        }

        public float[] scaleMesh(float[] _mesh, float k, float kx = 1.0f, float ky = 1.0f, float kz = 1.0f)
        {
            var mesh = new float[_mesh.Length];
            for (int i = 0; i < mesh.Length; i += 3)
            {
                mesh[i] = _mesh[i] * k * kx;
                mesh[i + 1] = _mesh[i + 1] * k * ky;
                mesh[i + 2] = _mesh[i + 2] * k * kz;
            }
            return mesh;
        }
        void addPointMesh(Point3d_GL[] points, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var mesh = new List<float>();
            foreach (var p in points)
            {
                mesh.Add((float)p.x);
                mesh.Add((float)p.y);
                mesh.Add((float)p.z);
            }
            addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Points, r, g, b);
        }
        void addLineFanMesh(float[] startpoint, float[] points, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var mesh = new float[points.Length * 2];
            var j = 0;
            for(int i=0; i<points.Length-3;i+=3)
            {
                mesh[j] = startpoint[0]; j++;
                mesh[j] = startpoint[1]; j++;
                mesh[j] = startpoint[2]; j++;
                mesh[j] = points[i]; j++;
                mesh[j] = points[i+1]; j++;
                mesh[j] = points[i+2]; j++;
            }
            addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Lines, r, g, b);
        }
        public void addLineMesh(Point3d_GL[] points, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var mesh = new List<float>();
            foreach (var p in points)
            {
                mesh.Add((float)p.x);
                mesh.Add((float)p.y);
                mesh.Add((float)p.z);
            }
            addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Lines, r, g, b);
        }
        void addLineMesh(Vertex4f[] points, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var mesh = new float[points.Length * 3];
            int ind = 0;
            foreach (var p in points)
            {
                mesh[ind] = p.x; ind++;
                mesh[ind] = p.y; ind++;
                mesh[ind] = p.z; ind++;
            }
            addMeshWithoutNorm(mesh, PrimitiveType.Lines, r, g, b);
        }
        public void addMeshWithoutNorm(float[] gl_vertex_buffer_data, PrimitiveType primitiveType, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var normal_buffer_data = new float[gl_vertex_buffer_data.Length];
            var color_buffer_data = new float[gl_vertex_buffer_data.Length];
            for (int i = 0; i < color_buffer_data.Length; i += 3)
            {
                color_buffer_data[i] = r;
                color_buffer_data[i + 1] = g;
                color_buffer_data[i + 2] = b;

                normal_buffer_data[i] = 0.1f;
                normal_buffer_data[i + 1] = 0.1f;
                normal_buffer_data[i + 2] = 0.1f;
            }
            add_buff_gl(gl_vertex_buffer_data, color_buffer_data, normal_buffer_data, primitiveType);
        }
        void addMeshColor(float[] gl_vertex_buffer_data, float[] gl_color_buffer_data, PrimitiveType primitiveType, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var normal_buffer_data = new float[gl_vertex_buffer_data.Length];
            Point3d_GL p1, p2, p3, U, V, Norm1, Norm;
            for (int i = 0; i < normal_buffer_data.Length; i += 9)
            {
                p1 = new Point3d_GL(gl_vertex_buffer_data[i], gl_vertex_buffer_data[i + 1], gl_vertex_buffer_data[i + 2]);
                p2 = new Point3d_GL(gl_vertex_buffer_data[i + 3], gl_vertex_buffer_data[i + 4], gl_vertex_buffer_data[i + 5]);
                p3 = new Point3d_GL(gl_vertex_buffer_data[i + 6], gl_vertex_buffer_data[i + 7], gl_vertex_buffer_data[i + 8]);
                U = p1 - p2;
                V = p1 - p3;
                Norm = new Point3d_GL(
                    U.y * V.z - U.z * V.y,
                    U.z * V.x - U.x * V.z,
                    U.x * V.y - U.y * V.x);
                Norm1 = Norm.normalize();
                normal_buffer_data[i] = (float)Norm1.x;
                normal_buffer_data[i + 1] = (float)Norm1.y;
                normal_buffer_data[i + 2] = (float)Norm1.z;

                normal_buffer_data[i + 3] = (float)Norm1.x;
                normal_buffer_data[i + 4] = (float)Norm1.y;
                normal_buffer_data[i + 5] = (float)Norm1.z;

                normal_buffer_data[i + 6] = (float)Norm1.x;
                normal_buffer_data[i + 7] = (float)Norm1.y;
                normal_buffer_data[i + 8] = (float)Norm1.z;
            }
            // Console.WriteLine("vert len " + gl_vertex_buffer_data.Length);
            add_buff_gl(gl_vertex_buffer_data, gl_color_buffer_data, normal_buffer_data, primitiveType);
        }
        public void addMesh(float[] gl_vertex_buffer_data, PrimitiveType primitiveType, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var normal_buffer_data = computeNormals(gl_vertex_buffer_data);
            var color_buffer_data = new float[gl_vertex_buffer_data.Length];
            for (int i = 0; i < color_buffer_data.Length; i += 3)
            {
                color_buffer_data[i] = r;
                color_buffer_data[i + 1] = g;
                color_buffer_data[i + 2] = b;
            }
            add_buff_gl(gl_vertex_buffer_data, color_buffer_data, normal_buffer_data, primitiveType);
        }

        float[] computeNormals(float[] gl_vertex_buffer_data)
        {
            var normal_buffer_data = new float[gl_vertex_buffer_data.Length];
            Point3d_GL p1, p2, p3, U, V, Norm1, Norm;
            for (int i = 0; i < normal_buffer_data.Length; i += 9)
            {
                p1 = new Point3d_GL(gl_vertex_buffer_data[i], gl_vertex_buffer_data[i + 1], gl_vertex_buffer_data[i + 2]);
                p2 = new Point3d_GL(gl_vertex_buffer_data[i + 3], gl_vertex_buffer_data[i + 4], gl_vertex_buffer_data[i + 5]);
                p3 = new Point3d_GL(gl_vertex_buffer_data[i + 6], gl_vertex_buffer_data[i + 7], gl_vertex_buffer_data[i + 8]);
                U = p1 - p2;
                V = p1 - p3;
                Norm = new Point3d_GL(
                    U.y * V.z - U.z * V.y,
                    U.z * V.x - U.x * V.z,
                    U.x * V.y - U.y * V.x);
                Norm1 = Norm.normalize();
                normal_buffer_data[i] = (float)Norm1.x;
                normal_buffer_data[i + 1] = (float)Norm1.y;
                normal_buffer_data[i + 2] = (float)Norm1.z;

                normal_buffer_data[i + 3] = (float)Norm1.x;
                normal_buffer_data[i + 4] = (float)Norm1.y;
                normal_buffer_data[i + 5] = (float)Norm1.z;

                normal_buffer_data[i + 6] = (float)Norm1.x;
                normal_buffer_data[i + 7] = (float)Norm1.y;
                normal_buffer_data[i + 8] = (float)Norm1.z;
            }
            return normal_buffer_data;
        }


        #endregion


        #region shader
        string[] assembCode(string[] paths)
        {
            var text = "";
            foreach (var path in paths)
                text += File.ReadAllText(path);
            return new string[] { text };
        }
        void debugShaderComp(uint ShaderName)
        {
            int compiled;

            Gl.GetShader(ShaderName, ShaderParameterName.CompileStatus, out compiled);
            if (compiled != 0)
            {
                Console.WriteLine("SHADER COMPILE");
                return;
            }


            // Throw exception on compilation errors
            const int logMaxLength = 1024;

            StringBuilder infolog = new StringBuilder(logMaxLength);
            int infologLength;

            Gl.GetShaderInfoLog(ShaderName, logMaxLength, out infologLength, infolog);

            throw new InvalidOperationException($"unable to compile shader: {infolog}");
        }
        private uint compileShader(string[] shSource, ShaderType shaderType)
        {
            uint ShaderID = Gl.CreateShader(shaderType);
            Gl.ShaderSource(ShaderID, shSource);
            Gl.CompileShader(ShaderID);
            debugShaderComp(ShaderID);
            return ShaderID;
        }
        private uint createShader(string[] VertexSourceGL, string[] GeometryShaderGL, string[] FragmentSourceGL)
        {
            bool geom = false;
            uint GeometryShaderID = 0;
            if (GeometryShaderGL!=null)
            {
                geom = true;
            }
            var VertexShaderID = compileShader(VertexSourceGL, ShaderType.VertexShader);
            var FragmentShaderID = compileShader(FragmentSourceGL, ShaderType.FragmentShader);
            if (geom)
            {
                GeometryShaderID = compileShader(GeometryShaderGL, ShaderType.GeometryShader);
            }

            uint ProgrammID = Gl.CreateProgram();
            Gl.AttachShader(ProgrammID, VertexShaderID);
            Gl.AttachShader(ProgrammID, FragmentShaderID);
            if(geom)
            {
                Gl.AttachShader(ProgrammID, GeometryShaderID);
            }
            Gl.LinkProgram(ProgrammID);

            int linked;

            Gl.GetProgram(ProgrammID, ProgramProperty.LinkStatus, out linked);

            if (linked == 0)
            {
                const int logMaxLength = 1024;

                StringBuilder infolog = new StringBuilder(logMaxLength);
                int infologLength;

                Gl.GetProgramInfoLog(ProgrammID, 1024, out infologLength, infolog);

                throw new InvalidOperationException($"unable to link program: {infolog}");
            }

            Gl.DeleteShader(VertexShaderID);
            Gl.DeleteShader(FragmentShaderID);
            if(geom)
            {
                Gl.DeleteShader(GeometryShaderID);
            }
            return ProgrammID;
        }
        private uint createShaderCompute(string[] ComputeSourceGL)
        {

            var ComputeShaderID = compileShader(ComputeSourceGL, ShaderType.ComputeShader);

            uint ProgrammID = Gl.CreateProgram();
            Gl.AttachShader(ProgrammID, ComputeShaderID);
            Gl.LinkProgram(ProgrammID);

            int linked;

            Gl.GetProgram(ProgrammID, ProgramProperty.LinkStatus, out linked);

            if (linked == 0)
            {
                const int logMaxLength = 1024;

                StringBuilder infolog = new StringBuilder(logMaxLength);
                int infologLength;

                Gl.GetProgramInfoLog(ProgrammID, 1024, out infologLength, infolog);

                throw new InvalidOperationException($"unable to link program: {infolog}");
            }

            Gl.DeleteShader(ComputeShaderID);
            return ProgrammID;
        }

        #endregion
    }
}
