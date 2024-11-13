using EntityFramework.Exceptions.SqlServer;
using KafApp.Models;
using KafApp.Config;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Reflection.Emit;

namespace KafApp.Repo.Server
{

  public  class MyDbContext : DbContext
    {






        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ////optionsBuilder.UseSqlServer("Server=DESKTOP-CATPQG7\\SQLEXPRESS;Database=myDaaBase;Trusted_Connection=True;TrustServerCertificate=True;");

            optionsBuilder.UseSqlServer(AppSettings.ConnectionString)
                .UseExceptionProcessor();
           // optionsBuilder.UseSqlServer("Server=DESKTOP-CATPQG7\\SQLEXPRESS;Database=AtlasStandAlone1000;Trusted_Connection=True;TrustServerCertificate=True;");


        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("seketo");
            //modelBuilder.HasDefaultSchema(Schema);
            managemodule(modelBuilder);
            crmmodule(modelBuilder);
            financialmodule(modelBuilder);
            Inventorymodule(modelBuilder);
            invoicesmodule(modelBuilder);
            test(modelBuilder);
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
            modelBuilder.Entity<User>().HasIndex(p => p.UserName).IsUnique();
            modelBuilder.Entity<User>().Navigation(p => p.Group).AutoInclude();

            //RELATION
            modelBuilder.Entity<Usersgroup>().HasMany<User>(g => g.Userslist).WithOne(p => p.Group).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>().HasMany<Note>(g => g.Noteslist).WithOne(p => p.User).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>().HasMany<Session>(g => g.SessionsList).WithOne(p => p.User).OnDelete(DeleteBehavior.Restrict);

        }




        #endregion



        #region CRM MODULE
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }


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
        public DbSet<ProfitDistribute> ProfitDistributes { get; set; }


        void financialmodule(ModelBuilder modelBuilder)
        {
            //UNIQUE
            modelBuilder.Entity<PaymentOption>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Partner>().HasIndex(p => p.Name).IsUnique();

            //RELATION
            modelBuilder.Entity<Partner>().HasMany<ProfitDistribute>(g => g.ProfitDistributeList).WithOne(p => p.Partner).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FinancialCycle>().HasMany<ProfitDistribute>(g => g.ProfitDistributeList).WithOne(p => p.FinancialCycle).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FinancialCycle>().HasMany<Session>(g => g.SessionsList).WithOne(p => p.FinancialCycle).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FinancialCycle>().HasQueryFilter(p => p.Id > 1);



            modelBuilder.Entity<ProfitDistribute>().Navigation(k => k.Session).AutoInclude();
            modelBuilder.Entity<ProfitDistribute>().Navigation(k => k.Partner).AutoInclude();
            modelBuilder.Entity<ProfitDistribute>().Navigation(k => k.FinancialCycle).AutoInclude();
        }
        #endregion



        #region HR
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<EmployeeDeduction> EmployeeDeductions { get; set; }
        void HRmodule(ModelBuilder modelBuilder)
        {
            //UNIQUE
            modelBuilder.Entity<Employee>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Role>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Employee>().Navigation(p => p.Role).AutoInclude();

            modelBuilder.Entity<EmployeeDeduction>().Navigation(k=>k.Session).AutoInclude();
            modelBuilder.Entity<Role>().HasMany(g => g.EmployeesList).WithOne(p => p.Role).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Employee>().HasMany(g => g.DeductionsList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Restrict);

        }

        #endregion




        #region inventory MODULE
        public DbSet<ProductInStore> ProductInStores { get; set; }
        public DbSet<ProductUnit> Productsunits { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<PosGroup> PosGroups { get; set; }


        void Inventorymodule(ModelBuilder modelBuilder)
        {
            //UNIQUE

            modelBuilder.Entity<ProductUnit>().Navigation(p => p.Product).AutoInclude();
            modelBuilder.Entity<ProductUnit>().Navigation(p => p.Product).AutoInclude();
            modelBuilder.Entity<Product>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<ProductInStore>().HasIndex(p => new { p.ProductId, p.StoreId }).IsUnique();
            modelBuilder.Entity<Category>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Store>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<PosGroup>().HasIndex(p => p.Name).IsUnique();

            //RELATION
            modelBuilder.Entity<PosGroup>().HasMany<ProductUnit>(g => g.ProductUnitList).WithOne(p => p.PosGroup).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Category>().HasMany<Product>(g => g.ProductsList).WithOne(p => p.Category).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Product>().HasMany<ProductInStore>(g => g.ProductInStoreList).WithOne(p => p.Product).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Store>().HasMany<ProductInStore>(g => g.ProductInStoreList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Product>().HasMany(g => g.ProductUnitList).WithOne(p => p.Product).OnDelete(DeleteBehavior.Cascade);

        }

        #endregion













        #region Invoices
        public DbSet<Supply> Supplys { get; set; }
        public DbSet<SupplyItem> SupplyItems { get; set; }
        public DbSet<SupplyBonus> SupplyBonus { get; set; }
        public DbSet<SupplierReturn> SupplierReturns { get; set; }
        public DbSet<SupplierReturnItem> SupplierReturnItems { get; set; }


        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<CustomerOrderItem> CustomerOrderItems { get; set; }
        public DbSet<CustomerOrderBonus> CustomerOrderBonus { get; set; }
        public DbSet<CustomerReturn> CustomerReturns { get; set; }
        public DbSet<CustomerReturnItem> CustomerReturnItems { get; set; }


        public DbSet<CashOrder> CashOrders { get; set; }
        public DbSet<CashOrderItem> CashOrderItems { get; set; }
        public DbSet<CashOrderBonus> CashOrderBonus { get; set; }
        public DbSet<GenericReturn> GenericReturns { get; set; }
        public DbSet<GenericReturnItem> GenericReturnItems { get; set; }











        void invoicesmodule(ModelBuilder modelBuilder)
        {
            //UNIQUE
            modelBuilder.Entity<SupplyItem>().HasIndex(p => new { p.ParentId,p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<SupplyBonus>().HasIndex(p => new { p.ParentId,p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<SupplierReturnItem>().HasIndex(p => new { p.ParentId,p.ProductUnitId }).IsUnique();

            modelBuilder.Entity<CustomerOrderItem>().HasIndex(p => new { p.ParentId,p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<CustomerOrderBonus>().HasIndex(p => new { p.ParentId,p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<CustomerReturnItem>().HasIndex(p => new { p.ParentId,p.ProductUnitId }).IsUnique();

            modelBuilder.Entity<CashOrderItem>().HasIndex(p => new { p.ParentId,p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<CashOrderBonus>().HasIndex(p => new { p.ParentId,p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<GenericReturnItem>().HasIndex(p => new { p.ParentId, p.ProductUnitId }).IsUnique();

            modelBuilder.Entity<SupplyItem>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<CustomerOrderItem>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<CustomerReturnItem>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<SupplierReturnItem>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<CashOrderItem>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<SupplyBonus>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<CashOrderBonus>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<CustomerOrderBonus>().Navigation(p => p.ProductUnit).AutoInclude();































            //RELATION


            #region Supply

            modelBuilder.Entity<Supplier>().HasMany(g => g.SupplysList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Supplier>().HasMany(g => g.SupplierReturnsList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Supply>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<Supply>().Navigation(g => g.Parent).AutoInclude();
            modelBuilder.Entity<Supply>().HasMany(g => g.ItemsList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Supply>().HasMany(g => g.BonusList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<SupplierReturn>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<SupplierReturn>().Navigation(g => g.Parent).AutoInclude();

            modelBuilder.Entity<SupplierReturn>().HasMany(g => g.ItemsList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<SupplyItem>().HasMany(g => g.ReturnList).WithOne(p => p.SupplyItem).OnDelete(DeleteBehavior.Restrict);

            #endregion


            #region CustomerOrder

            modelBuilder.Entity<Customer>().HasMany(g => g.CustomerOrdersList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Customer>().HasMany(g => g.CustomerReturnsList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerOrder>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<CustomerOrder>().Navigation(g => g.Parent).AutoInclude();
            modelBuilder.Entity<CustomerOrder>().HasMany(g => g.ItemsList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CustomerOrder>().HasMany(g => g.BonusList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<CustomerReturn>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<CustomerReturn>().Navigation(g => g.Parent).AutoInclude();
            modelBuilder.Entity<CustomerReturn>().HasMany(g => g.ItemsList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Cascade);



            modelBuilder.Entity<CustomerOrderItem>().HasMany(g => g.ReturnList).WithOne(p => p.CustomerOrderItem).OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region CashOrder

            modelBuilder.Entity<CashOrder>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<CashOrder>().HasMany(g => g.ItemsList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CashOrder>().HasMany(g => g.BonusList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<GenericReturn>().Navigation(g => g.Session).AutoInclude();

            modelBuilder.Entity<GenericReturn>().HasMany(g => g.ItemsList).WithOne(p => p.Parent).OnDelete(DeleteBehavior.Cascade);

            #endregion










            #region Product


            modelBuilder.Entity<Product>().HasMany(g => g.ProductInStoreList).WithOne(p => p.Product).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Product>().HasMany(g => g.ProductUnitList).WithOne(p => p.Product).OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region ProductUnit

            modelBuilder.Entity<ProductUnit>().Navigation(g => g.Product).AutoInclude();
            modelBuilder.Entity<ProductUnit>().HasMany(g => g.SupplyItemList).WithOne(p => p.ProductUnit).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductUnit>().HasMany(g => g.SupplyBonusList).WithOne(p => p.ProductUnit).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductUnit>().HasMany(g => g.SupplierReturnItemList).WithOne(p => p.ProductUnit).OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<ProductUnit>().HasMany(g => g.CustomerOrderItemList).WithOne(p => p.ProductUnit).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductUnit>().HasMany(g => g.CustomerOrderBonusList).WithOne(p => p.ProductUnit).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductUnit>().HasMany(g => g.CustomerReturnItemList).WithOne(p => p.ProductUnit).OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<ProductUnit>().HasMany(g => g.CashOrderItemList).WithOne(p => p.ProductUnit).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductUnit>().HasMany(g => g.CashOrderBonusList).WithOne(p => p.ProductUnit).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductUnit>().HasMany(g => g.GenericReturnItemList).WithOne(p => p.ProductUnit).OnDelete(DeleteBehavior.Restrict);
            #endregion




            #region Store

            modelBuilder.Entity<Store>().HasMany(g => g.SupplyList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Store>().HasMany(g => g.SupplierReturnList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Store>().HasMany(g => g.CustomerOrderList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Store>().HasMany(g => g.CustomerReturnList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Store>().HasMany(g => g.CashOrderList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Store>().HasMany(g => g.GenericReturnList).WithOne(p => p.Store).OnDelete(DeleteBehavior.Restrict);







            #endregion





            #region Session





            modelBuilder.Entity<Session>().HasMany(g => g.SupplyList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany(g => g.SupplierReturnList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany(g => g.CustomerOrderList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany(g => g.SupplierReturnList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany(g => g.CashOrderList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany(g => g.GenericReturnList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);




            modelBuilder.Entity<Session>().HasMany(g => g.InFlowList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Session>().HasMany(g => g.OutFlowList).WithOne(p => p.Session).OnDelete(DeleteBehavior.Restrict);

            #endregion





            #region PaymentOption



            modelBuilder.Entity<PaymentOption>().HasMany(g => g.OutFlowlist).WithOne(p => p.Payment).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PaymentOption>().HasMany(g => g.InFlowlist).WithOne(p => p.Payment).OnDelete(DeleteBehavior.Restrict);



            #endregion





        }


        #endregion






        public DbSet<OutFlow> OutFlows { get; set; }
        public DbSet<InFlow> InFlows { get; set; }

        private void test(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InFlow>().Navigation(g => g.Session).AutoInclude();

            modelBuilder.Entity<InFlow>()
             .HasDiscriminator<string>(k=>k.FlowType)  
             .HasValue<CapitalDeposit>("CapitalDeposit")
             .HasValue<CustomerOrderPayment>("CustomerOrderPayment")
             .HasValue<SupplierReturnPayment>("SupplierReturn")
             .HasValue<CashOrderPayment>("CashOrder")
             .HasValue<CustomerPayment>("CustomerPayment");





            modelBuilder.Entity<Partner>()
                .HasMany(g => g.CapitalDepositList)
                .WithOne(p => p.Partner)
                .HasForeignKey(k => k.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CustomerOrder>()
                .HasMany(g => g.PaymentsList)
                .WithOne(p => p.CustomerOrder)
                .HasForeignKey(k => k.CustomerOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CashOrder>()
                .HasMany(g => g.PaymentsList)
                .WithOne(p => p.CashOrder)
                .HasForeignKey(k => k.CashOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupplierReturn>()
                .HasMany(g => g.PaymentsList)
                .WithOne(p => p.SupplierReturn)
                .HasForeignKey(k => k.SupplierReturnId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Customer>()
                .HasMany(g => g.CustomerPaymentsList)
                .WithOne(p => p.Customer)
                .HasForeignKey(k => k.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);











            modelBuilder.Entity<OutFlow>().Navigation(g => g.Session).AutoInclude();

            modelBuilder.Entity<OutFlow>()
             .HasDiscriminator<string>(k => k.FlowType)  
             .HasValue<Expence>("GenericExpence") 
             .HasValue<CashOrderExpence>("CashOrdersExpence") 
             .HasValue<CustomerOrderExpence>("CustomerOrdersExpence") 
             .HasValue<CustomerReturnExpence>("CustomerReturnsExpence") 
             .HasValue<SupplierReturnExpence>("SupplierReturnsExpence") 
             .HasValue<SupplyExpence>("SupplysExpence") 
             .HasValue<EmployeeIncentive>("Incentive") 
             .HasValue<SupplyPayment>("SupplyPayment") 
             .HasValue<CapitalWithdrawal>("CapitalWithdrawal")  
             .HasValue<SupplierPayment>("SupplierPayment")  
             .HasValue<EmployeeAdvancePayment>("AdvancePayment")
             .HasValue<Employ