namespace client
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
            this.connect_button = new System.Windows.Forms.Button();
            this.ip_label = new System.Windows.Forms.Label();
            this.port_label = new System.Windows.Forms.Label();
            this.ip_box = new System.Windows.Forms.TextBox();
            this.port_box = new System.Windows.Forms.TextBox();
            this.username_box = new System.Windows.Forms.TextBox();
            this.password_box = new System.Windows.Forms.TextBox();
            this.logs = new System.Windows.Forms.RichTextBox();
            this.username_label = new System.Windows.Forms.Label();
            this.password_label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // connect_button
            // 
            this.connect_button.Location = new System.Drawing.Point(54, 325);
            this.connect_button.Name = "connect_button";
            this.connect_button.Size = new System.Drawing.Size(107, 33);
            this.connect_button.TabIndex = 0;
            this.connect_button.Text = "Connect";
            this.connect_button.UseVisualStyleBackColor = true;
            this.connect_button.Click += new System.EventHandler(this.connect_button_Click);
            // 
            // ip_label
            // 
            this.ip_label.AutoSize = true;
            this.ip_label.Location = new System.Drawing.Point(41, 77);
            this.ip_label.Name = "ip_label";
            this.ip_label.Size = new System.Drawing.Size(28, 20);
            this.ip_label.TabIndex = 1;
            this.ip_label.Text = "IP:";
            // 
            // port_label
            // 
            this.port_label.AutoSize = true;
            this.port_label.Location = new System.Drawing.Point(41, 125);
            this.port_label.Name = "port_label";
            this.port_label.Size = new System.Drawing.Size(42, 20);
            this.port_label.TabIndex = 2;
            this.port_label.Text = "Port:";
            this.port_label.Click += new System.EventHandler(this.port_label_Click);
            // 
            // ip_box
            // 
            this.ip_box.Location = new System.Drawing.Point(136, 74);
            this.ip_box.Name = "ip_box";
            this.ip_box.Size = new System.Drawing.Size(100, 26);
            this.ip_box.TabIndex = 3;
            this.ip_box.TextChanged += new System.EventHandler(this.ip_box_TextChanged);
            // 
            // port_box
            // 
            this.port_box.Location = new System.Drawing.Point(136, 125);
            this.port_box.Name = "port_box";
            this.port_box.Size = new System.Drawing.Size(100, 26);
            this.port_box.TabIndex = 4;
            // 
            // username_box
            // 
            this.username_box.Location = new System.Drawing.Point(136, 172);
            this.username_box.Name = "username_box";
            this.username_box.Size = new System.Drawing.Size(100, 26);
            this.username_box.TabIndex = 5;
            this.username_box.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // password_box
            // 
            this.password_box.Location = new System.Drawing.Point(136, 223);
            this.password_box.Name = "password_box";
            this.password_box.Size = new System.Drawing.Size(100, 26);
            this.password_box.TabIndex = 6;
            // 
            // logs
            // 
            this.logs.Location = new System.Drawing.Point(323, 74);
            this.logs.Name = "logs";
            this.logs.Size = new System.Drawing.Size(307, 284);
            this.logs.TabIndex = 8;
            this.logs.Text = "";
            // 
            // username_label
            // 
            this.username_label.AutoSize = true;
            this.username_label.Location = new System.Drawing.Point(41, 172);
            this.username_label.Name = "username_label";
            this.username_label.Size = new System.Drawing.Size(87, 20);
            this.username_label.TabIndex = 9;
            this.username_label.Text = "Username:";
            // 
            // password_label
            // 
            this.password_label.AutoSize = true;
            this.password_label.Location = new System.Drawing.Point(41, 223);
            this.password_label.Name = "password_label";
            this.password_label.Size = new System.Drawing.Size(82, 20);
            this.password_label.TabIndex = 10;
            this.password_label.Text = "Password:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 414);
            this.Controls.Add(this.password_label);
            this.Controls.Add(this.username_label);
            this.Controls.Add(this.logs);
            this.Controls.Add(this.password_box);
            this.Controls.Add(this.username_box);
            this.Controls.Add(this.port_box);
            this.Controls.Add(this.ip_box);
            this.Controls.Add(this.port_label);
            this.Controls.Add(this.ip_label);
            this.Controls.Add(this.connect_button);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connect_button;
        private System.Windows.Forms.Label ip_label;
        private System.Windows.Forms.Label port_label;
        private System.Windows.Forms.TextBox ip_box;
        private System.Windows.Forms.TextBox port_box;
        private System.Windows.Forms.TextBox username_box;
        private System.Windows.Forms.TextBox password_box;
        private System.Windows.Forms.RichTextBox logs;
        private System.Windows.Forms.Label username_label;
        private System.Windows.Forms.Label password_label;
    }
}

