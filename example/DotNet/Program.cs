using System;
using System.Collections.Generic;
using System.Text;
using GaucheDotNet;

namespace Example
{
    static class Ext
    {
        public static void WriteLine(this GoshObj obj)
        {
            Console.WriteLine(obj.Object);
        }
    }

    public delegate double EventTest(int num, String str);

    class Program
    {
        private class Hoge
        {
            public short Num { get; set; }
            public String str;

            public EventTest eventTest;
            public event EventTest Event
            {
                add { eventTest += value; }
                remove { eventTest -= value; }
            }
        }

        static void Main(string[] args)
        {
            Gosh.Initialize();

            GoshModule user = Gosh.UserModule();
            GoshEvalPacket packet = new GoshEvalPacket();

            #region show Windows Form

            Console.WriteLine("show windows form...");
            //load assembly
            "(clr-reference 'System.Windows.Forms)".Eval().WriteLine();

            //register namespace shortcut
            "(clr-using 'System.Windows.Forms)".Eval().WriteLine();

            //type resolve test
            "(clr-resolve-type 'Form)".Eval().WriteLine();

            //create Form instance
            "(define form (clr-new 'Form))".Eval().WriteLine();

            //show and run the Form
            "(clr-call 'Run 'Application form)".Eval().WriteLine();

            #endregion

            #region access .Net Object

            Console.WriteLine("\n\naccess .Net Object");

            Hoge hoge = new Hoge();
            hoge.Num = 300;
            hoge.str = "Hoge";
            //define Hoge object
            GoshSymbol symHoge = Gosh.Intern("hoge");
            Gosh.Define(user, symHoge, hoge);

            //get property
            "(+ 1 2 (clr->int (clr-prop-get hoge 'Num)))".Eval().WriteLine();

            //set property
            GoshSymbol symNum = Gosh.Intern("num");
            Gosh.Define(user, symNum, 1000);
            "(clr-prop-set! hoge 'Num num)".Eval().WriteLine();

            "(+ 1 2 (clr->int (clr-prop-get hoge 'Num)))".Eval().WriteLine();

            //get field
            "(clr-field-get hoge 'str)".Eval().WriteLine();

            //set field
            "(clr-field-set! hoge 'str \"Foo\")".Eval().WriteLine();
            "(clr-field-get hoge 'str)".Eval().WriteLine();

            //add event
            "(define ev (lambda (num str) (print (clr->int num))  (print (clr->string str)) 3.14))".Eval().WriteLine();
            "(clr-event-add! hoge 'Event ev)".Eval().WriteLine();
            "(clr-event-add! hoge 'Event (lambda (num str) (print \"second\") (*  2 3.14)))".Eval().WriteLine();
            Console.WriteLine(hoge.eventTest(10, "hohoho"));
            //remove event
            "(clr-event-remove! hoge 'Event ev)".Eval().WriteLine();
            Console.WriteLine(hoge.eventTest(10, "hohoho"));

            #endregion

            #region create generic instance

            Console.WriteLine("\n\ncreate generic instance");

            //create List<int> object
            "(clr-using 'System.Collections.Generic)".Eval().WriteLine();
            "(define l (clr-new '(List int)))".Eval().WriteLine();

            //add three element
            "(clr-call 'Add l 1)".Eval().WriteLine();
            "(clr-call 'Add l 2)".Eval().WriteLine();
            "(clr-call 'Add l 3)".Eval().WriteLine();

            //get Count property
            "(clr-prop-get l 'Count)".Eval().WriteLine();
            // get element at index
            "(clr-prop-get l 0)".Eval().WriteLine();

            //create Dictionary object
            "(define dict (clr-new '(Dictionary string int)))".Eval().WriteLine();
            //add three key/value
            "(clr-prop-set! dict \"Zero\" 0)".Eval().WriteLine();
            "(clr-prop-set! dict \"One\" 1)".Eval().WriteLine();
            "(clr-prop-set! dict \"Two\" 2)".Eval().WriteLine();

            //get Count property
            "(clr-prop-get dict 'Count)".Eval().WriteLine();
            // get element at key
            "(clr-prop-get dict \"Two\")".Eval().WriteLine();

            #endregion

            #region operations on function
            Console.WriteLine("\n\noperations on function");

            //construct expression (+ 1 2 3 4) 
            GoshSymbol symAdd = Gosh.Intern("+");
            GoshObj add = Gosh.List(symAdd, 1, 2, 3, 4);
            //eval expression
            if (Gosh.Eval(add, user, packet) < 0)
            {
                GoshCondition exception = packet.Exception;
                Console.WriteLine(exception.ConditionTypeName);
                Console.WriteLine(exception.Message);
            }
            else
            {
                Console.WriteLine(packet[0].Object);
            }

            //get + function
            if (Gosh.Eval(symAdd, user, packet) < 0)
            {
                GoshCondition exception = packet.Exception;
                Console.WriteLine(exception.ConditionTypeName);
                Console.WriteLine(exception.Message);
            }
            else
            {
                //Gauche function to .Net function object
                GoshProc a = packet[0].To<GoshProc>();
                //apply function
                Console.WriteLine(a.Apply(1, 2, 3));
            }

            //create Gauche  function from .Net function
            GoshProc proc = Gosh.MakeSubr(() => 100);
            //eval function
            if (Gosh.Eval(Gosh.List(proc), user, packet) < 0)
            {
                GoshCondition exception = packet.Exception;
                Console.WriteLine(exception.ConditionTypeName);
                Console.WriteLine(exception.Message);
            }
            else
            {
                Console.WriteLine(packet[0].Object);
            }

            //create function that take argument
            GoshSymbol symMul = Gosh.Intern("clr-mul");
            Gosh.Define(user, symMul, Gosh.MakeSubr((int num) => 10 * num));
            "(clr-mul 2)".Eval().WriteLine();

            //get clr-mull function
            if (Gosh.EvalString("clr-mul", user, packet) < 0)
            {
                GoshCondition exception = packet.Exception;
                Console.WriteLine(exception.ConditionTypeName);
                Console.WriteLine(exception.Message);
            }
            else
            {
                //to .Net function object
                GoshProc pp = packet[0].To<GoshProc>();
                //apply
                Console.WriteLine(pp.Apply(30));
            }

            #endregion

        }

    }
}