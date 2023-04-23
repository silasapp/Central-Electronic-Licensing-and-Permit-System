using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using ELPS.Domain.Abstract;
using System.ComponentModel.DataAnnotations;
using ELPS.Domain.Concrete;
using ELPS.Domain.Entities;

namespace ELPS.Models
{
    public class ELPS_DBContext : DbContext, IDbContext
    {
        public ELPS_DBContext()
            : base("ELPS_DBContext")
        {
            this.Configuration.LazyLoadingEnabled = false;
            //this.Database.Initialize(false);
            //Database.SetInitializer<ELPS_DBContext>(new DropCreateDatabaseAlways<ELPS_DBContext>());
            Database.SetInitializer<ELPS_DBContext>(null);
            this.Configuration.ProxyCreationEnabled = false;
        }


       // public virtual DbSet<ManagerReminder> ManagerReminders { get; set; }
        public virtual DbSet<vReceipt> vReceipts { get; set; }
        //public virtual DbSet<Crawler> Crawlers { get; set; }
        //public virtual DbSet<iReceipt> iReceipts { get; set; }
        public virtual DbSet<Receipt> Receipts { get; set; }
       // public virtual DbSet<Leave> Leaves { get; set; }
        public virtual DbSet<vInvoice> vInvoices { get; set; }
        //public virtual DbSet<Remita_Transaction> Remita_Transactions { get; set; }
        // virtual DbSet<RunTime> RunTimes { get; set; }
        //public virtual DbSet<InspectionSchedule> InspectionSchedules { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        //public virtual DbSet<ManualRemitaValue> ManualRemitaValues { get; set; }
        //public virtual DbSet<RemitaPaymentStatus> RemitaPaymentStatuses { get; set; }
        //public virtual DbSet<vSpecGroupMembers> vSpecGroupMembers { get; set; }
        //public virtual DbSet<Waiver> Waivers { get; set; }
        //public virtual DbSet<vSpecGroup> vSpecGroups { get; set; }
        //public virtual DbSet<Multi_Inspection> Multi_Inspections { get; set; }
        //public virtual DbSet<BusinessType_Service> BusinessType_Services { get; set; }
        //public virtual DbSet<ApplicationJobSpecPresentation> ApplicationJobSpecPresentations { get; set; }
        //public virtual DbSet<vJobSpecApplicationCheck> vJobSpecApplicationChecks { get; set; }
        //public virtual DbSet<ApplicationForm> ApplicationForms { get; set; }
        //public virtual DbSet<Form> Forms { get; set; }
        //public virtual DbSet<vFormField> vFormFields { get; set; }
        //public virtual DbSet<vFormValue> vFormValues { get; set; }
        //public virtual DbSet<Field> Fields { get; set; }
        //public virtual DbSet<FieldValue> FieldValues { get; set; }

        //public virtual DbSet<ExpiredScheduledMeeting> ExpiredScheduledMeetings { get; set; }
        //public virtual DbSet<vMeetingScheduleApplication> vMeetingScheduleApplications { get; set; }
        //public virtual DbSet<ManagerScheduleMeeting> ManagerScheduleMeetings { get; set; }
        //public virtual DbSet<vManagerScheduleMeeting> vManagerScheduleMeetings { get; set; }

        public virtual DbSet<Staff> Staffs { get; set; }
        //public virtual DbSet<vInspection_routing_job_spec> vInspection_routing_job_specs { get; set; }

        //public virtual DbSet<vApplication_Services_Spec> vApplication_Services_Specs { get; set; }
        //MeetingSchedule
        public virtual DbSet<vUserBranch> vUserBranches { get; set; }
        //public virtual DbSet<MeetingSchedule> MeetingSchedules { get; set; }
        //public virtual DbSet<vApplicationDeskHistory> vApplicationDeskHistories { get; set; }
        //public virtual DbSet<ZoneBranchMapping> ZoneBranchMappings { get; set; }
        //public virtual DbSet<vZoneBranchMapping> vZoneBranchMappings { get; set; }
        //public virtual DbSet<vZone> vZones { get; set; }
        //public virtual DbSet<vZoneMapping> vZoneMappings { get; set; }
        public virtual DbSet<vCompanyDirector> vCompanyDirectors { get; set; }
        public virtual DbSet<vAddress> vAddresses { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<vCompanyTechnicalAgreement> vCompanyTechnicalAgreements { get; set; }
        public virtual DbSet<vCompanyMedical> vCompanyMedicals { get; set; }
        public virtual DbSet<vCompanyNsitf> vCompanyNsitfs { get; set; }
        public virtual DbSet<vCompanyProffessional> vCompanyProffessionals { get; set; }
        public virtual DbSet<vCompanyExpatriateQuota> vCompanyExpatriateQuotas { get; set; }
        public virtual DbSet<vApplication> vApplications { get; set; }
        //public virtual DbSet<vApplicationService> vApplicationServices { get; set; }
        //public virtual DbSet<vApplicationJobSpecification> vApplicationJobSpecifications { get; set; }
        //public virtual DbSet<vApplicationDocument> vApplicationDocuments { get; set; }
        //public virtual DbSet<vRequiredFile> vRequiredFiles { get; set; }
        public virtual DbSet<vCompanyFile> vCompanyFiles { get; set; }
        public virtual DbSet<Address> addresses { get; set; }
        public virtual DbSet<Application> applications { get; set; }
        //public virtual DbSet<Application_Desk_History> application_desk_history { get; set; }
        //public virtual DbSet<Application_Document> application_document { get; set; }
        //public virtual DbSet<Application_Job_Specification> application_job_specification { get; set; }
        //public virtual DbSet<Application_Service> application_service { get; set; }
        //public virtual DbSet<Audit_Trail> audit_trail { get; set; }
        //public virtual DbSet<AuthAssignment> authassignments { get; set; }
        //public virtual DbSet<AuthItem> authitems { get; set; }
        //public virtual DbSet<AuthItemChild> authitemchilds { get; set; }
        public virtual DbSet<Branch> branches { get; set; }
        //public virtual DbSet<C_Group> c_group { get; set; }
        //public virtual DbSet<C_Group_Description> c_group_description { get; set; }
        //public virtual DbSet<Cache> caches { get; set; }
        public virtual DbSet<Category> categories { get; set; }
        public virtual DbSet<Company> companies { get; set; }
        public virtual DbSet<Company_Director> company_directors { get; set; }
        public virtual DbSet<Company_Document> company_documents { get; set; }
        public virtual DbSet<Company_Expatriate_Quota> company_expatriate_quotas { get; set; }
        public virtual DbSet<Company_Key_Staff> company_key_staffs { get; set; }
        public virtual DbSet<Company_Medical> company_medicals { get; set; }
        public virtual DbSet<Company_Nsitf> company_nsitfs { get; set; }
        public virtual DbSet<Company_Proffessional> company_proffessional { get; set; }
        public virtual DbSet<Company_Technical_Agreement> company_technical_agreement { get; set; }
        public virtual DbSet<Country> countries { get; set; }
        //public virtual DbSet<Currency> currencies { get; set; }
        //public virtual DbSet<Customer_C_Group> customer_c_groups { get; set; }
        public virtual DbSet<Document_Type> document_types { get; set; }
        //public virtual DbSet<Faq> faqs { get; set; }
        //public virtual DbSet<Faq_Description> faq_descriptions { get; set; }
        public virtual DbSet<File> files { get; set; }
        //public virtual DbSet<Group> groups { get; set; }
        //public virtual DbSet<Inspection_Routing> inspection_routings { get; set; }
        public virtual DbSet<Invoice> invoices { get; set; }
        //public virtual DbSet<Job_Specification> job_specifications { get; set; }
        public virtual DbSet<Key_Staff_Certificate> key_staff_certificates { get; set; }
        public virtual DbSet<Medical_Organisation> medical_organisations { get; set; }
        public virtual DbSet<Message> messages { get; set; }
        //public virtual DbSet<Message_Backend> message_backends { get; set; }
        //public virtual DbSet<Misplaced_Payments> misplaced_payments { get; set; }
        public virtual DbSet<Nsitf> nsitfs { get; set; }
        //public virtual DbSet<Page> pages { get; set; }
        //public virtual DbSet<Password_History> password_history { get; set; }
        //public virtual DbSet<Payment_Notification_Request> payment_notification_requests { get; set; }
        public virtual DbSet<Payment_Transaction> payment_transactions { get; set; }
        public virtual DbSet<Permit> permits { get; set; }
        public virtual DbSet<Proffessional_Organisation> proffessional_organisations { get; set; }
        //public virtual DbSet<Profile> profiles { get; set; }
        //public virtual DbSet<Profiles_Fields> profiles_fields { get; set; }
        //public virtual DbSet<Reversal_Transaction> reversal_transaction { get; set; }
        //public virtual DbSet<Service> services { get; set; }
        //public virtual DbSet<Setting> settings { get; set; }
        //public virtual DbSet<User_Group> user_groups { get; set; }
        //public virtual DbSet<User> users { get; set; }
        //public virtual DbSet<Zone> zones { get; set; }
        //public virtual DbSet<Zone_Mapping> zone_mappings { get; set; }
        //public virtual DbSet<Branch_Group> branch_groups { get; set; }
        //public virtual DbSet<Document_Type_Category> document_type_categories { get; set; }
        //public virtual DbSet<Document_Type_Job_Specification> document_type_job_specifications { get; set; }
        //public virtual DbSet<Document_Type_Service> document_type_services { get; set; }
        //public virtual DbSet<Inspection_Routing_Category> inspection_routing_categories { get; set; }
        //public virtual DbSet<Inspection_Routing_Job_Specification> inspection_routing_job_specifications { get; set; }
        //public virtual DbSet<Inspection_Routing_Service> inspection_routing_services { get; set; }
        //public virtual DbSet<vGroupDetail> vgroupdetail { get; set; }
        public virtual DbSet<AuditLog> AuditLogs { get; set; }
        //public virtual DbSet<Department> Departments { get; set; }
        //public virtual DbSet<vJobSpecification> vJobSpecification { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<vCategoryDocument> vCategoryDocuments { get; set; }
        //public virtual DbSet<vServiceDocument> vServiceDocuments { get; set; }
        //public virtual DbSet<Inspection_Rule> Inspection_Rules { get; set; }
        //public virtual DbSet<vInspection_Rule> vInspection_Rules { get; set; }
        public virtual DbSet<WorkRole> WorkRoles { get; set; }
        //public virtual DbSet<Application_Processing> Application_Processings { get; set; }
        //public virtual DbSet<vApplication_Processing> vApplication_Processings { get; set; }
        public virtual DbSet<UserBranch> UserBranches { get; set; }
        //public virtual DbSet<vApplicationAddress> vApplicationAddresses { get; set; }
        //public virtual DbSet<vApplicationInspection> vApplicationInspection { get; set; }
        //public virtual DbSet<Document_Type_Application> Document_Type_Applications { get; set; }
        public virtual DbSet<vPermit> vPermits { get; set; }
        //public virtual DbSet<SpecGroup> SpecGroups { get; set; }
        //public virtual DbSet<SpecGroupMember> SpecGroupMembers { get; set; }
        //public virtual DbSet<vPaymentNotification> vPaymentNotification { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
        }

        // This is overridden to prevent someone from calling SaveChanges without specifying the user making the change
        public override int SaveChanges()
        {
            throw new InvalidOperationException("User ID must be provided");
        }

        public int SaveChanges(string userId)
        {
            if (userId.ToLower() == "system" || userId.ToLower() == "admin")
                return base.SaveChanges();
            else
                throw new InvalidOperationException("User ID must be provided");
        }

        public int SaveChanges(string userId, string Ip)
        {
            foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == System.Data.Entity.EntityState.Added || p.State == System.Data.Entity.EntityState.Deleted || p.State == System.Data.Entity.EntityState.Modified))
            {
                // For each changed record, get the audit record entries and add them
                foreach (AuditLog x in GetAuditRecordsForChange(ent, userId, Ip))
                {
                    this.AuditLogs.Add(x);
                }
            }
            // Call the original SaveChanges(), which will save both the changes made and the audit records
            return base.SaveChanges();
        }

        private List<AuditLog> GetAuditRecordsForChange(DbEntityEntry dbEntry, string userId, string Ip)
        {
            List<AuditLog> result = new List<AuditLog>();

            DateTime changeTime = DateTime.UtcNow;

            // Get the Table() attribute, if one exists
            TableAttribute tableAttr = dbEntry.Entity.GetType().GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() as TableAttribute;

            // Get table name (if it has a Table attribute, use that, otherwise get the pluralized name)
            string tableName = tableAttr != null ? tableAttr.Name : dbEntry.Entity.GetType().Name;

            // Get primary key value (If you have more than one key column, this will need to be adjusted)
            if (dbEntry != null)
            {
                string keyName = dbEntry.Entity.GetType().GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Any()).Name;
                //string keyName = dbEntry.Entity.GetType().GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Count() < 0).Name;
                if (dbEntry.State == System.Data.Entity.EntityState.Added)
                {
                    // For Inserts, just add the whole record
                    // If the entity implements IDescribableEntity, use the description from Describe(), otherwise use ToString()
                    result.Add(new AuditLog()
                    {
                        AuditLogId = Guid.NewGuid(),
                        UserId = userId,
                        EventDateUTC = changeTime,
                        EventType = "A", // Added
                        TableName = tableName,
                        //RecordId = dbEntry.CurrentValues.GetValue<object>(keyName).ToString(),  // Again, adjust this if you have a multi-column key
                        RecordId = dbEntry.CurrentValues.GetValue<object>(keyName).ToString(),  // Again, adjust this if you have a multi-column key
                        //"Couldnt get this, will comeback to this",
                        ColumnName = "*ALL",    // Or make it nullable, whatever you want
                        NewValue = dbEntry.CurrentValues.ToObject().ToString()
                    }
                        );
                }
                else if (dbEntry.State == System.Data.Entity.EntityState.Deleted)
                {
                    // Same with deletes, do the whole record, and use either the description from Describe() or ToString()
                    result.Add(new AuditLog()
                    {
                        AuditLogId = Guid.NewGuid(),
                        UserId = userId,
                        EventDateUTC = changeTime,
                        EventType = "D", // Deleted
                        TableName = tableName,
                        RecordId = dbEntry.OriginalValues.GetValue<object>(keyName).ToString(),
                        ColumnName = "*ALL",
                        IP = Ip,
                        NewValue = dbEntry.OriginalValues.ToObject().ToString()
                    }
                        );
                }
                else if (dbEntry.State == System.Data.Entity.EntityState.Modified)
                {
                    foreach (string propertyName in dbEntry.OriginalValues.PropertyNames)
                    {
                        // For updates, we only want to capture the columns that actually changed
                        if (!object.Equals(dbEntry.OriginalValues.GetValue<object>(propertyName), dbEntry.CurrentValues.GetValue<object>(propertyName)))
                        {
                            result.Add(new AuditLog()
                            {
                                AuditLogId = Guid.NewGuid(),
                                UserId = userId,
                                EventDateUTC = changeTime,
                                EventType = "M",    // Modified
                                TableName = tableName,
                                RecordId = dbEntry.OriginalValues.GetValue<object>(keyName).ToString(),
                                ColumnName = propertyName,
                                OriginalValue = dbEntry.OriginalValues.GetValue<object>(propertyName) == null ? null : dbEntry.OriginalValues.GetValue<object>(propertyName).ToString(),
                                NewValue = dbEntry.CurrentValues.GetValue<object>(propertyName) == null ? null : dbEntry.CurrentValues.GetValue<object>(propertyName).ToString()
                            }
                                );
                        }
                    }
                }
            }
            // Otherwise, don't do anything, we don't care about Unchanged or Detached entities

            return result;
        }

    }

    public class AuditDTO
    {
        
    }

    public class AuditHelper
    {

    }
}
