/*
 * ClrMethodCallStruct.h
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


#ifndef CLRMETHODCALLSTRUCT_H
#define CLRMETHODCALLSTRUCT_H

#ifdef __cplusplus
extern "C" {
#endif

typedef enum {
    TYPESPEC_ATTR_NORMAL,
    TYPESPEC_ATTR_REF,
    TYPESPEC_ATTR_OUT,
    TYPESPEC_ATTR_PARAMS,
    TYPESPEC_ATTR_UNSPECIFY
}TypeSpecAttr;

typedef struct TypeSpecRec
{
    const char* name;
    struct TypeSpecRec* genericSpec;
    int numGenericSpec;
    struct TypeSpecRec* paramSpec;
    int numParamSpec;
    TypeSpecAttr attr;
} TypeSpec;

typedef enum {
    OBJWRAP_CLROBJECT,
    OBJWRAP_INT,
    OBJWRAP_STRING
}ObjKind;

typedef struct ObjWrapperRec
{
    ObjKind kind;
    //kindがCLROBJECTならGCHandleに変換可能なポインタが設定される
    //それ以外の場合は、生のGaucheオブジェクトのポインタが設定される
    void* ptr;
    //kindがCLROBJECT以外の場合に、kindごとに異なる意味の値が設定される
    void* value;
} ObjWrapper;

#ifdef __cplusplus
}
#endif

#endif //CLRMETHODCALLSTRUCT_H
