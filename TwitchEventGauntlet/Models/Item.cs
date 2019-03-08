using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchEventGauntlet.Models
{
    public class Item : WheelObject
    {

        public bool IsBuff { get; set; }
        public bool IsDebuff { get; set; }
        public bool IsLight { get; set; }
        public int Limit { get; set; }
        public int Charges { get; set; }
        public int MaxCharges { get; set; }
        public bool IsGlued { get; set; }

        public Item(int id, string name, string description, bool isBuff, bool isDebuff, bool isLight, int limit, int maxCharges)
        {
            Id = id;
            Name = name;
            Description = description;
            IsBuff = isBuff;
            IsDebuff = isDebuff;
            IsLight = isLight;
            Limit = limit;
            MaxCharges = maxCharges;
            Charges = Charges;
            IsGlued = false;
        }
    }
}
