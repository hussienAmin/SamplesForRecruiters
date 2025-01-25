
using EntityFramework.Exceptions.SqlServer;
using KafApp.Models;

using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Reflection.Emit;

namespace KafApp.Repo
{

    public class MyDbContext : DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer("Server=DESKTOP-CATPQG7\\SQLEXPRESS;Database=myDaaBase;Trusted_Connection=True;TrustServerCertificate=True;");

            //optionsBuilder.UseSqlServer(AppSettings.ConnectionString)
            optionsBuilder.UseSqlServer("Server=DESKTOP-CATPQG7\\SQLEXPRESS;Database=AtlabbbbbbsStandAlone1000;User ID=seketo;Password=112233445566;TrustServerCertificate=True;")
                  .UseExceptionProcessor();


        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("seketo");

            #region Core
            modelBuilder.Entity<FinancialCycle>().HasIndex(p => p.Name).IsUnique();

            modelBuilder.Entity<Usersgroup>().HasIndex(p => p.Name).IsUnique();


            modelBuilder.Entity<User>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<User>().HasIndex(p => p.UserName).IsUnique();
            modelBuilder.Entity<User>().Navigation(p => p.Group).AutoInclude();
            modelBuilder.Entity<User>()
                .HasOne(p => p.Group)
                .WithMany(g => g.Userslist)
                .HasForeignKey(k => k.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Session>()
                .HasOne(p => p.User)
                .WithMany(g => g.SessionsList)
                .HasForeignKey(k => k.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Session>()
                .HasOne(p => p.FinancialCycle)
                .WithMany(g => g.SessionsList)
                .HasForeignKey(k => k.FinancialCycleId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion



            #region Stock
            modelBuilder.Entity<Store>().HasIndex(p => p.Name).IsUnique();


            modelBuilder.Entity<Category>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Product>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(g => g.ProductsList)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductUnit>().HasIndex(p => new { p.Name, p.ProductId }).IsUnique();
            modelBuilder.Entity<ProductUnit>().Navigation(g => g.Product).AutoInclude();
            modelBuilder.Entity<ProductUnit>()
                .HasOne(p => p.Product)
                .WithMany(g => g.ProductUnitList)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<ProductInStore>().HasIndex(p => new { p.ProductId, p.StoreId }).IsUnique();
            modelBuilder.Entity<ProductInStore>().Navigation(p => p.Product).AutoInclude();
            modelBuilder.Entity<ProductInStore>().Navigation(p => p.Store).AutoInclude();
            modelBuilder.Entity<ProductInStore>()
                .HasOne(p => p.Product)
                .WithMany(g => g.ProductInStoreList)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProductInStore>()
                .HasOne(p => p.Store)
                .WithMany(g => g.ProductInStoreList)
                .HasForeignKey(p => p.StoreId)
                .OnDelete(DeleteBehavior.Cascade);





            ////////////////////////////// Stocktaking///////////////////////////////

            modelBuilder.Entity<Stocktaking>().Navigation(p => p.Store).AutoInclude();
            modelBuilder.Entity<Stocktaking>().Navigation(p => p.Session).AutoInclude();
            modelBuilder.Entity<Stocktaking>().Navigation(p => p.StockReconciliations).AutoInclude();
            modelBuilder.Entity<Stocktaking>().Navigation(p => p.StocktakingItems).AutoInclude();
            modelBuilder.Entity<Stocktaking>()
                .HasOne(p => p.Store)
                .WithMany(g => g.StocktakingList)
                .HasForeignKey(k => k.StoreId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Stocktaking>()
                .HasOne(p => p.Session)
                .WithMany(g => g.StocktakingList)
                .HasForeignKey(k => k.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StocktakingItem>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<StocktakingItem>()
                .HasOne(p => p.Stocktaking)
                .WithMany(g => g.StocktakingItems)
                .HasForeignKey(k => k.StocktakingId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<StocktakingItem>()
                .HasOne(p => p.ProductUnit)
                .WithMany(p => p.StocktakingItemList)
                .HasForeignKey(p => p.UnitId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<StockReconciliation>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<StockReconciliation>()
                .HasOne(p => p.Stocktaking)
                .WithMany(g => g.StockReconciliations)
                .HasForeignKey(k => k.StocktakingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockReconciliation>()
                .HasOne(p => p.ProductUnit)
                .WithMany(p => p.StockReconciliationList)
                .HasForeignKey(p => p.UnitId)
                .OnDelete(DeleteBehavior.Restrict);


            ///////////////////////////////////////  StockTransfer    ///////////////////////////////////////////

            //modelBuilder.Entity<StockTransfer>().Navigation(p => p.From).AutoInclude();
            //modelBuilder.Entity<StockTransfer>().Navigation(p => p.To).AutoInclude();
            modelBuilder.Entity<StockTransfer>().Navigation(p => p.Session).AutoInclude();
            modelBuilder.Entity<StockTransfer>()
                .HasOne(p => p.From)
                .WithMany(p => p.StockTranferFromList)
                .HasForeignKey(p => p.FromId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StockTransfer>()
                .HasOne(p => p.To)
                .WithMany(p => p.StockTranferToList)
                .HasForeignKey(p => p.ToId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockTransfer>()
                .HasOne(p => p.Session)
                .WithMany(p => p.StockTransferList)
                .HasForeignKey(p => p.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<StockTransfer>()
                .HasOne(p => p.ProductUnit)
                .WithMany(p => p.StockTransferList)
                .HasForeignKey(p => p.UnitId)
                .OnDelete(DeleteBehavior.Restrict);


            #endregion









            #region Payment


            modelBuilder.Entity<PaymentOption>().HasIndex(p => p.Name).IsUnique();

















            modelBuilder.Entity<Expence>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<Expence>()
                .HasOne(g => g.Session)
                .WithMany(g => g.ExpenceList)
                .HasForeignKey(g => g.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Expence>()
                .HasOne(g => g.Payment)
                .WithMany(g => g.Expencelist)
                .HasForeignKey(g => g.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);







            #endregion




            #region Supply

            modelBuilder.Entity<Supplier>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<SupplierPayment>().Navigation(g => g.Supplier).AutoInclude();
            modelBuilder.Entity<SupplierPayment>()
               .HasOne(p => p.Supplier)
               .WithMany(g => g.SupplierPaymentsList)
               .HasForeignKey(k => k.SupplierId)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupplierPayment>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<SupplierPayment>()
                .HasOne(g => g.Session)
                .WithMany(g => g.SupplierPaymentList)
                .HasForeignKey(g => g.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupplierPayment>()
                .HasOne(g => g.Payment)
                .WithMany(g => g.SupplierPaymentlist)
                .HasForeignKey(g => g.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Supply>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<Supply>().Navigation(g => g.Parent).AutoInclude();
            modelBuilder.Entity<Supply>().Navigation(g => g.Store).AutoInclude();
            modelBuilder.Entity<Supply>()
                .HasOne(p => p.Session)
                .WithMany(g => g.SupplyList)
                .HasForeignKey(k => k.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Supply>()
                .HasOne(p => p.Parent)
                .WithMany(g => g.SupplysList)
                .HasForeignKey(k => k.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Supply>()
                .HasOne(p => p.Store)
                .WithMany(g => g.SupplyList)
                .HasForeignKey(k => k.StoreId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<SupplyItem>().HasIndex(p => new { p.ParentId, p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<SupplyItem>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<SupplyItem>()
                .HasOne(p => p.Parent)
                .WithMany(g => g.ItemsList)
                .HasForeignKey(k => k.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupplyItem>()
                .HasOne(p => p.ProductUnit)
                .WithMany(g => g.SupplyItemList)
                .HasForeignKey(k => k.ProductUnitId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<SupplyBonus>().HasIndex(p => new { p.ParentId, p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<SupplyBonus>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<SupplyBonus>()
                .HasOne(p => p.Parent)
                .WithMany(g => g.BonusList)
                .HasForeignKey(k => k.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupplyBonus>()
                .HasOne(p => p.ProductUnit)
                .WithMany(g => g.SupplyBonusList)
                .HasForeignKey(k => k.ProductUnitId)
                .OnDelete(DeleteBehavior.Restrict);







            modelBuilder.Entity<SupplyPayment>()
                .HasOne(p => p.Supply)
                .WithMany(g => g.PaymentsList)
                .HasForeignKey(k => k.SupplyId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SupplyPayment>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<SupplyPayment>()
                .HasOne(g => g.Session)
                .WithMany(g => g.SupplyPaymentList)
                .HasForeignKey(g => g.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupplyPayment>()
                .HasOne(g => g.Payment)
                .WithMany(g => g.SupplyPaymentlist)
                .HasForeignKey(g => g.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);


            ///////////////////////////////////////  SupplierReturn //////////////////////

            modelBuilder.Entity<SupplierReturn>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<SupplierReturn>().Navigation(g => g.Parent).AutoInclude();
            modelBuilder.Entity<SupplierReturn>().Navigation(g => g.Store).AutoInclude();
            modelBuilder.Entity<SupplierReturn>()
                .HasOne(p => p.Session)
                .WithMany(g => g.SupplierReturnList)
                .HasForeignKey(k => k.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SupplierReturn>()
                .HasOne(p => p.Parent)
                .WithMany(g => g.SupplierReturnsList)
                .HasForeignKey(k => k.ParentId)
                .OnDelete(DeleteBehavior.Restrict);










            modelBuilder.Entity<SupplierReturnItem>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<SupplierReturnItem>().HasIndex(p => new { p.ParentId, p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<SupplierReturnItem>()
                .HasOne(p => p.Parent)
                .WithMany(g => g.ItemsList)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SupplierReturnItem>()
                .HasOne(p => p.ProductUnit)
                .WithMany(g => g.SupplierReturnItemList)
                .HasForeignKey(g => g.ProductUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupplierReturnItem>()
                .HasOne(p => p.SupplyItem)
                .WithMany(g => g.ReturnList)
                .HasForeignKey(g => g.SupplyItemId)
                .OnDelete(DeleteBehavior.Restrict);



            #endregion



            #region CustomerOrder
            modelBuilder.Entity<Customer>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<CustomerPayment>().Navigation(g => g.Customer).AutoInclude();
            modelBuilder.Entity<CustomerPayment>()
                 .HasOne(p => p.Customer)
                 .WithMany(g => g.CustomerPaymentsList)
                 .HasForeignKey(k => k.CustomerId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerPayment>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<CustomerPayment>()
                .HasOne(g => g.Session)
                .WithMany(g => g.CustomerPaymentList)
                .HasForeignKey(g => g.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CustomerPayment>()
                .HasOne(g => g.Payment)
                .WithMany(g => g.CustomerPaymentlist)
                .HasForeignKey(g => g.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);




            ///////////////////////////////////////  CustomerOrder //////////////////////

            modelBuilder.Entity<CustomerOrder>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<CustomerOrder>().Navigation(g => g.Parent).AutoInclude();
            modelBuilder.Entity<CustomerOrder>().Navigation(g => g.Store).AutoInclude();
            modelBuilder.Entity<CustomerOrder>()
                .HasOne(p => p.Session)
                .WithMany(g => g.CustomerOrderList)
                .HasForeignKey(k => k.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CustomerOrder>()
                .HasOne(p => p.Parent)
                .WithMany(g => g.CustomerOrdersList)
                .HasForeignKey(k => k.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerOrder>()
                .HasOne(p => p.Store)
                .WithMany(g => g.CustomerOrderList)
                .HasForeignKey(k => k.StoreId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CustomerOrderPayment>()
                .HasOne(p => p.CustomerOrder)
                .WithMany(g => g.PaymentsList)
                .HasForeignKey(k => k.CustomerOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomerOrderPayment>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<CustomerOrderPayment>()
                .HasOne(g => g.Session)
                .WithMany(g => g.CustomerOrderPaymentList)
                .HasForeignKey(g => g.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CustomerOrderPayment>()
                .HasOne(g => g.Payment)
                .WithMany(g => g.CustomerOrderPaymentlist)
                .HasForeignKey(g => g.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);






            modelBuilder.Entity<CustomerOrderItem>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<CustomerOrderItem>().HasIndex(p => new { p.ParentId, p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<CustomerOrderItem>()
                .HasOne(p => p.Parent)
                .WithMany(g => g.ItemsList)
                .HasForeignKey(k => k.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CustomerOrderItem>()
                .HasOne(p => p.ProductUnit)
                .WithMany(g => g.CustomerOrderItemList)
                .HasForeignKey(k => k.ProductUnitId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<CustomerOrderBonus>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<CustomerOrderBonus>().HasIndex(p => new { p.ParentId, p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<CustomerOrderBonus>()
                .HasOne(p => p.Parent)
                .WithMany(g => g.BonusList)
                .HasForeignKey(k => k.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CustomerOrderBonus>()
                .HasOne(p => p.ProductUnit)
                .WithMany(g => g.CustomerOrderBonusList)
                .HasForeignKey(k => k.ProductUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            ///////////////////////////////////////  Return //////////////////////

            modelBuilder.Entity<CustomerReturn>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<CustomerReturn>().Navigation(g => g.Parent).AutoInclude();
            modelBuilder.Entity<CustomerReturn>().Navigation(g => g.Store).AutoInclude();
            modelBuilder.Entity<CustomerReturn>()
                .HasOne(p => p.Session)
                .WithMany(g => g.CustomerReturnsList)
                .HasForeignKey(k => k.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CustomerReturn>()
                .HasOne(p => p.Parent)
                .WithMany(g => g.CustomerReturnsList)
                .HasForeignKey(k => k.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CustomerReturn>()
                .HasOne(p => p.Store)
                .WithMany(g => g.CustomerReturnList)
                .HasForeignKey(k => k.StoreId)
                .OnDelete(DeleteBehavior.Restrict);





            modelBuilder.Entity<CustomerReturnItem>().HasIndex(p => new { p.ParentId, p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<CustomerReturnItem>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<CustomerReturnItem>()
                .HasOne(p => p.Parent)
                .WithMany(g => g.ItemsList)
                .HasForeignKey(k => k.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CustomerReturnItem>()
                .HasOne(p => p.ProductUnit)
                .WithMany(g => g.CustomerReturnItemList)
                .HasForeignKey(k => k.ProductUnitId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CustomerReturnItem>()
                .HasOne(p => p.CustomerOrderItem)
                .WithMany(g => g.ReturnList)
                .HasForeignKey(k => k.CustomerOrderItemId)
                .OnDelete(DeleteBehavior.Restrict);



            #endregion







            #region CashOrder

            modelBuilder.Entity<CashOrder>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<CashOrder>().Navigation(g => g.Store).AutoInclude();
            modelBuilder.Entity<CashOrder>()
                .HasOne(p => p.Session)
                .WithMany(g => g.CashOrderList)
                .HasForeignKey(k => k.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CashOrder>()
                .HasOne(p => p.Store)
                .WithMany(g => g.CashOrderList)
                .HasForeignKey(k => k.StoreId)
                .OnDelete(DeleteBehavior.Restrict);






            modelBuilder.Entity<CashOrderItem>().HasIndex(p => new { p.ParentId, p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<CashOrderItem>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<CashOrderItem>()
                .HasOne(p => p.Parent)
                .WithMany(g => g.ItemsList)
                .HasForeignKey(k => k.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CashOrderItem>()
                .HasOne(p => p.ProductUnit)
                .WithMany(g => g.CashOrderItemList)
                .HasForeignKey(k => k.ProductUnitId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<CashOrderBonus>().HasIndex(p => new { p.ParentId, p.ProductUnitId }).IsUnique();
            modelBuilder.Entity<CashOrderBonus>().Navigation(p => p.ProductUnit).AutoInclude();
            modelBuilder.Entity<CashOrderBonus>()
                .HasOne(p => p.Parent)
                .WithMany(g => g.BonusList)
                .HasForeignKey(k => k.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CashOrderBonus>()
                .HasOne(p => p.ProductUnit)
                .WithMany(g => g.CashOrderBonusList)
                .HasForeignKey(k => k.ProductUnitId)
                .OnDelete(DeleteBehavior.Restrict);


            #endregion




















            #region Partner
            modelBuilder.Entity<Partner>().HasIndex(p => p.Name).IsUnique();

            modelBuilder.Entity<CapitalWithdrawal>()
                .HasOne(p => p.Partner)
                .WithMany(g => g.CapitalWithdrawalList)
                .HasForeignKey(k => k.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CapitalDeposit>()
                .HasOne(p => p.Partner)
                .WithMany(g => g.CapitalDepositList)
                .HasForeignKey(k => k.PartnerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CapitalDeposit>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<CapitalDeposit>()
                .HasOne(g => g.Session)
                .WithMany(g => g.CapitalDepositList)
                .HasForeignKey(g => g.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CapitalDeposit>()
                .HasOne(g => g.Payment)
                .WithMany(g => g.CapitalDepositlist)
                .HasForeignKey(g => g.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CapitalWithdrawal>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<CapitalWithdrawal>()
                .HasOne(g => g.Session)
                .WithMany(g => g.CapitalWithdrawalList)
                .HasForeignKey(g => g.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CapitalWithdrawal>()
                .HasOne(g => g.Payment)
                .WithMany(g => g.CapitalWithdrawallist)
                .HasForeignKey(g => g.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<ProfitDistribute>().Navigation(k => k.Session).AutoInclude();
            modelBuilder.Entity<ProfitDistribute>().Navigation(k => k.Partner).AutoInclude();
            modelBuilder.Entity<ProfitDistribute>().Navigation(k => k.FinancialCycle).AutoInclude();
            modelBuilder.Entity<ProfitDistribute>()
                .HasOne(p => p.Partner)
                .WithMany(g => g.ProfitDistributeList)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProfitDistribute>()
                .HasOne(p => p.FinancialCycle)
                .WithMany(g => g.ProfitDistributeList)
                .OnDelete(DeleteBehavior.Restrict);







            #endregion











            #region Note


            modelBuilder.Entity<Note>()
                .HasOne(p => p.User)
                .WithMany(g => g.Noteslist)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Note>().Navigation(p => p.User).AutoInclude();



            #endregion



            #region HR


            modelBuilder.Entity<Role>().HasIndex(p => p.Name).IsUnique();



            modelBuilder.Entity<Employee>().HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Employee>().Navigation(p => p.Role).AutoInclude();
            modelBuilder.Entity<Employee>()
                .HasOne(p => p.Role)
                .WithMany(g => g.EmployeesList)
                .HasForeignKey(k => k.ParentId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<EmployeeDeduction>().Navigation(k => k.Session).AutoInclude();
            modelBuilder.Entity<EmployeeDeduction>()
                .HasOne(p => p.Parent)
                .WithMany(g => g.DeductionsList)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeAdvancePayment>()
                .HasOne(p => p.Employee)
                .WithMany(g => g.AdvancePaymentsList)
                .HasForeignKey(k => k.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeIncentive>()
                .HasOne(p => p.Employee)
                .WithMany(g => g.IncentivesList)
                .HasForeignKey(k => k.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeSalaryDisbursement>()
                .HasOne(p => p.Employee)
                .WithMany(g => g.SalaryDisbursementsList)
                .HasForeignKey(k => k.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeSalaryDisbursement>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<EmployeeSalaryDisbursement>()
                .HasOne(g => g.Session)
                .WithMany(g => g.EmployeeSalaryDisbursementList)
                .HasForeignKey(g => g.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeSalaryDisbursement>()
                .HasOne(g => g.Payment)
                .WithMany(g => g.EmployeeSalaryDisbursementlist)
                .HasForeignKey(g => g.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeAdvancePayment>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<EmployeeAdvancePayment>()
                .HasOne(g => g.Session)
                .WithMany(g => g.EmployeeAdvancePaymentList)
                .HasForeignKey(g => g.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeAdvancePayment>()
                .HasOne(g => g.Payment)
                .WithMany(g => g.EmployeeAdvancePaymentlist)
                .HasForeignKey(g => g.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<EmployeeIncentive>().Navigation(g => g.Session).AutoInclude();
            modelBuilder.Entity<EmployeeIncentive>()
                .HasOne(g => g.Session)
                .WithMany(g => g.EmployeeIncentiveList)
                .HasForeignKey(g => g.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<EmployeeIncentive>()
                .HasOne(g => g.Payment)
                .WithMany(g => g.EmployeeIncentivelist)
                .HasForeignKey(g => g.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<EmployeeSalaryDisbursement>().HasIndex(p => new { p.EmployeeId, p.Year, p.Mounth }).IsUnique();



            #endregion












        }




        #region Core
        public DbSet<Session> Sessions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Usersgroup> UsersGroups { get; set; }

        public DbSet<FinancialCycle> FinancialCycles { get; set; }
        public DbSet<PaymentOption> Payments { get; set; }



        public DbSet<Store> Stores { get; set; }

        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductUnit> Productsunits { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductInStore> ProductInStores { get; set; }

        public DbSet<Stocktaking> Stocktakings { get; set; }
        public DbSet<StocktakingItem> StocktakingItems { get; set; }
        public DbSet<StockReconciliation> StockReconciliations { get; set; }
        public DbSet<StockTransfer> StockTranfers { get; set; }


        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomersReconciliation> CustomersReconciliations { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<CustomerOrderItem> CustomerOrderItems { get; set; }
        public DbSet<CustomerOrderBonus> CustomerOrderBonus { get; set; }
        public DbSet<CustomerReturn> CustomerReturns { get; set; }
        public DbSet<CustomerReturnItem> CustomerReturnItems { get; set; }


        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<SuppliersReconciliation> SuppliersReconciliations { get; set; }

        public DbSet<Supply> Supplys { get; set; }
        public DbSet<SupplyItem> SupplyItems { get; set; }
        public DbSet<SupplyBonus> SupplyBonus { get; set; }
        public DbSet<SupplierReturn> SupplierReturns { get; set; }
        public DbSet<SupplierReturnItem> SupplierReturnItems { get; set; }


        public DbSet<CashOrder> CashOrders { get; set; }
        public DbSet<CashOrderItem> CashOrderItems { get; set; }
        public DbSet<CashOrderBonus> CashOrderBonus { get; set; }
        public DbSet<GenericReturn> GenericReturns { get; set; }
        public DbSet<GenericReturnItem> GenericReturnItems { get; set; }













        public DbSet<Expence> Expences { get; set; }
        public DbSet<EmployeeIncentive> EmployeeIncentives { get; set; }
        public DbSet<SupplyPayment> SupplyPayments { get; set; }
        public DbSet<CapitalWithdrawal> CapitalWithdrawals { get; set; }
        public DbSet<SupplierPayment> SupplierPayments { get; set; }
        public DbSet<EmployeeAdvancePayment> EmployeeAdvancePayments { get; set; }
        public DbSet<EmployeeSalaryDisbursement> EmployeeSalaryDisbursements { get; set; }
        public DbSet<CustomerPayment> CustomerPayments { get; set; }
        public DbSet<CustomerOrderPayment> CustomerOrderPayments { get; set; }
        public DbSet<CapitalDeposit> CapitalDeposits { get; set; }






        #endregion


        #region Note

        public DbSet<Note> Notes { get; set; }




        #endregion


        #region HR



        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<EmployeeDeduction> EmployeeDeductions { get; set; }


        #endregion


        #region Partner


        public DbSet<Partner> Partners { get; set; }
        public DbSet<ProfitDistribute> ProfitDistributes { get; set; }



        #endregion





    }

}
