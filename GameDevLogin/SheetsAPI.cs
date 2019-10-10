using Google.Apis.Sheets.v4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;
using System.ComponentModel;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using System.Runtime.Serialization.Formatters.Binary;
using Google.Apis.Sheets.v4.Data;

namespace GameDevLogin
{
    public class SheetsAPI
    {
        public static void InitSheets()
        {
            //Load the spread sheet linking data from a file
            try
            {
                using (var stream = File.OpenRead("SheetInfo.xml"))
                {
                    IFormatter formatter = new BinaryFormatter();
                    info = (SpreadSheetInfo)formatter.Deserialize(stream);
                }
                info.PropertyChanged += WriteSheetChanges;
            }
            catch
            {
                info = new SpreadSheetInfo();
                info.PropertyChanged += WriteSheetChanges;
                info.SheetId = "";
            }
        }

        /// <summary>
        /// Name of the application
        /// </summary>
        public const string ApplicationName = "Osu Game dev login";

        /// <summary>
        /// Scope of the appicaltion
        /// </summary>
        private static string[] Scopes = { SheetsService.Scope.Spreadsheets};

        /// <summary>
        /// Cached google sheets service object
        /// </summary>
        private static SheetsService cachedService;

        /// <summary>
        /// Google sheets service object
        /// </summary>
        public static SheetsService Service
        {
            get {
                if (cachedService != null)
                    return cachedService;

                UserCredential credential;
                using (var stream =
                    new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                cachedService = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
                return cachedService;
            }
        }
        
        /// <summary>
        /// Info about the spread sheet
        /// </summary>
        public static SpreadSheetInfo info;

        /// <summary>
        /// Write the spread sheet changes out to a file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static void WriteSheetChanges(object sender, PropertyChangedEventArgs args)
        {
            using (var stream = File.OpenWrite("SheetInfo.xml"))
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, info);
            }
        }

        /// <summary>
        /// Get all login tokens on the spread sheet
        /// </summary>
        /// <returns></returns>
        private static List<LoginToken> GetLoginTokens()
        {
            var spreadSheet = Service.Spreadsheets.Get(info.SheetId).Execute();
            string sheetTitle = spreadSheet.Sheets[0].Properties.Title;
            SpreadsheetsResource.ValuesResource.GetRequest request = Service.Spreadsheets.Values.Get(info.SheetId, $"{sheetTitle}!A2:C");
            var response = request.Execute();
            IList<IList<Object>> values = response.Values;
            List<LoginToken> logins = new List<LoginToken>();
            if (values == null)
                return logins;
            for (int i = 0; i < values.Count; i++)
            {
                try
                {
                    logins.Add(new LoginToken()
                    {
                        row = i + 2,
                        id = int.Parse(values[i][0] as string),
                        name = values[i][1] as string,
                        CreationDate = DateTime.Parse(values[i][2] as string)
                    });
                }
                catch { };
            }
            cachedTokens = logins;
            return logins;
        }

        /// <summary>
        /// Cached mapping of users ids to their login info
        /// </summary>
        private static List<LoginToken> cachedTokens = new List<LoginToken>();

        /// <summary>
        /// Get the login token for a user
        /// </summary>
        /// <param name="id">Id of the suer</param>
        /// <returns></returns>
        private static LoginToken GetLoginToken(int id)
        {
            return cachedTokens.FirstOrDefault(value => value.id == id) 
                ?? GetLoginTokens().FirstOrDefault(value => value.id == id);
        }

        /// <summary>
        /// Add an entry for the current time for a user
        /// </summary>
        /// <param name="id">Id of the user</param>
        /// <returns>username or error</returns>
        public static Errorable<string> Login(int id)
        {
            //get the users login token
            LoginToken token = GetLoginToken(id);
            if(token == null)
                return new Errorable<string>("Login not found", true);

            //get the title of the sheet
            var spreadSheet = Service.Spreadsheets.Get(info.SheetId).Execute();
            string sheetTitle = spreadSheet.Sheets[0].Properties.Title;

            //find the latest date
            var response = Service.Spreadsheets.Values.Get(info.SheetId, $"{sheetTitle}!1:1").Execute();
            int column = response.Values[0].Count;
            string columnName = IntToColumn(column-1);

            //udapte the cells
            response = Service.Spreadsheets.Values.Get(info.SheetId, $"{sheetTitle}!{columnName}1").Execute();
            if (!(response.Values.Count == 1 && response.Values[0].Count == 1))
                return new Errorable<string>("Error on getting sheet info", true);
            if(0 == string.Compare(response.Values[0][0] as string,DateTime.Now.ToString("yyyy/MM/dd"))
                || 0 == string.Compare(response.Values[0][0] as string, DateTime.Now.ToString("yyyy-MM-dd")))
            {
                updateValue("1", $"{sheetTitle}!{columnName}{token.row}");
            }
            else
            {
                columnName = IntToColumn(column);
                updateValue(DateTime.Now.ToString("yyyy/MM/dd"), $"{sheetTitle}!{columnName}1");
                updateValue("1", $"{sheetTitle}!{columnName}{token.row}");

            }
            return new Errorable<string>(token.name);
        }

        /// <summary>
        /// Update a value of the spread sheet 
        /// </summary>
        /// <param name="value">values to be set</param>
        /// <param name="range">place to setit</param>
        /// <returns></returns>
        public static BatchUpdateValuesResponse updateValue(string value, string range)
        {
            IList<IList<Object>> values = new List<IList<object>>
            {
                new List<object>()
            };
            values[0].Add(value);
            ValueRange body = new ValueRange() { Values = values };


            List<ValueRange> ranges = new List<ValueRange>();
            ranges.Add(new ValueRange() { Range = range, Values = values });

            BatchUpdateValuesRequest body2 = new BatchUpdateValuesRequest();
            body2.Data = ranges;
            body2.ValueInputOption = "USER_ENTERED";

            return Service.Spreadsheets.Values.BatchUpdate(body2, info.SheetId).Execute();
        }

        /// <summary>
        /// Register a user on the spread sheet
        /// </summary>
        /// <param name="id">Id of the user</param>
        /// <param name="username">Username</param>
        /// <returns>If the user was registered</returns>
        public static bool register(int id, string username)
        {
            //check that the user is not in the list
            List<LoginToken> tokens = GetLoginTokens();
            if (tokens.Any(item => item.id == id))
                return false;
            
            // get the new user row number
            int row = 2;
            if (tokens.Count != 0)
                row = tokens.Max(item => item.row) + 1;

            // get the title of the spread sheet
            var spreadSheet = Service.Spreadsheets.Get(info.SheetId).Execute();
            string sheetTitle = spreadSheet.Sheets[0].Properties.Title;

            // Formatt the user data
            IList<IList<object>> data = new List<IList<object>>();
            data.Add(new List<object>());
            data[0].Add($"{id:D9}");
            data[0].Add(username);
            data[0].Add(DateTime.Now.ToString("yyyy/MM/dd"));
            List<ValueRange> ranges = new List<ValueRange>();
            ranges.Add(new ValueRange() { Range = $"{sheetTitle}!A{row}:C{row}", Values=data});

            //Create a new request to change sheet cells
            BatchUpdateValuesRequest body2 = new BatchUpdateValuesRequest();
            body2.Data = ranges;
            body2.ValueInputOption = "USER_ENTERED";

            //submit the request to google sheets
            BatchUpdateValuesResponse result = Service.Spreadsheets.Values.BatchUpdate(body2, info.SheetId).Execute();
            return result.Responses.Count == 1;
        }

        /// <summary>
        /// Converts an integer index for a column into the google spreadsheets character index
        /// </summary>
        /// <param name="columnNumber">integer index starting with 0</param>
        /// <returns>character index starting with A</returns>
        private static string IntToColumn(int columnNumber)
        {
            string toReturn = "";
            while(columnNumber > 0)
            {
                columnNumber = Math.DivRem(columnNumber, 26, out var remainder);
                toReturn = (char)(remainder + (int)'A') + toReturn;
            }
            return toReturn;
        }

        /// <summary>
        /// Structure to encapsalte login data from the form
        /// </summary>
        private class LoginToken
        {
            public int row;
            public int id;
            public string name;
            public DateTime CreationDate;
        }

        /// <summary>
        /// Errerable value return
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class Errorable<T>
        {
            /// <summary>
            /// returned value
            /// </summary>
            public T value;

            /// <summary>
            /// Is the return errored
            /// </summary>
            public bool isErrored;

            /// <summary>
            /// Message for the error
            /// </summary>
            public string ErrorMessage;
            public Errorable(T val)
            {
                value = val;
                isErrored = false;
            }
            public Errorable(string message, bool isError = true)
            {
                isErrored = isError;
                ErrorMessage = message;
            }

        }

    }
}
