using System;
using System.Reflection;

namespace BinanceTR.Net.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumLabelAttribute : Attribute
    {
        public static readonly EnumLabelAttribute Default = new EnumLabelAttribute();
        private string label;

        public EnumLabelAttribute() : this(string.Empty)
        {
        }

        public EnumLabelAttribute(string label)
        {
            this.label = label;
        }

        public virtual string Label
        {
            get
            {
                return LabelValue;
            }
        }

        protected string LabelValue
        {
            get
            {
                return label;
            }
            set
            {
                label = value;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            EnumLabelAttribute other = obj as EnumLabelAttribute;

            return (other != null) && other.Label == Label;
        }

        public override int GetHashCode()
        {
            return Label.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return (Equals(Default));
        }
    }

    public static class EnumLabelExtensions
    {
        public static string GetLabel(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    EnumLabelAttribute attr = Attribute.GetCustomAttribute(field, typeof(EnumLabelAttribute)) as EnumLabelAttribute;
                    if (attr != null)
                    {
                        return attr.Label;
                    }
                }
            }

            return string.Empty;
        }

        public static T GetEnumByLabel<T>(this string @this) where T : Enum
        {
            // Get Default Value
            var defaultValue = default(T);

            // Check Point
            if (string.IsNullOrEmpty(@this))
            {
                return defaultValue;
            }

            // Action
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                if (@this.Trim().ToLower().Equals(item.ToString().ToLower())
                    || @this.Trim().ToLower().Equals(item.GetLabel().ToLower()))
                {
                    return item;
                }
            }

            // Return Dummy
            return defaultValue;
        }

        public static T GetEnumByValue<T>(this int @this) where T : Enum
        {
            // Get Default Value
            var defaultValue = default(T);

            // Action
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                Enum test = Enum.Parse(typeof(T), item.ToString()) as Enum;
                int intValue = Convert.ToInt32(test);

                if (@this == intValue)
                {
                    return item;
                }
            }

            // Return Dummy
            return defaultValue;
        }

        public static int GetValue<T>(this T @this) where T : Enum
        {
            return Convert.ToInt32(@this);
        }

        public static int GetValueByLabel<T>(this string @this) where T : Enum
        {
            return Convert.ToInt32(@this.GetEnumByLabel<T>());
        }

        public static string GetLabelByValue<T>(this int @this) where T : Enum
        {
            return @this.GetEnumByValue<T>().GetLabel();
        }
    }

}
