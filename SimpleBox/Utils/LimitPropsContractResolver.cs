using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SimpleBox.Utils
{
    public sealed class LimitPropsContractResolver : DefaultContractResolver
    {
        private readonly string[] _props;

        public LimitPropsContractResolver(string[] props) => _props = props;

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) =>
            base.CreateProperties(type, memberSerialization).Where(p => _props.Contains(p.PropertyName))
                .ToList();
    }

    [AttributeUsage(AttributeTargets.Class)]
    sealed class LimitPropsAttribute : Attribute
    {
        public LimitPropsAttribute(string[] props) => Props = props;

        public string[] Props;
    }
}
