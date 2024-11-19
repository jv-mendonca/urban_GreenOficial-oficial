using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Tela_Cultivo;
using tela_de_logins;
using Tela_Saude2;
using UrbanGreenProject;

namespace DashboardForm
{
    public partial class Dashboard : Form
    {
        private string nomeBanco = "fazenda_urbana_Urban_Green_pim4";
        private string connectionString;
        private string nomeUsuario;
        private int currentIndex = 0;
        private EstoqueForm estoqueForm; // Variável para armazenar a instância do EstoqueForm
        private List<Monitoramento> monitoramentos = new List<Monitoramento>();

        public Dashboard(string primeiroNome)
        {
            nomeUsuario = primeiroNome;
            connectionString = $"Server=MENDONÇA\\SQLEXPRESS;Database={nomeBanco};Trusted_Connection=True;TrustServerCertificate=True;";
            InitializeComponent();
            ExibirNomeUsuario();

            CarregarPlantacao();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            ExibirMonitoramento(currentIndex);



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
                MessageBox.Show("Nome do usuário não fornecido.");
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
    LEFT JOIN Estoque e ON p.cod_plantacao = e.cod_plantacao -- Junção com a tabela Estoque
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
            // Aqui você deve implementar a lógica para obter o total de luz disponível da sua base de dados.
            // Para exemplo, vou retornar um valor fixo, mas você deve buscar esse valor na tabela correspondente.

            // Substitua isso pela lógica real
            return 1000; // Exemplo: 1000 unidades de luz disponíveis
        }







        private decimal ObterTotalAguaDisponivel(int codPlantacao)
        {
            // Aqui você deve implementar a lógica para obter o total de água disponível da sua base de dados.
            // Para exemplo, vou retornar um valor fixo, mas você deve buscar esse valor na tabela correspondente.

            // Substitua isso pela lógica real
            return 1000; // Exemplo: 1000 litros disponíveis
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
                // Se não houver monitoramentos, exibe valores padrão ou deixa as caixas vazias
                txtEspecie.Text = "Indisponível";
                txtTipo.Text = "Indisponível";
                txtDataDePLantio.Text = "Indisponível";
                txtPrevisao.Text = "Indisponível";
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
                txtEspecie.Text = monitoramento.Especie ?? "Indisponível";
                txtTipo.Text = monitoramento.TipoPlantacao ?? "Indisponível";
                txtDataDePLantio.Text = FormatarData(monitoramento.DataPlantio);
                txtPrevisao.Text = FormatarData(monitoramento.DataPrevista);
                txtSaude.Text = monitoramento.Saude;

                // Chama o método AtualizarSaude para atualizar a cor e imagem da saúde
                AtualizarSaude(monitoramento.Saude);

                txtPorcentagemAguaGasta.Text = monitoramento.PorcentagemAguaGasta > 0 ? $"{monitoramento.PorcentagemAguaGasta:F2}%" : "Indefinido";
                txtPorcetagemLuz.Text = monitoramento.PorcentagemLuzGasta > 0 ? $"{monitoramento.PorcentagemLuzGasta:F2}%" : "Indefinido";
                txtTemperatura.Text = monitoramento.Temperatura > 0 ? $"{monitoramento.Temperatura:F2} °C" : "Indefinido";
                txt_lote.Text = monitoramento.Lote ?? "Indefinido";

                CentralizarTexto(txtEspecie, caixaEspecie);
                CentralizarTexto(txtTipo, caixaTipo);
                CentralizarTexto(txtDataDePLantio, caixaPlantado);
                CentralizarTexto(txtPrevisao, caixaPrevisao);
                CentralizarTexto(txtPorcentagemAguaGasta, caixaAgua);
                CentralizarTexto(txtTemperatura, caixaTemperatura);
                CentralizarTexto(txtPorcetagemLuz, caixaLuz);
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

            // Caminho absoluto para as imagens de saúde
            string basePath = @"C:\UrbanGreenProject\Urban_Green_PROJECT\Tela_Login\Tela_Login\Tela_Inicial\DashboardForm\DashboardForm\Resources";
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
                default:
                    txtSaude.ForeColor = Color.Gray;
                    break;
            }

            if (caminhoImagem != null && File.Exists(caminhoImagem))
            {
                imagemFolha.Image?.Dispose(); // Libera a imagem anterior, se houver
                imagemFolha.Image = Image.FromFile(caminhoImagem);
                CentralizarImagemNaCaixa();
                PosicionarSaudeNaParteInferior();
            }
            else
            {
                imagemFolha.Image = null; // Remove a imagem se não houver uma correspondente
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



        private void btn_Proximo_Click(object sender, EventArgs e)
        {
            currentIndex = (currentIndex < monitoramentos.Count - 1) ? currentIndex + 1 : 0;
            ExibirMonitoramento(currentIndex);
        }


        private void btn_Anterio_Click(object sender, EventArgs e)
        {
            currentIndex = (currentIndex > 0) ? currentIndex - 1 : monitoramentos.Count - 1;
            ExibirMonitoramento(currentIndex);
        }

        private string FormatarData(DateTime data)
        {
            return data.ToString("dd/MM/yyyy");
        }








        // Verifica se existem monitoramentos disponíveis

        private void AtualizarDados(int index)
        {
            // Atualiza os dados nas caixas com base no índice atual
            var monitoramento = monitoramentos[index];

            txtEspecie.Text = monitoramento.Especie ?? "Indisponível"; // Supondo que isso não seja nulo
            txtTipo.Text = monitoramento.TipoPlantacao ?? "Indisponível";
            txtDataDePLantio.Text = monitoramento.DataPlantio.ToString("d");
            txtPrevisao.Text = monitoramento.Previsao.ToString("d");
            txtSaude.Text = monitoramento.Saude.ToString();
            txtPorcentagemAguaGasta.Text = monitoramento.PorcentagemAguaGasta.ToString("0.00%");
            txtPorcetagemLuz.Text = monitoramento.PorcentagemLuzGasta.ToString("0.00%");
            txtTemperatura.Text = monitoramento.Temperatura.ToString("0.00 °C");
        }

        private void ExibirIndisponivel()
        {
            txtEspecie.Text = "Indisponível";
            txtTipo.Text = "Indisponível";
            txtDataDePLantio.Text = "Indisponível";
            txtPrevisao.Text = "Indisponível";
            txtSaude.Text = "Indefinido";
            txtPorcentagemAguaGasta.Text = "0.00%";
            txtPorcetagemLuz.Text = "0.00%";
            txtTemperatura.Text = "0.00 °C";
        }






        private void button1_Click_1(object sender, EventArgs e)
        {
            EstoqueForm estoqueForm = new EstoqueForm();
            this.Hide(); // Opcional: oculta o formulário atual
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
            public DateTime Previsao { get; set; } // Adicionando a previsão

            // Propriedade do Lote
            public string Lote { get; set; } // Representa a quantidade ou identificador do lote relacionado

            // Construtor que gera valores aleatórios para umidade
            public Monitoramento()
            {
                UmidadeSolo = new Random().NextDouble() * 100; // Gera valor entre 0 e 100
            }
        }

        private void tela_saida_Click(object sender, EventArgs e)
        {

        }

        private void BarraPesquisa_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
