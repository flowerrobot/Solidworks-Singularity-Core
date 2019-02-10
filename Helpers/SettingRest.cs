using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SingularityCore
{
    internal class SettingRest<TAttribute, TValue> : IDisposable
    {
        public SettingRest(TAttribute att, string propertyName, TValue value)
        {
            Att = att;
            Value = value;
            PropertyName = propertyName;
        }

        private TAttribute Att { get; }
        private TValue Value { get; }
        private string PropertyName { get; }
        public void Dispose()
        {
            Att.GetType().GetProperty(PropertyName)?.SetValue(Att, Value, null);
            //PropertyInfo property = typeof(Att).GetProperty("Reference");

            //property.SetValue(myAccount, "...", null);
        }
    }
}
