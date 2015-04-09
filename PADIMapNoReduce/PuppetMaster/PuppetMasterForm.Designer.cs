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
            this.bt_script = new System.Windows.Forms.Button();
            this.ofd_script = new System.Windows.Forms.OpenFileDialog();
            this.tb_scriptFileName = new System.Windows.Forms.TextBox();
            this.tb_scriptFileAddress = new System.Windows.Forms.TextBox();
            this.bt_submitScript = new System.Windows.Forms.Button();
            this.tb_singleCommand = new System.Windows.Forms.TextBox();
            this.lb_singleCommand = new System.Windows.Forms.Label();
            this.bt_singleCommand = new System.Windows.Forms.Button();
            this.ofd_config = new System.Windows.Forms.OpenFileDialog();
            this.bt_submitConfig = new System.Windows.Forms.Button();
            this.tb_configFileAddress = new System.Windows.Forms.TextBox();
            this.tb_configFileName = new System.Windows.Forms.TextBox();
            this.bt_openConfig = new System.Windows.Forms.Button();
            this.tb_puppetHost = new System.Windows.Forms.TextBox();
            this.lb_puppetHost = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lb_PuppetMasterURL
            // 
            this.lb_PuppetMasterURL.AutoSize = true;
            this.lb_PuppetMasterURL.Location = new System.Drawing.Point(13, 64);
            this.lb_PuppetMasterURL.Name = "lb_PuppetMasterURL";
            this.lb_PuppetMasterURL.Size = new System.Drawing.Size(140, 13);
            this.lb_PuppetMasterURL.TabIndex = 0;
            this.lb_PuppetMasterURL.Text = "Puppet Master Service URI:";
            // 
            // tb_PuppetMasterURL
            // 
            this.tb_PuppetMasterURL.Enabled = false;
            this.tb_PuppetMasterURL.Location = new System.Drawing.Point(162, 57);
            this.tb_PuppetMasterURL.Name = "tb_PuppetMasterURL";
            this.tb_PuppetMasterURL.Size = new System.Drawing.Size(489, 20);
            this.tb_PuppetMasterURL.TabIndex = 1;
            // 
            // bt_script
            // 
            this.bt_script.Enabled = false;
            this.bt_script.Location = new System.Drawing.Point(16, 221);
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
            this.tb_scriptFileName.Enabled = false;
            this.tb_scriptFileName.Location = new System.Drawing.Point(98, 223);
            this.tb_scriptFileName.Name = "tb_scriptFileName";
            this.tb_scriptFileName.Size = new System.Drawing.Size(553, 20);
            this.tb_scriptFileName.TabIndex = 5;
            // 
            // tb_scriptFileAddress
            // 
            this.tb_scriptFileAddress.Enabled = false;
            this.tb_scriptFileAddress.Location = new System.Drawing.Point(16, 251);
            this.tb_scriptFileAddress.Name = "tb_scriptFileAddress";
            this.tb_scriptFileAddress.Size = new System.Drawing.Size(635, 20);
            this.tb_scriptFileAddress.TabIndex = 6;
            // 
            // bt_submitScript
            // 
            this.bt_submitScript.Enabled = false;
            this.bt_submitScript.Location = new System.Drawing.Point(657, 223);
            this.bt_submitScript.Name = "bt_submitScript";
            this.bt_submitScript.Size = new System.Drawing.Size(115, 48);
            this.bt_submitScript.TabIndex = 8;
            this.bt_submitScript.Text = "Submit Script";
            this.bt_submitScript.UseVisualStyleBackColor = true;
            this.bt_submitScript.Click += new System.EventHandler(this.bt_submitScript_Click);
            // 
            // tb_singleCommand
            // 
            this.tb_singleCommand.Enabled = false;
            this.tb_singleCommand.Location = new System.Drawing.Point(98, 295);
            this.tb_singleCommand.Name = "tb_singleCommand";
            this.tb_singleCommand.Size = new System.Drawing.Size(553, 20);
            this.tb_singleCommand.TabIndex = 9;
            // 
            // lb_singleCommand
            // 
            this.lb_singleCommand.AutoSize = true;
            this.lb_singleCommand.Location = new System.Drawing.Point(6, 298);
            this.lb_singleCommand.Name = "lb_singleCommand";
            this.lb_singleCommand.Size = new System.Drawing.Size(86, 13);
            this.lb_singleCommand.TabIndex = 10;
            this.lb_singleCommand.Text = "Single Command";
            // 
            // bt_singleCommand
            // 
            this.bt_singleCommand.Enabled = false;
            this.bt_singleCommand.Location = new System.Drawing.Point(657, 292);
            this.bt_singleCommand.Name = "bt_singleCommand";
            this.bt_singleCommand.Size = new System.Drawing.Size(115, 23);
            this.bt_singleCommand.TabIndex = 11;
            this.bt_singleCommand.Text = "Submit Command";
            this.bt_singleCommand.UseVisualStyleBackColor = true;
            this.bt_singleCommand.Click += new System.EventHandler(this.bt_singleCommand_Click);
            // 
            // ofd_config
            // 
            this.ofd_config.FileName = "ofd_config";
            // 
            // bt_submitConfig
            // 
            this.bt_submitConfig.Location = new System.Drawing.Point(657, 121);
            this.bt_submitConfig.Name = "bt_submitConfig";
            this.bt_submitConfig.Size = new System.Drawing.Size(115, 48);
            this.bt_submitConfig.TabIndex = 16;
            this.bt_submitConfig.Text = "Submit Config";
            this.bt_submitConfig.UseVisualStyleBackColor = true;
            this.bt_submitConfig.Click += new System.EventHandler(this.bt_submitConfig_Click);
            // 
            // tb_configFileAddress
            // 
            this.tb_configFileAddress.Location = new System.Drawing.Point(16, 149);
            this.tb_configFileAddress.Name = "tb_configFileAddress";
            this.tb_configFileAddress.Size = new System.Drawing.Size(635, 20);
            this.tb_configFileAddress.TabIndex = 15;
            // 
            // tb_configFileName
            // 
            this.tb_configFileName.Location = new System.Drawing.Point(98, 121);
            this.tb_configFileName.Name = "tb_configFileName";
            this.tb_configFileName.Size = new System.Drawing.Size(553, 20);
            this.tb_configFileName.TabIndex = 14;
            // 
            // bt_openConfig
            // 
            this.bt_openConfig.Location = new System.Drawing.Point(16, 119);
            this.bt_openConfig.Name = "bt_openConfig";
            this.bt_openConfig.Size = new System.Drawing.Size(75, 23);
            this.bt_openConfig.TabIndex = 13;
            this.bt_openConfig.Text = "Open Config";
            this.bt_openConfig.UseVisualStyleBackColor = true;
            this.bt_openConfig.Click += new System.EventHandler(this.bt_openConfig_Click);
            // 
            // tb_puppetHost
            // 
            this.tb_puppetHost.Enabled = false;
            this.tb_puppetHost.Location = new System.Drawing.Point(162, 12);
            this.tb_puppetHost.Name = "tb_puppetHost";
            this.tb_puppetHost.Size = new System.Drawing.Size(489, 20);
            this.tb_puppetHost.TabIndex = 18;
            // 
            // lb_puppetHost
            // 
            this.lb_puppetHost.AutoSize = true;
            this.lb_puppetHost.Location = new System.Drawing.Point(13, 19);
            this.lb_puppetHost.Name = "lb_puppetHost";
            this.lb_puppetHost.Size = new System.Drawing.Size(148, 13);
            this.lb_puppetHost.TabIndex = 17;
            this.lb_puppetHost.Text = "Puppet Master Host Machine:";
            // 
            // PuppetMasterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.tb_puppetHost);
            this.Controls.Add(this.lb_puppetHost);
            this.Controls.Add(this.bt_submitConfig);
            this.Controls.Add(this.tb_configFileAddress);
            this.Controls.Add(this.tb_configFileName);
            this.Controls.Add(this.bt_openConfig);
            this.Controls.Add(this.bt_singleCommand);
            this.Controls.Add(this.lb_singleCommand);
            this.Controls.Add(this.tb_singleCommand);
            this.Controls.Add(this.bt_submitScript);
            this.Controls.Add(this.tb_scriptFileAddress);
            this.Controls.Add(this.tb_scriptFileName);
            this.Controls.Add(this.bt_script);
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
        private System.Windows.Forms.Button bt_script;
        private System.Windows.Forms.OpenFileDialog ofd_script;
        private System.Windows.Forms.TextBox tb_scriptFileName;
        private System.Windows.Forms.TextBox tb_scriptFileAddress;
        private System.Windows.Forms.Button bt_submitScript;
        private System.Windows.Forms.TextBox tb_singleCommand;
        private System.Windows.Forms.Label lb_singleCommand;
        private System.Windows.Forms.Button bt_singleCommand;
        private System.Windows.Forms.OpenFileDialog ofd_config;
        private System.Windows.Forms.Button bt_submitConfig;
        private System.Windows.Forms.TextBox tb_configFileAddress;
        private System.Windows.Forms.TextBox tb_configFileName;
        private System.Windows.Forms.Button bt_openConfig;
        private System.Windows.Forms.TextBox tb_puppetHost;
        private System.Windows.Forms.Label lb_puppetHost;
    }
}

