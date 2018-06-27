using System.Collections.Generic;
using System.Text;

public class ScriptSpellInput
{
    public string type;
    public string id;
    public string displayName;
    public bool targetRequired;
    public SpellDescriptor.SpellTargetType targetType;
    public Dictionary<string, object> variables;
    public Dictionary<string, string> methods;

    public string className { get { return id; } }
    public string scriptBaseClass
    {
        get
        {
            switch (type)
            {
                case "Instant": return "InstantSpell";
                case "Continuous": return "ContinuousSpell";
                case "Toggle": return "ToggleSpell";
                case "Staged": return "StagedSpell";
                default: return "Spell";
            }
        }
    }
    public string nativeBaseClass { get { return "Script" + scriptBaseClass; } }

    public string GenerateCode()
    {
        var str = new StringBuilder();
        
        str.AppendFormat("Class.{0} = {\n", className);
        str.AppendFormat("\t__inherit = '{0}',\n", scriptBaseClass);
        str.AppendFormat("\t\n");
        foreach (var variable in variables)
        {
            str.AppendFormat("\t{0} = {1},", variable.Key, variable.Value);
        }
        str.AppendFormat("}\n\n");

        var classMethods = ScriptSpellDatabase.GetMethodsList(scriptBaseClass);
        foreach (var method in methods)
        {
            var descriptor = classMethods.Find(m => m.name == method.Key);
            if (descriptor != null)
            {
                str.AppendFormat("function {0}:{1}\n", className, descriptor.Signature);
            }
            else
            {
                MagicLog.LogErrorFormat("Unknown method {0} for script spell {1}", method.Key, id);
                str.AppendFormat("function {0}:{1}(...)\n", className, method.Key);
            }

            foreach (var line in method.Value.Split('\n'))
            {
                str.AppendFormat("\t{0}\n", line);
            }
            str.AppendFormat("end\n\n");
        }

        return str.ToString();
    }
}

public static class ScriptSpellDatabase
{
    public class MethodDescriptor
    {
        public string name;
        public List<string> parameters;

        public string Signature
        {
            get
            {
                string str = "";

                str += name;
                str += "(";
                for (int i = 0; i < parameters.Count; ++i)
                {
                    if (i > 0) { str += ", "; }
                    str += parameters[i];
                }
                str += ")";

                return str;
            }
        }
    }
    
    public static List<MethodDescriptor> GetMethodsList(string spellClass)
    {
        var list = new List<MethodDescriptor>();

        list.Add(new MethodDescriptor
        {
            name = "TryFindTarget",
            parameters = new List<string> { "wizard" },
        });
        list.Add(new MethodDescriptor
        {
            name = "OnTargetLost",
            parameters = new List<string>(),
        });
        list.Add(new MethodDescriptor
        {
            name = "OnFocusLost",
            parameters = new List<string>(),
        });

        switch (spellClass)
        {
            case "InstantSpell":
                list.Add(new MethodDescriptor
                {
                    name = "Cast",
                    parameters = new List<string>(),
                });
                break;
            case "ContinuousSpell":
                list.Add(new MethodDescriptor
                {
                    name = "OnBegin",
                    parameters = new List<string>(),
                });
                list.Add(new MethodDescriptor
                {
                    name = "Activate",
                    parameters = new List<string> { "deltaTime" },
                });
                list.Add(new MethodDescriptor
                {
                    name = "OnFinish",
                    parameters = new List<string>(),
                });
                break;
            case "ToggleSpell":
                list.Add(new MethodDescriptor
                {
                    name = "Activate",
                    parameters = new List<string> { "deltaTime" },
                });
                list.Add(new MethodDescriptor
                {
                    name = "OnToggle",
                    parameters = new List<string> { "active" },
                });
                break;
            case "StagedSpell":
                list.Add(new MethodDescriptor
                {
                    name = "OnBegin",
                    parameters = new List<string>(),
                });
                list.Add(new MethodDescriptor
                {
                    name = "Cast",
                    parameters = new List<string> { "deltaTime" },
                });
                list.Add(new MethodDescriptor
                {
                    name = "Execute",
                    parameters = new List<string> { "deltaTime" },
                });
                list.Add(new MethodDescriptor
                {
                    name = "OnFinish",
                    parameters = new List<string>(),
                });
                break;
        }

        return list;
    }

    public static Dictionary<string, ScriptSpellInput> Spells = new Dictionary<string, ScriptSpellInput>();
    public static List<SpellDescriptor> SpellDescriptors = new List<SpellDescriptor>();
}
