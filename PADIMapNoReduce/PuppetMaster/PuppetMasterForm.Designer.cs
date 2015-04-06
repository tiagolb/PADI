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
            this.bt_submitScript = new System.Windows.Forms.Button();
            this.tb_singleCommand = new System.Windows.Forms.TextBox();
            this.lb_singleCommand = new System.Windows.Forms.Label();
            this.bt_singleCommand = new System.Windows.Forms.Button();
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
            // tb_singleCommand
            // 
            this.tb_singleCommand.Location = new System.Drawing.Point(116, 187);
            this.tb_singleCommand.Name = "tb_singleCommand";
            this.tb_singleCommand.Size = new System.Drawing.Size(535, 20);
            this.tb_singleCommand.TabIndex = 9;
            // 
            // lb_singleCommand
            // 
            this.lb_singleCommand.AutoSize = true;
            this.lb_singleCommand.Location = new System.Drawing.Point(24, 190);
            this.lb_singleCommand.Name = "lb_singleCommand";
            this.lb_singleCommand.Size = new System.Drawing.Size(86, 13);
            this.lb_singleCommand.TabIndex = 10;
            this.lb_singleCommand.Text = "Single Command";
            this.lb_singleCommand.Click += new System.EventHandler(this.label1_Click);
            // 
            // bt_singleCommand
            // 
            this.bt_singleCommand.Location = new System.Drawing.Point(657, 184);
            this.bt_singleCommand.Name = "bt_singleCommand";
            this.bt_singleCommand.Size = new System.Drawing.Size(115, 23);
            this.bt_singleCommand.TabIndex = 11;
            this.bt_singleCommand.Text = "Submit Command";
            this.bt_singleCommand.UseVisualStyleBackColor = true;
            // 
            // PuppetMasterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.bt_singleCommand);
            this.Controls.Add(this.lb_singleCommand);
            this.Controls.Add(this.tb_singleCommand);
            this.Controls.Add(this.bt_submitScript);
            this.Controls.Add(this.tb_scriptFileAddress);
            this.Controls.Add(this.tb_scriptFileName);
            this.Controls.Add(this.bt_script);
            this.Controls.Add(this.bt_PuppetMasterURLSubmit);
            this.Controls.Add(this.tb_PuppetMasterURL);
            this.Controls.Add(this.lb_PuppetMasterURL);
            this.Name = "PuppetMasterForm";
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
        private System.Windows.Forms.Button bt_submitScript;
        private System.Windows.Forms.TextBox tb_singleCommand;
        private System.Windows.Forms.Label lb_singleCommand;
        private System.Windows.Forms.Button bt_singleCommand;
    }
}

