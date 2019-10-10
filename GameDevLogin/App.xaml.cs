using System.Windows;

namespace GameDevLogin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //initialize the required componets
            MagReaderHelper.Init();
            SheetsAPI.InitSheets();
        }
    }
}
