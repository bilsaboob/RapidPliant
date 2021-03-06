﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Mvx.Utils
{
    public static class ReflectionExtensions
    {
        public static PropertyInfo GetPropertyInfo<TProperty>(this object source, Expression<Func<TProperty>> propertyLambda)
        {
            var type = source.GetType();

            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a method, not a property.", propertyLambda.ToString()));

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.", propertyLambda.ToString()));

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format("Expresion '{0}' refers to a property that is not from type {1}.", propertyLambda.ToString(), type));

            return propInfo;
        }

        public static MemberInfoPath GetMemberInfoPath<TProperty>(this object source, Expression<Func<TProperty>> propertyExpression)
        {
            MemberExpression expr;
            switch (propertyExpression.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var ue = propertyExpression.Body as UnaryExpression;
                    expr = ((ue != null) ? ue.Operand : null) as MemberExpression;
                    break;
                default:
                    expr = propertyExpression.Body as MemberExpression;
                break;
            }

            var memberInfos = new List<MemberInfo>();
            while (expr != null)
            {
                memberInfos.Add(expr.Member);
                expr = expr.Expression as MemberExpression;
            }
            memberInfos.Reverse();

            return new MemberInfoPath(memberInfos);
        }

        public static int GetTypeDistanceTo(this Type type, Type toType)
        {
            var minDistance = GetTypeDistanceToInternal(type, toType, 0);
            return minDistance;
        }

        private static int GetTypeDistanceToInternal(Type type, Type toType, int distance)
        {
            if (type == toType)
                return distance;

            var hasNewMinistance = false;
            var minSubDistance = int.MaxValue;

            //Check the base type!
            if (type.BaseType != null && !type.BaseType.IsPrimitive && type.BaseType != typeof(object))
            {
                var baseTypeDistance = GetTypeDistanceToInternal(type.BaseType, toType, distance + 1);
                if (minSubDistance > baseTypeDistance)
                {
                    hasNewMinistance = true;
                    minSubDistance = baseTypeDistance;
                }
            }

            if (hasNewMinistance)
                return minSubDistance;

            //If it's an interface type we are looking for, we can also check the interfaces!
            if (toType.IsInterface)
            {
                var interfaces = type.GetInterfaces();
                if (interfaces != null && interfaces.Length > 0)
                {
                    foreach (var interfaceType in interfaces)
                    {
                        var interfaceTypeDistance = GetTypeDistanceToInternal(interfaceType, toType, distance + 1);
                        if (minSubDistance > interfaceTypeDistance)
                        {
                            hasNewMinistance = true;
                            minSubDistance = interfaceTypeDistance;
                        }
                    }
                }
            }

            return minSubDistance;
        }
    }
}
