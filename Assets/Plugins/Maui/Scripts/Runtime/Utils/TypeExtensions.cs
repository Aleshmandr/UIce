﻿using System;

namespace Maui.Utils
{
	public static class TypeExtensions
	{
		public static bool ImplementsOrDerives(this Type type, Type baseType)
		{
			bool result = false;

			if (type != null)
			{
				if (baseType.IsGenericType)
				{
					if (baseType.IsGenericTypeDefinition)
					{
						if (baseType.IsInterface)
						{
							int i = 0;
							Type[] interfaces = type.GetInterfaces();

							while (result == false && i < interfaces.Length)
							{
								Type @interface = interfaces[i];
								result = @interface.IsGenericType && @interface.GetGenericTypeDefinition() == baseType;
								i++;
							}
						}
							
						result |= type.IsGenericType && type.GetGenericTypeDefinition() == baseType;
					}
					else
					{
						result = baseType.IsAssignableFrom(type);
					}
				}
				else
				{
					result = baseType.IsAssignableFrom(type);
				}
					
				result |= ImplementsOrDerives(type.BaseType, baseType);
			}

			return result;
		}

		public static Type GetGenericTypeTowardsRoot(this Type type)
		{
			Type result = null;

			if (type != null)
			{
				if (type.IsGenericType)
				{
					result = type;
				}
				else
				{
					result = GetGenericTypeTowardsRoot(type.BaseType);
				}
			}

			return result;
		}
		
		public static string GetPrettifiedName(this Type t)
		{
			string result;

			if (t == typeof(bool))
			{
				result = "bool";
			}
			else if (t == typeof(byte))
			{
				result = "byte";
			}
			else if (t == typeof(sbyte))
			{
				result = "sbyte";
			}
			else if (t == typeof(char))
			{
				result = "char";
			}
			else if (t == typeof(decimal))
			{
				result = "decimal";
			}
			else if (t == typeof(float))
			{
				result = "float";
			}
			else if (t == typeof(double))
			{
				result = "double";
			}
			else if (t == typeof(int))
			{
				result = "int";
			}
			else if (t == typeof(uint))
			{
				result = "uint";
			}
			else if (t == typeof(long))
			{
				result = "long";
			}
			else if (t == typeof(ulong))
			{
				result = "ulong";
			}
			else if (t == typeof(short))
			{
				result = "short";
			}
			else if (t == typeof(ushort))
			{
				result = "ushort";
			}
			else if (t == typeof(string))
			{
				result = "string";
			}
			else if (t == typeof(object))
			{
				result = "object";
			}
			else
			{
				result = t.Name;
			}

			return result;
		}
	}
}