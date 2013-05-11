using System;
using System.Collections.Generic;
using System.Text;
using GaucheDotNet;

namespace Example
{
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

        private static void EvalStringInUser(String exp)
        {
            GoshEvalPacket packet = new GoshEvalPacket();

            if (Gosh.EvalString(exp, Gosh.UserModule(), packet) < 0)
            {
                GoshCondition exception = packet.Exception;
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.ConditionTypeName);
            }
            else
            {
                Console.WriteLine(packet[0].Object);
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
            EvalStringInUser("(clr-reference 'System.Windows.Forms)");

            //register namespace shortcut
            EvalStringInUser("(clr-using 'System.Windows.Forms)");

            //type resolve test
            EvalStringInUser("(clr-resolve-type 'Form)");

            //create Form instance
            EvalStringInUser("(define form (clr-new 'Form))");

            //show and run the Form
            EvalStringInUser("(clr-call 'Run 'Application form)");

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
            EvalStringInUser("(+ 1 2 (clr->int (clr-prop-get hoge 'Num)))");

            //set property
            GoshSymbol symNum = Gosh.Intern("num");
            Gosh.Define(user, symNum, 1000);
            EvalStringInUser("(clr-prop-set! hoge 'Num num)");

            EvalStringInUser("(+ 1 2 (clr->int (clr-prop-get hoge 'Num)))");

            //get field
            EvalStringInUser("(clr-field-get hoge 'str)");

            //set field
            EvalStringInUser("(clr-field-set! hoge 'str \"Foo\")");
            EvalStringInUser("(clr-field-get hoge 'str)");

            //add event
            EvalStringInUser("(define ev (lambda (num str) (print (clr->int num))  (print (clr->string str)) 3.14))");
            EvalStringInUser("(clr-event-add! hoge 'Event ev)");
            EvalStringInUser("(clr-event-add! hoge 'Event (lambda (num str) (print \"second\") (*  2 3.14)))");
            Console.WriteLine(hoge.eventTest(10, "hohoho"));
            //remove event
            EvalStringInUser("(clr-event-remove! hoge 'Event ev)");
            Console.WriteLine(hoge.eventTest(10, "hohoho"));

            #endregion

            #region create generic instance

            Console.WriteLine("\n\ncreate generic instance");

            //create List<int> object
            EvalStringInUser("(clr-using 'System.Collections.Generic)");
            EvalStringInUser("(define l (clr-new '(List int)))");

            //add three element
            EvalStringInUser("(clr-call 'Add l 1)");
            EvalStringInUser("(clr-call 'Add l 2)");
            EvalStringInUser("(clr-call 'Add l 3)");

            //get Count property
            EvalStringInUser("(clr-prop-get l 'Count)");
            // get element at index
            EvalStringInUser("(clr-prop-get l 0)");

            //create Dictionary object
            EvalStringInUser("(define dict (clr-new '(Dictionary string int)))");
            //add three key/value
            EvalStringInUser("(clr-prop-set! dict \"Zero\" 0)");
            EvalStringInUser("(clr-prop-set! dict \"One\" 1)");
            EvalStringInUser("(clr-prop-set! dict \"Two\" 2)");

            //get Count property
            EvalStringInUser("(clr-prop-get dict 'Count)");
            // get element at key
            EvalStringInUser("(clr-prop-get dict \"Two\")");

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
            EvalStringInUser("(clr-mul 2)");

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