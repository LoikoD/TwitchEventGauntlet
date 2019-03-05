using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            }
        }
        public int Subs
        {
            get { return _subs; }
            set
            {
                _subs = value;
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
        
        public Data data = new Data();
        
        public StreamerViewModel(string name)
        {
            IsLoading = false;
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
                IsLoading = true;


                Name = name;
                data.Name = Name;
                GameName = data.GetGames().Last();
                Section = data.GetCurrentSection();
                SubGoalStr = data.GetSubGoal();
                NumGamesStr = data.GetNumGames();
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



            });

            //Task.Delay(500).Wait();
            IsLoading = false;
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

    }

}
