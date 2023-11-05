using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Infrastructure.Dapper
{
    /// <summary>
    /// 分页
    /// </summary>
    [Serializable]
    public class PagedList<T>
    {
        /// <summary>
        /// 当前页码下数据
        /// </summary>
        public IEnumerable<T> CurrentPageItems { get; set; }
        /// <summary>
        /// 当前页码
        /// </summary>
        public int CurrentPageIndex { get; set; }
        /// <summary>
        /// 每页显示数量
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalItemCount { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPageCount { get { return (int)Math.Ceiling(TotalItemCount / (double)PageSize); } }
    }


    public static partial class Extensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="currentPageItems">当前页码下数据</param>
        /// <param name="pageSize">每页显示数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="totalSize">总数量</param>
        /// <returns></returns>
        public static PagedList<T> ToPagedList<T>(this IEnumerable<T> currentPageItems, int pageSize, int pageIndex, int totalSize)
        {
            return new PagedList<T>() { CurrentPageItems = currentPageItems, PageSize = pageSize, CurrentPageIndex = pageIndex, TotalItemCount = totalSize };
        }

    }
}
