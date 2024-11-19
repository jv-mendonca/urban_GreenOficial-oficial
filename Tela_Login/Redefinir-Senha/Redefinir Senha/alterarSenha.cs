using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using tela_de_logins;

namespace Redefinir_Senha
{
    public partial class alterarSenha : Form
    {
        private BancoDeDados bancoDeDados;

        public alterarSenha()
        {
            InitializeComponent();
            this.FormClosed += AlterarSenha_FormClosed;
            bancoDeDados = new BancoDeDados("fazenda_urbana_Urban_Green_pim4");
            bancoDeDados.CriarBancoDeDadosSeNaoExistir();
            bancoDeDados.CriarTabelasSeNaoExistirem();
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // Associa os eventos KeyPress para impedir caracteres não numéricos
            inputDdi.KeyPress += ApenasNumeros_KeyPress;
            input_ddd.KeyPress += ApenasNumeros_KeyPress;
            input_whastapp.KeyPress += ApenasNumeros_KeyPress;

            // Define o limite de caracteres para cada campo
            inputDdi.MaxLength = 3;       // Limite de 3 dígitos para o DDI
            input_ddd.MaxLength = 2;      // Limite de 2 dígitos para o DDD
            input_whastapp.MaxLength = 9; // Limite de até 9 dígitos para o número de telefone
        }

        private void ApenasNumeros_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Verifica se o caractere não é um número e não é uma tecla de controle (como Backspace)
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // Bloqueia a entrada de caracteres não numéricos
            }
        }

        private void AlterarSenha_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private string GerarCodigoAleatorio()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Gera um código de 6 dígitos
        }

        private void EnviarCodigoPorWhatsApp(string email, string ddi, string ddd, string numeroTelefone, string codigoRecuperacao)
        {
            // Verifica se o e-mail existe
            string usuarioQuery = "SELECT COUNT(*) FROM usuario WHERE email = @Email";

            try
            {
                using (SqlConnection con = bancoDeDados.Conectar())
                {
                    using (SqlCommand cmd = new SqlCommand(usuarioQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);

                        int count = (int)cmd.ExecuteScalar();

                        if (count > 0)
                        {
                            string numeroCompleto = $"{ddi}{ddd}{numeroTelefone}"; // Concatena DDI, DDD e número
                            EnviarWhatsApp(numeroCompleto, codigoRecuperacao);
                            AbrirTelaRecuperacao(codigoRecuperacao, email, ddi, ddd, numeroTelefone);
                            MessageBox.Show("Código de redefinição enviado para o seu WhatsApp.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("E-mail não encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show("Erro ao acessar o banco de dados: " + sqlEx.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnviarWhatsApp(string numeroTelefone, string codigoRecuperacao)
        {
            string mensagem = $"Seu código de recuperação é: {codigoRecuperacao}";
            string url = $"https://api.whatsapp.com/send?phone={numeroTelefone}&text={Uri.EscapeDataString(mensagem)}";

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir o WhatsApp: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void AbrirTelaRecuperacao(string codigoRecuperacao, string email, string ddi, string ddd, string numeroTelefone)
        {
            Criar_Nova_Senha telaAlterarSenha = new Criar_Nova_Senha(codigoRecuperacao, email, ddi, ddd, numeroTelefone);
            this.Hide();
            telaAlterarSenha.Show();
        }

        private void button_Gcod_Click(object sender, EventArgs e)
        {
            string email = input_email.Text.Trim();
            string ddi = inputDdi.Text.Trim();
            string ddd = input_ddd.Text.Trim();
            string numeroTelefone = input_whastapp.Text.Trim();
            string codigoRecuperacao = GerarCodigoAleatorio();

            // Envia o código via WhatsApp
            EnviarCodigoPorWhatsApp(email, ddi, ddd, numeroTelefone, codigoRecuperacao);
        }

        
      
       
    }
}
