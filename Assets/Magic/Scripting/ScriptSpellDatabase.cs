using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

[Serializable]
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
        
        str.AppendFormat("Class.{0} = {{\n", className);
        str.AppendFormat("\t__inherit = '{0}',\n", scriptBaseClass);
        str.AppendFormat("\t\n");
        foreach (var variable in variables)
        {
            str.AppendFormat("\t{0} = {1},\n", variable.Key, variable.Value);
        }
        str.AppendFormat("}}\n\n");

        var classMethods = ScriptSpellDatabase.GetMethodsList(scriptBaseClass);
        foreach (var method in methods)
        {
            var descriptor = classMethods.Find(m => m.name == method.Key);
            if (descriptor != null)
            {
                str.AppendFormat("function {0}:{1}\n", className, descriptor.signature);
            }
            else
            {
                MagicLog.LogErrorFormat("Unknown method {0} for script spell {1}", method.Key, id);
                if (!string.IsNullOrEmpty(method.Key))
                {
                    str.AppendFormat("function {0}:{1}(...)\n", className, method.Key);
                }
            }

            if (!string.IsNullOrEmpty(method.Key))
            {
                foreach (var line in method.Value.Split('\n'))
                {
                    str.AppendFormat("\t{0}\n", line);
                }
                str.AppendFormat("end\n\n");
            }
        }

        return str.ToString();
    }
}

[Serializable]
class SerializableSpellDescriptor
{
    public Guid guid;
    public string id;
    public string spellClass;
    public SpellDescriptor.SpellTargetType targetType;
    public bool targetRequired;

    public int paramLevel;
    public Energy.Element paramElement;
    public Energy.Shape paramShape;
    //public UnityEngine.Object paramObj;
    public StatusEffect.Type paramStatusEffect;
    public int paramInteger;
    public float paramReal;

    public string displayName;
    //public Sprite icon;
    public bool visible = true;

    public string spellScriptClass;

    public bool isScriptSpell { get { return !string.IsNullOrEmpty(spellScriptClass); } }

    public void ReadFrom(SpellDescriptor descriptor)
    {
        guid = descriptor.guid;
        id = descriptor.id;
        spellClass = descriptor.spellClass;
        targetType = descriptor.targetType;
        targetRequired = descriptor.targetRequired;

        paramLevel = descriptor.parameters.level;
        paramElement = descriptor.parameters.element;
        paramShape = descriptor.parameters.shape;
        //paramObj = descriptor.parameters.obj;
        paramStatusEffect = descriptor.parameters.statusEffect;
        paramInteger = descriptor.parameters.integer;
        paramReal = descriptor.parameters.real;

        displayName = descriptor.displayName;
        //icon = descriptor.icon;
        visible = descriptor.visible;

        if (descriptor is ScriptSpellDescriptor)
        {
            spellScriptClass = (descriptor as ScriptSpellDescriptor).spellScriptClass;
        }
    }

    public void WriteTo(SpellDescriptor descriptor)
    {
        descriptor.guid = guid;
        descriptor.id = id;
        descriptor.name = id;
        descriptor.spellClass = spellClass;
        descriptor.targetType = targetType;
        descriptor.targetRequired = targetRequired;

        descriptor.parameters = new SpellParameters();
        descriptor.parameters.level = paramLevel;
        descriptor.parameters.element = paramElement;
        descriptor.parameters.shape = paramShape;
        //descriptor.parameters.obj = paramObj;
        descriptor.parameters.statusEffect = paramStatusEffect;
        descriptor.parameters.integer = paramInteger;
        descriptor.parameters.real = paramReal;

        descriptor.displayName = displayName;
        //descriptor.icon = icon;
        descriptor.visible = visible;

        if (descriptor is ScriptSpellDescriptor)
        {
            (descriptor as ScriptSpellDescriptor).spellScriptClass = spellScriptClass;
        }
    }
}


public static class ScriptSpellDatabase
{
    public class MethodDescriptor
    {
        public string name;
        public List<string> parameters;

        public string signature
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

    public static string spellsPath { get { return Application.persistentDataPath + "/Spells/"; } }

    public static List<MethodDescriptor> GetMethodsList(string scriptSpellClass)
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

        switch (scriptSpellClass)
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

    static ScriptSpellDatabase()
    {
        if (Directory.Exists(spellsPath))
        {
            var bf = new BinaryFormatter();
            var allFiles = Directory.GetFiles(spellsPath, "*", SearchOption.TopDirectoryOnly);
            foreach (var filePath in allFiles)
            {
                var file = File.Open(filePath, FileMode.Open);
                if (filePath.EndsWith(".spellInput"))
                {
                    var input = (ScriptSpellInput)bf.Deserialize(file);
                    spells.Add(input.id, input);
                }
                else if (filePath.EndsWith(".spellDescriptor"))
                {
                    var serializableDescriptor = (SerializableSpellDescriptor)bf.Deserialize(file);

                    SpellDescriptor descriptor = null;
                    if (serializableDescriptor.isScriptSpell)
                    {
                        descriptor = ScriptableObject.CreateInstance<ScriptSpellDescriptor>();
                    }
                    else
                    {
                        descriptor = ScriptableObject.CreateInstance<SpellDescriptor>();
                    }
                    serializableDescriptor.WriteTo(descriptor);

                    spellDescriptors.Add(descriptor);
                }
                file.Close();
            }
        }
    }

    public static void DoNothing() { } //Initialized using this

    public static void AddSpellImplementation(ScriptSpellInput input)
    {
        spells.Add(input.id, input);
        SaveSpellImplementation(input);
    }

    public static void SaveSpellImplementation(ScriptSpellInput input)
    {
        var bf = new BinaryFormatter();
        Directory.CreateDirectory(spellsPath);
        var file = File.OpenWrite(spellsPath + input.id + ".spellInput");
        bf.Serialize(file, input);
        file.Close();
    }

    public static void RemoveSpellImplementation(string id)
    {
        if (spells.Remove(id))
        {
            File.Delete(spellsPath + id + ".spellInput");
        }
    }

    public static void AddSpellDescriptor(SpellDescriptor descriptor)
    {
        spellDescriptors.Add(descriptor);

        var bf = new BinaryFormatter();
        Directory.CreateDirectory(spellsPath);
        var file = File.Create(spellsPath + descriptor.id + descriptor.parameters.level + ".spellDescriptor");
        var serializableDescriptor = new SerializableSpellDescriptor();
        serializableDescriptor.ReadFrom(descriptor);
        bf.Serialize(file, serializableDescriptor);
        file.Close();
    }

    public static void RemoveSpellDescriptor(string id)
    {
        var descriptor = spellDescriptors.Find(sd => sd.id == id);
        if (descriptor == null)
        {
            return;
        }

        if (spellDescriptors.Remove(descriptor))
        {
            File.Delete(spellsPath + descriptor.id + descriptor.parameters.level + ".spellDescriptor");
        }
    }
    
    public static Dictionary<string, ScriptSpellInput> spells = new Dictionary<string, ScriptSpellInput>();
    public static List<SpellDescriptor> spellDescriptors = new List<SpellDescriptor>();
}
