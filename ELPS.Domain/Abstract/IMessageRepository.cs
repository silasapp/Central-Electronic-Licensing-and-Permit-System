using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELPS.Domain.Entities;

namespace ELPS.Domain.Abstract
{
    public interface IMessageRepository : IGenericRepository<Message>
    {
        /// <summary>
        /// This return a Message Model by initializing its parameter
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="msgBody"></param>
        /// <param name="senderId"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        Message CreateMessage(int companyId, string msgBody, string senderId, string subject);
    }
}
