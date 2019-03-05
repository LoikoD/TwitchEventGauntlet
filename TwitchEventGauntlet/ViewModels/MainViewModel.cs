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
        
        public void LoadFakerPage()
        {
            ActivateItem(new StreamerViewModel("Mistafaker"));
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
