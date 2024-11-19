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

            // Passando um nome padr�o para o Dashboard. Voc� pode modificar isso conforme necess�rio.
            string nomeUsuario = "Usu�rio"; // Pode ser substitu�do por l�gica para obter um nome real
            Application.Run(new Dashboard(nomeUsuario)); // Passando o nome para o construtor do Dashboard
        }
    }
}
