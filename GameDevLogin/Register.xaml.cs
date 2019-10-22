using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GameDevLogin
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Page
    {
        public Register()
        {
            InitializeComponent();
            // register the form to recieve magnetic strip reader info
            MagReaderHelper.IdRead += MagReaderHelper_IdRead;
        }

        /// <summary>
        /// Notify the register page that the mag strip reader has read a magstrim
        /// </summary>
        /// <param name="id">Id stored on the magnetic strip</param>
        private void MagReaderHelper_IdRead(Int64 id)
        {
            if (!this.IsVisible)
                return;
            IdInputField.Text = $"{id}";
            if(NameInputField.IsFocused)
            {
              
                var match = MagReaderHelper.IdRegex.Match(NameInputField.Text);
                if(match.Success)
                {
                    NameInputField.Text = NameInputField.Text.Remove(match.Index, match.Length);
                }
                
            }

        }

        /// <summary>
        /// register a new person base on the input fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubmitButton_Click(Object sender, RoutedEventArgs e)
        {
            //check the information inputed
            if(int.TryParse(IdInputField.Text, out var id) && !string.IsNullOrWhiteSpace(NameInputField.Text))
            {
                //try to register
                if(SheetsAPI.register(id, NameInputField.Text))
                {
                    //login
                    new Task(() => SheetsAPI.Login(id)).Start();
                    NameInputField.Text = "";
                    IdInputField.Text = "";
                    MessageBox.Show("User added successfully", "Login suc", MessageBoxButton.OK);
                }
            }
        }
    }
}
