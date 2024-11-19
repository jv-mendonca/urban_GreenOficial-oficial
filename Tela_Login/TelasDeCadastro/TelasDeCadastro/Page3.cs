using System;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using tela_de_logins;

namespace TelasDeCadastro
{
    public partial class Page3 : Form
    {
        private BancoDeDados bancoDeDados;
        private int codFuncionario;
        private bool deveSalvarDados = false; // Variável para controlar se os dados devem ser salvos

        public Page3(int codFuncionario)
        {
            InitializeComponent();
            bancoDeDados = new BancoDeDados("fazenda_urbana_Urban_Green_pim4");
            this.codFuncionario = codFuncionario;
            this.AutoScaleMode = AutoScaleMode.Dpi; // Ajusta para o DPI do sistema
            this.FormClosing += new FormClosingEventHandler(Page3_FormClosing); // Adiciona o manipulador de eventos
        }

        private int VerificarOuInserirEstado(string estado)
        {
            string querySelect = "SELECT cod_estado FROM estado WHERE unidade_federativa = @Estado";
            string queryInsert = "INSERT INTO estado (cod_estado, unidade_federativa) VALUES (@CodEstado, @Estado)";

            using (SqlConnection con = bancoDeDados.Conectar())
            {
                using (SqlCommand cmd = new SqlCommand(querySelect, con))
                {
                    cmd.Parameters.AddWithValue("@Estado", estado);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        cmd.CommandText = queryInsert;
                        int novoId = GerarNumeroAleatorioUnico("estado", "cod_estado");
                        cmd.Parameters.AddWithValue("@CodEstado", novoId);
                        cmd.ExecuteNonQuery();
                        return novoId;
                    }
                }
            }
        }

        private void Btn_continuar_Click(object sender, EventArgs e)
        {
            // Verificar se todos os campos obrigatórios estão preenchidos
            if (string.IsNullOrWhiteSpace(txtEstado.Text) || string.IsNullOrWhiteSpace(txtCidade.Text) ||
                string.IsNullOrWhiteSpace(txtBairro.Text) || string.IsNullOrWhiteSpace(txtLogradouro.Text) ||
                string.IsNullOrWhiteSpace(txtNumero.Text) || string.IsNullOrWhiteSpace(txtCep.Text) ||
                string.IsNullOrWhiteSpace(txtTelefone.Text))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios.", "Campos Obrigatórios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Obter o estado digitado pelo usuário e converter para maiúsculas
            string estadoDigitado = txtEstado.Text.ToUpper();
            string cidade = txtCidade.Text.ToUpper();
            string bairro = txtBairro.Text.ToUpper();
            string logradouro = txtLogradouro.Text.ToUpper();
            string numero = txtNumero.Text;
            string cep = txtCep.Text.Replace(" ", "").Replace("-", ""); // Remove espaços e hífens
            string telefoneCompleto = txtTelefone.Text.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", ""); // Remove espaços e caracteres especiais
            string complemento = txtComplemento.Text.ToUpper(); // Obter o complemento e converter para maiúsculas

            try
            {
                // Verificar ou inserir o estado
                int idEstado = VerificarOuInserirEstado(estadoDigitado);

                // Verificar ou inserir a cidade
                int idCidade = VerificarOuInserirCidade(cidade, idEstado);

                // Verificar ou inserir o bairro
                int idBairro = VerificarOuInserirBairro(bairro, idCidade);

                // Verificar ou inserir o CEP
                int idCep = VerificarOuInserirCep(cep);

                // Verificar ou inserir o endereço no banco
                int idEndereco = VerificarOuInserirEndereco(idCidade, idBairro, idCep, logradouro, numero, complemento);

                // Adicionar também o endereço ao funcionário
                InserirFuncionarioEndereco(codFuncionario, idEndereco);

                // Inserir o telefone e obter o código do telefone inserido
                int codTelefone = InserirTelefone(telefoneCompleto);
                if (codTelefone != -1) // Verifica se a inserção do telefone foi bem-sucedida
                {
                    InserirFuncionarioTelefone(codFuncionario, codTelefone);
                }

                deveSalvarDados = true; // Marca que os dados foram salvos
                Page4 page4 = new Page4(codFuncionario);
                page4.Show();  // Abre a Page4
                this.Close(); // Fecha a página após salvar os dados
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar dados: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Page3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!deveSalvarDados) // Se não houver necessidade de salvar dados, exibe a mensagem
            {
                DialogResult resultado = MessageBox.Show("Tem certeza que deseja sair? Todos os dados não salvos serão perdidos.", "Confirmação", MessageBoxButtons.YesNo);
                if (resultado == DialogResult.Yes)
                {
                    Application.Exit(); // Fecha toda a aplicação
                }
                else
                {
                    e.Cancel = true; // Cancela o fechamento se o usuário não confirmar
                }
            }
            else
            {
                this.Dispose(); // Opcional: Dispose do formulário aqui, mas normalmente o Close já faz isso.
            }
        }

        // Método para garantir que o número gerado seja único
        private int GerarNumeroAleatorioUnico(string tabela, string coluna)
        {
            int novoNumero;
            bool numeroExiste;

            // Loop até encontrar um número que não esteja duplicado
            do
            {
                Random random = new Random();
                novoNumero = random.Next(100000, 999999);

                // Verifica se o número já existe no banco de dados
                string query = $"SELECT COUNT(1) FROM {tabela} WHERE {coluna} = @Numero";

                using (SqlConnection con = bancoDeDados.Conectar())
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Numero", novoNumero);
                        numeroExiste = (int)cmd.ExecuteScalar() > 0;
                    }
                }

            } while (numeroExiste);

            return novoNumero;
        }

        private int VerificarOuInserirCidade(string cidade, int idEstado)
        {
            string querySelect = "SELECT cod_cidade FROM cidade WHERE nome_cidade = @Cidade AND cod_estado = @Estado";
            string queryInsert = "INSERT INTO cidade (cod_cidade, nome_cidade, cod_estado) VALUES (@CodCidade, @Cidade, @Estado)";

            using (SqlConnection con = bancoDeDados.Conectar())
            {
                using (SqlCommand cmd = new SqlCommand(querySelect, con))
                {
                    cmd.Parameters.AddWithValue("@Cidade", cidade);
                    cmd.Parameters.AddWithValue("@Estado", idEstado);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        cmd.CommandText = queryInsert;
                        int novoId = GerarNumeroAleatorioUnico("cidade", "cod_cidade");
                        cmd.Parameters.AddWithValue("@CodCidade", novoId);
                        cmd.ExecuteNonQuery();
                        return novoId;
                    }
                }
            }
        }

        private int VerificarOuInserirBairro(string bairro, int idCidade)
        {
            string querySelect = "SELECT cod_bairro FROM bairro WHERE nome_bairro = @Bairro AND cod_cidade = @Cidade";
            string queryInsert = "INSERT INTO bairro (cod_bairro, nome_bairro, cod_cidade) VALUES (@CodBairro, @Bairro, @Cidade)";

            using (SqlConnection con = bancoDeDados.Conectar())
            {
                using (SqlCommand cmd = new SqlCommand(querySelect, con))
                {
                    cmd.Parameters.AddWithValue("@Bairro", bairro);
                    cmd.Parameters.AddWithValue("@Cidade", idCidade);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        cmd.CommandText = queryInsert;
                        int novoId = GerarNumeroAleatorioUnico("bairro", "cod_bairro");
                        cmd.Parameters.AddWithValue("@CodBairro", novoId);
                        cmd.ExecuteNonQuery();
                        return novoId;
                    }
                }
            }
        }

        private int VerificarOuInserirCep(string cep)
        {
            string querySelect = "SELECT cod_cep FROM cep WHERE cep = @Cep";
            string queryInsert = "INSERT INTO cep (cod_cep, cep) VALUES (@CodCep, @Cep)";

            using (SqlConnection con = bancoDeDados.Conectar())
            {
                using (SqlCommand cmd = new SqlCommand(querySelect, con))
                {
                    cmd.Parameters.AddWithValue("@Cep", cep);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        cmd.CommandText = queryInsert;
                        int novoId = GerarNumeroAleatorioUnico("cep", "cod_cep");
                        cmd.Parameters.AddWithValue("@CodCep", novoId);
                        cmd.ExecuteNonQuery();
                        return novoId;
                    }
                }
            }
        }

        private int VerificarOuInserirEndereco(int idCidade, int idBairro, int idCep, string logradouro, string numero, string complemento)
        {
            string querySelect = "SELECT cod_endereco FROM endereco WHERE logradouro = @Logradouro AND numero = @Numero AND cod_cidade = @Cidade AND cod_bairro = @Bairro AND cod_cep = @Cep";
            string queryInsert = "INSERT INTO endereco (cod_endereco, logradouro, numero, complemento, cod_cidade, cod_bairro, cod_cep) VALUES (@CodEndereco, @Logradouro, @Numero, @Complemento, @Cidade, @Bairro, @Cep)";

            using (SqlConnection con = bancoDeDados.Conectar())
            {
                using (SqlCommand cmd = new SqlCommand(querySelect, con))
                {
                    cmd.Parameters.AddWithValue("@Logradouro", logradouro);
                    cmd.Parameters.AddWithValue("@Numero", numero);
                    cmd.Parameters.AddWithValue("@Cidade", idCidade);
                    cmd.Parameters.AddWithValue("@Bairro", idBairro);
                    cmd.Parameters.AddWithValue("@Cep", idCep);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        cmd.CommandText = queryInsert;
                        int novoId = GerarNumeroAleatorioUnico("endereco", "cod_endereco");
                        cmd.Parameters.AddWithValue("@CodEndereco", novoId);
                        cmd.Parameters.AddWithValue("@Complemento", complemento);
                        cmd.ExecuteNonQuery();
                        return novoId;
                    }
                }
            }
        }

        private void InserirFuncionarioEndereco(int idFuncionario, int idEndereco)
        {
            string queryInsert = "INSERT INTO funcionario_endereco (cod_funcionario, cod_endereco) VALUES (@CodFuncionario, @CodEndereco)";

            using (SqlConnection con = bancoDeDados.Conectar())
            {
                using (SqlCommand cmd = new SqlCommand(queryInsert, con))
                {
                    cmd.Parameters.AddWithValue("@CodFuncionario", idFuncionario);
                    cmd.Parameters.AddWithValue("@CodEndereco", idEndereco);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        // Método para extrair DDI, DDD e número
        private (string Ddi, string Ddd, string Numero) ExtrairTelefone(string telefoneCompleto)
        {
            telefoneCompleto = telefoneCompleto.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            string ddi = "55"; // DDI padrão para o Brasil

            string ddd = "";
            string numero = "";

            if (telefoneCompleto.Length >= 10)
            {
                ddd = telefoneCompleto.Substring(0, 2);
                numero = telefoneCompleto.Substring(2);
            }
            else
            {
                throw new FormatException("Número de telefone inválido.");
            }

            return (ddi, ddd, numero);
        }

        // Método para inserir telefone
        private int InserirTelefone(string telefoneCompleto)
        {
            try
            {
                var (ddi, ddd, numero) = ExtrairTelefone(telefoneCompleto);

                int codTelefone = GerarNumeroAleatorioUnico("telefone", "cod_telefone");
                string query = "INSERT INTO telefone (cod_telefone, ddi, ddd, numero_telefone) VALUES (@CodTelefone, @Ddi, @Ddd, @Numero)";

                using (SqlConnection con = bancoDeDados.Conectar())
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@CodTelefone", codTelefone);
                        cmd.Parameters.AddWithValue("@Ddi", ddi);
                        cmd.Parameters.AddWithValue("@Ddd", ddd);
                        cmd.Parameters.AddWithValue("@Numero", numero);
                        cmd.ExecuteNonQuery();
                    }
                }

                return codTelefone; // Retorna o código do telefone inserido
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "Erro de Formato", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1; // Retorna um valor inválido em caso de erro
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao inserir telefone: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        // Método para associar telefone ao funcionário
        private void InserirFuncionarioTelefone(int codFuncionario, int codTelefone)
        {
            string query = "INSERT INTO funcionario_telefone(cod_funcionario, cod_telefone) VALUES (@CodFuncionario, @CodTelefone)";

            using (SqlConnection con = bancoDeDados.Conectar())
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CodFuncionario", codFuncionario);
                    cmd.Parameters.AddWithValue("@CodTelefone", codTelefone);
                    cmd.ExecuteNonQuery();
                }
            }
        }






        private void ExcluirDadosFuncionario(int codFuncionario)
        {
            // Adicione aqui a lógica para excluir os dados do funcionário
            using (SqlConnection con = bancoDeDados.Conectar())
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    // Excluir telefone associado ao funcionário
                    cmd.CommandText = "DELETE FROM funcionario_telefone WHERE cod_funcionario = @CodFuncionario";
                    cmd.Parameters.AddWithValue("@CodFuncionario", codFuncionario);
                    cmd.ExecuteNonQuery();

                    // Excluir endereço associado ao funcionário
                    cmd.CommandText = "DELETE FROM funcionario_endereco WHERE cod_funcionario = @CodFuncionario";
                    cmd.ExecuteNonQuery();

                    // Excluir o funcionário
                    cmd.CommandText = "DELETE FROM funcionario WHERE cod_funcionario = @CodFuncionario";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void btn_voltar_Click(object sender, EventArgs e)
        {

            ExcluirDadosFuncionario(codFuncionario);
            Page1 page1 = new Page1();

            // Exibe o formulário de login
            page1.Show();

            // Oculta o formulário atual
            this.Hide();


        }
    }
}