using Guna.UI2.WinForms;
using System;
using Microsoft.Data.SqlClient;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using tela_de_logins;
using Tela_Login;

namespace TelasDeCadastro
{
    public partial class Page1 : Form
    {
        private int codFuncionario;
        private BancoDeDados bancoDeDados;
        private bool deveSalvarDados;

        public Page1()
        {
            InitializeComponent();

            this.FormClosing += Page1_FormClosing;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            bancoDeDados = new BancoDeDados("fazenda_urbana_Urban_Green_pim4");
        }

        public void LimparDados()
        {
            nomeCompleto.Text = string.Empty;
            cpfUsuario.Text = string.Empty;
            digitoCpf.Text = string.Empty;
            rgUsuario.Text = string.Empty;
            rgDigito.Text = string.Empty;
            dataNascimentoUsuario.Text = string.Empty;
        }

        private bool ValidarCPF(string cpf)
        {
            // Remove apenas para validação
            string cpfLimpo = cpf.Replace(".", "").Replace("-", "").Replace(" ", "").Trim();

            if (cpfLimpo.Length != 11 || !long.TryParse(cpfLimpo, out _))
                return false;

            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += (int)(cpfLimpo[i] - '0') * (10 - i);

            int digito1 = 11 - (soma % 11);
            if (digito1 >= 10) digito1 = 0;

            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += (int)(cpfLimpo[i] - '0') * (11 - i);

            int digito2 = 11 - (soma % 11);
            if (digito2 >= 10) digito2 = 0;

            return cpfLimpo[9] == (char)(digito1 + '0') && cpfLimpo[10] == (char)(digito2 + '0');
        }

        private bool ValidarRG(string rg)
        {
            // Remove caracteres especiais para validação
            string rgLimpo = rg.Replace(".", "").Replace("-", "").Replace("/", "").Trim();

            if (rgLimpo.Length < 8 || !long.TryParse(rgLimpo.Substring(0, 8), out _))
                return false;

            string rgBase = rgLimpo.Substring(0, 8);
            char digitoVerificador = rgLimpo.Length > 8 ? rgLimpo[8] : '0'; // Verifica se há um dígito verificador

            int soma = 0;
            int peso = 2;

            for (int i = rgBase.Length - 1; i >= 0; i--)
            {
                soma += (rgBase[i] - '0') * peso;
                peso++;
                if (peso > 9) peso = 2;
            }

            int resultado = soma % 11;
            char digitoCalculado = (resultado < 10) ? (char)(resultado + '0') : 'X';

            return digitoCalculado == digitoVerificador;
        }

        private bool ValidarDataNascimento(DateTime dataNascimento)
        {
            DateTime dataMinima = new DateTime(1900, 1, 1);
            DateTime dataMaxima = DateTime.Today;

            return dataNascimento >= dataMinima && dataNascimento <= dataMaxima;
        }

        private int GerarCodigoFuncionario()
        {
            Random random = new Random();
            return random.Next(10000, 99999);
        }

        private void InserirFuncionario(int codFuncionario, string rg, string cpf, DateTime dataNascimento, string primeiroNome, string ultimoNome)
        {
            int codCargo = 2; // Definindo o código do cargo como 2
            string query = "INSERT INTO funcionario (cod_funcionario, cod_cargo, rg, cpf, data_nascimento, primeiro_nome, ultimo_nome) VALUES (@CodFuncionario, @CodCargo, @Rg, @Cpf, @DataNascimento, @primeiroNome, @ultimoNome)";

            using (SqlConnection con = bancoDeDados.Conectar())
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CodFuncionario", codFuncionario);
                    cmd.Parameters.AddWithValue("@CodCargo", codCargo);
                    cmd.Parameters.AddWithValue("@Rg", rg.Replace(".", "").Replace("-", "").Replace("/", "").Trim()); // Remove caracteres especiais ao salvar
                    cmd.Parameters.AddWithValue("@Cpf", cpf.Replace(".", "").Replace("-", "").Replace(" ", "").Trim()); // Remove caracteres especiais ao salvar
                    cmd.Parameters.AddWithValue("@DataNascimento", dataNascimento);
                    cmd.Parameters.AddWithValue("@primeiroNome", primeiroNome);
                    cmd.Parameters.AddWithValue("@ultimoNome", ultimoNome);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void Page1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!deveSalvarDados)
            {
                DialogResult resultado = MessageBox.Show("Tem certeza que deseja sair? Todos os dados não salvos serão perdidos.", "Confirmação", MessageBoxButtons.YesNo);
                e.Cancel = (resultado == DialogResult.No);
            }
        }

        private void botao_continuar_Click(object sender, EventArgs e)
        {
            string nomeCompleto = this.nomeCompleto.Text.Trim();
            var partesNome = nomeCompleto.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string primeiroNome = partesNome.Length > 0 ? partesNome[0] : string.Empty;
            string ultimoNome = partesNome.Length > 1 ? partesNome[^1] : string.Empty;

            string cpf = $"{cpfUsuario.Text.Trim()}{digitoCpf.Text.Trim()}";
            string rg = $"{rgUsuario.Text.Trim()}{rgDigito.Text.Trim()}";

            if (!ValidarCPF(cpf))
            {
                MessageBox.Show("CPF inválido! Por favor, insira um CPF válido.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ValidarRG(rg))
            {
                MessageBox.Show("RG inválido! Por favor, insira um RG válido.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!DateTime.TryParse(dataNascimentoUsuario.Text.Trim(), out DateTime dataNascimento))
            {
                MessageBox.Show("Data de nascimento inválida! Por favor, insira uma data no formato correto (dd/MM/yyyy).", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ValidarDataNascimento(dataNascimento))
            {
                MessageBox.Show("Data de nascimento inválida! Por favor, insira uma data válida.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            codFuncionario = GerarCodigoFuncionario();

            try
            {
                InserirFuncionario(codFuncionario, rg, cpf, dataNascimento, primeiroNome, ultimoNome);
                deveSalvarDados = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao cadastrar funcionário: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Criar uma instância da Page3, passando os dados necessários e a referência da Page1
            Page3 page3 = new Page3(codFuncionario);

            page3.Show(); // Mostra a Page3
            this.Hide(); // Oculta a Page1
        }

        private void botao_voltar_Click_1(object sender, EventArgs e)
        {
            Form1 loginForm = new Form1();
            loginForm.Show();
            this.Hide();
        }
    }
}
