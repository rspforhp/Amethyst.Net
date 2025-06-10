using System;
using System.Reflection;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

namespace HarmonyLib
{
	internal class Tools
	{
		internal static readonly bool isWindows = Environment.OSVersion.Platform.Equals(PlatformID.Win32NT);

		internal struct TypeAndName
		{
			internal Type type;
			internal string name;
		}

		internal static TypeAndName TypColonName(string typeColonName)
		{
			if (typeColonName is null)
				throw new ArgumentNullException(nameof(typeColonName));
			var parts = typeColonName.Split(':');
			if (parts.Length != 2)
				throw new ArgumentException($" must be specified as 'Namespace.Type1.Type2:MemberName", nameof(typeColonName));
			return new TypeAndName() { type = TypeByName(parts[0]), name = parts[1] };
		}

		internal static void ValidateFieldType<F>(FieldInfo fieldInfo)
		{
			var returnType = typeof(F);
			var fieldType = fieldInfo.FieldType;
			if (returnType == fieldType)
				return;
			if (fieldType.IsEnum)
			{
				var underlyingType = Enum.GetUnderlyingType(fieldType);
				if (returnType != underlyingType)
					throw new ArgumentException("FieldRefAccess return type must be the same as FieldType or " +
						$"FieldType's underlying integral type ({underlyingType}) for enum types");
			}
			else if (fieldType.IsValueType)
			{
				// Boxing/unboxing is not allowed for ref values of value types.
				throw new ArgumentException("FieldRefAccess return type must be the same as FieldType for value types");
			}
			else
			{
				if (returnType.IsAssignableFrom(fieldType) is false)
					throw new ArgumentException("FieldRefAccess return type must be assignable from FieldType for reference types");
			}
		}

		
		internal static FieldInfo GetInstanceField(Type type, string fieldName)
		{
			var fieldInfo = Field(type, fieldName);
			if (fieldInfo is null)
				throw new MissingFieldException(type.Name, fieldName);
			if (fieldInfo.IsStatic)
				throw new ArgumentException("Field must not be static");
			return fieldInfo;
		}

		internal static bool FieldRefNeedsClasscast(Type delegateInstanceType, Type declaringType)
		{
			var needCastclass = false;
			if (delegateInstanceType != declaringType)
			{
				needCastclass = delegateInstanceType.IsAssignableFrom(declaringType);
				if (needCastclass is false && declaringType.IsAssignableFrom(delegateInstanceType) is false)
					throw new ArgumentException("FieldDeclaringType must be assignable from or to T (FieldRefAccess instance type) - " +
						"\"instanceOfT is FieldDeclaringType\" must be possible");
			}
			return needCastclass;
		}

		internal static void ValidateStructField<T, F>(FieldInfo fieldInfo) where T : struct
		{
			if (fieldInfo.IsStatic)
				throw new ArgumentException("Field must not be static");
			if (fieldInfo.DeclaringType != typeof(T))
				throw new ArgumentException("FieldDeclaringType must be T (StructFieldRefAccess instance type)");
		}
	}
}
