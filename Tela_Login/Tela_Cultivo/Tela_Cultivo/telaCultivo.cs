using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using tela_de_logins;
using DashboardForm;
using Tela_Saude2;
using UrbanGreenProject;

namespace Tela_Cultivo
{
    public partial class telaCultivo : Form
    {
        private SqlConnection connection;
        private SqlDataAdapter adapter;
        private DataTable cultivoTable;
        private string connectionString;
        private string primeiroNome;

        public telaCultivo()
        {
            InitializeComponent();
            InitializeDatabaseConnection();
            LoadCultivoData();
            ConfigureDataGridView();


        }



        private void InitializeDatabaseConnection()
        {
            string nomeBanco = "fazenda_urbana_Urban_Green_pim4"; // Nome do banco de dados
            connectionString = $"Server=MENDONÇA\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";

            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("A string de conexão não foi configurada corretamente.");
            }
            else
            {
                connection = new SqlConnection(connectionString); // Inicializa a conexão
            }
        }


        private void LoadCultivoData()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                // Modifique a consulta SQL para incluir a coluna 'status_cultivo'
                adapter = new SqlDataAdapter("SELECT cod_plantacao, especie, tipo_plantacao, data_plantio, data_prevista, status_cultivo, saude_plantacao FROM Plantacao", connection);

                // Configurar InsertCommand
                SqlCommand insertCommand = new SqlCommand(
                    "INSERT INTO Plantacao (cod_plantacao, especie, tipo_plantacao, data_plantio, data_prevista, status_cultivo, saude_plantacao) " +
                    "VALUES (@cod_plantacao, @especie, @tipo_plantacao, @data_plantio, @data_prevista, @status_cultivo, @saude_plantacao)", connection);
                insertCommand.Parameters.Add("@cod_plantacao", SqlDbType.Int, 0, "cod_plantacao");
                insertCommand.Parameters.Add("@especie", SqlDbType.NVarChar, 100, "especie");
                insertCommand.Parameters.Add("@tipo_plantacao", SqlDbType.NVarChar, 100, "tipo_plantacao");
                insertCommand.Parameters.Add("@data_plantio", SqlDbType.Date, 0, "data_plantio");
                insertCommand.Parameters.Add("@data_prevista", SqlDbType.Date, 0, "data_prevista");
                insertCommand.Parameters.Add("@status_cultivo", SqlDbType.NVarChar, 50, "status_cultivo"); // Adiciona a coluna 'status_cultivo'
                insertCommand.Parameters.Add("@saude_plantacao", SqlDbType.NVarChar, 100, "saude_plantacao");
                adapter.InsertCommand = insertCommand;

                // Configurar UpdateCommand
                SqlCommand updateCommand = new SqlCommand(
                    "UPDATE Plantacao SET especie = @especie, tipo_plantacao = @tipo_plantacao, data_plantio = @data_plantio, data_prevista = @data_prevista, status_cultivo = @status_cultivo, saude_plantacao = @saude_plantacao " +
                    "WHERE cod_plantacao = @cod_plantacao", connection);
                updateCommand.Parameters.Add("@especie", SqlDbType.NVarChar, 100, "especie");
                updateCommand.Parameters.Add("@tipo_plantacao", SqlDbType.NVarChar, 100, "tipo_plantacao");
                updateCommand.Parameters.Add("@data_plantio", SqlDbType.Date, 0, "data_plantio");
                updateCommand.Parameters.Add("@data_prevista", SqlDbType.Date, 0, "data_prevista");
                updateCommand.Parameters.Add("@status_cultivo", SqlDbType.NVarChar, 50, "status_cultivo"); // Adiciona a coluna 'status_cultivo'
                updateCommand.Parameters.Add("@cod_plantacao", SqlDbType.Int, 0, "cod_plantacao");
                updateCommand.Parameters.Add("@saude_plantacao", SqlDbType.NVarChar, 100, "saude_plantacao");
                adapter.UpdateCommand = updateCommand;

                // Configurar DeleteCommand
                SqlCommand deleteCommand = new SqlCommand("DELETE FROM Plantacao WHERE cod_plantacao = @cod_plantacao", connection);
                deleteCommand.Parameters.Add("@cod_plantacao", SqlDbType.Int, 0, "cod_plantacao");
                adapter.DeleteCommand = deleteCommand;

                // Carregar os dados no DataTable
                cultivoTable = new DataTable();
                adapter.Fill(cultivoTable);
                tabelaCultivo.AllowUserToAddRows = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar dados: " + ex.Message);
            }
        }






        private void ConfigureDataGridView()
        {
            // Configurar o DataGridView
            tabelaCultivo.DataSource = cultivoTable;

            // Desabilitar a geração automática de colunas
            tabelaCultivo.AutoGenerateColumns = false;

            // Criar colunas manualmente
            tabelaCultivo.Columns.Clear();

            // Adicionando colunas com alinhamento centralizado
            tabelaCultivo.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "cod_plantacao",
                HeaderText = "Código da Plantação",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            tabelaCultivo.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "especie",
                HeaderText = "Tipo cultivo",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            tabelaCultivo.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "tipo_plantacao",
                HeaderText = "Nome da Plantação",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            tabelaCultivo.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "data_plantio",
                HeaderText = "Data de Plantio",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "d", Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            tabelaCultivo.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "data_prevista",
                HeaderText = "Data Estimada",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "d", Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Adicionando a coluna "Status do Cultivo" editável
            tabelaCultivo.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "status_cultivo",
                HeaderText = "Status do Cultivo",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            DataGridViewComboBoxColumn saudeColumn = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "saude_plantacao", // Nome da propriedade no DataTable
                HeaderText = "Saúde da Plantação",
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                DataSource = new List<string> { "Saudavel", "Intermediario", "Perigo" }, // Valores fixos
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }






            };
            tabelaCultivo.Columns.Add(saudeColumn);

            // Configurando as propriedades dos cabeçalhos das colunas (titulos)
            foreach (DataGridViewColumn column in tabelaCultivo.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.ForeColor = Color.White; // Cor branca para o texto do título
                column.HeaderCell.Style.BackColor = Color.FromArgb(37, 213, 116); // Cor de fundo verde para os cabeçalhos
            }
        }




        private bool isEditingRow = false; // Flag para saber se há uma linha em edição

        private void btn_NewCultivo_Click(object sender, EventArgs e)
        {
            // Verificar se há uma linha em edição
            if (isEditingRow)
            {
                MessageBox.Show("Finalize a edição da linha atual antes de adicionar uma nova.");
                return;
            }

            int novoCodigo = GetRandomPlantacaoCode();
            DataRow newRow = cultivoTable.NewRow();
            newRow["cod_plantacao"] = novoCodigo;
            newRow["especie"] = "";
            newRow["tipo_plantacao"] = "";
            newRow["status_cultivo"] = "";
            newRow["data_plantio"] = DBNull.Value;
            newRow["data_prevista"] = DBNull.Value;
            newRow["saude_plantacao"] = "";

            cultivoTable.Rows.Add(newRow);
            tabelaCultivo.Refresh();

            // Forçar atualização do DataGridView e iniciar edição na primeira célula
            tabelaCultivo.CurrentCell = tabelaCultivo.Rows[tabelaCultivo.Rows.Count - 1].Cells[1];



        }

        private void tabelaCultivo_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Verificar se todos os campos obrigatórios da linha foram preenchidos
            DataGridViewRow row = tabelaCultivo.Rows[e.RowIndex];

            if (ValidarLinhaPreenchida(row))
            {
                // Se todos os campos obrigatórios estiverem preenchidos, liberar a possibilidade de adicionar uma nova linha
                isEditingRow = false;
            }
            else
            {
                // Caso contrário, continuar bloqueado para adicionar nova linha
                isEditingRow = true;
            }
        }

        private bool ValidarLinhaPreenchida(DataGridViewRow linha)
        {
            // Verifica se todos os campos obrigatórios estão preenchidos na linha
            return !string.IsNullOrWhiteSpace(linha.Cells["especie"].Value.ToString()) &&
                   !string.IsNullOrWhiteSpace(linha.Cells["tipo_plantacao"].Value.ToString()) &&
                   !string.IsNullOrWhiteSpace(linha.Cells["status_cultivo"].Value.ToString()) &&
                   linha.Cells["data_plantio"].Value != DBNull.Value &&
                   linha.Cells["data_prevista"].Value != DBNull.Value;
        }





        private int GetRandomPlantacaoCode()
        {
            Random random = new Random();
            int newCode;

            do
            {
                newCode = random.Next(1, 10000); // Gera um número aleatório entre 1 e 9999
            } while (DoesCodeExist(newCode)); // Verifica se o código já existe

            return newCode; // Retorna o código único gerado
        }

        private bool DoesCodeExist(int codPlantacao)
        {
            using (SqlConnection connection = new SqlConnection("Server=MENDONÇA\\SQLEXPRESS;Database=fazenda_urbana_Urban_Green_pim4;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Plantacao WHERE cod_plantacao = @cod_plantacao", connection);
                    command.Parameters.AddWithValue("@cod_plantacao", codPlantacao);
                    int count = (int)command.ExecuteScalar();

                    return count > 0; // Retorna true se o código já existe, false caso contrário
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao verificar código: " + ex.Message);
                    return true; // Retorna true para evitar a inserção em caso de erro
                }
            }
        }
        private bool ValidarLinhaPreenchida(DataRow linha)
        {
            // Verifica se todos os campos obrigatórios estão preenchidos
            return !string.IsNullOrWhiteSpace(linha["especie"].ToString()) &&
                   !string.IsNullOrWhiteSpace(linha["tipo_plantacao"].ToString()) &&
                   !string.IsNullOrWhiteSpace(linha["status_cultivo"].ToString()) &&
                   linha["data_plantio"] != DBNull.Value &&
                   linha["data_prevista"] != DBNull.Value;
        }




        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            // Validar se o DataTable possui alterações
            if (cultivoTable.GetChanges() != null)
            {
                // Verificar as linhas antes de salvar
                foreach (DataRow linha in cultivoTable.Rows)
                {
                    if (linha.RowState == DataRowState.Added || linha.RowState == DataRowState.Modified)
                    {
                        if (!ValidarLinhaPreenchida(linha))
                        {
                            MessageBox.Show("Por favor, preencha todos os campos obrigatórios.");
                            return; // Interrompe se a linha não passar na validação
                        }
                    }
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        // Atualiza os dados na tabela de pragas/doenças usando o DataAdapter
                        adapter.Update(cultivoTable); // Insere ou atualiza as linhas alteradas no banco

                        MessageBox.Show("Dados de cultivo salvo");

                        // Confirmar as alterações no DataTable
                        cultivoTable.AcceptChanges();

                        tabelaCultivo.Refresh(); // Atualizar exibição no DataGridView

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao salvar dados: " + ex.Message);
                        Console.WriteLine("Erro ao salvar os dados: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Nenhuma alteração para salvar.");
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        // DASHBOAR
        private void button4_Click(object sender, EventArgs e)
        {
            // Verifica se o formulário já está aberto
            foreach (Form f in Application.OpenForms)
            {
                if (f is Dashboard) // Se o formulário Dashboard já estiver aberto
                {
                    f.Show(); // Exibe o formulário
                    this.Hide(); // Oculta o formulário atual
                    return; // Sai do método sem criar uma nova instância
                }
            }

            // Se o formulário não estiver aberto, cria uma nova instância
            Dashboard dashboardForm = new Dashboard(primeiroNome);
            this.Hide(); // Oculta o formulário atual
            dashboardForm.Show(); // Exibe o novo formulário
        }

        private void btn_monitoramento_Click(object sender, EventArgs e)
        {

        }

        private void btn_estoque_Click(object sender, EventArgs e)
        {
            EstoqueForm estoqueForm = new EstoqueForm();
            this.Hide(); // Opcional: oculta o formulário atual
            estoqueForm.Show();
        }

        private void btn_saude_Click(object sender, EventArgs e)
        {
            telaSaude telassa = new telaSaude();
            this.Hide();
            telassa.Show();
        }

        private void btn_relatorio_Click(object sender, EventArgs e)
        {

        }

        private void BarraPesquisa_TextChanged(object sender, EventArgs e)
        {
            string pesquisa = BarraPesquisa.Text.ToLower(); // Pega o texto da barra e converte para minúsculas para facilitar a comparação

            // Verifica se o texto digitado corresponde a algum comando para abrir uma tela
            if (pesquisa.Contains("dashboard"))
            {
                // Se o texto contém "dashboard", abre a tela Dashboard
                AbrirTelaDashboard();
            }
            else if (pesquisa.Contains("estoque"))
            {
                // Se o texto contém "estoque", abre a tela Estoque
                AbrirTelaEstoque();
            }
            else if (pesquisa.Contains("cultivo"))
            {
                // Se o texto contém "cultivo", abre a tela Cultivo
                AbrirTelaCultivo();
            }
            else if (pesquisa.Contains("saude"))
            {
                // Se o texto contém "saude", abre a tela Saude
                AbrirTelaSaude();
            }

        }

        private void AbrirTelaDashboard()
        {
            // Verifica se o Dashboard já está aberto, senão, cria uma nova instância
            foreach (Form f in Application.OpenForms)
            {
                if (f is Dashboard)
                {
                    f.Show(); // Exibe o formulário
                    this.Hide(); // Oculta o formulário atual
                    return;
                }
            }

            Dashboard dashboardForm = new Dashboard(primeiroNome);
            this.Hide();
            dashboardForm.Show();
        }

        private void AbrirTelaEstoque()
        {
            // Verifica se o Estoque já está aberto
            foreach (Form f in Application.OpenForms)
            {
                if (f is EstoqueForm)
                {
                    f.Show();
                    this.Hide();
                    return;
                }
            }

            EstoqueForm estoqueForm = new EstoqueForm();
            this.Hide();
            estoqueForm.Show();
        }

        private void AbrirTelaCultivo()
        {
            // Verifica se o Cultivo já está aberto
            foreach (Form f in Application.OpenForms)
            {
                if (f is telaCultivo)
                {
                    f.Show();
                    this.Hide();
                    return;
                }
            }

            telaCultivo cultivoForm = new telaCultivo();
            this.Hide();
            cultivoForm.Show();
        }

        private void AbrirTelaSaude()
        {
            // Verifica se a tela de Saúde já está aberta
            foreach (Form f in Application.OpenForms)
            {
                if (f is telaSaude)
                {
                    f.Show();
                    this.Hide();
                    return;
                }
            }

            telaSaude saudeForm = new telaSaude();
            this.Hide();
            saudeForm.Show();
        }
    }
}
