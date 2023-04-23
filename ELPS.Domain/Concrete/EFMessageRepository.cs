using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Domain.Concrete;

namespace ELPS.Domain.Concrete
{
    public class EFMessageRepository : GenericRepository<ELPSContext, Message>, IMessageRepository
    {
        public object UtilityHelper { get; private set; }

        public Message CreateMessage(int companyId, string msgBody, string senderId, string subject)
        {
            return new Message()
            {
                Company_Id = companyId,
                Content = msgBody,
                Date = DateTime.Now,
                Read = 0,
                Subject = subject,
                Sender_Id = senderId,
            };
        }
    }
}
