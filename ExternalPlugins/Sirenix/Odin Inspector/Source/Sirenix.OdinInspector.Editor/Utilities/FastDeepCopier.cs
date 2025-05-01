//-----------------------------------------------------------------------
// <copyright file="FastDeepCopier.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

    using Sirenix.Serialization;
    using Sirenix.Serialization.Utilities;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    public static class FastDeepCopier
    {
        private static readonly object LOCK = new object();
        private static readonly Dictionary<Type, DeepCopierWeakNoPolymorphism> DeepWeakCopiers = new Dictionary<Type, DeepCopierWeakNoPolymorphism>(FastTypeComparer.Instance);
        private static readonly Dictionary<Type, Delegate> DeepCopiers = new Dictionary<Type, Delegate>(FastTypeComparer.Instance);
        private static readonly Type[] CopyFromToArgSignature = new Type[3] { typeof(object), typeof(object), typeof(Dictionary<object, object>) };

        private static readonly MethodInfo Array_GetLength = typeof(Array).GetMethod("GetLength", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(int) }, null);
        private static readonly MethodInfo Buffer_BlockCopy = typeof(Buffer).GetMethod("BlockCopy", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(Array), typeof(int), typeof(Array), typeof(int), typeof(int) }, null);
        private static readonly MethodInfo FastDeepCopier_CopyMultiDimensionalArray = typeof(FastDeepCopier).GetMethod("CopyMultiDimensionalArray", BindingFlags.Public | BindingFlags.Static);

        private static readonly Action<ILGenerator> EmitLdArg0 = il => il.Emit(OpCodes.Ldarg_0);
        private static readonly Action<ILGenerator> EmitLdArg1 = il => il.Emit(OpCodes.Ldarg_1);

        private static readonly Action<ILGenerator> EmitLdArg0IndRef = il => { il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldind_Ref); };
        private static readonly Action<ILGenerator> EmitLdArg1IndRef = il => { il.Emit(OpCodes.Ldarg_1); il.Emit(OpCodes.Ldind_Ref); };

        public delegate void DeepCopierNoPolymorphism<T>(ref T from, ref T to, Dictionary<object, object> references);
        public delegate void DeepCopierWeakNoPolymorphism(object from, object to, Dictionary<object, object> references);

        private static int emitNameIncrement;

        private static object cachedReferenceDicts_LOCK = new object();
        private static readonly Dictionary<object, object>[] cachedReferenceDicts = new Dictionary<object, object>[4];

        private static readonly Type TypeOf_String = typeof(string);
        private static readonly Type TypeOf_Object = typeof(object);
        private static readonly Type TypeOf_InspectorProperty = typeof(InspectorProperty);

        public static Dictionary<object, object> ClaimCachedReferenceDict()
        {
            lock (cachedReferenceDicts_LOCK)
            {
                var array = cachedReferenceDicts;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] != null)
                    {
                        var result = array[i];
                        array[i] = null;
                        result.Clear();
                        return result;
                    }
                }

                return new Dictionary<object, object>(ReferenceEqualityComparer<object>.Default);
            }
        }

        public static void ReleaseCachedReferenceDict(Dictionary<object, object> dict)
        {
            dict.Clear();
            lock (cachedReferenceDicts_LOCK)
            {
                var array = cachedReferenceDicts;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] == null)
                    {
                        array[i] = dict;
                        return;
                    }
                }
            }
        }

        private static bool ShouldCopyTypeByRef(Type type)
        {
            return object.ReferenceEquals(type, TypeOf_String)
                    || typeof(UnityEngine.Object).IsAssignableFrom(type)
                    || typeof(MemberInfo).IsAssignableFrom(type)
                    || typeof(Module).IsAssignableFrom(type)
                    || typeof(Assembly).IsAssignableFrom(type)
                    || object.ReferenceEquals(type, TypeOf_InspectorProperty)
                    || typeof(PropertyTree).IsAssignableFrom(type);
        }

        public static class Accelerator<T>
        {
            private static readonly Type TypeOf_T = typeof(T);
            private static readonly bool CopyByRef = object.ReferenceEquals(TypeOf_T, TypeOf_String)
                    || typeof(UnityEngine.Object).IsAssignableFrom(TypeOf_T)
                    || typeof(MemberInfo).IsAssignableFrom(TypeOf_T)
                    || typeof(Module).IsAssignableFrom(TypeOf_T)
                    || typeof(Assembly).IsAssignableFrom(TypeOf_T)
                    || object.ReferenceEquals(TypeOf_T, TypeOf_InspectorProperty)
                    || typeof(PropertyTree).IsAssignableFrom(TypeOf_T);
            private static readonly bool IsObjectOrInterface = object.ReferenceEquals(TypeOf_T, TypeOf_Object) || TypeOf_T.IsInterface;

            public static T DeepCopyWithManualReferences(T instance, Dictionary<object, object> references)
            {
                if (CopyByRef || object.ReferenceEquals(instance, null)) return instance;

                var instanceType = instance.GetType();
                var isPreciseType = object.ReferenceEquals(instanceType, TypeOf_T);

                if (!isPreciseType && IsObjectOrInterface)
                {
                    // We have to check the actual instance for whether to copy by ref
                    if (object.ReferenceEquals(instanceType, TypeOf_String)
                        || instance is UnityEngine.Object
                        || instance is MemberInfo
                        || instance is Module
                        || instance is Assembly
                        || object.ReferenceEquals(instanceType, TypeOf_InspectorProperty)
                        || instance is PropertyTree)
                    {
                        return instance;
                    }
                }

                object existingCopy;

                if (references != null && references.TryGetValue(instance, out existingCopy))
                {
                    return (T)existingCopy;
                }

                T copy;
                var array = instance as Array;

                if (array != null)
                {
                    var rank = instanceType.GetArrayRank();

                    if (rank == 1)
                    {
                        var copyArr = Array.CreateInstance(instanceType.GetElementType(), array.Length);

                        var elementType = instanceType.GetElementType();
                        copy = (T)(object)copyArr;

                        if (elementType.IsPrimitive)
                        {
                            Buffer.BlockCopy(array, 0, copyArr, 0, array.Length * Marshal.SizeOf(elementType));
                            return copy;
                        }
                    }
                    else
                    {
                        int[] ranks = new int[rank];

                        for (int i = 0; i < rank; i++)
                        {
                            ranks[i] = array.GetLength(i);
                        }

                        copy = (T)(object)Array.CreateInstance(instanceType.GetElementType(), ranks);
                    }
                }
                else
                {
                    copy = (T)FormatterServices.GetUninitializedObject(instanceType);
                }

                if (references != null)
                {
                    references.Add(instance, copy);
                }

                if (!isPreciseType)
                {
                    var weakCopier = GetDeepCopierWeakNoPolymorphism(instanceType);
                    object weakCopy = copy;
                    weakCopier(instance, weakCopy, references);
                    copy = (T)weakCopy;
                }
                else
                {
                    var copier = GetDeepCopierNoPolymorphism<T>();
                    copier(ref instance, ref copy, references);
                }

                return copy;
            }
        }

        public static T DeepCopy<T>(T instance, bool referenceTracking = true)
        {
            var references = referenceTracking ? ClaimCachedReferenceDict() : null;
            try
            {
                return Accelerator<T>.DeepCopyWithManualReferences(instance, references);
            }
            finally
            {
                if (references != null)
                {
                    ReleaseCachedReferenceDict(references);
                }
            }
        }

        public static T DeepCopyWithManualReferences<T>(T instance, Dictionary<object, object> references)
        {
            return Accelerator<T>.DeepCopyWithManualReferences(instance, references);
        }

        public static void DeepCopyFromToStruct<T>(ref T from, ref T to, bool referenceTracking = true) where T : struct
        {
            var references = referenceTracking ? ClaimCachedReferenceDict() : null;
            try
            {
                DeepCopyFromToStructWithManualReferences<T>(ref from, ref to, references);
            }
            finally
            {
                if (references != null)
                {
                    ReleaseCachedReferenceDict(references);
                }
            }
        }

        public static void DeepCopyFromToStructWithManualReferences<T>(ref T from, ref T to, Dictionary<object, object> references) where T : struct
        {
            var copier = GetDeepCopierNoPolymorphism<T>();
            copier(ref from, ref to, references);
        }

        public static void DeepCopyFromToClass<T>(T from, T to, bool referenceTracking = true) where T : class
        {
            var references = referenceTracking ? ClaimCachedReferenceDict() : null;
            try
            {
                DeepCopyFromToClassWithManualReferences<T>(from, to, references);
            }
            finally
            {
                if (references != null)
                {
                    ReleaseCachedReferenceDict(references);
                }
            }
        }

        public static void DeepCopyFromToClassWithManualReferences<T>(T from, T to, Dictionary<object, object> references) where T : class
        {
            if (object.ReferenceEquals(from, null) || object.ReferenceEquals(to, null)) throw new ArgumentException("Object to copy from or to cannot be null.");
            if (object.ReferenceEquals(from, to)) return;
            var type = from.GetType();
            if (!object.ReferenceEquals(type, to.GetType())) throw new ArgumentException("Object to copy from and to must be the same type; from was '" + from.GetType().GetNiceName() + "' and to was '" + from.GetType().GetNiceName() + "'.");

            if (!object.ReferenceEquals(type, typeof(T)))
            {
                var weakCopier = GetDeepCopierWeakNoPolymorphism(type);
                weakCopier(from, to, references);
            }
            else
            {
                var copier = GetDeepCopierNoPolymorphism<T>();
                copier(ref from, ref to, references);
            }
        }

        public static DeepCopierNoPolymorphism<T> GetDeepCopierNoPolymorphism<T>()
        {
            Delegate deepCopierDelegate;
            var type = typeof(T);

            lock (LOCK)
            {
                if (!DeepCopiers.TryGetValue(type, out deepCopierDelegate))
                {
                    var deepCopyMethod = new DynamicMethod(
                        "DeepCopy_" + type.GetNiceFullName() + "_" + emitNameIncrement++,
                        typeof(void),
                        new Type[] { typeof(T).MakeByRefType(), typeof(T).MakeByRefType(), typeof(Dictionary<object, object>) },
                        true);

                    var il = deepCopyMethod.GetILGenerator();
                    var isValueType = type.IsValueType;

                    EmitTypeCopyingLogic(type, il, isValueType ? EmitLdArg0 : EmitLdArg0IndRef, isValueType ? EmitLdArg1 : EmitLdArg1IndRef, isValueType);

                    il.Emit(OpCodes.Ret);

                    deepCopierDelegate = deepCopyMethod.CreateDelegate(typeof(DeepCopierNoPolymorphism<T>));
                    DeepCopiers.Add(type, deepCopierDelegate);
                }
            }

            return deepCopierDelegate as DeepCopierNoPolymorphism<T>;
        }

        public static DeepCopierWeakNoPolymorphism GetDeepCopierWeakNoPolymorphism(Type type)
        {
            DeepCopierWeakNoPolymorphism deepCopier;

            lock (LOCK)
            {
                if (!DeepWeakCopiers.TryGetValue(type, out deepCopier))
                {
                    var deepCopyMethodBuilder = new DynamicMethod(
                        "DeepCopyWeak_" + type.GetNiceFullName() + "_" + emitNameIncrement++,
                        typeof(void),
                        CopyFromToArgSignature,
                        true);

                    var il = deepCopyMethodBuilder.GetILGenerator();
                    var isValueType = type.IsValueType;

                    if (isValueType)
                    {
                        EmitTypeCopyingLogic(type, il, CreateLdArgUnboxGenerator(OpCodes.Ldarg_0, type), CreateLdArgUnboxGenerator(OpCodes.Ldarg_1, type), true);
                    }
                    else
                    {
                        var from = il.DeclareLocal(type);
                        var to = il.DeclareLocal(type);

                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Castclass, type);
                        il.Emit(OpCodes.Stloc, from);

                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Castclass, type);
                        il.Emit(OpCodes.Stloc, to);

                        EmitTypeCopyingLogic(type, il, CreateLdLocGenerator(from), CreateLdLocGenerator(to), true);
                    }

                    il.Emit(OpCodes.Ret);

                    deepCopier = (DeepCopierWeakNoPolymorphism)deepCopyMethodBuilder.CreateDelegate(typeof(DeepCopierWeakNoPolymorphism));
                    DeepWeakCopiers.Add(type, deepCopier);
                }
            }

            return deepCopier;
        }

        private static Action<ILGenerator> CreateLdLocGenerator(LocalBuilder local)
        {
            return il => il.Emit(OpCodes.Ldloc, local);
        }

        private static Action<ILGenerator> CreateLdArgUnboxGenerator(OpCode ldArg, Type type)
        {
            return il =>
            {
                il.Emit(ldArg);
                il.Emit(OpCodes.Unbox, type);
            };
        }

        private static void EmitTypeCopyingLogic(Type type, ILGenerator il, Action<ILGenerator> emitLoadCopyFrom, Action<ILGenerator> emitLoadCopyTo, bool isValueType)
        {
            if (type.IsArray)
            {
                EmitArrayCopyingLogic(type, type.GetElementType(), il, emitLoadCopyFrom, emitLoadCopyTo);
            }
            else
            {
                EmitFieldCopyingLogic(type, il, emitLoadCopyFrom, emitLoadCopyTo, isValueType);
            }
        }

        private static void EmitArrayCreationLogic(Type arrayType, Type elementType, ILGenerator il, Action<ILGenerator> emitLoadCopyFrom)
        {
            var rank = arrayType.GetArrayRank();

            if (rank == 1)
            {
                emitLoadCopyFrom(il);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Newarr, elementType);
            }
            else
            {
                var constructorSignature = new Type[rank];

                for (int i = 0; i < rank; i++)
                {
                    constructorSignature[i] = typeof(int);
                }

                var ctor = arrayType.GetConstructor(constructorSignature);

                for (int i = 0; i < rank; i++)
                {
                    emitLoadCopyFrom(il);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Callvirt, Array_GetLength);
                }

                il.Emit(OpCodes.Newobj, ctor);
            }
        }

        public static void CopyMultiDimensionalArray<T>(Array from, Array to, Dictionary<object, object> references)
        {
            var arrayType = from.GetType();
            var elementType = typeof(T);

            Func<T, Dictionary<object, object>, T> copier = null;

            if (elementType.IsValueType)
            {
                if (!StructCanBeCopiedDirectly(elementType))
                {
                    var structCopier = GetDeepCopierNoPolymorphism<T>();
                    copier = (value, refs) =>
                    {
                        T fromV = value;
                        T toV = default(T);
                        structCopier(ref fromV, ref toV, refs);
                        return toV;
                    };
                }
            }
            else
            {
                copier = DeepCopyWithManualReferences<T>;
            }

            var ranks = arrayType.GetArrayRank();
            var rankSizes = new long[ranks];
            var indices = new long[ranks];

            for (int i = 0; i < ranks; i++)
            {
                rankSizes[i] = from.GetLength(i);
            }

            while (true)
            {
                bool allIndicesDoneIterating = true;

                for (int i = 0; i < indices.Length; i++)
                {
                    if (indices[i] != rankSizes[i] - 1)
                    {
                        allIndicesDoneIterating = false;
                        break;
                    }
                }

                var value = (T)from.GetValue(indices);
                to.SetValue(copier != null ? copier(value, references) : value, indices);

                if (allIndicesDoneIterating)
                {
                    break;
                }

                for (int i = indices.Length - 1; i >= 0; i--)
                {
                    if (++indices[i] < rankSizes[i])
                        break;

                    indices[i] = 0;
                }
            }
        }

        private static void EmitArrayCopyingLogic(Type arrayType, Type elementType, ILGenerator il, Action<ILGenerator> loadFrom, Action<ILGenerator> loadTo)
        {
            //Debug.Log("Copy array: " + arrayType.GetNiceName());

            if (arrayType.GetArrayRank() != 1)
            {
                var copyMethod = FastDeepCopier_CopyMultiDimensionalArray.MakeGenericMethod(elementType);

                loadFrom(il);
                loadTo(il);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Call, copyMethod);
                return;
            }

            if (elementType.IsPrimitive)
            {
                var getType = typeof(object).GetMethod("GetType");

                loadFrom(il);
                il.Emit(OpCodes.Ldc_I4_0);
                loadTo(il);
                il.Emit(OpCodes.Ldc_I4_0);
                loadFrom(il);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Ldc_I4, Marshal.SizeOf(elementType));
                il.Emit(OpCodes.Mul);
                il.Emit(OpCodes.Call, Buffer_BlockCopy);
            }
            else
            {
                var iLocal = il.DeclareLocal(typeof(int));
                var loopLengthCheck = il.DefineLabel();
                var loopBody = il.DefineLabel();

                il.Emit(OpCodes.Br, loopLengthCheck);

                // i = 0
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc, iLocal);

                il.MarkLabel(loopBody);

                if (elementType.IsValueType)
                {
                    if (StructCanBeCopiedDirectly(elementType))
                    {
                        loadTo(il);
                        il.Emit(OpCodes.Ldloc, iLocal);
                        loadFrom(il);
                        il.Emit(OpCodes.Ldloc, iLocal);
                        il.Emit(OpCodes.Ldelem, elementType);
                        il.Emit(OpCodes.Stelem, elementType);
                    }
                    else
                    {
                        EmitFieldCopyingLogic(elementType, il,
                            il2 =>
                            {
                                loadFrom(il2);
                                il2.Emit(OpCodes.Ldloc, iLocal);
                                il2.Emit(OpCodes.Ldelema, elementType);
                            },
                            il2 =>
                            {
                                loadTo(il2);
                                il2.Emit(OpCodes.Ldloc, iLocal);
                                il2.Emit(OpCodes.Ldelema, elementType);
                            },
                            true);
                    }
                }
                else
                {
                    var deepCopyElementMethod = typeof(Accelerator<>).MakeGenericType(elementType).GetMethod("DeepCopyWithManualReferences", BindingFlags.Public | BindingFlags.Static);

                    loadTo(il);
                    il.Emit(OpCodes.Ldloc, iLocal);
                    loadFrom(il);
                    il.Emit(OpCodes.Ldloc, iLocal);
                    il.Emit(OpCodes.Ldelem, elementType);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, deepCopyElementMethod);
                    il.Emit(OpCodes.Stelem, elementType);
                }

                // i++
                il.Emit(OpCodes.Ldloc, iLocal);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stloc, iLocal);

                // i < from.Length
                il.MarkLabel(loopLengthCheck);
                il.Emit(OpCodes.Ldloc, iLocal);
                loadFrom(il);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Conv_I4);
                il.Emit(OpCodes.Clt);
                il.Emit(OpCodes.Brtrue, loopBody);
            }
        }

        private static void EmitFieldCopyingLogic(Type type, ILGenerator il, Action<ILGenerator> loadFrom, Action<ILGenerator> loadTo, bool isValueType)
        {
            var fields = FormatterUtilities.GetSerializableMembers(type, SerializationPolicies.Everything);

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i] as FieldInfo;

                //Debug.Log("Field in closure is set to " + field.Name + " -> " + field.FieldType.GetNiceName());

                if (field == null)
                {
                    throw new Exception("SerializationPolicies.Everything is serializing a non-field member '" + fields[i].Name + "' on type '" + type.GetNiceName() + "'");
                }

                if (field.DeclaringType == typeof(UnityEngine.Object)) continue; // Let's not copy their internal pointers and so on
                var fieldType = field.FieldType;

                if (FormatterUtilities.IsPrimitiveType(fieldType))
                {
                    loadTo(il);
                    loadFrom(il);
                    il.Emit(OpCodes.Ldfld, field);
                    il.Emit(OpCodes.Stfld, field);
                }
                else if (fieldType.IsValueType)
                {
                    if (StructCanBeCopiedDirectly(fieldType))
                    {
                        loadTo(il);
                        loadFrom(il);
                        il.Emit(OpCodes.Ldfld, field);
                        il.Emit(OpCodes.Stfld, field);
                    }
                    else
                    {
                        EmitFieldCopyingLogic(fieldType, il,
                            il2 =>
                            {
                                loadFrom(il2);
                                il2.Emit(OpCodes.Ldflda, field);
                            },
                            il2 =>
                            {
                                loadTo(il2);
                                il2.Emit(OpCodes.Ldflda, field);
                            },
                            true);
                    }
                }
                else if (fieldType.IsArray && fieldType.GetArrayRank() == 1)
                {
                    var elementType = fieldType.GetElementType();
                    var notNullCase = il.DefineLabel();
                    var done = il.DefineLabel();

                    loadFrom(il);
                    il.Emit(OpCodes.Ldfld, field);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Ceq);
                    il.Emit(OpCodes.Brfalse, notNullCase);

                    // Null case
                    loadTo(il);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Stfld, field);
                    il.Emit(OpCodes.Br, done);

                    // Not null case
                    il.MarkLabel(notNullCase);

                    Action<ILGenerator> loadArray = il2 =>
                    {
                        loadFrom(il2);
                        il2.Emit(OpCodes.Ldfld, field);
                    };

                    loadTo(il);
                    EmitArrayCreationLogic(fieldType, elementType, il, loadArray);
                    il.Emit(OpCodes.Stfld, field);

                    EmitArrayCopyingLogic(fieldType, elementType, il, loadArray,
                        il2 =>
                        {
                            loadTo(il2);
                            il2.Emit(OpCodes.Ldfld, field);
                        });

                    il.MarkLabel(done);
                }
                else
                {
                    var deepCopyMethod = typeof(Accelerator<>).MakeGenericType(fieldType).GetMethod("DeepCopyWithManualReferences", BindingFlags.Public | BindingFlags.Static);
                    loadTo(il);
                    loadFrom(il);
                    il.Emit(OpCodes.Ldfld, field);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, deepCopyMethod);
                    il.Emit(OpCodes.Stfld, field);
                }
            }
        }

        private static bool StructCanBeCopiedDirectly(Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            for (int i = 0; i < fields.Length; i++)
            {
                var fieldType = fields[i].FieldType;

                if (FormatterUtilities.IsPrimitiveType(fieldType)) continue;
                if (!fieldType.IsValueType) return false;
                if (!StructCanBeCopiedDirectly(fieldType)) return false;
            }

            return true;
        }

    }
}
#endif