using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.HttpDto
{
    public class UserInfo
    {
        public Guid Id { get; set; }

        public string NickName { get; set; }
        public string UserName => NickName;

        public string Mobile { get; set; }
        public string UserPhone => Mobile;
        public string HeadImgUrl { get; set; }
    }
}
