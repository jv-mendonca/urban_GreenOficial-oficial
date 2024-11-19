using TelasDeCadastro;

namespace DashboardForm
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Passando um nome padrão para o Dashboard. Você pode modificar isso conforme necessário.
            string nomeUsuario = "Usuário"; // Pode ser substituído por lógica para obter um nome real
            Application.Run(new Dashboard(nomeUsuario)); // Passando o nome para o construtor do Dashboard
        }
    }
}
