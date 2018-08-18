using Harmony.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Harmony
{
	/// <summary>A patches.</summary>
	public class Patches
	{
		/// <summary>The prefixes.</summary>
		public readonly ReadOnlyCollection<Patch> Prefixes;

		/// <summary>The postfixes.</summary>
		public readonly ReadOnlyCollection<Patch> Postfixes;

		/// <summary>The transpilers.</summary>
		public readonly ReadOnlyCollection<Patch> Transpilers;

		/// <summary>Gets the owners.</summary>
		/// <value>The owners.</value>
		///
		public ReadOnlyCollection<string> Owners
		{
			get
			{
				var result = new HashSet<string>();
				result.UnionWith(Prefixes.Select(p => p.owner));
				result.UnionWith(Postfixes.Select(p => p.owner));
				result.UnionWith(Transpilers.Select(p => p.owner));
				return result.ToList().AsReadOnly();
			}
		}

		/// <summary>Constructor.</summary>
		/// <param name="prefixes">	The prefixes.</param>
		/// <param name="postfixes">  The postfixes.</param>
		/// <param name="transpilers">The transpilers.</param>
		///
		public Patches(Patch[] prefixes, Patch[] postfixes, Patch[] transpilers)
		{
			if (prefixes == null) prefixes = new Patch[0];
			if (postfixes == null) postfixes = new Patch[0];
			if (transpilers == null) transpilers = new Patch[0];

			Prefixes = prefixes.ToList().AsReadOnly();
			Postfixes = postfixes.ToList().AsReadOnly();
			Transpilers = transpilers.ToList().AsReadOnly();
		}
	}

	/// <summary>A harmony instance.</summary>
	public class HarmonyInstance
	{
		readonly string id;

		/// <summary>Gets the identifier.</summary>
		/// <value>The identifier.</value>
		///
		public string Id => id;

		/// <summary>True to debug.</summary>
		public static bool DEBUG = false;

		private static bool selfPatchingDone = false;

		HarmonyInstance(string id)
		{
			if (DEBUG)
			{
				var assembly = typeof(HarmonyInstance).Assembly;
				var version = assembly.GetName().Version;
				var location = assembly.Location;
				if (location == null || location == "") location = new Uri(assembly.CodeBase).LocalPath;
				FileLog.Log("### Harmony id=" + id + ", version=" + version + ", location=" + location);
				var callingMethod = GetOutsideCaller();
				var callingAssembly = callingMethod.DeclaringType.Assembly;
				location = callingAssembly.Location;
				if (location == null || location == "") location = new Uri(callingAssembly.CodeBase).LocalPath;
				FileLog.Log("### Started from " + callingMethod.FullDescription() + ", location " + location);
				FileLog.Log("### At " + DateTime.Now.ToString("yyyy-MM-dd hh.mm.ss"));
			}

			this.id = id;

			if (!selfPatchingDone)
			{
				selfPatchingDone = true;
				SelfPatching.PatchOldHarmonyMethods();
			}
		}

		/// <summary>Creates a new HarmonyInstance.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <param name="id">The identifier.</param>
		/// <returns>A HarmonyInstance.</returns>
		///
		public static HarmonyInstance Create(string id)
		{
			if (id == null) throw new Exception("id cannot be null");
			return new HarmonyInstance(id);
		}

		private MethodBase GetOutsideCaller()
		{
			var trace = new StackTrace(true);
			foreach (var frame in trace.GetFrames())
			{
				var method = frame.GetMethod();
				if (method.DeclaringType.Namespace != typeof(HarmonyInstance).Namespace)
					return method;
			}
			throw new Exception("Unexpected end of stack trace");
		}

		/// <summary>Patch all.</summary>
		public void PatchAll()
		{
			var method = new StackTrace().GetFrame(1).GetMethod();
			var assembly = method.ReflectedType.Assembly;
			PatchAll(assembly);
		}

		/// <summary>Patch all.</summary>
		/// <param name="assembly">The assembly.</param>
		///
		public void PatchAll(Assembly assembly)
		{
			assembly.GetTypes().Do(type =>
			{
				var parentMethodInfos = type.GetHarmonyMethods();
				if (parentMethodInfos != null && parentMethodInfos.Count() > 0)
				{
					var info = HarmonyMethod.Merge(parentMethodInfos);
					var processor = new PatchProcessor(this, type, info);
					processor.Patch();
				}
			});
		}

		/// <summary>Patches.</summary>
		/// <param name="original">  The original.</param>
		/// <param name="prefix">	  (Optional) The prefix.</param>
		/// <param name="postfix">	  (Optional) The postfix.</param>
		/// <param name="transpiler">(Optional) The transpiler.</param>
		/// <returns>A DynamicMethod.</returns>
		///
		public DynamicMethod Patch(MethodBase original, HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null)
		{
			var processor = new PatchProcessor(this, new List<MethodBase> { original }, prefix, postfix, transpiler);
			return processor.Patch().FirstOrDefault();
		}

		/// <summary>Unpatch all.</summary>
		/// <param name="harmonyID">(Optional) Identifier for the harmony.</param>
		///
		public void UnpatchAll(string harmonyID = null)
		{
			bool IDCheck(Patch patchInfo) => harmonyID == null || patchInfo.owner == harmonyID;

			var originals = GetPatchedMethods().ToList();
			foreach (var original in originals)
			{
				var info = GetPatchInfo(original);
				info.Prefixes.DoIf(IDCheck, patchInfo => Unpatch(original, patchInfo.patch));
				info.Postfixes.DoIf(IDCheck, patchInfo => Unpatch(original, patchInfo.patch));
				info.Transpilers.DoIf(IDCheck, patchInfo => Unpatch(original, patchInfo.patch));
			}
		}

		/// <summary>Unpatches.</summary>
		/// <param name="original"> The original.</param>
		/// <param name="type">		 The type.</param>
		/// <param name="harmonyID">(Optional) Identifier for the harmony.</param>
		///
		public void Unpatch(MethodBase original, HarmonyPatchType type, string harmonyID = null)
		{
			var processor = new PatchProcessor(this, new List<MethodBase> { original });
			processor.Unpatch(type, harmonyID);
		}

		/// <summary>Unpatches.</summary>
		/// <param name="original">The original.</param>
		/// <param name="patch">	The patch.</param>
		///
		public void Unpatch(MethodBase original, MethodInfo patch)
		{
			var processor = new PatchProcessor(this, new List<MethodBase> { original });
			processor.Unpatch(patch);
		}

		/// <summary>Query if 'harmonyID' has any patches.</summary>
		/// <param name="harmonyID">Identifier for the harmony.</param>
		/// <returns>True if any patches, false if not.</returns>
		///
		public bool HasAnyPatches(string harmonyID)
		{
			return GetPatchedMethods()
				.Select(original => GetPatchInfo(original))
				.Any(info => info.Owners.Contains(harmonyID));
		}

		/// <summary>Gets patch information.</summary>
		/// <param name="method">The method.</param>
		/// <returns>The patch information.</returns>
		///
		public Patches GetPatchInfo(MethodBase method)
		{
			return PatchProcessor.GetPatchInfo(method);
		}

		/// <summary>Gets the patched methods in this collection.</summary>
		/// <returns>An enumerator that allows foreach to be used to process the patched methods in this collection.</returns>
		///
		public IEnumerable<MethodBase> GetPatchedMethods()
		{
			return HarmonySharedState.GetPatchedMethods();
		}

		/// <summary>Version information.</summary>
		/// <param name="currentVersion">[out] The current version.</param>
		/// <returns>A Dictionary&lt;string,Version&gt;</returns>
		///
		public Dictionary<string, Version> VersionInfo(out Version currentVersion)
		{
			currentVersion = typeof(HarmonyInstance).Assembly.GetName().Version;
			var assemblies = new Dictionary<string, Assembly>();
			GetPatchedMethods().Do(method =>
			{
				var info = HarmonySharedState.GetPatchInfo(method);
				info.prefixes.Do(fix => assemblies[fix.owner] = fix.patch.DeclaringType.Assembly);
				info.postfixes.Do(fix => assemblies[fix.owner] = fix.patch.DeclaringType.Assembly);
				info.transpilers.Do(fix => assemblies[fix.owner] = fix.patch.DeclaringType.Assembly);
			});

			var result = new Dictionary<string, Version>();
			assemblies.Do(info =>
			{
				var assemblyName = info.Value.GetReferencedAssemblies().FirstOrDefault(a => a.FullName.StartsWith("0Harmony, Version"));
				if (assemblyName != null)
					result[info.Key] = assemblyName.Version;
			});
			return result;
		}
	}
}
