// file:	Tools\Traverse.cs
//
// summary:	Implements the traverse class
/// 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Harmony
{
	/// <summary>A traverse.</summary>
	/// <typeparam name="T">Generic type parameter.</typeparam>
	///
	public class Traverse<T>
	{
		private Traverse traverse;

		Traverse()
		{
		}

		/// <summary>Constructor.</summary>
		/// <param name="traverse">The traverse.</param>
		///
		public Traverse(Traverse traverse)
		{
			this.traverse = traverse;
		}

		/// <summary>Gets or sets the value.</summary>
		/// <value>The value.</value>
		///
		public T Value
		{
			get => traverse.GetValue<T>();
			set => traverse.SetValue(value);
		}
	}

	/// <summary>A traverse.</summary>
	public class Traverse
	{
		static AccessCache Cache;

		Type _type;
		object _root;
		readonly MemberInfo _info;
		MethodBase _method;
		readonly object[] _params;

		[MethodImpl(MethodImplOptions.Synchronized)]
		static Traverse()
		{
			if (Cache == null)
				Cache = new AccessCache();
		}

		/// <summary>Creates a new Traverse.</summary>
		/// <param name="type">The type.</param>
		/// <returns>A Traverse.</returns>
		///
		public static Traverse Create(Type type)
		{
			return new Traverse(type);
		}

		/// <summary>Creates a new Traverse.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <returns>A Traverse.</returns>
		///
		public static Traverse Create<T>()
		{
			return Create(typeof(T));
		}

		/// <summary>Creates a new Traverse.</summary>
		/// <param name="root">The root.</param>
		/// <returns>A Traverse.</returns>
		///
		public static Traverse Create(object root)
		{
			return new Traverse(root);
		}

		/// <summary>Creates with type.</summary>
		/// <param name="name">The name.</param>
		/// <returns>The new with type.</returns>
		///
		public static Traverse CreateWithType(string name)
		{
			return new Traverse(AccessTools.TypeByName(name));
		}

		Traverse()
		{
		}

		/// <summary>Constructor.</summary>
		/// <param name="type">The type.</param>
		///
		public Traverse(Type type)
		{
			_type = type;
		}

		/// <summary>Constructor.</summary>
		/// <param name="root">The root.</param>
		///
		public Traverse(object root)
		{
			_root = root;
			_type = root?.GetType();
		}

		Traverse(object root, MemberInfo info, object[] index)
		{
			_root = root;
			_type = root?.GetType();
			_info = info;
			_params = index;
		}

		Traverse(object root, MethodInfo method, object[] parameter)
		{
			_root = root;
			_type = method.ReturnType;
			_method = method;
			_params = parameter;
		}

		/// <summary>Gets a value.</summary>
		/// <returns>The value.</returns>
		///
		public object GetValue()
		{
			if (_info is FieldInfo)
				return ((FieldInfo)_info).GetValue(_root);
			if (_info is PropertyInfo)
				return ((PropertyInfo)_info).GetValue(_root, AccessTools.all, null, _params, CultureInfo.CurrentCulture);
			if (_method != null)
				return _method.Invoke(_root, _params);
			if (_root == null && _type != null) return _type;
			return _root;
		}

		/// <summary>Gets a value.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <returns>The value.</returns>
		///
		public T GetValue<T>()
		{
			var value = GetValue();
			if (value == null) return default(T);
			return (T)value;
		}

		/// <summary>Gets a value.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <param name="arguments">The arguments.</param>
		/// <returns>The value.</returns>
		///
		public object GetValue(params object[] arguments)
		{
			if (_method == null)
				throw new Exception("cannot get method value without method");
			return _method.Invoke(_root, arguments);
		}

		/// <summary>Gets a value.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="arguments">The arguments.</param>
		/// <returns>The value.</returns>
		///
		public T GetValue<T>(params object[] arguments)
		{
			if (_method == null)
				throw new Exception("cannot get method value without method");
			return (T)_method.Invoke(_root, arguments);
		}

		/// <summary>Sets a value.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <param name="value">The value.</param>
		/// <returns>A Traverse.</returns>
		///
		public Traverse SetValue(object value)
		{
			if (_info is FieldInfo)
				((FieldInfo)_info).SetValue(_root, value, AccessTools.all, null, CultureInfo.CurrentCulture);
			if (_info is PropertyInfo)
				((PropertyInfo)_info).SetValue(_root, value, AccessTools.all, null, _params, CultureInfo.CurrentCulture);
			if (_method != null)
				throw new Exception("cannot set value of method " + _method.FullDescription());
			return this;
		}

		/// <summary>Gets value type.</summary>
		/// <returns>The value type.</returns>
		///
		public Type GetValueType()
		{
			if (_info is FieldInfo)
				return ((FieldInfo)_info).FieldType;
			if (_info is PropertyInfo)
				return ((PropertyInfo)_info).PropertyType;
			return null;
		}

		Traverse Resolve()
		{
			if (_root == null && _type != null) return this;
			return new Traverse(GetValue());
		}

		/// <summary>Types.</summary>
		/// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
		/// <param name="name">The name.</param>
		/// <returns>A Traverse.</returns>
		///
		public Traverse Type(string name)
		{
			if (name == null) throw new ArgumentNullException("name cannot be null");
			if (_type == null) return new Traverse();
			var type = AccessTools.Inner(_type, name);
			if (type == null) return new Traverse();
			return new Traverse(type);
		}

		/// <summary>Fields.</summary>
		/// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
		/// <param name="name">The name.</param>
		/// <returns>A Traverse&lt;T&gt;</returns>
		///
		public Traverse Field(string name)
		{
			if (name == null) throw new ArgumentNullException("name cannot be null");
			var resolved = Resolve();
			if (resolved._type == null) return new Traverse();
			var info = Cache.GetFieldInfo(resolved._type, name);
			if (info == null) return new Traverse();
			if (info.IsStatic == false && resolved._root == null) return new Traverse();
			return new Traverse(resolved._root, info, null);
		}

		/// <summary>Fields.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="name">The name.</param>
		/// <returns>A Traverse&lt;T&gt;</returns>
		///
		public Traverse<T> Field<T>(string name)
		{
			return new Traverse<T>(Field(name));
		}

		/// <summary>Gets the fields.</summary>
		/// <returns>A List&lt;string&gt;</returns>
		///
		public List<string> Fields()
		{
			var resolved = Resolve();
			return AccessTools.GetFieldNames(resolved._type);
		}

		/// <summary>Properties.</summary>
		/// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
		/// <param name="name"> The name.</param>
		/// <param name="index">(Optional) Zero-based index of the.</param>
		/// <returns>A Traverse&lt;T&gt;</returns>
		///
		public Traverse Property(string name, object[] index = null)
		{
			if (name == null) throw new ArgumentNullException("name cannot be null");
			var resolved = Resolve();
			if (resolved._root == null || resolved._type == null) return new Traverse();
			var info = Cache.GetPropertyInfo(resolved._type, name);
			if (info == null) return new Traverse();
			return new Traverse(resolved._root, info, index);
		}

		/// <summary>Properties.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="name"> The name.</param>
		/// <param name="index">(Optional) Zero-based index of the.</param>
		/// <returns>A Traverse&lt;T&gt;</returns>
		///
		public Traverse<T> Property<T>(string name, object[] index = null)
		{
			return new Traverse<T>(Property(name, index));
		}

		/// <summary>Gets the properties.</summary>
		/// <returns>A List&lt;string&gt;</returns>
		///
		public List<string> Properties()
		{
			var resolved = Resolve();
			return AccessTools.GetPropertyNames(resolved._type);
		}

		/// <summary>Methods.</summary>
		/// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
		/// <param name="name">		 The name.</param>
		/// <param name="arguments">The arguments.</param>
		/// <returns>A Traverse.</returns>
		///
		public Traverse Method(string name, params object[] arguments)
		{
			if (name == null) throw new ArgumentNullException("name cannot be null");
			var resolved = Resolve();
			if (resolved._type == null) return new Traverse();
			var types = AccessTools.GetTypes(arguments);
			var method = Cache.GetMethodInfo(resolved._type, name, types);
			if (method == null) return new Traverse();
			return new Traverse(resolved._root, (MethodInfo)method, arguments);
		}

		/// <summary>Methods.</summary>
		/// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
		/// <param name="name">		  The name.</param>
		/// <param name="paramTypes">List of types of the parameters.</param>
		/// <param name="arguments"> (Optional) The arguments.</param>
		/// <returns>A Traverse.</returns>
		///
		public Traverse Method(string name, Type[] paramTypes, object[] arguments = null)
		{
			if (name == null) throw new ArgumentNullException("name cannot be null");
			var resolved = Resolve();
			if (resolved._type == null) return new Traverse();
			var method = Cache.GetMethodInfo(resolved._type, name, paramTypes);
			if (method == null) return new Traverse();
			return new Traverse(resolved._root, (MethodInfo)method, arguments);
		}

		/// <summary>Gets the methods.</summary>
		/// <returns>A List&lt;string&gt;</returns>
		///
		public List<string> Methods()
		{
			var resolved = Resolve();
			return AccessTools.GetMethodNames(resolved._type);
		}

		/// <summary>Queries if a given field exists.</summary>
		/// <returns>True if it succeeds, false if it fails.</returns>
		///
		public bool FieldExists()
		{
			return _info != null;
		}

		/// <summary>Queries if a given method exists.</summary>
		/// <returns>True if it succeeds, false if it fails.</returns>
		///
		public bool MethodExists()
		{
			return _method != null;
		}

		/// <summary>Queries if a given type exists.</summary>
		/// <returns>True if it succeeds, false if it fails.</returns>
		///
		public bool TypeExists()
		{
			return _type != null;
		}

		/// <summary>Iterate fields.</summary>
		/// <param name="source">Source for the.</param>
		/// <param name="action">The action.</param>
		///
		public static void IterateFields(object source, Action<Traverse> action)
		{
			var sourceTrv = Create(source);
			AccessTools.GetFieldNames(source).ForEach(f => action(sourceTrv.Field(f)));
		}

		/// <summary>Iterate fields.</summary>
		/// <param name="source">Source for the.</param>
		/// <param name="target">Target for the.</param>
		/// <param name="action">The action.</param>
		///
		public static void IterateFields(object source, object target, Action<Traverse, Traverse> action)
		{
			var sourceTrv = Create(source);
			var targetTrv = Create(target);
			AccessTools.GetFieldNames(source).ForEach(f => action(sourceTrv.Field(f), targetTrv.Field(f)));
		}

		/// <summary>Iterate fields.</summary>
		/// <param name="source">Source for the.</param>
		/// <param name="target">Target for the.</param>
		/// <param name="action">The action.</param>
		///
		public static void IterateFields(object source, object target, Action<string, Traverse, Traverse> action)
		{
			var sourceTrv = Create(source);
			var targetTrv = Create(target);
			AccessTools.GetFieldNames(source).ForEach(f => action(f, sourceTrv.Field(f), targetTrv.Field(f)));
		}

		/// <summary>Iterate properties.</summary>
		/// <param name="source">Source for the.</param>
		/// <param name="action">The action.</param>
		///
		public static void IterateProperties(object source, Action<Traverse> action)
		{
			var sourceTrv = Create(source);
			AccessTools.GetPropertyNames(source).ForEach(f => action(sourceTrv.Property(f)));
		}

		/// <summary>Iterate properties.</summary>
		/// <param name="source">Source for the.</param>
		/// <param name="target">Target for the.</param>
		/// <param name="action">The action.</param>
		///
		public static void IterateProperties(object source, object target, Action<Traverse, Traverse> action)
		{
			var sourceTrv = Create(source);
			var targetTrv = Create(target);
			AccessTools.GetPropertyNames(source).ForEach(f => action(sourceTrv.Property(f), targetTrv.Property(f)));
		}

		/// <summary>Iterate properties.</summary>
		/// <param name="source">Source for the.</param>
		/// <param name="target">Target for the.</param>
		/// <param name="action">The action.</param>
		///
		public static void IterateProperties(object source, object target, Action<string, Traverse, Traverse> action)
		{
			var sourceTrv = Create(source);
			var targetTrv = Create(target);
			AccessTools.GetPropertyNames(source).ForEach(f => action(f, sourceTrv.Property(f), targetTrv.Property(f)));
		}

		/// <summary>The copy fields.</summary>
		public static Action<Traverse, Traverse> CopyFields = (from, to) => { to.SetValue(from.GetValue()); };

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		///
		public override string ToString()
		{
			var value = _method ?? GetValue();
			return value?.ToString();
		}
	}
}