using System;
using System.Collections.Generic;

namespace Harmony
{
	/// <summary>Specifies the type of method</summary>
	public enum MethodType
	{
		/// <summary>This is a normal method</summary>
		Normal,
		/// <summary>This is a getter</summary>
		Getter,
		/// <summary>This is a setter</summary>
		Setter,
		/// <summary>This is a constructor</summary>
		Constructor,
		/// <summary>This is a static constructor</summary>
		StaticConstructor
	}

	/// <summary>[Obsolete] Specifies the type of property</summary>
	[Obsolete("This enum will be removed in the next major version. To define special methods, use MethodType")]
	public enum PropertyMethod
	{
		/// <summary>[Obsolete] This is a getter</summary>
		Getter,
		/// <summary>[Obsolete] This is a setter</summary>
		Setter
	}

	/// <summary>Specifies the type of argument</summary>
	public enum ArgumentType
	{
		/// <summary>This is a normal argument</summary>
		Normal,
		/// <summary>This is a reference argument (ref)</summary>
		Ref,
		/// <summary>This is an out argument (out)</summary>
		Out,
		/// <summary>This is a pointer argument (&amp;)</summary>
		Pointer
	}

	/// <summary>Specifies the type of patch</summary>
	public enum HarmonyPatchType
	{
		/// <summary>Any patch</summary>
		All,
		/// <summary>A prefix patch</summary>
		Prefix,
		/// <summary>A postfix patch</summary>
		Postfix,
		/// <summary>A transpiler</summary>
		Transpiler
	}

	/// <summary>The base class for all Harmony annotations (not meant to be used directly)</summary>
	public class HarmonyAttribute : Attribute
	{
		/// <summary>The common information for all attributes</summary>
		public HarmonyMethod info = new HarmonyMethod();
	}

	/// <summary>The main Harmony annotation class</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class HarmonyPatch : HarmonyAttribute
	{
		/// <summary>An empty annotation can be used together with TargetMethod(s)</summary>
		///
		public HarmonyPatch()
		{
		}

		/// <summary>An annotation that specifies a class to patch</summary>
		/// <param name="declaringType">The declaring class</param>
		///
		public HarmonyPatch(Type declaringType)
		{
			info.declaringType = declaringType;
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="declaringType">The declaring class</param>
		/// <param name="argumentTypes">The argument types of the method or constructor to patch</param>
		///
		public HarmonyPatch(Type declaringType, Type[] argumentTypes)
		{
			info.declaringType = declaringType;
			info.argumentTypes = argumentTypes;
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="declaringType">The declaring class</param>
		/// <param name="methodName">The name of the method, property or constructor to patch</param>
		///
		public HarmonyPatch(Type declaringType, string methodName)
		{
			info.declaringType = declaringType;
			info.methodName = methodName;
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="declaringType">The declaring class</param>
		/// <param name="methodName">The name of the method, property or constructor to patch</param>
		/// <param name="argumentTypes">An array of argument types to target overloads</param>
		///
		public HarmonyPatch(Type declaringType, string methodName, params Type[] argumentTypes)
		{
			info.declaringType = declaringType;
			info.methodName = methodName;
			info.argumentTypes = argumentTypes;
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="declaringType">The declaring class</param>
		/// <param name="methodName">The name of the method, property or constructor to patch</param>
		/// <param name="argumentTypes">An array of argument types to target overloads</param>
		/// <param name="argumentVariations">An array of extra argument subtypes (ref, out, pointer)</param>
		///
		public HarmonyPatch(Type declaringType, string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations)
		{
			info.declaringType = declaringType;
			info.methodName = methodName;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="declaringType">The declaring class</param>
		/// <param name="methodType">The type of entry: method, getter, setter or constructor</param>
		///
		public HarmonyPatch(Type declaringType, MethodType methodType)
		{
			info.declaringType = declaringType;
			info.methodType = methodType;
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="declaringType">The declaring class</param>
		/// <param name="methodType">The type of entry: method, getter, setter or constructor</param>
		/// <param name="argumentTypes">An array of argument types to target overloads</param>
		///
		public HarmonyPatch(Type declaringType, MethodType methodType, params Type[] argumentTypes)
		{
			info.declaringType = declaringType;
			info.methodType = methodType;
			info.argumentTypes = argumentTypes;
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="declaringType">The declaring class</param>
		/// <param name="methodType">The type of entry: method, getter, setter or constructor</param>
		/// <param name="argumentTypes">An array of argument types to target overloads</param>
		/// <param name="argumentVariations">An array of extra argument subtypes (ref, out, pointer)</param>
		///
		public HarmonyPatch(Type declaringType, MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations)
		{
			info.declaringType = declaringType;
			info.methodType = methodType;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="declaringType">The declaring class</param>
		/// <param name="methodName">The name of the method, property or constructor to patch</param>
		/// <param name="methodType">The type of entry: method, getter, setter or constructor</param>
		///
		public HarmonyPatch(Type declaringType, string methodName, MethodType methodType)
		{
			info.declaringType = declaringType;
			info.methodName = methodName;
			info.methodType = methodType;
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="methodName">The name of the method, property or constructor to patch</param>
		///
		public HarmonyPatch(string methodName)
		{
			info.methodName = methodName;
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="methodName">The name of the method, property or constructor to patch</param>
		/// <param name="argumentTypes">An array of argument types to target overloads</param>
		///
		public HarmonyPatch(string methodName, params Type[] argumentTypes)
		{
			info.methodName = methodName;
			info.argumentTypes = argumentTypes;
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="methodName">The name of the method, property or constructor to patch</param>
		/// <param name="argumentTypes">An array of argument types to target overloads</param>
		/// <param name="argumentVariations">An array of extra argument subtypes (ref, out, pointer)</param>
		///
		public HarmonyPatch(string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations)
		{
			info.methodName = methodName;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="methodName">The name of the method, property or constructor to patch</param>
		/// <param name="methodType">The type of entry: method, getter, setter or constructor</param>
		///
		public HarmonyPatch(string methodName, MethodType methodType)
		{
			info.methodName = methodName;
			info.methodType = methodType;
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="methodType">The type of entry: method, getter, setter or constructor</param>
		///
		public HarmonyPatch(MethodType methodType)
		{
			info.methodType = methodType;
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="methodType">The type of entry: method, getter, setter or constructor</param>
		/// <param name="argumentTypes">An array of argument types to target overloads</param>
		///
		public HarmonyPatch(MethodType methodType, params Type[] argumentTypes)
		{
			info.methodType = methodType;
			info.argumentTypes = argumentTypes;
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="methodType">The type of entry: method, getter, setter or constructor</param>
		/// <param name="argumentTypes">An array of argument types to target overloads</param>
		/// <param name="argumentVariations">An array of extra argument subtypes (ref, out, pointer)</param>
		///
		public HarmonyPatch(MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations)
		{
			info.methodType = methodType;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="argumentTypes">An array of argument types to target overloads</param>
		///
		public HarmonyPatch(Type[] argumentTypes)
		{
			info.argumentTypes = argumentTypes;
		}

		/// <summary>An annotation that specifies a method, property or constructor to patch</summary>
		/// <param name="argumentTypes">An array of argument types to target overloads</param>
		/// <param name="argumentVariations">An array of extra argument subtypes (ref, out, pointer)</param>
		///
		public HarmonyPatch(Type[] argumentTypes, ArgumentType[] argumentVariations)
		{
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		/// <summary>[Obsolete] An annotation that specifies a property to patch</summary>
		/// <param name="propertyName">Name of the property</param>
		/// <param name="type">The type</param>
		///
		[Obsolete("This attribute will be removed in the next major version. Use HarmonyPatch together with MethodType.Getter or MethodType.Setter instead")]
		public HarmonyPatch(string propertyName, PropertyMethod type)
		{
			info.methodName = propertyName;
			info.methodType = type == PropertyMethod.Getter ? MethodType.Getter : MethodType.Setter;
		}

		//

		private void ParseSpecialArguments(Type[] argumentTypes, ArgumentType[] argumentVariations)
		{
			if (argumentVariations == null || argumentVariations.Length == 0)
			{
				info.argumentTypes = argumentTypes;
				return;
			}

			if (argumentTypes.Length < argumentVariations.Length)
				throw new ArgumentException("argumentVariations contains more elements than argumentTypes", nameof(argumentVariations));

			var types = new List<Type>();
			for (var i = 0; i < argumentTypes.Length; i++)
			{
				var type = argumentTypes[i];
				switch (argumentVariations[i])
				{
					case ArgumentType.Ref:
					case ArgumentType.Out:
						type = type.MakeByRefType();
						break;
					case ArgumentType.Pointer:
						type = type.MakePointerType();
						break;
				}
				types.Add(type);
			}
			info.argumentTypes = types.ToArray();
		}
	}

	/// <summary>A Harmony annotation</summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class HarmonyPatchAll : HarmonyAttribute
	{
		/// <summary>A Harmony annotation to define that all methods in a class are to be patched</summary>
		public HarmonyPatchAll()
		{
		}
	}

	/// <summary>A Harmony annotation</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class HarmonyPriority : HarmonyAttribute
	{
		/// <summary>A Harmony annotation to define patch priority</summary>
		/// <param name="priority">The priority</param>
		///
		public HarmonyPriority(int priority)
		{
			info.priority = priority;
		}
	}

	/// <summary>A Harmony annotation</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class HarmonyBefore : HarmonyAttribute
	{
		/// <summary>A Harmony annotation to define that a patch comes before another patch</summary>
		/// <param name="before">The harmony ID of the other patch</param>
		///
		public HarmonyBefore(params string[] before)
		{
			info.before = before;
		}
	}

	/// <summary>A Harmony annotation</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class HarmonyAfter : HarmonyAttribute
	{
		/// <summary>A Harmony annotation to define that a patch comes after another patch</summary>
		/// <param name="after">The harmony ID of the other patch</param>
		///
		public HarmonyAfter(params string[] after)
		{
			info.after = after;
		}
	}

	/// <summary>Specifies the Prepare function in a patch class</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyPrepare : Attribute
	{
	}

	/// <summary>Specifies the Cleanup function in a patch class</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyCleanup : Attribute
	{
	}

	/// <summary>Specifies the TargetMethod function in a patch class</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyTargetMethod : Attribute
	{
	}

	/// <summary>Specifies the TargetMethods function in a patch class</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyTargetMethods : Attribute
	{
	}

	/// <summary>Specifies the Prefix function in a patch class</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyPrefix : Attribute
	{
	}

	/// <summary>Specifies the Postfix function in a patch class</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyPostfix : Attribute
	{
	}

	/// <summary>Specifies the Transpiler function in a patch class</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyTranspiler : Attribute
	{
	}

	/// <summary>A Harmony annotation</summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class HarmonyArgument : Attribute
	{
		/// <summary>The name of the original argument</summary>
		public string OriginalName { get; private set; }

		/// <summary>The index of the original argument</summary>
		public int Index { get; private set; }

		/// <summary>The new name of the original argument</summary>
		public string NewName { get; private set; }

		/// <summary>An annotation to declare injected arguments by name</summary>
		public HarmonyArgument(string originalName) : this(originalName, null)
		{
		}

		/// <summary>An annotation to declare injected arguments by index</summary>
		/// <param name="index">Zero-based index</param>
		///
		public HarmonyArgument(int index) : this(index, null)
		{
		}

		/// <summary>An annotation to declare injected arguments by renaming them</summary>
		/// <param name="originalName">Name of the original argument</param>
		/// <param name="newName">New name</param>
		///
		public HarmonyArgument(string originalName, string newName)
		{
			OriginalName = originalName;
			Index = -1;
			NewName = newName;
		}

		/// <summary>An annotation to declare injected arguments by index and renaming them</summary>
		/// <param name="index">Zero-based index</param>
		/// <param name="name">New name</param>
		///
		public HarmonyArgument(int index, string name)
		{
			OriginalName = null;
			Index = index;
			NewName = name;
		}
	}

	// This attribute is for Harmony patching itself to the latest
	//
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
	internal class UpgradeToLatestVersion : Attribute
	{
		/// <summary>The version.</summary>
		public int version;

		public UpgradeToLatestVersion(int version)
		{
			this.version = version;
		}
	}
}