using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XikeonBrShared
{
    public class Sync
    {
        public Status status;
        public Action<string, object> Notify;

        public Sync(PropertyChangedEventHandler OnPropertyChangeCallback)
        {
            status = new Status();
            status.PropertyChanged += OnPropertyChangeCallback;
        }

        public void HandleSyncEvent(string group, string fieldName, dynamic value)
        {
            //dynamic a = GetType().GetProperty(group);
            Type t = GetType();
            FieldInfo modelField = t.GetFields().Where(f => f.FieldType.ToString() == group).First(); // (group); // BindingFlags.Instance);
            ISharedModel model = modelField.GetValue(this) as ISharedModel;
            PropertyInfo prop = model.GetType().GetProperties().Where(p => p.Name == fieldName).First();
            
            RealNotify("[Shared] Hello {0}", string.Format("{0}.{1} = {2}", group, fieldName, value));
            RealNotify("[Shared] Found model: {0}", string.Format("{0} - {1}", modelField.Name, model));
            RealNotify("[Shared] Found prop: {0}", string.Format("{0} -{1}", prop.Name, prop.PropertyType));
            RealNotify("[Shared] Found prop value: {0}", prop.GetValue(model, null));

            //model.GetType()

            //model.SetWithoutSync(prop.Name, prop.PropertyType, value, new Action<dynamic>((resp) => RealNotify("[Shared] from model: {0}", resp)));

            //prop.SetMethod.Invoke(status, new object[] { Convert.ChangeType(value, prop.PropertyType) });
            //prop.SetValue(status, Convert.ChangeType(value, prop.PropertyType), null);
        }

        public void RealNotify(string format, object args) // used to have params object[]
        {
            Notify(format, args);
        }
    }
}
