using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Tela_Cultivo;
using static DashboardForm.Dashboard;

namespace Tela_Saude2
{
    public partial class telaSaude : Form
    {

        private SqlConnection connection;
        private SqlDataAdapter adapter;
        private DataTable saudeTable;
        private string connectionString;
        private int indiceAtual = 0;  // Armazena o índice da espécie atual a ser exibida
        private List<string> especies = new List<string>(); // Lista de espécies para navegação



        public telaSaude()
        {
            InitializeComponent();
            InitialDataBaseConnection();
            loadSaude();
            ConfigureDataGridView();
            CarregarTiposParaComboBox(); // Carrega os tipos de plantação no ComboBox
            AtualizarGraficos();
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
                p.cod_plantacao,         -- Código da plantação
                p.tipo_plantacao,        -- Tipo de plantação
                pd.cod_pragas_doencas,   -- Código das pragas/doenças
                pd.nome_comum,           -- Nome comum da praga/doença
                pd.nome_cientifico,      -- Nome científico da praga/doença
                pd.tipo,                 -- Tipo da praga/doença
                pd.data_deteccao,        -- Data de detecção
                pd.eficacia,             -- Eficácia do controle
                pd.severidade,           -- Severidade
                pd.metodo_controle       -- Método de controle usado
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


                // Adicionando os parâmetros ao comando
                insertSaude.Parameters.Add("@cod_pragas_doencas", SqlDbType.Int, 0, "cod_pragas_doencas");
                insertSaude.Parameters.Add("@cod_plantacao", SqlDbType.Int, 0, "cod_plantacao");
                insertSaude.Parameters.Add("@nome_comum", SqlDbType.VarChar, 100, "nome_comum");
                insertSaude.Parameters.Add("@nome_cientifico", SqlDbType.VarChar, 100, "nome_cientifico");
                insertSaude.Parameters.Add("@tipo", SqlDbType.VarChar, 100, "tipo");
                insertSaude.Parameters.Add("@data_deteccao", SqlDbType.Date, 0, "data_deteccao");
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

                // Adicionando os parâmetros ao comando
                updateSaude.Parameters.Add("@cod_pragas_doencas", SqlDbType.Int, 0, "cod_pragas_doencas");
                updateSaude.Parameters.Add("@cod_plantacao", SqlDbType.Int, 0, "cod_plantacao");
                updateSaude.Parameters.Add("@nome_comum", SqlDbType.VarChar, 100, "nome_comum");
                updateSaude.Parameters.Add("@nome_cientifico", SqlDbType.VarChar, 100, "nome_cientifico");
                updateSaude.Parameters.Add("@tipo", SqlDbType.VarChar, 100, "tipo");
                updateSaude.Parameters.Add("@data_deteccao", SqlDbType.Date, 0, "data_deteccao");
                updateSaude.Parameters.Add("@eficacia", SqlDbType.Int, 0, "eficacia");

                updateSaude.Parameters.Add("@severidade", SqlDbType.VarChar, 100, "severidade");
                updateSaude.Parameters.Add("@metodo_controle", SqlDbType.VarChar, 255, "metodo_controle");
                adapter.UpdateCommand = updateSaude;

                SqlCommand deleteSaude = new SqlCommand(
                "DELETE FROM Controle_pragas_doencas " +
                "WHERE cod_pragas_doencas = @cod_pragas_doencas", connection);

                // Adicionando o parâmetro ao comando
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



            // Tipo de Plantação (ComboBox)
            DataGridViewComboBoxColumn comboColumn = new DataGridViewComboBoxColumn
            {
                Name = "tipo_plantacao",
                HeaderText = "Tipo de Plantação",
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
                HeaderText = "data Detecçao",
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


            // Configurando as propriedades dos cabeçalhos das colunas (titulos)
            foreach (DataGridViewColumn column in tabela_Doenca.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.ForeColor = Color.White; // Cor branca para o texto do título
                column.HeaderCell.Style.BackColor = Color.FromArgb(37, 213, 116); // Cor de fundo verde para os cabeçalhos
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
            using (SqlConnection connection = new SqlConnection("Server=MENDONÇA\\SQLEXPRESS;Database=fazenda_urbana_Urban_Green_pim4;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                try
                {
                    connection.Open();
                    // Altere a consulta para verificar a tabela e coluna corretas (Controle_pragas_doencas)
                    SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Controle_pragas_doencas WHERE cod_pragas_doencas = @cod_pragas_doencas", connection);
                    command.Parameters.AddWithValue("@cod_pragas_doencas", codPragasDoencas);
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



        private bool isEditingRow = false;


        private void btn_newLine_Doenca_Click_1(object sender, EventArgs e)
        {
            if (isEditingRow)
            {
                MessageBox.Show("Finalize a edição da linha atual antes de adicionar uma nova.");
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
            comboColumn.DisplayMember = "tipo_plantacao"; // O que será exibido no ComboBox
            comboColumn.ValueMember = "cod_plantacao"; // Valor interno vinculado ao campo
        }


        private bool ValidarLinhasPragasDoencas(DataRow linha)
        {
            // Verificar se os campos obrigatórios estão preenchidos
            if (linha["nome_comum"] == DBNull.Value || string.IsNullOrEmpty(linha["nome_comum"].ToString()) ||
                linha["nome_cientifico"] == DBNull.Value || string.IsNullOrEmpty(linha["nome_cientifico"].ToString()) ||
                linha["tipo"] == DBNull.Value || string.IsNullOrEmpty(linha["tipo"].ToString()) ||
                linha["data_deteccao"] == DBNull.Value ||
                linha["eficacia"] == DBNull.Value ||
                linha["severidade"] == DBNull.Value ||
                string.IsNullOrEmpty(linha["metodo_controle"].ToString()))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios relacionados à praga/doença.");
                return false; // Retorna falso se algum campo obrigatório não estiver preenchido
            }

            // Verificar se a eficácia está em um formato válido (número entre 0 e 100)
            if (Convert.ToDecimal(linha["eficacia"]) < 0 || Convert.ToDecimal(linha["eficacia"]) > 100)
            {
                MessageBox.Show("A eficácia deve ser um número entre 0 e 100.");
                return false;
            }



            // Verificar se a data de detecção é válida (não no futuro)
            if (Convert.ToDateTime(linha["data_deteccao"]) > DateTime.Now)
            {
                MessageBox.Show("A data de detecção não pode ser no futuro.");
                return false;
            }

            return true; // Retorna verdadeiro se todas as validações forem atendidas
        }

        private void btn_Adicionar_Click(object sender, EventArgs e)
        {
            // Validar se o DataTable possui alterações
            if (saudeTable.GetChanges() != null)
            {
                // Verificar as linhas antes de salvar
                foreach (DataRow linha in saudeTable.Rows)
                {
                    if (linha.RowState == DataRowState.Added || linha.RowState == DataRowState.Modified)
                    {
                        if (!ValidarLinhasPragasDoencas(linha))
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
                        adapter.Update(saudeTable); // Insere ou atualiza as linhas alteradas no banco

                        MessageBox.Show("Dados de pragas/doenças salvos com sucesso!");

                        // Confirmar as alterações no DataTable
                        saudeTable.AcceptChanges();

                        tabela_Doenca.Refresh(); // Atualizar exibição no DataGridView
                        AtualizarGraficos();
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



        private int MapearSeveridadeParaValor(string severidade)
        {
            switch (severidade)
            {
                case "Alta": return 70;
                case "Média": return 50;
                case "Baixa": return 30;
                default: return 0; // Valor padrão caso o texto não corresponda
            }
        }

        private Dictionary<string, double> CalcularPorcentagemPorEspecie()
        {
            var resultados = new Dictionary<string, double>();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                string query = @"
            SELECT 
                p.especie AS Especie,
                AVG(pd.eficacia) AS MediaEficacia,
                AVG(
                    CASE 
                        WHEN pd.severidade = 'Alta' THEN 70
                        WHEN pd.severidade = 'Média' THEN 50
                        WHEN pd.severidade = 'Baixa' THEN 30
                        ELSE 0
                    END
                ) AS MediaSeveridade
            FROM 
                Plantacao p
            LEFT JOIN 
                Controle_pragas_doencas pd ON p.cod_plantacao = pd.cod_plantacao
            WHERE 
                pd.eficacia IS NOT NULL AND pd.severidade IS NOT NULL
            GROUP BY 
                p.especie";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string especie = reader["Especie"].ToString();
                        double mediaEficacia = reader["MediaEficacia"] != DBNull.Value
                            ? Convert.ToDouble(reader["MediaEficacia"])
                            : 0;

                        double mediaSeveridade = reader["MediaSeveridade"] != DBNull.Value
                            ? Convert.ToDouble(reader["MediaSeveridade"])
                            : 0;

                        // Combinar eficácia e severidade em uma única porcentagem
                        double porcentagem = (mediaEficacia / (mediaEficacia + mediaSeveridade)) * 100;
                        resultados[especie] = porcentagem;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao calcular porcentagens por espécie: {ex.Message}");
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return resultados;
        }



        private void AtualizarGraficos()
        {
            var dadosEspecies = CalcularPorcentagemPorEspecie();
            especies = dadosEspecies.Keys.ToList(); // Atualiza a lista de espécies
            int totalEspecies = especies.Count;

            // Atualiza os gráficos com base no índice atual
            if (totalEspecies > 0 && indiceAtual < totalEspecies)
            {
                // Exibe as espécies 1, 2 e 3
                if (indiceAtual < totalEspecies)
                {
                    grafico1.Value = (int)dadosEspecies[especies[indiceAtual]]; // Primeira espécie
                    graficototal1.Text = $"{grafico1.Value}%";
                    titulografico1.Text = especies[indiceAtual]; // Atualiza o título do gráfico 1
                    PosicionarSaudeNaParteSuperior(titulografico1, caixagrafico1);
                }
                else
                {
                    grafico1.Value = 0;
                    graficototal1.Text = "0%";
                    titulografico1.Text = "Sem dados";
                    PosicionarSaudeNaParteSuperior(titulografico1, caixagrafico1); // Posiciona o título na parte superior de caixagrafico1
                }

                if (indiceAtual + 1 < totalEspecies)
                {
                    grafico2.Value = (int)dadosEspecies[especies[indiceAtual + 1]]; // Segunda espécie
                    graficototal2.Text = $"{grafico2.Value}%";
                    titulografico2.Text = especies[indiceAtual + 1]; // Atualiza o título do gráfico 2
                    PosicionarSaudeNaParteSuperior(titulografico2, caixagrafico2);
                }

                else
                {
                    grafico2.Value = 0;
                    graficototal2.Text = "0%";
                    titulografico2.Text = "Sem dados";
                    PosicionarSaudeNaParteSuperior(titulografico2, caixagrafico2); // Posiciona o título na parte superior de caixagrafico2
                }

                if (indiceAtual + 2 < totalEspecies)
                {
                    grafico3.Value = (int)dadosEspecies[especies[indiceAtual + 2]]; // Terceira espécie
                    graficototal03.Text = $"{grafico3.Value}%";
                    titulografico3.Text = especies[indiceAtual + 2]; // Atualiza o título do gráfico 3
                    PosicionarSaudeNaParteSuperior(titulografico3, caixagrafico3);
                }
                else
                {
                    grafico3.Value = 0;
                    graficototal03.Text = "0%";
                    // titulografico3.Text = "Sem dados";
                    PosicionarSaudeNaParteSuperior(titulografico3, caixagrafico3); // Posiciona o título na parte superior de caixagrafico3
                }
            }
        }


        private void PosicionarSaudeNaParteSuperior(Control txtControl, Control caixaControl)
        {
            // Define o deslocamento vertical para o topo (com um pequeno espaço de 10px)
            txtControl.Top = 10; // Deixe o texto bem no topo, ajustando conforme necessário

            // Centraliza horizontalmente na caixa
            txtControl.Left = (caixaControl.Width - txtControl.Width) / 2;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (indiceAtual + 3 < especies.Count) // Garante que não ultrapasse o número total de espécies
            {
                indiceAtual += 3;  // Move para o próximo conjunto de 3 espécies
                AtualizarGraficos();  // Atualiza os gráficos
            }

        }

        private void btn_Anterio_Click(object sender, EventArgs e)
        {
            if (indiceAtual > 0) // Garante que não vá para um índice negativo
            {
                indiceAtual -= 3;  // Move para o conjunto anterior de 3 espécies
                AtualizarGraficos();  // Atualiza os gráficos
            }
        }

        //dashboard
        private void button4_Click(object sender, EventArgs e)
        {

        }
        //cultivo
        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void btn_estoque_Click(object sender, EventArgs e)
        {

        }

        private void btn_monitoramento_Click(object sender, EventArgs e)
        {

        }

        private void btn_saude_Click(object sender, EventArgs e)
        {

        }

        private void btn_relatorio_Click(object sender, EventArgs e)
        {

        }

        private void BarraPesquisa_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
        


   
