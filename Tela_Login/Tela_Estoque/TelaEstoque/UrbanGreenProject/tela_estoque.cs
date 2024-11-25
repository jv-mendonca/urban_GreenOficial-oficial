using System;
using System.Data;
using System.Windows.Forms;
using DashboardForm;
using Microsoft.Data.SqlClient;
using Tela_Cultivo;
using Tela_Login;
using Tela_Saude2;

namespace UrbanGreenProject
{
    public partial class EstoqueForm : Form
    {
        private SqlConnection connection;
        private SqlDataAdapter adapter;
        private DataTable estoqueTable;
        private string connectionString;
        private string primeiroNome;


        public EstoqueForm()
        {
            InitializeComponent();
            InitialDataBaseConnection();
            ConfigureDataGridView();
            CarregarTiposParaComboBox(); // Carrega os tipos de plantação no ComboBox
            LoadEstoque();

        }

        private void InitialDataBaseConnection()
        {
            string nomeBanco = "fazenda_urbana_Urban_Green_pim4";
            connectionString = $"Server=MENDONÇA\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";


            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("A String de conexão nao foi configurada corretamente");
            }
            else
            {
                connection = new SqlConnection(connectionString);
            }
        }

        private void LoadEstoque()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                adapter = new SqlDataAdapter(@"
                            SELECT 
                    c.cod_colheita, 
                    c.data_colhida, 
                    c.quantidade_colhida, 
                    e.cod_estoque, 
                    e.data_entrada, 
                    e.nome_produto, 
                    e.quantidade,
                    e.unidade_medida, 
                    e.lote,
                    p.cod_plantacao,   -- Adiciona o cod_plantacao
                    p.tipo_plantacao   -- Adiciona o tipo_plantacao
                FROM 
                    Plantacao p
                LEFT JOIN 
                    Colheita c ON p.cod_plantacao = c.cod_plantacao
                LEFT JOIN 
                    Estoque e ON p.cod_plantacao = e.cod_plantacao
                WHERE 
                    c.data_colhida IS NOT NULL 
                    AND c.quantidade_colhida IS NOT NULL 
                    AND e.nome_produto IS NOT NULL 
                    AND e.quantidade IS NOT NULL 
                    AND e.unidade_medida IS NOT NULL 
                    AND e.lote IS NOT NULL
                ", connection);






                // INSERT
                SqlCommand insertColheita = new SqlCommand(
                    "INSERT INTO colheita (cod_colheita, cod_plantacao, data_colhida, quantidade_colhida) " +
                    "VALUES (@cod_colheita, @cod_plantacao, @data_colhida, @quantidade_colhida)", connection);

                // Correção: Remover o argumento de tamanho (0)
                insertColheita.Parameters.Add("@cod_colheita", SqlDbType.Int);
                insertColheita.Parameters.Add("@cod_plantacao", SqlDbType.Int);
                insertColheita.Parameters.Add("@data_colhida", SqlDbType.DateTime);
                insertColheita.Parameters.Add("@quantidade_colhida", SqlDbType.Int);

                adapter.InsertCommand = insertColheita;


                // UPDATE
                SqlCommand updateColheita = new SqlCommand(
                    "UPDATE colheita SET cod_plantacao = @cod_plantacao, data_colhida = @data_colhida, quantidade_colhida = @quantidade_colhida " +
                    "WHERE cod_colheita = @cod_colheita", connection);

                // Correção: Remover o argumento de tamanho (0)
                updateColheita.Parameters.Add("@cod_plantacao", SqlDbType.Int);
                updateColheita.Parameters.Add("@data_colhida", SqlDbType.DateTime);
                updateColheita.Parameters.Add("@quantidade_colhida", SqlDbType.Int);
                updateColheita.Parameters.Add("@cod_colheita", SqlDbType.Int);

                adapter.UpdateCommand = updateColheita;


                // DELETE
                SqlCommand deleteColheita = new SqlCommand(
                    "DELETE FROM colheita WHERE cod_colheita = @cod_colheita", connection);

                // Correção: Remover o argumento de tamanho (0)
                deleteColheita.Parameters.Add("@cod_colheita", SqlDbType.Int);

                adapter.DeleteCommand = deleteColheita;



                // INSERT
                SqlCommand insertEstoque = new SqlCommand(
                    "INSERT INTO estoque (cod_estoque, cod_plantacao, data_entrada, nome_produto, quantidade, unidade_medida, lote) " +
                    "VALUES (@cod_estoque, @cod_plantacao, @data_entrada, @nome_produto, @quantidade, @unidade_medida, @lote)", connection);
                insertEstoque.Parameters.Add("@cod_estoque", SqlDbType.Int, 0, "cod_estoque");
                insertEstoque.Parameters.Add("@cod_plantacao", SqlDbType.Int, 0, "cod_plantacao");
                insertEstoque.Parameters.Add("@data_entrada", SqlDbType.Date, 0, "data_entrada");
                insertEstoque.Parameters.Add("@nome_produto", SqlDbType.VarChar, 100, "nome_produto");
                insertEstoque.Parameters.Add("@quantidade", SqlDbType.Int, 0, "quantidade");
                insertEstoque.Parameters.Add("@unidade_medida", SqlDbType.VarChar, 10, "unidade_medida");
                insertEstoque.Parameters.Add("@lote", SqlDbType.VarChar, 100, "lote");
                adapter.InsertCommand = insertEstoque;

                // UPDATE
                SqlCommand updateEstoque = new SqlCommand(
                    "UPDATE estoque SET cod_plantacao = @cod_plantacao, data_entrada = @data_entrada, nome_produto = @nome_produto, " +
                    "quantidade = @quantidade, unidade_medida = @unidade_medida, lote = @lote WHERE cod_estoque = @cod_estoque", connection);
                updateEstoque.Parameters.Add("@cod_plantacao", SqlDbType.Int, 0, "cod_plantacao");
                updateEstoque.Parameters.Add("@data_entrada", SqlDbType.Date, 0, "data_entrada");
                updateEstoque.Parameters.Add("@nome_produto", SqlDbType.VarChar, 100, "nome_produto");
                updateEstoque.Parameters.Add("@quantidade", SqlDbType.Int, 0, "quantidade");
                updateEstoque.Parameters.Add("@unidade_medida", SqlDbType.VarChar, 10, "unidade_medida");
                updateEstoque.Parameters.Add("@lote", SqlDbType.VarChar, 100, "lote");
                updateEstoque.Parameters.Add("@cod_estoque", SqlDbType.Int, 0, "cod_estoque");
                adapter.UpdateCommand = updateEstoque;

                // DELETE
                SqlCommand deleteEstoque = new SqlCommand(
                    "DELETE FROM estoque WHERE cod_estoque = @cod_estoque", connection);
                deleteEstoque.Parameters.Add("@cod_estoque", SqlDbType.Int, 0, "cod_estoque");
                adapter.DeleteCommand = deleteEstoque;

                estoqueTable = new DataTable();
                adapter.Fill(estoqueTable);
                tabela_Estoque.DataSource = estoqueTable;

                tabela_Estoque.AllowUserToAddRows = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar dados: ", ex.Message);
            }

        }

        private void ConfigureDataGridView()
        {
            tabela_Estoque.DataSource = estoqueTable;

            tabela_Estoque.AutoGenerateColumns = false;
            tabela_Estoque.Columns.Clear();

            // Código Colheita (Texto)
            tabela_Estoque.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "cod_colheita",
                HeaderText = "ID Colheita",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Tipo de Plantação (ComboBox)
            DataGridViewComboBoxColumn comboColumn = new DataGridViewComboBoxColumn
            {
                Name = "tipo_plantacao",
                HeaderText = "Plantação",
                DataPropertyName = "cod_plantacao",
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            tabela_Estoque.Columns.Add(comboColumn);

            // Data Colhida (Texto)
            tabela_Estoque.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "data_colhida",
                HeaderText = "Data Colhida",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "dd/MM/yyyy" }
            });

            // Quantidade Colhida (Texto)
            tabela_Estoque.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "quantidade_colhida",
                HeaderText = "Quantidade Colhida",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Código Estoque (Texto)
            tabela_Estoque.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "cod_estoque",
                HeaderText = "ID Estoque",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Data Entrada (Texto)
            tabela_Estoque.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "data_entrada",
                HeaderText = "Data Entrada",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "dd/MM/yyyy" }
            });

            // Nome Produto (Texto)
            tabela_Estoque.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "nome_produto",
                HeaderText = "Produto",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Quantidade Estoque (Texto)
            tabela_Estoque.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "quantidade",
                HeaderText = "Quantidade Estoque",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Unidade Medida (Texto)
            tabela_Estoque.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "unidade_medida",
                HeaderText = "Medida",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Lote (Texto)
            tabela_Estoque.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "lote",
                HeaderText = "Lote",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Estilo do cabeçalho
            foreach (DataGridViewColumn column in tabela_Estoque.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter; // Alinhamento centralizado
                column.HeaderCell.Style.ForeColor = Color.White; // Cor branca para o texto
                column.HeaderCell.Style.BackColor = Color.FromArgb(37, 213, 116); // Cor verde para o fundo do cabeçalho
            }

            // Ajustar altura do cabeçalho
            tabela_Estoque.ColumnHeadersHeight = 50; // Define a altura do cabeçalho (ajuste conforme necessário)


            // Adiciona o evento de validação
            tabela_Estoque.CellValidating += Tabela_Estoque_CellValidating;
        }

        private void Tabela_Estoque_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            string columnName = tabela_Estoque.Columns[e.ColumnIndex].DataPropertyName;

            // Valida campos de data
            if (columnName == "data_colhida" || columnName == "data_entrada")
            {
                if (!DateTime.TryParse(e.FormattedValue.ToString(), out _))
                {
                    MessageBox.Show("Insira uma data válida no formulario.", "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true; // Cancela a edição
                }
            }

            // Valida campos de texto que não devem conter números
            else if (columnName == "nome_produto" || columnName == "unidade_medida")
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(e.FormattedValue.ToString(), @"\d"))
                {
                    MessageBox.Show("Este campo não pode conter números.", "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true; // Cancela a edição
                }
            }

            // Valida campos de número que não devem conter letras
            else if (columnName == "quantidade_colhida" || columnName == "quantidade" || columnName == "lote")
            {
                if (!int.TryParse(e.FormattedValue.ToString(), out _))
                {
                    MessageBox.Show("Este campo deve conter apenas números.", "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true; // Cancela a edição
                }
            }
        }


        private bool isEditingRow = false;








        private int getRandomColheitaCode()
        {
            Random random = new Random();
            int newCode;

            do
            {
                newCode = random.Next(1, 999);
            } while (DoesCodeExist(newCode));
            return newCode;
        }

        private int getRandomEstoqueCode()
        {
            Random random = new Random();
            int newCode;

            do
            {
                newCode = random.Next(1, 999);
            } while (DoesCodeExist(newCode));
            return (newCode);
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

        // Método para carregar os tipos de plantação no ComboBox
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
            DataGridViewComboBoxColumn comboColumn = (DataGridViewComboBoxColumn)tabela_Estoque.Columns["tipo_plantacao"];
            comboColumn.DataSource = plantacaoTable;
            comboColumn.DisplayMember = "tipo_plantacao"; // O que será exibido no ComboBox
            comboColumn.ValueMember = "cod_plantacao"; // Valor interno vinculado ao campo
        }



        private string ObterCodPlantacaoPorTipo(string tipoPlantacao)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT cod_plantacao FROM Plantacao WHERE tipo_plantacao = @tipo_plantacao";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@tipo_plantacao", tipoPlantacao);

                connection.Open();
                var result = command.ExecuteScalar();
                return result?.ToString();
            }
        }


        private bool ValidarLinhas(DataRow linha)
        {
            // Verificar se os campos obrigatórios estão preenchidos
            if (linha["data_colhida"] == DBNull.Value || linha["quantidade_colhida"] == DBNull.Value ||
                string.IsNullOrEmpty(linha["nome_produto"].ToString()) || linha["quantidade"] == DBNull.Value ||
                string.IsNullOrEmpty(linha["unidade_medida"].ToString()) || string.IsNullOrEmpty(linha["lote"].ToString()))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios.");
                return false; // Retorna falso se algum campo obrigatório não estiver preenchido
            }

            // Verificar se a quantidade está em um formato válido (número maior que 0)
            if (Convert.ToInt32(linha["quantidade_colhida"]) <= 0)
            {
                MessageBox.Show("A quantidade colhida deve ser maior que zero.");
                return false;
            }

            // Verificar se a quantidade em estoque está em um formato válido (número maior que 0)
            if (Convert.ToInt32(linha["quantidade"]) <= 0)
            {
                MessageBox.Show("A quantidade em estoque deve ser maior que zero.");
                return false;
            }

            return true; // Retorna verdadeiro se todas as linhas estiverem válidas
        }





        private void buttonAddRow_Click_1(object sender, EventArgs e)
        {
            // Adiciona uma nova linha vazia na DataGridView
            DataRow newRow = estoqueTable.NewRow();

            // Gerar novos códigos
            int novoCodigo = getRandomColheitaCode();
            int novoCodigo2 = getRandomEstoqueCode();




            newRow["cod_colheita"] = novoCodigo;
            newRow["data_colhida"] = DBNull.Value;
            newRow["quantidade_colhida"] = DBNull.Value;
            newRow["cod_estoque"] = novoCodigo2;
            newRow["data_entrada"] = DBNull.Value;
            newRow["nome_produto"] = "";
            newRow["quantidade"] = DBNull.Value;
            newRow["unidade_medida"] = "";
            newRow["lote"] = "";
            newRow["tipo_plantacao"] = "";

            // Adiciona nova linha
            estoqueTable.Rows.Add(newRow);

            // Atualiza a DataGridView
            tabela_Estoque.DataSource = estoqueTable;

            isEditingRow = true;

        }



        private void SalvarRegistro_Click(object sender, EventArgs e)
        {
            // Validar se o DataTable possui alterações
            if (estoqueTable.GetChanges() != null)
            {
                // Verificar as linhas antes de salvar
                foreach (DataRow linha in estoqueTable.Rows)
                {
                    if (linha.RowState == DataRowState.Added || linha.RowState == DataRowState.Modified)
                    {
                        if (!ValidarLinhas(linha))
                        {
                            MessageBox.Show("Por favor, preencha todos os campos obrigatórios.");
                            return;
                        }
                    }
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        // Inserir dados na tabela de colheita
                        foreach (DataRow row in estoqueTable.Rows)
                        {
                            if (row.RowState == DataRowState.Added)
                            {
                                try
                                {
                                    // Verifique os valores antes de inserir
                                    var codColheita = row["cod_colheita"];
                                    var codPlantacao = row["cod_plantacao"];
                                    var dataColhida = row["data_colhida"];
                                    var quantidadeColhida = row["quantidade_colhida"];

                                    if (codColheita == DBNull.Value || codPlantacao == DBNull.Value || dataColhida == DBNull.Value || quantidadeColhida == DBNull.Value)
                                    {
                                        MessageBox.Show("Faltando dados obrigatórios na colheita.");
                                        continue;  // Skip this iteration if data is missing
                                    }

                                    // Inserir dados na tabela de colheita
                                    SqlCommand insertColheita = new SqlCommand(
                                        "INSERT INTO colheita (cod_colheita, cod_plantacao, data_colhida, quantidade_colhida) " +
                                        "VALUES (@cod_colheita, @cod_plantacao, @data_colhida, @quantidade_colhida)", connection);

                                    insertColheita.Parameters.AddWithValue("@cod_colheita", codColheita);
                                    insertColheita.Parameters.AddWithValue("@cod_plantacao", codPlantacao);
                                    insertColheita.Parameters.AddWithValue("@data_colhida", dataColhida);
                                    insertColheita.Parameters.AddWithValue("@quantidade_colhida", quantidadeColhida);

                                    // Executa a inserção na tabela de colheita
                                    int result = insertColheita.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        Console.WriteLine($"Colheita inserida com sucesso para cod_colheita: {codColheita}");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Erro ao inserir colheita.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Erro ao inserir colheita: " + ex.Message);
                                    Console.WriteLine("Erro na inserção da colheita: " + ex.Message);
                                }
                            }
                        }

                        // Atualizando as mudanças no DataTable para a tabela de estoque
                        adapter.Update(estoqueTable);
                        MessageBox.Show("Estoque salvo com sucesso!");

                        // Confirma as mudanças feitas no DataTable
                        estoqueTable.AcceptChanges();
                        tabela_Estoque.Refresh(); // Atualiza a exibição no DataGridView
                        MessageBox.Show("Dados salvos com sucesso!");
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



        private void button2_Click(object sender, EventArgs e)
        {
            telaCultivo telaCultivo = new telaCultivo();
            this.Hide();
            telaCultivo.Show();
        }
        //dashboard
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

        private void tela_saida_Click(object sender, EventArgs e)
        {


            // Exibir a tela de login
            Form1 telaLogin = new Form1();
            this.Hide();
            telaLogin.Show();

        }
    }
}

