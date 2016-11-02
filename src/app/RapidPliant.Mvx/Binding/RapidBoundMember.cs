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
            Parts = ParsePathParts(path);
            _index = 0;
        }

        public PathIterator(IEnumerable<PathPart> parts)
        {
            Parts = parts.ToArray();
            _index = 0;
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

    public class PropertyWithPath
    {
        public PropertyWithPath()
        {
        }

        public object GetPropertyValue(object rootDataContext, object thisDataContext, object target, PathIterator pathIter)
        {
            var targetProperty = ResolveTargetProperty(rootDataContext, thisDataContext, ref target, pathIter);
            if (targetProperty == null)
                return null;

            var value = targetProperty.GetValue(target);
            return value;
        }

        public object SetPropertyValue(object rootDataContext, object thisDataContext, object target, PathIterator pathIter, object value)
        {
            var targetProperty = ResolveTargetProperty(rootDataContext, thisDataContext, ref target, pathIter);
            if (targetProperty == null)
                return null;

            targetProperty.SetValue(target, value);
            return value;
        }

        private PropertyInfo ResolveTargetProperty(object rootDataContext, object thisDataContext, ref object target, PathIterator pathIter)
        {
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

            var lastMemberPart = ResolvePath(ref target, pathIter);
            if (lastMemberPart == null)
                return null;

            return target.GetType().GetProperty(lastMemberPart.Name);
        }

        protected PathPart ResolvePath(ref object target, PathIterator pathIter)
        {
            PathPart lastPart = pathIter.Current;
            while (true)
            {
                var part = pathIter.Current;
                lastPart = part;

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
