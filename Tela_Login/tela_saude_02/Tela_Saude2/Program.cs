namespace Tela_Saude2
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Inicia o aplicativo, com o formul�rio 'telaSaude' como o principal
            Application.Run(new telaSaude());
        }
    }
}
