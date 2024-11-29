using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DashboardForm;
using Tela_Cultivo;
using UrbanGreenProject;
using Tela_Saude2;
using System.Diagnostics;

namespace Tela_Login.tela_relatorio
{
    public partial class telaRelatorio : Form
    {
        private string primeiroNome;
        public telaRelatorio()
        {
            InitializeComponent();
        }

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            try
            {
                // Verifica se o TextBox está vazio
                if (string.IsNullOrWhiteSpace(txtRelatorio.Text))
                {
                    MessageBox.Show("Por favor, insira o texto do relatório antes de salvar.");
                    return;
                }

                // Defina o caminho onde o arquivo será salvo
                string caminho = @"C:\Users\joaok\OneDrive\Documentos\Relatorio.txt";

                // Salva o conteúdo do TextBox em um arquivo .txt
                File.WriteAllText(caminho, txtRelatorio.Text);

                // Mensagem para o usuário
                MessageBox.Show("Relatório salvo com sucesso em " + caminho);
            }
            catch (Exception ex)
            {
                // Mensagem de erro
                MessageBox.Show("Ocorreu um erro ao salvar o relatório: " + ex.Message);
            }
        }



        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Se o formulário não estiver aberto, cria uma nova instância
            Dashboard dashboardForm = new Dashboard(primeiroNome);
            this.Hide(); // Oculta o formulário atual
            dashboardForm.Show(); // Exibe o novo formulário
        }

        private void button2_Click(object sender, EventArgs e)
        {
            telaCultivo cultivoForm = new telaCultivo();
            this.Hide();
            cultivoForm.Show();
        }

        private void btn_estoque_Click(object sender, EventArgs e)
        {
            EstoqueForm estoqueForm = new EstoqueForm();
            this.Hide(); // Opcional: oculta o formulário atual
            estoqueForm.Show();
        }

        private void btn_saude_Click(object sender, EventArgs e)
        {
            telaSaude saudeForm = new telaSaude();
            this.Hide();
            saudeForm.Show();
        }

        private void tela_saida_Click(object sender, EventArgs e)
        {
            // Exibir a tela de login
            Form1 telaLogin = new Form1();
            this.Hide();
            telaLogin.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Caminho do arquivo PDF
                string caminhoPdf = @"C:\Users\joaok\OneDrive\Downloads\Manual do Software.pdf";

                // Verifica se o arquivo existe
                if (File.Exists(caminhoPdf))
                {
                    // Cria um novo processo para abrir o PDF no visualizador padrão do sistema
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = caminhoPdf,
                        UseShellExecute = true // Usar o shell do sistema para abrir o arquivo com o programa padrão
                    };
                    Process.Start(startInfo);
                }
                else
                {
                    MessageBox.Show("O arquivo PDF não foi encontrado.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao tentar abrir o arquivo PDF: " + ex.Message);
            }
        }
    }
}
