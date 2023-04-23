using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using ELPS.Domain.Concrete;

namespace ELPS.Domain.Concrete
{
    public class EFPayment_TransactionRepository : GenericRepository<ELPSContext, Payment_Transaction>, IPayment_TransactionRepository
    {
    }
}
