using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public enum Result
    {
        Failure,
        Success,
        Notification,
        PageDefined
    }
    public enum ResultType
    {
        Failure,
        Success
    }
    public class HelpModel
    {
    }
    public class ErrorModel
    {
        public int Field { get; set; }
        public int Description { get; set; }
    }
    public class Response
    {
        public string Result { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string ResultType { get; set; }
        public string ReturnUrl { get; set; }
        public object Parameter1 { get; set; }
        public object Parameter2 { get; set; }
        public object Parameter3 { get; set; }
    }
}
