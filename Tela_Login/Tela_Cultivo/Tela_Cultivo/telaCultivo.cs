using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;
using tela_de_logins;

namespace Tela_Cultivo
{
    public partial class telaCultivo : Form
    {
        private SqlConnection connection;
        private SqlDataAdapter adapter;
        private DataTable cultivoTable;


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
            string connectionString = $"Server=MENDON�A\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";

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
                adapter = new SqlDataAdapter("SELECT cod_plantacao, especie, tipo_plantacao, data_plantio, data_prevista, status_cultivo FROM Plantacao", connection);

                // Configurar InsertCommand
                SqlCommand insertCommand = new SqlCommand(
                    "INSERT INTO Plantacao (cod_plantacao, especie, tipo_plantacao, data_plantio, data_prevista, status_cultivo) " +
                    "VALUES (@cod_plantacao, @especie, @tipo_plantacao, @data_plantio, @data_prevista, @status_cultivo)", connection);
                insertCommand.Parameters.Add("@cod_plantacao", SqlDbType.Int, 0, "cod_plantacao");
                insertCommand.Parameters.Add("@especie", SqlDbType.NVarChar, 100, "especie");
                insertCommand.Parameters.Add("@tipo_plantacao", SqlDbType.NVarChar, 100, "tipo_plantacao");
                insertCommand.Parameters.Add("@data_plantio", SqlDbType.Date, 0, "data_plantio");
                insertCommand.Parameters.Add("@data_prevista", SqlDbType.Date, 0, "data_prevista");
                insertCommand.Parameters.Add("@status_cultivo", SqlDbType.NVarChar, 50, "status_cultivo"); // Adiciona a coluna 'status_cultivo'
                adapter.InsertCommand = insertCommand;

                // Configurar UpdateCommand
                SqlCommand updateCommand = new SqlCommand(
                    "UPDATE Plantacao SET especie = @especie, tipo_plantacao = @tipo_plantacao, data_plantio = @data_plantio, data_prevista = @data_prevista, status_cultivo = @status_cultivo " +
                    "WHERE cod_plantacao = @cod_plantacao", connection);
                updateCommand.Parameters.Add("@especie", SqlDbType.NVarChar, 100, "especie");
                updateCommand.Parameters.Add("@tipo_plantacao", SqlDbType.NVarChar, 100, "tipo_plantacao");
                updateCommand.Parameters.Add("@data_plantio", SqlDbType.Date, 0, "data_plantio");
                updateCommand.Parameters.Add("@data_prevista", SqlDbType.Date, 0, "data_prevista");
                updateCommand.Parameters.Add("@status_cultivo", SqlDbType.NVarChar, 50, "status_cultivo"); // Adiciona a coluna 'status_cultivo'
                updateCommand.Parameters.Add("@cod_plantacao", SqlDbType.Int, 0, "cod_plantacao");
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

            cultivoTable.Rows.Add(newRow);
            tabelaCultivo.Refresh();

            // For�ar atualiza��o do DataGridView e iniciar edi��o na primeira c�lula
            tabelaCultivo.CurrentCell = tabelaCultivo.Rows[tabelaCultivo.Rows.Count - 1].Cells[1];
            tabelaCultivo.BeginEdit(true); // Inicia a edi��o da c�lula

            // Marcar que estamos editando uma nova linha
            isEditingRow = true;
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
            // Validar se o DataTable possui linhas para serem salvas
            if (cultivoTable.GetChanges() != null)
            {
                // Remove as linhas vazias ou inv�lidas antes de salvar
                foreach (DataRow linha in cultivoTable.Rows)
                {
                    if (linha.RowState == DataRowState.Added || linha.RowState == DataRowState.Modified)
                    {
                        if (!ValidarLinhaPreenchida(linha))
                        {
                            MessageBox.Show("Por favor, preencha todos os campos obrigat�rios.");
                            return;
                        }
                    }
                }

                using (SqlConnection connection = new SqlConnection("Server=MENDON�A\\SQLEXPRESS;Database=fazenda_urbana_Urban_Green_pim4;Trusted_Connection=True;TrustServerCertificate=True;"))
                {
                    try
                    {
                        connection.Open();
                        adapter.Update(cultivoTable);
                        MessageBox.Show("Dados salvos com sucesso!");
                        LoadCultivoData(); // Recarregar os dados ap�s salvar
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao salvar dados: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Nenhuma altera��o para salvar.");
            }
        }

    }
}
