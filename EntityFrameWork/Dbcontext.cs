
using EntityFramework.Exceptions.SqlServer;
using KafApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Reflection.Emit;

namespace KafApp.Repo
{

    class MyDbContext: DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ////optionsBuilder.UseSqlServer("Server=DESKTOP-CATPQG7\\SQLEXPRESS;Database=myDaaBase;Trusted_Connection=True;TrustServerCertificate=True;");

            //optionsBuilder.UseSqlServer(conn);
            optionsBuilder.UseSqlServer("Server=DESKTOP-CATPQG7\\SQLEXPRESS;Database=AtlasStandAlone1000;Trusted_Connection=True;TrustServerCertificate=True;");
            optionsBuilder.UseExceptionProcessor();
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("seketo");
            managemodule(modelBuilder);
            crmmodule(modelBuilder);
            financialmodule(modelBuilder);
            Inventorymodule(modelBuilder);
            invoicesmodule(modelBuilder);

        }




        #region MANAGE MODULE

        public DbSet<Session> Sessions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Usersgroup> UsersGroups { get; set; }
        public DbSet<Note> Notes { get; set; }

        

        void managemodule(ModelBuilder modelBuilder)
        {
            //UNIQUE
          
            modelBuilder.Entity<Usersgroup>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<User>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<User>().HasIndex(p => p.Alias).IsUnique();

            //RELATION
            modelBuilder.Entity<Usersgroup>().HasMany<User>(g => g.Userslist).WithOne(p => p.Group).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>().HasMany<Note>(g => g.Noteslist).WithOne(p => p.User).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>().HasMany<Session>(g => g.SessionsList).WithOne(p => p.User).OnDelete(DeleteBehavior.Restrict);

        }




        #endregion



        #region CRM MODULE
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> suppliers { get; set; }


        void crmmodule(ModelBuilder modelBuilder)
        {
            //UNIQUE
            modelBuilder.Entity<Customer>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Supplier>().HasIndex(p => p.Name).IsUnique();


        }

        #endregion







        #region financial MODULE
        public DbSet<FinancialCycle> FinancialCycles { get; set; }
        public DbSet<Balance> Balance { get; set; }
        public DbSet<PaymentOption> Payments { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<ProfitsDivision> ProfitsDivisions { get; set; }
       

        void financialmodule(ModelBuilder modelBuilder)
        {
            //UNIQUE
            modelBuilder.Entity<PaymentOption>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Partner>().HasIndex(p => p.Name).IsUnique();

            //RELATION
            modelBuilder.Entity<Partner>().HasMany<ProfitsDivision>(g => g.ProfitsDivisionList).WithOne(p => p.Partner).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FinancialCycle>().HasMany<ProfitsDivision>(g => g.ProfitsDivisionList).WithOne(p => p.FinancialCycle).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>().HasMany<FinancialCycle>(g => g.FinancialCyclesList).WithOne(p => p.User).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FinancialCycle>().HasMany<Session>(g => g.SessionsList).WithOne(p => p.FinancialCycle).OnDelete(DeleteBehavior.Restrict);

        }
        #endregion








        #region inventory MODULE
        public DbSet<ProductInStore> ProductInStores { get; set; }
        public DbSet<ProductUnit> Productsunits { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Unit> Units { get; set; }

        void Inventorymodule(ModelBuilder modelBuilder)
        {
            //UNIQUE
            modelBuilder.Entity<Product>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<ProductUnit>().HasIndex(p => new { p.UnitId, p.ProductId }).IsUnique();
            modelBuilder.Entity<ProductInStore>().HasIndex(p => new { p.ProductId, p.StoreId }).IsUnique();
            modelBuilder.Entity<Category>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Store>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Unit>().HasIndex(p => p.Name).IsUnique();

            //RELATION
            modelBuilder.Entity<Category>().HasMany<Product>(g => g.ProductsList).WithOne(p => p.Category).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasMany<ProductInStore>(g => g.ProductInStoreList).WithOne(p => p.Product).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Store>().HasMany<ProductInStore>(g => g.ProductInStoreList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Unit>().HasMany(g => g.ProductUnitsList).WithOne(pu => pu.Unit).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Product>().HasMany(g => g.ProductUnitList).WithOne(p => p.Product).OnDelete(DeleteBehavior.Restrict);

        }

        #endregion













        #region Invoices
        public DbSet<Supply> Supplys { get; set; }
        public DbSet<SupplyItem> SupplyItems { get; set; }
        public DbSet<SupplyPayment> supplyPayments { get; set; }
        public DbSet<SupplierPayment> supplierPayments { get; set; }
        public DbSet<DeletedSupply> DeletedSupplys { get; set; }
        public DbSet<DeletedSupplyItem> DeletedSupplyItems { get; set; }

        public DbSet<SupplierReturn> SupplierReturns { get; set; }
        public DbSet<SupplierReturnItem> SupplierReturnItems { get; set; }

        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<CustomerOrderItem> CustomerOrderItems { get; set; }
        public DbSet<CustomerOrderPayment> CustomerOrderPayments { get; set; }
        public DbSet<CustomerPayment> CustomerPayments { get; set; }
        public DbSet<DeletedCustomerOrder> DeletedCustomerOrders { get; set; }
        public DbSet<DeletedCustomerOrderItem> DeletedCustomerOrderItems { get; set; }

        public DbSet<CustomerReturn> CustomerReturns { get; set; }
        public DbSet<CustomerReturnItem> CustomerReturnItems { get; set; }

        public DbSet<CashOrder> CashOrders { get; set; }
        public DbSet<CashOrderItem> CashOrderItems { get; set; }
        public DbSet<GenericReturn> GenericReturns { get; set; }
        public DbSet<GenericReturnItem> GenericReturnItems { get; set; }

        public DbSet<CapitalWithdrawal> CapitalWithdrawals { get; set; }
        public DbSet<CapitalDeposit> CapitalDeposits { get; set; }
        
        
        
        
        public DbSet<Expence> Expences { get; set; }


        void invoicesmodule(ModelBuilder modelBuilder)
        {
            //UNIQUE

            //RELATION
            modelBuilder.Entity<CashOrder>().HasMany<CashOrderItem>(g => g.ItemsList).WithOne(p => p.CashOrder).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<GenericReturn>().HasMany<GenericReturnItem>(g => g.ItemsList).WithOne(p => p.GenericReturn).OnDelete(DeleteBehavior.Restrict);

            
            
            modelBuilder.Entity<Customer>().HasMany<CustomerOrder>(g => g.CustomerOrdersList).WithOne(p => p.Customer).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Customer>().HasMany<DeletedCustomerOrder>(g => g.DeletedCustomerOrdersList).WithOne(p => p.Customer).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Customer>().HasMany<CustomerReturn>(g => g.CustomerReturnsList).WithOne(p => p.Customer).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Customer>().HasMany<CustomerPayment>(g => g.CustomerPaymentsList).WithOne(p => p.Customer).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CustomerOrder>().HasMany<CustomerOrderItem>(g => g.ItemsList).WithOne(p => p.CustomerOrder).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DeletedCustomerOrder>().HasMany<DeletedCustomerOrderItem>(g => g.ItemsList).WithOne(p => p.DeletedCustomerOrder).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CustomerOrder>().HasMany<CustomerOrderPayment>(g => g.PaymentsList).WithOne(p => p.CustomerOrder).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CustomerReturn>().HasMany<CustomerReturnItem>(g => g.ItemsList).WithOne(p => p.CustomerReturn).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>().HasMany<SupplyItem>(g => g.SupplyItemList).WithOne(p => p.Product).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasMany<CustomerReturnItem>(g => g.CustomerReturnItemList).WithOne(p => p.Product).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasMany<CustomerOrderItem>(g => g.CustomerOrderItemList).WithOne(p => p.Product).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasMany<GenericReturnItem>(g => g.GenericReturnItemList).WithOne(p => p.Product).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasMany<SupplierReturnItem>(g => g.SupplierReturnItemList).WithOne(p => p.Product).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasMany<CashOrderItem>(g => g.CashOrderItemList).WithOne(p => p.Product).OnDelete(DeleteBehavior.Restrict);
           
            modelBuilder.Entity<Unit>().HasMany(g => g.SupplyItemList).WithOne(p => p.Unit).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Unit>().HasMany(g => g.CustomerReturnItemList).WithOne(p => p.Unit).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Unit>().HasMany(g => g.DeletedCustomerOrderItemsList).WithOne(p => p.Unit).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Unit>().HasMany(g => g.DeletedSupplyItemsList).WithOne(p => p.Unit).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Unit>().HasMany(g => g.GenericReturnItemList).WithOne(p => p.Unit).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Unit>().HasMany(g => g.SupplierReturnItemList).WithOne(p => p.Unit).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Unit>().HasMany(g => g.CashOrderItemList).WithOne(p => p.Unit).OnDelete(DeleteBehavior.Restrict);
           
            modelBuilder.Entity<Store>().HasMany<SupplyItem>(g => g.SupplyItemList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Store>().HasMany<CustomerReturnItem>(g => g.CustomerReturnItemList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Store>().HasMany<CustomerOrderItem>(g => g.CustomerOrderItemList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Store>().HasMany<GenericReturnItem>(g => g.GenericReturnItemList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Store>().HasMany<SupplierReturnItem>(g => g.SupplierReturnItemList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Store>().HasMany<CashOrderItem>(g => g.CashOrderItemList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Supplier>().HasMany<Supply>(g => g.SupplysList).WithOne(p => p.Supplier).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Supplier>().HasMany<DeletedSupply>(g => g.DeletedSupplysList).WithOne(p => p.Supplier).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Supplier>().HasMany<SupplierReturn>(g => g.SupplierReturnsList).WithOne(p => p.Supplier).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Supplier>().HasMany<SupplierPayment>(g => g.SupplierPaymentsList).WithOne(p => p.Supplier).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Supply>().HasMany<SupplyItem>(g => g.ItemsList).WithOne(p => p.Supply).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DeletedSupply>().HasMany<DeletedSupplyItem>(g => g.ItemsList).WithOne(p => p.DeletedSupply).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Supply>().HasMany<SupplyPayment>(g => g.PaymentsList).WithOne(p => p.Supply).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SupplierReturn>().HasMany<SupplierReturnItem>(g => g.ItemsList).WithOne(p => p.SupplierReturn).OnDelete(DeleteBehavior.Cascade);



            modelBuilder.Entity<Session>().HasMany<Supply>(g => g.SupplyList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<DeletedSupply>(g => g.DeletedSupplyList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<SupplyPayment>(g => g.SupplyPaymentList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<SupplierPayment>(g => g.SupplierPaymentList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<SupplierReturn>(g => g.SupplierReturnList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<CustomerOrder>(g => g.CustomerOrderList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<DeletedCustomerOrder>(g => g.DeletedCustomerOrderList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<CustomerPayment>(g => g.CustomerPaymentsList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<CustomerReturn>(g => g.CustomerReturnsList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<CustomerOrderPayment>(g => g.CustomerOrderPaymentsList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<CapitalDeposit>(g => g.CapitalDepositList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<CapitalWithdrawal>(g => g.CapitalWithdrawalList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<CashOrder>(g => g.CashOrdersList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<GenericReturn>(g => g.GenericReturnsList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany<Expence>(g => g.ExpencesList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);





            modelBuilder.Entity<PaymentOption>().HasMany<SupplyPayment>(g => g.supplyPaymentslist).WithOne(p => p.Payment).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PaymentOption>().HasMany<SupplierPayment>(g => g.supplierPaymentslist).WithOne(p => p.Payment).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PaymentOption>().HasMany<CapitalDeposit>(g => g.CapitalDepositList).WithOne(p => p.Payment).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PaymentOption>().HasMany<CapitalWithdrawal>(g => g.CapitalWithdrawalList).WithOne(p => p.Payment).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PaymentOption>().HasMany<CashOrder>(g => g.CashOrderslist).WithOne(p => p.Payment).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PaymentOption>().HasMany<GenericReturn>(g => g.GenericReturnslist).WithOne(p => p.Payment).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PaymentOption>().HasMany<SupplierReturn>(g => g.SupplierReturnslist).WithOne(p => p.Payment).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PaymentOption>().HasMany<CustomerReturn>(g => g.CustomerReturnslist).WithOne(p => p.Payment).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PaymentOption>().HasMany<Expence>(g => g.Expenceslist).WithOne(p => p.Payment).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PaymentOption>().HasMany<CapitalDeposit>(g => g.CapitalDepositList).WithOne(p => p.Payment).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Partner>().HasMany<CapitalWithdrawal>(g => g.CapitalWithdrawalList).WithOne(p => p.Partner).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Partner>().HasMany<CapitalDeposit>(g => g.CapitalDepositList).WithOne(p => p.Partner).OnDelete(DeleteBehavior.Restrict);
        }


        #endregion

    }

}
