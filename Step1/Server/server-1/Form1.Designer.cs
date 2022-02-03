namespace server_1
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
            this.tPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.logs = new System.Windows.Forms.RichTextBox();
            this.bListen = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tPort
            // 
            this.tPort.Location = new System.Drawing.Point(119, 134);
            this.tPort.Name = "tPort";
            this.tPort.Size = new System.Drawing.Size(100, 22);
            this.tPort.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(74, 136);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port:";
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(427, 90);
            this.logs.Name = "logs";
            this.logs.Size = new System.Drawing.Size(329, 371);
            this.logs.TabIndex = 4;
            this.logs.Text = "";
            // 
            // bListen
            // 
            this.bListen.Location = new System.Drawing.Point(112, 174);
            this.bListen.Name = "bListen";
            this.bListen.Size = new System.Drawing.Size(75, 23);
            this.bListen.TabIndex = 5;
            this.bListen.Text = "Listen";
            this.bListen.UseVisualStyleBackColor = true;
            this.bListen.Click += new System.EventHandler(this.bListen_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(768, 540);
            this.Controls.Add(this.bListen);
            this.Controls.Add(this.logs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tPort);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox tPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox logs;
        private System.Windows.Forms.Button bListen;
    }
}

