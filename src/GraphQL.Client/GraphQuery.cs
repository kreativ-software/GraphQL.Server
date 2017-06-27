﻿using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphQL.Client
{
    public class GraphQuery<T> : IGraphQuery
    {
        public string Operation { get; private set; }
        public IEnumerable<Field> Selections { get; private set; }
        public T Data { get; set; }

        public GraphQuery(string operation)
        {
            Operation = operation;
        }

        public GraphQuery(string operation, IEnumerable<Field> selections)
        {
            Operation = operation;
            Selections = selections;
        }

        public string GetSelectFields()
        {
            var output = Selections != null && Selections.Any() ? GetFieldsForSelections(Selections) : GetFieldsForType(typeof(T));
            return output;
        }

        public void SetOutput(JToken obj)
        {
            if (obj != null)
            {
                Data = obj.ToObject<T>();
            }
        }

        private string GetFieldsForSelections(IEnumerable<Field> selections)
        {
            var fields = new List<string>();
            foreach (var selection in selections)
            {
                var name = $"{PascalCase(selection.Name)}";
                if (selection.Arguments.Any())
                {
                    var arguments = selection.Arguments.Select(a => $"{PascalCase(a.Name)}:{JsonConvert.SerializeObject(a.Value)}");
                    name = $"{name}({string.Join(",", arguments)})";
                }
                if (selection.SelectionSet.Children.Any())
                {
                    fields.Add($"{name}{GetFieldsForSelections(selection.SelectionSet.Selections.OfType<Field>())}");
                    continue;
                }
                fields.Add(name);
            }
            return $"{{{string.Join(" ", fields)}}}";
        }

        private string GetFieldsForType(Type type)
        {
            if (type.IsArray) return GetFieldsForType(type.GetElementType());
            if (type != typeof(string) &&
                type.GetInterfaces().Any(t => t.Name.Contains("IEnumerable")) &&
                type.GenericTypeArguments.Length > 0)
            {
                return GetFieldsForType(type.GenericTypeArguments[0]);
            }
            var fields = new List<string>();
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyType = propertyInfo.PropertyType;
                if (propertyType.IsArray)
                {
                    fields.Add($"{PascalCase(propertyInfo.Name)}{GetFieldsForType(propertyType.GetElementType())}");
                    continue;
                }
                if (propertyType != typeof(string))
                {
                    if (propertyType.GetInterfaces().Any(t => t.Name.Contains("IEnumerable")))
                    {
                        fields.Add($"{PascalCase(propertyInfo.Name)}{GetFieldsForType(propertyType.GenericTypeArguments[0])}");
                        continue;
                    }
                    if (propertyType.IsClass || propertyType.IsInterface)
                    {
                        fields.Add($"{PascalCase(propertyInfo.Name)}{GetFieldsForType(propertyType)}");
                        continue;
                    }
                }
                fields.Add(PascalCase(propertyInfo.Name));
            }
            return $"{{{string.Join(" ", fields)}}}";
        }

        private static string PascalCase(string text)
        {
            return char.ToLower(text[0]) + text.Substring(1);
        }
    }

    public interface IGraphQuery
    {
        string Operation { get; }
        string GetSelectFields();
        void SetOutput(JToken value);
    }
}