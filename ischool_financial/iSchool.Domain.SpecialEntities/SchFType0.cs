using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace iSchool
{
    public readonly partial struct SchFType0 : IEquatable<SchFType0>
    {
        /// <summary>
        /// 总类型code
        /// </summary>
        public readonly string Code;
        /// <summary>
        /// 是否有效总类型
        /// </summary>
        public readonly bool IsValid;

        public SchFType0(string code) : this(code, SchFTypeUtil.GetItem(code))
        { }

        public SchFType0(byte grade, byte type, bool? discount, bool? diglossia, bool? chinese)
        {
            this.Grade = grade;
            this.Type = type;
            this.Discount = discount ?? false;
            this.Diglossia = diglossia ?? false;
            this.Chinese = chinese ?? false;

            var attrs = SchFTypeUtil.CodeAttrs(grade, type, discount, diglossia, chinese);
            var x = SchFTypeUtil.Dict.SingleOrDefault(_ => _.attrs == attrs);
            this.Code = x.code;
            this.IsValid = x.code == null;

            if (this.Code == null)
            {
                this.Code = $"lx{this.Grade % 10}{(this.Type > 10 ? this.Type / 10 % 10 : this.Type)}";
            }
        }

        private SchFType0(string code, (string code, string desc, long attrs)? x) : this(x != null, x ?? (code, null, 0)) { }

        private SchFType0(bool isvalid, (string code, string desc, long attrs)? x)
        {
            this.IsValid = isvalid;
            this.Code = x?.code;
            SchFTypeUtil.ReslAttrs(x?.attrs ?? 0L, out this.Grade, out this.Type, out this.Discount, out this.Diglossia, out this.Chinese);

            if (this.Code == null)
            {
                this.Code = $"lx{this.Grade % 10}{(this.Type > 10 ? this.Type / 10 % 10 : this.Type)}";
            }
        }

        public readonly byte Grade;
        public readonly byte Type;
        /// <summary>
        /// 是否普惠
        /// </summary>
        public readonly bool Discount;
        /// <summary>
        /// 是否双语
        /// </summary>
        public readonly bool Diglossia;
        /// <summary>
        /// 是否中国人学校
        /// </summary>
        public readonly bool Chinese;

        /// <summary>
        /// 是否归到国际化
        /// </summary>
        public bool HasInternational => Type == 3 || Type == 4 || Diglossia;

        public override bool Equals(object obj)
        {
            if (obj is SchFType0 sch) return Equals(sch);
            return false;
        }

        public bool Equals(SchFType0 sch) => this.Code == sch.Code;
        public override int GetHashCode() => Code.GetHashCode();
        public override string ToString() => Code;

        #region
        public SchFType0 SetGrade(byte grade) => new SchFType0(grade, Type, Discount, Diglossia, Chinese);
        public SchFType0 SetType(byte type) => new SchFType0(Grade, type, Discount, Diglossia, Chinese);
        public SchFType0 SetDiscount(bool? discount) => new SchFType0(Grade, Type, discount ?? false, Diglossia, Chinese);
        public SchFType0 SetDiglossia(bool? diglossia) => new SchFType0(Grade, Type, Discount, diglossia ?? false, Chinese);
        public SchFType0 SetChinese(bool? chinese) => new SchFType0(Grade, Type, Discount, Diglossia, chinese ?? false);
        #endregion

        /// <summary>
        /// can parse by : <br/>
        /// $"{code}" <br/>
        /// $"{desc}" <br/>
        /// $"{attrs}"
        /// </summary>
        /// <returns></returns>
        public static SchFType0 Parse(string str)
        {
            if (string.IsNullOrEmpty(str)) return new SchFType0();

            if (str.StartsWith("lx"))
            {
                return new SchFType0(str);
            }

            if (long.TryParse(str, out var attrs))
            {
                var x = SchFTypeUtil.Dict.SingleOrDefault(_ => _.attrs == attrs);
                if (x.code == null && x.desc == null) return new SchFType0(false, (null, null, attrs));
                return new SchFType0(true, x);
            }

            {
                var x = SchFTypeUtil.Dict.SingleOrDefault(_ => _.desc == str);
                if (x.code == null && x.desc == null) return new SchFType0(false, ("", str, 0));
                return new SchFType0(true, x);
            }
        }

        #region
        //static string[] yel(string str, int parts)
        //{
        //    var _ss = new string[parts];
        //    var _str = str;
        //    for (var c = 0; c < parts; c++)
        //    {
        //        var i = _str.IndexOf('.');
        //        if (i < 0 && c != parts - 1) throw new ArgumentException();
        //        _ss[c] = i < 0 ? _str : _str.Substring(0, i);
        //        _str = _str.Substring(i + 1);
        //    }
        //    return _ss;
        //}
        #endregion

        /// <summary>
        /// 总类型中文名
        /// </summary>
        /// <returns></returns>
        public string GetDesc() => SchFTypeUtil.GetItem(Code)?.desc;
    }

    public static class SchFTypeUtil
    {
        /// <summary>
        /// [总类型code(数据保存), 总类型中文名, 特征码]
        /// <br/>
        /// 特征码=attrs(2进制转10进制)+grade(2位)+type(2位)
        /// </summary>
        public static readonly IReadOnlyList<(string code, string desc, long attrs)> Dict = new List<(string, string, long)>
        {
            ("lx110", "公办幼儿园",     00101L),
            ("lx120", "普通民办幼儿园", 00102L),
            ("lx121", "民办普惠幼儿园", 10102L),
            ("lx130", "国际幼儿园",     00103L),
            ("lx199", "幼托机构",       00199L),
            ("lx210", "公办小学",       00201L),
            ("lx220", "普通民办小学",   00202L),
            ("lx231", "双语小学",       20202L),
            ("lx230", "外国人小学",     00204L),
            ("lx310", "公办初中",       00301L),
            ("lx320", "普通民办初中",   00302L),
            ("lx331", "双语初中",       20302L),
            ("lx330", "外国人初中",     00304L),
            ("lx410", "公办高中",       00401L),
            ("lx420", "普通民办高中",   00402L),
            ("lx431", "双语高中",       20402L),
            ("lx432", "国际高中",       40403L),
            ("lx430", "外国人高中",     00404L),
            ("lx180", "港澳台幼儿园",   00180L),
            ("lx280", "港澳台小学",     00280L),
            ("lx380", "港澳台初中",     00380L),
            ("lx480", "港澳台高中",     00480L),
        };

        public static (string code, string desc, long attrs)? GetItem(string code)
        {
            var x = Dict.SingleOrDefault(_ => _.code == code);
            if (x.code == null && x.desc == null) return null;
            return x;
        }

        public static string GetCode(string desc)
        {
            foreach (var x in Dict)
            {
                if (x.desc == desc)
                    return x.code;
            }
            return null;
        }

        public static long CodeAttrs(byte grade, byte type, bool? discount, bool? diglossia, bool? chinese)
        {
            return type + ((long)grade * 100) + 10000L * ((discount == true ? 1 : 0) | (diglossia == true ? 2 : 0) | (chinese == true ? 4 : 0));
        }

        public static void ReslAttrs(long attrs, out byte grade, out byte type, out bool discount, out bool diglossia, out bool chinese)
        {
            type = (byte)(attrs % 100);
            attrs /= 100;
            grade = (byte)(attrs % 100);
            attrs /= 100;
            discount = (attrs & 1L) == 1L;
            diglossia = (attrs & 2L) == 2L;
            chinese = (attrs & 4L) == 4L;
        }
    }
}
