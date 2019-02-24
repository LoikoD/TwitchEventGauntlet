using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventGauntlet.ViewModels
{
    public class StreamerViewModel : Screen
    {

        private string _color;
        private string _gameName;
        private string _section;
        private int _leftGames;
        private int _sumGames;
        private int _subs;
        private int _subsNeed;

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
        public int SumGames
        {
            get { return _sumGames; }
            set
            {
                _sumGames = value;
                NotifyOfPropertyChange(() => SumGames);
                NotifyOfPropertyChange(() => SectionStr);

            }
        }
        public string SectionStr
        {
            get
            {
                return Section + " (" + LeftGames + "/" + SumGames + ")";
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











        public StreamerViewModel(string name)
        {
            Name = name;
            switch (Name)
            {
                case "MistaFaker":
                    Id = 0;
                    Color = "Orange";
                    GameName = "Puzzle3000";
                    Section = "200-400";
                    Subs = 0;
                    SubsNeed = 25;
                    break;
                case "Melharucos":
                    Id = 1;
                    Color = "Yellow";
                    GameName = "RacePRO";
                    Section = "100-200";
                    Subs = 1;
                    SubsNeed = 20;
                    break;
                case "UncleBjorn":
                    Id = 2;
                    Color = "Green";
                    GameName = "haHAA";
                    Section = "0-100";
                    Subs = 2;
                    SubsNeed = 15;
                    break;
                case "Lasqa":
                    Id = 3;
                    Color = "Blue";
                    GameName = "3Head";
                    Section = "1500+";
                    Subs = 3;
                    SubsNeed = 10;
                    break;
                default:
                    Color = "White";
                    break;
            }
        }

        public void AddSub()
        {
            ++Subs;
            if (Subs >= SubsNeed)
            {
                Subs -= SubsNeed;
                SubsNeed += 5;
                SumGames++;
            }
        }
    }
}
