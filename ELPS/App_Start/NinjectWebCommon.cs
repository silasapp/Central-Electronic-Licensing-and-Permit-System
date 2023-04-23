[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(ELPS.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(ELPS.App_Start.NinjectWebCommon), "Stop")]

namespace ELPS.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using ELPS.Domain.Abstract;
    using ELPS.Domain.Concrete;

    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {

            kernel.Bind<ICompanyNameHistoryRepository>().To<EFCompanyNameHistoryRepository>();
            kernel.Bind<IExpiringNotificationRepository>().To<EFExpiringNotificationRepository>();
            kernel.Bind<IvExpiringLicenseRepository>().To<EFvExpiringLicenseRepository>();
            kernel.Bind<IRawRemitaResponseRepository>().To<EFRawRemitaResponseRepository>();
            kernel.Bind<IExternalAppIdentityRepository>().To<EFExternalAppIdentityRepository>();
            kernel.Bind<IvAffiliateRepository>().To<EFvAffiliateRepository>();
            kernel.Bind<IAffiliateRepository>().To<EFAffiliateRepository>();
            kernel.Bind<IvFacilityFileRepository>().To<EFvFacilityFileRepository>();
            kernel.Bind<IvFacilityRepository>().To<EFvFacilityRepository>();
            kernel.Bind<IFacilityRepository>().To<EFFacilityRepository>();
            kernel.Bind<IFacilityDocumentRepository>().To<EFFacilityDocumentRepository>();
            kernel.Bind<IvApplicationRepository>().To<EFvApplicationRepository>();
            kernel.Bind<IvReceiptRepository>().To<EFvReceiptRepository>();
            kernel.Bind<IDocument_TypeRepository>().To<EFDocument_TypeRepository>();
            kernel.Bind<IStaffRepository>().To<EFStaffRepository>();
            kernel.Bind<IvBranchRepository>().To<EFvBranchRepository>();
            kernel.Bind<IZoneRepository>().To<EFZoneRepository>();
            kernel.Bind<IvZoneRepository>().To<EFvZoneRepository>();
            kernel.Bind<IZoneStateRepository>().To<EFZoneStateRepository>();
            kernel.Bind<IvZoneStateRepository>().To<EFvZoneStateRepository>();
            kernel.Bind<IStateRepository>().To<EFStateRepository>();
            kernel.Bind<IKey_Staff_CertificateRepository>().To<EFKey_Staff_CertificateRepository>();
            kernel.Bind<IReceiptRepository>().To<EFReceiptRepository>();
            kernel.Bind<ILicenseRepository>().To<EFLicenseRepository>();
            kernel.Bind<IInvoiceRepository>().To<EFInvoiceRepository>();
            kernel.Bind<IApplicationRepository>().To<EFApplicationRepository>();
            kernel.Bind<IAddressRepository>().To<EFAddressRepository>();
            kernel.Bind<IvAddressRepository>().To<EFvAddressRepository>();
            kernel.Bind<IvCompanyFileRepository>().To<EFvCompanyFilesRepository>();
            kernel.Bind<IAppIdentityRepository>().To<EFAppIdentityRepository>();
            kernel.Bind<IApplicationRequirementRepository>().To<EFApplicationRequirementRepository>();
            kernel.Bind<IBranchRepository>().To<EFBranchRepository>();
            kernel.Bind<ICompany_DirectorRepository>().To<EFCompany_DirectorRepository>();
            kernel.Bind<ICompany_DocumentRepository>().To<EFCompany_DocumentRepository>();
            kernel.Bind<ICompany_Expatriate_QuotaRepository>().To<EFCompany_Expatriate_QuotaRepository>();
            kernel.Bind<ICompany_Key_StaffRepository>().To<EFCompany_Key_StaffRepository>();
            kernel.Bind<ICompany_MedicalRepository>().To<EFCompany_MedicalRepository>();
            kernel.Bind<ICompany_ProffessionalRepository>().To<EFCompany_ProffessionalRepository>();
            kernel.Bind<ICompany_Technical_AgreementRepository>().To<EFCompany_Technical_AgreementRepository>();
            kernel.Bind<ICompany_NsitfRepository>().To<EFCompany_NsitfRepository>();
            kernel.Bind<IFileRepository>().To<EFFileRepository>();
            kernel.Bind<ICompanyRepository>().To<EFCompanyRepository>();
            kernel.Bind<IMessageRepository>().To<EFMessageRepository>();
            kernel.Bind<INsitfRepository>().To<EFNsitfRepository>();
            kernel.Bind<IPayment_TransactionRepository>().To<EFPayment_TransactionRepository>();
            kernel.Bind<IPermitRepository>().To<EFPermitRepository>();
            kernel.Bind<IvPermitRepository>().To<EFvPermitRepository>();
            kernel.Bind<IRemitaPaymentStatusRepository>().To<EFRemitaPaymentStatusRepository>();
            kernel.Bind<IUserBranchRepository>().To<EFUserBranchRepository>();
            kernel.Bind<ICountryRepository>().To<EFCountryRepository>();
            kernel.Bind<IvCompanyNsitfRepository>().To<EFvCompanyNsitfRepository>();
            kernel.Bind<IvCompanyMedicalRepository>().To<EFvCompanyMedicalRepository>();
            kernel.Bind<IvCompanyProffessionalRepository>().To<EFvCompanyProffessionalRepository>();
            kernel.Bind<IvCompanyExpatriateQuotaRepository>().To<EFvCompanyExpatriateQuotaRepository>();
            kernel.Bind<IvCompanyTechnicalAgreementRepository>().To<EFvCompanyTechnicalAgreementRepository>();
            kernel.Bind<IvCompanyDirectorRepository>().To<EFvCompanyDirectorRepository>();
            kernel.Bind<IDivisionRepo>().To<EFDivisionsRepo>();

        }
    }
}
