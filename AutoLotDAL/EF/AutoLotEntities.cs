using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using AutoLotDAL.Interception;
using AutoLotDAL.Models;

namespace AutoLotDAL.EF
{
    public partial class AutoLotEntities : DbContext
    {
        //This is the built-in logging interceptor

        public AutoLotEntities() : base("name=AutoLotConnection")
        {
            //Interceptor code
            var context = (this as IObjectContextAdapter).ObjectContext;
            context.ObjectMaterialized += OnObjectMaterialized;
            context.SavingChanges += OnSavingChanges;
        }

        #region Interceptor loggers (simple and custom)
        //static readonly DatabaseLogger DatabaseLogger = new DatabaseLogger("sqllog.txt", true); //true indicates whether the log should be appended to (default is false)
        
        //public AutoLotEntities() : base("name=AutoLotConnection")
        //{
        //    //DbInterception.Add(new ConsoleWriterInterceptor());  //This registers the Interceptor; registering them in code isolates them from changes to the configuration file
        //    DatabaseLogger.StartLogging(); //This uses the built-in logging interceptor for simple logging
        //    DbInterception.Add(DatabaseLogger); //This uses the built-in logging interceptor for simple logging
        //} 

        //This is necessary for the built-in interceptor logger
        //protected override void Dispose(bool disposing)
        //{
        //    DbInterception.Remove(DatabaseLogger);
        //    DatabaseLogger.StopLogging();
        //    base.Dispose(disposing);
        //}
        #endregion

        public virtual DbSet<CreditRisk> CreditRisks { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Inventory> Inventory { get; set; }
        public virtual DbSet<Order> Orders { get; set; }

        //fires just after SaveChanges() is called but before DB is updated
        private void OnSavingChanges(object sender, EventArgs eventArgs)
        {
            //Sender is of type ObjectContext. Can get current and original values, and cancel/modify the save operation as desired.
            var context = sender as ObjectContext;
            if (context == null) return;

            foreach (ObjectStateEntry item in context.ObjectStateManager.GetObjectStateEntries(EntityState.Modified | EntityState.Added))
            {
                //Do something important here
                if ((item.Entity as Inventory) != null)
                {
                    var entity = (Inventory)item.Entity;
                    if (entity.Color == "Red")
                    {
                        item.RejectPropertyChanges(nameof(entity.Color));
                    }
                }
            }
        }

        //provides access to entities being reconstituted; fired just after model's props are populated but just before context serves it up
        private void OnObjectMaterialized(object sender, System.Data.Entity.Core.Objects.ObjectMaterializedEventArgs e)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inventory>()
                .HasMany(e => e.Orders)
                .WithRequired(e => e.Car)
                .WillCascadeOnDelete(false);
        }


    }
}
