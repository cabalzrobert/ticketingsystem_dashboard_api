using System.Dynamic;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace Comm.DTO
{
    public class DynamicItem: DynamicObject
    {
        readonly Dictionary<string, object> _properties = new Dictionary<string, object>();
        public int Count => _properties.Keys.Count;
            
        public DynamicItem() { }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string lowername = binder.Name.ToLower();
            if (_properties.ContainsKey(lowername))
            {
                result = _properties[lowername];
                return true;
            }
            result = null;
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _properties[binder.Name.ToLower()] = value;
            return true;
        }
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _properties.Keys;
        }
    }

}

/*
https://docs.microsoft.com/en-us/dotnet/api/system.dynamic.dynamicobject?view=netframework-4.8
*/