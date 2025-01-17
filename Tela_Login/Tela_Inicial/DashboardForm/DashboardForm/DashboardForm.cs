using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Tela_Cultivo;
using tela_de_logins;
using Tela_Login;
using Tela_Login.tela_relatorio;
using Tela_Saude2;
using TheArtOfDevHtmlRenderer.Adapters;
using UrbanGreenProject;

namespace DashboardForm
{
    public partial class Dashboard : Form
    {
        private SqlConnection connection;
        private SqlDataAdapter adapter;

        private string nomeBanco = "fazenda_urbana_Urban_Green_pim4";
        private string connectionString;
        private string nomeUsuario;
        private int currentIndex = 0;
        private int indiceAtual = 0;  // Armazena o �ndice da esp�cie atual a ser exibida
        private EstoqueForm estoqueForm; // Vari�vel para armazenar a inst�ncia do EstoqueForm
        private List<Monitoramento> monitoramentos = new List<Monitoramento>();
        private string primeiroNome;

        private DataTable controle_aguaTable;
        private DataTable controle_luztable;
        public Dashboard(string primeiroNome)
        {
            nomeUsuario = primeiroNome;
            connectionString = $"Server=MENDON�A\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";
            InitializeComponent();
            ExibirNomeUsuario();
            CarregarPlantacao();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            ExibirMonitoramento(currentIndex);
            LoadControle_agua();
            ConfigureDataGridView();
            CarregarTiposParaComboBox();
            loadControleLuz();
            ConfigureDataGridView2();
            CarregarTiposParaComboBox2();
            AtualizarGraficos();



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
           l.temperatura, 
           SUM(e.quantidade) AS TotalEstoque, 
           e.lote -- Incluindo a coluna 'lote' da tabela Estoque
    FROM Plantacao p
    LEFT JOIN controle_Agua c ON p.cod_plantacao = c.cod_plantacao
    LEFT JOIN Controle_Luz l ON p.cod_plantacao = l.cod_plantacao
    
    LEFT JOIN Estoque e ON p.cod_plantacao = e.cod_plantacao -- Jun��o com a tabela Estoque
    GROUP BY p.cod_plantacao, p.especie, p.tipo_plantacao, p.data_plantio, p.data_prevista, p.saude_plantacao, l.temperatura, e.lote";

            ExecutarConsulta(query, reader =>
            {
                int codPlantacao = Convert.ToInt32(reader["cod_plantacao"]);
                string especie = reader["especie"] != DBNull.Value ? reader["especie"].ToString() : "Desconhecida";
                string tipoPlantacao = reader["tipo_plantacao"] != DBNull.Value ? reader["tipo_plantacao"].ToString() : "Desconhecido";
                DateTime dataPlantio = reader["data_plantio"] != DBNull.Value ? Convert.ToDateTime(reader["data_plantio"]) : DateTime.MinValue;
                DateTime dataPrevista = reader["data_prevista"] != DBNull.Value ? Convert.ToDateTime(reader["data_prevista"]) : DateTime.MinValue;
                string saude = reader["saude_plantacao"] != DBNull.Value ? reader["saude_plantacao"].ToString() : "Indefinido";

                // Corrigindo a convers�o para decimal
                double totalAguaGasta = reader["TotalAguaGasta"] != DBNull.Value
                    ? Convert.ToDouble(reader["TotalAguaGasta"])
                    : 0;

                double totalGastoLuz = reader["TotalGastoLuz"] != DBNull.Value
                    ? Convert.ToDouble(reader["TotalGastoLuz"])
                    : 0;

                int temperatura = reader["temperatura"] != DBNull.Value
                    ? Convert.ToInt32(reader["temperatura"])
                    : 0;

                string lote = reader["lote"] != DBNull.Value ? reader["lote"].ToString() : "Desconhecido"; // Obtendo o valor de lote

                double totalAguaDisponivel = ObterTotalAguaDisponivel(codPlantacao); // Mudando para decimal
                double porcentagemAguaGasta = totalAguaDisponivel > 0 ? (totalAguaGasta / totalAguaDisponivel) * 100 : 0;

                double totalLuzDisponivel = ObterTotalLuzDisponivel(codPlantacao); // Mudando para decimal
                double porcentagemLuzGasta = totalLuzDisponivel > 0 ? (totalGastoLuz / totalLuzDisponivel) * 100 : 0;

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
                    Temperatura = temperatura,
                    Lote = lote // Armazenando o valor de lote
                };

                monitoramentos.Add(monitoramento);
            });
        }

        // Altera��es na l�gica de consulta para garantir que tudo seja decimal

        private double ObterTotalLuzDisponivel(int codPlantacao)
        {
            // Exemplo de valor fixo retornado como decimal
            return 1000; // 'M' indica que � um decimal
        }

        private double ObterTotalAguaDisponivel(int codPlantacao)
        {
            // Exemplo de valor fixo retornado como decimal
            return 1000; // 'M' indica que � um decimal
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
                txtPorcetangemLUZ.Text = "Indefinido";
                txtTemperatura.Text = "Indefinido";
                txt_lote.Text = "Indefinido";

                CentralizarTexto(txtEspecie, caixaEspecie);
                CentralizarTexto(txtTipo, caixaTipo);
                CentralizarTexto(txtDataDePLantio, caixaPlantado);
                CentralizarTexto(txtPrevisao, caixaPrevisao);
                CentralizarTexto(txtPorcentagemAguaGasta, caixaAgua);
                CentralizarTexto(txtTemperatura, caixaTemperatura);
                CentralizarTexto(txtPorcetangemLUZ, caixaLuz);
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
                txtPorcetangemLUZ.Text = monitoramento.PorcentagemLuzGasta > 0 ? $"{monitoramento.PorcentagemLuzGasta:F2}%" : "Indefinido";
                txtTemperatura.Text = monitoramento.Temperatura > 0 ? $"{monitoramento.Temperatura:F2} �C" : "Indefinido";
                txt_lote.Text = monitoramento.Lote ?? "Indefinido";

                CentralizarTexto(txtEspecie, caixaEspecie);
                CentralizarTexto(txtTipo, caixaTipo);
                CentralizarTexto(txtDataDePLantio, caixaPlantado);

                CentralizarTexto(txtTemperatura, caixaTemperatura);

                CentralizarTexto(txt_lote, caixa_lote);
            }
        }

        private Dictionary<int, double> CalcularPorcentagemAguaPorPlantacao()
        {
            var resultados = new Dictionary<int, double>();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                string query = @"
            SELECT 
                p.cod_plantacao AS CodPlantacao,
                SUM(c.quantidade_agua) AS TotalAguaGasta
                
            FROM 
                Plantacao p
            LEFT JOIN 
                Controle_Agua c ON p.cod_plantacao = c.cod_plantacao
            GROUP BY 
                p.cod_plantacao";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int codPlantacao = Convert.ToInt32(reader["CodPlantacao"]);
                        double totalAguaGasta = reader["TotalAguaGasta"] != DBNull.Value
                            ? Convert.ToDouble(reader["TotalAguaGasta"])
                            : 0;

                        double totalAguaDisponivel = ObterTotalAguaDisponivel(codPlantacao);
                        double porcentagemAguaGasta = (totalAguaGasta / totalAguaDisponivel) * 100;

                        resultados[codPlantacao] = porcentagemAguaGasta;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao calcular porcentagens de �gua por planta��o: {ex.Message}");
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

        private Dictionary<int, double> CalcularPorcentagemLuzPorPlantacao()
        {
            var resultados = new Dictionary<int, double>();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                string query = @"
        SELECT 
            p.cod_plantacao AS CodPlantacao,
            COALESCE(SUM(l.intensidade_luz * DATEDIFF(MINUTE, l.hora_inicial, l.hora_final) / 60.0), 0) AS TotalGastoLuz
        FROM 
            Plantacao p
        LEFT JOIN 
            Controle_Luz l ON p.cod_plantacao = l.cod_plantacao
        GROUP BY 
            p.cod_plantacao";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int codPlantacao = Convert.ToInt32(reader["CodPlantacao"]);
                        double totalGastoLuz = Convert.ToDouble(reader["TotalGastoLuz"]);

                        // Obter o valor de luz dispon�vel para a planta��o
                        double totalLuzDisponivel = ObterTotalLuzDisponivel(codPlantacao); // Deve retornar a quantidade total de luz dispon�vel
                        double porcentagemLuzGasta = totalLuzDisponivel > 0 ? (totalGastoLuz / totalLuzDisponivel) * 100 : 0;

                        resultados[codPlantacao] = porcentagemLuzGasta;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao calcular porcentagens de luz por planta��o: {ex.Message}");
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
            var dadosAguaPorPlantacao = CalcularPorcentagemAguaPorPlantacao();
            var dadosLuzPorPlantacao = CalcularPorcentagemLuzPorPlantacao(); // Fun��o para calcular porcentagem de luz

            List<int> codPlantacoes = dadosAguaPorPlantacao.Keys.ToList(); // Lista de c�digos de planta��o
            int totalPlantacoes = codPlantacoes.Count;

            // Atualiza o gr�fico com base no �ndice atual para �gua
            if (totalPlantacoes > 0 && currentIndex < totalPlantacoes)
            {
                // Recupera a porcentagem de �gua para a planta��o no �ndice atual
                double porcentagemAgua = dadosAguaPorPlantacao[codPlantacoes[currentIndex]];

                // Atualiza o gr�fico com a porcentagem de �gua da planta��o atual
                graficoagua.Value = (int)porcentagemAgua; // Exibe a porcentagem no gr�fico
                txtPorcentagemAguaGasta.Text = $"{graficoagua.Value}%"; // Exibe a porcentagem no texto
            }
            else
            {
                graficoagua.Value = 0;
                txtPorcentagemAguaGasta.Text = "0";
            }

            // Atualiza o gr�fico com base no �ndice atual para luz
            if (totalPlantacoes > 0 && currentIndex < totalPlantacoes)
            {
                // Recupera a porcentagem de luz para a planta��o no �ndice atual
                double porcentagemLuz = dadosLuzPorPlantacao[codPlantacoes[currentIndex]];

                // Atualiza o gr�fico com a porcentagem de luz da planta��o atual
                graficoLuz.Value = (int)porcentagemLuz; // Exibe a porcentagem no gr�fico
                txtPorcetangemLUZ.Text = $"{graficoLuz.Value}%"; // Exibe a porcentagem no texto
            }
            else
            {
                graficoLuz.Value = 0;
                txtPorcetangemLUZ.Text = "0";
            }

            // Posiciona a sa�de na parte superior (se necess�rio)
            PosicionarSaudeNaParteSuperior(titulografico1, caixaAgua);
            PosicionarSaudeNaParteSuperior(titulografico2, caixaLuz); // Para o gr�fico de luz
        }







        private void btn_Anterio_Click_1(object sender, EventArgs e)
        {
            currentIndex = (currentIndex > 0) ? currentIndex - 1 : monitoramentos.Count - 1;
            ExibirMonitoramento(currentIndex);
            AtualizarGraficos();
        }

        private void PosicionarSaudeNaParteSuperior(Control txtControl, Control caixaControl)
        {
            // Define o deslocamento vertical para o topo (com um pequeno espa�o de 10px)
            txtControl.Top = 10; // Deixe o texto bem no topo, ajustando conforme necess�rio

            // Centraliza horizontalmente na caixa
            txtControl.Left = (caixaControl.Width - txtControl.Width) / 2;
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
                    caminhoImagem = Path.Combine(basePath, "verdinha.png");
                    break;
                case "Intermediario":
                    txtSaude.ForeColor = Color.Orange;
                    caminhoImagem = Path.Combine(basePath, "amarelo.png");
                    break;
                case "Perigo":
                    txtSaude.ForeColor = Color.Red;
                    caminhoImagem = Path.Combine(basePath, "vermelha.jpg");
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


        private void LoadControle_agua()
        {
            try
            {
                if (connection == null)
                {
                    string connectionString = $"Server=MENDON�A\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";
                    connection = new SqlConnection(connectionString);
                }

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                // Use o `adapter` global
                adapter = new SqlDataAdapter(
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
                AND a.quantidade_agua IS NOT NULL",
                    connection);

                // Configurar o InsertCommand
                SqlCommand insertAgua = new SqlCommand(
                    "INSERT INTO controle_agua (cod_controle, cod_plantacao, hora_inicial, hora_final, quantidade_agua) " +
                    "VALUES (@cod_controle, @cod_plantacao, @hora_inicial, @hora_final, @quantidade_agua)",
                    connection);
                insertAgua.Parameters.Add("@cod_controle", SqlDbType.Int, 0, "cod_controle");
                insertAgua.Parameters.Add("@cod_plantacao", SqlDbType.Int, 0, "cod_plantacao");
                insertAgua.Parameters.Add("@hora_inicial", SqlDbType.Time, 0, "hora_inicial");
                insertAgua.Parameters.Add("@hora_final", SqlDbType.Time, 0, "hora_final");
                insertAgua.Parameters.Add("@quantidade_agua", SqlDbType.Int, 0, "quantidade_agua");
                adapter.InsertCommand = insertAgua;

                // Configurar o UpdateCommand
                SqlCommand updateCommand = new SqlCommand(
                    "UPDATE Controle_agua " +
                    "SET cod_plantacao = @cod_plantacao, hora_inicial = @hora_inicial, hora_final = @hora_final, quantidade_agua = @quantidade_agua " +
                    "WHERE cod_controle = @cod_controle",
                    connection);
                updateCommand.Parameters.Add("@cod_plantacao", SqlDbType.Int, 0, "cod_plantacao");
                updateCommand.Parameters.Add("@hora_inicial", SqlDbType.Time, 0, "hora_inicial");
                updateCommand.Parameters.Add("@hora_final", SqlDbType.Time, 0, "hora_final");
                updateCommand.Parameters.Add("@quantidade_agua", SqlDbType.Int, 0, "quantidade_agua");
                updateCommand.Parameters.Add("@cod_controle", SqlDbType.Int, 0, "cod_controle");
                adapter.UpdateCommand = updateCommand;

                // Preencher o DataTable
                controle_aguaTable = new DataTable();
                adapter.Fill(controle_aguaTable);
                tabela_Agua.DataSource = controle_aguaTable;
                tabela_Agua.AllowUserToAddRows = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar dados: " + ex.Message);
            }
        }


        private void ConfigureDataGridView()
        {

            tabela_Agua.DataSource = controle_aguaTable;
            tabela_Agua.AutoGenerateColumns = false;
            tabela_Agua.Columns.Clear();


            tabela_Agua.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "cod_controle",
                HeaderText = "C�digo",
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


        private bool DoesCodeExist(int codControle)
        {
            using (SqlConnection connection = new SqlConnection("Server=MENDON�A\\SQLEXPRESS;Database=fazenda_urbana_Urban_Green_pim4;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                try
                {
                    connection.Open();
                    // Altere a consulta para verificar a tabela e coluna corretas (Controle_pragas_doencas)
                    SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Controle_Agua WHERE cod_controle = @cod_controle", connection);
                    command.Parameters.AddWithValue("@cod_controle", codControle);
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

        private void btn_addRow_Click_1(object sender, EventArgs e)
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
        private void RecarregarDados()
        {
            try
            {
                // Limpa o DataTable para evitar duplica��o de dados
                controle_aguaTable.Clear();

                // Preenche novamente o DataTable com os dados atualizados do banco
                adapter.Fill(controle_aguaTable);

                // Atualiza os dados na tabela
                tabela_Agua.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao recarregar dados: " + ex.Message);
            }
        }



        private void loadControleLuz()
        {
            try
            {
                // Verificar a conex�o
                if (connection == null)
                {
                    string connectionString = $"Server=MENDON�A\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";
                    connection = new SqlConnection(connectionString);
                }

                // Abrir a conex�o se necess�rio
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                // Definir o SqlDataAdapter e a consulta
                SqlDataAdapter adapter = new SqlDataAdapter(
                    @"SELECT 
        p.cod_plantacao,
        p.tipo_plantacao,
        l.cod_luz,
        l.hora_inicial,
        l.hora_final,
        l.duracao_luz,
        l.intensidade_luz,
        l.data_inicial,
        l.data_final,
        l.fonte_luz,
        l.tipo_luz,
        l.temperatura
    FROM
        plantacao p
    LEFT JOIN
        Controle_luz l ON p.cod_plantacao = l.cod_plantacao
   ", connection);



                // Definir o DataSource do DataGridView
                tabela_Luz.DataSource = controle_luztable;
                tabela_Luz.AllowUserToAddRows = false;  // Desativa a linha de inser��o autom�tica

                // Verificar se h� pelo menos uma linha e remover a primeira linha
                if (tabela_Luz.Rows.Count > 0)
                {
                    tabela_Luz.Rows.RemoveAt(0);
                }

                // Definir o comando de inser��o
                SqlCommand insertLuz = new SqlCommand(
                    @"INSERT INTO Controle_Luz 
    (cod_luz, cod_plantacao, hora_inicial, hora_final, duracao_luz, intensidade_luz, data_inicial, data_final, fonte_luz, tipo_luz,temperatura) 
    VALUES 
    (@CodLuz, @CodPlantacao, @HoraInicial, @HoraFinal, @Duracao_luz, @Intensidade_luz, @DataInicial, @DataFinal, @FonteLuz, @TipoLuz, @temperatura)", connection);

                // Adicionar os par�metros do comando de inser��o
                insertLuz.Parameters.Add("@CodLuz", SqlDbType.Int);
                insertLuz.Parameters.Add("@CodPlantacao", SqlDbType.Int);
                insertLuz.Parameters.Add("@HoraInicial", SqlDbType.Time);
                insertLuz.Parameters.Add("@HoraFinal", SqlDbType.Time);
                insertLuz.Parameters.Add("@Duracao_luz", SqlDbType.Time);
                insertLuz.Parameters.Add("@Intensidade_luz", SqlDbType.Decimal);
                insertLuz.Parameters.Add("@DataInicial", SqlDbType.DateTime);
                insertLuz.Parameters.Add("@DataFinal", SqlDbType.DateTime);
                insertLuz.Parameters.Add("@FonteLuz", SqlDbType.NVarChar);
                insertLuz.Parameters.Add("@TipoLuz", SqlDbType.NVarChar);
                insertLuz.Parameters.Add("@temperatura", SqlDbType.Int);

                // Definir o comando de atualiza��o
                SqlCommand updateLuz = new SqlCommand(
                    @"UPDATE Controle_Luz
    SET 
        cod_plantacao = @CodPlantacao,
        hora_inicial = @HoraInicial,
        hora_final = @HoraFinal,
        duracao = @Duracao_luz,
        intensidade_luz = @Intensidade_luz,
        data_inicial = @DataInicial,
        data_final = @DataFinal,
        fonte_luz = @FonteLuz,
        tipo_luz = @TipoLuz
        temperatura = @temperatura,
    WHERE 
        cod_luz = @CodLuz", connection);

                // Adicionar os par�metros do comando de atualiza��o
                updateLuz.Parameters.Add("@CodLuz", SqlDbType.Int);
                updateLuz.Parameters.Add("@CodPlantacao", SqlDbType.Int);
                updateLuz.Parameters.Add("@HoraInicial", SqlDbType.Time);
                updateLuz.Parameters.Add("@HoraFinal", SqlDbType.Time);
                updateLuz.Parameters.Add("@Duracao_luz", SqlDbType.Time);
                updateLuz.Parameters.Add("@Intensidade_luz", SqlDbType.Decimal);
                updateLuz.Parameters.Add("@DataInicial", SqlDbType.DateTime);
                updateLuz.Parameters.Add("@DataFinal", SqlDbType.DateTime);
                updateLuz.Parameters.Add("@FonteLuz", SqlDbType.NVarChar);
                updateLuz.Parameters.Add("@TipoLuz", SqlDbType.NVarChar);
                updateLuz.Parameters.Add("@temperatura", SqlDbType.Int);
                // Iterar pelas linhas para atualizar ou inserir dados
                foreach (DataGridViewRow row in tabela_Luz.Rows)
                {
                    if (row.IsNewRow) continue;  // Ignora a linha de inser��o autom�tica do DataGridView

                    // Preencher os par�metros com os valores da linha
                    int cod_Luz = Convert.ToInt32(row.Cells["cod_luz"].Value);
                    int codPlant = Convert.ToInt32(row.Cells["cod_plantacao"].Value);
                    int intensidade = row.Cells["intensidade_luz"].Value != DBNull.Value ? Convert.ToInt32(row.Cells["intensidade_luz"].Value) : 0;
                    int temperatura = row.Cells["temperatura"].Value != DBNull.Value ? Convert.ToInt32(row.Cells["temperatura"].Value) : 0;
                    // Verifica��o de DBNull para os campos TimeSpan
                    TimeSpan? horaInicial = row.Cells["hora_inicial"].Value != DBNull.Value ? (TimeSpan?)row.Cells["hora_inicial"].Value : null;
                    TimeSpan? horaFinal = row.Cells["hora_final"].Value != DBNull.Value ? (TimeSpan?)row.Cells["hora_final"].Value : null;
                    TimeSpan? duracao = row.Cells["duracao_luz"].Value != DBNull.Value ? (TimeSpan?)row.Cells["duracao_luz"].Value : null;

                    // Verifica��o de DBNull para os campos DateTime
                    DateTime? dataInicial = row.Cells["data_inicial"].Value != DBNull.Value ? (DateTime?)row.Cells["data_inicial"].Value : null;
                    DateTime? dataFinal = row.Cells["data_final"].Value != DBNull.Value ? (DateTime?)row.Cells["data_final"].Value : null;

                    // Verifica��o de DBNull para os campos de texto
                    string fonteLuz = row.Cells["fonte_luz"].Value != DBNull.Value ? row.Cells["fonte_luz"].Value.ToString() : null;
                    string tipoLuz = row.Cells["tipo_luz"].Value != DBNull.Value ? row.Cells["tipo_luz"].Value.ToString() : null;

                    // Se o cod_luz for 0, faz uma inser��o
                    if (cod_Luz == 0)
                    {
                        insertLuz.Parameters["@cod_luz"].Value = DBNull.Value;  // Para inser��o, deixa cod_luz como NULL
                        insertLuz.Parameters["@cod_plantacao"].Value = codPlant;
                        insertLuz.Parameters["@hora_inicial"].Value = horaInicial ?? (object)DBNull.Value;
                        insertLuz.Parameters["@hora_final"].Value = horaFinal ?? (object)DBNull.Value;
                        insertLuz.Parameters["@duracao_luz"].Value = duracao ?? (object)DBNull.Value;
                        insertLuz.Parameters["@intensidade_luz"].Value = intensidade;
                        insertLuz.Parameters["@data_inicial"].Value = dataInicial ?? (object)DBNull.Value;
                        insertLuz.Parameters["@data_final"].Value = dataFinal ?? (object)DBNull.Value;
                        insertLuz.Parameters["@fonte_luz"].Value = fonteLuz ?? (object)DBNull.Value;
                        insertLuz.Parameters["@tipo_luz"].Value = tipoLuz ?? (object)DBNull.Value;
                        insertLuz.Parameters["@temperatura"].Value = temperatura;

                        // Executa a inser��o
                        insertLuz.ExecuteNonQuery();
                    }
                    else
                    {
                        // Se o cod_luz j� existe, faz a atualiza��o
                        updateLuz.Parameters["@cod_luz"].Value = cod_Luz;
                        updateLuz.Parameters["@cod_plantacao"].Value = codPlant;
                        updateLuz.Parameters["@hora_inicial"].Value = horaInicial ?? (object)DBNull.Value;
                        updateLuz.Parameters["@hora_final"].Value = horaFinal ?? (object)DBNull.Value;
                        updateLuz.Parameters["@duracao_luz"].Value = duracao ?? (object)DBNull.Value;
                        updateLuz.Parameters["@intensidade_luz"].Value = intensidade;
                        updateLuz.Parameters["@data_inicial"].Value = dataInicial ?? (object)DBNull.Value;
                        updateLuz.Parameters["@data_final"].Value = dataFinal ?? (object)DBNull.Value;
                        updateLuz.Parameters["@fonte_luz"].Value = fonteLuz ?? (object)DBNull.Value;
                        updateLuz.Parameters["@tipo_luz"].Value = tipoLuz ?? (object)DBNull.Value;
                        updateLuz.Parameters["@temperatura"].Value = temperatura;

                        // Executa a atualiza��o
                        updateLuz.ExecuteNonQuery();
                    }
                }

                // Atualizar o DataTable ap�s inser��o ou atualiza��o
                controle_luztable = new DataTable();
                controle_luztable.Clear();
                adapter.Fill(controle_luztable);
                tabela_Luz.Refresh();  // Atualiza o DataGridView para refletir as mudan�as

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


        private void ConfigureDataGridView2()
        {
            tabela_Luz.DataSource = controle_luztable;

            // Desabilita a gera��o autom�tica de colunas e limpa as colunas existentes
            tabela_Luz.AutoGenerateColumns = false;
            tabela_Luz.Columns.Clear();

            // Coluna: C�digo (ReadOnly)
            tabela_Luz.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "cod_Luz",
                HeaderText = "C�digo",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Coluna: Tipo de Planta��o (ComboBox)
            DataGridViewComboBoxColumn comboColumn1 = new DataGridViewComboBoxColumn
            {
                Name = "tipo_plantacao",
                HeaderText = "Tipo de Planta��o",
                DataPropertyName = "cod_plantacao", // Vinculado ao valor interno no DataTable
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },

            };

            tabela_Luz.Columns.Add(comboColumn1);

            // Coluna: Hora Inicial
            tabela_Luz.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "hora_inicial",
                HeaderText = "Hora Inicial",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "hh\\:mm", Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Coluna: Hora Final
            tabela_Luz.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "hora_final",
                HeaderText = "Hora Final",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "hh\\:mm", Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Coluna: Dura��o
            tabela_Luz.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "duracao_luz",
                HeaderText = "Dura��o",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "hh\\:mm", Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Coluna: Intensidade
            tabela_Luz.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "intensidade_luz",
                HeaderText = "Intensidade",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Coluna: Data Inicial
            tabela_Luz.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "data_inicial",
                HeaderText = "Data Inicial",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy", Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Coluna: Data Final
            tabela_Luz.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "data_final",
                HeaderText = "Data Final",
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy", Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Coluna: Fonte de Luz
            tabela_Luz.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "fonte_luz",
                HeaderText = "Fonte de Luz",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Coluna: Tipo de Luz
            tabela_Luz.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "tipo_luz",
                HeaderText = "Tipo de Luz",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            tabela_Luz.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "temperatura",
                HeaderText = "Temperatura",
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            foreach (DataGridViewColumn column in tabela_Luz.Columns)
            {
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                column.HeaderCell.Style.ForeColor = Color.White; // Cor branca para o texto do t�tulo
                column.HeaderCell.Style.BackColor = Color.FromArgb(37, 213, 116); // Cor de fundo verde para os cabe�alhos
            }

        }

        private void CarregarTiposParaComboBox2()
        {
            DataTable plantacaoTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT cod_plantacao, tipo_plantacao FROM Plantacao";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.Fill(plantacaoTable); // Preenche os dados da tabela com a consulta
            }



            // Configura o ComboBox para a coluna "tipo_plantacao" da tabela_Luz
            DataGridViewComboBoxColumn comboColumn1 = (DataGridViewComboBoxColumn)tabela_Luz.Columns["tipo_plantacao"];
            comboColumn1.DataSource = plantacaoTable;
            comboColumn1.DisplayMember = "tipo_plantacao"; // O que ser� exibido no ComboBox
            comboColumn1.ValueMember = "cod_plantacao";   // O valor real armazenado (cod_plantacao)
        }
        private int GetRandomControleLuz()
        {
            Random rad = new Random();
            int newCode;
            do
            {
                newCode = rad.Next(1, 999);
            } while (DoesCodeExist2(newCode));
            return newCode;
        }






        private bool DoesCodeExist2(int cod_luz)
        {
            using (SqlConnection connection = new SqlConnection("Server=MENDON�A\\SQLEXPRESS;Database=fazenda_urbana_Urban_Green_pim4;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                try
                {
                    connection.Open();
                    // Altere a consulta para verificar a tabela e coluna corretas (Controle_pragas_doencas)
                    SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Controle_Luz WHERE cod_luz= @cod_luz", connection);
                    command.Parameters.AddWithValue("@cod_luz", cod_luz);
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

        private void btn_linha_luz_Click(object sender, EventArgs e)
        {
            if (isEditingRow)
            {
                MessageBox.Show("Finalize a edi��o da linha atual antes de adicionar uma nova.");
                return;
            }

            // Gera um novo c�digo para a luz
            int NovoCodigo = GetRandomControleLuz();

            // Cria uma nova linha na tabela de controle de luz
            DataRow newRow = controle_luztable.NewRow();

            // Preenche os valores iniciais para a nova linha
            newRow["cod_luz"] = NovoCodigo;
            newRow["tipo_plantacao"] = DBNull.Value; // C�digo da planta��o n�o definido inicialmente
            newRow["hora_inicial"] = DBNull.Value; // Hora inicial n�o definida
            newRow["hora_final"] = DBNull.Value; // Hora final n�o definida
            newRow["duracao_luz"] = DBNull.Value; // Dura��o da luz n�o definida
            newRow["intensidade_luz"] = DBNull.Value; // Intensidade padr�o
            newRow["data_inicial"] = DBNull.Value; // Data inicial n�o definida
            newRow["data_final"] = DBNull.Value; // Data final n�o definida
            newRow["fonte_luz"] = DBNull.Value; // Fonte de luz n�o definida
            newRow["tipo_luz"] = DBNull.Value; // Tipo de luz n�o definido

            // Adiciona a nova linha � tabela
            controle_luztable.Rows.Add(newRow);

            // Atualiza a exibi��o da tabela para refletir a nova linha

        }


        private void inserirDadosLuz_Click(object sender, EventArgs e)
        {
            // Validar se o DataTable possui altera��es
            if (controle_luztable.GetChanges() != null)
            {
                // Verificar as linhas antes de salvar
                foreach (DataRow linha in controle_luztable.Rows)
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
                        SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Controle_luz", connection);

                        // Criar o SqlCommandBuilder para gerar os comandos INSERT/UPDATE/DELETE automaticamente
                        SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);

                        // Atualiza as linhas no banco de dados (inserindo ou atualizando conforme necess�rio)
                        adapter.Update(controle_luztable);  // 'controle_aguaTable' � o seu DataTable

                        // Exibe mensagem de sucesso
                        MessageBox.Show("Dados de controle de �gua salvos com sucesso!");

                        // Confirma as altera��es no DataTable
                        controle_luztable.AcceptChanges();

                        // Atualiza o DataGridView para refletir as mudan�as
                        tabela_Luz.Refresh();

                        // Aqui voc� pode adicionar a chamada para atualizar os gr�ficos, se necess�rio
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
                MessageBox.Show("Nenhuma altera��o para salvar.");
            }


        }


        private void btn_estoque_Click(object sender, EventArgs e)
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

        private void btn_saude_Click_1(object sender, EventArgs e)
        {
            telaSaude telassa = new telaSaude();
            this.Hide();
            telassa.Show();
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




        private bool isEditingRow = false;



        private void btn_Proximo_Click(object sender, EventArgs e)
        {
            currentIndex = (currentIndex < monitoramentos.Count - 1) ? currentIndex + 1 : 0;
            ExibirMonitoramento(currentIndex);
            AtualizarGraficos();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Caminho do arquivo PDF
                string caminhoPdf = @"C:\Users\joaok\OneDrive\Downloads\Manual do Software.pdf";

                // Verifica se o arquivo existe
                if (File.Exists(caminhoPdf))
                {
                    // Cria um novo processo para abrir o PDF no visualizador padr�o do sistema
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = caminhoPdf,
                        UseShellExecute = true // Usar o shell do sistema para abrir o arquivo com o programa padr�o
                    };
                    Process.Start(startInfo);
                }
                else
                {
                    MessageBox.Show("O arquivo PDF n�o foi encontrado.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao tentar abrir o arquivo PDF: " + ex.Message);
            }

        }

        private void tela_saida_Click_1(object sender, EventArgs e)
        {
            // Exibir a tela de login
            Form1 telaLogin = new Form1();
            this.Hide();
            telaLogin.Show();
        }

        private void txtPorcetangemLUZ_Click(object sender, EventArgs e)
        {

        }

        private void salvar_Click(object sender, EventArgs e)
        {
            // Validar se o DataTable possui altera��es
            if (controle_aguaTable.GetChanges() != null)
            {
                // Verificar as linhas antes de salvar
                foreach (DataRow linha in controle_aguaTable.Rows)
                {
                    if (linha.RowState == DataRowState.Added || linha.RowState == DataRowState.Modified)
                    {
                        if (!ValidarLinhaAgua(linha))
                        {
                            MessageBox.Show("Por favor, preenFha todos os campos obrigat�rios.");
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
                        adapter.Update(controle_aguaTable); // Insere ou atualiza as linhas alteradas no banco

                        MessageBox.Show("Dados de pragas/doen�as salvos com sucesso!");

                        // Confirmar as altera��es no DataTable
                        controle_aguaTable.AcceptChanges();

                        tabela_Agua.Refresh(); // Atualizar exibi��o no DataGridView
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
                MessageBox.Show("Nenhuma altera��o para salvar.");
            }
        }

        private void btn_relatorio_Click_1(object sender, EventArgs e)
        {
            telaRelatorio telaRelatorio = new telaRelatorio();
            this.Hide();
            telaRelatorio.Show();
        }
    }

}






