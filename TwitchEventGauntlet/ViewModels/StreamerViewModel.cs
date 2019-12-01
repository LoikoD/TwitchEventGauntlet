using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TwitchEventGauntlet.Models;

namespace TwitchEventGauntlet.ViewModels
{
    public class StreamerViewModel : Screen
    {

        private string _color = "White";
        private string _gameName = "";
        private string _section = "";
        private int _leftGames = 0;
        private int _completedGames = 0;
        private int _subs = 0;
        private int _subsNeed = 0;
        private bool _isLoading = false;
        private string _itemDescription;
        private ObservableCollection<string> _itemPaths;
        private ObservableCollection<string> _smallItemPaths;
        private int _loadingValue;
        private int _inventorySize;

        private InfoViewModel infoViewModel;
        private IWindowManager WindowManager;

        public int Id { get; set; }
        public string Name { get; set; }





        public string Color
        {
            get { return _color; }
            set
            {
                _color = value;
                NotifyOfPropertyChange(() => Color);
            }
        }
        public string Section
        {
            get { return _section; }
            set
            {
                _section = value;
                infoViewModel.Section = Section;
                NotifyOfPropertyChange(() => Section);
                NotifyOfPropertyChange(() => SectionStr);
            }
        }
        public int LeftGames
        {
            get { return _leftGames; }
            set
            {
                _leftGames = value;
                infoViewModel.LeftGames = LeftGames;
                NotifyOfPropertyChange(() => LeftGames);
                NotifyOfPropertyChange(() => SectionStr);

            }
        }
        public int CompletedGames
        {
            get { return _completedGames; }
            set
            {
                _completedGames = value;
                infoViewModel.CompletedGames = CompletedGames;
                NotifyOfPropertyChange(() => CompletedGames);
                NotifyOfPropertyChange(() => SectionStr);

            }
        }
        public string NumGamesStr
        {
            get { return CompletedGames + "/" + LeftGames; }
            set
            {
                string[] values = value.Split('/');
                if (values.Count() > 1)
                {
                    CompletedGames = Convert.ToInt32(values[0]);
                    LeftGames = Convert.ToInt32(values[1]);
                }
                else
                {
                    CompletedGames = 0;
                    LeftGames = Convert.ToInt32(values[0]);
                }
            }
        }
        public string SectionStr
        {
            get
            {
                return Section + " (" + CompletedGames + "/" + LeftGames + ")";
            }
        }
        public string GameName
        {
            get { return _gameName; }
            set
            {
                _gameName = value;
                infoViewModel.GameName = GameName;
                NotifyOfPropertyChange(() => GameName);
            }
        }
        public string SubGoalStr
        {
            get { return Subs + "/" + SubsNeed; }
            set
            {
                string[] values = value.Split('/', ' ');
                Subs = Convert.ToInt32(values[0]);
                SubsNeed = Convert.ToInt32(values[1]);
                infoViewModel.Subs = Convert.ToInt32(values[0]);
                infoViewModel.SubsNeed = Convert.ToInt32(values[1]);

            }
        }
        public int Subs
        {
            get { return _subs; }
            set
            {
                _subs = value;
                infoViewModel.Subs = Subs;
                NotifyOfPropertyChange(() => Subs);
                NotifyOfPropertyChange(() => SubGoalStr);
            }
        }
        public int SubsNeed 
        {
            get { return _subsNeed; }
            set
            {
                _subsNeed = value;
                infoViewModel.SubsNeed = SubsNeed;
                NotifyOfPropertyChange(() => SubsNeed);
                NotifyOfPropertyChange(() => SubGoalStr);
            }
        }
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
            }
        }
        public string ItemDescription
        {
            get { return _itemDescription; }
            set
            {
                _itemDescription = value;
                NotifyOfPropertyChange(() => ItemDescription);
            }
        }
        public ObservableCollection<string> ItemPaths
        {
            get { return _itemPaths; }
            set
            {
                _itemPaths = value;
                NotifyOfPropertyChange(() => ItemPaths);
            }
        }
        public ObservableCollection<string> SmallItemPaths
        {
            get { return _smallItemPaths; }
            set
            {
                _smallItemPaths = value;
                NotifyOfPropertyChange(() => SmallItemPaths);
            }
        }
        public int LoadingValue
        {
            get { return _loadingValue; }
            set
            {
                if (value > LoadingValue)
                {
                    for (int i = LoadingValue; i < value; ++i)
                    {
                        _loadingValue = i;
                        NotifyOfPropertyChange(() => LoadingValue);
                        Task.Delay(5).Wait();
                    }
                }
                else
                {
                    _loadingValue = value;
                    NotifyOfPropertyChange(() => LoadingValue);
                }
            }
        }
        public int InventorySize
        {
            get { return _inventorySize; }
            set
            {
                _inventorySize = value;
                NotifyOfPropertyChange(() => InventorySize);
            }
        }

        public List<Item> items;
        public Data data = new Data();


        public StreamerViewModel(string name)
        {
            LoadingValue = 0;
            IsLoading = true;
            infoViewModel = new InfoViewModel();
            WindowManager = new WindowManager();
            ItemPaths = new ObservableCollection<string>();
            SmallItemPaths = new ObservableCollection<string>();
            while (ItemPaths.Count < 16)
            {
                ItemPaths.Add("/Icons/null.png");
            }
            while (SmallItemPaths.Count < 16)
            {
                SmallItemPaths.Add("/Icons/null.png");
            }
            OverlayService.GetInstance().Show = (str) =>
            {
                OverlayService.GetInstance().Text = str;
            };
            UpdateInfo(name);
            
            
        }

        public async void UpdateInfo(string name)
        {
            await Task.Factory.StartNew(() =>
            {


                Name = name;
                data.Name = Name;
                Task task = Task.Factory.StartNew(() => { LoadingValue = 25; });
                GameName = data.GetGames().Last();
                Task.WaitAny(task);
                task = Task.Factory.StartNew(() => { LoadingValue = 35; });
                Task.WaitAny();
                Section = data.GetCurrentSection();
                Task.WaitAny(task);
                task = Task.Factory.StartNew(() => { LoadingValue = 50; });
                SubGoalStr = data.GetSubGoal();
                Task.WaitAny(task);
                task = Task.Factory.StartNew(() => { LoadingValue = 70; });
                NumGamesStr = data.GetNumGames(Section);
                Task.WaitAny(task);
                task = Task.Factory.StartNew(() => { LoadingValue = 90; });
                switch (Name)
                {
                    case "Mistafaker":

                        Id = 0;
                        Color = "Orange";
                        break;
                    case "Melharucos":
                        Id = 1;
                        Color = "Yellow";
                        break;
                    case "UncleBjorn":
                        Id = 2;
                        Color = "Green";
                        break;
                    case "Lasqa":
                        Id = 3;
                        Color = "Blue";
                        break;
                    default:
                        Color = "White";
                        break;
                }

                items = new List<Item>();
                InventorySize = data.GetInventorySize(Id);
                Task.WaitAny(task);
                task = Task.Factory.StartNew(() => { LoadingValue = 100; });
                items = data.GetItemList(Id);
                if (items != null)
                {
                    for (int i = 0; i < 16; ++i)
                    {
                        List<Item> itemsI = items.Where(item => item.CellId == i).ToList();
                        foreach (Item item in itemsI)
                        {
                            if (item.IsSmall)
                            {
                                SmallItemPaths[i] = ("/Icons/" + item.Id + ".png");
                            }
                            else
                            {
                                ItemPaths[i] = ("/Icons/" + item.Id + ".png");
                            }
                        }
                    }
                }

                Task.WaitAny(task);
                IsLoading = false;
            });

            //Task.Delay(500).Wait();
        }

        public void AddSub()
        {
            ++Subs;
            if (Subs >= SubsNeed)
            {
                Subs -= SubsNeed;
                SubsNeed += 5;
                LeftGames++;
                data.AddGameToSection(Section);
            }
            data.SetSubsNum(Subs, SubsNeed);
        }

        public void DropGame()
        {
            data.DropCurrent();
            PrevSection();
            data.SetNewGameSectionField(Section);
            WindowManager = new WindowManager();
            NewGameViewModel newGameViewModel = new NewGameViewModel();
            while (true)
            {
                if (WindowManager.ShowDialog(newGameViewModel) == true)
                {
                    GameName = newGameViewModel.GameName;
                    data.SetNewGameNameField(GameName);
                    return;
                }
            }
        }

        public void Reroll()
        {
            data.RerollCurrent();
            data.SetNewGameSectionField(Section);
            WindowManager = new WindowManager();
            NewGameViewModel newGameViewModel = new NewGameViewModel();
            while (true)
            {
                if (WindowManager.ShowDialog(newGameViewModel) == true)
                {
                    GameName = newGameViewModel.GameName;
                    data.SetNewGameNameField(GameName);
                    return;
                }
            }
        }
        public void Completed()
        {
            data.CompleteCurrent();
            CompletedGames++;
            data.AddCompletedGameToSection(Section);
            if (CompletedGames == LeftGames)
            {
                CompletedGames = 0;
                NextSection();
            }
            data.SetNewGameSectionField(Section);
            WindowManager = new WindowManager();
            NewGameViewModel newGameViewModel = new NewGameViewModel();
            while (true)
            {
                if (WindowManager.ShowDialog(newGameViewModel) == true)
                {
                    GameName = newGameViewModel.GameName;
                    data.SetNewGameNameField(GameName);
                    return;
                }
            }
        }

        public void NextSection()
        {
            switch (Section)
            {
                case "0-100":
                    Section = "100-200";
                    NumGamesStr = data.GetNumGames(Section);
                    break;
                case "100-200":
                    Section = "200-400";
                    NumGamesStr = data.GetNumGames(Section);
                    break;
                case "200-400":
                    Section = "400-550";
                    NumGamesStr = data.GetNumGames(Section);
                    break;
                case "400-550":
                    Section = "550-700";
                    NumGamesStr = data.GetNumGames(Section);
                    break;
                case "550-700":
                    Section = "700-1000";
                    NumGamesStr = data.GetNumGames(Section);
                    break;
                case "700-1000":
                    Section = "1000-1500";
                    NumGamesStr = data.GetNumGames(Section);
                    break;
                case "1000-1500":
                    Section = "1500+";
                    NumGamesStr = data.GetNumGames(Section);
                    break;
                case "1500+":
                    EndChallenge();
                    break;
                default:
                    throw new ArgumentException("ERROR: Previous section unknowned");
            }
        }


        public void PrevSection()
        {
            switch (Section)
            {
                case "0-100":
                    LeftGames++;
                    data.AddGameToSection(Section);
                    break;
                case "100-200":
                    Section = "0-100";
                    NumGamesStr = data.GetNumGames(Section);
                    LeftGames++;
                    data.AddGameToSection(Section);
                    break;
                case "200-400":
                    Section = "100-200";
                    NumGamesStr = data.GetNumGames(Section);
                    LeftGames++;
                    data.AddGameToSection(Section);
                    break;
                case "400-550":
                    Section = "200-400";
                    NumGamesStr = data.GetNumGames(Section);
                    LeftGames++;
                    data.AddGameToSection(Section);
                    break;
                case "550-700":
                    Section = "400-550";
                    NumGamesStr = data.GetNumGames(Section);
                    LeftGames++;
                    data.AddGameToSection(Section);
                    break;
                case "700-1000":
                    Section = "550-700";
                    NumGamesStr = data.GetNumGames(Section);
                    LeftGames++;
                    data.AddGameToSection(Section);
                    break;
                case "1000-1500":
                    Section = "700-1000";
                    NumGamesStr = data.GetNumGames(Section);
                    LeftGames++;
                    data.AddGameToSection(Section);
                    break;
                case "1500+":
                    Section = "1000-1500";
                    NumGamesStr = data.GetNumGames(Section);
                    LeftGames++;
                    data.AddGameToSection(Section);
                    break;
                default:
                    throw new ArgumentException("ERROR: Previous section unknowned");
            }
        }

        public void EndChallenge()
        {
            throw new NotImplementedException("Congratulations! You've finished the challenge, but unfortunately this case has not been implemented yet. We're sorry!");
        }

        public void OpenInfoWindow()
        {
            WindowManager = new WindowManager();
            WindowManager.ShowWindow(infoViewModel);
        }

        public void ItemHover(int index)
        {
            if (items != null)
            {
                ItemDescription = items.Where(i => i.CellId == index).FirstOrDefault()?.Description;
            }
        }

        public void SmallItemHover(int index)
        {
            if (items != null)
            {
                ItemDescription = items.Where(i => i.CellId == index && i.IsSmall == true).FirstOrDefault()?.Description;
            }
        }

        public void UseItem(int cellId, bool isSmall)
        {
            int itemIndex = items.FindIndex(i => i.CellId == cellId && i.IsSmall == isSmall);

            int itemId = items[itemIndex].Id;
            switch (itemId)
            {
                case 33: // Замок работяг
                    Random random = new Random();
                    int cellToBlock = random.Next(0, InventorySize - 1);
                    bool dropped = DropItem(cellToBlock);
                    if (dropped)
                    {
                        //AssignItemToCell(cellToBlock);
                    }
                    break;
                case 34: // Повязка Рембо
                    break;
            }






            //Reduce charges of item
            if (items[itemIndex].MaxCharges != 0)
            {
                items[itemIndex].Charges--;
                //NotifyOfPropertyChange(() => items[itemIndex].Charges);
                if (items[itemIndex].Charges < 1)
                {
                    DropItem(cellId, isSmall);
                }
            }

        }

        public bool DropItem(int cellId, bool isSmall = false)
        {
            throw new NotImplementedException("DropItem has not been implemented yet");
        }
    }

}
