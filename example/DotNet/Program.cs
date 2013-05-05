using System;
using System.Collections.Generic;
using System.Text;
using GaucheDotNet;

namespace Example
{
	class Program
	{
        private class Hoge
        {
            public short Num { get; set; }
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

            //GoshString str = Gosh.MakeString("あいうえお");//Gauche.MakeFillString(3, 'あ');
            //GoshString str = Gosh.MakeFillString(3, 'あ');
            //Console.WriteLine(str.ToString());
            //Gosh.Printf(str.Ptr);

            GoshModule user = Gosh.UserModule();
            GoshEvalPacket packet = new GoshEvalPacket();

			#region show Windows Form

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

			Hoge hoge = new Hoge();
            hoge.Num = 300;
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

			#endregion

			#region create generic instance

			//create List<int> object
			EvalStringInUser("(clr-using 'System.Collections.Generic)");
			EvalStringInUser("(define l (clr-new '(List int)))");

			//add three element
			EvalStringInUser("(clr-call 'Add l 1)");
			EvalStringInUser("(clr-call 'Add l 2)");
			EvalStringInUser("(clr-call 'Add l 3)");

			//get Count property
			EvalStringInUser("(clr-prop-get l 'Count)");

			#endregion

			#region operations on function

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
            if(Gosh.Eval(Gosh.List(proc), user, packet) < 0)
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
