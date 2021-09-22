using System;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using Microsoft.Maui;
using System.Globalization;

namespace Rg.Plugins.Popup.Converters.TypeConverters
{
    public class EasingTypeConverter : TypeConverter
    {
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value != null)
            {
                var fieldInfo = typeof(Easing).GetRuntimeFields()?.FirstOrDefault(fi =>
                {
                    if (fi.IsStatic)
                        return fi.Name == value.ToString();
                    return false;
                });
                if (fieldInfo != null)
                {
                    var fieldValue = fieldInfo.GetValue(null);
                    if (fieldValue != null)
                        return (Easing)fieldValue;
                }
            }
            throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Easing)}");
        }
    }
}
