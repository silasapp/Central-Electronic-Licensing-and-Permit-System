using ELPS.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ELPS.Controllers
{
    public class TestController : ApiController
    {
        IAddressRepository _addrep;

        public TestController(IAddressRepository addrep)
        {
            _addrep = addrep;
        }

        public IQueryable<int> GetvComments()
        {
            var y = Request;
            if (y.RequestUri!=null)
            {
                var z = y.RequestUri;
                if (z.AbsolutePath=="")
                {
                    
                }
            }
            var x=new List<int>();
            x.Add(1);
            return  x.AsQueryable();// db.vComments;
        }

    }
}
