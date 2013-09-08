using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GaucheDotNet.Native;

namespace GaucheDotNet
{
    public static class RuntimeExtension
    {
        public static GoshObj Eval(this object expr)
        {
            return EvalWithModule(expr, Gosh.UserModule());
        }

        public static GoshObj EvalWithModule(this object expr, GoshModule module)
        {
            GoshEvalPacket packet = new GoshEvalPacket();
            if (Gosh.Eval(expr, module, packet) < 0)
            {
                GoshCondition condition = packet.Exception;
                Exception e = Gosh.ClrConditionInnerException(condition);
                if (e == null)
                {
                    e = new GoshException(condition.ToString());
                }
                throw e;
            }
            else
            {
                //TODO multiple value
                return packet[0];
            }
        }

        public static GoshObj Eval(this string expr, params object[] args)
        {
            return EvalWithModule(expr, Gosh.UserModule(), args);
        }

        static readonly Regex INDEXREPLACE = new Regex("{(?<index>\\d+)}", RegexOptions.Compiled);

        public static GoshObj EvalWithModule(this string expr, GoshModule module, params object[] args)
        {
            Guid[] replacements = new Guid[args.Length];
            GoshSymbol[] symbols = new GoshSymbol[args.Length];

            expr = INDEXREPLACE.Replace(expr, (m) =>
                {
                    int index = Convert.ToInt32(m.Groups["index"].Value);
                    Guid guid = replacements[index];
                    if (guid == Guid.Empty)
                    {
                        guid = Guid.NewGuid();
                        replacements[index] = guid;
                    }
                    return guid.ToString();
                });

            try
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    Guid guid = replacements[i];
                    if (guid != Guid.Empty)
                    {
                        symbols[i] = Gosh.MakeSymbol(guid.ToString(), true);
                        Gosh.Define(module, symbols[i], args[i]);
                    }
                }

                GoshEvalPacket packet = new GoshEvalPacket();
                if (Gosh.EvalString(expr, module, packet) < 0)
                {
                    GoshCondition condition = packet.Exception;
                    Exception e = Gosh.ClrConditionInnerException(condition);
                    if (e == null)
                    {
                        e = new GoshException(condition.ToString());
                    }
                    throw e;
                }
                else
                {
                    //TODO multiple value
                    return packet[0];
                }
            }
            finally
            {
#if HAS_DELETE_BINDING
                for (int i = 0; i < args.Length; ++i)
                {
                    Guid guid = replacements[i];
                    if (guid != Guid.Empty)
                    {
                        Gosh.DeleteBinding(module, symbols[i], BindingFlag.None);
                    }
                }
#endif
            }
        }

    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ExtensionAttribute : Attribute { }
}
