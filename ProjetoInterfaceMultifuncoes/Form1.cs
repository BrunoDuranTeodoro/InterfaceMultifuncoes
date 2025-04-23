using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjetoInterfaceMultifuncoes
{
    public partial class Form1 : Form
    {
        private StringBuilder bufferRecebido = new StringBuilder();

        // Construtor: Inicializa a interface e configura o temporizador e o LED
        public Form1()
        {
            InitializeComponent();
            timerCOM.Interval = 2000;  // Intervalo de 2 segundos
            timerCOM.Start();          // Inicia o timer
            picBoxLED.Tag = "Desligado"; // Estado inicial do LED
            picBoxLED.Image = Image.FromFile("C:\\Users\\Aluno\\Desktop\\InterfaceMultifuncoes\\Imagens\\LedDesligado.jpg");
            picBoxLED.SizeMode = PictureBoxSizeMode.StretchImage; // Ajusta o tamanho da imagem
        }

        // Evento ao fechar o formulário: Fecha a porta serial se estiver aberta
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
            }
        }

        // Recebe dados da porta serial e chama o processamento quando necessário
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                bufferRecebido.Append(serialPort1.ReadExisting());
                if (bufferRecebido.ToString().Contains("\n"))
                {
                    this.Invoke((MethodInvoker)trataDadosRecebidos);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao receber dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Atualiza a lista de portas COM disponíveis no comboBox
        private void atualizaListaCOMs()
        {
            comboBox1.Items.Clear();
            string[] portas = SerialPort.GetPortNames();
            if (portas.Length == 0)
            {
                comboBox1.Items.Add("Nenhuma porta encontrada");
                comboBox1.SelectedIndex = 0;
                return;
            }
            foreach (string porta in portas)
                comboBox1.Items.Add(porta);
        }

        // Processa os dados recebidos, atualiza os gauges da interface
        private void trataDadosRecebidos()
        {
            try
            {
                string[] linhas = bufferRecebido.ToString().Split('\n');
                bufferRecebido.Clear();

                if (linhas.Length > 0)
                {
                    bufferRecebido.Append(linhas[linhas.Length - 1]);
                }

                string dadosRecebidos = linhas[0].Trim();
                string[] dados = dadosRecebidos.Split(',');

                if (dados.Length == 3)
                {
                    double temperatura = double.Parse(dados[0].Replace(".", ","));
                    double tensaoA0 = double.Parse(dados[2].Replace(".", ","));

                    aGauge1.Value = Convert.ToInt32(temperatura);
                    aGauge3.Value = Convert.ToInt32((tensaoA0 / 5) * 100);
                }
                else
                {
                    MessageBox.Show("Dados recebidos estão com formato incorreto.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao processar dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Conecta à porta serial selecionada e configura as opções
        private void conectarSerial()
        {
            if (comboBox1.SelectedItem == null || comboBox1.SelectedItem.ToString() == "Nenhuma porta encontrada")
            {
                MessageBox.Show("Selecione uma porta COM válida", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                serialPort1.PortName = comboBox1.SelectedItem.ToString();
                serialPort1.BaudRate = 9600;
                serialPort1.Parity = Parity.None;
                serialPort1.StopBits = StopBits.One;
                serialPort1.Handshake = Handshake.None;
                serialPort1.Open();
                btnConectar.Text = "Desconectar";
                btnConectar.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao conectar: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Desconecta da porta serial
        private void desconectarSerial()
        {
            try
            {
                serialPort1.Close();
                comboBox1.Enabled = true;
                btnConectar.Text = "Conectar";
                btnConectar.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao desconectar: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Evento ao clicar no botão de conectar/desconectar
        private void btnConectar_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                desconectarSerial();
            }
            else
            {
                conectarSerial();
            }
        }

        // Evento do temporizador que atualiza a lista de portas COM
        private void timerCOM_Tick(object sender, EventArgs e)
        {
            atualizaListaCOMs();
        }

        // Evento ao clicar no PictureBox do LED, alternando entre ligar/desligar o LED
        private async void btnLED_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    if (btnLED.Text == "Ligar")
                    {
                        serialPort1.Write("L\n");  // Comando para ligar o LED
                        btnLED.Text = "Desligar";
                        picBoxLED.Image = Image.FromFile("C:\\Users\\Aluno\\Desktop\\InterfaceMultifuncoes\\Imagens\\LedLigado.jpg");
                        picBoxLED.SizeMode = PictureBoxSizeMode.StretchImage;
                        Thread.Sleep(500);
                    }
                    else
                    {
                        serialPort1.Write("D\n");  // Comando para Desligar o LED
                        btnLED.Text = "Ligar";
                        picBoxLED.Image = Image.FromFile("C:\\Users\\Aluno\\Desktop\\InterfaceMultifuncoes\\Imagens\\LedDesligado.jpg");
                        picBoxLED.SizeMode = PictureBoxSizeMode.StretchImage;
                        Thread.Sleep(500);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao enviar comando: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
