using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain.Enum
{
    /// <summary>
    /// 类型(
    /// 1:学校荣誉;
    /// 2:学生荣誉;
    /// 3:校长风采;
    /// 4:教师风采;
    /// 5:硬件设施;
    /// 6:社团活动;
    /// 7:各年级课程表;
    /// 8:作息时间表;
    /// 9:校车路线;
    /// 10:学校品牌;)
    /// </summary>
    [Obsolete("use 'SchoolImageEnum' instead")]
    public enum ImageType
    {
        /// <summary>
        /// 学校荣誉
        /// </summary>
        Schoolhonor = 1,


        /// <summary>
        /// 学生荣誉
        /// </summary>
        Studenthonor = 2,


        /// <summary>
        /// 校长风采
        /// </summary>
        Principal = 3,


        /// <summary>
        /// 教师风采
        /// </summary>
        Teacher = 4,

    }
}
