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

#include"dotnet_type.gen.h"

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

SCM_EXTERN int Scm_ExtendedPairP(ScmObj obj)
{
  return SCM_EXTENDED_PAIR_P(obj);
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

