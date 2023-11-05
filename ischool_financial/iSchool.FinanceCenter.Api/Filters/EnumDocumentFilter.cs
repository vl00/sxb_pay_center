using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace iSchool.FinanceCenter.Api.Filters
{
    public class EnumDocumentFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                model.Enum.Clear();
                Enum.GetNames(context.Type)
                    .ToList()
                    .ForEach(name => model.Enum.Add(new OpenApiString($"{Convert.ToInt64(Enum.Parse(context.Type, name))} = {name}({GetEnumDes(context.Type.GetField(name))})")));
            }
        }

        /// <summary>
        /// 根据枚举值，获取描述
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string GetEnumDes(FieldInfo field)
        {
            var value = "";
            object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);    //获取描述属性
            if (objs == null || objs.Length == 0)    //当描述属性没有时，直接返回名称
                return value;
            DescriptionAttribute descriptionAttribute = (DescriptionAttribute)objs[0];
            value = descriptionAttribute.Description;
            return value;
        }

        public string GetDescription(Type t, object value)
        {
            foreach (MemberInfo mInfo in t.GetMembers())
            {
                if (mInfo.Name == t.GetEnumName(value))
                {
                    foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                    {
                        if (attr.GetType() == typeof(DescriptionAttribute))
                        {
                            return ((DescriptionAttribute)attr).Description;
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}
