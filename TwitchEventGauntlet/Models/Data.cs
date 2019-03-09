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
        public int LastGameId { get; set; }
        public List<Item> itemList;

        public string SpreadsheetId { get; set; }
        public string Range { get; set; }
        public SheetsService Service { get; set; }
        public ValueRange Response { get; set; }
        public ValueRange UpdateRequestBody { get; set; }

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

            itemList = new List<Item>();
            itemList = GetFullItemList();

            // Create Google Sheets API service.
            Service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define request parameters.
            SpreadsheetId = "1N2HA0RLv6Q_Lxv9y8vBbmlOTv8a2pep_XRp1vicOPx0";
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
                UpdateRequestBody = new ValueRange();
                UpdateRequestBody.Values = new List<IList<object>>() { lobg };

                SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = Service.Spreadsheets.Values.Update(UpdateRequestBody, SpreadsheetId, Name + "!S7");
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
                            UpdateRequestBody = new ValueRange();
                            UpdateRequestBody.Values = new List<IList<object>>() { lobg };
                            SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = Service.Spreadsheets.Values.Update(UpdateRequestBody, SpreadsheetId, Name + "!B3");
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
                            UpdateRequestBody = new ValueRange();
                            UpdateRequestBody.Values = new List<IList<object>>() { lobg };
                            SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = Service.Spreadsheets.Values.Update(UpdateRequestBody, SpreadsheetId, Name + "!B3");
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
                UpdateRequestBody = new ValueRange();
                UpdateRequestBody.Values = new List<IList<object>>() { lobg };
                SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = Service.Spreadsheets.Values.Update(UpdateRequestBody, SpreadsheetId, Name + "!D" + (LastGameId+1).ToString());
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                UpdateValuesResponse updateResponse = updateRequest.Execute();
            });
        }

        public async void SetNewGameNameField(string name)
        {
            await Task.Factory.StartNew(() =>
            {
                var lobg = new List<object>() { name };
                UpdateRequestBody = new ValueRange();
                UpdateRequestBody.Values = new List<IList<object>>() { lobg };
                SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest = Service.Spreadsheets.Values.Update(UpdateRequestBody, SpreadsheetId, Name + "!E" + (LastGameId + 1).ToString());
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

        public List<Item> GetFullItemList()
        {
            List<Item> items = new List<Item>();
            if (File.Exists("ItemsData.json"))
            {
                items = JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText("ItemsData.json"));
            }
            else
            {
                throw new FileNotFoundException("Data.GetFullItemList(): File not found: \"ItemsData.json\"");
            }
            /*   StreamReader reader = File.OpenText("../../Data/items.txt");
               string line;
               if ((line = reader.ReadLine()) != null)
               {
                   while ((line = reader.ReadLine()) != null)
                   {
                       string[] fields = line.Split('\t');
                       if (fields.Count() > 7)
                       {
                           items.Add(new Item(Convert.ToInt32(fields[0]), fields[1], fields[2], Convert.ToBoolean(fields[3]), Convert.ToBoolean(fields[4]),
                               Convert.ToBoolean(fields[5]), Convert.ToInt32(fields[6]), Convert.ToInt32(fields[7])));
                       }
                       else
                       {
                           throw new FileFormatException("ERROR: data.GetItemList(): line don't have enough fields");
                       }
                       return items;
                   }
               }
               else
               {
                   throw new FileLoadException("ERROR: data.GetItemList(): file don't have any lines");
               }

               throw new FileLoadException("ERROR: data.GetItemList(): file don't have any lines");
               */
           /* items.Add(new Item(33,
                "Замок работяг",
                "Замок работяг. Перманент. Дебафф. Блокирует одну ячейку инвентаря. После получения этой шмотки производится ролл колеса для определения, какая именно ячейка будет заблокирована. Если в заблокированной ячейке был другой предмет, он немедленно сбрасывается и не может быть использован. Замок так же может сбросить и дебафающие шмотки. Если Замок работяг открывается Стримерским ключиком, то стример получает доп.ячейку инвентаря.",
                false, true, false, 0, 0));
            items.Add(new Item(34,
                "Повязка Рембо",
                "Повязка Рембо. Перманент. Дебафф. Вынуждает стримера проходить все игры на самом высоком уровне сложности, если игра позволяет выбрать сложность. Если выпадает во время прохождения игры, то сложность меняется если игра позволяет сделать это посреди прохождения. Если в инвентаре и повязка Рэмбо и очки EZ, то оба предмета сбрасываются. Лимит - 1.",
                false, true, false, 1, 0));
            items.Add(new Item(35,
                "Взрывчатка",
                "Взрывчатка. Перманент. Дебафф. После прохождения каждой игры стример бросает монетку, где одна сторона - взрыв, вторая - ничего. При взрыве сбрасывается, а стример получает +игру. Если взрыва нет, то шмотка остается в инвентаре до тех пор, пока не взорвется или не будет сброшена. Если в инвентаре больше одной взрывчатки, то после каждой игры производится столько роллов, сколько взрывчаток в инвентаре.",
                false, true, false, 0, 0));
            items.Add(new Item(36,
                "Очки EZ",
                "Очки EZ. Перманент. Бафф. Стример проходит любую игру на легком уровне сложности, если игра позволяет выбрать сложность. Если в инвентаре и повязка Рэмбо и очки EZ, то оба предмета сбрасываются. Лимит - 1.",
                true, false, false, 1, 0));
            items.Add(new Item(37,
                "Напёрсток удачи",
                "Напёрсток удачи. Перманент. Бафф. При каждом активации колеса фортуны, стример, до ролла, заменяет неугодный ему пункт на другой, который есть в колесе. Два напёрстка - стример меняет два пункта. Лимит - 2. ",
                true, false, false, 2, 0));
            items.Add(new Item(38,
                "Свиток реролла",
                "Свиток реролла. Разовая шмотка. Бафф. Позволяет рерольнуть игру в любой момент. Лимит - 1",
                true, false, true, 1, 1));
            items.Add(new Item(39,
                "Стримерский ключик",
                "Стримерский ключик. Разовая шмотка. Бафф. Открывает замок работяг (снимает блокировку с ячейки инвентаря). Легкий, не занимает слот в инвентаре. Максимально допустимое количество одновременно - 1. ",
                true, false, true, 1, 1));
            items.Add(new Item(40,
                "Шар всезнания",
                "Шар всезнания. Разовая шмотка. Бафф. Позволяет стримеру смотреть прохождение текущей игры на любом ресурсе в обход правила №10. После прохождения игры теряет свою силу и разбивается. Лимит - 1 ",
                true, false, false, 1, 1));
            items.Add(new Item(41,
                "Чокер боли",
                "Чокер боли. Многоразовая шмотка. Дебафф. Пока находится в инвентаре стримера, счетчик сабов для +игры становится равен 10 постоянно. После сбрасывания чокера, счетчик возвращается к тому значению, которое было до появления этой шмотки. Если в инвентаре и корона короля Петучей и Чокер боли, то оба предмета сбрасываются.  Количество зарядов - 5. Повторное выпадение чокера прибавляет 5 зарядов к уже имеющемуся. ",
                false, true, false, 0, 5));
            items.Add(new Item(42,
                "Жопа Мидаса",
                "Жопа Мидаса. Многоразовая шмотка. Дебафф. В отличии от руки Мидаса, жопа Мидаса удешевляет все вокруг. Прокрут колеса стоит 1000 вместо 2500 тысяч рублей. Количество зарядов - 5. Если у стримера уже есть жопа Мидаса и выпадает вторая, то просто добавляется пять зарядов к первой жопе, а вторая жопа сбрасывается. Если в инвентаре и жопа Мидаса и рука Мидаса, то оба предмета сбрасываются. Количество зарядов при этом неважно. ",
                false, true, false, 0, 5));
            items.Add(new Item(43,
                "Подтянутые штаны",
                "Подтянутые штаны. Разовая шмотка. СуперБафф (единственный в своем роде, сильнее ЛЮБОГО дебаффа). Игнорирует правила любого дебаффа. Подтягивает штаны на любую шмотку с дебаффом и удаляется из инвентаря стримера.",
                true, false, false, 0, 1));
            items.Add(new Item(44,
                "Зелье легкости",
                "Зелье легкости. Разовая шмотка. Бафф. Должно быть выпито мгновенно после получения. Добавляет еще одну ячейку в инвентарь. ",
                true, false, false, 0, 1));
            items.Add(new Item(45,
                "Шаурма от Ласки",
                "Шаурма от Ласки. Разовая шмотка. Дебафф. Должна быть употреблена мгновенно после получения. Отнимает одну ячейку инвентаря по выбору стримера. Бафф\\Дебафф при этом скидывается, но если в инвентаре есть и бафф и дебаффы, то обязательно должна быть скинута ячейка с баффом. Пустая ячейка номиннально считается лучше дебаффа, поэтому если есть пустые ячейки, то они должны быть отняты перед тем, как дебафф коснется ячеек с дебаффом. Если есть только пустые ячейки + баффы, то стример сам решает что скинуть: пустую ячейку или ячейку с баффом.",
                false, true, false, 0, 1));
            items.Add(new Item(46,
                "Яблоко раздора",
                "Яблоко раздора. Разовая шмотка. Бафф. При активации стример вправе единоразово проигнорировать любой пункт колеса фортуны, даже из тех событий и шмоток, которые уже в инвентаре или в режиме ожидания (но не придумывать свой). Все остальные стримеры могут мошнить на этого человека до конца ивента. Лимит - 1",
                true, false, false, 1, 1));
            items.Add(new Item(47,
                "Четырехлистный клевер",
                "Четырехлистный клевер. Специфический ролл. Шмотка. Бафф. Количество зарядов: 1. Используется только перед роллом игры. Стример может сам решить, перед каким роллом заюзать клевер. В фильтре ролла выставляется фильтр оценки не ниже 80. Стоимость игры любая (0-5000), зачет в текущий ценовой отрезок. Лимит - 1",
                true, false, false, 1, 1));
            items.Add(new Item(48,
                "Воровской карман",
                "Воровской карман. Разовый бафф. У стримера появляется дополнительная ячейка инвентаря (даже если уже имеется максимально допустимой количество ячеек), куда стример может положить любую шмотку из инвентаря любого стримера, в том числе из своего. Шмотка в воровском кармане приобретает свойство \"Разовая\" и исчезает после использования. При этом стример сам решает, когда применять свойство шмотки из воровского кармана. (К примеру \"повязка Рэмбо\" становится активной лишь на одну игру, а не на все, взрывчатка исчезает после одного броска монеты, даже если выпала сторона, при которой она не взрывается и т.д.). Лимит воровских карманов - 1",
                true, false, false, 1, 1));
            items.Add(new Item(49,
                "Уголёк",
                "Уголёк. Перманент. Не бафф и не дебафф. Уголек кидается в другого стримера. Тот стример получает уголек в свой инвентарь. Уголек просто занимает слот и ничего не делает. Если пустых слотов у стримера нет, ничего не происходит. Считается сильнее баффа, поэтому при заполненном инвентаре не может быть скинута другим баффом, чтобы освободить ячейку, а если выпадает дебафф, то скидывается только поле того, как были скинуты баффы с других ячеек. Может быть сброшена при выпадения \"Шаурмы Ласки\" без приоритета над баффами.",
                false, false, false, 0, 0));
            items.Add(new Item(50,
                "Плащ Макса Пейна",
                "Плащ Макса Пейна. Разовая шмотка. Бафф. Таймер текущий игры переходит в режим Bullet time - итоговый показатель таймера от старта до конца\\реролла\\дропа сокращается вдвое. Два плаща Макса Пейна не могут быть заюзаны на одну и ту же игру. Лимит - 1",
                true, false, false, 1, 1));
            items.Add(new Item(51,
                "Косичка Бьерна",
                "Косичка Бьерна. Специфический ролл. Шмотка. Дебафф. Должна быть использована при следующем ролле игры. В фильтрах ролла выставляется фильтр на оценку игры SGG не выше 25, а минимальное время - 6 часов. Стоимость игры любая (0-5000), зачет в текущий ценовой отрезок. Имеет приоритет над клевером. Лимит - 1 ",
                false, true, false, 1, 1));
            items.Add(new Item(52,
                "Парные кольца несчастья",
                "Парные кольца несчастья. Перманент. Дебафф. Стример отдает одно кольцо другому стримеру.  Шмотки, дающие дебафф работают на обоих. Кольца автоматически сбрасываются у обоих если одному из стримеров удается каким-то образом это кольцо сбросить. У одного стримера не может быть \"связи\" больше чем с одним стримером. Дебаффы копируют свойство дебаффа, а не саму шмотку.",
                false, true, false, 1, 0));
            items.Add(new Item(53,
                "Плитка шоколада",
                "Плитка шоколада. Разовая шмотка. Бафф. У стримера все в шоколаде. При активации, прохождение текущей игры происходит без участия каких-либо баффов и дебаффов. Абсолютно все события с колеса фортуны переносятся на следующую игру. Лимит - 1 ",
                true, false, false, 1, 1));
            items.Add(new Item(54,
                "Корона короля Петучей",
                "Корона короля Петучей. Шмотка. Бафф. Количество зарядов: 1. Сабгол, необходимый для +игры становится 50. После активация сабгола сбрасывается, а актуальный сабгол возвращается к тому значению, каким он был до появления короны. Если в инвентаре и корона короля Петучей и Чокер боли, то оба предмета сбрасываются. Лимит - 1",
                true, false, false, 1, 1));
            items.Add(new Item(55,
                "Венок успеха",
                "Венок успеха. Шмотка. Бафф. Количество зарядов: 1. Должна быть активирована перед роллом колеса. Стример сам выбирает перед каким конкретно. Колесо роллится только из положительных пунктов и баффов, однако содержит в себе пять пустых позиций, при которых ничего не происходит. Лимит - 1",
                true, false, false, 1, 1));
            items.Add(new Item(56,
                "Рука Мидаса",
                "Рука Мидаса. Многоразовая шмотка. Бафф. Все вокруг становится дорого-богато. Прокрут колеса стоит 5000 рублей вместо 2500. Количество зарядов - 3. Если у стримера уже есть рука Мидаса и выпадает вторая, то просто добавляется три заряда к первой руке, а вторая рука сбрасывается. Если в инвентаре и жопа Мидаса и рука Мидаса, то оба предмета сбрасываются. Количество зарядов при этом неважно.",
                true, false, false, 0, 3));
            items.Add(new Item(57,
                "Монета отстоя",
                "Монета отстоя. Многоразовая шмотка. Дебафф. После каждого прокрута колеса, при выпадении положительных событий или баффов, вынуждает стримера загадать сторону и подбросить монетку, где та сторона, которую стример загадал - прок события\\шмотки, другая сторона - скип события\\шмотки. Количество зарядов на подброс монетки - 5. Лимит - 1",
                false, true, false, 0, 5));
            items.Add(new Item(58,
                "Дырявый парашют",
                "Дырявый парашют. Шмотка. Бафф. При дропе игры стример может выбрать сторону и бросить монетку. Если выпала  выбранная сторона, стример не возвращается на отрезок назад. После подброса монеты парашют сбрасывается. Лимит - 1.",
                true, false, false, 1, 1));
            items.Add(new Item(59,
                "Скотч",
                "Скотч. Легкая шмотка. Бафф. Стример может плотно примотать любую шмотку из своего инвентаря скотчем. Эта шмотка приобретает вторую жизнь: при активации механики сброса (Забывайкин, Три топора) шмотка остается в инвентаре, спадает только скотч. Тоже самое касается механики замены баффа на дебафф. Если инвентарь заполнен дебаффами и одной бафф-шмоткой со скотчем, то при выпадении еще одного дебаффа стример сбрасывает и выпавший дебафф и скотч. Намотанный на шмотку скотч уже не считается отдельной шмоткой, поэтому стример может забрать второй скотч к себе в инвентарь, если он ему выпадет. Лимит - 1.",
                true, false, true, 1, 1));
            items.Add(new Item(60,
                "Кредитка Хакса",
                "Кредитка Хакса. Многоразовая шмотка. Дебафф. Количество зарядов: 3. После активации колеса фортуны используется один заряд, который добавляет 5 сабов в сабгол. Если выпадает вторая то добавляет 3 заряда к текущей шмотке. ",
                true, false, false, 0, 3));
            File.WriteAllText("ItemsData.json", JsonConvert.SerializeObject(items));
            */

            return items;

        }

        public List<Item> GetItemList(int streamerId)
        {
            List<Item> items = new List<Item>();
            char letterRange = (char)('K' + streamerId);

            ValueRange itemResponse = Service.Spreadsheets.Values.Get(SpreadsheetId, "Правила!" + letterRange + "20:" + letterRange + "43").Execute();
            IList<IList<object>> values = itemResponse.Values;
            if (values != null && values.Count > 0)
            {
                for (int i = 0; i < values.Count; ++i)
                {
                    var row = values[i];
                    if (row.Count > 0 && row[0] != null && !string.IsNullOrWhiteSpace(row[0].ToString()))
                    {
                        string[] fields = row[0].ToString().Split(',');
                        bool isGlued = false;
                        bool isSmall = false;
                        int charges = 0;
                        int id = 0;
                        int cellId = 0;
                        for (int j = 0; j < fields.Count(); ++j)
                        {
                            if (fields[j].First() == '_')
                            {
                                isSmall = true;
                                fields[j] = fields[j].Substring(1);
                            }
                            else if (fields[j].First() == '*')
                            {
                                isGlued = true;
                                fields[j] = fields[j].Except("*").ToString();
                            }
                            string[] idAndCell = fields[j].Split(':');
                            if (idAndCell.Count() > 1)
                            {
                                try
                                {
                                    id = Convert.ToInt32(idAndCell[0]);
                                    items.Add(itemList.Where(item => item.Id == id).FirstOrDefault());
                                    items.Last().IsSmall = isSmall;
                                    items.Last().IsGlued = isGlued;
                                    string[] cellAndCharges = idAndCell[1].Split('(', ')');
                                    cellId = Convert.ToInt32(cellAndCharges[0]);
                                    items.Last().CellId = cellId;
                                    if (cellAndCharges.Count() > 1)
                                    {
                                        charges = Convert.ToInt32(cellAndCharges[1]);
                                        items.Last().Charges = charges;
                                    }
                                }
                                catch (FormatException)
                                {
                                    Console.WriteLine("Data.GetItemList(): Error! FormatException(\"Not int value where should be int value\")");
                                }
                                

                            }
                            else
                            {
                                Console.WriteLine("Data.GetItemList(): Error! Cell(row: " + i + " , field: " + j + ") of inventory has wrong format");
                            }
                        }
                    }
                }
                return items;
            }
            else
            {
                Console.WriteLine("No data found.");
                return null;
            }

        }

        public int GetInventorySize(int streamerId)
        {
            //get sheet id by sheet name
            var getRequest = Service.Spreadsheets.Get(SpreadsheetId);
            getRequest.IncludeGridData = true;
            char letterRange = (char)('K' + streamerId);
            getRequest.Ranges = "Правила!" + letterRange + "20:" + letterRange + "43";
            Spreadsheet spr = getRequest.Execute();
            
            Sheet sh = spr.Sheets.Where(s => s.Properties.Title == "Правила").FirstOrDefault();
            IList<GridData> shData = sh.Data;
            GridData gridData = shData[0];
            int i = 0;
            //Color cellColor = new Color()
            //{

            //}
            string background = "{\"alpha\":null,\"blue\":0.8509804,\"green\":0.8509804,\"red\":0.8509804,\"ETag\":null}";
            Color cellColor;
            int size = 0;
            while (i < 24)
            {
                cellColor = gridData.RowData[i].Values[0].UserEnteredFormat.BackgroundColor;
                if (background == JsonConvert.SerializeObject(cellColor))
                {
                    return size;
                }
                ++size;
                i += 3;
            }
            return size;
        }
    }
}
