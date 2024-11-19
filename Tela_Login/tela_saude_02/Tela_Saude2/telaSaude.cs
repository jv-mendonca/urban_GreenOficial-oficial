using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Tela_Cultivo;

namespace Tela_Saude2
{
    public partial class telaSaude : Form
    {

        private SqlConnection connection;
        private SqlDataAdapter adapter;
        private DataTable saudeTable;
        private string connectionString;


        public telaSaude()
        {
            InitializeComponent();
            InitialDataBaseConnection();
            loadSaude();
            ConfigureDataGridView();
            CarregarTiposParaComboBox(); // Carrega os tipos de planta��o no ComboBox

        }


        private void InitialDataBaseConnection()
        {
            string nomeBanco = "fazenda_urbana_Urban_Green_pim4";
            connectionString = $"Server=MENDON�A\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";


            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("A String de conex�o nao foi configurada corretamente");
            }
            else
            {
                connection = new SqlConnection(connectionString);
            }
        }


        private void loadSaude()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }


                adapter = new SqlDataAdapter(
            @"SELECT 
                p.cod_plantacao,         -- C�digo da planta��o
                p.tipo_plantacao,        -- Tipo de planta��o
                pd.cod_pragas_doencas,   -- C�digo das pragas/doen�as
                pd.nome_comum,           -- Nome comum da praga/doen�a
                pd.nome_cientifico,      -- Nome cient�fico da praga/doen�a
                pd.tipo,                 -- Tipo da praga/doen�a
                pd.data_deteccao,        -- Data de detec��o
                pd.eficacia,             -- Efic�cia do controle
                pd.severidade,           -- Severidade
                pd.metodo_controle       -- M�todo de controle usado
            FROM 
                Plantacao p
            LEFT JOIN 
                Controle_pragas_doencas pd ON p.cod_plantacao = pd.cod_plantacao
            WHERE 
                p.cod_plantacao IS NOT NULL
                AND pd.nome_comum IS NOT NULL
                AND pd.nome_cientifico IS NOT NULL
                AND pd.tipo IS NOT NULL
                AND pd.data_deteccao IS NOT NULL
                AND pd.eficacia IS NOT NULL
                AND pd.severidade IS NOT NULL
                AND pd.metodo_controle IS NOT NULL", connection);



                SqlCommand insertSaude = new SqlCommand(
                    "insert into Controle_pragas_doencas (cod_pragas_doencas, cod_plantacao, nome_comum,nome_cientifico,tipo, data_deteccao,eficacia, severidade,metodo_controle) " +
                    "Values (@cod_pragas_doencas,@cod_plantacao, @nome_comum, @nome_cientifico, @tipo, @data_deteccao, @eficacia, @severidade, @metodo_controle)", connection);


                // Adicionando os par�metros ao comando
                insertSaude.Parameters.Add("@cod_pragas_doencas", SqlDbType.Int,0,"cod_pragas_doencas");
                insertSaude.Parameters.Add("@cod_plantacao", SqlDbType.Int,0 ,"cod_plantacao");
                insertSaude.Parameters.Add("@nome_comum", SqlDbType.VarChar, 100, "nome_comum");
                insertSaude.Parameters.Add("@nome_cientifico", SqlDbType.VarChar, 100,"nome_cientifico");
                insertSaude.Parameters.Add("@tipo", SqlDbType.VarChar, 100, "tipo");
                insertSaude.Parameters.Add("@data_deteccao", SqlDbType.Date,0, "data_deteccao");
                insertSaude.Parameters.Add("@eficacia", SqlDbType.Int, 0, "eficacia");
               
                insertSaude.Parameters.Add("@severidade", SqlDbType.VarChar, 100, "severidade");
                insertSaude.Parameters.Add("@metodo_controle", SqlDbType.VarChar, 255, "metodo_controle");
                adapter.InsertCommand = insertSaude;


                SqlCommand updateSaude = new SqlCommand(
                    "UPDATE Controle_pragas_doencas " +
                    "SET cod_plantacao = @cod_plantacao, " +
                    "    nome_comum = @nome_comum, " +
                    "    nome_cientifico = @nome_cientifico, " +
                    "    tipo = @tipo, " +
                    "    data_deteccao = @data_deteccao, " +
                    "    eficacia = @eficacia, " +
                    "    severidade = @severidade, " +
                    "    metodo_controle = @metodo_controle " +
                    "WHERE cod_pragas_doencas = @cod_pragas_doencas", connection);

                // Adicionando os par�metros ao comando
                updateSaude.Parameters.Add("@cod_pragas_doencas", SqlDbType.Int, 0, "cod_pragas_doencas");
                updateSaude.Parameters.Add("@cod_plantacao", SqlDbType.Int,0, "cod_plantacao");
                updateSaude.Parameters.Add("@nome_comum", SqlDbType.VarChar, 100, "nome_comum");
                updateSaude.Parameters.Add("@nome_cientifico", SqlDbType.VarChar, 100, "nome_cientifico");
                updateSaude.Parameters.Add("@tipo", SqlDbType.VarChar, 100, "tipo");
                updateSaude.Parameters.Add("@data_deteccao", SqlDbType.Date, 0, "data_deteccao");
                updateSaude.Parameters.Add("@eficacia", SqlDbType.Int, 0, "eficacia");
                
                updateSaude.Parameters.Add("@severidade", SqlDbType.VarChar, 100,"severidade");
                updateSaude.Parameters.Add("@metodo_controle", SqlDbType.VarChar, 255,"metodo_controle");
                adapter.UpdateCommand = updateSaude;

                SqlCommand deleteSaude = new SqlCommand(
                "DELETE FROM Controle_pragas_doencas " +
                "WHERE cod_pragas_doencas = @cod_pragas_doencas", connection);

                // Adicionando o par�metro ao comando
                deleteSaude.Parameters.Add("@cod_pragas_doencas", SqlDbType.Int);
                adapter.DeleteCommand = deleteSaude;

                saudeTable = new DataTable();
                adapter.Fill(saudeTable);
                tabela_Doenca.DataSource = saudeTable;

                tabela_Doenca.AllowUserToAddRows = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }


            }
        }

        private void ConfigureDataGridView()
        {
            tabela_Doenca.DataSource = saudeTable;
            tabela_Doenca.AutoGenerateColumns = false;
            tabela_Doenca.Columns.Clear();


            tabela_Doenca.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "cod_pragas_doencas",
                HeaderText = "codigo",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });



            // Tipo de Planta��o (ComboBox)
            DataGridViewComboBoxColumn comboColumn = new DataGridViewComboBoxColumn
            {
                Name = "tipo_plantacao",
                HeaderText = "Tipo de Planta��o",
                DataPropertyName = "cod_plantacao", // Vinculado ao valor interno no DataTable
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            tabela_Doenca.Columns.Add(comboColumn);


            tabela_Doenca.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "nome_comum",
                HeaderText = "nome comum",
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                }
            });


            tabela_Doenca.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "nome_cientifico",
                HeaderText = "nome cientifico",
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                }
            });

            tabela_Doenca.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "tipo",
                HeaderText = "tipo",
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                }
            });


            tabela_Doenca.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "data_deteccao",
                HeaderText = "data Detec�ao",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "dd/MM/yyyy" }

            });


            tabela_Doenca.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "eficacia",
                HeaderText = "eficacia",
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                }
            });

            tabela_Doenca.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "severidade",
                HeaderText = "severidade",
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                }
            });

            tabela_Doenca.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "metodo_controle",
                HeaderText = "metodo_controle",
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                }
            });


            // Configurando as propriedades dos cabe�alhos das colunas (titulos)
            foreach (DataGridViewColumn column in tabela_Doenca.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.ForeColor = Color.White; // Cor branca para o texto do t�tulo
                column.HeaderCell.Style.BackColor = Color.FromArgb(37, 213, 116); // Cor de fundo verde para os cabe�alhos
            }

        }
        private int GetRandomPragasDoencasCode()
        {
            Random random = new Random();
            int newCode;
            do
            {
                newCode = random.Next(1, 999);

            } while (DoesCodeExist(newCode));
            return newCode;
        }


        private bool DoesCodeExist(int codPragasDoencas)
{
    using (SqlConnection connection = new SqlConnection("Server=MENDON�A\\SQLEXPRESS;Database=fazenda_urbana_Urban_Green_pim4;Trusted_Connection=True;TrustServerCertificate=True;"))
    {
        try
        {
            connection.Open();
            // Altere a consulta para verificar a tabela e coluna corretas (Controle_pragas_doencas)
            SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Controle_pragas_doencas WHERE cod_pragas_doencas = @cod_pragas_doencas", connection);
            command.Parameters.AddWithValue("@cod_pragas_doencas", codPragasDoencas);
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



        private bool isEditingRow = false;


        private void btn_newLine_Doenca_Click_1(object sender, EventArgs e)
        {
            if (isEditingRow)
            {
                MessageBox.Show("Finalize a edi��o da linha atual antes de adicionar uma nova.");
                return;
            }

            int NovoCodigo = GetRandomPragasDoencasCode();



            DataRow newRow = saudeTable.NewRow();
            newRow["cod_pragas_doencas"] = NovoCodigo;
            newRow["tipo_plantacao"] = "";
            newRow["nome_comum"] = "";
            newRow["nome_cientifico"] = "";
            newRow["tipo"] = "";
            newRow["data_deteccao"] = DBNull.Value;
            newRow["eficacia"] = DBNull.Value;
            newRow["severidade"] = "";
            newRow["metodo_controle"] = "";


            saudeTable.Rows.Add(newRow);


        }


        private void CarregarTiposParaComboBox()
        {
            DataTable plantacaoTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT cod_plantacao, tipo_plantacao FROM Plantacao";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.Fill(plantacaoTable); // Preenche os dados da tabela com a consulta
            }

            // Configura o ComboBox para usar esses dados
            DataGridViewComboBoxColumn comboColumn = (DataGridViewComboBoxColumn)tabela_Doenca.Columns["tipo_plantacao"];
            comboColumn.DataSource = plantacaoTable;
            comboColumn.DisplayMember = "tipo_plantacao"; // O que ser� exibido no ComboBox
            comboColumn.ValueMember = "cod_plantacao"; // Valor interno vinculado ao campo
        }


        private bool ValidarLinhasPragasDoencas(DataRow linha)
        {
            // Verificar se os campos obrigat�rios est�o preenchidos
            if (linha["nome_comum"] == DBNull.Value || string.IsNullOrEmpty(linha["nome_comum"].ToString()) ||
                linha["nome_cientifico"] == DBNull.Value || string.IsNullOrEmpty(linha["nome_cientifico"].ToString()) ||
                linha["tipo"] == DBNull.Value || string.IsNullOrEmpty(linha["tipo"].ToString()) ||
                linha["data_deteccao"] == DBNull.Value ||
                linha["eficacia"] == DBNull.Value ||
                linha["severidade"] == DBNull.Value ||
                string.IsNullOrEmpty(linha["metodo_controle"].ToString()))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigat�rios relacionados � praga/doen�a.");
                return false; // Retorna falso se algum campo obrigat�rio n�o estiver preenchido
            }

            // Verificar se a efic�cia est� em um formato v�lido (n�mero entre 0 e 100)
            if (Convert.ToDecimal(linha["eficacia"]) < 0 || Convert.ToDecimal(linha["eficacia"]) > 100)
            {
                MessageBox.Show("A efic�cia deve ser um n�mero entre 0 e 100.");
                return false;
            }



            // Verificar se a data de detec��o � v�lida (n�o no futuro)
            if (Convert.ToDateTime(linha["data_deteccao"]) > DateTime.Now)
            {
                MessageBox.Show("A data de detec��o n�o pode ser no futuro.");
                return false;
            }

            return true; // Retorna verdadeiro se todas as valida��es forem atendidas
        }

        private void btn_Adicionar_Click(object sender, EventArgs e)
        {
            // Validar se o DataTable possui altera��es
            if (saudeTable.GetChanges() != null)
            {
                // Verificar as linhas antes de salvar
                foreach (DataRow linha in saudeTable.Rows)
                {
                    if (linha.RowState == DataRowState.Added || linha.RowState == DataRowState.Modified)
                    {
                        if (!ValidarLinhasPragasDoencas(linha))
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
                        adapter.Update(saudeTable); // Insere ou atualiza as linhas alteradas no banco

                        MessageBox.Show("Dados de pragas/doen�as salvos com sucesso!");

                        // Confirmar as altera��es no DataTable
                        saudeTable.AcceptChanges();
                        tabela_Doenca.Refresh(); // Atualizar exibi��o no DataGridView
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

    }
}
        


   