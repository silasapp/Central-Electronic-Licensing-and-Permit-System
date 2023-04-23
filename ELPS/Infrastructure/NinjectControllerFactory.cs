using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ELPS.Domain.Abstract;
using ELPS.Domain.Concrete;

namespace ELPS.Infrastructure
{
    public class NinjectControllerFactory : DefaultControllerFactory
    {
        private IKernel ninjectKernel;

        public NinjectControllerFactory()
        {
            ninjectKernel = new StandardKernel();

            AddBindings();
        }
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            return controllerType == null ? null : (IController)ninjectKernel.Get(controllerType);
        }

        private void AddBindings()
        {
            ninjectKernel.Bind<IvPermit_with_amountRepository>().To<EFvPermit_with_amountRepository>();
            ninjectKernel.Bind<IMailReceiptRepository>().To<EFMailReceiptRepository>();
            ninjectKernel.Bind<IExpiringNotificationRepository>().To<EFExpiringNotificationRepository>();
            ninjectKernel.Bind<IvExpiringLicenseRepository>().To<EFvExpiringLicenseRepository>();
            ninjectKernel.Bind<IRawRemitaResponseRepository>().To<EFRawRemitaResponseRepository>();
            ninjectKernel.Bind<IvAccountReportRepository>().To<EFvAccountReportRepository>();
            ninjectKernel.Bind<IvAffiliateRepository>().To<EFvAffiliateRepository>();
            ninjectKernel.Bind<IAffiliateRepository>().To<EFAffiliateRepository>();
            ninjectKernel.Bind<IvFacilityFileRepository>().To<EFvFacilityFileRepository>();
            ninjectKernel.Bind<IvFD_FDRepository>().To<EFvFD_FDRepository>();
            ninjectKernel.Bind<IFacilityDocumentRepository>().To<EFFacilityDocumentRepository>();
            ninjectKernel.Bind<IFacilityRepository>().To<EFFacilityRepository>();
            ninjectKernel.Bind<IvFacilityRepository>().To<EFvFacilityRepository>();
            ninjectKernel.Bind<IvZoneStateRepository>().To<EFvZoneStateRepository>();
            ninjectKernel.Bind<IZoneRepository>().To<EFZoneRepository>();
            ninjectKernel.Bind<IvZoneRepository>().To<EFvZoneRepository>();
            ninjectKernel.Bind<IZoneStateRepository>().To<EFZoneStateRepository>();
            ninjectKernel.Bind<IvBranchRepository>().To<EFvBranchRepository>();
            ninjectKernel.Bind<ICompanyNameHistoryRepository>().To<EFCompanyNameHistoryRepository>();
            ninjectKernel.Bind<IPermitCategoryRepository>().To<EFPermitCategoryRepository>();
            ninjectKernel.Bind<IvUserRoleRepository>().To<EFvUserRoleRepository>();
            ninjectKernel.Bind<IAspNetUserRepository>().To<EFAspNetUserRepository>();
            ninjectKernel.Bind<IvLockedOutUserRepository>().To<EFvLockedOutUserRepository>();
            ninjectKernel.Bind<ILockedOutUserRepository>().To<EFLockedOutUserRepository>();
            ninjectKernel.Bind<IvApplicationRepository>().To<EFvApplicationRepository>();
            ninjectKernel.Bind<IvPaymentTransactionRepository>().To<EFvPaymentTransactionRepository>();
            ninjectKernel.Bind<IvReceiptRepository>().To<EFvReceiptRepository>();
            ninjectKernel.Bind<IvInvoiceRepository>().To<EFvInvoiceRepository>();
            ninjectKernel.Bind<IInvoiceRepository>().To<EFInvoiceRepository>();
            ninjectKernel.Bind<IReceiptRepository>().To<EFReceiptRepository>();
            ninjectKernel.Bind<IApplicationRepository>().To<EFApplicationRepository>();
            ninjectKernel.Bind<IAddressRepository>().To<EFAddressRepository>();
            ninjectKernel.Bind<IAppIdentityRepository>().To<EFAppIdentityRepository>();
            ninjectKernel.Bind<IApplicationRequirementRepository>().To<EFApplicationRequirementRepository>();
            ninjectKernel.Bind<IBranchRepository>().To<EFBranchRepository>();
            ninjectKernel.Bind<ICategoryRepository>().To<ICategoryRepository>();
            ninjectKernel.Bind<IvCompanyRepository>().To<EFvCompanyRepository>();
            ninjectKernel.Bind<ICompanyRepository>().To<EFCompanyRepository>();
            ninjectKernel.Bind<ICompany_DirectorRepository>().To<EFCompany_DirectorRepository>();
            ninjectKernel.Bind<ICompany_DocumentRepository>().To<EFCompany_DocumentRepository>();
            ninjectKernel.Bind<ICompany_Key_StaffRepository>().To<EFCompany_Key_StaffRepository>();
            ninjectKernel.Bind<ICompany_MedicalRepository>().To<EFCompany_MedicalRepository>();
            ninjectKernel.Bind<ICompany_NsitfRepository>().To<EFCompany_NsitfRepository>();
            ninjectKernel.Bind<ICompany_ProffessionalRepository>().To<EFCompany_ProffessionalRepository>();
            ninjectKernel.Bind<ICompany_Technical_AgreementRepository>().To<EFCompany_Technical_AgreementRepository>();
            ninjectKernel.Bind<ICompany_Expatriate_QuotaRepository>().To<EFCompany_Expatriate_QuotaRepository>();
            ninjectKernel.Bind<ICountryRepository>().To<EFCountryRepository>();
            ninjectKernel.Bind<IDocument_TypeRepository>().To<EFDocument_TypeRepository>();
            ninjectKernel.Bind<IFileRepository>().To<EFFileRepository>();
            ninjectKernel.Bind<IKey_Staff_CertificateRepository>().To<EFKey_Staff_CertificateRepository>();
            ninjectKernel.Bind<ILicenseRepository>().To<EFLicenseRepository>();
            ninjectKernel.Bind<IMessageRepository>().To<EFMessageRepository>();
            ninjectKernel.Bind<IPayment_TransactionRepository>().To<EFPayment_TransactionRepository>();
            ninjectKernel.Bind<IPermitRepository>().To<EFPermitRepository>();
            ninjectKernel.Bind<IRemitaPaymentStatusRepository>().To<EFRemitaPaymentStatusRepository>();
            ninjectKernel.Bind<IStateRepository>().To<EFStateRepository>();
            ninjectKernel.Bind<IUserBranchRepository>().To<EFUserBranchRepository>();
            ninjectKernel.Bind<IvAddressRepository>().To<EFvAddressRepository>();
            ninjectKernel.Bind<IvCompanyNsitfRepository>().To<EFvCompanyNsitfRepository>();
            ninjectKernel.Bind<IvCompanyMedicalRepository>().To<EFvCompanyMedicalRepository>();
            ninjectKernel.Bind<IvCompanyProffessionalRepository>().To<EFvCompanyProffessionalRepository>();
            ninjectKernel.Bind<IvCompanyExpatriateQuotaRepository>().To<EFvCompanyExpatriateQuotaRepository>();
            ninjectKernel.Bind<IvCompanyFileRepository>().To<EFvCompanyFilesRepository>();
            ninjectKernel.Bind<IvCompanyTechnicalAgreementRepository>().To<EFvCompanyTechnicalAgreementRepository>();
            ninjectKernel.Bind<IvCompanyDirectorRepository>().To<EFvCompanyDirectorRepository>();
            ninjectKernel.Bind<IvPermitRepository>().To<EFvPermitRepository>();
            ninjectKernel.Bind<IStaffRepository>().To<EFStaffRepository>();
            ninjectKernel.Bind<IDivisionRepo>().To<EFDivisionsRepo>();
            ninjectKernel.Bind<IPortalToDivision>().To<EFPortalToDivision>();
        }

        private void AddBindingsx()
        {
            //ninjectKernel.Bind<IvSpecGroupRepository>().To<EFvSpecGroupRepository>();
            //ninjectKernel.Bind<IMulti_InspectionRepository>().To<EFMulti_InspectionRepository>();
            //ninjectKernel.Bind<IBusinessType_ServiceRepository>().To<EFBusinessType_ServiceRepository>();
            //ninjectKernel.Bind<IvJobSpecApplicationCheckRepository>().To<EFvJobSpecApplicationCheckRepository>();
            //ninjectKernel.Bind<IApplicationFormRepository>().To<EFApplicationFormRepository>();
            //ninjectKernel.Bind<IApplicationJobSpecPresentationRepository>().To<EFApplicationJobSpecPresentationRepository>();
            //ninjectKernel.Bind<IFieldRepository>().To<EFFieldRepository>();
            //ninjectKernel.Bind<IFieldValueRepository>().To<EFFieldValueRepository>();
            //ninjectKernel.Bind<IFormRepository>().To<EFFormRepository>();
            //ninjectKernel.Bind<IvFormFieldRepository>().To<EFvFormFieldRepository>();
            //ninjectKernel.Bind<IvFormValueRepository>().To<EFvFormValueRepository>();
            //ninjectKernel.Bind<IWaiverRepository>().To<EFWaiverRepository>();

            //ninjectKernel.Bind<IExpiredScheduledMeetingRepository>().To<EFExpiredScheduledMeetingRepository>();
            //ninjectKernel.Bind<IvMeetingScheduleApplicationRepository>().To<EFvMeetingScheduleApplicationRepository>();
            //ninjectKernel.Bind<IManagerScheduleMeetingRepository>().To<EFManagerScheduleMeetingRepository>();
            //ninjectKernel.Bind<IvManagerScheduleMeetingRepository>().To<EFvManagerScheduleMeetingRepository>();
            //ninjectKernel.Bind<IvApplication_Services_SpecsRepository>().To<EFvApplication_Services_Specs>();
            //ninjectKernel.Bind<IvInspection_routing_job_specRepository>().To<EFvInspection_routing_job_specRepository>();
            //ninjectKernel.Bind<IvZoneBranchMappingRepository>().To<EFvZoneBranchMappingRepository>();
            //ninjectKernel.Bind<IStaffRepository>().To<EFStaffRepository>();
            //ninjectKernel.Bind<IMeetingScheduleRepository>().To<EFMeetingScheduleRepository>();
            //ninjectKernel.Bind<IvUserBranchRepository>().To<EFvUserBranchRepository>();
            //ninjectKernel.Bind<IvApplication_Desk_HistoryRepository>().To<EFvApplication_Desk_HistoryRepository>();
            //ninjectKernel.Bind<IZoneBranchMappingRepository>().To<EFZoneBranchMappingRepository>();
            //ninjectKernel.Bind<IvZoneRepository>().To<EFvZoneRepository>();
            //ninjectKernel.Bind<IvZoneMappingRepository>().To<EFvZoneMappingRepository>();
            //ninjectKernel.Bind<IvGroupDetailRepository>().To<EFvGroupDetailRepository>();
            //ninjectKernel.Bind<IvAddressRepository>().To<EFvAddressRepository>();
            //ninjectKernel.Bind<IvCompanyExpatriateQuotaRepository>().To<EFvCompanyExpatriateQuotaRepository>();
            //ninjectKernel.Bind<IvCompanyMedicalRepository>().To<EFvCompanyMedicalRepository>();
            //ninjectKernel.Bind<IvCompanyNsitfRepository>().To<EFvCompanyNsitfRepository>();
            //ninjectKernel.Bind<IvCompanyProffessionalRepository>().To<EFvCompanyProffessionalRepository>();
            //ninjectKernel.Bind<IvCompanyTechnicalAgreementRepository>().To<EFvCompanyTechnicalAgreementRepository>();
            //ninjectKernel.Bind<IvCompanyDirectorRepository>().To<EFvCompanyDirectorRepository>();
            //ninjectKernel.Bind<IvPermitRepository>().To<EFvPermitRepository>();
            //ninjectKernel.Bind<IvApplicationDocumentRepository>().To<EFvApplicationDocumentRepository>();
            //ninjectKernel.Bind<IvApplicationJobSpecificationRepository>().To<EFvApplicationJobSpecificationRepository>();
            //ninjectKernel.Bind<IvApplicationRepository>().To<EFvApplicationRepository>();
            //ninjectKernel.Bind<IvApplicationServiceRepository>().To<EFvApplicationServiceRepository>();
            //ninjectKernel.Bind<IvRequiredFileRepository>().To<EFvRequiredFileRepository>();
            //ninjectKernel.Bind<IvCompanyFileRepository>().To<EFvCompanyFilesRepository>();
            //ninjectKernel.Bind<IApplicationRepository>().To<EFApplicationRepository>();
            //ninjectKernel.Bind<IApplication_Desk_HistoryRepository>().To<EFApplication_Desk_HistoryRepository>();
            //ninjectKernel.Bind<IApplication_DocumentRepository>().To<EFApplication_DocumentRepository>();
            //ninjectKernel.Bind<IApplication_Job_SpecificationRepository>().To<EFApplication_Job_SpecificationRepository>();
            //ninjectKernel.Bind<IApplication_ServiceRepository>().To<EFApplication_ServiceRepository>();
            //ninjectKernel.Bind<IAspNetRolesRepository>().To<EFAspNetRoleRepository>();
            //ninjectKernel.Bind<IAudit_TrailRepository>().To<EFAudit_TrailRepository>();
            //ninjectKernel.Bind<IAuthAssignmentRepository>().To<EFAuthAssignmentRepository>();
            //ninjectKernel.Bind<IAuthItemRepository>().To<EFAuthItemRepository>();
            //ninjectKernel.Bind<IAuthItemChildRepository>().To<EFAuthItemChildRepository>();
            //ninjectKernel.Bind<IBranch_GroupRepository>().To<EFBranch_GroupRepository>();
            //ninjectKernel.Bind<IC_GroupRepository>().To<EFC_GroupRepository>();
            //ninjectKernel.Bind<IC_Group_DescriptionRepository>().To<EFC_Group_DescriptionRepository>();
            //ninjectKernel.Bind<ICacheRepository>().To<EFCacheRepository>();
            //ninjectKernel.Bind<ICompany_DirectorRepository>().To<EFCompany_DirectorRepository>();
            //ninjectKernel.Bind<ICompany_DocumentRepository>().To<EFCompany_DocumentRepository>();
            //ninjectKernel.Bind<ICompany_Expatriate_QuotaRepository>().To<EFCompany_Expatriate_QuotaRepository>();
            //ninjectKernel.Bind<ICompany_Key_StaffRepository>().To<EFCompany_Key_StaffRepository>();
            //ninjectKernel.Bind<ICompany_MedicalRepository>().To<EFCompany_MedicalRepository>();
            //ninjectKernel.Bind<ICompany_NsitfRepository>().To<EFCompany_NsitfRepository>();
            //ninjectKernel.Bind<ICompany_ProffessionalRepository>().To<EFCompany_ProffessionalRepository>();
            //ninjectKernel.Bind<ICompany_Technical_AgreementRepository>().To<EFCompany_Technical_AgreementRepository>();
            //ninjectKernel.Bind<ICurrencyRepository>().To<EFCurrencyRepository>();
            //ninjectKernel.Bind<ICustomer_C_GroupRepository>().To<EFCustomer_C_GroupRepository>();
            //ninjectKernel.Bind<IDepartmentRepository>().To<EFDepartmentRepository>();
            //ninjectKernel.Bind<IDocument_TypeRepository>().To<EFDocument_TypeRepository>();
            //ninjectKernel.Bind<IDocument_Type_CategoryRepository>().To<EFDocument_Type_CategoryRepository>();
            //ninjectKernel.Bind<IDocument_Type_Job_SpecificationRepository>().To<EFDocument_Type_Job_SpecificationRepository>();
            //ninjectKernel.Bind<IDocument_Type_ServiceRepository>().To<EFDocument_Type_ServiceRepository>();
            //ninjectKernel.Bind<IFaqRepository>().To<EFFaqRepository>();
            //ninjectKernel.Bind<IFaq_DescriptionRepository>().To<EFFaq_DescriptionRepository>();
            //ninjectKernel.Bind<IGroupRepository>().To<EFGroupRepository>();
            //ninjectKernel.Bind<IInspection_RoutingRepository>().To<EFInspection_RoutingRepository>();
            //ninjectKernel.Bind<IInspection_Routing_CategoryRepository>().To<EFInspection_Routing_CategoryRepository>();
            //ninjectKernel.Bind<IInspection_Routing_Job_SpecificationRepository>().To<EFInspection_Routing_Job_SpecificationRepository>();
            //ninjectKernel.Bind<IInspection_Routing_ServiceRepository>().To<EFInspection_Routing_ServiceRepository>();
            //ninjectKernel.Bind<IInvoiceRepository>().To<EFInvoiceRepository>();
            //ninjectKernel.Bind<IJob_SpecificationRepository>().To<EFJob_SpecificationRepository>();
            //ninjectKernel.Bind<IvJobSpecificationRepository>().To<EFvJobSpecificationRepository>();
            //ninjectKernel.Bind<IKey_Staff_CertificateRepository>().To<EFKey_Staff_CertificateRepository>();
            //ninjectKernel.Bind<IMedical_OrganisationRepository>().To<EFMedical_OrganisationRepository>();
            //ninjectKernel.Bind<IMessage_BackendRepository>().To<EFMessage_BackendRepository>();
            //ninjectKernel.Bind<IMisplaced_PaymentsRepository>().To<EFMisplaced_PaymentsRepository>();
            //ninjectKernel.Bind<INsitfRepository>().To<EFNsitfRepository>();
            //ninjectKernel.Bind<IOgisContextRepository>().To<EFOgisContextRepository>();
            //ninjectKernel.Bind<IPageRepository>().To<EFPageRepository>();
            //ninjectKernel.Bind<IPassword_HistoryRepository>().To<EFPassword_HistoryRepository>();
            //ninjectKernel.Bind<IPayment_Notification_RequestRepository>().To<EFPayment_Notification_RequestRepository>();
            //ninjectKernel.Bind<IPayment_TransactionRepository>().To<EFPayment_TransactionRepository>();
            //ninjectKernel.Bind<IPermitRepository>().To<EFPermitRepository>();
            //ninjectKernel.Bind<IProffessional_OrganisationRepository>().To<EFProffessional_OrganisationRepository>();
            //ninjectKernel.Bind<IProfileRepository>().To<EFProfileRepository>();
            //ninjectKernel.Bind<IProfiles_FieldsRepository>().To<EFProfiles_FieldsRepository>();
            //ninjectKernel.Bind<IReversal_TransactionRepository>().To<EFReversal_TransactionRepository>();
            //ninjectKernel.Bind<IServiceRepository>().To<EFServiceRepository>();
            //ninjectKernel.Bind<ISettingRepository>().To<EFSettingRepository>();
            //ninjectKernel.Bind<IUserRepository>().To<EFUserRepository>();
            //ninjectKernel.Bind<IUser_GroupRepository>().To<EFUser_GroupRepositoryitory>();
            //ninjectKernel.Bind<IZoneRepository>().To<EFZoneRepository>();
            //ninjectKernel.Bind<IZone_MappingRepository>().To<EFZone_MappingRepository>();
            //ninjectKernel.Bind<IvCategoryDocumentRepository>().To<EFvCategoryDocumentRepository>();
            //ninjectKernel.Bind<IvServiceDocumentRepository>().To<EFvServiceDocumentRepository>();
            //ninjectKernel.Bind<IInspection_RuleRepository>().To<EFInspection_RuleRepository>();
            //ninjectKernel.Bind<IvInspection_RuleRepository>().To<EFvInspection_RuleRepository>();
            //ninjectKernel.Bind<IApplication_ProcessingRepository>().To<EFApplication_ProcessingRepository>();
            //ninjectKernel.Bind<IvApplication_ProcessingRepository>().To<EFvApplication_ProcessingRepository>();

            //ninjectKernel.Bind<IWorkRoleRepository>().To<EFWorkRoleRepository>();
            //ninjectKernel.Bind<IUserBranchRepository>().To<EFUserBranchRepository>();
            //ninjectKernel.Bind<IvApplicationAddressRepository>().To<EFvApplicationAddressRepository>();
            //ninjectKernel.Bind<IvApplicationInspectionRepository>().To<EFvApplicationInspectionRepository>();
            //ninjectKernel.Bind<IDocument_Type_ApplicationRepository>().To<EFDocument_Type_ApplicationRepository>();
            //ninjectKernel.Bind<ISpecGroupRepository>().To<EFSpecGroupRepository>();
            //ninjectKernel.Bind<ISpecGroupMemberRepository>().To<EFSpecGroupMemberRepository>();
            //ninjectKernel.Bind<IvPaymentNotificationRepository>().To<EFvPaymentNotificationRepository>();
            //ninjectKernel.Bind<IManagerReminderRepository>().To<EFManagerReminderRepository>();
            //ninjectKernel.Bind<IvReceiptRepository>().To<EFvReceiptRepository>();
            //ninjectKernel.Bind<ICrawlerRepository>().To<EFCrawlerRepository>();
            //ninjectKernel.Bind<I_ReceiptRepository>().To<EF_ReceiptRepository>();
            //ninjectKernel.Bind<IReceiptRepository>().To<EFReceiptRepository>();
            //ninjectKernel.Bind<ILeaveRepository>().To<EFLeaveRepository>();
            //ninjectKernel.Bind<IvInvoiceRepository>().To<EFvInvoiceRepository>();
            //ninjectKernel.Bind<IRemita_TransactionRepository>().To<EFRemita_TransactionRepository>();
            //ninjectKernel.Bind<IRunTimeRepository>().To<EFRunTimeRepository>();
            //ninjectKernel.Bind<IInspectionScheduleRepository>().To<EFInspectionScheduleRepository>();
            //ninjectKernel.Bind<INotificationRepository>().To<EFNotificationRepository>();
            //ninjectKernel.Bind<IManualRemitaValueRepository>().To<EFManualRemitaValueRepository>();
            //ninjectKernel.Bind<IRemitaPaymentStatusRepository>().To<EFRemitaPaymentStatusRepository>();
            //ninjectKernel.Bind<IvSpecGroupMembersRepository>().To<EFvSpecGroupMemberRepository>();


        }

    }
}