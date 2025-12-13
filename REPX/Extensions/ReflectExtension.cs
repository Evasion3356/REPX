using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace REPX.Extensions
{
	// Token: 0x0200001C RID: 28
	internal static class ReflectExtension
	{
		// Token: 0x06000099 RID: 153 RVA: 0x000069E8 File Offset: 0x00004BE8
		internal static T GetField<T>(this object obj, string name)
		{
			string text = obj.GetType().FullName + ":" + name;
			FieldInfo fieldInfo;
			bool flag = !ReflectExtension._fieldCache.TryGetValue(text, out fieldInfo);
			if (flag)
			{
				FieldInfo field = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
				if (field == null)
				{
					throw new MissingFieldException("Field '" + name + "' not found");
				}
				fieldInfo = field;
				ReflectExtension._fieldCache[text] = fieldInfo;
			}
			return (T)((object)fieldInfo.GetValue(obj));
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00006A6C File Offset: 0x00004C6C
		internal static void SetField<T>(this object obj, string name, T value)
		{
			string text = obj.GetType().FullName + ":" + name;
			FieldInfo fieldInfo;
			bool flag = !ReflectExtension._fieldCache.TryGetValue(text, out fieldInfo);
			if (flag)
			{
				FieldInfo field = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
				if (field == null)
				{
					throw new MissingFieldException("Field '" + name + "' not found");
				}
				fieldInfo = field;
				ReflectExtension._fieldCache[text] = fieldInfo;
			}
			fieldInfo.SetValue(obj, value);
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00006AEC File Offset: 0x00004CEC
		internal static T GetStaticField<T>(this Type type, string name)
		{
			string text = type.FullName + ":static:" + name;
			FieldInfo fieldInfo;
			bool flag = !ReflectExtension._fieldCache.TryGetValue(text, out fieldInfo);
			if (flag)
			{
				FieldInfo field = type.GetField(name, BindingFlags.Static | BindingFlags.NonPublic);
				if (field == null)
				{
					throw new MissingFieldException("Static field '" + name + "' not found in " + type.Name);
				}
				fieldInfo = field;
				ReflectExtension._fieldCache[text] = fieldInfo;
			}
			return (T)((object)fieldInfo.GetValue(null));
		}

		// Token: 0x0600009C RID: 156 RVA: 0x00006B6C File Offset: 0x00004D6C
		internal static void SetStaticField<T>(this Type type, string name, T value)
		{
			string text = type.FullName + ":static:" + name;
			FieldInfo fieldInfo;
			bool flag = !ReflectExtension._fieldCache.TryGetValue(text, out fieldInfo);
			if (flag)
			{
				FieldInfo field = type.GetField(name, BindingFlags.Static | BindingFlags.NonPublic);
				if (field == null)
				{
					throw new MissingFieldException("Static field '" + name + "' not found in " + type.Name);
				}
				fieldInfo = field;
				ReflectExtension._fieldCache[text] = fieldInfo;
			}
			fieldInfo.SetValue(null, value);
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00006BE8 File Offset: 0x00004DE8
		internal static T GetProperty<T>(this object obj, string name)
		{
			string text = obj.GetType().FullName + ":" + name;
			PropertyInfo propertyInfo;
			bool flag = !ReflectExtension._propertyCache.TryGetValue(text, out propertyInfo);
			if (flag)
			{
				PropertyInfo property = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
				if (property == null)
				{
					throw new MissingMemberException("Property '" + name + "' not found");
				}
				propertyInfo = property;
				ReflectExtension._propertyCache[text] = propertyInfo;
			}
			return (T)((object)propertyInfo.GetValue(obj));
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00006C6C File Offset: 0x00004E6C
		internal static void SetProperty<T>(this object obj, string name, T value)
		{
			string text = obj.GetType().FullName + ":" + name;
			PropertyInfo propertyInfo;
			bool flag = !ReflectExtension._propertyCache.TryGetValue(text, out propertyInfo);
			if (flag)
			{
				PropertyInfo property = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
				if (property == null)
				{
					throw new MissingMemberException("Property '" + name + "' not found");
				}
				propertyInfo = property;
				ReflectExtension._propertyCache[text] = propertyInfo;
			}
			propertyInfo.SetValue(obj, value);
		}

		// Token: 0x0600009F RID: 159 RVA: 0x00006CEC File Offset: 0x00004EEC
		internal static IEnumerator GetCoroutine(this object obj, string name, params object[] parameters)
		{
			object obj2 = obj.InvokeMethod(name, parameters);
			return (IEnumerator)obj2;
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00006D10 File Offset: 0x00004F10
		internal static IEnumerator GetStaticCoroutine(this Type type, string name, params object[] parameters)
		{
			object obj = type.InvokeStaticMethod(name, parameters);
			return (IEnumerator)obj;
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00006D34 File Offset: 0x00004F34
		internal static object InvokeMethod(this object obj, string name, params object[] parameters)
		{
			string text = string.Join(",", parameters.Select((object p) => p.GetType().Name));
			string text2 = string.Concat(new string[]
			{
				obj.GetType().FullName,
				":",
				name,
				"(",
				text,
				")"
			});
			MethodInfo methodInfo;
			bool flag = !ReflectExtension._methodCache.TryGetValue(text2, out methodInfo);
			if (flag)
			{
				MethodInfo method = obj.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic, null, parameters.Select((object p) => p.GetType()).ToArray<Type>(), null);
				if (method == null)
				{
					throw new MissingMethodException("Method '" + name + "' not found");
				}
				methodInfo = method;
				ReflectExtension._methodCache[text2] = methodInfo;
			}
			return methodInfo.Invoke(obj, parameters);
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00006E34 File Offset: 0x00005034
		internal static object InvokeStaticMethod(this Type type, string name, params object[] parameters)
		{
			string text = string.Join(",", parameters.Select((object p) => p.GetType().Name));
			string text2 = string.Concat(new string[] { type.FullName, ":static:", name, "(", text, ")" });
			MethodInfo methodInfo;
			bool flag = !ReflectExtension._methodCache.TryGetValue(text2, out methodInfo);
			if (flag)
			{
				MethodInfo method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic, null, parameters.Select((object p) => p.GetType()).ToArray<Type>(), null);
				if (method == null)
				{
					throw new MissingMethodException("Static method '" + name + "' not found in " + type.Name);
				}
				methodInfo = method;
				ReflectExtension._methodCache[text2] = methodInfo;
			}
			return methodInfo.Invoke(null, parameters);
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x00006F30 File Offset: 0x00005130
		internal static Type GetNestedType(this Type type, string name)
		{
			string text = type.FullName + ":" + name;
			Type type2;
			bool flag = !ReflectExtension._nestedTypeCache.TryGetValue(text, out type2);
			if (flag)
			{
				Type nestedType = type.GetNestedType(name, BindingFlags.NonPublic);
				if (nestedType == null)
				{
					throw new TypeLoadException("Nested type '" + name + "' not found in " + type.Name);
				}
				type2 = nestedType;
				ReflectExtension._nestedTypeCache[text] = type2;
			}
			return type2;
		}

		// Token: 0x04000031 RID: 49
		private const BindingFlags NonPublicFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

		// Token: 0x04000032 RID: 50
		private static readonly ConcurrentDictionary<string, FieldInfo> _fieldCache = new ConcurrentDictionary<string, FieldInfo>();

		// Token: 0x04000033 RID: 51
		private static readonly ConcurrentDictionary<string, PropertyInfo> _propertyCache = new ConcurrentDictionary<string, PropertyInfo>();

		// Token: 0x04000034 RID: 52
		private static readonly ConcurrentDictionary<string, MethodInfo> _methodCache = new ConcurrentDictionary<string, MethodInfo>();

		// Token: 0x04000035 RID: 53
		private static readonly ConcurrentDictionary<string, Type> _nestedTypeCache = new ConcurrentDictionary<string, Type>();
	}
}
