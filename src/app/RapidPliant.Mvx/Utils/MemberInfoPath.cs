using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Mvx.Utils
{
    public class MemberInfoPath
    {
        private string _fullPathName;
        private string _lastPathName;
        private string _firstPathName;
        private List<MemberInfo> _memberInfos;

        public MemberInfoPath(IEnumerable<MemberInfo> memberInfos)
        {
            _memberInfos = memberInfos.ToList();
        }

        public IReadOnlyList<MemberInfo> MemberInfos { get { return _memberInfos; } }

        public string FullPathName
        {
            get
            {
                if (_fullPathName == null)
                {
                    _fullPathName = string.Join(".", MemberInfos.Select(info=>info.Name));
                }

                return _fullPathName;
            }
        }

        public string LastPathName
        {
            get
            {
                if (_lastPathName == null)
                {
                    _lastPathName = MemberInfos.Last().Name;
                }

                return _lastPathName;
            }
        }

        public string FirstPathName
        {
            get
            {
                if (_firstPathName == null)
                {
                    _firstPathName = MemberInfos.First().Name;
                }

                return _firstPathName;
            }
        }
    }

    public class QualifiedMember
    {        
        public QualifiedMember(MemberInfoPath path)
        {
            Path = path;

            MemberInfo = Path.MemberInfos.LastOrDefault();

            Name = MemberInfo.Name;
        }

        public string Name { get; private set; }

        public MemberInfo MemberInfo { get; private set; }

        public MemberInfoPath Path { get; private set; }

        public object GetValueForRoot(object rootObj)
        {
            var remainingMembers = Path.MemberInfos.ToList();
            var target = rootObj;
            while (target != null && remainingMembers.Count > 0)
            {
                var memberInfo = remainingMembers[0];
                remainingMembers.RemoveAt(0);
                target = GetValue(memberInfo, target);
            }

            if (remainingMembers.Count > 0)
                return null;

            return target;
        }
        
        public object GetValueForTarget(object targetObj)
        {
            return null;
        }

        public void SetValueForRoot(object rootObj, object value)
        {
            var remainingMembers = Path.MemberInfos.ToList();
            var target = rootObj;
            while (target != null && remainingMembers.Count > 1)
            {
                var memberInfo = remainingMembers[0];
                remainingMembers.RemoveAt(0);
                target = GetValue(memberInfo, target);
            }
            
            if (remainingMembers.Count > 1) 
                return;

            SetValue(target, remainingMembers[0], value);
        }
        
        public void SetValueForTarget(object targetObj, object value)
        {
        }

        public static object GetValue(MemberInfo memberInfo, object target)
        {
            if (target == null)
                return null;

            var vm = target as RapidViewModel;
            if (vm != null)
            {
                var value = vm.get(memberInfo.Name);
                return value;
            }

            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(target);
            }

            var propInfo = memberInfo as PropertyInfo;
            if (propInfo != null)
            {
                return propInfo.GetValue(target);
            }

            return null;
        }

        private void SetValue(object target, MemberInfo memberInfo, object value)
        {
            if(target == null)
                return;

            var vm = target as RapidViewModel;
            if (vm != null)
            {
                vm.set(memberInfo.Name, value);
                return;
            }

            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(target, value);
                return;
            }

            var propInfo = memberInfo as PropertyInfo;
            if (propInfo != null)
            {
                propInfo.SetValue(target, value);
                return;
            }
        }
    } 
}
