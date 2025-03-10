﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XHS.Models.Enum;

namespace XHS.Models.XHS.InputModel
{
    public class UserPostedInputModel
    {
        /// <summary>
        /// 数量
        /// </summary>
        public int num { get; set; } = 30;

        public string cursor { get; set; } = "";

        public string user_id { get; set; }
        /// <summary>
        /// 笔记类型
        /// </summary>
        public NoteTypeEnum NoteTypeEnum { get; set; } = NoteTypeEnum.UserPosted;
    }
}
