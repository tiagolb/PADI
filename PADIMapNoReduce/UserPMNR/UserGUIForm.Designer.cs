namespace UserPMNR
{
    partial class UserGUIForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tb_entryURL = new System.Windows.Forms.TextBox();
            this.bt_entryURL = new System.Windows.Forms.Button();
            this.ofd_inputFile = new System.Windows.Forms.OpenFileDialog();
            this.bt_inputFile = new System.Windows.Forms.Button();
            this.bt_outputFolder = new System.Windows.Forms.Button();
            this.tb_safeInputFile = new System.Windows.Forms.TextBox();
            this.tb_absOutputFolder = new System.Windows.Forms.TextBox();
            this.tb_absInputFile = new System.Windows.Forms.TextBox();
            this.tb_splits = new System.Windows.Forms.TextBox();
            this.lb_splits = new System.Windows.Forms.Label();
            this.lb_mapClass = new System.Windows.Forms.Label();
            this.tb_mapClass = new System.Windows.Forms.TextBox();
            this.tb_absDllFile = new System.Windows.Forms.TextBox();
            this.tb_safeDllFile = new System.Windows.Forms.TextBox();
            this.bt_dllFile = new System.Windows.Forms.Button();
            this.lb_entryURL = new System.Windows.Forms.Label();
            this.fbd_outputFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.ofd_inputDll = new System.Windows.Forms.OpenFileDialog();
            this.bt_submitJob = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tb_entryURL
            // 
            this.tb_entryURL.Location = new System.Drawing.Point(128, 12);
            this.tb_entryURL.Name = "tb_entryURL";
            this.tb_entryURL.Size = new System.Drawing.Size(526, 20);
            this.tb_entryURL.TabIndex = 0;
            // 
            // bt_entryURL
            // 
            this.bt_entryURL.Location = new System.Drawing.Point(660, 12);
            this.bt_entryURL.Name = "bt_entryURL";
            this.bt_entryURL.Size = new System.Drawing.Size(112, 23);
            this.bt_entryURL.TabIndex = 1;
            this.bt_entryURL.Text = "Submit Entry URL";
            this.bt_entryURL.UseVisualStyleBackColor = true;
            this.bt_entryURL.Click += new System.EventHandler(this.bt_entryURL_Click);
            // 
            // ofd_inputFile
            // 
            this.ofd_inputFile.FileName = "ofd_inputFile";
            // 
            // bt_inputFile
            // 
            this.bt_inputFile.Location = new System.Drawing.Point(12, 302);
            this.bt_inputFile.Name = "bt_inputFile";
            this.bt_inputFile.Size = new System.Drawing.Size(106, 23);
            this.bt_inputFile.TabIndex = 2;
            this.bt_inputFile.Text = "Input File";
            this.bt_inputFile.UseVisualStyleBackColor = true;
            this.bt_inputFile.Click += new System.EventHandler(this.bt_inputFile_Click);
            // 
            // bt_outputFolder
            // 
            this.bt_outputFolder.Location = new System.Drawing.Point(12, 360);
            this.bt_outputFolder.Name = "bt_outputFolder";
            this.bt_outputFolder.Size = new System.Drawing.Size(106, 23);
            this.bt_outputFolder.TabIndex = 3;
            this.bt_outputFolder.Text = "Output Folder";
            this.bt_outputFolder.UseVisualStyleBackColor = true;
            this.bt_outputFolder.Click += new System.EventHandler(this.bt_outputFolder_Click);
            // 
            // tb_safeInputFile
            // 
            this.tb_safeInputFile.Location = new System.Drawing.Point(128, 304);
            this.tb_safeInputFile.Name = "tb_safeInputFile";
            this.tb_safeInputFile.ReadOnly = true;
            this.tb_safeInputFile.Size = new System.Drawing.Size(644, 20);
            this.tb_safeInputFile.TabIndex = 4;
            // 
            // tb_absOutputFolder
            // 
            this.tb_absOutputFolder.Location = new System.Drawing.Point(128, 362);
            this.tb_absOutputFolder.Name = "tb_absOutputFolder";
            this.tb_absOutputFolder.ReadOnly = true;
            this.tb_absOutputFolder.Size = new System.Drawing.Size(644, 20);
            this.tb_absOutputFolder.TabIndex = 5;
            // 
            // tb_absInputFile
            // 
            this.tb_absInputFile.Location = new System.Drawing.Point(12, 331);
            this.tb_absInputFile.Name = "tb_absInputFile";
            this.tb_absInputFile.ReadOnly = true;
            this.tb_absInputFile.Size = new System.Drawing.Size(760, 20);
            this.tb_absInputFile.TabIndex = 6;
            // 
            // tb_splits
            // 
            this.tb_splits.Location = new System.Drawing.Point(128, 396);
            this.tb_splits.Name = "tb_splits";
            this.tb_splits.Size = new System.Drawing.Size(644, 20);
            this.tb_splits.TabIndex = 8;
            // 
            // lb_splits
            // 
            this.lb_splits.AutoSize = true;
            this.lb_splits.Location = new System.Drawing.Point(41, 399);
            this.lb_splits.Name = "lb_splits";
            this.lb_splits.Size = new System.Drawing.Size(43, 13);
            this.lb_splits.TabIndex = 9;
            this.lb_splits.Text = "N Splits";
            // 
            // lb_mapClass
            // 
            this.lb_mapClass.AutoSize = true;
            this.lb_mapClass.Location = new System.Drawing.Point(41, 427);
            this.lb_mapClass.Name = "lb_mapClass";
            this.lb_mapClass.Size = new System.Drawing.Size(56, 13);
            this.lb_mapClass.TabIndex = 10;
            this.lb_mapClass.Text = "Map Class";
            // 
            // tb_mapClass
            // 
            this.tb_mapClass.Location = new System.Drawing.Point(128, 424);
            this.tb_mapClass.Name = "tb_mapClass";
            this.tb_mapClass.Size = new System.Drawing.Size(644, 20);
            this.tb_mapClass.TabIndex = 11;
            // 
            // tb_absDllFile
            // 
            this.tb_absDllFile.Location = new System.Drawing.Point(12, 482);
            this.tb_absDllFile.Name = "tb_absDllFile";
            this.tb_absDllFile.ReadOnly = true;
            this.tb_absDllFile.Size = new System.Drawing.Size(760, 20);
            this.tb_absDllFile.TabIndex = 14;
            // 
            // tb_safeDllFile
            // 
            this.tb_safeDllFile.Location = new System.Drawing.Point(128, 455);
            this.tb_safeDllFile.Name = "tb_safeDllFile";
            this.tb_safeDllFile.ReadOnly = true;
            this.tb_safeDllFile.Size = new System.Drawing.Size(644, 20);
            this.tb_safeDllFile.TabIndex = 13;
            // 
            // bt_dllFile
            // 
            this.bt_dllFile.Location = new System.Drawing.Point(12, 453);
            this.bt_dllFile.Name = "bt_dllFile";
            this.bt_dllFile.Size = new System.Drawing.Size(106, 23);
            this.bt_dllFile.TabIndex = 12;
            this.bt_dllFile.Text = ".dll File";
            this.bt_dllFile.UseVisualStyleBackColor = true;
            this.bt_dllFile.Click += new System.EventHandler(this.bt_dllFile_Click);
            // 
            // lb_entryURL
            // 
            this.lb_entryURL.AutoSize = true;
            this.lb_entryURL.Location = new System.Drawing.Point(24, 15);
            this.lb_entryURL.Name = "lb_entryURL";
            this.lb_entryURL.Size = new System.Drawing.Size(85, 13);
            this.lb_entryURL.TabIndex = 15;
            this.lb_entryURL.Text = "Client Entry URL";
            // 
            // ofd_inputDll
            // 
            this.ofd_inputDll.FileName = "ofd_inputDll";
            // 
            // bt_submitJob
            // 
            this.bt_submitJob.Location = new System.Drawing.Point(309, 517);
            this.bt_submitJob.Name = "bt_submitJob";
            this.bt_submitJob.Size = new System.Drawing.Size(150, 23);
            this.bt_submitJob.TabIndex = 16;
            this.bt_submitJob.Text = "Submit Job";
            this.bt_submitJob.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = global::UserPMNR.Properties.Resources.Panorama_NYC_X3000;
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(12, 41);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(760, 255);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 17;
            this.pictureBox1.TabStop = false;
            // 
            // UserGUIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.bt_submitJob);
            this.Controls.Add(this.lb_entryURL);
            this.Controls.Add(this.tb_absDllFile);
            this.Controls.Add(this.tb_safeDllFile);
            this.Controls.Add(this.bt_dllFile);
            this.Controls.Add(this.tb_mapClass);
            this.Controls.Add(this.lb_mapClass);
            this.Controls.Add(this.lb_splits);
            this.Controls.Add(this.tb_splits);
            this.Controls.Add(this.tb_absInputFile);
            this.Controls.Add(this.tb_absOutputFolder);
            this.Controls.Add(this.tb_safeInputFile);
            this.Controls.Add(this.bt_outputFolder);
            this.Controls.Add(this.bt_inputFile);
            this.Controls.Add(this.bt_entryURL);
            this.Controls.Add(this.tb_entryURL);
            this.Name = "UserGUIForm";
            this.Text = "User Application";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tb_entryURL;
        private System.Windows.Forms.Button bt_entryURL;
        private System.Windows.Forms.OpenFileDialog ofd_inputFile;
        private System.Windows.Forms.Button bt_inputFile;
        private System.Windows.Forms.Button bt_outputFolder;
        private System.Windows.Forms.TextBox tb_safeInputFile;
        private System.Windows.Forms.TextBox tb_absOutputFolder;
        private System.Windows.Forms.TextBox tb_absInputFile;
        private System.Windows.Forms.TextBox tb_splits;
        private System.Windows.Forms.Label lb_splits;
        private System.Windows.Forms.Label lb_mapClass;
        private System.Windows.Forms.TextBox tb_mapClass;
        private System.Windows.Forms.TextBox tb_absDllFile;
        private System.Windows.Forms.TextBox tb_safeDllFile;
        private System.Windows.Forms.Button bt_dllFile;
        private System.Windows.Forms.Label lb_entryURL;
        private System.Windows.Forms.FolderBrowserDialog fbd_outputFolder;
        private System.Windows.Forms.OpenFileDialog ofd_inputDll;
        private System.Windows.Forms.Button bt_submitJob;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

