

namespace FaceAnalyse
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
       // private System.ComponentModel.IContainer components = null;
        System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.glControl1 = new OpenGL.GlControl();
            this.richTextBox_debug = new System.Windows.Forms.RichTextBox();
            this.trackBar_X_L = new System.Windows.Forms.TrackBar();
            this.trackBar_Y_L = new System.Windows.Forms.TrackBar();
            this.trackBar_Z_L = new System.Windows.Forms.TrackBar();
            this.but_textureVision = new System.Windows.Forms.Button();
            this.imageBox1 = new Emgu.CV.UI.ImageBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_X_L)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_Y_L)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_Z_L)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // glControl1
            // 
            this.glControl1.Animation = true;
            this.glControl1.AutoSize = true;
            this.glControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.glControl1.ColorBits = ((uint)(24u));
            this.glControl1.DepthBits = ((uint)(24u));
            this.glControl1.Location = new System.Drawing.Point(12, 12);
            this.glControl1.MultisampleBits = ((uint)(8u));
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(1000, 1000);
            this.glControl1.StencilBits = ((uint)(0u));
            this.glControl1.TabIndex = 0;
            this.glControl1.ContextCreated += new System.EventHandler<OpenGL.GlControlEventArgs>(this.glControl1_ContextCreated);
            this.glControl1.ContextDestroying += new System.EventHandler<OpenGL.GlControlEventArgs>(this.glControl1_ContextDestroying);
            this.glControl1.Render += new System.EventHandler<OpenGL.GlControlEventArgs>(this.glControl1_Render);
            this.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseDown);
            this.glControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseMove);
            // 
            // richTextBox_debug
            // 
            this.richTextBox_debug.Location = new System.Drawing.Point(1578, 12);
            this.richTextBox_debug.Name = "richTextBox_debug";
            this.richTextBox_debug.Size = new System.Drawing.Size(314, 201);
            this.richTextBox_debug.TabIndex = 1;
            this.richTextBox_debug.Text = "";
            // 
            // trackBar_X_L
            // 
            this.trackBar_X_L.Location = new System.Drawing.Point(1347, 10);
            this.trackBar_X_L.Maximum = 100;
            this.trackBar_X_L.Minimum = -100;
            this.trackBar_X_L.Name = "trackBar_X_L";
            this.trackBar_X_L.Size = new System.Drawing.Size(225, 45);
            this.trackBar_X_L.TabIndex = 2;
            this.trackBar_X_L.Scroll += new System.EventHandler(this.trackBar_X_L_Scroll);
            // 
            // trackBar_Y_L
            // 
            this.trackBar_Y_L.Location = new System.Drawing.Point(1347, 61);
            this.trackBar_Y_L.Maximum = 100;
            this.trackBar_Y_L.Minimum = -100;
            this.trackBar_Y_L.Name = "trackBar_Y_L";
            this.trackBar_Y_L.Size = new System.Drawing.Size(225, 45);
            this.trackBar_Y_L.TabIndex = 3;
            this.trackBar_Y_L.Scroll += new System.EventHandler(this.trackBar_Y_L_Scroll);
            // 
            // trackBar_Z_L
            // 
            this.trackBar_Z_L.Location = new System.Drawing.Point(1347, 112);
            this.trackBar_Z_L.Maximum = 100;
            this.trackBar_Z_L.Minimum = -100;
            this.trackBar_Z_L.Name = "trackBar_Z_L";
            this.trackBar_Z_L.Size = new System.Drawing.Size(225, 45);
            this.trackBar_Z_L.TabIndex = 4;
            this.trackBar_Z_L.Value = 40;
            this.trackBar_Z_L.Scroll += new System.EventHandler(this.trackBar_Z_L_Scroll);
            // 
            // but_textureVision
            // 
            this.but_textureVision.Location = new System.Drawing.Point(1018, 12);
            this.but_textureVision.Name = "but_textureVision";
            this.but_textureVision.Size = new System.Drawing.Size(115, 43);
            this.but_textureVision.TabIndex = 6;
            this.but_textureVision.Text = "Убрать текстуру";
            this.but_textureVision.UseVisualStyleBackColor = true;
            this.but_textureVision.Click += new System.EventHandler(this.but_textureVision_Click);
            // 
            // imageBox1
            // 
            this.imageBox1.Location = new System.Drawing.Point(1018, 219);
            this.imageBox1.Name = "imageBox1";
            this.imageBox1.Size = new System.Drawing.Size(800, 800);
            this.imageBox1.TabIndex = 2;
            this.imageBox1.TabStop = false;
            this.imageBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.imageBox1_Move);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(1019, 61);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(322, 152);
            this.richTextBox1.TabIndex = 7;
            this.richTextBox1.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.imageBox1);
            this.Controls.Add(this.but_textureVision);
            this.Controls.Add(this.trackBar_Z_L);
            this.Controls.Add(this.trackBar_Y_L);
            this.Controls.Add(this.trackBar_X_L);
            this.Controls.Add(this.richTextBox_debug);
            this.Controls.Add(this.glControl1);
            this.Name = "MainForm";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_X_L)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_Y_L)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_Z_L)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OpenGL.GlControl glControl1;
        private System.Windows.Forms.RichTextBox richTextBox_debug;
        private System.Windows.Forms.TrackBar trackBar_X_L;
        private System.Windows.Forms.TrackBar trackBar_Y_L;
        private System.Windows.Forms.TrackBar trackBar_Z_L;
        private System.Windows.Forms.Button but_textureVision;
        private Emgu.CV.UI.ImageBox imageBox1;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}

