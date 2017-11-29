using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XikeonBrShared
{
    public class Status : ISharedModel
    {
        public int Countdown { get; set; }
        public bool ShowCountdown { get; set; }
        public bool GameStarted { get; set; }
    }
}
