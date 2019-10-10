using System;
using System.Windows;
using System.Windows.Controls;
using Google.Apis.Sheets.v4.Data;

namespace GameDevLogin
{
    /// <summary>
    /// Interaction logic for SheetController.xaml
    /// </summary>
    public partial class SheetController : Page
    {
        public SheetController()
        {
            InitializeComponent();
            this.SheetID.Text = SheetsAPI.info.SheetId;
        }

        private void CreateNewSheet(Object sender, RoutedEventArgs e)
        {
            //tell google drive to link a sheet
            var sheet = new Spreadsheet() { Properties = new SpreadsheetProperties() };
            sheet.Properties.Title = "GameDevLogin";
            var response = SheetsAPI.Service.Spreadsheets.Create(sheet).Execute();
            SheetsAPI.info.SheetId = response.SpreadsheetId;
            this.SheetID.Text = SheetsAPI.info.SheetId;
        }

        /// <summary>
        /// Link a spreadsheet to this program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinkSheet(Object sender, RoutedEventArgs e)
        {
            try
            {
                //link the sheet
                var sheetInfo = SheetsAPI.Service.Spreadsheets.Get(SheetID.Text).Execute();
                SheetsAPI.info.SheetId = this.SheetID.Text;
                MessageBox.Show("Sheet linked", "Success", MessageBoxButton.OK);
            }
            catch
            {
                // show a failure message
                MessageBox.Show("Sheet ID is not valid", "Invalid Input", MessageBoxButton.OK);
            }
            
        }
    }
}
