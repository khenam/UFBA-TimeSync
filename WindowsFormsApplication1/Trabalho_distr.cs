using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Forms;
using TimeSyncBase.Connection;
using TimeSyncNodes;
using Timer = System.Timers.Timer;

namespace WindowsFormsApplication1
{
    public partial class Principal : Form
    {
        private ClientNode _node;
        private Timer _refrashTimer = new Timer();

        public Principal()
        {
            InitializeComponent();
            _refrashTimer.Interval = 10;
            _refrashTimer.Elapsed += refrashTable;
        }

        private void refrashTable(object sender, ElapsedEventArgs e)
        {
            updateViewConnections(this, _node.GetActiveConnections());   
        }

        private void visualizaçãoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*var Form = new Trabalho_distr_();
            Form.Show();*/

            if(Tabela.Visible == false)
            {
                Tabela.Visible = true;
            }
            else{

                Tabela.Visible = false;
            }
           

        }

        private void sairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Deseja encerrar a aplicação ?", "Aviso", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Environment.Exit(0);
            }
        }

        private void ajudaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Trabalho de Sistemas de Distribuídos para Mestrado de Mecatrônica - 2015 - UFBA", "Ajuda");
                     
                
        }

        private void servidorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new Servidor_config();
            form.ShowDialog(this);
            if (form.Node != null)
            {
                _node = form.Node;
//                registerEvents(_node);
                _node.StartService();
                updateViewConnections(this, _node.GetActiveConnections());
                _refrashTimer.Enabled = true;
            }
            form.Dispose();
        }

        private void registerEvents(ClientNode clientNode)
        {
            clientNode.OnNodesConnectedChange = updateViewConnections;
        }

        private void updateViewConnections(object sender, List<ConnectionBase> e)
        {
            try
            {
                Tabela.Invoke((MethodInvoker)(() =>
                {
                    try
                    {
                        if (Tabela.Rows.Count > 0)
                            Tabela.Rows.Clear();
                        foreach (var refNode in e)
                        {
                            Tabela.Rows.Add(refNode.GetIP().ToString(), refNode.GetPort().ToString(), refNode.GetLocalTime().GetDateTime().ToString());
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    
                }));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void clienteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new Cliente_config();
            form.ShowDialog(this);
            form.Dispose();
        }

        private void Principal_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_node != null)
                _node.StopService();
        }
    }
}
