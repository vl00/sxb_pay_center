using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
    public enum TagType
    {
        /// <summary>
        /// 办学类型
        /// </summary>
        [Description("办学类型")]
        RunSchool = 1,
        /// <summary>
        /// 学校认证
        /// </summary>
        [Description("学校认证")]
        SchoolAccre = 2,
        /// <summary>
        /// 出国方向
        /// </summary>
        [Description("出国方向")]
        Abroad = 3,
        /// <summary>
        /// 招生对象
        /// </summary>
        [Description("招生对象")]
        Recruit = 4,
        /// <summary>
        /// 考试科目
        /// </summary>
        [Description("考试科目")]
        Subject = 5,
        /// <summary>
        /// 课程设置
        /// </summary>
        [Description("课程设置")]
        CourseSet = 6,
        /// <summary>
        /// 课程认证
        /// </summary>
        [Description("课程认证")]
        CourseAccre = 7,
        /// <summary>
        /// 特色课程
        /// </summary>
        [Description("特色课程")]
        CharacteristicCourse = 8,
    }
    /// <summary>
    /// 年份字段field的类别
    /// </summary>
    public enum SchoolExtFiledYearTag
    {
        /// <summary>
        /// 招生年龄段起始
        /// </summary>
        [Description("招生年龄段起始")]
        Age,
        /// <summary>
        /// 招生年龄段结尾
        /// </summary>
        [Description("招生年龄段结尾")]
        MaxAge,
        /// <summary>
        /// 招生人数
        /// </summary>
        [Description("招生人数")]
        Count,
        /// <summary>
        /// 报名所需资料
        /// </summary>
        [Description("报名所需资料")]
        Data,
        /// <summary>
        /// 报名方式
        /// </summary>
        [Description("报名方式")]
        Contact,
        /// <summary>
        /// 奖学金计划
        /// </summary>
        [Description("奖学金计划")]
        Scholarship,
        /// <summary>
        /// 招生对象
        /// </summary>
        [Description("招生对象")]
        Target,
        /// <summary>
        /// 考试科目
        /// </summary>
        [Description("考试科目")]
        Subjects,
        /// <summary>
        /// 往期考试内容
        /// </summary>
        [Description("往期考试内容")]
        Pastexam,
        /// <summary>
        /// 学校划片范围
        /// </summary>
        [Description("学校划片范围")]
        Range,
        /// <summary>
        /// 申请费用
        /// </summary>
        [Description("申请费用")]
        Applicationfee,
        /// <summary>
        /// 学费
        /// </summary>
        [Description("学费")]
        Tuition,
		/// <summary>
        /// 招生日期
        /// </summary>
        [Description("招生日期")]
        Date,
		/// <summary>
        /// 录取分数线
        /// </summary>
        [Description("录取分数线")]
        Point,
		/// <summary>
        /// 对口学校
        /// </summary>
        [Description("对口学校")]
        Counterpart,
		/// <summary>
        /// 其他费用
        /// </summary>
        [Description("其他费用")]
        Otherfee

    }
}
