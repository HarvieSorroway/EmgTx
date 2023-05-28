using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModFixerTx
{
    public class ModFixerTx
    {
        public bool hookOn;
        public static List<ModFixerTx> modHookBases = new List<ModFixerTx>();

        public string _assembly;
        public string _namespace;
        public string _id;

        Assembly assembly;
        public Assembly Assembly
        {
            get
            {
                if (assembly == null)
                {
                    foreach (var mod in ModManager.ActiveMods)
                    {
                        if (mod.id == _id)
                        {
                            string path = mod.path + Path.DirectorySeparatorChar + "plugins";
                            string assemblyPath = "";
                            foreach (var dllPath in Directory.GetFiles(path))
                            {
                                if (dllPath.EndsWith(".dll") && dllPath.ToLower().Contains(_assembly.ToLower()))
                                {
                                    assemblyPath = dllPath;
                                    break;
                                }
                            }

                            assembly = Assembly.LoadFrom(assemblyPath);
                        }
                    }
                }

                return assembly;
            }
        }
        public ModFixerTx(string _assembly, string _namespace, string _id)
        {
            this._assembly = _assembly;
            this._namespace = _namespace;
            this._id = _id;
        }

        public virtual void Apply()
        {
        }

        public Type GetClass(string className)
        {
            return assembly.GetType(className);
        }

        public T GetStaticFieldValue<T>(Type type, string memberName, BindingFlags bindingFlags)
        {
            return (T)type.GetField(memberName, bindingFlags).GetValue(null);
        }
    }
}
