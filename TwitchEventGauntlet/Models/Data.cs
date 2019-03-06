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
        private int _lastGameId;

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
        public int LastGameId
        {
            get { return _lastGameId; }
            set { _lastGameId = value; }
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
            SpreadsheetId = "1-at-FSQXIr9epC5bSd-0zDppJxYokceDhWE4jNTWP20";
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
                Console.WriteLine("Data.GetGames(): values.Count = " + values.Count);
                for (int i = 3; i < values.Count; ++i)
                {
                    var row = values[i];
                    if (row.Count > 4 && row[4] != null && !string.IsNullOrWhiteSpace(row[4].ToString()))
                    {
                        Games.Add(row[4].ToString());
                        LastGameId = i;
                    }
                }
                Sheet sh = Service.Spreadsheets.Get(SpreadsheetId).Execute().Sheets.Where(s => s.Properties.Title == Name).FirstOrDefault();
                GridRange merge = sh.Merges.Where(m => m.StartRowIndex == LastGameId && m.StartColumnIndex == 4).FirstOrDefault();
                int? index = merge?.EndRowIndex;
                if (index != null)
                {
                    LastGameId = index.GetValueOrDefault() - 1;
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

        public string GetNumGames(string section)
        {
            IList<IList<object>> values = Response.Values;
            string games;
            if (values != null && values.Count > 2)
            {
                games = values[2][1].ToString();
                int sectionId = 1;
                switch (section)
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
                int index = sectionId * 2 - 1;
                List<string> sectionAndNums = games.Split('x', ' ', 'х').ToList();
                for (int i = 0; i < sectionAndNums.Count; ++i)
                {
                    if (string.IsNullOrWhiteSpace(sectionAndNums[i]))
                    {
                        sectionAndNums.RemoveAt(i);
                        --i;
                    }
                }
                if (sectionAndNums.Count > index)
                {
                    if (sectionAndNums[index].First() == '(')
                    {
                        string[] gamesSplit = sectionAndNums[index].Split('(', ')');
                        if (gamesSplit.Count() > 1)
                        {
                            return gamesSplit[1];
                        }
                        else
                        {
                            throw new ArgumentException("Can't find number of games");
                        }
                    }
                    else
                    {
                        return sectionAndNums[index];
                    }
                }
            }
            throw new ArgumentException("Can't find number of games");
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
                    }
                }
                return curSection;
            }
            Console.WriteLine("No data found.");
            return null;
        }

        public async void SetSubsNum(int subs, int subsNeed)
        {
            await Task.Factory.StartNew(() =>
            {
                var lobg = new List<object>() { (subs + "/" + subsNeed).ToString() };
                updateRequestBody = new ValueRange();
                updateRequestBody.Values = new List<IList<object>>() { lobg };

                SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = Service.Spreadsheets.Values.Update(updateRequestBody, SpreadsheetId, Name + "!S7");
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                // To execute asynchronously in an async method, replace `request.Execute()` as shown:
                UpdateValuesResponse updateResponse = updateRequest.Execute();
                // UpdateValuesResponse updateResponse = await updateRequest.ExecuteAsync();
            });
        }

        public async void AddGameToSection(string section)
        {
            await Task.Factory.StartNew(() =>
            {
                SpreadsheetsResource.ValuesResource.GetRequest request = Service.Spreadsheets.Values.Get(SpreadsheetId, Range);
                Response = request.Execute();
                IList<IList<object>> values = Response.Values;
                if (values != null && values.Count > 2)
                {
                    if (values[2] != null && values[2].Count > 1)
                    {
                        string sections = values[2][1].ToString();

                        string splitter = section.Substring(1);
                        string splitterRus = splitter + "х";
                        splitter += "x";
                        string[] splittedBySection = sections.Split(new string[] { splitter, splitterRus }, StringSplitOptions.None);
                        if (splittedBySection.Count() > 1)
                        {
                            string[] splittedBySpaces = splittedBySection[1].Split(' ');
                            
                            if (splittedBySpaces.Count() > 0)
                            {
                                if (splittedBySpaces[0].First() == '(')
                                {
                                    string[] splittedByBrackets = splittedBySpaces[0].Split('(', '/', ')');
                                    if (splittedByBrackets.Count() > 2)
                                    {
                                        int numGames = Convert.ToInt32(splittedByBrackets[2]);
                                        ++numGames;
                                        splittedByBrackets[2] = numGames.ToString();
                                        splittedBySpaces[0] = "(" + splittedByBrackets[1] + "/" + splittedByBrackets[2] + ")";
                                    }
                                    else
                                    {
                                        throw new ArgumentException("Can't find information about sections");
                                    }
                                }
                                else
                                {
                                    int numGames = Convert.ToInt32(splittedBySpaces[0]);
                                    ++numGames;
                                    splittedBySpaces[0] = numGames.ToString();
                                }
                                splittedBySection[1] = splittedBySpaces[0];
                                if (splittedBySpaces.Count() > 1)
                                {
                                    for (int i = 1; i < splittedBySpaces.Count(); ++i)
                                    {
                                        splittedBySection[1] += " ";
                                        splittedBySection[1] += splittedBySpaces[i];
                                    }
                                }
                                else
                                {
                                    throw new ArgumentException("Can't find information about sections");
                                }
                            }
                            else
                            {
                                throw new ArgumentException("Can't find information about sections");
                            }
                            sections = splittedBySection[0] + splitter + splittedBySection[1];
                            var lobg = new List<object>() { sections };
                            updateRequestBody = new ValueRange();
                            updateRequestBody.Values = new List<IList<object>>() { lobg };
                            SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = Service.Spreadsheets.Values.Update(updateRequestBody, SpreadsheetId, Name + "!B3");
                            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                            UpdateValuesResponse updateResponse = updateRequest.Execute();
                        }
                        else
                        {
                            throw new ArgumentException("Can't find information about sections");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Can't find information about sections");
                    }
                }
                else
                {
                    throw new ArgumentException("Can't find information about sections");
                }
            });
        }

        public async void AddCompletedGameToSection(string section)
        {
            await Task.Factory.StartNew(() =>
            {
                SpreadsheetsResource.ValuesResource.GetRequest request = Service.Spreadsheets.Values.Get(SpreadsheetId, Range);
                Response = request.Execute();
                IList<IList<object>> values = Response.Values;
                if (values != null && values.Count > 2)
                {
                    if (values[2] != null && values[2].Count > 1)
                    {
                        string sections = values[2][1].ToString();

                        string splitter = section.Substring(1);
                        splitter += "x";
                        string[] splittedBySection = sections.Split(new string[] { splitter }, StringSplitOptions.None);
                        if (splittedBySection.Count() > 1)
                        {
                            string[] splittedBySpaces = splittedBySection[1].Split(' ');
                            
                            if (splittedBySpaces.Count() > 0)
                            {
                                if (splittedBySpaces[0].First() == '(')
                                {
                                    string[] splittedByBrackets = splittedBySpaces[0].Split('(', '/', ')');
                                    if (splittedByBrackets.Count() > 2)
                                    {
                                        int numGames = Convert.ToInt32(splittedByBrackets[1]);
                                        ++numGames;
                                        if (numGames == Convert.ToInt32(splittedByBrackets[2]))
                                        {
                                            splittedBySpaces[0] = numGames.ToString();
                                        }
                                        else
                                        {
                                            splittedByBrackets[1] = numGames.ToString();
                                            splittedBySpaces[0] = "(" + splittedByBrackets[1] + "/" + splittedByBrackets[2] + ")";
                                        }
                                    }
                                }
                                else
                                {
                                    int numGames = Convert.ToInt32(splittedBySpaces[0]);
                                    splittedBySpaces[0] = "(1/" + splittedBySpaces[0] + ")";
                                }
                                splittedBySection[1] = splittedBySpaces[0];
                                if (splittedBySpaces.Count() > 1)
                                {
                                    for (int i = 1; i < splittedBySpaces.Count(); ++i)
                                    {
                                        splittedBySection[1] += " ";
                                        splittedBySection[1] += splittedBySpaces[i];
                                    }
                                }
                            }
                            sections = splittedBySection[0] + splitter + splittedBySection[1];
                            var lobg = new List<object>() { sections };
                            updateRequestBody = new ValueRange();
                            updateRequestBody.Values = new List<IList<object>>() { lobg };
                            SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = Service.Spreadsheets.Values.Update(updateRequestBody, SpreadsheetId, Name + "!B3");
                            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                            UpdateValuesResponse updateResponse = updateRequest.Execute();
                        }
                    }
                }
            });
        }

        public async void RerollCurrent()
        {
            await Task.Factory.StartNew(() =>
            {
                int prevGameId = LastGameId;
                LastGameId++;

                //get sheet id by sheet name
                Spreadsheet spr = Service.Spreadsheets.Get(SpreadsheetId).Execute();
                Sheet sh = spr.Sheets.Where(s => s.Properties.Title == Name).FirstOrDefault();
                int sheetId = (int)sh.Properties.SheetId;

                //define cell background to light blue color
                var userEnteredFormat = new CellFormat()
                {
                    BackgroundColor = new Color()
                    {
                        Red = 0.427451f,
                        Green = 0.619608f,
                        Blue = 0.921569f,
                        Alpha = 1
                    }
                };
                BatchUpdateSpreadsheetRequest bussr = new BatchUpdateSpreadsheetRequest();
                
                //create the update request for cells from the first row
                var updateCellsRequest = new Request()
                {
                    RepeatCell = new RepeatCellRequest()
                    {
                        Range = new GridRange
                        {
                            SheetId = sheetId,
                            StartColumnIndex = 5,
                            StartRowIndex = prevGameId,
                            EndColumnIndex = 6,
                            EndRowIndex = prevGameId + 1
                        },
                        Cell = new CellData()
                        {
                            UserEnteredFormat = userEnteredFormat,
                            UserEnteredValue = new ExtendedValue()
                            {
                                StringValue = "Rerolled"
                            }
                        },
                        Fields = "UserEnteredFormat(BackgroundColor),UserEnteredValue(StringValue)"
                    }
                };



                bussr.Requests = new List<Request>();
                bussr.Requests.Add(updateCellsRequest);
                var bur = Service.Spreadsheets.BatchUpdate(bussr, SpreadsheetId);
                bur.Execute();
            });
        }

        public async void DropCurrent()
        {
            await Task.Factory.StartNew(() =>
            {
                int prevGameId = LastGameId;
                LastGameId++;

                //get sheet id by sheet name
                Spreadsheet spr = Service.Spreadsheets.Get(SpreadsheetId).Execute();
                Sheet sh = spr.Sheets.Where(s => s.Properties.Title == Name).FirstOrDefault();
                int sheetId = (int)sh.Properties.SheetId;

                //define cell background to light burgundy color
                var userEnteredFormat = new CellFormat()
                {
                    BackgroundColor = new Color()
                    {
                        Blue = 0.1451f,
                        Red = 0.8f,
                        Green = 0.254902f,
                        Alpha = 1
                    }
                };
                BatchUpdateSpreadsheetRequest bussr = new BatchUpdateSpreadsheetRequest();
                
                //create the update request for cells from the first row
                var updateCellsRequest = new Request()
                {
                    RepeatCell = new RepeatCellRequest()
                    {
                        Range = new GridRange
                        {
                            SheetId = sheetId,
                            StartColumnIndex = 5,
                            StartRowIndex = prevGameId,
                            EndColumnIndex = 6,
                            EndRowIndex = prevGameId + 1
                        },
                        Cell = new CellData()
                        {
                            UserEnteredFormat = userEnteredFormat,
                            UserEnteredValue = new ExtendedValue()
                            {
                                StringValue = "Dropped"
                            }
                        },
                        Fields = "UserEnteredFormat(BackgroundColor),UserEnteredValue(StringValue)"
                    }
                };

                bussr.Requests = new List<Request>();
                bussr.Requests.Add(updateCellsRequest);
                var bur = Service.Spreadsheets.BatchUpdate(bussr, SpreadsheetId);
                bur.Execute();
            });
        }

        public async void CompleteCurrent()
        {
            await Task.Factory.StartNew(() =>
            {
                int prevGameId = LastGameId;
                LastGameId++;

                //get sheet id by sheet name
                Spreadsheet spr = Service.Spreadsheets.Get(SpreadsheetId).Execute();
                Sheet sh = spr.Sheets.Where(s => s.Properties.Title == Name).FirstOrDefault();
                int sheetId = (int)sh.Properties.SheetId;

                //define cell background to light green color
                var userEnteredFormat = new CellFormat()
                {
                    BackgroundColor = new Color()
                    {
                        Red = 0.576471f,
                        Green = 0.7686275f,
                        Blue = 0.4901961f,
                        Alpha = 1
                    }
                };
                BatchUpdateSpreadsheetRequest bussr = new BatchUpdateSpreadsheetRequest();

                //create the update request for cells from the first row
                var updateCellsRequest = new Request()
                {
                    RepeatCell = new RepeatCellRequest()
                    {
                        Range = new GridRange
                        {
                            SheetId = sheetId,
                            StartColumnIndex = 5,
                            StartRowIndex = prevGameId,
                            EndColumnIndex = 6,
                            EndRowIndex = prevGameId + 1
                        },
                        Cell = new CellData()
                        {
                            UserEnteredFormat = userEnteredFormat,
                            UserEnteredValue = new ExtendedValue()
                            {
                                StringValue = "Completed"
                            }
                        },
                        Fields = "UserEnteredFormat(BackgroundColor),UserEnteredValue(StringValue)"
                    }
                };

                bussr.Requests = new List<Request>();
                bussr.Requests.Add(updateCellsRequest);
                var bur = Service.Spreadsheets.BatchUpdate(bussr, SpreadsheetId);
                bur.Execute();

            });
        }

        public async void SetNewGameSectionField(string section)
        {
            await Task.Factory.StartNew(() =>
            {
                //Task.Delay(1000).Wait(); //Maybe for a bit delay between two async data methods
                var lobg = new List<object>() { section };
                updateRequestBody = new ValueRange();
                updateRequestBody.Values = new List<IList<object>>() { lobg };
                SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = Service.Spreadsheets.Values.Update(updateRequestBody, SpreadsheetId, Name + "!D" + (LastGameId+1).ToString());
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                UpdateValuesResponse updateResponse = updateRequest.Execute();
            });
        }

        public async void SetNewGameNameField(string name)
        {
            await Task.Factory.StartNew(() =>
            {
                var lobg = new List<object>() { name };
                updateRequestBody = new ValueRange();
                updateRequestBody.Values = new List<IList<object>>() { lobg };
                SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = Service.Spreadsheets.Values.Update(updateRequestBody, SpreadsheetId, Name + "!E" + (LastGameId + 1).ToString());
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                UpdateValuesResponse updateResponse = updateRequest.Execute();
                SetResultFieldToStarted();
            });
        }

        public void SetResultFieldToStarted()
        {
            //get sheet id by sheet name
            Spreadsheet spr = Service.Spreadsheets.Get(SpreadsheetId).Execute();
            Sheet sh = spr.Sheets.Where(s => s.Properties.Title == Name).FirstOrDefault();
            int sheetId = (int)sh.Properties.SheetId;

            //define cell background to light yellow color
            var userEnteredFormat = new CellFormat()
            {
                BackgroundColor = new Color()
                {
                    Red = 1,
                    Green = 0.8509804f,
                    Blue = 0.4f,
                    Alpha = 1
                }
            };
            BatchUpdateSpreadsheetRequest bussr = new BatchUpdateSpreadsheetRequest();

            //create the update request for cells from the first row
            var updateCellsRequest = new Request()
            {
                RepeatCell = new RepeatCellRequest()
                {
                    Range = new GridRange
                    {
                        SheetId = sheetId,
                        StartColumnIndex = 5,
                        StartRowIndex = LastGameId,
                        EndColumnIndex = 6,
                        EndRowIndex = LastGameId + 1
                    },
                    Cell = new CellData()
                    {
                        UserEnteredFormat = userEnteredFormat,
                        UserEnteredValue = new ExtendedValue()
                        {
                            StringValue = "Started"
                        }
                    },
                    Fields = "UserEnteredFormat(BackgroundColor),UserEnteredValue(StringValue)"
                }
            };

            bussr.Requests = new List<Request>();
            bussr.Requests.Add(updateCellsRequest);
            var bur = Service.Spreadsheets.BatchUpdate(bussr, SpreadsheetId);
            bur.Execute();
        }

       /* public List<Item> GetItemList()
        {
            List<Item> items = new List<Item>();

        }*/
    }
}
