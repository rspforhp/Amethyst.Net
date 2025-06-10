using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace HarmonyLib
{
	/// <summary>General extensions for common cases</summary>
	///
	public static class GeneralExtensions
	{
		/// <summary>Joins an enumeration with a value converter and a delimiter to a string</summary>
		/// <typeparam name="T">The inner type of the enumeration</typeparam>
		/// <param name="enumeration">The enumeration</param>
		/// <param name="converter">An optional value converter (from T to string)</param>
		/// <param name="delimiter">An optional delimiter</param>
		/// <returns>The values joined into a string</returns>
		///
		public static string Join<T>(this IEnumerable<T> enumeration, Func<T, string> converter = null, string delimiter = ", ")
		{
			converter ??= t => t.ToString();
			return enumeration.Aggregate("", (prev, curr) => prev + (prev.Length > 0 ? delimiter : "") + converter(curr));
		}

		/// <summary>Converts an array of types (for example methods arguments) into a human readable form</summary>
		/// <param name="parameters">The array of types</param>
		/// <returns>A human readable description including brackets</returns>
		///
		public static string Description(this Type[] parameters)
		{
			if (parameters is null)
				return "NULL";
			return $"({parameters.Join(p => p.FullDescription())})";
		}

		/// <summary>A full description of a type</summary>
		/// <param name="type">The type</param>
		/// <returns>A human readable description</returns>
		///
		public static string FullDescription(this Type type)
		{
			if (type is null)
				return "null";

			var ns = type.Namespace;
			if (string.IsNullOrEmpty(ns) is false)
				ns += ".";
			var result = ns + type.Name;

			if (type.IsGenericType)
			{
				result += "<";
				var subTypes = type.GetGenericArguments();
				for (var i = 0; i < subTypes.Length; i++)
				{
					if (result
#if NET8_0_OR_GREATER
					.EndsWith('<')
#else
					.EndsWith("<", StringComparison.Ordinal)
#endif
					is false)
						result += ", ";
					result += subTypes[i].FullDescription();
				}
				result += ">";
			}
			return result;
		}

		/// <summary>A a full description of a method or a constructor without assembly details but with generics</summary>
		/// <param name="member">The method/constructor</param>
		/// <returns>A human readable description</returns>
		///
		public static string FullDescription(this MethodBase member)
		{
			if (member is null)
				return "null";
			var returnType = AccessTools.GetReturnedType(member);

			var result = new StringBuilder();
			if (member.IsStatic)
				_ = result.Append("static ");
			if (member.IsAbstract)
				_ = result.Append("abstract ");
			if (member.IsVirtual)
				_ = result.Append("virtual ");
			_ = result.Append($"{returnType.FullDescription()} ");
			if (member.DeclaringType is not null)
				_ = result.Append($"{member.DeclaringType.FullDescription()}::");
			var parameterString = member.GetParameters().Join(p => $"{p.ParameterType.FullDescription()} {p.Name}");
			_ = result.Append($"{member.Name}({parameterString})");
			return result.ToString();
		}

		/// <summary>A helper converting parameter infos to types</summary>
		/// <param name="pinfo">The array of parameter infos</param>
		/// <returns>An array of types</returns>
		///
		public static Type[] Types(this ParameterInfo[] pinfo) => [.. pinfo.Select(pi => pi.ParameterType)];

		/// <summary>A helper to access a value via key from a dictionary</summary>
		/// <typeparam name="S">The key type</typeparam>
		/// <typeparam name="T">The value type</typeparam>
		/// <param name="dictionary">The dictionary</param>
		/// <param name="key">The key</param>
		/// <returns>The value for the key or the default value (of T) if that key does not exist</returns>
		///
		public static T GetValueSafe<S, T>(this Dictionary<S, T> dictionary, S key)
		{
			if (dictionary.TryGetValue(key, out var result))
				return result;
			return default;
		}

		/// <summary>A helper to access a value via key from a dictionary with extra casting</summary>
		/// <typeparam name="T">The value type</typeparam>
		/// <param name="dictionary">The dictionary</param>
		/// <param name="key">The key</param>
		/// <returns>The value for the key or the default value (of T) if that key does not exist or cannot be cast to T</returns>
		///
		public static T GetTypedValue<T>(this Dictionary<string, object> dictionary, string key)
		{
			if (dictionary.TryGetValue(key, out var result))
				if (result is T)
					return (T)result;
			return default;
		}

		/// <summary>Escapes Unicode and ASCII non printable characters</summary>
		/// <param name="input">The string to convert</param>
		/// <param name="quoteChar">The string to convert</param>
		/// <returns>A string literal surrounded by <paramref name="quoteChar"/></returns>
		///
		public static string ToLiteral(this string input, string quoteChar = "\"")
		{
			var literal = new StringBuilder(input.Length + 2);
			_ = literal.Append(quoteChar);
			foreach (var c in input)
			{
				switch (c)
				{
					case '\'':
						_ = literal.Append(@"\'");
						break;
					case '\"':
						_ = literal.Append("\\\"");
						break;
					case '\\':
						_ = literal.Append(@"\\");
						break;
					case '\0':
						_ = literal.Append(@"\0");
						break;
					case '\a':
						_ = literal.Append(@"\a");
						break;
					case '\b':
						_ = literal.Append(@"\b");
						break;
					case '\f':
						_ = literal.Append(@"\f");
						break;
					case '\n':
						_ = literal.Append(@"\n");
						break;
					case '\r':
						_ = literal.Append(@"\r");
						break;
					case '\t':
						_ = literal.Append(@"\t");
						break;
					case '\v':
						_ = literal.Append(@"\v");
						break;
					default:
						if (c >= 0x20 && c <= 0x7e)
							_ = literal.Append(c);
						else
						{
							_ = literal.Append(@"\u");
							_ = literal.Append(((int)c).ToString("x4"));
						}
						break;
				}
			}
			_ = literal.Append(quoteChar);
			return literal.ToString();
		}
	}

	/// <summary>Extensions for <see cref="CodeInstruction"/></summary>
	///
	public static class CodeInstructionExtensions
	{
		internal static readonly HashSet<OpCode> opcodesCalling =
		[
			OpCodes.Call,
			OpCodes.Callvirt
		];

		internal static readonly HashSet<OpCode> opcodesLoadingLocalByAddress =
		[
			OpCodes.Ldloca_S,
			OpCodes.Ldloca
		];

		internal static readonly HashSet<OpCode> opcodesLoadingLocalNormal =
		[
			OpCodes.Ldloc_0,
			OpCodes.Ldloc_1,
			OpCodes.Ldloc_2,
			OpCodes.Ldloc_3,
			OpCodes.Ldloc_S,
			OpCodes.Ldloc
		];

		internal static readonly HashSet<OpCode> opcodesStoringLocal =
		[
			OpCodes.Stloc_0,
			OpCodes.Stloc_1,
			OpCodes.Stloc_2,
			OpCodes.Stloc_3,
			OpCodes.Stloc_S,
			OpCodes.Stloc
		];

		internal static readonly HashSet<OpCode> opcodesLoadingArgumentByAddress =
		[
			OpCodes.Ldarga_S,
			OpCodes.Ldarga
		];

		internal static readonly HashSet<OpCode> opcodesLoadingArgumentNormal =
		[
			OpCodes.Ldarg_0,
			OpCodes.Ldarg_1,
			OpCodes.Ldarg_2,
			OpCodes.Ldarg_3,
			OpCodes.Ldarg_S,
			OpCodes.Ldarg
		];

		internal static readonly HashSet<OpCode> opcodesStoringArgument =
		[
			OpCodes.Starg_S,
			OpCodes.Starg
		];

		internal static readonly HashSet<OpCode> opcodesBranching =
		[
			OpCodes.Br_S,
			OpCodes.Brfalse_S,
			OpCodes.Brtrue_S,
			OpCodes.Beq_S,
			OpCodes.Bge_S,
			OpCodes.Bgt_S,
			OpCodes.Ble_S,
			OpCodes.Blt_S,
			OpCodes.Bne_Un_S,
			OpCodes.Bge_Un_S,
			OpCodes.Bgt_Un_S,
			OpCodes.Ble_Un_S,
			OpCodes.Blt_Un_S,
			OpCodes.Br,
			OpCodes.Brfalse,
			OpCodes.Brtrue,
			OpCodes.Beq,
			OpCodes.Bge,
			OpCodes.Bgt,
			OpCodes.Ble,
			OpCodes.Blt,
			OpCodes.Bne_Un,
			OpCodes.Bge_Un,
			OpCodes.Bgt_Un,
			OpCodes.Ble_Un,
			OpCodes.Blt_Un
		];

		static readonly HashSet<OpCode> constantLoadingCodes =
		[
			OpCodes.Ldc_I4_M1,
			OpCodes.Ldc_I4_0,
			OpCodes.Ldc_I4_1,
			OpCodes.Ldc_I4_2,
			OpCodes.Ldc_I4_3,
			OpCodes.Ldc_I4_4,
			OpCodes.Ldc_I4_5,
			OpCodes.Ldc_I4_6,
			OpCodes.Ldc_I4_7,
			OpCodes.Ldc_I4_8,
			OpCodes.Ldc_I4,
			OpCodes.Ldc_I4_S,
			OpCodes.Ldc_I8,
			OpCodes.Ldc_R4,
			OpCodes.Ldc_R8,
			OpCodes.Ldstr
		];

		/// <summary>Returns if an <see cref="OpCode"/> is initialized and valid</summary>
		/// <param name="code">The <see cref="OpCode"/></param>
		/// <returns></returns>
		public static bool IsValid(this OpCode code) => code.Size > 0;

	}

	/// <summary>Extensions for a sequence of <see cref="CodeInstruction"/></summary>
	///


	/// <summary>General extensions for collections</summary>
	/// 
	public static class CollectionExtensions
	{
		/// <summary>A simple way to execute code for every element in a collection</summary>
		/// <typeparam name="T">The inner type of the collection</typeparam>
		/// <param name="sequence">The collection</param>
		/// <param name="action">The action to execute</param>
		///
		public static void Do<T>(this IEnumerable<T> sequence, Action<T> action)
		{
			if (sequence is null)
				return;
			var enumerator = sequence.GetEnumerator();
			while (enumerator.MoveNext())
				action(enumerator.Current);
		}

		/// <summary>A simple way to execute code for elements in a collection matching a condition</summary>
		/// <typeparam name="T">The inner type of the collection</typeparam>
		/// <param name="sequence">The collection</param>
		/// <param name="condition">The predicate</param>
		/// <param name="action">The action to execute</param>
		///
		public static void DoIf<T>(this IEnumerable<T> sequence, Func<T, bool> condition, Action<T> action) => sequence.Where(condition).Do(action);

		/// <summary>A helper to add an item to a collection</summary>
		/// <typeparam name="T">The inner type of the collection</typeparam>
		/// <param name="sequence">The collection</param>
		/// <param name="item">The item to add</param>
		/// <returns>The collection containing the item</returns>
		///
		[SuppressMessage("Style", "IDE0300")]
		public static IEnumerable<T> AddItem<T>(this IEnumerable<T> sequence, T item) => (sequence ?? []).Concat(new T[] { item });

		/// <summary>A helper to add an item to an array</summary>
		/// <typeparam name="T">The inner type of the collection</typeparam>
		/// <param name="sequence">The array</param>
		/// <param name="item">The item to add</param>
		/// <returns>The array containing the item</returns>
		///
		public static T[] AddToArray<T>(this T[] sequence, T item) => [.. AddItem(sequence, item)];

		/// <summary>A helper to add items to an array</summary>
		/// <typeparam name="T">The inner type of the collection</typeparam>
		/// <param name="sequence">The array</param>
		/// <param name="items">The items to add</param>
		/// <returns>The array containing the items</returns>
		///
		public static T[] AddRangeToArray<T>(this T[] sequence, T[] items) => [.. (sequence ?? Enumerable.Empty<T>()), .. items];

		// NOTE: These extension methods may collide with extension methods from other libraries users may be using,
		// just due to their general nature and naming commonality.
		// This is also a concern for the existing public extension methods in this file,
		// but it's too late to make such extension method internal like these.

		// Returns a new dictionary with entries merged from given dictionaries.
		// For key collisions, latter dictionary values are favored.
		// None of the given dictionaries are mutated.
		internal static Dictionary<K, V> Merge<K, V>(this IEnumerable<KeyValuePair<K, V>> firstDict, params IEnumerable<KeyValuePair<K, V>>[] otherDicts)
		{
			var dict = new Dictionary<K, V>();
			foreach (var pair in firstDict)
				dict[pair.Key] = pair.Value;
			foreach (var otherDict in otherDicts)
			{
				foreach (var pair in otherDict)
					dict[pair.Key] = pair.Value;
			}
			return dict;
		}

		// Returns a new dictionary copied from given dictionary with keys run through a transform function.
		internal static Dictionary<K, V> TransformKeys<K, V>(this Dictionary<K, V> origDict, Func<K, K> transform)
		{
			var dict = new Dictionary<K, V>();
			foreach (var pair in origDict)
				dict.Add(transform(pair.Key), pair.Value);
			return dict;
		}
	}

	/// <summary>General extensions for collections</summary>
	/// 
	public static class MethodBaseExtensions
	{
		/// <summary>Tests a class member if it has an IL method body (external methods for example don't have a body)</summary>
		/// <param name="member">The member to test</param>
		/// <returns>Returns true if the member has an IL body or false if not</returns>
		public static bool HasMethodBody(this MethodBase member) => (member.GetMethodBody()?.GetILAsByteArray()?.Length ?? 0) > 0;
	}
}
