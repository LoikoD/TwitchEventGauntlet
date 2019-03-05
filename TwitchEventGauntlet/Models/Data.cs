using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace TwitchEventGauntlet.Models
{
    public class Data
    {
        public string GameName { get; set; }
        private string _name;
        public string Name
        {   get { return _name; }
            set
            {
                _name = value;
                Range = Name + "!A:S";
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        Service.Spreadsheets.Values.Get(SpreadsheetId, Range);
                
                Response = request.Execute();
            }
        }
        public string SpreadsheetId { get; set; }
        public string Range { get; set; }
        public SheetsService Service { get; set; }
        public ValueRange Response { get; set; }
        public ValueRange updateRequestBody { get; set; }

        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Google Sheets API .NET Quickstart";

        public Data()
        {

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

            // Create Google Sheets API service.
            Service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define request parameters.
            SpreadsheetId = "1SFZfSn2gmD0jjsTWrsrx0UJDws8k6cBD4ydfKAL4L2M";
            Range = Name + "!A:S";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    Service.Spreadsheets.Values.Get(SpreadsheetId, Range);
            Response = request.Execute();


            


        }

        public List<string> GetGames()
        {
            IList<IList<object>> values = Response.Values;
            if (values != null && values.Count > 0)
            {
                List<string> Games = new List<string>();
                for (int i = 3; i < values.Count; ++i)
                {
                    var row = values[i];
                    if (row.Count > 4 && row[4] != null && !string.IsNullOrWhiteSpace(row[4].ToString()))
                    {
                        Games.Add(row[4].ToString());
                    }
                }
                return Games;
            }
            else
            {
                Console.WriteLine("No data found.");
                return null;
            }
        }

        public string GetSubGoal()
        {
            IList<IList<object>> values = Response.Values;
            if (values != null && values.Count > 6 )
            {
                if (values[6] != null && values[6].Count > 18)
                {
                    if (!string.IsNullOrWhiteSpace(values[6][18].ToString()))
                    {
                        return values[6][18].ToString();
                    }
                }
            }
            Console.WriteLine("No data found.");
            return null;
        }

        public string GetNumGames()
        {
            IList<IList<object>> values = Response.Values;
            string games;
            if (values != null && values.Count > 2)
            {
                games = values[2][1].ToString();
                string[] gamesSplit = games.Split('(', ')');
                if (gamesSplit.Count() > 1)
                {
                    return gamesSplit[1];
                }
                else
                {
                    string curSection = "";
                    for (int i = 3; i < values.Count(); ++i)
                    {
                        var row = values[i];
                        if (row.Count > 3 && row[3] != null && !string.IsNullOrWhiteSpace(row[3].ToString()))
                        {
                            curSection = row[3].ToString();
                            Console.WriteLine(curSection);
                        }
                    }

                    int sectionId = 1;
                    switch (curSection)
                    {
                        case "0-100":
                            sectionId = 1;
                            break;
                        case "100-200":
                            sectionId = 2;
                            break;
                        case "200-400":
                            sectionId = 3;
                            break;
                        case "400-550":
                            sectionId = 4;
                            break;
                        case "550-700":
                            sectionId = 5;
                            break;
                        case "700-1000":
                            sectionId = 6;
                            break;
                        case "1000-1500":
                            sectionId = 7;
                            break;
                        case "1500+":
                            sectionId = 8;
                            break;
                        default:
                            sectionId = 1;
                            break;

                    }

                    Console.WriteLine(sectionId);
                    int index = sectionId * 2 - 1;

                    Console.WriteLine(index);

                    List<string> sectionAndNums = gamesSplit[0].Split('x', ' ').ToList();

                    foreach (string element in sectionAndNums)
                    {
                        Console.WriteLine(element);
                    }
                    for (int i = 0; i < sectionAndNums.Count; ++i)
                    {
                        if (string.IsNullOrWhiteSpace(sectionAndNums[i]))
                        {
                            Console.WriteLine("Deleting " + sectionAndNums[i]);
                            sectionAndNums.RemoveAt(i);
                            --i;
                        }
                    }
                    foreach(string element in sectionAndNums)
                    {
                        Console.WriteLine(element);
                    }
                    if (sectionAndNums.Count > index)
                    {
                        return sectionAndNums[index];
                    }
                }
            }
            Console.WriteLine("No data found.");
            return null;
        }

        public string GetCurrentSection()
        {
            IList<IList<object>> values = Response.Values;
            if (values != null && values.Count > 0)
            {
                string curSection = "";
                for (int i = 3; i < values.Count(); ++i)
                {
                    var row = values[i];
                    if (row.Count > 3 && row[3] != null && !string.IsNullOrWhiteSpace(row[3].ToString()))
                    {
                        curSection = row[3].ToString();
                        Console.WriteLine(curSection);
                    }
                }
                return curSection;
            }
            Console.WriteLine("No data found.");
            return null;
        }

        public void SetSubsNum(int subs, int subsNeed)
        {
            var lobg = new List<object>() { (subs + "/" + subsNeed).ToString() };
            updateRequestBody = new ValueRange();
            updateRequestBody.Values = new List<IList<object>>() { lobg };

            SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = Service.Spreadsheets.Values.Update(updateRequestBody, SpreadsheetId, Name + "!S7");
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            // To execute asynchronously in an async method, replace `request.Execute()` as shown:
            UpdateValuesResponse updateResponse = updateRequest.Execute();
            // UpdateValuesResponse updateResponse = await updateRequest.ExecuteAsync();
        }

        public void AddGameToSection(string section)
        {
            SpreadsheetsResource.ValuesResource.GetRequest request =
                        Service.Spreadsheets.Values.Get(SpreadsheetId, Range);
            Response = request.Execute();
            IList<IList<object>> values = Response.Values;
            if (values != null && values.Count > 2)
            {
                Console.WriteLine(1);
                if (values[2] != null && values[2].Count > 1)
                {
                    Console.WriteLine(2);
                    string sections = values[2][1].ToString();

                    string splitter = section.Substring(1);
                    Console.WriteLine(section);
                    Console.WriteLine(sections);
                    Console.WriteLine(splitter);
                    splitter += "x";
                    Console.WriteLine(splitter);
                    Console.WriteLine("Sections before: " + sections);
                    string[] splittedBySection = sections.Split(new string[] { splitter }, StringSplitOptions.None);
                    if (splittedBySection.Count() > 1)
                    {
                        Console.WriteLine(3);

                        Console.WriteLine("SplittedBySection[1] before: " + splittedBySection[1]);
                        string[] splittedBySpaces = splittedBySection[1].Split(' ');

                        for (int i = 0; i < splittedBySpaces.Count(); ++i)
                        { Console.WriteLine("splittedBySpaces" + i + ": " + splittedBySpaces[i]); }
                        if (splittedBySpaces.Count() > 0)
                        {
                            Console.WriteLine(4);
                            if (splittedBySpaces[0].First() == '(')
                            {
                                Console.WriteLine(5);
                                Console.WriteLine("SplittedBySpaces[0] before: " + splittedBySpaces[0]);
                                string[] splittedByBrackets = splittedBySpaces[0].Split('(', '/', ')');
                                if (splittedByBrackets.Count() > 2)
                                {
                                    Console.WriteLine(6);
                                    int numGames = Convert.ToInt32(splittedByBrackets[2]);
                                    ++numGames;
                                    splittedByBrackets[2] = numGames.ToString();
                                    splittedBySpaces[0] = "(" + splittedByBrackets[1] + "/" + splittedByBrackets[2] + ")";
                                    Console.WriteLine("SplittedBySpaces[0] after: " + splittedBySpaces[0]);
                                }
                            }
                            else
                            {
                                Console.WriteLine(7);
                                int numGames = Convert.ToInt32(splittedBySpaces[0]);
                                ++numGames;
                                splittedBySpaces[0] = numGames.ToString();
                                Console.WriteLine("SplittedBySpaces[0] after: " + splittedBySpaces[0]);
                            }
                            splittedBySection[1] = splittedBySpaces[0];
                            if (splittedBySpaces.Count() > 1)
                            {
                                Console.WriteLine(9);
                                for (int i = 1; i < splittedBySpaces.Count(); ++i)
                                {
                                    splittedBySection[1] += " ";
                                    splittedBySection[1] += splittedBySpaces[i];
                                }
                            }
                            Console.WriteLine("SplittedBySection[1] after: " + splittedBySection[1]);
                        }
                        Console.WriteLine(10);
                        sections = splittedBySection[0] + splitter + splittedBySection[1];
                        Console.WriteLine("Sections after: " + sections);
                        var lobg = new List<object>() { sections };
                        updateRequestBody = new ValueRange();
                        updateRequestBody.Values = new List<IList<object>>() { lobg };
                        SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = Service.Spreadsheets.Values.Update(updateRequestBody, SpreadsheetId, Name + "!B3");
                        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                        UpdateValuesResponse updateResponse = updateRequest.Execute();
                    }
                }
            }
        }
    }
}
