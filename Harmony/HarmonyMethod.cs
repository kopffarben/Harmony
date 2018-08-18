using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Harmony
{
	/// <summary>A harmony method.</summary>
	public class HarmonyMethod
	{
		/// <summary>The method.</summary>
		public MethodInfo method; // need to be called 'method'

		public Type declaringType;
		public string methodName;
		public MethodType? methodType;
		public Type[] argumentTypes;
		public int prioritiy = -1;
		public string[] before;
		public string[] after;

		public HarmonyMethod()
		{
		}

		void ImportMethod(MethodInfo theMethod)
		{
			method = theMethod;
			if (method != null)
			{
				var infos = method.GetHarmonyMethods();
				if (infos != null)
					Merge(infos).CopyTo(this);
			}
		}

		/// <summary>Constructor.</summary>
		/// <param name="method">The method.</param>
		///
		public HarmonyMethod(MethodInfo method)
		{
			ImportMethod(method);
		}

		/// <summary>Constructor.</summary>
		/// <param name="type">		  The type.</param>
		/// <param name="name">		  The name.</param>
		/// <param name="parameters">(Optional) Options for controlling the operation.</param>
		///
		public HarmonyMethod(Type type, string name, Type[] parameters = null)
		{
			var method = AccessTools.Method(type, name, parameters);
			ImportMethod(method);
		}

		/// <summary>Harmony fields.</summary>
		/// <returns>A List&lt;string&gt;</returns>
		///
		public static List<string> HarmonyFields()
		{
			return AccessTools
				.GetFieldNames(typeof(HarmonyMethod))
				.Where(s => s != "method")
				.ToList();
		}

		/// <summary>Merges the given attributes.</summary>
		/// <param name="attributes">The attributes.</param>
		/// <returns>A HarmonyMethod.</returns>
		///
		public static HarmonyMethod Merge(List<HarmonyMethod> attributes)
		{
			var result = new HarmonyMethod();
			if (attributes == null) return result;
			var resultTrv = Traverse.Create(result);
			attributes.ForEach(attribute =>
			{
				var trv = Traverse.Create(attribute);
				HarmonyFields().ForEach(f =>
				{
					var val = trv.Field(f).GetValue();
					if (val != null)
						resultTrv.Field(f).SetValue(val);
				});
			});
			return result;
		}

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		///
		public override string ToString()
		{
			var result = "HarmonyMethod[";
			var trv = Traverse.Create(this);
			HarmonyFields().ForEach(f =>
			{
				result += f + '=' + trv.Field(f).GetValue();
			});
			return result + "]";
		}
	}

	/// <summary>A harmony method extensions.</summary>
	public static class HarmonyMethodExtensions
	{
		/// <summary>A HarmonyMethod extension method that copies to.</summary>
		/// <param name="from">from to act on.</param>
		/// <param name="to">  to.</param>
		///
		public static void CopyTo(this HarmonyMethod from, HarmonyMethod to)
		{
			if (to == null) return;
			var fromTrv = Traverse.Create(from);
			var toTrv = Traverse.Create(to);
			HarmonyMethod.HarmonyFields().ForEach(f =>
			{
				var val = fromTrv.Field(f).GetValue();
				if (val != null) toTrv.Field(f).SetValue(val);
			});
		}

		/// <summary>A HarmonyMethod extension method that makes a deep copy of this HarmonyMethodExtensions.</summary>
		/// <param name="original">The original to act on.</param>
		/// <returns>A copy of this HarmonyMethodExtensions.</returns>
		///
		public static HarmonyMethod Clone(this HarmonyMethod original)
		{
			var result = new HarmonyMethod();
			original.CopyTo(result);
			return result;
		}

		/// <summary>A HarmonyMethod extension method that merges.</summary>
		/// <param name="master">The master to act on.</param>
		/// <param name="detail">The detail.</param>
		/// <returns>A HarmonyMethod.</returns>
		///
		public static HarmonyMethod Merge(this HarmonyMethod master, HarmonyMethod detail)
		{
			if (detail == null) return master;
			var result = new HarmonyMethod();
			var resultTrv = Traverse.Create(result);
			var masterTrv = Traverse.Create(master);
			var detailTrv = Traverse.Create(detail);
			HarmonyMethod.HarmonyFields().ForEach(f =>
			{
				var baseValue = masterTrv.Field(f).GetValue();
				var detailValue = detailTrv.Field(f).GetValue();
				resultTrv.Field(f).SetValue(detailValue ?? baseValue);
			});
			return result;
		}

		/// <summary>A MethodBase extension method that gets harmony methods.</summary>
		/// <param name="type">The type to act on.</param>
		/// <returns>The harmony methods.</returns>
		///
		public static List<HarmonyMethod> GetHarmonyMethods(this Type type)
		{
			return type.GetCustomAttributes(true)
						.Where(attr => attr is HarmonyAttribute)
						.Cast<HarmonyAttribute>()
						.Select(attr => attr.info)
						.ToList();
		}

		/// <summary>A MethodBase extension method that gets harmony methods.</summary>
		/// <param name="method">The method to act on.</param>
		/// <returns>The harmony methods.</returns>
		///
		public static List<HarmonyMethod> GetHarmonyMethods(this MethodBase method)
		{
			if (method is DynamicMethod) return new List<HarmonyMethod>();
			return method.GetCustomAttributes(true)
						.Where(attr => attr is HarmonyAttribute)
						.Cast<HarmonyAttribute>()
						.Select(attr => attr.info)
						.ToList();
		}
	}
}