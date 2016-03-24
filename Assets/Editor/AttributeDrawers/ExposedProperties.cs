using System;
using System.Collections.Generic;
using System.Reflection;
using Forgelight.Attributes;
using UnityEditor;
using UnityEngine;

namespace Forgelight.Editor.AttributeDrawers
{
    public static class ExposeProperties
    {
        public static void Expose(PropertyField[] properties)
        {
            GUILayoutOption[] emptyOptions = new GUILayoutOption[0];

            EditorGUILayout.BeginVertical(emptyOptions);

            foreach (PropertyField field in properties)
            {

                EditorGUILayout.BeginHorizontal(emptyOptions);

                switch (field.Type)
                {
                    case SerializedPropertyType.Integer:
                        field.SetValue(EditorGUILayout.IntField(field.Name, (int)field.GetValue(), emptyOptions));
                        break;

                    case SerializedPropertyType.Float:
                        field.SetValue(EditorGUILayout.FloatField(field.Name, (float)field.GetValue(), emptyOptions));
                        break;

                    case SerializedPropertyType.Boolean:
                        field.SetValue(EditorGUILayout.Toggle(field.Name, (bool)field.GetValue(), emptyOptions));
                        break;

                    case SerializedPropertyType.String:
                        field.SetValue(EditorGUILayout.TextField(field.Name, (string)field.GetValue(), emptyOptions));
                        break;

                    case SerializedPropertyType.Vector2:
                        field.SetValue(EditorGUILayout.Vector2Field(field.Name, (Vector2)field.GetValue(), emptyOptions));
                        break;

                    case SerializedPropertyType.Vector3:
                        field.SetValue(EditorGUILayout.Vector3Field(field.Name, (Vector3)field.GetValue(), emptyOptions));
                        break;

                    case SerializedPropertyType.Enum:
                        field.SetValue(EditorGUILayout.EnumPopup(field.Name, (Enum)field.GetValue(), emptyOptions));
                        break;

                    case SerializedPropertyType.ObjectReference:
                        field.SetValue(EditorGUILayout.ObjectField(field.Name, (UnityEngine.Object)field.GetValue(), field.GetPropertyType(), true, emptyOptions));
                        break;

                }

                EditorGUILayout.EndHorizontal();

            }

            EditorGUILayout.EndVertical();

        }

        public static PropertyField[] GetProperties(object obj)
        {
            List<PropertyField> fields = new List<PropertyField>();

            PropertyInfo[] infos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo info in infos)
            {

                if (!(info.CanRead && info.CanWrite))
                    continue;

                object[] attributes = info.GetCustomAttributes(true);

                bool isExposed = false;

                foreach (object o in attributes)
                {
                    if (o.GetType() == typeof(ExposePropertyAttribute))
                    {
                        isExposed = true;
                        break;
                    }
                }

                if (!isExposed)
                    continue;

                SerializedPropertyType type = PropertyField.GetPropertyType(info);
                PropertyField field = new PropertyField(obj, info, type);
                fields.Add(field);
            }

            return fields.ToArray();

        }

    }

    public class PropertyField
    {
        object instance;
        PropertyInfo info;
        SerializedPropertyType type;

        MethodInfo getter;
        MethodInfo setter;

        public SerializedPropertyType Type
        {
            get
            {
                return type;
            }
        }

        public string Name
        {
            get
            {
                return ObjectNames.NicifyVariableName(info.Name);
            }
        }

        public PropertyField(object instance, PropertyInfo info, SerializedPropertyType type)
        {
            this.instance = instance;
            this.info = info;
            this.type = type;

            getter = this.info.GetGetMethod();
            setter = this.info.GetSetMethod();
        }

        public object GetValue()
        {
            return getter.Invoke(instance, null);
        }

        public void SetValue(object value)
        {
            setter.Invoke(instance, new[] { value });
        }

        public Type GetPropertyType()
        {
            return info.PropertyType;
        }

        public static SerializedPropertyType GetPropertyType(PropertyInfo info)
        {
            Type type = info.PropertyType;

            if (type == typeof(int))
            {
                return SerializedPropertyType.Integer;
            }

            if (type == typeof(float))
            {
                return SerializedPropertyType.Float;
            }

            if (type == typeof(bool))
            {
                return SerializedPropertyType.Boolean;
            }

            if (type == typeof(string))
            {
                return SerializedPropertyType.String;
            }

            if (type == typeof(Vector2))
            {
                return SerializedPropertyType.Vector2;
            }

            if (type == typeof(Vector3))
            {
                return SerializedPropertyType.Vector3;
            }

            if (type.IsEnum)
            {
                return SerializedPropertyType.Enum;
            }

            return SerializedPropertyType.ObjectReference;
        }

    }
}
