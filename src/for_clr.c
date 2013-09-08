/*
 * for_clr.c
 *
 * MIT License
 * Copyright 2013 aharisu
 * All rights reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 *
 * aharisu
 * foo.yobina@gmail.com
 */

#define LIBGAUCHE_EXT_BODY
#include<gauche.h>
#include <fcntl.h>		/* for _O_BINMODE on windows. */

#include"gauche_dotnet.h"
#include"dotnet_type.gen.h"

SCM_EXTERN void Scm_InstallErrorHandler(ScmObj ehandler)
{
  Scm_VM()->exceptionHandler = ehandler;
}

SCM_EXTERN ClrObject Scm_ClrConditionInnerException(ScmObj obj)
{
  if(SCM_ISA(obj, SCM_CLASS_CLR_ERROR))
  {
    return ((ScmClrObject*)((ScmClrError*)obj)->clrException)->data;
  }
  else
  {
    return NULL;
  }
}

SCM_EXTERN int Scm_TypedClosureP(ScmObj obj)
{
  return SCM_TYPED_CLOSURE_P(obj);
}

SCM_EXTERN int Scm_TypedClosureSkipCheckClosureP(ScmObj obj)
{
  return SCM_TYPED_CLOSURE_SKIP_CHECK_CLOSURE_P(obj);
}

SCM_EXTERN int Scm_ListP(ScmObj obj)
{
  return SCM_LISTP(obj);
}

SCM_EXTERN int Scm_PairP(ScmObj obj)
{
  return SCM_PAIRP(obj);
}

SCM_EXTERN int Scm_NullP(ScmObj obj)
{
  return SCM_NULLP(obj);
}

SCM_EXTERN int Scm_StringP(ScmObj obj)
{
  return SCM_STRINGP(obj);
}

SCM_EXTERN int Scm_KeywordP(ScmObj obj)
{
  return SCM_KEYWORDP(obj);
}

SCM_EXTERN int Scm_SymbolP(ScmObj obj)
{
  return SCM_SYMBOLP(obj);
}

SCM_EXTERN int Scm_ExtendedPairP(ScmObj obj)
{
  return SCM_EXTENDED_PAIR_P(obj);
}

SCM_EXTERN int Scm_HashTableP(ScmObj obj)
{
  return SCM_HASH_TABLE_P(obj);
}

SCM_EXTERN int Scm_VMP(ScmObj obj)
{
  return SCM_VMP(obj);
}

SCM_EXTERN int Scm_PortP(ScmObj obj)
{
  return SCM_PORTP(obj);
}

SCM_EXTERN int Scm_BignumP(ScmObj obj)
{
  return SCM_BIGNUMP(obj);
}

SCM_EXTERN int Scm_VectorP(ScmObj obj)
{
  return SCM_VECTORP(obj);
}

SCM_EXTERN int Scm_UVectorP(ScmObj obj)
{
  return SCM_UVECTORP(obj);
}

SCM_EXTERN ScmClass* Scm_GetUVectorClass(int type)
{
  switch(type)
  {
    case SCM_UVECTOR_S8: return SCM_CLASS_S8VECTOR;
    case SCM_UVECTOR_U8: return SCM_CLASS_S8VECTOR;
    case SCM_UVECTOR_S16: return SCM_CLASS_S8VECTOR;
    case SCM_UVECTOR_U16: return SCM_CLASS_S8VECTOR;
    case SCM_UVECTOR_S32: return SCM_CLASS_S8VECTOR;
    case SCM_UVECTOR_U32: return SCM_CLASS_S8VECTOR;
    case SCM_UVECTOR_S64: return SCM_CLASS_S8VECTOR;
    case SCM_UVECTOR_U64: return SCM_CLASS_S8VECTOR;
    case SCM_UVECTOR_F16: return SCM_CLASS_S8VECTOR;
    case SCM_UVECTOR_F32: return SCM_CLASS_S8VECTOR;
    case SCM_UVECTOR_F64: return SCM_CLASS_S8VECTOR;
    default: return NULL;
  }
}

SCM_EXTERN int Scm_UVectorLength(ScmUVector* vec)
{
  return SCM_UVECTOR_SIZE(vec);
}

SCM_EXTERN void Scm_UVectorCopy(ScmUVector* srcVec, void* dest, int size)
{
  memcpy(dest, SCM_UVECTOR_ELEMENTS(srcVec), size);
}

SCM_EXTERN void Scm_UVectorCopyF16(ScmUVector* srcVec, double* dest, int len)
{
  int i = 0;
  for(;i < len;++i)
  {
    dest[i] = Scm_HalfToDouble(SCM_F16VECTOR_ELEMENT(srcVec, i));
  }
}

#define def_uvector_accessor(tag, type)  \
  SCM_EXTERN type SCM_CPP_CAT3(Scm_,tag,VectorRef)(ScmUVector* vec, int i) \
  { \
    return SCM_CPP_CAT3(SCM_, tag, VECTOR_ELEMENT)(vec, i); \
  } \
  SCM_EXTERN void SCM_CPP_CAT3(Scm_,tag,VectorSet)(ScmUVector* vec, int i, type val)  \
  { \
    SCM_CPP_CAT3(SCM_,tag, VECTOR_ELEMENT)(vec, i) = val; \
  } \

def_uvector_accessor(S8, signed char)
def_uvector_accessor(U8, char)
def_uvector_accessor(S16, short)
def_uvector_accessor(U16, unsigned short)
def_uvector_accessor(S32, int)
def_uvector_accessor(U32, unsigned int)
def_uvector_accessor(S64, ScmInt64)
def_uvector_accessor(U64, ScmUInt64)
def_uvector_accessor(F32, float)
def_uvector_accessor(F64, double)

SCM_EXTERN double Scm_F16VectorRef(ScmUVector* vec, int i)
{
  return Scm_HalfToDouble(SCM_F16VECTOR_ELEMENT(vec, i));
}

SCM_EXTERN void Scm_F16VectorSet(ScmUVector* vec, int i, double val)
{
  SCM_F16VECTOR_ELEMENT(vec, i) = Scm_DoubleToHalf(val);
}

/* Returns FALSE if the process doesn't have a console. */
static int init_console(void)
{
#if defined(GAUCHE_WINDOWS)
#  if defined(GAUCHE_WINDOWS_NOCONSOLE)
    close(0);               /* just in case */
    close(1);               /* ditto */
    close(2);               /* ditto */
    open("NUL", O_RDONLY);
    open("NUL", O_WRONLY);
    open("NUL", O_WRONLY);
    return FALSE;
#  else /*!defined(GAUCHE_WINDOWS_NOCONSOLE)*/
    /* This saves so much trouble */
    _setmode(_fileno(stdin),  _O_BINARY);
    _setmode(_fileno(stdout), _O_BINARY);
    _setmode(_fileno(stderr), _O_BINARY);
    return TRUE;
#  endif /*!defined(GAUCHE_WINDOS_NOCONSOLE)*/
#else  /*!defined(GAUCHE_WINDOWS)*/
    return TRUE;
#endif /*!defined(GAUCHE_WINDOWS)*/
}

/* signal handler setup.  let's catch as many signals as possible. */
static void sig_setup(void)
{
    sigset_t set;
    sigfillset(&set);
    sigdelset(&set, SIGABRT);
    sigdelset(&set, SIGILL);
#ifdef SIGKILL
    sigdelset(&set, SIGKILL);
#endif
#ifdef SIGCONT
    sigdelset(&set, SIGCONT);
#endif
#ifdef SIGSTOP
    sigdelset(&set, SIGSTOP);
#endif
    sigdelset(&set, SIGSEGV);
#ifdef SIGBUS
    sigdelset(&set, SIGBUS);
#endif /*SIGBUS*/
#if defined(GC_LINUX_THREADS)
    /* some signals are used in the system */
    sigdelset(&set, SIGPWR);  /* used in gc */
    sigdelset(&set, SIGXCPU); /* used in gc */
    sigdelset(&set, SIGUSR1); /* used in linux threads */
    sigdelset(&set, SIGUSR2); /* used in linux threads */
#endif /*GC_LINUX_THREADS*/
#if defined(GC_FREEBSD_THREADS)
    sigdelset(&set, SIGUSR1); /* used by GC to stop the world */
    sigdelset(&set, SIGUSR2); /* used by GC to restart the world */
#endif /*GC_FREEBSD_THREADS*/
    Scm_SetMasterSigmask(&set);
}

/* Load gauche-init.scm */
static void load_gauche_init(void)
{
    ScmLoadPacket lpak;
    if (Scm_Load("gauche-init.scm", 0, &lpak) < 0) {
        Scm_Printf(SCM_CURERR, "gosh: WARNING: Error while loading initialization file: %A(%A).\n",
                   Scm_ConditionMessage(lpak.exception),
                   Scm_ConditionTypeName(lpak.exception));
    }
    //extended module for gauche-dotnet
    if (Scm_Require(SCM_MAKE_STR("dotnet"), 0, &lpak) < 0) {
      Scm_Warn("couldn't load dotnet\n");
    } else {
      Scm_ImportModule(SCM_CURRENT_MODULE(),
          SCM_INTERN("dotnet"),
          SCM_FALSE, 0);
    }
}

SCM_EXTERN void GaucheDotNetInitialize()
{
  ScmLoadPacket lpak;
  int has_console;

  has_console = init_console();
  GC_INIT();
  Scm_Init(GAUCHE_SIGNATURE);
  sig_setup();

  Scm__SetupPortsForWindows(has_console);

  load_gauche_init();
}

