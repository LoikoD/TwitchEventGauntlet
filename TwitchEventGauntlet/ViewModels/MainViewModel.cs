using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventGauntlet.ViewModels
{
    class MainViewModel : Conductor<object>
    {
        private string _gameSection;

        public string GameSection
        {
            get { return _gameSection; }
            set { _gameSection = value; }
        }
        
        public void LoadFakerPage()
        {
            ActivateItem(new StreamerViewModel("MistaFaker"));
        }
        public void LoadMelPage()
        {
            ActivateItem(new StreamerViewModel("Melharucos"));
        }
        public void LoadBjornPage()
        {
            ActivateItem(new StreamerViewModel("UncleBjorn"));
        }
        public void LoadLasqaPage()
        {
            ActivateItem(new StreamerViewModel("Lasqa"));
        }
    }
}
