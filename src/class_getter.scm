;;;
;;; class_getter.scm
;;;
;;; MIT License
;;; Copyright 2013 aharisu
;;; All rights reserved.
;;;
;;; Permission is hereby granted, free of charge, to any person obtaining a copy
;;; of this software and associated documentation files (the "Software"), to deal
;;; in the Software without restriction, including without limitation the rights
;;; to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
;;; copies of the Software, and to permit persons to whom the Software is
;;; furnished to do so, subject to the following conditions:
;;;
;;; The above copyright notice and this permission notice shall be included in all
;;; copies or substantial portions of the Software.
;;;
;;;
;;; THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
;;; IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
;;; FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
;;; AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
;;; LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
;;; OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
;;; SOFTWARE.
;;;
;;;
;;; aharisu
;;; foo.yobina@gmail.com
;;;


(define classes 
  '(
  Scm_IntegerClass
  Scm_RealClass
  Scm_PairClass
  Scm_NullClass
  Scm_BoolClass
  Scm_SymbolClass
  Scm_StringClass
  Scm_ClrObjectClass
  Scm_ComplexClass
  Scm_RationalClass
  Scm_CharClass
  Scm_BoxClass
  Scm_EOFObjectClass
  Scm_UndefinedObjectClass
  Scm_TopClass
  Scm_KeywordClass
  Scm_HashTableClass
  Scm_VectorClass
  Scm_UVectorClass
  Scm_S8VectorClass
  Scm_U8VectorClass
  Scm_S16VectorClass
  Scm_U16VectorClass
  Scm_S32VectorClass
  Scm_U32VectorClass
  Scm_S64VectorClass
  Scm_U64VectorClass
  Scm_F16VectorClass
  Scm_F32VectorClass
  Scm_F64VectorClass
  ;Scm_LazyPairClass
  ;Scm_ListClass
  ;Scm_BottomClass
  ;Scm_ClassClass
  ;Scm_UnknownClass
  ;Scm_ObjectClass
  ;Scm_ForeignPointerClass
  ;Scm_ProcedureClass
  ;Scm_GenericClass
  ;Scm_MethodClass
  ;Scm_SyntaxClass
  ;Scm_NextMethodClass
  ;Scm_MacroClass
  ;Scm_PromiseClass
  ;Scm_RegexpClass
  ;Scm_RegMatchClass
  ;Scm_SysSigsetClass
  ;Scm_PortClass
  ;Scm_CodingAwarePortClass
  ;Scm_LimitedLengthPortClass
  ;Scm_WeakVectorClass
  ;Scm_WeakHashTableClass
  ;Scm_ReadContextClass
  ;Scm_ReadReferenceClass
  ;Scm_StringPointerClass
  ;Scm_SyntacticClosureClass
  ;Scm_IdentifierClass
  ;Scm_VMClass
  ;Scm_ModuleClass
  ;Scm_ConditionClass
  ;Scm_MessageConditionClass
  ;Scm_SeriousConditionClass
  ;Scm_ErrorClass
  ;Scm_SystemErrorClass
  ;Scm_UnhandledSignalErrorClass
  ;Scm_ReadErrorClass
  ;Scm_IOErrorClass
  ;Scm_PortErrorClass
  ;Scm_IOReadErrorClass
  ;Scm_IOWriteErrorClass
  ;Scm_IOClosedErrorClass
  ;Scm_IOUnitErrorClass
  ;Scm_CompoundConditionClass
  ;Scm_SeriousCompoundConditionClass
  ;Scm_ThreadExceptionClass
  ;Scm_JoinTimeoutExceptionClass
  ;Scm_AbandonedMutexExceptionClass
  ;Scm_TerminatedThreadExceptionClass
  ;Scm_UncaughtExceptionClass
  ;Scm_CollectionClass
  ;Scm_SequenceClass
  ;Scm_DictionaryClass
  ;Scm_OrderedDictionaryClass
  ;Scm_TreeMapClass
  ;Scm_AutoloadClass
  ;Scm_SysStatClass
  ;Scm_TimeClass
  ;Scm_SysTmClass
  ;Scm_SysGroupClass
  ;Scm_SysPasswdClass
  ;Scm_SysFdsetClass
  ;Scm_CharSetClass
  ;Scm_GlocClass
))

(use gauche.sequence)

(define-constant offset 6)

(define (output-class-getter index class)
  (print "else if(SCM_EQ(klass, &" class ")) return " (+ index offset) ";")
  )

(define (trim-scm-class class)
  (let* ([class (x->string class)]
         [class (substring class 4 (string-length class))]
         [class (substring class 0 (- (string-length class) 5))])
    class))

(define (main args)
  (with-output-to-file 
    "class_getter.c"
    (lambda ()
      (print
"
//Auto generation code. Do not edit.
#define LIBGAUCHE_EXT_BODY
#include<gauche.h>
#include\"dotnet_type.gen.h\"
SCM_EXTERN int Scm_IsKnownType(ScmObj obj)
{
  ScmClass* klass = Scm_ClassOf(obj);

  if (SCM_CLASS_APPLICABLE_P(klass))
  {
    if (SCM_PROCEDURE_TYPE(obj) == SCM_PROC_CLOSURE) return 0;
    else if (SCM_PROCEDURE_TYPE(obj) == SCM_PROC_SUBR) return 1;
    else if (SCM_PROCEDURE_TYPE(obj) == SCM_PROC_METHOD) return 2;
    else if (SCM_PROCEDURE_TYPE(obj) == SCM_PROC_GENERIC) return 3;
    else if (SCM_PROCEDURE_TYPE(obj) == SCM_PROC_NEXT_METHOD) return 4;
  }
  else if(SCM_CONDITIONP(obj))
  {
    return 5;
  }
")
      (for-each-with-index
        output-class-getter
        classes)
      (print "  return -1;")
      (print "}")
      ))

  (with-output-to-file 
    "ClassType.cs"
    (lambda ()
      (print 
"
//Auto generation code. Do not edit.
namespace GaucheDotNet
{
  public static partial class Cast
  {
    public enum KnownClass : int
    {
      Unknown = -1,
      Closure = 0,
      Subr = 1,
      Method = 2,
      Generic = 3,
      NextMethod = 4,
      Condition = 5,")
      (for-each-with-index
        (lambda (index class)
          (print "      " (trim-scm-class class) " = " (+ index offset) ","))
        classes)
      (print
"    }
  }
}
")
  )))

