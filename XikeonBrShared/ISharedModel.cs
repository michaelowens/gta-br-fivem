using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XikeonBrShared
{
    public class ISharedModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void SetWithoutSync(string fieldName, Type type, dynamic value, Action<dynamic> cb)
        {
            PropertyInfo prop = GetType().GetProperties().Where(p => p.Name == fieldName).First();

            cb(type);
            
            prop.SetValue(this, Convert.ChangeType(value, type), null);
        }
    }
}
