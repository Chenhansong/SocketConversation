namespace WebAListener
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.send = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.filebutton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(912, 653);
            this.panel1.TabIndex = 0;
            this.panel1.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.panel1_ControlAdded);
            // 
            // send
            // 
            this.send.Location = new System.Drawing.Point(690, 704);
            this.send.Name = "send";
            this.send.Size = new System.Drawing.Size(75, 23);
            this.send.TabIndex = 1;
            this.send.Text = "send";
            this.send.UseVisualStyleBackColor = true;
            this.send.Click += new System.EventHandler(this.send_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(324, 687);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(324, 40);
            this.textBox1.TabIndex = 2;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // filebutton
            // 
            this.filebutton.Location = new System.Drawing.Point(817, 704);
            this.filebutton.Name = "filebutton";
            this.filebutton.Size = new System.Drawing.Size(75, 23);
            this.filebutton.TabIndex = 3;
            this.filebutton.Text = "File";
            this.filebutton.UseVisualStyleBackColor = true;
            this.filebutton.Click += new System.EventHandler(this.filebutton_Click);
            this.filebutton.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.filebutton_ControlAdded);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(936, 739);
            this.Controls.Add(this.filebutton);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.send);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button send;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button filebutton;
    }
}

