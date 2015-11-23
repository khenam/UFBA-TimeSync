namespace WindowsFormsApplication1
{
    partial class Servidor_config
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
            this.Ip_servidor = new System.Windows.Forms.Label();
            this.tbxHostServer = new System.Windows.Forms.TextBox();
            this.Porta_servidor = new System.Windows.Forms.Label();
            this.tbxPortServer = new System.Windows.Forms.TextBox();
            this.Botao_Servidor = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Ip_servidor
            // 
            this.Ip_servidor.AutoSize = true;
            this.Ip_servidor.Location = new System.Drawing.Point(12, 31);
            this.Ip_servidor.Name = "Ip_servidor";
            this.Ip_servidor.Size = new System.Drawing.Size(59, 13);
            this.Ip_servidor.TabIndex = 0;
            this.Ip_servidor.Text = "IP Servidor";
            this.Ip_servidor.Click += new System.EventHandler(this.label1_Click);
            // 
            // tbxHostServer
            // 
            this.tbxHostServer.Location = new System.Drawing.Point(104, 28);
            this.tbxHostServer.Name = "tbxHostServer";
            this.tbxHostServer.Size = new System.Drawing.Size(168, 20);
            this.tbxHostServer.TabIndex = 1;
			this.tbxHostServer.TextChanged += new System.EventHandler(this.tbxHostServer_TextChanged);
            // 
            // Porta_servidor
            // 
            this.Porta_servidor.AutoSize = true;
            this.Porta_servidor.Location = new System.Drawing.Point(12, 56);
            this.Porta_servidor.Name = "Porta_servidor";
            this.Porta_servidor.Size = new System.Drawing.Size(74, 13);
            this.Porta_servidor.TabIndex = 3;
            this.Porta_servidor.Text = "Porta Servidor";
            // 
            // tbxPortServer
            // 
            this.tbxPortServer.Location = new System.Drawing.Point(104, 53);
            this.tbxPortServer.Name = "tbxPortServer";
            this.tbxPortServer.Size = new System.Drawing.Size(167, 20);
            this.tbxPortServer.TabIndex = 4;
            this.tbxPortServer.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // Botao_Servidor
            // 
            this.Botao_Servidor.Location = new System.Drawing.Point(104, 97);
            this.Botao_Servidor.Name = "Botao_Servidor";
            this.Botao_Servidor.Size = new System.Drawing.Size(75, 23);
            this.Botao_Servidor.TabIndex = 5;
            this.Botao_Servidor.Text = "Enviar";
            this.Botao_Servidor.UseVisualStyleBackColor = true;
            this.Botao_Servidor.Click += new System.EventHandler(this.Botao_Servidor_Click);
            // 
            // Servidor_config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(293, 132);
            this.Controls.Add(this.Botao_Servidor);
            this.Controls.Add(this.tbxPortServer);
            this.Controls.Add(this.Porta_servidor);
            this.Controls.Add(this.tbxHostServer);
            this.Controls.Add(this.Ip_servidor);
            this.Name = "Servidor_config";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Configuração do Servidor";
            this.Load += new System.EventHandler(this.Servidor_config_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Ip_servidor;
        private System.Windows.Forms.TextBox tbxHostServer;
        private System.Windows.Forms.Label Porta_servidor;
        private System.Windows.Forms.TextBox tbxPortServer;
        private System.Windows.Forms.Button Botao_Servidor;
    }
}