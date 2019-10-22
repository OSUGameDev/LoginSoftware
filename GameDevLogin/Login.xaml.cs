using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameDevLogin
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
            MagReaderHelper.IdRead += MagReaderHelper_IdRead;
        }

        private void SubmitButtonPressed(object sender, RoutedEventArgs e)
        {
            if(ManualIdField.Text.Length == 9 && Int64.TryParse(ManualIdField.Text, out var id))
            {
                MagReaderHelper_IdRead(id);
            }
        }

        private void MagReaderHelper_IdRead(Int64 id)
        {
            //check that this form is the active one
            if (!IsVisible)
                return;
            ResponseText.Content = "";
            //try to login
            var name = SheetsAPI.Login(id);
            
            //give the user feedback
            if(name.isErrored)
            {
                ResponseText.Content = "An Error Occured: " + name.ErrorMessage;
            }
            else
            {
                ResponseText.Content = "Welcome " + name.value;
            }
            ManualIdField.Text = "";


        }
    }
}
