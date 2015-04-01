namespace PuppetMasterPMNR {
    partial class PuppetMasterForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.lb_PuppetMasterURL = new System.Windows.Forms.Label();
            this.tb_PuppetMasterURL = new System.Windows.Forms.TextBox();
            this.bt_PuppetMasterURLSubmit = new System.Windows.Forms.Button();
            this.bt_script = new System.Windows.Forms.Button();
            this.ofd_script = new System.Windows.Forms.OpenFileDialog();
            this.tb_scriptFileName = new System.Windows.Forms.TextBox();
            this.tb_scriptFileAddress = new System.Windows.Forms.TextBox();
            this.tb_console = new System.Windows.Forms.TextBox();
            this.bt_submitScript = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lb_PuppetMasterURL
            // 
            this.lb_PuppetMasterURL.AutoSize = true;
            this.lb_PuppetMasterURL.Location = new System.Drawing.Point(13, 13);
            this.lb_PuppetMasterURL.Name = "lb_PuppetMasterURL";
            this.lb_PuppetMasterURL.Size = new System.Drawing.Size(143, 13);
            this.lb_PuppetMasterURL.TabIndex = 0;
            this.lb_PuppetMasterURL.Text = "Puppet Master Service URL:";
            // 
            // tb_PuppetMasterURL
            // 
            this.tb_PuppetMasterURL.Location = new System.Drawing.Point(162, 6);
            this.tb_PuppetMasterURL.Name = "tb_PuppetMasterURL";
            this.tb_PuppetMasterURL.Size = new System.Drawing.Size(489, 20);
            this.tb_PuppetMasterURL.TabIndex = 1;
            // 
            // bt_PuppetMasterURLSubmit
            // 
            this.bt_PuppetMasterURLSubmit.Location = new System.Drawing.Point(657, 3);
            this.bt_PuppetMasterURLSubmit.Name = "bt_PuppetMasterURLSubmit";
            this.bt_PuppetMasterURLSubmit.Size = new System.Drawing.Size(115, 23);
            this.bt_PuppetMasterURLSubmit.TabIndex = 2;
            this.bt_PuppetMasterURLSubmit.Text = "Launch Service";
            this.bt_PuppetMasterURLSubmit.UseVisualStyleBackColor = true;
            this.bt_PuppetMasterURLSubmit.Click += new System.EventHandler(this.bt_PuppetMasterURLSubmit_Click);
            // 
            // bt_script
            // 
            this.bt_script.Location = new System.Drawing.Point(16, 68);
            this.bt_script.Name = "bt_script";
            this.bt_script.Size = new System.Drawing.Size(75, 23);
            this.bt_script.TabIndex = 4;
            this.bt_script.Text = "Open Script";
            this.bt_script.UseVisualStyleBackColor = true;
            this.bt_script.Click += new System.EventHandler(this.bt_script_Click);
            // 
            // ofd_script
            // 
            this.ofd_script.FileName = "ofd_scriptFile";
            // 
            // tb_scriptFileName
            // 
            this.tb_scriptFileName.Location = new System.Drawing.Point(98, 70);
            this.tb_scriptFileName.Name = "tb_scriptFileName";
            this.tb_scriptFileName.Size = new System.Drawing.Size(553, 20);
            this.tb_scriptFileName.TabIndex = 5;
            // 
            // tb_scriptFileAddress
            // 
            this.tb_scriptFileAddress.Location = new System.Drawing.Point(16, 98);
            this.tb_scriptFileAddress.Name = "tb_scriptFileAddress";
            this.tb_scriptFileAddress.Size = new System.Drawing.Size(635, 20);
            this.tb_scriptFileAddress.TabIndex = 6;
            // 
            // tb_console
            // 
            this.tb_console.BackColor = System.Drawing.SystemColors.MenuText;
            this.tb_console.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_console.ForeColor = System.Drawing.Color.Lime;
            this.tb_console.Location = new System.Drawing.Point(16, 142);
            this.tb_console.Multiline = true;
            this.tb_console.Name = "tb_console";
            this.tb_console.Size = new System.Drawing.Size(756, 408);
            this.tb_console.TabIndex = 7;
            this.tb_console.Text = "> ";
            // 
            // bt_submitScript
            // 
            this.bt_submitScript.Location = new System.Drawing.Point(657, 70);
            this.bt_submitScript.Name = "bt_submitScript";
            this.bt_submitScript.Size = new System.Drawing.Size(115, 48);
            this.bt_submitScript.TabIndex = 8;
            this.bt_submitScript.Text = "Submit Script";
            this.bt_submitScript.UseVisualStyleBackColor = true;
            this.bt_submitScript.Click += new System.EventHandler(this.bt_submitScript_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.bt_submitScript);
            this.Controls.Add(this.tb_console);
            this.Controls.Add(this.tb_scriptFileAddress);
            this.Controls.Add(this.tb_scriptFileName);
            this.Controls.Add(this.bt_script);
            this.Controls.Add(this.bt_PuppetMasterURLSubmit);
            this.Controls.Add(this.tb_PuppetMasterURL);
            this.Controls.Add(this.lb_PuppetMasterURL);
            this.Name = "Form1";
            this.Text = "PuppetMaster";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lb_PuppetMasterURL;
        private System.Windows.Forms.TextBox tb_PuppetMasterURL;
        private System.Windows.Forms.Button bt_PuppetMasterURLSubmit;
        private System.Windows.Forms.Button bt_script;
        private System.Windows.Forms.OpenFileDialog ofd_script;
        private System.Windows.Forms.TextBox tb_scriptFileName;
        private System.Windows.Forms.TextBox tb_scriptFileAddress;
        private System.Windows.Forms.TextBox tb_console;
        private System.Windows.Forms.Button bt_submitScript;
    }
}

