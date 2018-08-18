// file:	Attributes.cs
//
// summary:	Implements the attributes class
/// 
using System;
using System.Collections.Generic;

namespace Harmony
{
	/// <summary>Values that represent method types.</summary>
	public enum MethodType
	{
		/// <summary>An enum constant representing the normal option.</summary>
		Normal,
		/// <summary>An enum constant representing the getter option.</summary>
		Getter,
		/// <summary>An enum constant representing the setter option.</summary>
		Setter,
		/// <summary>An enum constant representing the constructor option.</summary>
		Constructor,
		/// <summary>An enum constant representing the static constructor option.</summary>
		StaticConstructor
	}

	[Obsolete("This enum will be removed in the next major version. To define special methods, use MethodType")]
	public enum PropertyMethod
	{
		/// <summary>An enum constant representing the getter option.</summary>
		Getter,
		/// <summary>An enum constant representing the setter option.</summary>
		Setter
	}

	/// <summary>Values that represent argument types.</summary>
	public enum ArgumentType
	{
		/// <summary>An enum constant representing the normal option.</summary>
		Normal,
		/// <summary>An enum constant representing the Reference option.</summary>
		Ref,
		/// <summary>An enum constant representing the out option.</summary>
		Out,
		/// <summary>An enum constant representing the pointer option.</summary>
		Pointer
	}

	/// <summary>Values that represent harmony patch types.</summary>
	public enum HarmonyPatchType
	{
		/// <summary>An enum constant representing all option.</summary>
		All,
		/// <summary>An enum constant representing the prefix option.</summary>
		Prefix,
		/// <summary>An enum constant representing the postfix option.</summary>
		Postfix,
		/// <summary>An enum constant representing the transpiler option.</summary>
		Transpiler
	}

	public class HarmonyAttribute : Attribute
	{
		public HarmonyMethod info = new HarmonyMethod();
	}

	/// <summary>A harmony patch.</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class HarmonyPatch : HarmonyAttribute
	{
		public HarmonyPatch()
		{
		}

		/// <summary>starting with 'Type'.</summary>
		/// <param name="declaringType">Type of the declaring.</param>
		///
		public HarmonyPatch(Type declaringType)
		{
			info.declaringType = declaringType;
		}

		/// <summary>Constructor.</summary>
		/// <param name="declaringType">Type of the declaring.</param>
		/// <param name="argumentTypes">List of types of the arguments.</param>
		///
		public HarmonyPatch(Type declaringType, Type[] argumentTypes)
		{
			info.declaringType = declaringType;
			info.argumentTypes = argumentTypes;
		}

		/// <summary>Constructor.</summary>
		/// <param name="declaringType">Type of the declaring.</param>
		/// <param name="methodName">	  Name of the method.</param>
		///
		public HarmonyPatch(Type declaringType, string methodName)
		{
			info.declaringType = declaringType;
			info.methodName = methodName;
		}

		/// <summary>Constructor.</summary>
		/// <param name="declaringType">Type of the declaring.</param>
		/// <param name="methodName">	  Name of the method.</param>
		/// <param name="argumentTypes">List of types of the arguments.</param>
		///
		public HarmonyPatch(Type declaringType, string methodName, params Type[] argumentTypes)
		{
			info.declaringType = declaringType;
			info.methodName = methodName;
			info.argumentTypes = argumentTypes;
		}

		/// <summary>Constructor.</summary>
		/// <param name="declaringType">		 Type of the declaring.</param>
		/// <param name="methodName">			 Name of the method.</param>
		/// <param name="argumentTypes">		 List of types of the arguments.</param>
		/// <param name="argumentVariations">The argument variations.</param>
		///
		public HarmonyPatch(Type declaringType, string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations)
		{
			info.declaringType = declaringType;
			info.methodName = methodName;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		/// <summary>Constructor.</summary>
		/// <param name="declaringType">Type of the declaring.</param>
		/// <param name="methodType">	  Type of the method.</param>
		///
		public HarmonyPatch(Type declaringType, MethodType methodType)
		{
			info.declaringType = declaringType;
			info.methodType = methodType;
		}

		/// <summary>Constructor.</summary>
		/// <param name="declaringType">Type of the declaring.</param>
		/// <param name="methodType">	  Type of the method.</param>
		/// <param name="argumentTypes">List of types of the arguments.</param>
		///
		public HarmonyPatch(Type declaringType, MethodType methodType, params Type[] argumentTypes)
		{
			info.declaringType = declaringType;
			info.methodType = methodType;
			info.argumentTypes = argumentTypes;
		}

		/// <summary>Constructor.</summary>
		/// <param name="declaringType">		 Type of the declaring.</param>
		/// <param name="methodType">			 Type of the method.</param>
		/// <param name="argumentTypes">		 List of types of the arguments.</param>
		/// <param name="argumentVariations">The argument variations.</param>
		///
		public HarmonyPatch(Type declaringType, MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations)
		{
			info.declaringType = declaringType;
			info.methodType = methodType;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		/// <summary>Constructor.</summary>
		/// <param name="declaringType">Type of the declaring.</param>
		/// <param name="propertyName"> Name of the property.</param>
		/// <param name="methodType">	  Type of the method.</param>
		///
		public HarmonyPatch(Type declaringType, string propertyName, MethodType methodType)
		{
			info.declaringType = declaringType;
			info.methodName = propertyName;
			info.methodType = methodType;
		}

		/// <summary>starting with 'string'.</summary>
		/// <param name="methodName">Name of the method.</param>
		///
		public HarmonyPatch(string methodName)
		{
			info.methodName = methodName;
		}

		/// <summary>Constructor.</summary>
		/// <param name="methodName">	  Name of the method.</param>
		/// <param name="argumentTypes">List of types of the arguments.</param>
		///
		public HarmonyPatch(string methodName, params Type[] argumentTypes)
		{
			info.methodName = methodName;
			info.argumentTypes = argumentTypes;
		}

		/// <summary>Constructor.</summary>
		/// <param name="methodName">			 Name of the method.</param>
		/// <param name="argumentTypes">		 List of types of the arguments.</param>
		/// <param name="argumentVariations">The argument variations.</param>
		///
		public HarmonyPatch(string methodName, Type[] argumentTypes, ArgumentType[] argumentVariations)
		{
			info.methodName = methodName;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		/// <summary>Constructor.</summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="methodType">  Type of the method.</param>
		///
		public HarmonyPatch(string propertyName, MethodType methodType)
		{
			info.methodName = propertyName;
			info.methodType = methodType;
		}

		/// <summary>starting with 'MethodType'.</summary>
		/// <param name="methodType">Type of the method.</param>
		///
		public HarmonyPatch(MethodType methodType)
		{
			info.methodType = methodType;
		}

		/// <summary>Constructor.</summary>
		/// <param name="methodType">	  Type of the method.</param>
		/// <param name="argumentTypes">List of types of the arguments.</param>
		///
		public HarmonyPatch(MethodType methodType, params Type[] argumentTypes)
		{
			info.methodType = methodType;
			info.argumentTypes = argumentTypes;
		}

		/// <summary>Constructor.</summary>
		/// <param name="methodType">			 Type of the method.</param>
		/// <param name="argumentTypes">		 List of types of the arguments.</param>
		/// <param name="argumentVariations">The argument variations.</param>
		///
		public HarmonyPatch(MethodType methodType, Type[] argumentTypes, ArgumentType[] argumentVariations)
		{
			info.methodType = methodType;
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

		/// <summary>starting with 'Type[]'.</summary>
		/// <param name="argumentTypes">List of types of the arguments.</param>
		///
		public HarmonyPatch(Type[] argumentTypes)
		{
			info.argumentTypes = argumentTypes;
		}

		/// <summary>Constructor.</summary>
		/// <param name="argumentTypes">		 List of types of the arguments.</param>
		/// <param name="argumentVariations">The argument variations.</param>
		///
		public HarmonyPatch(Type[] argumentTypes, ArgumentType[] argumentVariations)
		{
			ParseSpecialArguments(argumentTypes, argumentVariations);
		}

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

	/// <summary>A harmony patch all.</summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class HarmonyPatchAll : HarmonyAttribute
	{
		/// <summary>Default constructor.</summary>
		public HarmonyPatchAll()
		{
		}
	}

	/// <summary>A harmony priority.</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class HarmonyPriority : HarmonyAttribute
	{
		/// <summary>Constructor.</summary>
		/// <param name="prioritiy">The prioritiy.</param>
		///
		public HarmonyPriority(int prioritiy)
		{
			info.prioritiy = prioritiy;
		}
	}

	/// <summary>A harmony before.</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class HarmonyBefore : HarmonyAttribute
	{
		/// <summary>Constructor.</summary>
		/// <param name="before">A variable-length parameters list containing before.</param>
		///
		public HarmonyBefore(params string[] before)
		{
			info.before = before;
		}
	}

	/// <summary>A harmony after.</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class HarmonyAfter : HarmonyAttribute
	{
		/// <summary>Constructor.</summary>
		/// <param name="after">A variable-length parameters list containing after.</param>
		///
		public HarmonyAfter(params string[] after)
		{
			info.after = after;
		}
	}

	/// <summary>
	///   If you don't want to use the special method names you can annotate
	///   using the following attributes:
	/// </summary>
	///
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyPrepare : Attribute
	{
	}

	/// <summary>A harmony cleanup.</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyCleanup : Attribute
	{
	}

	/// <summary>A harmony target method.</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyTargetMethod : Attribute
	{
	}

	/// <summary>A harmony target methods.</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyTargetMethods : Attribute
	{
	}

	/// <summary>A harmony prefix.</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyPrefix : Attribute
	{
	}

	/// <summary>A harmony postfix.</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyPostfix : Attribute
	{
	}

	/// <summary>A harmony transpiler.</summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class HarmonyTranspiler : Attribute
	{
	}

	/// <summary>A harmony argument.</summary>
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class HarmonyArgument : Attribute
	{
		/// <summary>Gets the name of the original.</summary>
		/// <value>The name of the original.</value>
		///
		public string OriginalName { get; private set; }

		/// <summary>Gets the zero-based index of this HarmonyArgument.</summary>
		/// <value>The index.</value>
		///
		public int Index { get; private set; }

		/// <summary>Gets the name of the new.</summary>
		/// <value>The name of the new.</value>
		///
		public string NewName { get; private set; }

		/// <summary>Constructor.</summary>
		/// <param name="originalName">Name of the original.</param>
		///
		public HarmonyArgument(string originalName) : this(originalName, null)
		{
		}

		/// <summary>Constructor.</summary>
		/// <param name="index">Zero-based index of the.</param>
		///
		public HarmonyArgument(int index) : this(index, null)
		{
		}

		/// <summary>Constructor.</summary>
		/// <param name="originalName">Name of the original.</param>
		/// <param name="newName">		 Name of the new.</param>
		///
		public HarmonyArgument(string originalName, string newName)
		{
			OriginalName = originalName;
			Index = -1;
			NewName = newName;
		}

		/// <summary>Constructor.</summary>
		/// <param name="index">Zero-based index of the.</param>
		/// <param name="name"> The name.</param>
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