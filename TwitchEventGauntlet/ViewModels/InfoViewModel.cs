using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventGauntlet.ViewModels
{
    class InfoViewModel : Screen
    {
        private string _gameName;
        private string _sectionStr;
        private string _section;
        private int _leftGames;
        private int _completedGames;
        private string _subGoalStr;
        private int _subs;
        private int _subsNeed;

        public string GameName
        {
            get { return _gameName; }
            set
            {
                _gameName = value;
                NotifyOfPropertyChange(() => GameName);
            }
        }
        public string SectionStr
        {
            get
            {
                return Section + " (" + CompletedGames + "/" + LeftGames + ")";
            }
            set
            {
                _sectionStr = value;
                NotifyOfPropertyChange(() => SectionStr);
            }
        }
        public string Section
        {
            get { return _section; }
            set
            {
                _section = value;
                NotifyOfPropertyChange(() => SectionStr);
            }
        }
        public int LeftGames
        {
            get { return _leftGames; }
            set
            {
                _leftGames = value;
                NotifyOfPropertyChange(() => SectionStr);

            }
        }
        public int CompletedGames
        {
            get { return _completedGames; }
            set
            {
                _completedGames = value;
                NotifyOfPropertyChange(() => SectionStr);

            }
        }
        public string SubGoalStr
        { 
            get { return Subs + "/" + SubsNeed; }
            set
            {
                _subGoalStr = value;
                NotifyOfPropertyChange(() => SubGoalStr);
            }
        }
        public int Subs
        {
            get { return _subs; }
            set
            {
                _subs = value;
                NotifyOfPropertyChange(() => SubGoalStr);
            }
        }
        public int SubsNeed
        {
            get { return _subsNeed; }
            set
            {
                _subsNeed = value;
                NotifyOfPropertyChange(() => SubGoalStr);
            }
        }

    }
}
