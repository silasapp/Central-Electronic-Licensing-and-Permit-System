using ELPS.Domain.Entities;
using ELPS.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ELPS.Models
{
    public class DashboardViewModel
    {
        public List<AppIdentity> Licenses { get; set; }
        public Company Company { get; set; }
        public List<vCompanyFile> Documents { get; set; }
        public List<Message> Messages { get; set; }
    }

    public class CompanyLicenseDashboard
    {
        public Company Company { get; set; }
        public AppIdentity License { get; set; }
        public List<Application> Applications { get; set; }
        public List<Permit> Licenses { get; set; }

        public int Processing
        {
            get
            {
                return Applications.Where(a => a.Status.ToLower() == ApplicationStatus.Processing).Count();
            }
        }
        public int Approved
        {
            get
            {
                return Applications.Where(a => a.Status.ToLower() == ApplicationStatus.Approved).Count();
            }
        }
        public int Rejected
        {
            get
            {
                return Applications.Where(a => a.Status.ToLower() == ApplicationStatus.Rejected).Count();
            }
        }
        public int PaymentPending
        {
            get
            {
                return Applications.Where(a => a.Status.ToLower() == ApplicationStatus.PaymentPending).Count();
            }
        }
        public int PaymentCompleted
        {
            get
            {
                return Applications.Where(a => a.Status.ToLower() == ApplicationStatus.PaymentCompleted).Count();
            }
        }

        public int ValidLicensesCount
        {
            get
            {
                return Licenses.Where(a => !a.Expired).Count();
            }
        }
        public int ExpiredLicensesCount
        {
            get
            {
                return Licenses.Where(a => a.Expired).Count();
            }
        }
        public List<Permit> ValidLicenses
        {
            get
            {
                return Licenses.Where(a => !a.Expired).ToList();
            }
        }
        public List<Permit> ExpiredLicenses
        {
            get
            {
                return Licenses.Where(a => a.Expired).ToList();
            }
        }
    }

}