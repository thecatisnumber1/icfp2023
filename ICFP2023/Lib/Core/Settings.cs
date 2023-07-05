using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ICFP2023
{
    public record Setting(string Name, string ShortName, Type Type)
    {
        public bool TryParse(string input, out object value)
        {
            value = null;
            if (Type == typeof(string))
            {
                value = input;
                return true;
            }
            else if (Type == typeof(int))
            {
                if (int.TryParse(input, out int intValue))
                {
                    value = intValue;
                    return true;
                }
            }
            else if (Type == typeof(bool))
            {
                if (bool.TryParse(input, out bool boolValue))
                {
                    value = boolValue;
                    return true;
                }
            }

            return false;
        }

        public object Parse(string input)
        {
            if (!TryParse(input, out object result))
            {
                throw new ArgumentException($"Could not parse \"{input}\" as {Type.Name}");
            }
            return result;
        }
    }

    public abstract class SettingsBase
    {
        public List<Setting> AllSettings { get; }

        public SettingsBase()
        {
            AllSettings = new List<Setting>();
            var fields = GetFields();
            foreach (var field in fields)
            {
                var shortName = field.GetCustomAttribute<ShortHandAttribute>()?.ShortName;
                AllSettings.Add(new Setting(field.Name, shortName, field.FieldType));
            }
        }

        public object this[string name]
        {
            get => GetField(name).GetValue(this);
            set => GetField(name).SetValue(this, value);
        }

        public object this[Setting setting]
        {
            get => this[setting.Name];
            set => this[setting.Name] = value;
        }

        public void ParseAndSet(Setting setting, string input)
        {
            this[setting] = setting.Parse(input);
        }

        public bool TryParseAndSet(Setting setting, string input)
        {
            if (setting.TryParse(input, out object value))
            {
                this[setting] = value;
                return true;
            }
            return false;
        }

        public SettingsBase Copy()
        {
            SettingsBase newSettings = NewInstance();
            foreach (var setting in AllSettings)
            {
                newSettings[setting] = this[setting];
            }
            return newSettings;
        }

        public void ParseArgs(string[] args)
        {
            var settingMap = MakeFlagNameMap();

            for (int i = 0; i < args.Length; i++)
            {
                string originalArg = args[i];
                var arg = originalArg.ToLowerInvariant();

                if (!arg.StartsWith("-"))
                {
                    throw new ArgumentException($"Expected flag got : {originalArg}");
                }

                arg = arg.TrimStart('-');

                if (settingMap.TryGetValue(arg, out var setting))
                {
                    if (setting.Type == typeof(bool))
                    {
                        this[setting] = (arg == setting.Name || arg == setting.ShortName);
                    }
                    else if (i < args.Length - 1)
                    {
                        ParseAndSet(setting, args[++i]);
                    }
                    else
                    {
                        throw new ArgumentException($"Missing value for argument: {originalArg}");
                    }
                }
                else
                {
                    throw new ArgumentException($"Unknown flag: {originalArg}");
                }
            }
        }

        public override string ToString()
        {
            List<string> parts = new();

            foreach (var setting in AllSettings)
            {
                parts.Add($"{setting.Name} = {this[setting]}");
            }

            return $"Settings: {{ {string.Join(", ", parts)} }}";
        }

        private SettingsBase NewInstance()
        {
            return (SettingsBase)GetType().GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
        }

        private FieldInfo GetField(string name) => GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
        private FieldInfo[] GetFields() => GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        private Dictionary<string, Setting> MakeFlagNameMap()
        {
            var settingsList = AllSettings;
            Dictionary<string, Setting> argumentSettings = new();

            foreach (var setting in settingsList)
            {
                void AddName(string name)
                {
                    name = name.ToLowerInvariant();
                    if (argumentSettings.ContainsKey(name))
                    {
                        throw new Exception($"Flag name collision: {name}");
                    }
                    argumentSettings[name] = setting;
                }

                List<string> names = new() { setting.Name };
                if (setting.ShortName != null)
                    names.Add(setting.ShortName);

                foreach (string name in names)
                {
                    AddName(name);

                    if (setting.Type == typeof(bool))
                    {
                        AddName("no" + name);
                    }
                }
            }

            return argumentSettings;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ShortHandAttribute : Attribute
    {
        public string ShortName { get; }

        public ShortHandAttribute(string shortName)
        {
            ShortName = shortName;
        }
    }
}
