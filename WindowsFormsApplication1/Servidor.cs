using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeSyncNodes;

namespace WindowsFormsApplication1
{
    public partial class Servidor_config : Form
    {
        public ClientNode Node { get; set; }

        public Servidor_config()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Botao_Servidor_Click(object sender, EventArgs e)
        {
            int port;
            if (tbxHostServer.Text != String.Empty && Int32.TryParse(tbxPortServer.Text, out port))
            {
                Node = (ClientNode) NodeFactory.Build(ETypeNode.Client, tbxHostServer.Text, (uint?) port);
                Close();
            }
        }
        
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void Servidor_config_Load(object sender, EventArgs e)
        {
            
        }
    }
}
