using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XikeonBrShared
{
    public class ISharedModel : INotifyPropertyChanged
    {
        //public Action<Object, PropertyChangedEventArgs> PropertyChangeCallback;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public ISharedModel ()
        {
            //PropertyChanged += new PropertyChangedEventHandler(OnPropertyChange);
        }
    }
}
