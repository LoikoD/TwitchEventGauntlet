using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace TwitchEventGauntlet.ViewModels
{
    class NewGameViewModel : Screen
    {
        private string _gameName;

        public string GameName
        {
            get { return _gameName; }
            set
            {
                _gameName = value;
                NotifyOfPropertyChange(() => GameName);
            }
        }
        
        public void Confirm()
        {
            TryClose(true);
        }

    }
}
