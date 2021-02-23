using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Classifier.Helpers {

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class OptionAttribute : Attribute {

        public string Arg { get; private set; }
        public object DefaultValue { get; private set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }

        public OptionAttribute(string arg, object defaultValue) {
            this.Arg = arg;
            this.DefaultValue = defaultValue;
            this.Required = false;
        }

        public override string ToString() {
            if (!string.IsNullOrEmpty(this.Name)) {
                return this.Name;
            }
            if (!string.IsNullOrEmpty(this.Arg)) {
                return this.Arg;
            }
            return base.ToString();
        }

        public static T Parse<T>(string[] args) {
            int i = 0;
            int cnt = args.Length;
            var data = new Dictionary<string, string>();
            string lastEntry = null;

            while (i < cnt) {
                if (IsArgName(args[i])) {
                    if ((i + 1) < cnt && !IsArgName(args[i + 1])) {
                        lastEntry = args[i].TrimStart(ArgNamePrefix);
                        data[lastEntry] = args[i + 1];
                        i += 2;
                    } else {
                        lastEntry = args[i].TrimStart(ArgNamePrefix);
                        data[lastEntry] = "true";
                        i += 1;
                    }
                } else {
                    // append to the previous arg
                    if (lastEntry != null) {
                        data[lastEntry] = data[lastEntry] + " " + args[i];
                    }
                    i += 1;
                }
            }

            T result = Activator.CreateInstance<T>();

            string overrideData;

            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                var attr = property.GetCustomAttributes(typeof(OptionAttribute), false).FirstOrDefault() as OptionAttribute;
                if (attr == null) {
                    continue;
                }

                string[] names = attr.Arg.Split(new char[] { '|' });
                bool handled = false;
                foreach (string name in names) {
                    if (data.TryGetValue(name, out overrideData)) {
                        if (property.PropertyType == typeof(bool)) {
                            bool parsed;
                            handled = bool.TryParse(overrideData, out parsed);
                            property.SetValue(result, parsed, null);
                        } else if (property.PropertyType == typeof(int)) {
                            int parsed;
                            handled = int.TryParse(overrideData, out parsed);
                            property.SetValue(result, parsed, null);
                        } else if (property.PropertyType == typeof(string)) {
                            handled = true;
                            property.SetValue(result, overrideData, null);
                        } else if (property.PropertyType.IsEnum) {
                            handled = true;
                            overrideData = overrideData.ToLower();
                            property.SetValue(result, Enum.Parse(property.PropertyType, overrideData), null);
                        }
                    }
                }

                if (!handled && attr.Required) {
                    throw new ArgumentNullException(attr.ToString());
                }

                if (!handled) {
                    // try to use the default value
                    if (attr.DefaultValue != null) {
                        property.SetValue(result, attr.DefaultValue, null);
                    }
                }
            }

            return result;
        }

        private static char[] ArgNamePrefix = new char[] { '-', '/' };
        private static bool IsArgName(string arg) {
            return arg.StartsWith("-") || arg.StartsWith("/");
        }
    }
}