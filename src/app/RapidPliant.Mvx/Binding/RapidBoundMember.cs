using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RapidPliant.Mvx.Binding
{
    public class PathIterator
    {
        private int _index;
        private bool _started;

        public PathIterator(string path)
        {
            Parts = ParsePathPartsWithExpressions(path);
            _index = 0;
        }

        public PathIterator(IEnumerable<PathPart> parts)
        {
            Parts = parts.ToArray();
            _index = 0;
        }

        private PathPart[] ParsePathPartsWithExpressions(string path)
        {
            var ifElseParts = path.Split(new[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
            if (ifElseParts.Length == 2)
            {
                //some?x:y
                var ifParts = ParsePathPartsWithExpressions(ifElseParts[0]);
                var evalParts = ifElseParts[1].Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                var thenParts = ParsePathPartsWithExpressions(evalParts[0]);
                var elseParts = ParsePathPartsWithExpressions(evalParts[1]);
                return new [] { new IfThenElsePathPart(ifParts, thenParts, elseParts) };
            }

            var negated = path.StartsWith("!");
            if (negated)
            {
                path = path.Substring(1);
                var parts = ParsePathPartsWithExpressions(path);
                return new [] { new NegatedPathPart(parts) };
            }

            else
            {
                return ParsePathParts(path);
            }
        }

        private PathPart[] ParsePathParts(string path)
        {
            var pathParts = new List<PathPart>();

            var parts = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < parts.Length; ++i)
            {
                var pathPart = new PathPart();
                var part = parts[i].Trim();

                if (!ParseMethodWithArgs(pathPart, part))
                {
                    pathPart.Name = part;
                }

                pathParts.Add(pathPart);
            }

            return pathParts.ToArray();
        }

        private bool ParseMethodWithArgs(PathPart pathPart, string part)
        {
            var i = part.IndexOf("(");
            if (i == -1)
                return false;

            pathPart.Name = part.Substring(0, i);

            var remainder = part.Substring(i).Trim('(', ')').Trim();
            var argParts = remainder.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var argPart in argParts)
            {
                var argPath = new PathPart();
                argPath.Name = argPart.Trim();
                pathPart.AddSubPath(argPath);
            }

            return true;
        }

        public PathPart[] Parts { get; set; }

        public PathPart Current { get; private set; }
        public bool IsLast { get { return _index >= Parts.Length; } }

        public bool IsStarted { get { return _started; } }

        public bool MoveNext()
        {
            _started = true;
            if (_index < Parts.Length)
            {
                Current = Parts[_index++];
                return true;
            }
            return false;
        }
    }
    
    public class PathPart
    {
        public PathPart()
        {
            SubParts = new List<PathPart>();
        }

        public List<PathPart> SubParts { get; set; }

        public string Name { get; set; }

        public void AddSubPath(PathPart pathPart)
        {
            if (pathPart.Name != null)
                SubParts.Add(pathPart);
        }
    }

    public class IfThenElsePathPart : PathPart
    {
        public IfThenElsePathPart(PathPart[] ifParts, PathPart[] thenParts, PathPart[] elseParts)
        {
            IfPathParts = ifParts;
            ThenPathParts = thenParts;
            ElsePathParts = elseParts;
        }

        public PathPart[] IfPathParts { get; set; }
        public PathPart[] ThenPathParts { get; set; }
        public PathPart[] ElsePathParts { get; set; }
    }

    public class NegatedPathPart : PathPart
    {
        public NegatedPathPart(PathPart[] pathParts)
        {
            PathParts = pathParts;
        }

        public PathPart[] PathParts { get; set; }
    }

    public class PropertyWithPath
    {
        public PropertyWithPath()
        {
        }

        public object GetPropertyValue(object rootDataContext, object thisDataContext, object target, PathIterator pathIter)
        {
            PathPart lastPathPart;
            var targetProperty = ResolveTargetProperty(rootDataContext, thisDataContext, ref target, pathIter, out lastPathPart);
            if (targetProperty == null)
            {
                if (lastPathPart != null)
                    return lastPathPart.Name;

                return null;
            }

            var value = targetProperty.GetValue(target);
            return value;
        }

        public object SetPropertyValue(object rootDataContext, object thisDataContext, object target, PathIterator pathIter, object value)
        {
            PathPart lastPathPart;
            var targetProperty = ResolveTargetProperty(rootDataContext, thisDataContext, ref target, pathIter, out lastPathPart);
            if (targetProperty == null)
            {
                if (lastPathPart != null)
                    return lastPathPart.Name;

                return null;
            }

            targetProperty.SetValue(target, value);
            return value;
        }

        private PropertyInfo ResolveTargetProperty(object rootDataContext, object thisDataContext, ref object target, PathIterator pathIter, out PathPart lastPathPart)
        {
            lastPathPart = null;

            if (!pathIter.IsStarted && !pathIter.MoveNext())
            {
                return null;
            }

            if (pathIter.Current.Name == "root")
            {
                pathIter.MoveNext();
                target = rootDataContext;
            }
            else if (pathIter.Current.Name == "this")
            {
                pathIter.MoveNext();
                target = thisDataContext;
            }

            lastPathPart = ResolvePath(ref target, pathIter);
            if (lastPathPart == null)
                return null;
            
            return target.GetType().GetProperty(lastPathPart.Name);
        }

        protected PathPart ResolvePath(ref object target, PathIterator pathIter)
        {
            if (!pathIter.IsStarted && !pathIter.MoveNext())
            {
                return null;
            }

            PathPart lastPart = pathIter.Current;
            while (true)
            {
                var part = pathIter.Current;
                lastPart = part;

                var ifElsePart = part as IfThenElsePathPart;
                if (ifElsePart != null)
                {
                    return ResolveIfThenElsePath(ref target, ifElsePart);
                }

                if (pathIter.IsLast)
                    break;
                
                var memberName = part.Name;
                var prop = target.GetType().GetProperty(memberName);
                if (prop == null)
                    break;

                target = prop.GetValue(target);
                if (target == null)
                    break;

                if(!pathIter.MoveNext())
                    break;
            }

            return lastPart;
        }

        private PathPart ResolveIfThenElsePath(ref object target, IfThenElsePathPart ifElsePart)
        {
            var negate = false;
            var ifParts = ifElsePart.IfPathParts;
            if (ifElsePart.IfPathParts.Length == 1)
            {
                var negatedPart = ifElsePart.IfPathParts[0] as NegatedPathPart;
                if (negatedPart != null)
                {
                    negate = true;
                    ifParts = negatedPart.PathParts;
                }
            }

            var ifElseTarget = target;
            var ifPathIter = new PathIterator(ifParts);
            var path = ResolvePath(ref ifElseTarget, ifPathIter);

            var memberName = path.Name;
            var prop = target.GetType().GetProperty(memberName);
            if (prop == null)
                return null;

            if (prop.PropertyType != typeof(bool))
                return null;

            var val = (bool)prop.GetValue(target);
            if (negate) val = !val;

            if (val)
            {
                return ResolvePath(ref target, new PathIterator(ifElsePart.ThenPathParts));
            }
            else
            {
                return ResolvePath(ref target, new PathIterator(ifElsePart.ElsePathParts));
            }
        }
    }

    public class ActionMethodWithPath
    {
        public ActionMethodWithPath(FrameworkElement frameworkElem, string path)
        {
            var pathIter = new PathIterator(path);
            object rootDataContext;
            object thisDataContext;
            var target = RapidBindingHelpers.FindDataContexts(frameworkElem, pathIter, out rootDataContext, out thisDataContext);

            EvalActionMethod(rootDataContext, thisDataContext, target, pathIter);
        }

        public string Path { get; private set; }

        public object Root { get; private set; }

        public object Target { get; private set; }

        public MethodInfo TargetMethod { get; private set; }

        public object[] TargetMethodArgs { get; private set; }

        protected PathPart ResolvePath(ref object target, PathIterator pathIter)
        {
            PathPart lastPart = pathIter.Current;
            while (true)
            {
                var part = pathIter.Current;
                lastPart = part;

                var memberName = part.Name;
                var prop = target.GetType().GetProperty(memberName);
                if (prop == null)
                    break;

                target = prop.GetValue(target);
                if (target == null)
                    break;

                if(!pathIter.MoveNext())
                    break;
            }

            return lastPart;
        }

        private void EvalActionMethod(object rootDataContext, object thisDataContext, object target, PathIterator pathIter)
        {
            if (!pathIter.IsStarted && !pathIter.MoveNext())
            {
                return;
            }

            Root = rootDataContext;
            if (pathIter.Current.Name == "root")
            {
                pathIter.MoveNext();
                Target = rootDataContext;
            }
            else if (pathIter.Current.Name == "this")
            {
                pathIter.MoveNext();
                Target = thisDataContext;
            }
            else
            {
                Target = thisDataContext;
            }
            
            //Get the last part
            var lastPart = ResolvePath(ref target, pathIter);

            //Update the traget
            Target = target;

            if (lastPart != null)
            {
                TargetMethod = target.GetType().GetMethod(lastPart.Name);
                EvalActionMethodArgs(rootDataContext, thisDataContext, target, lastPart, TargetMethod);
            }
        }

        private void EvalActionMethodArgs(object rootDataContext, object thisDataContext, object lastPartDataContext, PathPart lastPart, MethodInfo methodInfo)
        {
            var args = new List<object>();

            var methodArgs = methodInfo.GetParameters();
            var argParts = lastPart.SubParts;

            var len = Math.Min(methodArgs.Length, argParts.Count);
            var i = 0;
            for (i = 0; i < len; ++i)
            {
                if (i >= methodArgs.Length)
                    break;

                if (i >= argParts.Count)
                    break;

                var methodArg = methodArgs[i];
                var argPart = argParts[i];

                if (argPart.Name == "this")
                {
                    args.Add(thisDataContext);
                }
                else if (argPart.Name == "root")
                {
                    args.Add(rootDataContext);
                }
                else
                {
                    args.Add(GetValue(methodArg.ParameterType, argPart.Name));
                }
            }

            for (int j = i; j < methodArgs.Length; j++)
            {
                var methodArg = methodArgs[i];
                args.Add(GetDefault(methodArg.ParameterType));
            }

            TargetMethodArgs = args.ToArray();
        }

        private object GetValue(Type valueType, string strVal)
        {
            if (valueType == typeof(string))
                return strVal;

            var converter = TypeDescriptor.GetConverter(valueType);
            if (converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFrom(strVal);
            }

            converter = TypeDescriptor.GetConverter(typeof(string));
            if (converter.CanConvertTo(valueType))
            {
                return converter.ConvertTo(strVal, valueType);
            }

            return GetDefault(valueType);
        }

        public object GetDefault(Type t)
        {
            return this.GetType().GetMethod("GetDefaultGeneric").MakeGenericMethod(t).Invoke(this, null);
        }

        public T GetDefaultGeneric<T>()
        {
            return default(T);
        }

        public void Invoke()
        {
            if (TargetMethod == null)
                return;

            TargetMethod.Invoke(Target, TargetMethodArgs);
        }
    }
}
