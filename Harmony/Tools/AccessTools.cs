// file:	Tools\AccessTools.cs
//
// summary:	Implements the access tools class
/// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace Harmony
{
	/// <summary>The access tools.</summary>
	public static class AccessTools
	{
		/// <summary>all.</summary>
		public static BindingFlags all = BindingFlags.Public
			| BindingFlags.NonPublic
			| BindingFlags.Instance
			| BindingFlags.Static
			| BindingFlags.GetField
			| BindingFlags.SetField
			| BindingFlags.GetProperty
			| BindingFlags.SetProperty;

		/// <summary>Type by name.</summary>
		/// <param name="name">The name.</param>
		/// <returns>A Type.</returns>
		///
		public static Type TypeByName(string name)
		{
			var type = Type.GetType(name, false);
			if (type == null)
				type = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(x => x.GetTypes())
					.FirstOrDefault(x => x.FullName == name);
			if (type == null)
				type = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(x => x.GetTypes())
					.FirstOrDefault(x => x.Name == name);
			return type;
		}

		/// <summary>Searches for the first including base types.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="type">  The type.</param>
		/// <param name="action">The action.</param>
		/// <returns>The found including base types.</returns>
		///
		public static T FindIncludingBaseTypes<T>(Type type, Func<Type, T> action)
		{
			while (true)
			{
				var result = action(type);
				if (result != null) return result;
				if (type == typeof(object)) return default(T);
				type = type.BaseType;
			}
		}

		/// <summary>Searches for the first including inner types.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="type">  The type.</param>
		/// <param name="action">The action.</param>
		/// <returns>The found including inner types.</returns>
		///
		public static T FindIncludingInnerTypes<T>(Type type, Func<Type, T> action)
		{
			var result = action(type);
			if (result != null) return result;
			foreach (var subType in type.GetNestedTypes(all))
			{
				result = FindIncludingInnerTypes(subType, action);
				if (result != null)
					break;
			}
			return result;
		}

		/// <summary>Fields.</summary>
		/// <param name="type">The type.</param>
		/// <param name="name">The name.</param>
		/// <returns>A FieldInfo.</returns>
		///
		public static FieldInfo Field(Type type, string name)
		{
			if (type == null || name == null) return null;
			return FindIncludingBaseTypes(type, t => t.GetField(name, all));
		}

		/// <summary>Fields.</summary>
		/// <param name="type">The type.</param>
		/// <param name="idx"> Zero-based index of the.</param>
		/// <returns>A FieldInfo.</returns>
		///
		public static FieldInfo Field(Type type, int idx)
		{
			return GetDeclaredFields(type).ElementAtOrDefault(idx);
		}

		/// <summary>Declared property.</summary>
		/// <param name="type">The type.</param>
		/// <param name="name">The name.</param>
		/// <returns>A PropertyInfo.</returns>
		///
		public static PropertyInfo DeclaredProperty(Type type, string name)
		{
			if (type == null || name == null) return null;
			return type.GetProperty(name, all);
		}

		/// <summary>Properties.</summary>
		/// <param name="type">The type.</param>
		/// <param name="name">The name.</param>
		/// <returns>A PropertyInfo.</returns>
		///
		public static PropertyInfo Property(Type type, string name)
		{
			if (type == null || name == null) return null;
			return FindIncludingBaseTypes(type, t => t.GetProperty(name, all));
		}

		/// <summary>Declared method.</summary>
		/// <param name="type">		  The type.</param>
		/// <param name="name">		  The name.</param>
		/// <param name="parameters">(Optional) Options for controlling the operation.</param>
		/// <param name="generics">  (Optional) The generics.</param>
		/// <returns>A MethodInfo.</returns>
		///
		public static MethodInfo DeclaredMethod(Type type, string name, Type[] parameters = null, Type[] generics = null)
		{
			if (type == null || name == null) return null;
			MethodInfo result;
			var modifiers = new ParameterModifier[] { };

			if (parameters == null)
				result = type.GetMethod(name, all);
			else
				result = type.GetMethod(name, all, null, parameters, modifiers);

			if (result == null) return null;
			if (generics != null) result = result.MakeGenericMethod(generics);
			return result;
		}

		/// <summary>Methods.</summary>
		/// <param name="type">		  The type.</param>
		/// <param name="name">		  The name.</param>
		/// <param name="parameters">(Optional) Options for controlling the operation.</param>
		/// <param name="generics">  (Optional) The generics.</param>
		/// <returns>A MethodInfo.</returns>
		///
		public static MethodInfo Method(Type type, string name, Type[] parameters = null, Type[] generics = null)
		{
			if (type == null || name == null) return null;
			MethodInfo result;
			var modifiers = new ParameterModifier[] { };
			if (parameters == null)
			{
				try
				{
					result = FindIncludingBaseTypes(type, t => t.GetMethod(name, all));
				}
				catch (AmbiguousMatchException)
				{
					result = FindIncludingBaseTypes(type, t => t.GetMethod(name, all, null, new Type[0], modifiers));
				}
			}
			else
			{
				result = FindIncludingBaseTypes(type, t => t.GetMethod(name, all, null, parameters, modifiers));
			}
			if (result == null) return null;
			if (generics != null) result = result.MakeGenericMethod(generics);
			return result;
		}

		/// <summary>Methods.</summary>
		/// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or illegal values.</exception>
		/// <param name="typeColonMethodname">The type colon methodname.</param>
		/// <param name="parameters">			  (Optional) Options for controlling the operation.</param>
		/// <param name="generics">			  (Optional) The generics.</param>
		/// <returns>A MethodInfo.</returns>
		///
		public static MethodInfo Method(string typeColonMethodname, Type[] parameters = null, Type[] generics = null)
		{
			if (typeColonMethodname == null) return null;
			var parts = typeColonMethodname.Split(':');
			if (parts.Length != 2)
				throw new ArgumentException("Method must be specified as 'Namespace.Type1.Type2:MethodName", nameof(typeColonMethodname));

			var type = TypeByName(parts[0]);
			return Method(type, parts[1], parameters, generics);
		}

		/// <summary>Gets method names.</summary>
		/// <param name="type">The type.</param>
		/// <returns>The method names.</returns>
		///
		public static List<string> GetMethodNames(Type type)
		{
			if (type == null) return new List<string>();
			return type.GetMethods(all).Select(m => m.Name).ToList();
		}

		/// <summary>Gets method names.</summary>
		/// <param name="instance">The instance.</param>
		/// <returns>The method names.</returns>
		///
		public static List<string> GetMethodNames(object instance)
		{
			if (instance == null) return new List<string>();
			return GetMethodNames(instance.GetType());
		}

		/// <summary>Declared constructor.</summary>
		/// <param name="type">		  The type.</param>
		/// <param name="parameters">(Optional) Options for controlling the operation.</param>
		/// <returns>A ConstructorInfo.</returns>
		///
		public static ConstructorInfo DeclaredConstructor(Type type, Type[] parameters = null)
		{
			if (type == null) return null;
			if (parameters == null) parameters = new Type[0];
			return type.GetConstructor(all, null, parameters, new ParameterModifier[] { });
		}

		/// <summary>Constructors.</summary>
		/// <param name="type">		  The type.</param>
		/// <param name="parameters">(Optional) Options for controlling the operation.</param>
		/// <returns>A ConstructorInfo.</returns>
		///
		public static ConstructorInfo Constructor(Type type, Type[] parameters = null)
		{
			if (type == null) return null;
			if (parameters == null) parameters = new Type[0];
			return FindIncludingBaseTypes(type, t => t.GetConstructor(all, null, parameters, new ParameterModifier[] { }));
		}

		/// <summary>Gets declared constructors.</summary>
		/// <param name="type">The type.</param>
		/// <returns>The declared constructors.</returns>
		///
		public static List<ConstructorInfo> GetDeclaredConstructors(Type type)
		{
			return type.GetConstructors(all).Where(method => method.DeclaringType == type).ToList();
		}

		/// <summary>Gets declared methods.</summary>
		/// <param name="type">The type.</param>
		/// <returns>The declared methods.</returns>
		///
		public static List<MethodInfo> GetDeclaredMethods(Type type)
		{
			return type.GetMethods(all).Where(method => method.DeclaringType == type).ToList();
		}

		/// <summary>Gets declared properties.</summary>
		/// <param name="type">The type.</param>
		/// <returns>The declared properties.</returns>
		///
		public static List<PropertyInfo> GetDeclaredProperties(Type type)
		{
			return type.GetProperties(all).Where(property => property.DeclaringType == type).ToList();
		}

		/// <summary>Gets declared fields.</summary>
		/// <param name="type">The type.</param>
		/// <returns>The declared fields.</returns>
		///
		public static List<FieldInfo> GetDeclaredFields(Type type)
		{
			return type.GetFields(all).Where(field => field.DeclaringType == type).ToList();
		}

		/// <summary>Gets returned type.</summary>
		/// <param name="method">The method.</param>
		/// <returns>The returned type.</returns>
		///
		public static Type GetReturnedType(MethodBase method)
		{
			var constructor = method as ConstructorInfo;
			if (constructor != null) return typeof(void);
			return ((MethodInfo)method).ReturnType;
		}

		/// <summary>Inners.</summary>
		/// <param name="type">The type.</param>
		/// <param name="name">The name.</param>
		/// <returns>A Type.</returns>
		///
		public static Type Inner(Type type, string name)
		{
			if (type == null || name == null) return null;
			return FindIncludingBaseTypes(type, t => t.GetNestedType(name, all));
		}

		/// <summary>First inner.</summary>
		/// <param name="type">		 The type.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A Type.</returns>
		///
		public static Type FirstInner(Type type, Func<Type, bool> predicate)
		{
			if (type == null || predicate == null) return null;
			return type.GetNestedTypes(all).FirstOrDefault(subType => predicate(subType));
		}

		/// <summary>First method.</summary>
		/// <param name="type">		 The type.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A MethodInfo.</returns>
		///
		public static MethodInfo FirstMethod(Type type, Func<MethodInfo, bool> predicate)
		{
			if (type == null || predicate == null) return null;
			return type.GetMethods(all).FirstOrDefault(method => predicate(method));
		}

		/// <summary>First constructor.</summary>
		/// <param name="type">		 The type.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A ConstructorInfo.</returns>
		///
		public static ConstructorInfo FirstConstructor(Type type, Func<ConstructorInfo, bool> predicate)
		{
			if (type == null || predicate == null) return null;
			return type.GetConstructors(all).FirstOrDefault(constructor => predicate(constructor));
		}

		/// <summary>First property.</summary>
		/// <param name="type">		 The type.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A PropertyInfo.</returns>
		///
		public static PropertyInfo FirstProperty(Type type, Func<PropertyInfo, bool> predicate)
		{
			if (type == null || predicate == null) return null;
			return type.GetProperties(all).FirstOrDefault(property => predicate(property));
		}

		/// <summary>Gets the types.</summary>
		/// <param name="parameters">Options for controlling the operation.</param>
		/// <returns>An array of type.</returns>
		///
		public static Type[] GetTypes(object[] parameters)
		{
			if (parameters == null) return new Type[0];
			return parameters.Select(p => p == null ? typeof(object) : p.GetType()).ToArray();
		}

		/// <summary>Gets field names.</summary>
		/// <param name="type">The type.</param>
		/// <returns>The field names.</returns>
		///
		public static List<string> GetFieldNames(Type type)
		{
			if (type == null) return new List<string>();
			return type.GetFields(all).Select(f => f.Name).ToList();
		}

		/// <summary>Gets field names.</summary>
		/// <param name="instance">The instance.</param>
		/// <returns>The field names.</returns>
		///
		public static List<string> GetFieldNames(object instance)
		{
			if (instance == null) return new List<string>();
			return GetFieldNames(instance.GetType());
		}

		/// <summary>Gets property names.</summary>
		/// <param name="type">The type.</param>
		/// <returns>The property names.</returns>
		///
		public static List<string> GetPropertyNames(Type type)
		{
			if (type == null) return new List<string>();
			return type.GetProperties(all).Select(f => f.Name).ToList();
		}

		/// <summary>Gets property names.</summary>
		/// <param name="instance">The instance.</param>
		/// <returns>The property names.</returns>
		///
		public static List<string> GetPropertyNames(object instance)
		{
			if (instance == null) return new List<string>();
			return GetPropertyNames(instance.GetType());
		}

		/// <summary>Field reference.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <typeparam name="U">Generic type parameter.</typeparam>
		/// <param name="obj">The object.</param>
		/// <returns>A ref U.</returns>
		///
		public delegate ref U FieldRef<T, U>(T obj);

		/// <summary>Field reference access.</summary>
		/// <exception cref="MissingFieldException">Thrown when a Missing Field error condition occurs.</exception>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <typeparam name="U">Generic type parameter.</typeparam>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns>A ref U.</returns>
		///
		public static FieldRef<T, U> FieldRefAccess<T, U>(string fieldName)
		{
			const BindingFlags bf = BindingFlags.NonPublic |
											BindingFlags.Instance |
											BindingFlags.DeclaredOnly;

			var fi = typeof(T).GetField(fieldName, bf);
			if (fi == null)
				throw new MissingFieldException(typeof(T).Name, fieldName);

			var s_name = "__refget_" + typeof(T).Name + "_fi_" + fi.Name;

			// workaround for using ref-return with DynamicMethod:
			// a.) initialize with dummy return value
			var dm = new DynamicMethod(s_name, typeof(U), new[] { typeof(T) }, typeof(T), true);

			// b.) replace with desired 'ByRef' return value
			var trv = Traverse.Create(dm);
			trv.Field("returnType").SetValue(typeof(U).MakeByRefType());
			trv.Field("m_returnType").SetValue(typeof(U).MakeByRefType());

			var il = dm.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldflda, fi);
			il.Emit(OpCodes.Ret);
			return (FieldRef<T, U>)dm.CreateDelegate(typeof(FieldRef<T, U>));
		}

		/// <summary>Field reference access.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <typeparam name="U">Generic type parameter.</typeparam>
		/// <param name="instance"> The instance.</param>
		/// <param name="fieldName">Name of the field.</param>
		/// <returns>A ref U.</returns>
		///
		public static ref U FieldRefAccess<T, U>(T instance, string fieldName)
		{
			return ref FieldRefAccess<T, U>(fieldName)(instance);
		}

		/// <summary>Throw missing member exception.</summary>
		/// <exception cref="MissingMemberException">Thrown when a Missing Member error condition occurs.</exception>
		/// <param name="type"> The type.</param>
		/// <param name="names">A variable-length parameters list containing names.</param>
		///
		public static void ThrowMissingMemberException(Type type, params string[] names)
		{
			var fields = string.Join(",", GetFieldNames(type).ToArray());
			var properties = string.Join(",", GetPropertyNames(type).ToArray());
			throw new MissingMemberException(string.Join(",", names) + "; available fields: " + fields + "; available properties: " + properties);
		}

		/// <summary>Gets default value.</summary>
		/// <param name="type">The type.</param>
		/// <returns>The default value.</returns>
		///
		public static object GetDefaultValue(Type type)
		{
			if (type == null) return null;
			if (type == typeof(void)) return null;
			if (type.IsValueType)
				return Activator.CreateInstance(type);
			return null;
		}

		/// <summary>Creates an instance.</summary>
		/// <exception cref="NullReferenceException">Thrown when a value was unexpectedly null.</exception>
		/// <param name="type">The type.</param>
		/// <returns>The new instance.</returns>
		///
		public static object CreateInstance(Type type)
		{
			if (type == null)
				throw new NullReferenceException("Cannot create instance for NULL type");
			var ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, new Type[0], null);
			if (ctor != null)
				return Activator.CreateInstance(type);
			return FormatterServices.GetUninitializedObject(type);
		}

		/// <summary>Makes deep copy.</summary>
		/// <param name="source">	  Source for the.</param>
		/// <param name="resultType">Type of the result.</param>
		/// <param name="processor"> (Optional) The processor.</param>
		/// <param name="pathRoot">  (Optional) The path root.</param>
		/// <returns>An object.</returns>
		///
		public static object MakeDeepCopy(object source, Type resultType, Func<string, Traverse, Traverse, object> processor = null, string pathRoot = "")
		{
			if (source == null)
				return null;

			var type = source.GetType();

			if (type.IsPrimitive)
				return source;

			if (type.IsEnum)
				return Enum.ToObject(resultType, (int)source);

			if (type.IsGenericType && resultType.IsGenericType)
			{
				var addOperation = FirstMethod(resultType, m => m.Name == "Add" && m.GetParameters().Count() == 1);
				if (addOperation != null)
				{
					var addableResult = Activator.CreateInstance(resultType);
					var addInvoker = MethodInvoker.GetHandler(addOperation);
					var newElementType = resultType.GetGenericArguments()[0];
					var i = 0;
					foreach (var element in source as IEnumerable)
					{
						var iStr = (i++).ToString();
						var path = pathRoot.Length > 0 ? pathRoot + "." + iStr : iStr;
						var newElement = MakeDeepCopy(element, newElementType, processor, path);
						addInvoker(addableResult, new object[] { newElement });
					}
					return addableResult;
				}

				// TODO: add dictionaries support
				// maybe use methods in Dictionary<KeyValuePair<TKey,TVal>>
			}

			if (type.IsArray && resultType.IsArray)
			{
				var elementType = resultType.GetElementType();
				var length = ((Array)source).Length;
				var arrayResult = Activator.CreateInstance(resultType, new object[] { length }) as object[];
				var originalArray = source as object[];
				for (var i = 0; i < length; i++)
				{
					var iStr = i.ToString();
					var path = pathRoot.Length > 0 ? pathRoot + "." + iStr : iStr;
					arrayResult[i] = MakeDeepCopy(originalArray[i], elementType, processor, path);
				}
				return arrayResult;
			}

			var ns = type.Namespace;
			if (ns == "System" || (ns?.StartsWith("System.") ?? false))
				return source;

			var result = CreateInstance(resultType);
			Traverse.IterateFields(source, result, (name, src, dst) =>
			{
				var path = pathRoot.Length > 0 ? pathRoot + "." + name : name;
				var value = processor != null ? processor(path, src, dst) : src.GetValue();
				dst.SetValue(MakeDeepCopy(value, dst.GetValueType(), processor, path));
			});
			return result;
		}

		/// <summary>Makes deep copy.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="source">	 Source for the.</param>
		/// <param name="result">	 [out] The result.</param>
		/// <param name="processor">(Optional) The processor.</param>
		/// <param name="pathRoot"> (Optional) The path root.</param>
		///
		public static void MakeDeepCopy<T>(object source, out T result, Func<string, Traverse, Traverse, object> processor = null, string pathRoot = "")
		{
			result = (T)MakeDeepCopy(source, typeof(T), processor, pathRoot);
		}

		/// <summary>Query if 'type' is structure.</summary>
		/// <param name="type">The type.</param>
		/// <returns>True if structure, false if not.</returns>
		///
		public static bool IsStruct(Type type)
		{
			return type.IsValueType && !IsValue(type) && !IsVoid(type);
		}

		/// <summary>Query if 'type' is class.</summary>
		/// <param name="type">The type.</param>
		/// <returns>True if class, false if not.</returns>
		///
		public static bool IsClass(Type type)
		{
			return !type.IsValueType;
		}

		/// <summary>Query if 'type' is value.</summary>
		/// <param name="type">The type.</param>
		/// <returns>True if value, false if not.</returns>
		///
		public static bool IsValue(Type type)
		{
			return type.IsPrimitive || type.IsEnum;
		}

		/// <summary>Query if 'type' is void.</summary>
		/// <param name="type">The type.</param>
		/// <returns>True if void, false if not.</returns>
		///
		public static bool IsVoid(Type type)
		{
			return type == typeof(void);
		}
	}
}