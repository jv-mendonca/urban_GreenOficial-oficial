using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Tela_Cultivo;
using tela_de_logins;
using Tela_Login;
using Tela_Saude2;
using TheArtOfDevHtmlRenderer.Adapters;
using UrbanGreenProject;

namespace DashboardForm
{
    public partial class Dashboard : Form
    {
        private SqlConnection connection;
        private string nomeBanco = "fazenda_urbana_Urban_Green_pim4";
        private string connectionString;
        private string nomeUsuario;
        private int currentIndex = 0;
        private EstoqueForm estoqueForm; // Vari�vel para armazenar a inst�ncia do EstoqueForm
        private List<Monitoramento> monitoramentos = new List<Monitoramento>();
        private string primeiroNome;
        private DataTable adapter;
        private DataTable controle_aguaTable;
        public Dashboard(string primeiroNome)
        {
            nomeUsuario = primeiroNome;
            connectionString = $"Server=MENDON�A\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";
            InitializeComponent();
            ExibirNomeUsuario();
            CarregarPlantacao();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            ExibirMonitoramento(currentIndex);
            loadControleAgua();
            ConfiguredataGridView();
            CarregarTiposParaComboBox();



        }

        private void ExibirNomeUsuario()
        {
            if (!string.IsNullOrEmpty(nomeUsuario))
            {
                labelPrimeiroNome.Text = nomeUsuario;
                labelPrimeiroNome.AutoSize = true;
                labelPrimeiroNome.ForeColor = Color.White;
                labelPrimeiroNome.BackColor = Color.Transparent;
                Cabecalho.Controls.Add(labelPrimeiroNome);
            }
            else
            {
                MessageBox.Show("Nome do usu�rio n�o fornecido.");
            }
        }





        private void CarregarPlantacao()
        {
            string query = @"
    SELECT p.cod_plantacao, p.especie, p.tipo_plantacao, p.data_plantio, p.data_prevista, p.saude_plantacao,
           SUM(c.quantidade_agua) AS TotalAguaGasta,
           COALESCE(SUM(l.intensidade_luz), 0) AS TotalGastoLuz,
           t.temperatura, 
           SUM(e.quantidade) AS TotalEstoque, 
           e.lote -- Incluindo a coluna 'lote' da tabela Estoque
    FROM Plantacao p
    LEFT JOIN controle_Agua c ON p.cod_plantacao = c.cod_plantacao
    LEFT JOIN Controle_Luz l ON p.cod_plantacao = l.cod_plantacao
    LEFT JOIN controle_temperatura t ON p.cod_plantacao = t.cod_plantacao
    LEFT JOIN Estoque e ON p.cod_plantacao = e.cod_plantacao -- Jun��o com a tabela Estoque
    GROUP BY p.cod_plantacao, p.especie, p.tipo_plantacao, p.data_plantio, p.data_prevista, p.saude_plantacao, t.temperatura, e.lote";

            ExecutarConsulta(query, reader =>
            {
                int codPlantacao = Convert.ToInt32(reader["cod_plantacao"]);
                string especie = reader["especie"] != DBNull.Value ? reader["especie"].ToString() : "Desconhecida";
                string tipoPlantacao = reader["tipo_plantacao"] != DBNull.Value ? reader["tipo_plantacao"].ToString() : "Desconhecido";
                DateTime dataPlantio = reader["data_plantio"] != DBNull.Value ? Convert.ToDateTime(reader["data_plantio"]) : DateTime.MinValue;
                DateTime dataPrevista = reader["data_prevista"] != DBNull.Value ? Convert.ToDateTime(reader["data_prevista"]) : DateTime.MinValue;
                string saude = reader["saude_plantacao"] != DBNull.Value ? reader["saude_plantacao"].ToString() : "Indefinido";

                decimal totalAguaGasta = reader["TotalAguaGasta"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("TotalAguaGasta")) : 0;
                decimal totalGastoLuz = reader["TotalGastoLuz"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("TotalGastoLuz")) : 0;
                decimal temperatura = reader["temperatura"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("temperatura")) : 0;


                string lote = reader["lote"] != DBNull.Value ? reader["lote"].ToString() : "Desconhecido"; // Obtendo o valor de lote

                decimal totalAguaDisponivel = ObterTotalAguaDisponivel(codPlantacao);
                decimal porcentagemAguaGasta = totalAguaDisponivel > 0 ? (totalAguaGasta / totalAguaDisponivel) * 100 : 0;

                decimal totalLuzDisponivel = ObterTotalLuzDisponivel(codPlantacao);
                decimal porcentagemLuzGasta = totalLuzDisponivel > 0 ? (totalGastoLuz / totalLuzDisponivel) * 100 : 0;

                porcentagemAguaGasta = Math.Min(porcentagemAguaGasta, 100);
                porcentagemLuzGasta = Math.Min(porcentagemLuzGasta, 100);



                Monitoramento monitoramento = new Monitoramento
                {
                    CodPlantacao = codPlantacao,
                    Especie = especie,
                    TipoPlantacao = tipoPlantacao,
                    DataPlantio = dataPlantio,
                    DataPrevista = dataPrevista,
                    Saude = saude,
                    PorcentagemAguaGasta = Math.Round(porcentagemAguaGasta, 2),
                    PorcentagemLuzGasta = Math.Round(porcentagemLuzGasta, 2),
                    TotalGastoLuz = Math.Round(totalGastoLuz, 2),
                    Temperatura = Math.Round(temperatura, 1),
                    Lote = lote // Armazenando o valor de lote
                };

                monitoramentos.Add(monitoramento);
            });
        }




        private decimal ObterTotalLuzDisponivel(int codPlantacao)
        {
            // Aqui voc� deve implementar a l�gica para obter o total de luz dispon�vel da sua base de dados.
            // Para exemplo, vou retornar um valor fixo, mas voc� deve buscar esse valor na tabela correspondente.

            // Substitua isso pela l�gica real
            return 1000; // Exemplo: 1000 unidades de luz dispon�veis
        }







        private decimal ObterTotalAguaDisponivel(int codPlantacao)
        {
            // Aqui voc� deve implementar a l�gica para obter o total de �gua dispon�vel da sua base de dados.
            // Para exemplo, vou retornar um valor fixo, mas voc� deve buscar esse valor na tabela correspondente.

            // Substitua isso pela l�gica real
            return 1000; // Exemplo: 1000 litros dispon�veis
        }



        private void ExecutarConsulta(string query, Action<SqlDataReader> action)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, con);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            action(reader);
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show("Erro ao acessar o banco de dados: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }







        private void ExibirMonitoramento(int index)
        {
            if (monitoramentos.Count == 0)
            {
                // Se n�o houver monitoramentos, exibe valores padr�o ou deixa as caixas vazias
                txtEspecie.Text = "Indispon�vel";
                txtTipo.Text = "Indispon�vel";
                txtDataDePLantio.Text = "Indispon�vel";
                txtPrevisao.Text = "Indispon�vel";
                txtSaude.Text = "Indefinido";
                txtPorcentagemAguaGasta.Text = "Indefinido";
                txtPorcetagemLuz.Text = "Indefinido";
                txtTemperatura.Text = "Indefinido";
                txt_lote.Text = "Indefinido";

                CentralizarTexto(txtEspecie, caixaEspecie);
                CentralizarTexto(txtTipo, caixaTipo);
                CentralizarTexto(txtDataDePLantio, caixaPlantado);
                CentralizarTexto(txtPrevisao, caixaPrevisao);
                CentralizarTexto(txtPorcentagemAguaGasta, caixaAgua);
                CentralizarTexto(txtTemperatura, caixaTemperatura);
                CentralizarTexto(txtPorcetagemLuz, caixaLuz);
                CentralizarTexto(txt_lote, caixa_lote);
            }
            else if (index >= 0 && index < monitoramentos.Count)
            {
                var monitoramento = monitoramentos[index];
                txtEspecie.Text = monitoramento.Especie ?? "Indispon�vel";
                txtTipo.Text = monitoramento.TipoPlantacao ?? "Indispon�vel";
                txtDataDePLantio.Text = FormatarData(monitoramento.DataPlantio);
                txtPrevisao.Text = FormatarData(monitoramento.DataPrevista);
                txtSaude.Text = monitoramento.Saude;

                // Chama o m�todo AtualizarSaude para atualizar a cor e imagem da sa�de
                AtualizarSaude(monitoramento.Saude);

                txtPorcentagemAguaGasta.Text = monitoramento.PorcentagemAguaGasta > 0 ? $"{monitoramento.PorcentagemAguaGasta:F2}%" : "Indefinido";
                txtPorcetagemLuz.Text = monitoramento.PorcentagemLuzGasta > 0 ? $"{monitoramento.PorcentagemLuzGasta:F2}%" : "Indefinido";
                txtTemperatura.Text = monitoramento.Temperatura > 0 ? $"{monitoramento.Temperatura:F2} �C" : "Indefinido";
                txt_lote.Text = monitoramento.Lote ?? "Indefinido";

                CentralizarTexto(txtEspecie, caixaEspecie);
                CentralizarTexto(txtTipo, caixaTipo);
                CentralizarTexto(txtDataDePLantio, caixaPlantado);

                CentralizarTexto(txtTemperatura, caixaTemperatura);

                CentralizarTexto(txt_lote, caixa_lote);
            }
        }









        private void CentralizarTexto(Control txtControl, Control caixaControl)
        {
            txtControl.Location = new Point(
                (caixaControl.Width - txtControl.Width) / 2,
                (caixaControl.Height - txtControl.Height) / 2
            );
        }

        private void AtualizarSaude(string saude)
        {
            txtSaude.Text = saude;

            // Caminho absoluto para as imagens de sa�de
            string basePath = @"C:\Users\joaok\OneDrive\cmder\�rea de Trabalho\urban_GreenOficial-Oficial\urban_GreenOficial-Oficial\urban_GreenOficial-main\Tela_Login\Tela_Inicial\DashboardForm\DashboardForm\Resources";
            string caminhoImagem = null;

            switch (saude)
            {
                case "Saudavel":
                    txtSaude.ForeColor = Color.Green;
                    caminhoImagem = Path.Combine(basePath, "folha-verde.png");
                    break;
                case "Intermediario":
                    txtSaude.ForeColor = Color.Orange;
                    caminhoImagem = Path.Combine(basePath, "folha_amarela.png");
                    break;
                case "Perigo":
                    txtSaude.ForeColor = Color.Red;
                    caminhoImagem = Path.Combine(basePath, "folha_vermelha.jpg");
                    break;

            }

            if (caminhoImagem != null && File.Exists(caminhoImagem))
            {
                imagemFolha.Image?.Dispose(); // Libera a imagem anterior, se houver
                imagemFolha.Image = Image.FromFile(caminhoImagem);
                CentralizarImagemNaCaixa();
                PosicionarSaudeNaParteInferior();
            }



        }


        private void CentralizarImagemNaCaixa()
        {
            if (imagemFolha.Image != null)
            {
                imagemFolha.SizeMode = PictureBoxSizeMode.CenterImage;
                imagemFolha.Location = new Point(
                    (caixaSaude.Width - imagemFolha.Width) / 2,
                    (caixaSaude.Height - imagemFolha.Height) / 2
                );
            }
        }

        private void PosicionarSaudeNaParteInferior()
        {
            txtSaude.Top = caixaSaude.Height - txtSaude.Height - 10; // Ajusta o deslocamento para manter o texto dentro da caixa
            txtSaude.Left = (caixaSaude.Width - txtSaude.Width) / 2; // Centraliza horizontalmente na caixa
        }







        private string FormatarData(DateTime data)
        {
            return data.ToString("dd/MM/yyyy");
        }


        private void loadControleAgua()
        {
            try
            {
                // Verifica se a conex�o est� inicializada
                if (connection == null)
                {
                    // Cria a inst�ncia da conex�o
                    string connectionString = $"Server=MENDON�A\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";
                    connection = new SqlConnection(connectionString);
                }

                // Verifica se a conex�o est� aberta, se n�o, abre a conex�o
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                // Consulta SQL corrigida
                SqlDataAdapter adapter = new SqlDataAdapter(
                    @"SELECT 
        p.cod_plantacao,
        p.tipo_plantacao,
        a.cod_controle,
        a.hora_inicial,
        a.hora_final,
        a.quantidade_agua
    FROM 
        Plantacao p
    LEFT JOIN 
        Controle_agua a ON p.cod_plantacao = a.cod_plantacao
    WHERE 
        p.cod_plantacao IS NOT NULL 
        AND a.hora_inicial IS NOT NULL 
        AND a.hora_final IS NOT NULL 
        AND a.quantidade_agua IS NOT NULL", connection);

                // Cria��o do DataTable para armazenar os dados
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                // Atribuindo os dados ao DataGridView
                tabela_Agua.DataSource = dataTable;

                // Comando para inser��o de dados na tabela Controle_agua
                SqlCommand insertAgua = new SqlCommand(
                    "INSERT INTO controle_agua (cod_controle, cod_plantacao, hora_inicial, hora_final, quantidade_agua) " +
                    "VALUES (@cod_controle, @cod_plantacao, @hora_inicial, @hora_final, @quantidade_agua)", connection);

                // Adicionando par�metros ao comando de inser��o
                insertAgua.Parameters.Add("@cod_controle", SqlDbType.Int);
                insertAgua.Parameters.Add("@cod_plantacao", SqlDbType.Int);
                insertAgua.Parameters.Add("@hora_inicial", SqlDbType.Time);
                insertAgua.Parameters.Add("@hora_final", SqlDbType.Time);
                insertAgua.Parameters.Add("@quantidade_agua", SqlDbType.Int);

                // Comando para atualiza��o de dados na tabela Controle_agua
                SqlCommand updateCommand = new SqlCommand(
                    "UPDATE Controle_agua " +
                    "SET cod_plantacao = @cod_plantacao, hora_inicial = @hora_inicial, hora_final = @hora_final, quantidade_agua = @quantidade_agua " +
                    "WHERE cod_controle = @cod_controle", connection);

                // Adiciona os par�metros ao comando de atualiza��o
                updateCommand.Parameters.AddWithValue("@cod_plantacao", SqlDbType.Int);
                updateCommand.Parameters.AddWithValue("@hora_inicial", SqlDbType.Time);
                updateCommand.Parameters.AddWithValue("@hora_final", SqlDbType.Time);
                updateCommand.Parameters.AddWithValue("@quantidade_agua", SqlDbType.Int);
                updateCommand.Parameters.AddWithValue("@cod_controle", SqlDbType.Int);

                // Percorrer as linhas do DataGridView e fazer a inser��o ou atualiza��o conforme necess�rio
                foreach (DataGridViewRow row in tabela_Agua.Rows)
                {
                    if (row.IsNewRow) continue;  // Ignorar a linha nova que o DataGridView adiciona automaticamente

                    // Preencher os par�metros com os valores da linha
                    int codCont = Convert.ToInt32(row.Cells["cod_controle"].Value);
                    int codPlant = Convert.ToInt32(row.Cells["cod_plantacao"].Value);  // Aqui � o cod_plantacao que ser� armazenado
                    TimeSpan horaInicial = (TimeSpan)row.Cells["hora_inicial"].Value;
                    TimeSpan horaFinal = (TimeSpan)row.Cells["hora_final"].Value;
                    int qtdAgua = Convert.ToInt32(row.Cells["quantidade_agua"].Value);

                    // Se o cod_controle for zero (novo), insira
                    if (codCont == 0)
                    {
                        insertAgua.Parameters["@cod_controle"].Value = codCont;
                        insertAgua.Parameters["@cod_plantacao"].Value = codPlant;  // Enviar o valor do cod_plantacao
                        insertAgua.Parameters["@hora_inicial"].Value = horaInicial;
                        insertAgua.Parameters["@hora_final"].Value = horaFinal;
                        insertAgua.Parameters["@quantidade_agua"].Value = qtdAgua;

                        // Executa a inser��o
                        insertAgua.ExecuteNonQuery();
                    }
                    else
                    {
                        // Se o cod_controle j� existir, fa�a a atualiza��o
                        updateCommand.Parameters["@cod_controle"].Value = codCont;
                        updateCommand.Parameters["@cod_plantacao"].Value = codPlant;  // Enviar o valor do cod_plantacao
                        updateCommand.Parameters["@hora_inicial"].Value = horaInicial;
                        updateCommand.Parameters["@hora_final"].Value = horaFinal;
                        updateCommand.Parameters["@quantidade_agua"].Value = qtdAgua;

                        // Executa a atualiza��o
                        updateCommand.ExecuteNonQuery();  // Execute o update para atualizar o banco
                    }
                }

                // Atualiza o DataTable ap�s inser��o ou atualiza��o
                controle_aguaTable = new DataTable();
                adapter.Fill(controle_aguaTable);
                tabela_Agua.AllowUserToAddRows = false;  // Desativa a op��o de adicionar novas linhas no DataGridView

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


        private int GetRandomControleAgua()
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





        private void ConfiguredataGridView()
        {
            // Atribui os dados ao DataGridView
            tabela_Agua.DataSource = controle_aguaTable;

            // Desabilita a gera��o autom�tica de colunas e limpa as colunas existentes
            tabela_Agua.AutoGenerateColumns = false;
            tabela_Agua.Columns.Clear();

            // Adiciona a coluna "cod_controle" como uma coluna de texto
            tabela_Agua.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "cod_controle",
                HeaderText = "C�digo",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Adiciona a coluna "tipo_plantacao" como uma coluna ComboBox
            DataGridViewComboBoxColumn comboColumn = new DataGridViewComboBoxColumn
            {
                Name = "tipo_plantacao",
                HeaderText = "Tipo de Planta��o",
                DataPropertyName = "cod_plantacao", // Vinculado ao valor interno no DataTable
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };

            // Adiciona o ComboBox na tabela
            tabela_Agua.Columns.Add(comboColumn);

            // Configura outras colunas conforme necess�rio, como hora_inicial, hora_final e quantidade_agua
            tabela_Agua.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "hora_inicial",
                HeaderText = "Hora Inicial",

                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            tabela_Agua.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "hora_final",
                HeaderText = "Hora Final",

                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            tabela_Agua.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "quantidade_agua",
                HeaderText = "Quantidade de �gua",

                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Configurando as propriedades dos cabe�alhos das colunas (titulos)
            foreach (DataGridViewColumn column in tabela_Agua.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.ForeColor = Color.White; // Cor branca para o texto do t�tulo
                column.HeaderCell.Style.BackColor = Color.FromArgb(37, 213, 116); // Cor de fundo verde para os cabe�alhos
            }
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
            DataGridViewComboBoxColumn comboColumn = (DataGridViewComboBoxColumn)tabela_Agua.Columns["tipo_plantacao"];
            comboColumn.DataSource = plantacaoTable;
            comboColumn.DisplayMember = "tipo_plantacao"; // O que ser� exibido no ComboBox
            comboColumn.ValueMember = "cod_plantacao"; // O valor real armazenado (cod_plantacao)
        }



        private void btn_addRow_Click(object sender, EventArgs e)
        {
            if (isEditingRow)
            {
                MessageBox.Show("Finalize a edi��o da linha atual antes de adicionar uma nova.");
                return;
            }

            int NovoCodigo = GetRandomControleAgua();

            // Cria uma nova linha
            DataRow newRow = controle_aguaTable.NewRow();
            newRow["cod_controle"] = NovoCodigo;
            newRow["tipo_plantacao"] = DBNull.Value;

            // Inicializa as colunas de hora com valores vazios
            newRow["hora_inicial"] = DBNull.Value;  // Mant�m a c�lula vazia para hora_inicial
            newRow["hora_final"] = DBNull.Value;    // Mant�m a c�lula vazia para hora_final

            newRow["quantidade_agua"] = DBNull.Value; // Se for num�rico, inicialize com 0

            // Adiciona a nova linha ao DataTable
            controle_aguaTable.Rows.Add(newRow);
        }



        private bool ValidarLinhaAgua(DataRow linha)
        {


            // Verificar se a hora inicial foi preenchida
            if (linha["hora_inicial"] == DBNull.Value || string.IsNullOrWhiteSpace(linha["hora_inicial"].ToString()))
            {
                MessageBox.Show("O campo 'Hora Inicial' n�o pode ser vazio.", "Erro de Valida��o", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Verificar se a hora final foi preenchida
            if (linha["hora_final"] == DBNull.Value || string.IsNullOrWhiteSpace(linha["hora_final"].ToString()))
            {
                MessageBox.Show("O campo 'Hora Final' n�o pode ser vazio.", "Erro de Valida��o", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Verificar se a quantidade de �gua foi preenchida corretamente
            if (linha["quantidade_agua"] == DBNull.Value || string.IsNullOrWhiteSpace(linha["quantidade_agua"].ToString()) || Convert.ToInt32(linha["quantidade_agua"]) <= 0)
            {
                MessageBox.Show("A 'Quantidade de �gua' deve ser um valor v�lido maior que 0.", "Erro de Valida��o", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Se tudo estiver preenchido corretamente
            return true;
        }


        private void salvar_Click(object sender, EventArgs e)
        {
            // Validar se o DataTable possui altera��es
            if (controle_aguaTable.GetChanges() != null)
            {
                // Verificar as linhas antes de salvar
                foreach (DataRow linha in controle_aguaTable.Rows)
                {
                    // Verifica se a linha foi adicionada ou modificada
                    if (linha.RowState == DataRowState.Added || linha.RowState == DataRowState.Modified)
                    {
                        if (!ValidarLinhaAgua(linha))
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

                        // Criar o SqlDataAdapter com a consulta SELECT
                        SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Controle_agua", connection);

                        // Criar o SqlCommandBuilder para gerar os comandos INSERT/UPDATE/DELETE automaticamente
                        SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);

                        // Atualiza as linhas no banco de dados (inserindo ou atualizando conforme necess�rio)
                        adapter.Update(controle_aguaTable);  // 'controle_aguaTable' � o seu DataTable

                        // Exibe mensagem de sucesso
                        MessageBox.Show("Dados de controle de �gua salvos com sucesso!");

                        // Confirma as altera��es no DataTable
                        controle_aguaTable.AcceptChanges();

                        // Atualiza o DataGridView para refletir as mudan�as
                        tabela_Agua.Refresh();

                        // Aqui voc� pode adicionar a chamada para atualizar os gr�ficos, se necess�rio
                        // AtualizarGraficos();
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







        private void button1_Click_1(object sender, EventArgs e)
        {
            EstoqueForm estoqueForm = new EstoqueForm();
            this.Hide(); // Opcional: oculta o formul�rio atual
            estoqueForm.Show();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            telaCultivo telaCultivo = new telaCultivo();
            this.Hide();
            telaCultivo.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            telaSaude telassaude = new telaSaude();
            this.Hide();
            telassaude.Show();
        }


        private void btn_monitoramento_Click(object sender, EventArgs e)
        {

        }

        private void btn_relatorio_Click(object sender, EventArgs e)
        {

        }






        public class Monitoramento
        {
            public int CodPlantacao { get; set; }
            public string Especie { get; set; }
            public string TipoPlantacao { get; set; }
            public DateTime DataPlantio { get; set; }
            public DateTime DataPrevista { get; set; }
            public string Saude { get; set; }
            public decimal PorcentagemAguaGasta { get; set; } // Propriedade existente
            public decimal PorcentagemLuzGasta { get; set; } // Nova propriedade para a porcentagem de luz
            public decimal TotalGastoLuz { get; set; } // Adicione esta linha
            public decimal Temperatura { get; set; } // Nova propriedade para a temperatura

            // Adicionando a propriedade de umidade do solo
            public double UmidadeSolo { get; set; }
            public DateTime Previsao { get; set; } // Adicionando a previs�o

            // Propriedade do Lote
            public string Lote { get; set; } // Representa a quantidade ou identificador do lote relacionado

            // Construtor que gera valores aleat�rios para umidade
            public Monitoramento()
            {
                UmidadeSolo = new Random().NextDouble() * 100; // Gera valor entre 0 e 100
            }
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

        private void btn_Proximo_Click_1(object sender, EventArgs e)
        {
            currentIndex = (currentIndex < monitoramentos.Count - 1) ? currentIndex + 1 : 0;
            ExibirMonitoramento(currentIndex);
        }

        private void btn_Anterio_Click(object sender, EventArgs e)
        {
            currentIndex = (currentIndex > 0) ? currentIndex - 1 : monitoramentos.Count - 1;
            ExibirMonitoramento(currentIndex);
        }


        private bool isEditingRow = false;

        private void tela_saida_Click(object sender, EventArgs e)
        {
            

                // Exibir a tela de login
                Form1 telaLogin = new Form1();
                this.Hide();
                telaLogin.Show();
            
        }
    }
}



  

