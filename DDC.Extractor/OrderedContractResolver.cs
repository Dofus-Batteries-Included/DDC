using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NJsonSchema.NewtonsoftJson.Converters;

namespace DDC.Extractor;

public class OrderedContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        string firstProperty = null;
        JsonConverterAttribute jsonInheritanceAttribute = type.GetCustomAttribute<JsonConverterAttribute>();
        if (jsonInheritanceAttribute != null && jsonInheritanceAttribute.ConverterType == typeof(JsonInheritanceConverter))
        {
            firstProperty = jsonInheritanceAttribute.ConverterParameters?[0] as string;
        }

        return base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName == firstProperty ? 0 : 1).ThenBy(p => p.PropertyName).ToList();
    }
}
