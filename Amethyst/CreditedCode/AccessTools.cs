using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading;

#if NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
using System.Runtime.CompilerServices;
#endif

namespace HarmonyLib
{
	// ---------------------------------------------------------------------------------------------------------------------
	// NOTE:
	// When adding or updating methods that have a first argument of 'Type type, please create/update a corresponding method
	// and xml documentation in AccessToolsExtensions!
	// ---------------------------------------------------------------------------------------------------------------------

	/// <summary>A helper class for reflection related functions</summary>
	///
	public static class AccessTools
	{
		private static Type[] allTypesCached = null;

		/// <summary>Shortcut for <see cref="BindingFlags"/> to simplify the use of reflections and make it work for any access level</summary>
		///
		public static readonly BindingFlags all = BindingFlags.Public // This should a be const, but changing from static (readonly) to const breaks binary compatibility.
			| BindingFlags.NonPublic
			| BindingFlags.Instance
			| BindingFlags.Static
			| BindingFlags.GetField
			| BindingFlags.SetField
			| BindingFlags.GetProperty
			| BindingFlags.SetProperty;

		/// <summary>Shortcut for <see cref="BindingFlags"/> to simplify the use of reflections and make it work for any access level but only within the current type</summary>
		///
		public static readonly BindingFlags allDeclared = all | BindingFlags.DeclaredOnly; // This should a be const, but changing from static (readonly) to const breaks binary compatibility.

		/// <summary>Enumerates all assemblies in the current app domain, excluding visual studio assemblies</summary>
		/// <returns>An enumeration of <see cref="Assembly"/></returns>
		///
		public static IEnumerable<Assembly> AllAssemblies() => AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Microsoft.VisualStudio") is false);

		/// <summary>Gets a type by name. Prefers a full name with namespace but falls back to the first type matching the name otherwise</summary>
		/// <param name="name">The name</param>
		/// <returns>A type or null if not found</returns>
		///
		public static Type TypeByName(string name)
		{
			var localType = Type.GetType(name, false);
			if (localType is not null)
				return localType;

			foreach (var assembly in AllAssemblies())
			{
				var specificType = assembly.GetType(name, false);
				if (specificType is not null)
					return specificType;
			}

			var allTypes = AllTypes().ToArray();

			var fullType = allTypes.FirstOrDefault(t => t.FullName == name);
			if (fullType is not null)
				return fullType;

			var partialType = allTypes.FirstOrDefault(t => t.Name == name);
			if (partialType is not null)
				return partialType;

			Debug.WriteLine($"AccessTools.TypeByName: Could not find type named {name}");
			return null;
		}

		/// <summary>Searches a type by regular expression. For exact searching, use <see cref="AccessTools.TypeByName(string)"/>.</summary>
		/// <param name="search">The regular expression that matches against Type.FullName or Type.Name</param>
		/// <param name="invalidateCache">Refetches the cached types if set to true</param>
		/// <returns>The first type where FullName or Name matches the search</returns>
		///
		public static Type TypeSearch(Regex search, bool invalidateCache = false)
		{
			if (allTypesCached == null || invalidateCache)
				allTypesCached = [.. AllTypes()];

			var fullType = allTypesCached.FirstOrDefault(t => search.IsMatch(t.FullName));
			if (fullType is not null)
				return fullType;

			var partialType = allTypesCached.FirstOrDefault(t => search.IsMatch(t.Name));
			if (partialType is not null)
				return partialType;

			Debug.WriteLine($"AccessTools.TypeSearch: Could not find type with regular expression {search}");
			return null;
		}

		/// <summary>Clears the type cache that <see cref="AccessTools.TypeSearch(Regex, bool)" /> uses.</summary>
		///
		public static void ClearTypeSearchCache() => allTypesCached = null;

		/// <summary>Gets all successfully loaded types from a given assembly</summary>
		/// <param name="assembly">The assembly</param>
		/// <returns>An array of types</returns>
		/// <remarks>
		/// This calls and returns <see cref="Assembly.GetTypes"/>, while catching any thrown <see cref="ReflectionTypeLoadException"/>.
		/// If such an exception is thrown, returns the successfully loaded types (<see cref="ReflectionTypeLoadException.Types"/>,
		/// filtered for non-null values).
		/// </remarks>
		///
		public static Type[] GetTypesFromAssembly(Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				Debug.WriteLine($"AccessTools.GetTypesFromAssembly: assembly {assembly} => {ex}");
				return [.. ex.Types.Where(type => type is not null)];
			}
		}

		/// <summary>Enumerates all successfully loaded types in the current app domain, excluding visual studio assemblies</summary>
		/// <returns>An enumeration of all <see cref="Type"/> in all assemblies, excluding visual studio assemblies</returns>
		///
		public static IEnumerable<Type> AllTypes() => AllAssemblies().SelectMany(GetTypesFromAssembly);

		/// <summary>Enumerates all inner types (non-recursive) of a given type</summary>
		/// <param name="type">The class/type to start with</param>
		/// <returns>An enumeration of all inner <see cref="Type"/></returns>
		///
		public static IEnumerable<Type> InnerTypes(Type type) => type.GetNestedTypes(all);

		/// <summary>Applies a function going up the type hierarchy and stops at the first non-<c>null</c> result</summary>
		/// <typeparam name="T">Result type of func()</typeparam>
		/// <param name="type">The class/type to start with</param>
		/// <param name="func">The evaluation function returning T</param>
		/// <returns>The first non-<c>null</c> result, or <c>null</c> if no match</returns>
		/// <remarks>
		/// The type hierarchy of a class or value type (including struct) does NOT include implemented interfaces,
		/// and the type hierarchy of an interface is only itself (regardless of whether that interface implements other interfaces).
		/// The top-most type in the type hierarchy of all non-interface types (including value types) is <see cref="object"/>.
		/// </remarks>
		///
		public static T FindIncludingBaseTypes<T>(Type type, Func<Type, T> func) where T : class
		{
			while (true)
			{
				var result = func(type);
				if (result is object)
					return result;
				type = type.BaseType;
				if (type is null)
					return null;
			}
		}

		/// <summary>Applies a function going into inner types and stops at the first non-<c>null</c> result</summary>
		/// <typeparam name="T">Generic type parameter</typeparam>
		/// <param name="type">The class/type to start with</param>
		/// <param name="func">The evaluation function returning T</param>
		/// <returns>The first non-<c>null</c> result, or <c>null</c> if no match</returns>
		///
		public static T FindIncludingInnerTypes<T>(Type type, Func<Type, T> func) where T : class
		{
			var result = func(type);
			if (result is object)
				return result;
			foreach (var subType in type.GetNestedTypes(all))
			{
				result = FindIncludingInnerTypes(subType, func);
				if (result is object)
					break;
			}
			return result;
		}


		/// <summary>Gets the reflection information for a directly declared field</summary>
		/// <param name="type">The class/type where the field is defined</param>
		/// <param name="name">The name of the field</param>
		/// <returns>A field or null when type/name is null or when the field cannot be found</returns>
		///
		public static FieldInfo DeclaredField(Type type, string name)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.DeclaredField: type is null");
				return null;
			}
			if (string.IsNullOrEmpty(name))
			{
				Debug.WriteLine("AccessTools.DeclaredField: name is null/empty");
				return null;
			}
			var fieldInfo = type.GetField(name, allDeclared);
			if (fieldInfo is null)
				Debug.WriteLine($"AccessTools.DeclaredField: Could not find field for type {type} and name {name}");
			return fieldInfo;
		}

		/// <summary>Gets the reflection information for a directly declared field</summary>
		/// <param name="typeColonName">The member in the form <c>TypeFullName:MemberName</c>, where TypeFullName matches the form recognized by <a href="https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype">Type.GetType</a> like <c>Some.Namespace.Type</c>.</param>
		/// <returns>A field or null when the field cannot be found</returns>
		///
		public static FieldInfo DeclaredField(string typeColonName)
		{
			var info = Tools.TypColonName(typeColonName);
			var fieldInfo = info.type.GetField(info.name, allDeclared);
			if (fieldInfo is null)
				Debug.WriteLine($"AccessTools.DeclaredField: Could not find field for type {info.type} and name {info.name}");
			return fieldInfo;
		}

		/// <summary>Gets the reflection information for a field by searching the type and all its super types</summary>
		/// <param name="type">The class/type where the field is defined</param>
		/// <param name="name">The name of the field (case sensitive)</param>
		/// <returns>A field or null when type/name is null or when the field cannot be found</returns>
		///
		public static FieldInfo Field(Type type, string name)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.Field: type is null");
				return null;
			}
			if (string.IsNullOrEmpty(name))
			{
				Debug.WriteLine("AccessTools.Field: name is null/empty");
				return null;
			}
			var fieldInfo = FindIncludingBaseTypes(type, t => t.GetField(name, all));
			if (fieldInfo is null)
				Debug.WriteLine($"AccessTools.Field: Could not find field for type {type} and name {name}");
			return fieldInfo;
		}

		/// <summary>Gets the reflection information for a field by searching the type and all its super types</summary>
		/// <param name="typeColonName">The member in the form <c>TypeFullName:MemberName</c>, where TypeFullName matches the form recognized by <a href="https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype">Type.GetType</a> like <c>Some.Namespace.Type</c>.</param>
		/// <returns>A field or null when the field cannot be found</returns>
		///
		public static FieldInfo Field(string typeColonName)
		{
			var info = Tools.TypColonName(typeColonName);
			var fieldInfo = FindIncludingBaseTypes(info.type, t => t.GetField(info.name, all));
			if (fieldInfo is null)
				Debug.WriteLine($"AccessTools.Field: Could not find field for type {info.type} and name {info.name}");
			return fieldInfo;
		}

		/// <summary>Gets the reflection information for a field</summary>
		/// <param name="type">The class/type where the field is declared</param>
		/// <param name="idx">The zero-based index of the field inside the class definition</param>
		/// <returns>A field or null when type is null or when the field cannot be found</returns>
		///
		public static FieldInfo DeclaredField(Type type, int idx)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.DeclaredField: type is null");
				return null;
			}
			var fieldInfo = GetDeclaredFields(type).ElementAtOrDefault(idx);
			if (fieldInfo is null)
				Debug.WriteLine($"AccessTools.DeclaredField: Could not find field for type {type} and idx {idx}");
			return fieldInfo;
		}

		/// <summary>Gets the reflection information for a directly declared property</summary>
		/// <param name="type">The class/type where the property is declared</param>
		/// <param name="name">The name of the property (case sensitive)</param>
		/// <returns>A property or null when type/name is null or when the property cannot be found</returns>
		///
		public static PropertyInfo DeclaredProperty(Type type, string name)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.DeclaredProperty: type is null");
				return null;
			}
			if (string.IsNullOrEmpty(name))
			{
				Debug.WriteLine("AccessTools.DeclaredProperty: name is null/empty");
				return null;
			}
			var property = type.GetProperty(name, allDeclared);
			if (property is null)
				Debug.WriteLine($"AccessTools.DeclaredProperty: Could not find property for type {type} and name {name}");
			return property;
		}

		/// <summary>Gets the reflection information for a directly declared property</summary>
		/// <param name="typeColonName">The member in the form <c>TypeFullName:MemberName</c>, where TypeFullName matches the form recognized by <a href="https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype">Type.GetType</a> like <c>Some.Namespace.Type</c>.</param>
		/// <returns>A property or null when the property cannot be found</returns>
		///
		public static PropertyInfo DeclaredProperty(string typeColonName)
		{
			var info = Tools.TypColonName(typeColonName);
			var property = info.type.GetProperty(info.name, allDeclared);
			if (property is null)
				Debug.WriteLine($"AccessTools.DeclaredProperty: Could not find property for type {info.type} and name {info.name}");
			return property;
		}

		/// <summary>Gets the reflection information for a directly declared indexer property</summary>
		/// <param name="type">The class/type where the indexer property is declared</param>
		/// <param name="parameters">Optional parameters to target a specific overload of multiple indexers</param>
		/// <returns>An indexer property or null when type is null or when it cannot be found</returns>
		///
		public static PropertyInfo DeclaredIndexer(Type type, Type[] parameters = null)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.DeclaredIndexer: type is null");
				return null;
			}

			try
			{
				// Can find multiple indexers without specified parameters, but only one with specified ones
				var indexer = parameters is null ?
					type.GetProperties(allDeclared).SingleOrDefault(property => property.GetIndexParameters().Length > 0)
					: type.GetProperties(allDeclared).FirstOrDefault(property => property.GetIndexParameters().Select(param => param.ParameterType).SequenceEqual(parameters));

				if (indexer is null)
					Debug.WriteLine($"AccessTools.DeclaredIndexer: Could not find indexer for type {type} and parameters {parameters?.Description()}");

				return indexer;
			}
			catch (InvalidOperationException ex)
			{
				throw new AmbiguousMatchException("Multiple possible indexers were found.", ex);
			}
		}

		/// <summary>Gets the reflection information for the getter method of a directly declared property</summary>
		/// <param name="type">The class/type where the property is declared</param>
		/// <param name="name">The name of the property (case sensitive)</param>
		/// <returns>A method or null when type/name is null or when the property cannot be found</returns>
		///
		public static MethodInfo DeclaredPropertyGetter(Type type, string name) => DeclaredProperty(type, name)?.GetGetMethod(true);

		/// <summary>Gets the reflection information for the getter method of a directly declared property</summary>
		/// <param name="typeColonName">The member in the form <c>TypeFullName:MemberName</c>, where TypeFullName matches the form recognized by <a href="https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype">Type.GetType</a> like <c>Some.Namespace.Type</c>.</param>
		/// <returns>A method or null when the property cannot be found</returns>
		///
		public static MethodInfo DeclaredPropertyGetter(string typeColonName) => DeclaredProperty(typeColonName)?.GetGetMethod(true);

		/// <summary>Gets the reflection information for the getter method of a directly declared indexer property</summary>
		/// <param name="type">The class/type where the indexer property is declared</param>
		/// <param name="parameters">Optional parameters to target a specific overload of multiple indexers</param>
		/// <returns>A method or null when type is null or when indexer property cannot be found</returns>
		///
		public static MethodInfo DeclaredIndexerGetter(Type type, Type[] parameters = null) => DeclaredIndexer(type, parameters)?.GetGetMethod(true);

		/// <summary>Gets the reflection information for the setter method of a directly declared property</summary>
		/// <param name="type">The class/type where the property is declared</param>
		/// <param name="name">The name of the property (case sensitive)</param>
		/// <returns>A method or null when type/name is null or when the property cannot be found</returns>
		///
		public static MethodInfo DeclaredPropertySetter(Type type, string name) => DeclaredProperty(type, name)?.GetSetMethod(true);

		/// <summary>Gets the reflection information for the Setter method of a directly declared property</summary>
		/// <param name="typeColonName">The member in the form <c>TypeFullName:MemberName</c>, where TypeFullName matches the form recognized by <a href="https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype">Type.GetType</a> like <c>Some.Namespace.Type</c>.</param>
		/// <returns>A method or null when the property cannot be found</returns>
		///
		public static MethodInfo DeclaredPropertySetter(string typeColonName) => DeclaredProperty(typeColonName)?.GetSetMethod(true);

		/// <summary>Gets the reflection information for the setter method of a directly declared indexer property</summary>
		/// <param name="type">The class/type where the indexer property is declared</param>
		/// <param name="parameters">Optional parameters to target a specific overload of multiple indexers</param>
		/// <returns>A method or null when type is null or when indexer property cannot be found</returns>
		///
		public static MethodInfo DeclaredIndexerSetter(Type type, Type[] parameters) => DeclaredIndexer(type, parameters)?.GetSetMethod(true);

		/// <summary>Gets the reflection information for a property by searching the type and all its super types</summary>
		/// <param name="type">The class/type</param>
		/// <param name="name">The name</param>
		/// <returns>A property or null when type/name is null or when the property cannot be found</returns>
		///
		public static PropertyInfo Property(Type type, string name)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.Property: type is null");
				return null;
			}
			if (string.IsNullOrEmpty(name))
			{
				Debug.WriteLine("AccessTools.Property: name is null/empty");
				return null;
			}
			var property = FindIncludingBaseTypes(type, t => t.GetProperty(name, all));
			if (property is null)
				Debug.WriteLine($"AccessTools.Property: Could not find property for type {type} and name {name}");
			return property;
		}

		/// <summary>Gets the reflection information for a property by searching the type and all its super types</summary>
		/// <param name="typeColonName">The member in the form <c>TypeFullName:MemberName</c>, where TypeFullName matches the form recognized by <a href="https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype">Type.GetType</a> like <c>Some.Namespace.Type</c>.</param>
		/// <returns>A property or null when the property cannot be found</returns>
		///
		public static PropertyInfo Property(string typeColonName)
		{
			var info = Tools.TypColonName(typeColonName);
			var property = FindIncludingBaseTypes(info.type, t => t.GetProperty(info.name, all));
			if (property is null)
				Debug.WriteLine($"AccessTools.Property: Could not find property for type {info.type} and name {info.name}");
			return property;
		}

		/// <summary>Gets the reflection information for an indexer property by searching the type and all its super types</summary>
		/// <param name="type">The class/type</param>
		/// <param name="parameters">Optional parameters to target a specific overload of multiple indexers</param>
		/// <returns>An indexer property or null when type is null or when it cannot be found</returns>
		///
		public static PropertyInfo Indexer(Type type, Type[] parameters = null)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.Indexer: type is null");
				return null;
			}

			// Can find multiple indexers without specified parameters, but only one with specified ones
			Func<Type, PropertyInfo> func = parameters is null ?
				t => t.GetProperties(all).SingleOrDefault(property => property.GetIndexParameters().Length > 0)
				: t => t.GetProperties(all).FirstOrDefault(property => property.GetIndexParameters().Select(param => param.ParameterType).SequenceEqual(parameters));

			try
			{
				var indexer = FindIncludingBaseTypes(type, func);

				if (indexer is null)
					Debug.WriteLine($"AccessTools.Indexer: Could not find indexer for type {type} and parameters {parameters?.Description()}");

				return indexer;
			}
			catch (InvalidOperationException ex)
			{
				throw new AmbiguousMatchException("Multiple possible indexers were found.", ex);
			}
		}

		/// <summary>Gets the reflection information for the getter method of a property by searching the type and all its super types</summary>
		/// <param name="type">The class/type</param>
		/// <param name="name">The name</param>
		/// <returns>A method or null when type/name is null or when the property cannot be found</returns>
		///
		public static MethodInfo PropertyGetter(Type type, string name) => Property(type, name)?.GetGetMethod(true);

		/// <summary>Gets the reflection information for the getter method of a property by searching the type and all its super types</summary>
		/// <param name="typeColonName">The member in the form <c>TypeFullName:MemberName</c>, where TypeFullName matches the form recognized by <a href="https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype">Type.GetType</a> like <c>Some.Namespace.Type</c>.</param>
		/// <returns>A method or null when type/name is null or when the property cannot be found</returns>
		///
		public static MethodInfo PropertyGetter(string typeColonName) => Property(typeColonName)?.GetGetMethod(true);

		/// <summary>Gets the reflection information for the getter method of an indexer property by searching the type and all its super types</summary>
		/// <param name="type">The class/type</param>
		/// <param name="parameters">Optional parameters to target a specific overload of multiple indexers</param>
		/// <returns>A method or null when type is null or when the indexer property cannot be found</returns>
		///
		public static MethodInfo IndexerGetter(Type type, Type[] parameters = null) => Indexer(type, parameters)?.GetGetMethod(true);

		/// <summary>Gets the reflection information for the setter method of a property by searching the type and all its super types</summary>
		/// <param name="type">The class/type</param>
		/// <param name="name">The name</param>
		/// <returns>A method or null when type/name is null or when the property cannot be found</returns>
		///
		public static MethodInfo PropertySetter(Type type, string name) => Property(type, name)?.GetSetMethod(true);

		/// <summary>Gets the reflection information for the setter method of a property by searching the type and all its super types</summary>
		/// <param name="typeColonName">The member in the form <c>TypeFullName:MemberName</c>, where TypeFullName matches the form recognized by <a href="https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype">Type.GetType</a> like <c>Some.Namespace.Type</c>.</param>
		/// <returns>A method or null when type/name is null or when the property cannot be found</returns>
		///
		public static MethodInfo PropertySetter(string typeColonName) => Property(typeColonName)?.GetSetMethod(true);

		/// <summary>Gets the reflection information for the setter method of an indexer property by searching the type and all its super types</summary>
		/// <param name="type">The class/type</param>
		/// <param name="parameters">Optional parameters to target a specific overload of multiple indexers</param>
		/// <returns>A method or null when type is null or when the indexer property cannot be found</returns>
		///
		public static MethodInfo IndexerSetter(Type type, Type[] parameters = null) => Indexer(type, parameters)?.GetSetMethod(true);

		/// <summary>Gets the reflection information for a directly declared method</summary>
		/// <param name="type">The class/type where the method is declared</param>
		/// <param name="name">The name of the method (case sensitive)</param>
		/// <param name="parameters">Optional parameters to target a specific overload of the method</param>
		/// <param name="generics">Optional list of types that define the generic version of the method</param>
		/// <returns>A method or null when type/name is null or when the method cannot be found</returns>
		///
		public static MethodInfo DeclaredMethod(Type type, string name, Type[] parameters = null, Type[] generics = null)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.DeclaredMethod: type is null");
				return null;
			}
			if (string.IsNullOrEmpty(name))
			{
				Debug.WriteLine("AccessTools.DeclaredMethod: name is null/empty");
				return null;
			}
			MethodInfo result;
			var modifiers = new ParameterModifier[] { };

			if (parameters is null)
				result = type.GetMethod(name, allDeclared);
			else
				result = type.GetMethod(name, allDeclared, null, parameters, modifiers);

			if (result is null)
			{
				Debug.WriteLine($"AccessTools.DeclaredMethod: Could not find method for type {type} and name {name} and parameters {parameters?.Description()}");
				return null;
			}

			if (generics is not null)
				result = result.MakeGenericMethod(generics);
			return result;
		}

		/// <summary>Gets the reflection information for a directly declared method</summary>
		/// <param name="typeColonName">The member in the form <c>TypeFullName:MemberName</c>, where TypeFullName matches the form recognized by <a href="https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype">Type.GetType</a> like <c>Some.Namespace.Type</c>.</param>
		/// <param name="parameters">Optional parameters to target a specific overload of the method</param>
		/// <param name="generics">Optional list of types that define the generic version of the method</param>
		/// <returns>A method or null when the method cannot be found</returns>
		///
		public static MethodInfo DeclaredMethod(string typeColonName, Type[] parameters = null, Type[] generics = null)
		{
			var info = Tools.TypColonName(typeColonName);
			return DeclaredMethod(info.type, info.name, parameters, generics);
		}

		/// <summary>Gets the reflection information for a method by searching the type and all its super types</summary>
		/// <param name="type">The class/type where the method is declared</param>
		/// <param name="name">The name of the method (case sensitive)</param>
		/// <param name="parameters">Optional parameters to target a specific overload of the method</param>
		/// <param name="generics">Optional list of types that define the generic version of the method</param>
		/// <returns>A method or null when type/name is null or when the method cannot be found</returns>
		///
		public static MethodInfo Method(Type type, string name, Type[] parameters = null, Type[] generics = null)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.Method: type is null");
				return null;
			}
			if (string.IsNullOrEmpty(name))
			{
				Debug.WriteLine("AccessTools.Method: name is null/empty");
				return null;
			}
			MethodInfo result;
			var modifiers = new ParameterModifier[] { };
			if (parameters is null)
			{
				try
				{
					result = FindIncludingBaseTypes(type, t => t.GetMethod(name, all));
				}
				catch (AmbiguousMatchException ex)
				{
					result = FindIncludingBaseTypes(type, t => t.GetMethod(name, all, null, [], modifiers));
					if (result is null)
					{
						throw new AmbiguousMatchException($"Ambiguous match in Harmony patch for {type}:{name}", ex);
					}
				}
			}
			else
			{
				result = FindIncludingBaseTypes(type, t => t.GetMethod(name, all, null, parameters, modifiers));
			}

			if (result is null)
			{
				Debug.WriteLine($"AccessTools.Method: Could not find method for type {type} and name {name} and parameters {parameters?.Description()}");
				return null;
			}

			if (generics is not null)
				result = result.MakeGenericMethod(generics);
			return result;
		}

		/// <summary>Gets the reflection information for a method by searching the type and all its super types</summary>
		/// <param name="typeColonName">The member in the form <c>TypeFullName:MemberName</c>, where TypeFullName matches the form recognized by <a href="https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype">Type.GetType</a> like <c>Some.Namespace.Type</c>.</param>
		/// <param name="parameters">Optional parameters to target a specific overload of the method</param>
		/// <param name="generics">Optional list of types that define the generic version of the method</param>
		/// <returns>A method or null when the method cannot be found</returns>
		///
		public static MethodInfo Method(string typeColonName, Type[] parameters = null, Type[] generics = null)
		{
			var info = Tools.TypColonName(typeColonName);
			return Method(info.type, info.name, parameters, generics);
		}



#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
		/// <summary>Gets the <see cref="IAsyncStateMachine.MoveNext"/> method of an async method's state machine</summary>
		/// <param name="method">Async method that creates the state machine internally</param>
		/// <returns>The internal <see cref="IAsyncStateMachine.MoveNext"/> method of the async state machine or <b>null</b> if no valid async method is detected</returns>
		///
		public static MethodInfo AsyncMoveNext(MethodBase method)
		{
			if (method is null)
			{
				Debug.WriteLine("AccessTools.AsyncMoveNext: method is null");
				return null;
			}

			var asyncAttribute = method.GetCustomAttribute<AsyncStateMachineAttribute>();
			if (asyncAttribute is null)
			{
				Debug.WriteLine($"AccessTools.AsyncMoveNext: Could not find AsyncStateMachine for {method.FullDescription()}");
				return null;
			}

			var asyncStateMachineType = asyncAttribute.StateMachineType;
			var asyncMethodBody = DeclaredMethod(asyncStateMachineType, nameof(IAsyncStateMachine.MoveNext));
			if (asyncMethodBody is null)
			{
				Debug.WriteLine($"AccessTools.AsyncMoveNext: Could not find async method body for {method.FullDescription()}");
				return null;
			}

			return asyncMethodBody;
		}
#endif

		/// <summary>Gets the names of all method that are declared in a type</summary>
		/// <param name="type">The declaring class/type</param>
		/// <returns>A list of method names</returns>
		///
		public static List<string> GetMethodNames(Type type)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.GetMethodNames: type is null");
				return [];
			}
			return [.. GetDeclaredMethods(type).Select(m => m.Name)];
		}

		/// <summary>Gets the names of all method that are declared in the type of the instance</summary>
		/// <param name="instance">An instance of the type to search in</param>
		/// <returns>A list of method names</returns>
		///
		public static List<string> GetMethodNames(object instance)
		{
			if (instance is null)
			{
				Debug.WriteLine("AccessTools.GetMethodNames: instance is null");
				return [];
			}
			return GetMethodNames(instance.GetType());
		}

		/// <summary>Gets the names of all fields that are declared in a type</summary>
		/// <param name="type">The declaring class/type</param>
		/// <returns>A list of field names</returns>
		///
		public static List<string> GetFieldNames(Type type)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.GetFieldNames: type is null");
				return [];
			}
			return [.. GetDeclaredFields(type).Select(f => f.Name)];
		}

		/// <summary>Gets the names of all fields that are declared in the type of the instance</summary>
		/// <param name="instance">An instance of the type to search in</param>
		/// <returns>A list of field names</returns>
		///
		public static List<string> GetFieldNames(object instance)
		{
			if (instance is null)
			{
				Debug.WriteLine("AccessTools.GetFieldNames: instance is null");
				return [];
			}
			return GetFieldNames(instance.GetType());
		}

		/// <summary>Gets the names of all properties that are declared in a type</summary>
		/// <param name="type">The declaring class/type</param>
		/// <returns>A list of property names</returns>
		///
		public static List<string> GetPropertyNames(Type type)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.GetPropertyNames: type is null");
				return [];
			}
			return [.. GetDeclaredProperties(type).Select(f => f.Name)];
		}

		/// <summary>Gets the names of all properties that are declared in the type of the instance</summary>
		/// <param name="instance">An instance of the type to search in</param>
		/// <returns>A list of property names</returns>
		///
		public static List<string> GetPropertyNames(object instance)
		{
			if (instance is null)
			{
				Debug.WriteLine("AccessTools.GetPropertyNames: instance is null");
				return [];
			}
			return GetPropertyNames(instance.GetType());
		}

		/// <summary>Gets the type of any class member of</summary>
		/// <param name="member">A member</param>
		/// <returns>The class/type of this member</returns>
		///
		public static Type GetUnderlyingType(this MemberInfo member)
		{
			return member.MemberType switch
			{
				MemberTypes.Event => ((EventInfo)member).EventHandlerType,
				MemberTypes.Field => ((FieldInfo)member).FieldType,
				MemberTypes.Method => ((MethodInfo)member).ReturnType,
				MemberTypes.Property => ((PropertyInfo)member).PropertyType,
				_ => throw new ArgumentException("Member must be of type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"),
			};
		}

		/// <summary>Returns a <see cref="MethodInfo"/> by searching for module-id and token</summary>
		/// <param name="moduleGUID">The module of the method</param>
		/// <param name="token">The token of the method</param>
		/// <returns></returns>
		public static MethodInfo GetMethodByModuleAndToken(string moduleGUID, int token)
		{
#if NET5_0_OR_GREATER
			Module module = null;
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var moduleVersionGUID = new Guid(moduleGUID);
			Parallel.ForEach(assemblies, (assembly) =>
			{
				var allModules = assembly.GetModules();
				for (var i = 0; i < allModules.Length; i++)
					if (allModules[i].ModuleVersionId == moduleVersionGUID)
					{
						module = allModules[i];
						break;
					}
			});
#else
			var module = AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => !a.FullName.StartsWith("Microsoft.VisualStudio"))
				.SelectMany(a => a.GetLoadedModules())
				.First(m => m.ModuleVersionId.ToString() == moduleGUID);
#endif
			return module == null ? null : (MethodInfo)module.ResolveMethod(token);
		}

		/// <summary>Test if a class member is actually an concrete implementation</summary>
		/// <param name="member">A member</param>
		/// <returns>True if the member is a declared</returns>
		///
		public static bool IsDeclaredMember<T>(this T member) where T : MemberInfo => member.DeclaringType == member.ReflectedType;

		/// <summary>Gets the real implementation of a class member</summary>
		/// <param name="member">A member</param>
		/// <returns>The member itself if its declared. Otherwise the member that is actually implemented in some base type</returns>
		///
		public static T GetDeclaredMember<T>(this T member) where T : MemberInfo
		{
			if (member.DeclaringType is null || member.IsDeclaredMember())
				return member;

			var metaToken = member.MetadataToken;
			var members = member.DeclaringType?.GetMembers(all) ?? [];
			foreach (var other in members)
				if (other.MetadataToken == metaToken)
					return (T)other;

			return member;
		}

		/// <summary>Gets the reflection information for a directly declared constructor</summary>
		/// <param name="type">The class/type where the constructor is declared</param>
		/// <param name="parameters">Optional parameters to target a specific overload of the constructor</param>
		/// <param name="searchForStatic">Optional parameters to only consider static constructors</param>
		/// <returns>A constructor info or null when type is null or when the constructor cannot be found</returns>
		///
		public static ConstructorInfo DeclaredConstructor(Type type, Type[] parameters = null, bool searchForStatic = false)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.DeclaredConstructor: type is null");
				return null;
			}
			parameters ??= [];
			var flags = searchForStatic ? allDeclared & ~BindingFlags.Instance : allDeclared & ~BindingFlags.Static;
			return type.GetConstructor(flags, null, parameters, []);
		}

		/// <summary>Gets the reflection information for a constructor by searching the type and all its super types</summary>
		/// <param name="type">The class/type where the constructor is declared</param>
		/// <param name="parameters">Optional parameters to target a specific overload of the method</param>
		/// <param name="searchForStatic">Optional parameters to only consider static constructors</param>
		/// <returns>A constructor info or null when type is null or when the method cannot be found</returns>
		///
		public static ConstructorInfo Constructor(Type type, Type[] parameters = null, bool searchForStatic = false)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.ConstructorInfo: type is null");
				return null;
			}
			parameters ??= [];
			var flags = searchForStatic ? all & ~BindingFlags.Instance : all & ~BindingFlags.Static;
			return FindIncludingBaseTypes(type, t => t.GetConstructor(flags, null, parameters, []));
		}

		/// <summary>Gets reflection information for all declared constructors</summary>
		/// <param name="type">The class/type where the constructors are declared</param>
		/// <param name="searchForStatic">Optional parameters to only consider static constructors</param>
		/// <returns>A list of constructor infos</returns>
		///
		public static List<ConstructorInfo> GetDeclaredConstructors(Type type, bool? searchForStatic = null)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.GetDeclaredConstructors: type is null");
				return [];
			}
			var flags = allDeclared;
			if (searchForStatic.HasValue)
				flags = searchForStatic.Value ? flags & ~BindingFlags.Instance : flags & ~BindingFlags.Static;
			return [.. type.GetConstructors(flags).Where(method => method.DeclaringType == type)];
		}

		/// <summary>Gets reflection information for all declared methods</summary>
		/// <param name="type">The class/type where the methods are declared</param>
		/// <returns>A list of methods</returns>
		///
		public static List<MethodInfo> GetDeclaredMethods(Type type)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.GetDeclaredMethods: type is null");
				return [];
			}
			return [.. type.GetMethods(allDeclared)];
		}

		/// <summary>Gets reflection information for all declared properties</summary>
		/// <param name="type">The class/type where the properties are declared</param>
		/// <returns>A list of properties</returns>
		///
		public static List<PropertyInfo> GetDeclaredProperties(Type type)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.GetDeclaredProperties: type is null");
				return [];
			}
			return [.. type.GetProperties(allDeclared)];
		}

		/// <summary>Gets reflection information for all declared fields</summary>
		/// <param name="type">The class/type where the fields are declared</param>
		/// <returns>A list of fields</returns>
		///
		public static List<FieldInfo> GetDeclaredFields(Type type)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.GetDeclaredFields: type is null");
				return [];
			}
			return [.. type.GetFields(allDeclared)];
		}

		/// <summary>Gets the return type of a method or constructor</summary>
		/// <param name="methodOrConstructor">The method/constructor</param>
		/// <returns>The return type</returns>
		///
		public static Type GetReturnedType(MethodBase methodOrConstructor)
		{
			if (methodOrConstructor is null)
			{
				Debug.WriteLine("AccessTools.GetReturnedType: methodOrConstructor is null");
				return null;
			}
			var constructor = methodOrConstructor as ConstructorInfo;
			if (constructor is not null)
				return typeof(void);
			return ((MethodInfo)methodOrConstructor).ReturnType;
		}

		/// <summary>Given a type, returns the first inner type matching a recursive search by name</summary>
		/// <param name="type">The class/type to start searching at</param>
		/// <param name="name">The name of the inner type (case sensitive)</param>
		/// <returns>The inner type or null if type/name is null or if a type with that name cannot be found</returns>
		///
		public static Type Inner(Type type, string name)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.Inner: type is null");
				return null;
			}
			if (string.IsNullOrEmpty(name))
			{
				Debug.WriteLine("AccessTools.Inner: name is null/empty");
				return null;
			}
			return FindIncludingBaseTypes(type, t => t.GetNestedType(name, all));
		}

		/// <summary>Given a type, returns the first inner type matching a recursive search with a predicate</summary>
		/// <param name="type">The class/type to start searching at</param>
		/// <param name="predicate">The predicate to search with</param>
		/// <returns>The inner type or null if type/predicate is null or if a type with that name cannot be found</returns>
		///
		public static Type FirstInner(Type type, Func<Type, bool> predicate)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.FirstInner: type is null");
				return null;
			}
			if (predicate is null)
			{
				Debug.WriteLine("AccessTools.FirstInner: predicate is null");
				return null;
			}
			return type.GetNestedTypes(all).FirstOrDefault(subType => predicate(subType));
		}

		/// <summary>Given a type, returns the first method matching a predicate</summary>
		/// <param name="type">The class/type to start searching at</param>
		/// <param name="predicate">The predicate to search with</param>
		/// <returns>The method or null if type/predicate is null or if a type with that name cannot be found</returns>
		///
		public static MethodInfo FirstMethod(Type type, Func<MethodInfo, bool> predicate)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.FirstMethod: type is null");
				return null;
			}
			if (predicate is null)
			{
				Debug.WriteLine("AccessTools.FirstMethod: predicate is null");
				return null;
			}
			return type.GetMethods(allDeclared).FirstOrDefault(method => predicate(method));
		}

		/// <summary>Given a type, returns the first constructor matching a predicate</summary>
		/// <param name="type">The class/type to start searching at</param>
		/// <param name="predicate">The predicate to search with</param>
		/// <returns>The constructor info or null if type/predicate is null or if a type with that name cannot be found</returns>
		///
		public static ConstructorInfo FirstConstructor(Type type, Func<ConstructorInfo, bool> predicate)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.FirstConstructor: type is null");
				return null;
			}
			if (predicate is null)
			{
				Debug.WriteLine("AccessTools.FirstConstructor: predicate is null");
				return null;
			}
			return type.GetConstructors(allDeclared).FirstOrDefault(constructor => predicate(constructor));
		}

		/// <summary>Given a type, returns the first property matching a predicate</summary>
		/// <param name="type">The class/type to start searching at</param>
		/// <param name="predicate">The predicate to search with</param>
		/// <returns>The property or null if type/predicate is null or if a type with that name cannot be found</returns>
		///
		public static PropertyInfo FirstProperty(Type type, Func<PropertyInfo, bool> predicate)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.FirstProperty: type is null");
				return null;
			}
			if (predicate is null)
			{
				Debug.WriteLine("AccessTools.FirstProperty: predicate is null");
				return null;
			}
			return type.GetProperties(allDeclared).FirstOrDefault(property => predicate(property));
		}

		/// <summary>Returns an array containing the type of each object in the given array</summary>
		/// <param name="parameters">An array of objects</param>
		/// <returns>An array of types or an empty array if parameters is null (if an object is null, the type for it will be object)</returns>
		///
		public static Type[] GetTypes(object[] parameters)
		{
			if (parameters is null)
				return [];
			return [.. parameters.Select(p => p is null ? typeof(object) : p.GetType())];
		}

		/// <summary>Creates an array of input parameters for a given method and a given set of potential inputs</summary>
		/// <param name="method">The method/constructor you are planing to call</param>
		/// <param name="inputs"> The possible input parameters in any order</param>
		/// <returns>An object array matching the method signature</returns>
		///
		public static object[] ActualParameters(MethodBase method, object[] inputs)
		{
			var inputTypes = inputs.Select(obj => obj?.GetType()).ToList();
			return [.. method.GetParameters().Select(p => p.ParameterType).Select(pType =>
			{
				var index = inputTypes.FindIndex(inType => inType is not null && pType.IsAssignableFrom(inType));
				if (index >= 0)
					return inputs[index];
				return GetDefaultValue(pType);
			})];
		}

#if NET35
		static readonly MethodInfo m_PrepForRemoting = Method(typeof(Exception), "PrepForRemoting") // MS .NET
			?? Method(typeof(Exception), "FixRemotingException"); // mono .NET
		static readonly FastInvokeHandler PrepForRemoting = MethodInvoker.GetHandler(m_PrepForRemoting);
#endif

		/// <summary>Rethrows an exception while preserving its stack trace (throw statement typically clobbers existing stack traces)</summary>
		/// <param name="exception">The exception to rethrow</param>
		///
		public static void RethrowException(Exception exception)
		{
#if NET35
			_ = PrepForRemoting(exception);
#else
			System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
#endif
			// For the sake of any static code analyzer, always throw exception, even if ExceptionDispatchInfo.Throw above was called.
			throw exception;
		}

		/// <summary>True if the current runtime is based on Mono, false otherwise (.NET)</summary>
		///
		public static bool IsMonoRuntime { get; } = Type.GetType("Mono.Runtime") is not null;

		/// <summary>True if the current runtime is .NET Framework, false otherwise (.NET Core or Mono, although latter isn't guaranteed)</summary>
		///
		public static bool IsNetFrameworkRuntime { get; } =
			Type.GetType("System.Runtime.InteropServices.RuntimeInformation", false)?.GetProperty("FrameworkDescription")
			.GetValue(null, null).ToString().StartsWith(".NET Framework") ?? IsMonoRuntime is false;

		/// <summary>True if the current runtime is .NET Core, false otherwise (Mono or .NET Framework)</summary>
		///
		public static bool IsNetCoreRuntime { get; } =
			Type.GetType("System.Runtime.InteropServices.RuntimeInformation", false)?.GetProperty("FrameworkDescription")
			.GetValue(null, null).ToString().StartsWith(".NET Core") ?? false;

		/// <summary>Throws a missing member runtime exception</summary>
		/// <param name="type">The type that is involved</param>
		/// <param name="names">A list of names</param>
		///
		public static void ThrowMissingMemberException(Type type, params string[] names)
		{
			var fields = string.Join(",", [.. GetFieldNames(type)]);
			var properties = string.Join(",", [.. GetPropertyNames(type)]);
			throw new MissingMemberException($"{string.Join(",", names)}; available fields: {fields}; available properties: {properties}");
		}

		/// <summary>Gets default value for a specific type</summary>
		/// <param name="type">The class/type</param>
		/// <returns>The default value</returns>
		///
		public static object GetDefaultValue(Type type)
		{
			if (type is null)
			{
				Debug.WriteLine("AccessTools.GetDefaultValue: type is null");
				return null;
			}
			if (type == typeof(void))
				return null;
			if (type.IsValueType)
				return Activator.CreateInstance(type);
			return null;
		}

		/// <summary>Creates an (possibly uninitialized) instance of a given type</summary>
		/// <param name="type">The class/type</param>
		/// <returns>The new instance</returns>
		///
		public static object CreateInstance(Type type)
		{
			if (type is null)
				throw new ArgumentNullException(nameof(type));
			var ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, binder: null,
				CallingConventions.Any, [], modifiers: null);
			if (ctor is not null)
				return ctor.Invoke(null);
#if NET5_0_OR_GREATER
			return System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type);
#else
			return System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
#endif
		}

		/// <summary>Creates an (possibly uninitialized) instance of a given type</summary>
		/// <typeparam name="T">The class/type</typeparam>
		/// <returns>The new instance</returns>
		///
		public static T CreateInstance<T>()
		{
			var instance = CreateInstance(typeof(T));
			// Not using `as` operator since it only works with reference types.
			if (instance is T typedInstance)
				return typedInstance;
			return default;
		}




		/// <summary>Tests if a type is a struct</summary>
		/// <param name="type">The type</param>
		/// <returns>True if the type is a struct</returns>
		///
		public static bool IsStruct(Type type)
		{
			if (type == null)
				return false;
			return type.IsValueType && !IsValue(type) && !IsVoid(type);
		}

		/// <summary>Tests if a type is a class</summary>
		/// <param name="type">The type</param>
		/// <returns>True if the type is a class</returns>
		///
		public static bool IsClass(Type type)
		{
			if (type == null)
				return false;
			return !type.IsValueType;
		}

		/// <summary>Tests if a type is a value type</summary>
		/// <param name="type">The type</param>
		/// <returns>True if the type is a value type</returns>
		///
		public static bool IsValue(Type type)
		{
			if (type == null)
				return false;
			return type.IsPrimitive || type.IsEnum;
		}

		/// <summary>Tests if a type is an integer type</summary>
		/// <param name="type">The type</param>
		/// <returns>True if the type represents some integer</returns>
		///
		public static bool IsInteger(Type type)
		{
			if (type == null)
				return false;
			return Type.GetTypeCode(type) switch
			{
				TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 => true,
				_ => false,
			};
		}

		/// <summary>Tests if a type is a floating point type</summary>
		/// <param name="type">The type</param>
		/// <returns>True if the type represents some floating point</returns>
		///
		public static bool IsFloatingPoint(Type type)
		{
			if (type == null)
				return false;
			return Type.GetTypeCode(type) switch
			{
				TypeCode.Decimal or TypeCode.Double or TypeCode.Single => true,
				_ => false,
			};
		}

		/// <summary>Tests if a type is a numerical type</summary>
		/// <param name="type">The type</param>
		/// <returns>True if the type represents some number</returns>
		///
		public static bool IsNumber(Type type) => IsInteger(type) || IsFloatingPoint(type);

		/// <summary>Tests if a type is void</summary>
		/// <param name="type">The type</param>
		/// <returns>True if the type is void</returns>
		///
		public static bool IsVoid(Type type) => type == typeof(void);

		/// <summary>Test whether an instance is of a nullable type</summary>
		/// <typeparam name="T">Type of instance</typeparam>
		/// <param name="instance">An instance to test</param>
		/// <returns>True if instance is of nullable type, false if not</returns>
		///
#pragma warning disable IDE0060
		public static bool IsOfNullableType<T>(T instance) => Nullable.GetUnderlyingType(typeof(T)) is not null;

		/// <summary>Tests whether a type or member is static, as defined in C#</summary>
		/// <param name="member">The type or member</param>
		/// <returns>True if the type or member is static</returns>
		///
		public static bool IsStatic(MemberInfo member)
		{
			if (member is null)
				throw new ArgumentNullException(nameof(member));
			return member.MemberType switch
			{
				MemberTypes.Constructor or MemberTypes.Method => ((MethodBase)member).IsStatic,
				MemberTypes.Event => IsStatic((EventInfo)member),
				MemberTypes.Field => ((FieldInfo)member).IsStatic,
				MemberTypes.Property => IsStatic((PropertyInfo)member),
				MemberTypes.TypeInfo or MemberTypes.NestedType => IsStatic((Type)member),
				_ => throw new ArgumentException($"Unknown member type: {member.MemberType}"),
			};
		}

		/// <summary>Tests whether a type is static, as defined in C#</summary>
		/// <param name="type">The type</param>
		/// <returns>True if the type is static</returns>
		///
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsStatic(Type type)
		{
			if (type is null)
				return false;
			return type.IsAbstract && type.IsSealed;
		}

		/// <summary>Tests whether a property is static, as defined in C#</summary>
		/// <param name="propertyInfo">The property</param>
		/// <returns>True if the property is static</returns>
		///
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsStatic(PropertyInfo propertyInfo)
		{
			if (propertyInfo is null)
				throw new ArgumentNullException(nameof(propertyInfo));
			return propertyInfo.GetAccessors(true)[0].IsStatic;
		}

		/// <summary>Tests whether an event is static, as defined in C#</summary>
		/// <param name="eventInfo">The event</param>
		/// <returns>True if the event is static</returns>
		///
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsStatic(EventInfo eventInfo)
		{
			if (eventInfo is null)
				throw new ArgumentNullException(nameof(eventInfo));
			return eventInfo.GetAddMethod(true).IsStatic;
		}

		/// <summary>Calculates a combined hash code for an enumeration of objects</summary>
		/// <param name="objects">The objects</param>
		/// <returns>The hash code</returns>
		///
		public static int CombinedHashCode(IEnumerable<object> objects)
		{
			var hash1 = (5381 << 16) + 5381;
			var hash2 = hash1;
			var i = 0;
			foreach (var obj in objects)
			{
				if (i % 2 == 0)
					hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ obj.GetHashCode();
				else
					hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ obj.GetHashCode();
				++i;
			}
			return hash1 + (hash2 * 1566083941);
		}
	}
}
