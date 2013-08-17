/*
 * gauche_dotnet.c
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

#include "gauche_dotnet.h"
#include "dotnet_type.gen.h"

static void Scm_finalize_TypedClosure(ScmObj obj, void* data){
  ScmTypedClosure* c = SCM_TYPED_CLOSURE(obj);
  int i;

  for(i = 0;i < c->numArgTypeSpec;++i) {
    ClrFreeTypeHandle(c->argTypeAry[i]);
  }
  c->numArgTypeSpec = 0;
  c->argTypeAry = 0;

  for(i = 0;i < c->numRetTypeSpec;++i) {
    ClrFreeTypeHandle(c->retTypeAry[i]);
  }
  c->numRetTypeSpec = 0;
  c->retTypeAry = 0;
}

ScmObj Scm_MakeTypedClosure(ScmClosure* closure
    ,int numArgTypeSpec, TypeSpec* typeSpecAry
    ,int numRetTypeSpec, TypeSpec* retSpecAry
    )
{
  ScmTypedClosure* c = SCM_NEW(ScmTypedClosure);
  int i;

  memcpy(c, closure, sizeof(ScmClosure));

  c->numArgTypeSpec = numArgTypeSpec;
  c->argTypeAry = SCM_NEW_ARRAY(void*, numArgTypeSpec);
  for(i = 0;i < numArgTypeSpec;++i) {
    c->argTypeAry[i] = ClrTypeSpecToTypeHandle(&(typeSpecAry[i]));
  }

  c->numRetTypeSpec = numRetTypeSpec;
  c->retTypeAry = SCM_NEW_ARRAY(void*, numRetTypeSpec);
  for(i = 0;i < numRetTypeSpec;++i) {
    c->retTypeAry[i] = ClrTypeSpecToTypeHandle(&(retSpecAry[i]));
  }

  Scm_RegisterFinalizer(SCM_OBJ(c), Scm_finalize_TypedClosure, NULL);

  return SCM_OBJ(c);
}

/*
 * Module initialization function.
 */
extern void Scm_Init_dotnetlib(ScmModule*);
extern void Scm_Init_dotnet_type(ScmModule*);
void Scm_Init_gauche_dotnet(void)
{
				ScmModule *mod;

				/* Register this DSO to Gauche */
				SCM_INIT_EXTENSION(gauche_dotnet);

				/* Create the module if it doesn't exist yet. */
				mod = SCM_MODULE(SCM_FIND_MODULE("dotnet", TRUE));
				
				/* Register stub-generated procedures */
				Scm_Init_dotnetlib(mod);
				Scm_Init_dotnet_type(mod);
}
