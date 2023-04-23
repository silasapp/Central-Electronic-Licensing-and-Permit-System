using AutoMapper;
using ELPS.Domain.Entities;
using ELPS.Domain.ViewDTOs;
using ELPS.Helpers;
using ELPS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace ELPS.App_Start
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {

                CreateMap<AccountRegisterDTO,Company>()
                    .ForMember(m=>m.User_Id,opt=>opt.MapFrom(m=>m.Email))
                    .ForMember(m=>m.Name,opt=>opt.MapFrom(m=>m.CompanyName))
                    .ForMember(m=>m.Business_Type,opt=>opt.MapFrom(m=>m.BusinessType))
                    .ForMember(m => m.RC_Number, opt => opt.MapFrom(m => m.RegistrationNumber))
                    .ForMember(m => m.Date, opt => opt.MapFrom(m=>UtilityHelper.CurrentTime.ToString())
                    );

                CreateMap<AccountRegisterDTO, NonCompanyUserModel>()
                    .ForMember(m => m.BizType, opt => opt.MapFrom(m => m.BusinessType));
           
        }
    }
    public class AccountMappingProfile:Profile
    {
        public AccountMappingProfile()
        {

        }
    }
}