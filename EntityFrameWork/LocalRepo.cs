using KafApp.Config;
using KafApp.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography;
using System.Security.Policy;


namespace KafApp.Repo.Server
{
    public partial class LocalRepo : Irepo
    {
        public LocalRepo()
        {
            db = new();
            transAction = new(db);
            nonTransaction = new(db);
            getData = new(db);

        }
        MyDbContext db;
        ExcuteTransaction transAction;
        ExcuteNonTransaction nonTransaction;
        GetData getData;



        public async Task<KafQueryResult> AddBonusToCashOrder(CashOrderBonus md)
        {
            transAction.action = async () =>
            {
                md.Validate();
                var Cash = await db.CashOrders.SingleAsync(j => j.Id == md.ParentId);
                await SubtractQunt(md.ProductUnitId, Cash.StoreId, md.UnitQuntity);
                await db.CashOrderBonus.AddAsync(md);
                await db.SaveChangesAsync();


                return await db.CashOrderBonus.Where(k => k.Id == md.Id)
                .Include(k => k.ProductUnitId)

                .SingleAsync();

            };
            return await transAction.Excute();
        }

        
        

        

        public async Task<KafQueryResult> AddItemToCustomerOrder(CustomerOrderItem md)
        {
            transAction.action = async () =>
            {
                md.Validate();
                var CustomerOrder = await db.CustomerOrders.SingleAsync(j => j.Id == md.ParentId);


                var Customer = await db.Customers.SingleAsync(j => j.Id == CustomerOrder.ParentId);
                Customer.CustomerOrderTotal = Customer.CustomerOrderTotal.KAfAdd(md.Value);
                Customer.ChecKCustomerConstraints();



                md.UnitCost = await CalcUnitCost(md.ProductUnitId);
                await SubtractQunt(md.ProductUnitId, CustomerOrder.StoreId, md.UnitQuntity, md.ItemCost);

                CustomerOrder.Total = CustomerOrder.Total.KAfAdd(md.Value);
                CustomerOrder.TotalCost = CustomerOrder.TotalCost.KAfAdd(md.ItemCost);
                CustomerOrder.ChecKCustomerOrderConstraints();




                FinancialCycle cycle = await db.Sessions.Where(o => o.Id == CustomerOrder.SessionId).Select(j => j.FinancialCycle).SingleOrDefaultAsync() ?? throw new KafCustomException(KafQueryStatus.NoOpenCycle);
                cycle.CustomersDebts = cycle.CustomersDebts.KAfAdd(md.Value);
                cycle.Stock = cycle.Stock.KAfSubtract(md.ItemCost);
                cycle.CustomerOrdersProfit = cycle.CustomerOrdersProfit.KAfAdd(md.ItemProf);

                await db.CustomerOrderItems.AddAsync(md);
                await db.SaveChangesAsync();


                return await db.CustomerOrderItems.SingleAsync(k => k.Id == md.Id);

            };
            var rs = await transAction.Excute();
            return rs;
        }

        
          public async Task<KafQueryResult> AddNewCashOrder(CashOrder md)
        {
            transAction.action = async () =>
            {
                md.Validate();

                foreach (var item in md.ItemsList)
                {
                    item.UnitCost = await CalcUnitCost(item.ProductUnitId);
                    await SubtractQunt(item.ProductUnitId, md.StoreId, item.UnitQuntity, item.ItemCost);
                    md.TotalCost = md.TotalCost.KAfAdd(item.ItemCost);

                }
                md.ChecKCashOrderConstraints();

                FinancialCycle cycle = await db.Sessions.Where(o => o.Id == md.SessionId).Select(j => j.FinancialCycle).SingleOrDefaultAsync() ?? throw new KafCustomException(KafQueryStatus.NoOpenCycle);
                cycle.Cash = cycle.Cash.KAfAdd(md.Paid).KAfSubtract(md.ExpenceValue);
                cycle.CashOrdersProfit = cycle.CashOrdersProfit.KAfAdd(md.TotalProf);
                cycle.CashOrdersDiscounts = cycle.CashOrdersDiscounts.KAfAdd(md.Discount).KAfSubtract(md.Added);
                cycle.CashOrdersExpences = cycle.CashOrdersExpences.KAfAdd(md.ExpenceValue);
                cycle.Stock = cycle.Stock.KAfSubtract(md.TotalCost);
                foreach (var item in md.BonusList)
                {
                    await SubtractQunt(item.ProductUnitId, md.StoreId, item.UnitQuntity);
                }
                await db.CashOrders.AddAsync(md);
                await db.SaveChangesAsync();
                md.ReferenceNumber = GenerateReferenceNumber("csh", md);
                if (md.Paid > 0 || md.ExpenceValue > 0)
                {
                    PaymentOption payment = await db.Payments.SingleAsync(j => j.Id == md.TempPaymentId);
                    payment.Credit = payment.Credit.KAfAdd(md.Paid).KAfSubtract(md.ExpenceValue);
                    payment.ChecKPaymentConstraint();
                    if (md.Paid > 0)
                    {
                        CashOrderPayment pay = new CashOrderPayment();
                        pay.SessionId = md.SessionId;
                        pay.CashOrderId = md.Id;
                        pay.PaymentId = md.TempPaymentId;
                        pay.Amount = md.Paid;
                        db.Set<CashOrderPayment>().Add(pay);
                        await db.SaveChangesAsync();
                        pay.ReferenceNumber = GenerateReferenceNumber("cshp", pay);
                        await db.SaveChangesAsync();
                    }
                    if (md.ExpenceValue > 0)
                    {
                        CashOrderExpence Exp = new CashOrderExpence();
                        Exp.SessionId = md.SessionId;
                        Exp.CashOrderId = md.Id;
                        Exp.PaymentId = md.TempPaymentId;
                        Exp.Amount = md.ExpenceValue;
                        db.Set<CashOrderExpence>().Add(Exp);
                        await db.SaveChangesAsync();
                    }

                }

                return await db.CashOrders.Where(k => k.Id == md.Id)
                .Include(k => k.Session)
                .SingleAsync();

            };
            var rs = await transAction.Excute();
            return rs;
        }

        }