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
            connectionString = $"Server=MENDON�A\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";

            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("A string de conex�o n�o foi configurada corretamente.");
            }
            else
            {
                connection = new SqlConnection(connectionString); // Inicializa a conex�o
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

            // Desabilitar a gera��o autom�tica de colunas
            tabelaCultivo.AutoGenerateColumns = false;

            // Criar colunas manualmente
            tabelaCultivo.Columns.Clear();

            // Adicionando colunas com alinhamento centralizado
            tabelaCultivo.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "cod_plantacao",
                HeaderText = "C�digo da Planta��o",
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
                HeaderText = "Nome da Planta��o",
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

            // Adicionando a coluna "Status do Cultivo" edit�vel
            tabelaCultivo.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "status_cultivo",
                HeaderText = "Status do Cultivo",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            DataGridViewComboBoxColumn saudeColumn = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "saude_plantacao", // Nome da propriedade no DataTable
                HeaderText = "Sa�de da Planta��o",
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                DataSource = new List<string> { "Saudavel", "Intermediario", "Perigo" }, // Valores fixos
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }






            };
            tabelaCultivo.Columns.Add(saudeColumn);

            // Configurando as propriedades dos cabe�alhos das colunas (titulos)
            foreach (DataGridViewColumn column in tabelaCultivo.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.ForeColor = Color.White; // Cor branca para o texto do t�tulo
                column.HeaderCell.Style.BackColor = Color.FromArgb(37, 213, 116); // Cor de fundo verde para os cabe�alhos
            }
        }




        private bool isEditingRow = false; // Flag para saber se h� uma linha em edi��o

        private void btn_NewCultivo_Click(object sender, EventArgs e)
        {
            // Verificar se h� uma linha em edi��o
            if (isEditingRow)
            {
                MessageBox.Show("Finalize a edi��o da linha atual antes de adicionar uma nova.");
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

            // For�ar atualiza��o do DataGridView e iniciar edi��o na primeira c�lula
            tabelaCultivo.CurrentCell = tabelaCultivo.Rows[tabelaCultivo.Rows.Count - 1].Cells[1];



        }

        private void tabelaCultivo_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Verificar se todos os campos obrigat�rios da linha foram preenchidos
            DataGridViewRow row = tabelaCultivo.Rows[e.RowIndex];

            if (ValidarLinhaPreenchida(row))
            {
                // Se todos os campos obrigat�rios estiverem preenchidos, liberar a possibilidade de adicionar uma nova linha
                isEditingRow = false;
            }
            else
            {
                // Caso contr�rio, continuar bloqueado para adicionar nova linha
                isEditingRow = true;
            }
        }

        private bool ValidarLinhaPreenchida(DataGridViewRow linha)
        {
            // Verifica se todos os campos obrigat�rios est�o preenchidos na linha
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
                newCode = random.Next(1, 10000); // Gera um n�mero aleat�rio entre 1 e 9999
            } while (DoesCodeExist(newCode)); // Verifica se o c�digo j� existe

            return newCode; // Retorna o c�digo �nico gerado
        }

        private bool DoesCodeExist(int codPlantacao)
        {
            using (SqlConnection connection = new SqlConnection("Server=MENDON�A\\SQLEXPRESS;Database=fazenda_urbana_Urban_Green_pim4;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Plantacao WHERE cod_plantacao = @cod_plantacao", connection);
                    command.Parameters.AddWithValue("@cod_plantacao", codPlantacao);
                    int count = (int)command.ExecuteScalar();

                    return count > 0; // Retorna true se o c�digo j� existe, false caso contr�rio
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao verificar c�digo: " + ex.Message);
                    return true; // Retorna true para evitar a inser��o em caso de erro
                }
            }
        }
        private bool ValidarLinhaPreenchida(DataRow linha)
        {
            // Verifica se todos os campos obrigat�rios est�o preenchidos
            return !string.IsNullOrWhiteSpace(linha["especie"].ToString()) &&
                   !string.IsNullOrWhiteSpace(linha["tipo_plantacao"].ToString()) &&
                   !string.IsNullOrWhiteSpace(linha["status_cultivo"].ToString()) &&
                   linha["data_plantio"] != DBNull.Value &&
                   linha["data_prevista"] != DBNull.Value;
        }




        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            // Validar se o DataTable possui altera��es
            if (cultivoTable.GetChanges() != null)
            {
                // Verificar as linhas antes de salvar
                foreach (DataRow linha in cultivoTable.Rows)
                {
                    if (linha.RowState == DataRowState.Added || linha.RowState == DataRowState.Modified)
                    {
                        if (!ValidarLinhaPreenchida(linha))
                        {
                            MessageBox.Show("Por favor, preencha todos os campos obrigat�rios.");
                            return; // Interrompe se a linha n�o passar na valida��o
                        }
                    }
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        // Atualiza os dados na tabela de pragas/doen�as usando o DataAdapter
                        adapter.Update(cultivoTable); // Insere ou atualiza as linhas alteradas no banco

                        MessageBox.Show("Dados de cultivo salvo");

                        // Confirmar as altera��es no DataTable
                        cultivoTable.AcceptChanges();

                        tabelaCultivo.Refresh(); // Atualizar exibi��o no DataGridView

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
                MessageBox.Show("Nenhuma altera��o para salvar.");
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        // DASHBOAR
        private void button4_Click(object sender, EventArgs e)
        {
            // Verifica se o formul�rio j� est� aberto
            foreach (Form f in Application.OpenForms)
            {
                if (f is Dashboard) // Se o formul�rio Dashboard j� estiver aberto
                {
                    f.Show(); // Exibe o formul�rio
                    this.Hide(); // Oculta o formul�rio atual
                    return; // Sai do m�todo sem criar uma nova inst�ncia
                }
            }

            // Se o formul�rio n�o estiver aberto, cria uma nova inst�ncia
            Dashboard dashboardForm = new Dashboard(primeiroNome);
            this.Hide(); // Oculta o formul�rio atual
            dashboardForm.Show(); // Exibe o novo formul�rio
        }

        private void btn_monitoramento_Click(object sender, EventArgs e)
        {

        }

        private void btn_estoque_Click(object sender, EventArgs e)
        {
            EstoqueForm estoqueForm = new EstoqueForm();
            this.Hide(); // Opcional: oculta o formul�rio atual
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
            string pesquisa = BarraPesquisa.Text.ToLower(); // Pega o texto da barra e converte para min�sculas para facilitar a compara��o

            // Verifica se o texto digitado corresponde a algum comando para abrir uma tela
            if (pesquisa.Contains("dashboard"))
            {
                // Se o texto cont�m "dashboard", abre a tela Dashboard
                AbrirTelaDashboard();
            }
            else if (pesquisa.Contains("estoque"))
            {
                // Se o texto cont�m "estoque", abre a tela Estoque
                AbrirTelaEstoque();
            }
            else if (pesquisa.Contains("cultivo"))
            {
                // Se o texto cont�m "cultivo", abre a tela Cultivo
                AbrirTelaCultivo();
            }
            else if (pesquisa.Contains("saude"))
            {
                // Se o texto cont�m "saude", abre a tela Saude
                AbrirTelaSaude();
            }

        }

        private void AbrirTelaDashboard()
        {
            // Verifica se o Dashboard j� est� aberto, sen�o, cria uma nova inst�ncia
            foreach (Form f in Application.OpenForms)
            {
                if (f is Dashboard)
                {
                    f.Show(); // Exibe o formul�rio
                    this.Hide(); // Oculta o formul�rio atual
                    return;
                }
            }

            Dashboard dashboardForm = new Dashboard(primeiroNome);
            this.Hide();
            dashboardForm.Show();
        }

        private void AbrirTelaEstoque()
        {
            // Verifica se o Estoque j� est� aberto
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
            // Verifica se o Cultivo j� est� aberto
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
            // Verifica se a tela de Sa�de j� est� aberta
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
