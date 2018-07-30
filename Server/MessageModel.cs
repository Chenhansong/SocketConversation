using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class MessageModel
    {
        public string Id { get; set; }

        /// <summary>
        /// 发送的文本消息
        /// </summary>
        public string MessageText { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 文件
        /// </summary>
        public string MessageFile { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 唯一表示符
        /// </summary>
        public string Guid { get; set; }
    }
}
