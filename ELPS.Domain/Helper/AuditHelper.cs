using ELPS.Domain.Abstract;
using ELPS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Helper
{
    class AuditHelper
    {
        private readonly IDbContext _dbContext;
        public AuditHelper(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddAuditLog(string userId, string Ip)
        {
            _dbContext.ChangeTracker.DetectChanges();
            List<AuditLog> result = new List<AuditLog>();

            foreach (var ent in _dbContext.ChangeTracker.Entries())
            {
                if (ent.Entity is AuditLog || ent.State == EntityState.Detached ||ent.State == EntityState.Unchanged)
                {
                    continue;
                }
                if (Ip == null)
                {
                    if (userId.ToLower() != "system" && userId.ToLower() != "admin")
                        throw new InvalidOperationException("User ID must be provided");
                }

                var auditEntry = new AuditEntry(ent, userId, Ip);
                result.Add(auditEntry._auditLog);
            }

            if (result.Any())
            {
                _dbContext.AuditLogs.AddRange(result);
            }
  
        }


    }

    public class AuditEntry
    {
        public DbEntityEntry dbEntry { get; set; }
        public AuditLog _auditLog { get; set; }


        public AuditEntry(DbEntityEntry entry, string userId, string Ip)
        {
            _auditLog = new AuditLog();
            _auditLog.EventDateUTC = DateTime.UtcNow;
            _auditLog.UserId = userId;
            _auditLog.IP = Ip;

            dbEntry = entry;
            SetChanges();
        }

        private void SetChanges()
        {
            // Get the Table() attribute, if one exists
            TableAttribute tableAttr = dbEntry.Entity.GetType().GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() as TableAttribute;
            // Get table name (if it has a Table attribute, use that, otherwise get the pluralized name)
            string tableName = tableAttr != null ? tableAttr.Name : dbEntry.Entity.GetType().Name;

            // Get primary key value (If you have more than one key column, this will need to be adjusted)
            if (dbEntry != null)
            {
                string keyName = dbEntry.Entity.GetType().GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0).Name;

                _auditLog.AuditLogId = Guid.NewGuid();
                _auditLog.EventDateUTC = DateTime.UtcNow;
                _auditLog.TableName = tableName;
               



                switch (dbEntry.State)
                {
                    case EntityState.Added:
                        _auditLog.EventType = "A"; // Added
                        _auditLog.ColumnName = "*ALL";
                        _auditLog.RecordId = dbEntry.CurrentValues.GetValue<object>(keyName).ToString();
                        _auditLog.NewValue = dbEntry.CurrentValues.ToObject().ToString();
                        break;

                    case EntityState.Deleted:
                        _auditLog.EventType = "D"; // Deleted
                        _auditLog.ColumnName = "*ALL";
                        _auditLog.RecordId = dbEntry.OriginalValues.GetValue<object>(keyName).ToString();
                        _auditLog.NewValue = dbEntry.OriginalValues.ToObject().ToString();
                        break;

                    case EntityState.Modified:
                        foreach (string propertyName in dbEntry.OriginalValues.PropertyNames)
                        {
                            //if (!object.Equals(dbEntry.OriginalValues.GetValue<object>(propertyName), dbEntry.CurrentValues.GetValue<object>(propertyName)))
                            //{
                                _auditLog.EventType = "M"; // Modified
                                _auditLog.ColumnName = "*ALL";//Not Implemented yet
                                _auditLog.RecordId = dbEntry.OriginalValues.GetValue<object>(keyName).ToString();
                                _auditLog.NewValue = dbEntry.CurrentValues.ToObject().ToString();
                            //}
                        }
                        break;
                }
            }
           
        }

    }
 
}
