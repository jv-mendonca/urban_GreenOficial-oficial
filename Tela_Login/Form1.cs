
using Microsoft.Data.SqlClient;
using Redefinir_Senha;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using tela_de_logins;
using Tela_Login.Properties;
using TelasDeCadastro;

namespace Tela_Login
{
    public partial class Form1 : Form
    {
        private string nomeBanco = "fazenda_urbana_Urban_Green_pim4";
        private bool senhaVisivel = false;
        

        public Form1()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            ConfigurarFormulario();
            CriarBancoDeDados();
            LabelMensagemErro.Visible = false;



            // Associar evento de clique na imagem VisualizarSenha
            VisualizarSenha.Click += VisualizarSenha_Click;
        }

        private void CriarBancoDeDados()
        {
            BancoDeDados bancoDeDados = new BancoDeDados(nomeBanco);

            if (!bancoDeDados.VerificaBancoDeDadosExistente())
            {
                try
                {
                    bancoDeDados.CriarBancoDeDadosSeNaoExistir();
                    bancoDeDados.CriarTabelasSeNaoExistirem();
                    MessageBox.Show("Banco de dados e tabelas criados com sucesso!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao criar banco de dados ou tabelas: " + ex.Message);
                }
            }
            else
            {
                this.Hide();
                this.Show();
            }
        }

        private void ConfigurarFormulario()
        {
            input_senha.UseSystemPasswordChar = true;
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            // Adiciona o evento KeyPress ao controle de senha
            input_senha.KeyPress += new KeyPressEventHandler(InputSenha_KeyPress);
        }

        private void InputSenha_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ' ')
            {
                e.Handled = true; // Impede a digitação do espaço
            }
        }

        // Evento para alternar visibilidade da senha ao clicar na imagem
        private void VisualizarSenha_Click(object sender, EventArgs e)
        {
            senhaVisivel = !senhaVisivel; // Alterna o estado de visibilidade
            input_senha.UseSystemPasswordChar = !senhaVisivel; // Atualiza a propriedade

            // Troca a imagem do olho dependendo do estado
            VisualizarSenha.Image = senhaVisivel ? Properties.Resources.eye : Properties.Resources.hidden; // Abre/fecha olho
        }

        private void CardLogin_Paint(object sender, PaintEventArgs e)
        {
            int cornerRadius = 20;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
                path.AddArc(CardLogin.Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
                path.AddArc(CardLogin.Width - cornerRadius, CardLogin.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
                path.AddArc(0, CardLogin.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
                path.CloseFigure();

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Brush b = new SolidBrush(Color.FromArgb(40, Color.Black)))
                {
                    e.Graphics.FillPath(b, path);
                }
            }
        }

        private void AbrirLinkNoNavegador(string url)
        {
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
                MessageBox.Show($"Erro ao abrir o link: {ex.Message}");
            }
        }

        private void linkTelefone1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AbrirLinkNoNavegador("https://api.whatsapp.com/send?phone=119371577119");
        }

        private void linkTelefone2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AbrirLinkNoNavegador("https://api.whatsapp.com/send?phone=119371577119");
        }

        private void linkWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AbrirLinkNoNavegador("https://dancing-kitten-ab5a83.netlify.app/");
        }

        private void linkEndereco_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AbrirLinkNoNavegador("https://maps.google.com/?q=Av.%20Marques%20UNIP");
        }

        private void linkemail_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string emailDestinatario = "joaok81mendonca@hotmail.com";
            string urlEmail = $"https://mail.google.com/mail/?view=cm&fs=1&to={emailDestinatario}";
            AbrirLinkNoNavegador(urlEmail);
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse
        );

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string connectionString = $"Server=MENDONÇA\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";
            string loginQuery = @"
    SELECT u.cod_usuario, c.nome_cargo, f.primeiro_nome
    FROM usuario u
    INNER JOIN funcionario f ON u.cod_funcionario = f.cod_funcionario
    INNER JOIN cargo c ON f.cod_cargo = c.cod_cargo
    WHERE u.email COLLATE SQL_Latin1_General_CP1_CS_AS = @Email 
    AND u.senha COLLATE SQL_Latin1_General_CP1_CS_AS = @Senha";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(loginQuery, con);
                cmd.Parameters.AddWithValue("@Email", input_user.Text.Trim());
                cmd.Parameters.AddWithValue("@Senha", input_senha.Text.Trim());

                try
                {
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read()) // Login bem-sucedido
                        {
                            string primeiroNome = dr["primeiro_nome"].ToString();
                            
                            DashboardForm.Dashboard telaInicial = new DashboardForm.Dashboard(primeiroNome); // Abrir a tela inicial
                            telaInicial.Show();

                            this.Hide();
                        }
                        else
                        {
                            ExibirMensagemErro("Usuário ou senha inválidos.");
                        }
                    }
                }
                catch (SqlException sqlEx)
                {
                    ExibirMensagemErro("Erro ao acessar o banco de dados: " + sqlEx.Message);
                }
                catch (Exception ex)
                {
                    ExibirMensagemErro("Erro: " + ex.Message);
                }
            }
        }

        private void ExibirMensagemErro(string mensagem)
        {
            LabelMensagemErro.Text = mensagem;
            LabelMensagemErro.ForeColor = Color.Red;
            LabelMensagemErro.Visible = true;

            input_user.Clear();
            input_senha.Clear();
            input_user.Focus();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Ocultar o Form1
            this.Hide();

            // Criar uma nova instância do formulário de redefinição de senha
            alterarSenha redefinirSenhaForm = new alterarSenha();
            redefinirSenhaForm.FormClosed += (s, args) => this.Show(); // Mostrar o Form1 novamente ao fechar o redefinirSenhaForm
            redefinirSenhaForm.Show(); // Exibir o formulário

        }

        private void btnCadastrar_Click(object sender, EventArgs e)
        {
            this.Hide(); // Oculta a tela de login
            Page1 paginaCadastro = new Page1(); // Cria uma nova instância da tela de cadastro
            paginaCadastro.FormClosed += (s, args) => this.Show(); // Mostra a tela de login novamente quando a tela de cadastro for fechada
            paginaCadastro.Show(); // Exibe a tela de cadastro

        }


    }
 }



