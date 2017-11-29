using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XikeonBrShared
{
    public class Sync
    {
        public Status status;

        public Sync(PropertyChangedEventHandler OnPropertyChangeCallback)
        {
            status = new Status();
            status.PropertyChanged += OnPropertyChangeCallback;
        }
    }
}
