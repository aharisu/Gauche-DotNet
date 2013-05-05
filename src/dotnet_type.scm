;;;
;;; dotnet_type.scm
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

(add-load-path ".")
(load "cv_struct_generator")

(use file.util)

(define (main args)
  (gen-type (simplify-path (path-sans-extension (car args)))
            structs 
            foreign-pointer
            (lambda () ;;prologue
              (cgen-extern "//pre defined header")
              (cgen-extern "#include \"ClrBridge.h\"")
              (cgen-extern "typedef void* ClrObject;")
              (cgen-body "")
              )
            (lambda () ;;epilogue
              ;;generate clr condition type
              (cgen-extern "
                           typedef struct ScmClrErrorRec {
                           ScmError common;
                           ScmObj clrException;
                           }ScmClrError;
                           SCM_CLASS_DECL(Scm_ClrErrorClass);
                           #define SCM_CLASS_CLR_ERROR (&Scm_ClrErrorClass)
                           ")
              (cgen-body "
                         static void clr_condition_print(ScmObj obj, ScmPort* port, ScmWriteContext* ctx)
                         {
                          ScmClass* k = Scm_ClassOf(obj);
                          Scm_Printf(port,  \"#<%A \\\"%30.1A\\\">\",
                                            Scm__InternalClassName(k),
                                            SCM_ERROR_MESSAGE(obj));
                         }

                         static ScmObj clr_condition_allocate(ScmClass* klass, ScmObj initargs)
                         {
                          ScmClrError* e = SCM_ALLOCATE(ScmClrError, klass);
                          SCM_SET_CLASS(e, klass);
                          SCM_ERROR_MESSAGE(e) = SCM_FALSE;
                          e->clrException = NULL;
                          return SCM_OBJ(e);
                         }

                         static ScmClass* clr_condition_cpl[] = {
                          SCM_CLASS_STATIC_PTR(Scm_ErrorClass),
                          SCM_CLASS_STATIC_PTR(Scm_MessageConditionClass),
                          SCM_CLASS_STATIC_PTR(Scm_SeriousConditionClass),
                          SCM_CLASS_STATIC_PTR(Scm_ConditionClass),
                          SCM_CLASS_STATIC_PTR(Scm_TopClass),
                          NULL
                         }; 

                         SCM_DEFINE_BASE_CLASS(Scm_ClrErrorClass,
                                                ScmClrError,
                                                clr_condition_print, NULL, NULL,
                                                clr_condition_allocate, 
                                                clr_condition_cpl);

                         ScmObj Scm_MakeClrError(ScmObj message, ScmObj clrException)
                         {
                          ScmClrError* e =
                            (ScmClrError*)(clr_condition_allocate(SCM_CLASS_CLR_ERROR, SCM_NIL));
                          SCM_ERROR_MESSAGE(e) = message;
                          e->clrException = clrException;
                          return SCM_OBJ(e);
                         }
                         ")
              (cgen-init "
                         Scm_InitStaticClassWithMeta(SCM_CLASS_CLR_ERROR,
                                                      \"<clr-error>\",
                                                      mod,
                                                      Scm_ClassOf(SCM_OBJ(SCM_CLASS_CONDITION)),
                                                      SCM_FALSE,
                                                      NULL, 0);
                         ")
              ))
              0)


;;sym-name sym-scm-type pointer? finalize-name finalize-ref
(define structs 
  '(
    (ClrObject <clr-object> #f "ReleaseClrObject" "")
    ))

;;sym-name sym-scm-type pointer? finalize finalize-ref 
(define foreign-pointer 
  '(
    ))
