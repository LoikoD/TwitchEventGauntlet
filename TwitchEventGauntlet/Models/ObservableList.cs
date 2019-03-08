using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventGauntlet.Models
{
    public class ObservableList : ObservableCollection<string>
    {

        public string _0
        {
            get { return Items[0]; }
        }
        public string _1
        {
            get { return Items[1]; }
        }
        public string _2
        {
            get { return Items[2]; }
        }
        public string _3
        {
            get { return Items[3]; }
        }
        public string _4
        {
            get { return Items[4]; }
        }
        public string _5
        {
            get { return Items[5]; }
        }

    }
}
