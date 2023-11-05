using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace iSchool.Infrastructure.Enums
{
    /// <summary>
    /// 枚举工具
    /// </summary>
    public static class EnumUtil
    {
        public static IEnumerable<(TEnum Value, string Desc)> GetDescs<TEnum>()
        {
            foreach (var em in Enum.GetNames(typeof(TEnum)))
            {
                var v = Enum.Parse(typeof(TEnum), em);
                var attr = typeof(TEnum).GetField(em).GetCustomAttributes<DescriptionAttribute>().FirstOrDefault();
                yield return ((TEnum)v, attr?.Description);
            }
        }

        /// <summary>
        /// 获取枚举描述性数据
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string GetDescription(this System.Enum @enum)
        {
            Type type = @enum.GetType();
            FieldInfo field = type.GetField(@enum.ToString());
            if (field == null)
                return default;

            var attr = field.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
            return attr?.Description;
        }

        /// <summary>
        /// get an enum [Description] desc
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetDesc(Enum enumValue)
        {
            var ty = enumValue.GetType();
            var f = Enum.GetName(ty, enumValue);
            var attr = f != null ? ty.GetField(f).GetCustomAttributes<DescriptionAttribute>().FirstOrDefault() : null;
            return attr?.Description;
        }


        public static string GetName(this Enum eEnum)
        {
            return Enum.GetName(eEnum.GetType(), eEnum);
        }

        public static TEnum ToEnum<TEnum>(this string str, bool ignoreCase = true) where TEnum : struct
        {
            return (TEnum)Enum.Parse(typeof(TEnum), str, ignoreCase);
        }

        public static int ToInt(this Enum eEnum)
        {
            return Convert.ToInt32(eEnum);
        }

        /// <summary>
        /// 生成select的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<SelectListItem> GetSelectItems<T>() where T : Enum
        {
            var list = GetDescs<T>();
            return list.Select(p =>
            new SelectListItem { Text = p.Desc, Value = (Convert.ToInt32(p.Value)).ToString() })
                .OrderBy(p => p.Value).ToList();
        }

        /// <summary>
        /// 生成select的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<SelectItemsKeyValues> GetSelectItems2<T>() where T : Enum
        {
            var list = GetDescs<T>();
            return list.Select(p =>
            new SelectItemsKeyValues { Value = p.Desc,  Key = (Convert.ToInt32(p.Value)),Sort= (Convert.ToInt32(p.Value)) })
                .OrderBy(p => p.Value).ToList();
        }

        #region 文件图标
        public static string GetAttachIcon(string filetype)
        {
            switch (filetype)
            {
                case "ai":
                    return "fileicon_ai";
                case "doc":
                    return "fileicon_doc";
                case "docx":
                    return "fileicon_docx";
                case "jpg":
                    return "fileicon_jpg";
                case "jpeg":
                    return "fileicon_jpeg";
                case "pdf":
                    return "fileicon_pdf";
                case "png":
                    return "fileicon_png";
                case "ppt":
                    return "fileicon_ppt";
                case "pptx":
                    return "fileicon_pptx";
                case "psd":
                    return "fileicon_psd";
                case "xls":
                    return "fileicon_xls";
                case "txt":
                    return "fileicon_txt";
                default:
                    return "fileicon_qitageshi";
            }
        }
        #endregion
    }


    /// <summary>
    /// 下拉框数据源实体
    /// </summary>
    public class SelectItemsKeyValues
    {
        /// <summary>
        /// 键
        /// </summary>
        public int Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Sort { get; set; }
    }

    /// <summary>
    /// 下拉框选项
    /// </summary>
    public class SelectItem
    {
        /// <summary>
        /// 值（用于展示）
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 键（用于传递参数）
        /// </summary>
        public int Value { get; set; }
    }
}
