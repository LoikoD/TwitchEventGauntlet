using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventGauntlet.Models
{
    class OverlayService
    {
        private static OverlayService _instance = new OverlayService();
        public static OverlayService GetInstance() => _instance;

        private OverlayService() { }

        public Action<string> Show { get; set; }

        public string Text { get; set; } = "";

        public void Close()
        {
            Text = "";
        }
    }
}
