﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using GraphQL.Types;
using GraphQL.Server.Security;

namespace GraphQL.Server
{
    public class FieldMapper
    {
        public static void AddAllFields(IContainer container, ComplexGraphType<Object> obj, Type type, bool declaredPropertiesOnly)
        {
            var filter = BindingFlags.Instance | BindingFlags.Public;
            if (declaredPropertiesOnly) filter = filter | BindingFlags.DeclaredOnly;
            var methods = type.GetMethods(filter);
            foreach (var propertyInfo in type.GetProperties(filter))
            {
                if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
                {
                    AddField(container, obj, type, propertyInfo, methods.FirstOrDefault(m => m.Name == $"Get{propertyInfo.Name}"));
                }
            }
        }

        public static void AddField(IContainer container, ComplexGraphType<Object> obj, Type type, PropertyInfo propertyInfo, MethodInfo methodInfo)
        {
            if (propertyInfo.PropertyType == typeof(IContainer)) return;
            var fieldType = propertyInfo.PropertyType;
            var fieldName = StringExtensions.PascalCase(propertyInfo.Name);
            var fieldDescription = "";
            var authFieldName = $"{type.FullName}.{propertyInfo.Name}";
            var sourceType = type.BaseType?.GenericTypeArguments.FirstOrDefault();
            QueryArguments arguments = null;

            Func<ResolveFieldContext<object>, object> contextResolve;
            if (methodInfo != null)
            {
                arguments = GetPropertyArguments(sourceType, methodInfo);
                // Custom mapping of property
                contextResolve = context =>
                {
                    AuthorizeProperty(container, authFieldName);
                    var output = methodInfo.Invoke(obj, GetArgumentValues(methodInfo, container, context));
                    return container.GetInstance<ApiSchema>().PropertyFilterManager.Filter(context, propertyInfo, authFieldName, output);
                };
            }
            else
            {
                // 1 to 1 mapping of property to source
                contextResolve = context =>
                {
                    AuthorizeProperty(container, authFieldName);
                    var properties = context.Source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    var sourceProp = properties.FirstOrDefault(p => p.Name == propertyInfo.Name);
                    if (sourceProp == null) throw new ArgumentException($"No matching source property found for GraphObject. Type: {type.Name} Property: {propertyInfo.Name}");
                    var output = sourceProp.GetValue(context.Source);
                    return container.GetInstance<ApiSchema>().PropertyFilterManager.Filter(context, propertyInfo, authFieldName, output);
                };
            }
            var graphType = TypeLoader.GetGraphType(fieldType);
            var nonNull = Enumerable.Any(propertyInfo.CustomAttributes, a => a.AttributeType == typeof(RequiredAttribute));
            if (nonNull)
            {
                graphType = typeof(NonNullGraphType<>).MakeGenericType(graphType);
            }
            var field = obj.Field(graphType, fieldName, fieldDescription, arguments, contextResolve);
            //field.ResolvedType = (IGraphType)Activator.CreateInstance(graphType);
            container.GetInstance<AuthorizationMap>().AddAuthorization(type, propertyInfo);
        }
        
        private static QueryArguments GetPropertyArguments(Type sourceType, MethodInfo methodInfo)
        {
            var args = new List<QueryArgument>();
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                if (parameterInfo.ParameterType.IsAssignableFrom(sourceType)
                    || parameterInfo.ParameterType.IsAssignableFrom(typeof(IContainer))) continue;
                var parameterGraphType = TypeLoader.GetGraphType(parameterInfo.ParameterType);
                object defaultValue = null;
                if (parameterInfo.HasDefaultValue)
                {
                    defaultValue = parameterInfo.DefaultValue;
                }
                else
                {
                    parameterGraphType = typeof(NonNullGraphType<>).MakeGenericType(parameterGraphType);
                }
                var argument = new QueryArgument(parameterGraphType)
                {
                    Name = StringExtensions.PascalCase(parameterInfo.Name),
                    DefaultValue = defaultValue
                };
                args.Add(argument);
            }
            return args.Count > 0 ? new QueryArguments(args) : null;
        }

        private static void AuthorizeProperty(IContainer container, string authFieldName)
        {
            var permissions = container.GetInstance<UserPermissions>().Permissions;
            if (!container.GetInstance<AuthorizationMap>().Authorize(authFieldName, permissions))
            {
                throw new AuthorizationException(authFieldName);
            }
        }

        private static object[] GetArgumentValues(MethodInfo methodInfo, IContainer container, ResolveFieldContext<object> context)
        {
            var arguments = new List<object>();
            var sourceType = context.Source.GetType();
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                if (parameterInfo.ParameterType == typeof(IContainer)) arguments.Add(container);
                else if (parameterInfo.ParameterType.IsAssignableFrom(sourceType)) arguments.Add(context.Source);
                else
                {
                    var argName = StringExtensions.PascalCase(parameterInfo.Name);
                    var argValue = context.Arguments.ContainsKey(argName) ? context.Arguments[argName] : null;
                    arguments.Add(argValue);
                }
            }
            return arguments.ToArray();
        }

        public static void AddFields<TOutput>(IContainer container, ComplexGraphType<object> obj) where TOutput : class
        {
            var outputType = typeof(TOutput);
            var filter = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
            var methods = outputType.GetMethods(filter);
            foreach (var propertyInfo in outputType.GetProperties(filter))
            {
                if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
                {
                    AddField(container, obj, outputType, propertyInfo, methods.FirstOrDefault(m => m.Name == $"Get{propertyInfo.Name}"));
                }
            }
        }
    }
}
