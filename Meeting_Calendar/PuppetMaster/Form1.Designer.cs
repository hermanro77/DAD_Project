namespace PuppetMaster
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.clientsListView = new System.Windows.Forms.ListView();
            this.label4 = new System.Windows.Forms.Label();
            this.serversListView = new System.Windows.Forms.ListView();
            this.runScriptButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Script:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 25);
            this.label2.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(32, 143);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 25);
            this.label3.TabIndex = 4;
            this.label3.Text = "Clients: ";
            // 
            // clientsListView
            // 
            this.clientsListView.Location = new System.Drawing.Point(37, 171);
            this.clientsListView.Name = "clientsListView";
            this.clientsListView.Size = new System.Drawing.Size(197, 267);
            this.clientsListView.TabIndex = 5;
            this.clientsListView.UseCompatibleStateImageBehavior = false;
            this.clientsListView.SelectedIndexChanged += new System.EventHandler(this.clientsListView_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(300, 143);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 25);
            this.label4.TabIndex = 6;
            this.label4.Text = "Servers:";
            // 
            // serversListView
            // 
            this.serversListView.Location = new System.Drawing.Point(305, 171);
            this.serversListView.Name = "serversListView";
            this.serversListView.Size = new System.Drawing.Size(191, 267);
            this.serversListView.TabIndex = 7;
            this.serversListView.UseCompatibleStateImageBehavior = false;
            this.serversListView.SelectedIndexChanged += new System.EventHandler(this.serversListView_SelectedIndexChanged);
            // 
            // runScriptButton
            // 
            this.runScriptButton.Location = new System.Drawing.Point(156, 25);
            this.runScriptButton.Name = "runScriptButton";
            this.runScriptButton.Size = new System.Drawing.Size(152, 43);
            this.runScriptButton.TabIndex = 8;
            this.runScriptButton.Text = "Choose script";
            this.runScriptButton.UseVisualStyleBackColor = true;
            this.runScriptButton.Click += new System.EventHandler(this.runScriptButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openScriptFileDialog";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.runScriptButton);
            this.Controls.Add(this.serversListView);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.clientsListView);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Puppetmaster";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView clientsListView;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView serversListView;
        private System.Windows.Forms.Button runScriptButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }

}

