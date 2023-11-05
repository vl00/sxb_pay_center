using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
    /// <summary>
    /// 录入算法-教师荣誉Lv
    /// </summary>
    public enum SchAlgTeacherHonorLvs
    {
        /// <summary>
        /// 国家级
        /// </summary>
        [Description("国家级")]
        LvCountry = 1,
        /// <summary>
        /// 省级
        /// </summary>
        [Description("省级")]
        LvProvince = 2,
        /// <summary>
        /// 市级
        /// </summary>
        [Description("市级")]
        LvCity = 3,
    }

    /// <summary>
    /// 面积单位
    /// </summary>
    public enum AcreageUnit
    {
        /// <summary>
        /// 亩
        /// </summary>
        [Description("亩")]
        mu = 1, //666,
        /// <summary>
        /// 平方米
        /// </summary>
        [Description("平方米")]
        m2 = 2, //1,
        ///// <summary>
        ///// 平方千米
        ///// </summary>
        //[Description("平方千米")]
        //km2 = 3, //1000000,
    }

    /// <summary>
    /// 泳池位置
    /// </summary>
    public enum SwimpoolWhere
    {
        /// <summary>
        /// 室内
        /// </summary>
        [Description("室内")]
        InRoom = 1,
        /// <summary>
        /// 室外
        /// </summary>
        [Description("室外")]
        OutRoom = 2,
    }

    /// <summary>
    /// 泳池温度
    /// </summary>
    public enum SwimpoolTemperature
    {
        /// <summary>
        /// 恒温
        /// </summary>
        [Description("恒温")]
        Constant = 1,
        /// <summary>
        /// 非恒温
        /// </summary>
        [Description("非恒温")]
        NotConstant = 2,
    }

    /// <summary>
    /// 住宿设施
    /// </summary>
    public enum LodgingFacilities
    {
        /// <summary>
        /// 空调
        /// </summary>
        [Description("空调")]
        AirCdt = 1,
        /// <summary>
        /// 阳台
        /// </summary>
        [Description("阳台")]
        Balcony = 2,
        /// <summary>
        /// 独立卫生间
        /// </summary>
        [Description("独立卫生间")]
        PriBathroom = 3,
    }

    /// <summary>
    /// 操场跑道长度
    /// </summary>
    public enum PgdLength
    {
        /// <summary>
        /// 100米
        /// </summary>
        [Description("100米")]
        Cri100m = 100,
        /// <summary>
        /// 200米
        /// </summary>
        [Description("200米")]
        Cri200m = 200,
        /// <summary>
        /// 400米
        /// </summary>
        [Description("400米")]
        Cri400m = 400,
    }
}
