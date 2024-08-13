using EntityFramework.Exceptions.Common;
using KafApp.Models;
using KafApp.ViewModels;
using kafmodels.ViewModels;


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Xml.Linq;

namespace KafApp.Repo
{

    public static class Access
    {
        private static void Validate(object model/*, out List<string> validationErrors*/)
        {
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, context, results, true);
            if ( !isValid ) 
            {
                throw new KafValidationtException();


            }
           // validationErrors = results.Select(x => x.ErrorMessage).ToList();
            
        }
        public static decimal KAfSubtract(decimal Val, decimal val2)
        {

            return Math.Round(Decimal.Subtract(Val, val2), 2);

        }
        #region Base

        static KafApiConfigModel ConfigModel;
        static MyDbContext db;
        static string Con;
        internal static void SetSource(string con)
        {
            var mmd = new KafApiConfigModel();
            mmd.SetTempConfig();
            ConfigModel = JsonSerializer.Deserialize<KafApiConfigModel>(File.ReadAllText("KafApiConfigModel.json"));
            Con = con;

        }
        private static async Task<T> SelectAsync<T>(Func<Task<T>> action)
        {
            db = new MyDbContext();
            try
            {
                return await action();
            }

            catch (TimeoutException ex)
            {
                AppAlerts.TimeoutAlert();

                return default(T);
            }
            catch (InvalidOperationException ex)
            {
                return default(T);
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }
        private static async Task<bool> ExecuteTRansActionAsync
            (Func<Task<int>> action, bool CloseSideBar = false, bool SkipAlert = false)
        {
            db = new MyDbContext();
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    int result = await action();
                    await transaction.CommitAsync();
                    if (result > 0)
                    {
                        AppAlerts.SavedAlert(SkipAlert, CloseSideBar);
                        return true;
                    }
                    else
                    {
                        AppAlerts.AllertConsistancyException();
                        return false;
                    }

                }
                catch (CashException ex)
                {
                    await transaction.RollbackAsync();

                    AppAlerts.CashLimitedAlert(); return false;

                } catch (KafValidationtException ex)
                {
                    await transaction.RollbackAsync();

                    AppAlerts.KafValidationtExceptionAlert(); return false;

                }
                catch (ConsistancyException ex)
                {
                    await transaction.RollbackAsync();

                    AppAlerts.AllertConsistancyException(); return false;

                }
                catch (BalanceException ex)
                {
                    await transaction.RollbackAsync();

                    AppAlerts.AllertBalanceException(ex.CustomeMessage); return false;

                }
                catch (NegativeSupplierCreditException ex)
                {
                    await transaction.RollbackAsync();

                    AppAlerts.NegativeSupplierCreditAlert(); return false;

                }
                catch (NegativeSupplyRestException ex)
                {
                    await transaction.RollbackAsync();

                    AppAlerts.NegativeSupplyRestAlert(); return false;

                }
                catch (NoCreditAvailableException ex)
                {
                    await transaction.RollbackAsync();

                    AppAlerts.NoCreditAvailableAlert(); return false;
                }
                catch (NoQuntityAvailableException ex)
                {
                    await transaction.RollbackAsync();

                    AppAlerts.NoQuntityAvailableAlert(); return false;
                }
                //catch (SqliteException ex)
                //{


                //    await transaction.RollbackAsync();
                //    switch (ex.SqliteExtendedErrorCode)
                //    {
                //        case 2067:
                //            AppAlerts.UniqueAlert(); return false;
                //        case 1811:
                //            AppAlerts.FK_ConstrainAlert(); return false;
                //        default:
                //            AppAlerts.DB_ErrorAlert(); return false;
                //    }

                //}

                catch (UniqueConstraintException k)
                {
                    await transaction.RollbackAsync();

                    AppAlerts.UniqueAlert();
                    return false;
                }
                catch (ReferenceConstraintException k)
                {
                    await transaction.RollbackAsync();

                    AppAlerts.FK_ConstrainAlert();
                    return false; ;
                }
                //catch (DbUpdateException ex)
                //{
                //    await transaction.RollbackAsync();


                //    if (ex.InnerException is SqliteException k)
                //    {
                //        switch (k.SqliteExtendedErrorCode)
                //        {
                //            case 2067: AppAlerts.UniqueAlert(); return false;
                //            case 1811: AppAlerts.FK_ConstrainAlert(); return false;
                //            default: AppAlerts.DB_ErrorAlert(); return false;
                //        }
                //    }
                //    else
                //    {
                //        AppAlerts.DB_ErrorAlert(); return false;
                //    }
                //}
                catch (Exception k)
                {
                    await transaction.RollbackAsync();
                    AppAlerts.DB_ErrorAlert(); return false;
                }


            }

        }






        #endregion

        #region Manage
        public static async Task<Session> CanLoginAndCreateNewSession(string name, string ppass, string hhash)
        {



            User usr = await SelectAsync(()
              => db.Users.Include(o => o.Group).SingleAsync(p => p.Alias == name && p.Password == ppass));
            if (usr == null)
                return null;

            if (!usr.Isauthonticated)
                return null;


           

            
            FinancialCycle finan = await db.FinancialCycles.Where(h => h.IsClosed == false).SingleAsync();

            if (finan == null)
                return null;


            Session Session = new Session();
            Session.UserId = usr.Id;
            
            Session.FinancialCycleId = finan.Id;
            Session.Start = DateTime.Now;
            if (await ExecuteTRansActionAsync(async () =>
             {
                 await db.Sessions.AddAsync(Session);
                 return await db.SaveChangesAsync();

             }, true))
            {
                Session.User = usr;
                
                return Session;
            }

            else return null;




        }


        public static async Task<AdminSession> TryLogAsAdmin(string nname, string pass)
        {
            if (ConfigModel.AdminName == nname && ConfigModel.AdminPassword == pass)
            {
                return new AdminSession()
                {
                    Name = nname,
                    Password = pass
                    ,
                    PreventNigativeCredits = ConfigModel.PreventNigativeCredits
                    ,
                    PreventNigativeCash = ConfigModel.PreventNigativeCash
                };
            }
            else
            {
                AppAlerts.WrongLogin();
                return null;
            }


        }


        public static async Task<bool> Isconnectiondone() =>
                await SelectAsync(() =>
                db.Database.CanConnectAsync());

       

        internal static Task<byte[]> GetAppLogo()
        {
            throw new NotImplementedException();
        }










        #region ApiConfig
        public static async Task<bool> ChangeNigativeCash()
        {
            ConfigModel.PreventNigativeCash = !ConfigModel.PreventNigativeCash;

            var g = ConfigModel.SetnewConfig();
            if (!g)
            {
                ConfigModel.PreventNigativeCash = !ConfigModel.PreventNigativeCash;
                AppAlerts.CantUpdateConfig();
            }

            return g;


        }

        public static async Task<bool> ChangeNigativeCredits()
        {
            ConfigModel.PreventNigativeCredits = !ConfigModel.PreventNigativeCredits;


            var g = ConfigModel.SetnewConfig();
            if (!g)
            {
                ConfigModel.PreventNigativeCredits = !ConfigModel.PreventNigativeCredits;
                AppAlerts.CantUpdateConfig();
            }

            return g;

        }

        #endregion







        #region Admin
        public static async Task<bool> UpdateAdminName(string name)
        {
            ConfigModel.AdminName = name;
            var g = ConfigModel.SetnewConfig();
            if (!g)
            {
                AppAlerts.CantUpdateConfig();
            }

            return g;

        }
        public static async Task<bool> UpdateAdminPassword(string pass)
        {
            ConfigModel.AdminPassword = pass;
            var g = ConfigModel.SetnewConfig();
            if (!g)
            {
                AppAlerts.CantUpdateConfig();
            }

            return g;
        }

        #endregion


        #region User
        public static async Task<List<User>> GetAllUsers() =>
            await SelectAsync(() =>
                db.Users.Where(h => !h.IsArchived).Include(o => o.Group).ToListAsync());

        public static async Task<bool> RemoveUser(int IId)
  => await ExecuteTRansActionAsync(()
      => db.Users.Where(p => p.Id == IId).ExecuteDeleteAsync());




        public static async Task<bool> AddUser(User md)
        => await ExecuteTRansActionAsync(async () =>
        {
            await db.Users.AddAsync(md);
            return await db.SaveChangesAsync();
        }, CloseSideBar: true);
        public static async Task<bool> UpdateUserName(int IId, string txt) =>
        await ExecuteTRansActionAsync(() =>
         db.Users.Where(p => p.Id == IId).ExecuteUpdateAsync(
             l => l.SetProperty(k => k.Name, txt)));
        public static async Task<bool> UpdateUserPassword(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Users.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Password, txt)));
        public static async Task<bool> UpdateUserAlias(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Users.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Alias, txt)));
        public static async Task<bool> UpdateUserGroup(int IId, int GroupId) =>
         await ExecuteTRansActionAsync(() =>
          db.Users.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.GroupId, GroupId)));

        public static async Task<bool> ArchiveUser(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.Users.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsArchived, true)));
        public static async Task<bool> RestoreUser(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.Users.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsArchived, false)));

        public static async Task<bool> AuthonticateUser(int IId) =>
  await ExecuteTRansActionAsync(() =>
   db.Users.Where(p => p.Id == IId).ExecuteUpdateAsync(
       l => l.SetProperty(k => k.Isauthonticated, true)));

        public static async Task<bool> DeAuthonticateUser(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.Users.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Isauthonticated, false)));



        #endregion


        #region UsersGroup
        public static async Task<List<Usersgroup>> GetAllUsersgroups() =>
           await SelectAsync(() =>
               db.UsersGroups.ToListAsync());
        public static async Task<bool> RemoveUsersgroup(int IId)
         => await ExecuteTRansActionAsync(()
             => db.UsersGroups.Where(p => p.Id == IId).ExecuteDeleteAsync());




        public static async Task<bool> AddUsersgroup(Usersgroup md)
        => await ExecuteTRansActionAsync(async () =>
        {
            await db.UsersGroups.AddAsync(md);
            return await db.SaveChangesAsync();
        }, CloseSideBar: true);

        public static async Task<bool> UpdateUserGroupName(int IId, string txt) =>
        await ExecuteTRansActionAsync(() =>
         db.UsersGroups.Where(p => p.Id == IId).ExecuteUpdateAsync(
             l => l.SetProperty(k => k.Name, txt)));

        public static async Task<bool> SaveAuthChanges(Usersgroup md) =>
        await ExecuteTRansActionAsync(() =>
         db.UsersGroups.Where(p => p.Id == md.Id).ExecuteUpdateAsync(
             l => l.SetProperty(k => k.ShowStore, md.ShowStore)
        .SetProperty(k => k.EditStore, md.EditStore)
        .SetProperty(k => k.DeleteStore, md.DeleteStore)
        .SetProperty(k => k.AddStore, md.AddStore)
        .SetProperty(k => k.ShowSupplier, md.ShowSupplier)
        .SetProperty(k => k.EditSupplier, md.EditSupplier)
        .SetProperty(k => k.DeleteSupplier, md.DeleteSupplier)
        .SetProperty(k => k.AddSupplier, md.AddSupplier)
        .SetProperty(k => k.ShowCustomer, md.ShowCustomer)
        .SetProperty(k => k.EditCustomer, md.EditCustomer)
        .SetProperty(k => k.DeleteCustomer, md.DeleteCustomer)
        .SetProperty(k => k.AddCustomer, md.AddCustomer)
        .SetProperty(k => k.ShowCategory, md.ShowCategory)
        .SetProperty(k => k.EditCategory, md.EditCategory)
        .SetProperty(k => k.DeleteCategory, md.DeleteCategory)
        .SetProperty(k => k.AddCategory, md.AddCategory)
        .SetProperty(k => k.ShowPartner, md.ShowPartner)
        .SetProperty(k => k.EditPartner, md.EditPartner)
        .SetProperty(k => k.DeletePartner, md.DeletePartner)
        .SetProperty(k => k.AddPartner, md.AddPartner)
        .SetProperty(k => k.ShowPaymentOption, md.ShowPaymentOption)
        .SetProperty(k => k.EditPaymentOption, md.EditPaymentOption)
        .SetProperty(k => k.DeletePaymentOption, md.DeletePaymentOption)
        .SetProperty(k => k.AddPaymentOption, md.AddPaymentOption)
        .SetProperty(k => k.ShowProduct, md.ShowProduct)
        .SetProperty(k => k.EditProduct, md.EditProduct)
        .SetProperty(k => k.DeleteProduct, md.DeleteProduct)
        .SetProperty(k => k.AddProduct, md.AddProduct)




             ));


        #endregion


    


        #region Notes
        public static async Task<List<Note>> GetAllNotes() =>
             await SelectAsync(() =>
             db.Notes.Include(h => h.User).ToListAsync());


        public static async Task<bool> RemoveNote(int IId)
    => await ExecuteTRansActionAsync(()
        => db.Notes.Where(p => p.Id == IId).ExecuteDeleteAsync());




        public static async Task<bool> AddNote(Note md)
        => await ExecuteTRansActionAsync(async () =>
        {
            await db.Notes.AddAsync(md);
            return await db.SaveChangesAsync();
        }, CloseSideBar: true);

        #endregion



        #endregion


        #region CRM


        #region Customer
        public static async Task<List<Customer>> GetAllCustomers() =>
await SelectAsync(() =>
  db.Customers.Where(o => !o.IsArchived).OrderBy(u => u.Name).ToListAsync());
        public static async Task GetCustomerData(Customer md)
        {

            //md.OrdersList = await db.CustomerOrders.Where(k => k.CustomerId == md.Id && !k.IsArchived).Include(k => k.Session).ToListAsync();
            //md.li = await db.CustomerReturns.Where(k => k.CustomerId == md.Id && !k.IsArchived).ToListAsync();
            //md.CustomerPaymentsList = await db.CustomerPayments.Where(k => k.CustomerId == md.Id && !k.IsArchived).ToListAsync();

        }
        public static async Task<bool> UpdateCustomerName(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Customers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Name, txt)));
        public static async Task<bool> UpdateCustomerMail(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Customers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Mail, txt)));
        public static async Task<bool> UpdateCustomerAddress(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Customers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Address, txt)));
        public static async Task<bool> UpdateCustomerPhone(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Customers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Phone, txt)));
        public static async Task<bool> UpdateCustomerNotes(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Customers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Notes, txt)));
        public static async Task<bool> ArchiveCustomer(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.Customers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsArchived, true)));
        public static async Task<bool> RestoreCustomer(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.Customers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsArchived, false)));
        public static async Task<bool> MarkCustomer(int IId) =>
await ExecuteTRansActionAsync(() =>
 db.Customers.Where(p => p.Id == IId).ExecuteUpdateAsync(
     l => l.SetProperty(k => k.IsFav, true)));
        public static async Task<bool> DeMarkCustomer(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.Customers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsFav, false)));



        public static async Task<bool> RemoveCustomer(int IId)
           => await ExecuteTRansActionAsync(()
               => db.Customers.Where(p => p.Id == IId).ExecuteDeleteAsync());




        public static async Task<bool> AddCustomer(Customer md)
        => await ExecuteTRansActionAsync(async () =>
        {
            await db.Customers.AddAsync(md);
            return await db.SaveChangesAsync();
        }, CloseSideBar: true);



        #endregion


        #region Supplier
        public static async Task<List<Supplier>> GetAllSuppliers() =>
            await SelectAsync(() =>
              db.suppliers.Where(o => !o.IsArchived).OrderBy(u => u.Name).ToListAsync());


        public static async Task<ArchivedSuppliersData> GetArchivedSuppliers(int LastId)
        {

            return new ArchivedSuppliersData()
            {
                Count = await SelectAsync(() =>
                  db.suppliers.Where(o => o.IsArchived && o.Id > LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.suppliers.Where(o => o.IsArchived && o.Id > LastId).Take(50).ToListAsync())
            };
        }




        public static async Task<ArchivedSuppliersData> GetArchivedSuppliersByName(int LastId, string name)
        {

            return new ArchivedSuppliersData()
            {
                Count = await SelectAsync(() =>
                  db.suppliers.Where(o => o.IsArchived && o.Name.Contains(name) && o.Id > LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.suppliers.Where(o => o.IsArchived && o.Name.Contains(name) && o.Id > LastId).Take(50).ToListAsync())
            };
        }
        public static async Task<ArchivedPaymentOptionsData> GetArchivedPaymentOptions(int LastId)
        {

            return new ArchivedPaymentOptionsData()
            {
                Count = await SelectAsync(() =>
                  db.Payments.Where(o => o.IsArchived && o.Id > LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Payments.Where(o => o.IsArchived && o.Id > LastId).Take(50).ToListAsync())
            };
        }



        public static async Task<ArchivedPaymentOptionsData> GetArchivedPaymentOptionsByName(int LastId, string name)
        {

            return new ArchivedPaymentOptionsData()
            {
                Count = await SelectAsync(() =>
                  db.Payments.Where(o => o.IsArchived && o.Name.Contains(name) && o.Id > LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Payments.Where(o => o.IsArchived && o.Name.Contains(name) && o.Id > LastId).Take(50).ToListAsync())
            };
        }

        public static async Task<ArchivedPartnersData> GetArchivedPartners(int LastId)
        {

            return new ArchivedPartnersData()
            {
                Count = await SelectAsync(() =>
                  db.Partners.Where(o => o.IsArchived && o.Id > LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Partners.Where(o => o.IsArchived && o.Id > LastId).Take(50).ToListAsync())
            };
        }



        public static async Task<ArchivedPartnersData> GetArchivedPartnersByName(int LastId, string name)
        {

            return new ArchivedPartnersData()
            {
                Count = await SelectAsync(() =>
                  db.Partners.Where(o => o.IsArchived && o.Name.Contains(name) && o.Id > LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Partners.Where(o => o.IsArchived && o.Name.Contains(name) && o.Id > LastId).Take(50).ToListAsync())
            };
        }

        public static async Task<ArchivedStoresData> GetArchivedStores(int LastId)
        {

            return new ArchivedStoresData()
            {
                Count = await SelectAsync(() =>
                  db.Stores.Where(o => o.IsArchived && o.Id > LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Stores.Where(o => o.IsArchived && o.Id > LastId).Take(50).ToListAsync())
            };
        }



        public static async Task<ArchivedStoresData> GetArchivedStoresByName(int LastId, string name)
        {

            return new ArchivedStoresData()
            {
                Count = await SelectAsync(() =>
                  db.Stores.Where(o => o.IsArchived && o.Name.Contains(name) && o.Id > LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Stores.Where(o => o.IsArchived && o.Name.Contains(name) && o.Id > LastId).Take(50).ToListAsync())
            };
        }


        public static async Task<ArchivedProductsData> GetArchivedProducts(int LastId)
        {

            return new ArchivedProductsData()
            {
                Count = await SelectAsync(() =>
                  db.Products.Where(o => o.IsArchived && o.Id > LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Products.Where(o => o.IsArchived && o.Id > LastId).Take(50).Include(m => m.Category).Include(m => m.Unit).ToListAsync())
            };
        }



        public static async Task<ArchivedProductsData> GetArchivedProductsByName(int LastId, string name)
        {

            return new ArchivedProductsData()
            {
                Count = await SelectAsync(() =>
                  db.Products.Where(o => o.IsArchived && o.Name.Contains(name) && o.Id > LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Products.Where(o => o.IsArchived && o.Name.Contains(name) && o.Id > LastId).Take(50).Include(m => m.Category).ToListAsync())
            };
        }






        public static async Task<ArchivedCustomersData> GetArchivedCustomers(int LastId)
        {

            return new ArchivedCustomersData()
            {
                Count = await SelectAsync(() =>
                  db.Customers.Where(o => o.IsArchived && o.Id > LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Customers.Where(o => o.IsArchived && o.Id > LastId).Take(50).ToListAsync())
            };
        }



        public static async Task<ArchivedCustomersData> GetArchivedCustomersByName(int LastId, string name)
        {

            return new ArchivedCustomersData()
            {
                Count = await SelectAsync(() =>
                  db.Customers.Where(o => o.IsArchived && o.Name.Contains(name) && o.Id > LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Customers.Where(o => o.IsArchived && o.Name.Contains(name) && o.Id > LastId).Take(50).ToListAsync())
            };
        }


        public static async Task GetSupplierData(Supplier md)
        {
            md.SupplysList = await db.Supplys.Where(k => k.SupplierId == md.Id && !k.IsArchived).Include(k => k.Session).ToListAsync();
            md.SupplierReturnsList = await db.SupplierReturns.Where(k => k.SupplierId == md.Id && !k.IsArchived).Include(k => k.Session).ToListAsync();
            md.SupplierPaymentsList = await db.supplierPayments.Where(k => k.SupplierId == md.Id && !k.IsArchived).Include(k => k.Session).ToListAsync();
        }
        public static async Task<bool> UpdateSupplierName(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.suppliers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Name, txt)));
        public static async Task<bool> UpdateSupplierMail(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.suppliers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Mail, txt)));
        public static async Task<bool> UpdateSupplierAddress(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.suppliers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Address, txt)));
        public static async Task<bool> UpdateSupplierPhone(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.suppliers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Phone, txt)));
        public static async Task<bool> UpdateSupplierNotes(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.suppliers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Notes, txt)));
        public static async Task<bool> ArchiveSupplier(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.suppliers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsArchived, true)));
        public static async Task<bool> RestoreSupplier(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.suppliers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsArchived, false)));

        public static async Task<bool> MarkSupplier(int IId) =>
await ExecuteTRansActionAsync(() =>
 db.suppliers.Where(p => p.Id == IId).ExecuteUpdateAsync(
     l => l.SetProperty(k => k.IsFav, true)));
        public static async Task<bool> DeMarkSupplier(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.suppliers.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsFav, false)));



        public static async Task<bool> RemoveSupplier(int IId)
           => await ExecuteTRansActionAsync(()
               => db.suppliers.Where(p => p.Id == IId).ExecuteDeleteAsync());




        public static async Task<bool> AddSupplier(Supplier md)
        => await ExecuteTRansActionAsync(async () =>
        {
            Validate(md);
            await db.suppliers.AddAsync(md);
            return await db.SaveChangesAsync();
        }, CloseSideBar: true);

        static async Task<int> SupplierTotalUp(int IId, decimal val)
        {
            var supplier = await db.suppliers.AsTracking().SingleAsync(j => j.Id == IId);
            supplier.SupplyTotal += val;

            await BalanceSuppliersDebtsUp(val);
            return await db.SaveChangesAsync();
        }
        static async Task<int> SupplierTotalDown(int IId, decimal val)
        {
            var supplier = await db.suppliers.AsTracking().SingleAsync(j => j.Id == IId);
            supplier.SupplyTotal = KAfSubtract(supplier.SupplyTotal, val);
            if (supplier.Credit < 0)
                throw new NegativeSupplierCreditException();
            await BalanceSuppliersDebtsDown(val);
            return await db.SaveChangesAsync();
        }
        static async Task<int> SupplierPaidUp(int IId, decimal val)
        {
            var supplier = await db.suppliers.AsTracking().SingleAsync(j => j.Id == IId);
            supplier.Paid += val;
            if (supplier.Credit < 0)
                throw new NegativeSupplierCreditException();
            await BalanceSuppliersDebtsDown(val);
            return await db.SaveChangesAsync();
        }
        static async Task<int> SupplierPaidDown(int IId, decimal val)
        {
            var supplier = await db.suppliers.AsTracking().SingleAsync(j => j.Id == IId);
            supplier.Paid = KAfSubtract(supplier.Paid, val);
            await BalanceSuppliersDebtsUp(val);
            return await db.SaveChangesAsync();
        }

        static async Task<int> SupplierReturnTotalUp(int IId, decimal val)
        {
            var supplier = await db.suppliers.AsTracking().SingleAsync(j => j.Id == IId);
            supplier.SupplierReturnTotal += val;
            if (supplier.Credit < 0)
                throw new NegativeSupplierCreditException();
            await BalanceSuppliersDebtsDown(val);
            return await db.SaveChangesAsync();
        }
        static async Task<int> SupplierReturnTotalDown(int IId, decimal val)
        {
            var supplier = await db.suppliers.AsTracking().SingleAsync(j => j.Id == IId);
            supplier.SupplierReturnTotal = KAfSubtract(supplier.SupplierReturnTotal, val);
            await BalanceSuppliersDebtsUp(val);
            return await db.SaveChangesAsync();
        }

        #endregion



        #endregion


        #region Financial
        public static async Task<FinancialViewViewData> GetFinancialViewData()
        {
            var balnce = await db.Balance.SingleAsync();

            return new FinancialViewViewData()
            {
                CustomersDebts = balnce.CustomersDebts
                ,
                SuppliersDebts = balnce.SuppliersDebts
                ,
                CashDebts = balnce.CashDebts
                ,
                Cash = balnce.Cash
                ,
                Capital = balnce.Capital
                ,
                Stock = balnce.Stock
                ,
                Profit = balnce.Profit


                ,
                PartnersList = await GetAllPartners()
          ,
                PaymentList = await GetAllPaymentOptions()
            };

        }
        #region Balance
        static async Task BalanceCustomersDebtsUp(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.CustomersDebts += val;

        }
        static async Task BalanceCustomersDebtsDown(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.CustomersDebts = KAfSubtract(balnce.CustomersDebts, val);

        }
        static async Task BalanceSuppliersDebtsUp(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.SuppliersDebts += val;

        }
        static async Task BalanceSuppliersDebtsDown(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.SuppliersDebts = KAfSubtract(balnce.SuppliersDebts, val);

        }

        static async Task<int> BalanceProfitUp(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.Profit += val;
            return await db.SaveChangesAsync();
        }
        static async Task<int> BalanceProfitDown(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.Profit = KAfSubtract(balnce.Profit, val);
            return await db.SaveChangesAsync();
        }
        static async Task BalanceCashUp(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.Cash += val;

        }
        static async Task BalanceCashDown(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.Cash = KAfSubtract(balnce.Cash, val);

        }
        static async Task BalanceStockUp(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.Stock += val;

        }
        static async Task BalanceStockDown(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.Stock = KAfSubtract(balnce.Stock, val);

        }
        static async Task<int> BalanceCapitalUp(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.Capital += val;
            return await db.SaveChangesAsync();
        }
        static async Task<int> BalanceCapitalDown(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.Capital = KAfSubtract(balnce.Capital, val);
            return await db.SaveChangesAsync();
        }
        static async Task<int> BalanceCashDebtsUp(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.CashDebts += val;
            return await db.SaveChangesAsync();
        }
        static async Task<int> BalanceCashDebtsDown(decimal val)
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();
            balnce.CashDebts = KAfSubtract(balnce.CashDebts, val);
            return await db.SaveChangesAsync();
        }
        static async Task DeepValidate(Balance balnce)
        {
            //var SupplierReturnItemsGroupedStoreIdsTotal = SupplierReturnItems.GroupBy(j => j.StoreId).Select(f => new
            //{
            //    StoreId = f.Key,
            //    total = f.Sum((f => f.UnitPrice * f.UnitQuntity))
            //                ,
            //    qun = f.Sum(f => f.UnitQuntity * (Productsunits.Where(g => g.ProductId == f.ProductId && g.UnitId == f.UnitId).Single().UnitValue))
            //}).ToList();

            var suppliers = await db.suppliers.ToListAsync();
            var Supplys = await db.Supplys.ToListAsync();
            var supplierPayments = await db.supplierPayments.ToListAsync();
            var supplyPayments = await db.supplyPayments.ToListAsync();
            var SupplyItems = await db.SupplyItems.ToListAsync();
            var SupplierReturns = await db.SupplierReturns.ToListAsync();
            var SupplierReturnItems = await db.SupplierReturnItems.ToListAsync();


            var Customers = await db.Customers.ToListAsync();
            var CustomerOrders = await db.CustomerOrders.ToListAsync();
            var CustomerPayments = await db.CustomerPayments.ToListAsync();
            var CustomerOrderPayments = await db.CustomerOrderPayments.ToListAsync();
            var CustomerOrderItems = await db.CustomerOrderItems.ToListAsync();
            var CustomerReturns = await db.CustomerReturns.ToListAsync();
            var CustomerReturnItems = await db.CustomerReturnItems.ToListAsync();

            var CashOrders = await db.CashOrders.ToListAsync();
            var CashOrderItems = await db.CashOrderItems.ToListAsync();
            var GenericReturns = await db.GenericReturns.ToListAsync();
            var GenericReturnItems = await db.GenericReturnItems.ToListAsync();

            var Productsunits = await db.Productsunits.ToListAsync();
            var Products = await db.Products.ToListAsync();
            var ProductInStores = await db.ProductInStores.ToListAsync();

            var Partners = await db.Partners.ToListAsync();
            var Payments = await db.Payments.ToListAsync();
            var Expences = await db.Expences.ToListAsync();
            var CapitalDeposits = await db.CapitalDeposits.ToListAsync();
            var CapitalWithdrawals = await db.CapitalWithdrawals.ToListAsync();


            #region balnce


            var Profitplus = Supplys.Select(f => f.Discount).Sum()
                - CustomerReturns.Select(f => f.Discount).Sum()
                - GenericReturns.Select(f => f.Discount).Sum()
                ;

            var Profitminus = CashOrders.Select(f => f.Discount).Sum()
                + CustomerOrders.Select(f => f.Discount).Sum()
                + SupplierReturns.Select(f => f.Discount).Sum()
                + Expences.Select(f => f.Value).Sum()

                ;

            var CalculateedProfit = Profitplus - Profitminus;


            if (
                balnce.CustomersDebts != Customers.Select(f => f.Credit).Sum()
                || balnce.SuppliersDebts != suppliers.Select(f => f.Credit).Sum()
                || balnce.Cash != Payments.Select(f => f.Credit).Sum()
                || balnce.Stock != Products.Select(f => f.Value).Sum()
                || balnce.Capital != Partners.Select(f => f.Credit).Sum()
                || balnce.Profit != CalculateedProfit

                ) { throw new BalanceException("balnce"); }





            #endregion

            #region check Supplier

            if (
               suppliers.Select(n => n.SupplyTotal).Sum() != Supplys.Select(n => n.Rest).Sum()
               || suppliers.Select(n => n.Paid).Sum() != supplierPayments.Select(n => n.Value).Sum()
               || suppliers.Select(n => n.SupplierReturnTotal).Sum() != SupplierReturns.Select(n => n.Rest).Sum()

                ) { throw new BalanceException("suppliers"); }

            if (Supplys.Select(n => n.Total).Sum() != SupplyItems.Select(j => j.Value).Sum()
                   || Supplys.Select(n => n.Paid).Sum() != supplyPayments.Select(j => j.Value).Sum())
                throw new BalanceException("Supply");


            if (SupplierReturns.Select(n => n.Total).Sum() != SupplierReturnItems.Select(j => j.Value).Sum())
                throw new BalanceException("SupplierReturns");



            foreach (var item in Supplys)
            {
                if (item.Total != SupplyItems.Where(j => j.SupplyId == item.Id).Select(j => j.Value).Sum()
                    || item.Paid != supplyPayments.Where(j => j.SupplyId == item.Id).Select(j => j.Value).Sum())
                    throw new BalanceException("foreachSupply");
            }
            foreach (var item in SupplierReturns)
            {
                if (item.Total != SupplierReturnItems.Where(j => j.SupplierReturnId == item.Id).Select(j => j.Value).Sum())
                    throw new BalanceException("foreachSupplierReturns");
            }

            foreach (var item in suppliers)
            {
                if (item.SupplyTotal != Supplys.Where(j => j.SupplierId == item.Id).Select(j => j.Rest).Sum()
                || item.SupplierReturnTotal != SupplierReturns.Where(j => j.SupplierId == item.Id).Select(j => j.Rest).Sum()
                    || item.Paid != supplierPayments.Where(j => j.SupplierId == item.Id).Select(j => j.Value).Sum())
                    throw new BalanceException("foreachsuppliers");
            }



            #endregion

            #region check Customer

            if (
               Customers.Select(n => n.OrderTotal).Sum() != CustomerOrders.Select(n => n.Rest).Sum()
               || Customers.Select(n => n.Paid).Sum() != CustomerPayments.Select(n => n.Value).Sum()
               || Customers.Select(n => n.ReturnTotal).Sum() != CustomerReturns.Select(n => n.Rest).Sum()

                ) { throw new BalanceException("Customers"); }

            if (CustomerOrders.Select(n => n.Total).Sum() != CustomerOrderItems.Select(j => j.Value).Sum()
                   || CustomerOrders.Select(n => n.Paid).Sum() != CustomerOrderPayments.Select(j => j.Value).Sum())
                throw new BalanceException("CustomerOrders");


            if (CustomerReturns.Select(n => n.Total).Sum() != CustomerReturnItems.Select(j => j.Value).Sum())
                throw new BalanceException("CustomerReturns");



            foreach (var item in CustomerOrders)
            {
                if (item.Total != CustomerOrderItems.Where(j => j.CustomerOrderId == item.Id).Select(j => j.Value).Sum()
                    || item.Paid != CustomerOrderPayments.Where(j => j.CustomerOrderId == item.Id).Select(j => j.Value).Sum())
                    throw new BalanceException("foreachCustomerOrders");
            }
            foreach (var item in CustomerReturns)
            {
                if (item.Total != CustomerReturnItems.Where(j => j.CustomerReturnId == item.Id).Select(j => j.Value).Sum())
                    throw new BalanceException("foreachCustomerReturns");
            }

            foreach (var item in Customers)
            {
                if (item.OrderTotal != CustomerOrders.Where(j => j.CustomerId == item.Id).Select(j => j.Rest).Sum()
                || item.ReturnTotal != CustomerReturns.Where(j => j.CustomerId == item.Id).Select(j => j.Rest).Sum()
                    || item.Paid != CustomerPayments.Where(j => j.CustomerId == item.Id).Select(j => j.Value).Sum())
                    throw new BalanceException("foreachCustomers");
            }



            #endregion





            #region check CashOrder

            if (CashOrders.Select(n => n.Total).Sum() != CashOrderItems.Select(j => j.Value).Sum())
                throw new BalanceException("CashOrders");


            if (GenericReturns.Select(n => n.Total).Sum() != GenericReturnItems.Select(j => j.Value).Sum())
                throw new BalanceException("GenericReturns");



            foreach (var item in CashOrders)
            {
                if (item.Total != CashOrderItems.Where(j => j.CashOrderId == item.Id).Select(j => j.Value).Sum())
                    throw new BalanceException("foreachCashOrders");
            }
            foreach (var item in GenericReturns)
            {
                if (item.Total != GenericReturnItems.Where(j => j.GenericReturnId == item.Id).Select(j => j.Value).Sum())
                    throw new BalanceException("foreachGenericReturns");
            }








            #endregion


            #region Stock

            var AllInValue = SupplyItems.Select(j => j.Value).Sum()
                + CustomerReturnItems.Select(j => j.Value).Sum()
                + GenericReturnItems.Select(j => j.Value).Sum();
            var AllOutValue = CustomerOrderItems.Select(j => j.Value).Sum()
                + SupplierReturnItems.Select(j => j.Value).Sum()
                + CashOrderItems.Select(j => j.Value).Sum();

            var AllInQun = SupplyItems.Select(j => j.UnitQuntity * (Productsunits.Where(g => g.ProductId == j.ProductId && g.UnitId == j.UnitId).Single().UnitValue)).Sum()
                + CustomerReturnItems.Select(j => j.UnitQuntity * (Productsunits.Where(g => g.ProductId == j.ProductId && g.UnitId == j.UnitId).Single().UnitValue)).Sum()
                + GenericReturnItems.Select(j => j.UnitQuntity * (Productsunits.Where(g => g.ProductId == j.ProductId && g.UnitId == j.UnitId).Single().UnitValue)).Sum();

            var AllOutQun = CustomerOrderItems.Select(j => j.UnitQuntity * (Productsunits.Where(g => g.ProductId == j.ProductId && g.UnitId == j.UnitId).Single().UnitValue)).Sum()
                + SupplierReturnItems.Select(j => j.UnitQuntity * (Productsunits.Where(g => g.ProductId == j.ProductId && g.UnitId == j.UnitId).Single().UnitValue)).Sum()
                + CashOrderItems.Select(j => j.UnitQuntity * (Productsunits.Where(g => g.ProductId == j.ProductId && g.UnitId == j.UnitId).Single().UnitValue)).Sum();


            if (
              AllInValue != AllOutValue + Products.Select(n => n.Value).Sum()
              || AllInQun != AllOutQun + Products.Select(n => n.Quntity).Sum()
              || ProductInStores.Select(n => n.Quntity).Sum() != Products.Select(n => n.Quntity).Sum()

               ) { throw new BalanceException("Products"); }

            foreach (var item in Products)
            {
                var InValue = SupplyItems.Where(j => j.ProductId == item.Id).Select(j => j.Value).Sum()
                + CustomerReturnItems.Where(j => j.ProductId == item.Id).Select(j => j.Value).Sum()
                + GenericReturnItems.Where(j => j.ProductId == item.Id).Select(j => j.Value).Sum();
                var OutValue = CustomerOrderItems.Where(j => j.ProductId == item.Id).Select(j => j.Value).Sum()
                    + SupplierReturnItems.Where(j => j.ProductId == item.Id).Select(j => j.Value).Sum()
                    + CashOrderItems.Where(j => j.ProductId == item.Id).Select(j => j.Value).Sum();

                var InQun = SupplyItems.Where(j => j.ProductId == item.Id).Select(j => j.UnitQuntity * (Productsunits.Where(g => g.ProductId == j.ProductId && g.UnitId == j.UnitId).Single().UnitValue)).Sum()
                    + CustomerReturnItems.Where(j => j.ProductId == item.Id).Select(j => j.UnitQuntity * (Productsunits.Where(g => g.ProductId == j.ProductId && g.UnitId == j.UnitId).Single().UnitValue)).Sum()
                    + GenericReturnItems.Where(j => j.ProductId == item.Id).Select(j => j.UnitQuntity * (Productsunits.Where(g => g.ProductId == j.ProductId && g.UnitId == j.UnitId).Single().UnitValue)).Sum();

                var OutQun = CustomerOrderItems.Where(j => j.ProductId == item.Id).Select(j => j.UnitQuntity * (Productsunits.Where(g => g.ProductId == j.ProductId && g.UnitId == j.UnitId).Single().UnitValue)).Sum()
                    + SupplierReturnItems.Where(j => j.ProductId == item.Id).Select(j => j.UnitQuntity * (Productsunits.Where(g => g.ProductId == j.ProductId && g.UnitId == j.UnitId).Single().UnitValue)).Sum()
                    + CashOrderItems.Where(j => j.ProductId == item.Id).Select(j => j.UnitQuntity * (Productsunits.Where(g => g.ProductId == j.ProductId && g.UnitId == j.UnitId).Single().UnitValue)).Sum();


                if (
                  InValue != OutValue + item.Value
                  || InQun != OutQun + item.Quntity
                  || ProductInStores.Where(j => j.ProductId == item.Id).Select(n => n.Quntity).Sum() != item.Quntity

                   ) { throw new BalanceException("foreachProducts"); }
            }


            #endregion

            #region Safe





            var InCash = SupplierReturns.Select(j => j.Paid).Sum()
               + CashOrders.Select(j => j.Paid).Sum()
               + CustomerOrderPayments.Select(j => j.Value).Sum()
               + CustomerPayments.Select(j => j.Value).Sum()
               + CapitalDeposits.Select(j => j.Value).Sum();

            var OutCash = GenericReturns.Select(j => j.Paid).Sum()
                + CustomerReturns.Select(j => j.Paid).Sum()
                + supplyPayments.Select(j => j.Value).Sum()
                + supplierPayments.Select(j => j.Value).Sum()
                + CapitalWithdrawals.Select(j => j.Value).Sum()
                + Expences.Select(j => j.Value).Sum();
            if (InCash != OutCash + Payments.Select(j => j.Credit).Sum())
                throw new BalanceException("Payments");



            foreach (var item in Payments)
            {
                var GroupedInCash = SupplierReturns.Where(j => j.PaymentId == item.Id).Select(j => j.Paid).Sum()
                       + CashOrders.Where(j => j.PaymentId == item.Id).Select(j => j.Paid).Sum()
                       + CustomerOrderPayments.Where(j => j.PaymentId == item.Id).Select(j => j.Value).Sum()
                       + CustomerPayments.Where(j => j.PaymentId == item.Id).Select(j => j.Value).Sum()
                       + CapitalDeposits.Where(j => j.PaymentId == item.Id).Select(j => j.Value).Sum();

                var GroupedOutCash = GenericReturns.Where(j => j.PaymentId == item.Id).Select(j => j.Paid).Sum()
                    + CustomerReturns.Where(j => j.PaymentId == item.Id).Select(j => j.Paid).Sum()
                    + supplyPayments.Where(j => j.PaymentId == item.Id).Select(j => j.Value).Sum()
                    + supplierPayments.Where(j => j.PaymentId == item.Id).Select(j => j.Value).Sum()
                    + CapitalWithdrawals.Where(j => j.PaymentId == item.Id).Select(j => j.Value).Sum()
                    + Expences.Where(j => j.PaymentId == item.Id).Select(j => j.Value).Sum();


                if (GroupedInCash != GroupedOutCash + item.Credit)
                    throw new BalanceException("foreachPayments");
            }


            if (CapitalDeposits.Select(j => j.Value).Sum() !=
                CapitalWithdrawals.Select(j => j.Value).Sum() +
                Partners.Select(j => j.Credit).Sum())
                throw new BalanceException("Partners");



            foreach (var item in Partners)
            {
                if (CapitalDeposits.Where(j => j.PartnerId == item.Id).Select(j => j.Value).Sum() !=
                CapitalWithdrawals.Where(j => j.PartnerId == item.Id).Select(j => j.Value).Sum() +
                item.Credit)
                    throw new BalanceException("foreachPartners");

            }





            #endregion












        }



        static async Task IsBalanced()
        {
            var balnce = await db.Balance.AsTracking().SingleAsync();

            if (!balnce.IsValid())
                throw new BalanceException();

            await DeepValidate(balnce);

        }
        #endregion


        #region PaymentOption
        public static async Task<PaymentOptionViewData> GetPaymentOptionViewData(PaymentOption md)
        {
            var g = new PaymentOptionViewData()
            {
                Credit = await SelectAsync(() =>
                db.Payments.Where(p => p.Id == md.Id).Select(b => b.Credit).SingleAsync()),
                CashDepositList = await SelectAsync(()
                => db.CapitalDeposits.Where(k => k.PaymentId == md.Id).Include(u => u.Session).Select(p
                => new CashDeposit(AppProccessType.CapitalDeposit)
                {
                    Value = p.Value
                ,
                    Session = p.Session,
                    Payment = md
                })
                .ToListAsync())
          ,
                CashWithdrawalList = (await SelectAsync(()
                => db.CapitalWithdrawals.Where(k => k.PaymentId == md.Id).Include(h => h.Session).Select(p
                => new CashWithdrawal(AppProccessType.CapitalWithdrawal)
                { Value = p.Value, Session = p.Session, Payment = md }).ToListAsync()))
                .Concat(await SelectAsync(()
                => db.supplyPayments.Where(k => k.PaymentId == md.Id).Include(h => h.Session).Select(p
                => new CashWithdrawal(AppProccessType.supplyPayments)
                { Value = p.Value, Session = p.Session, Payment = md }).ToListAsync()))

                                .Concat(await SelectAsync(()
                => db.supplierPayments.Where(k => k.PaymentId == md.Id).Include(h => h.Session).Select(p
                => new CashWithdrawal(AppProccessType.supplierPayments)
                { Value = p.Value, Session = p.Session, Payment = md }).ToListAsync()))






                .ToList()



            };
            return g;
        }


        public static async Task<List<PaymentOption>> GetAllPaymentOptions() =>
               await SelectAsync(() =>
               db.Payments.Where(o => !o.IsArchived).OrderBy(u => u.Name).ToListAsync());
        public static async Task<bool> UpdatePaymentOptionName(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Payments.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Name, txt)));

        public static async Task<bool> ArchivePaymentOption(int IId) =>
       await ExecuteTRansActionAsync(() =>
        db.Payments.Where(p => p.Id == IId).ExecuteUpdateAsync(
            l => l.SetProperty(k => k.IsArchived, true)));
        public static async Task<bool> RestorePaymentOption(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.Payments.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsArchived, false)));
        public static async Task<bool> UpdatePaymentOptionAccount(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Payments.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Account, txt)));
        public static async Task<bool> UpdatePaymentOptionNotes(int IId, string txt) =>
 await ExecuteTRansActionAsync(() =>
  db.Payments.Where(p => p.Id == IId).ExecuteUpdateAsync(
      l => l.SetProperty(k => k.Notes, txt)));



        public static async Task<bool> RemovePaymentOption(int IId)
 => await ExecuteTRansActionAsync(()
     => db.Payments.Where(p => p.Id == IId).ExecuteDeleteAsync());




        public static async Task<bool> AddPaymentOption(PaymentOption md)
        => await ExecuteTRansActionAsync(async () =>
        {
            await db.Payments.AddAsync(md);
            return await db.SaveChangesAsync();
        }, CloseSideBar: true);

        static async Task<int> CashUp(int iid, decimal val)
        {
            (await db.Payments.AsTracking().SingleAsync(j => j.Id == iid)).Credit += val;
            await BalanceCashUp(val);

            return await db.SaveChangesAsync();
        }
        static async Task<int> CashDown(int iid, decimal val)
        {
            var payment = await db.Payments.AsTracking().SingleAsync(j => j.Id == iid);

            if (payment.Credit < val)
                throw new CashException();

            payment.Credit = KAfSubtract(payment.Credit, val);

            await BalanceCashDown(val);
            return await db.SaveChangesAsync();
        }







        #endregion

        #region Partner
        public static async Task<List<Partner>> GetAllPartners() =>
         await SelectAsync(() =>
             db.Partners.Where(o => !o.IsArchived).OrderBy(u => u.Name).ToListAsync());

        public static async Task<PartnerViewData> GetPartnerViewData(Partner md)
        {
            return new PartnerViewData()
            {

                ProfitsDivisionList = await SelectAsync(() =>
        db.ProfitsDivisions.Where(o => o.PartnerId == md.Id).ToListAsync())
          ,
                CapitalDepositList = await SelectAsync(() =>
        db.CapitalDeposits.Where(o => o.PartnerId == md.Id).Include(h => h.Session).ToListAsync())
          ,
                CapitalWithdrawalList = await SelectAsync(() =>
        db.CapitalWithdrawals.Where(o => o.PartnerId == md.Id).Include(h => h.Session).ToListAsync())
            };

        }
        public static async Task<bool> RemovePartner(int IId)
          => await ExecuteTRansActionAsync(()
              => db.Partners.Where(p => p.Id == IId).ExecuteDeleteAsync());




        public static async Task<bool> AddPartner(Partner md)
        => await ExecuteTRansActionAsync(async () =>
        {
            await db.Partners.AddAsync(md);
            return await db.SaveChangesAsync();
        }, CloseSideBar: true);

        public static async Task<bool> UpdatePartnerName(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Partners.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Name, txt)));
        public static async Task<bool> UpdatePartnerMail(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Partners.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Mail, txt)));
        public static async Task<bool> UpdatePartnerAddress(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Partners.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Address, txt)));
        public static async Task<bool> UpdatePartnerPhone(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Partners.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Phone, txt)));
        public static async Task<bool> UpdatePartnerNotes(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Partners.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Notes, txt)));
        public static async Task<bool> ArchivePartner(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.Partners.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsArchived, true)));
        public static async Task<bool> RestorePartner(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.Partners.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsArchived, false)));

        static async Task<int> PartnerCreditUp(int IId, decimal val)
        {

            var partner = await db.Partners.AsTracking().SingleAsync(j => j.Id == IId);
            partner.Credit += val;
            var g = await db.SaveChangesAsync();
            await BalanceCapitalUp(val);

            return await db.SaveChangesAsync();
        }
        static async Task<int> PartnerCreditDown(int IId, decimal val)
        {
            var Partner = await db.Partners.AsTracking().SingleAsync(j => j.Id == IId);
            if (ConfigModel.PreventNigativeCredits && Partner.Credit < val)
                throw new NoCreditAvailableException();
            Partner.Credit = KAfSubtract(Partner.Credit, val);
            await BalanceCapitalDown(val);
            return await db.SaveChangesAsync();
        }
        #endregion


        #endregion


        #region Invoices
        public static async Task<InvoiceProductData> GetInvoiceProductData(int iid)
        {

            return new InvoiceProductData()
            {

                ProductInStoreList = await GetAllProductStores(iid)
          ,
                ProductUnitList = await GetAllProductUnits(iid)
            };

        }



        private static async Task<List<ProductUnit>> GetAllProductUnits(int iid)
        => await SelectAsync(() =>
                db.Productsunits.Include(k => k.Unit).Where(k => k.ProductId == iid).ToListAsync());

        private static async Task<List<ProductInStore>> GetAllProductStores(int iid)
                 => await SelectAsync(() =>
                db.ProductInStores.Include(k => k.Store).Where(k => k.ProductId == iid && !k.Store.IsArchived).ToListAsync());



        #region Daily
        public static async Task<DailyViewData> GetDailyViewData(DateTime date)
        {
           
            var h = await db.Sessions.Where(f => f.Start.Date == date).Select(g => g.Id).ToArrayAsync();
            return await SelectAsync(async () =>
            {
                return new DailyViewData()
                {

                    SupplysList = await db.Supplys.Where(k => !k.IsArchived && h.Contains(k.SessionId)).Include(k => k.Session).ThenInclude(k => k.User).ToListAsync()
              ,
                    CapitalDepositList = await db.CapitalDeposits.Where(k => !k.IsArchived && h.Contains(k.SessionId)).Include(k => k.Session).ThenInclude(k => k.User).ToListAsync()
              ,
                    CapitalWithdrawalList = await db.CapitalWithdrawals.Where(k => !k.IsArchived && h.Contains(k.SessionId)).Include(k => k.Session).ThenInclude(k => k.User).ToListAsync()
              ,
                    CustomerOrderList = await db.CustomerOrders.Where(k => !k.IsArchived && h.Contains(k.SessionId)).Include(k => k.Session).ThenInclude(k => k.User).ToListAsync()
              ,
                    CustomerOrderPaymentsList = await db.CustomerOrderPayments.Where(k => !k.IsArchived && h.Contains(k.SessionId)).Include(k => k.Session).ThenInclude(k => k.User).ToListAsync()
              ,
                    CustomerPaymentsList = await db.CustomerPayments.Where(k => !k.IsArchived && h.Contains(k.SessionId)).Include(k => k.Session).ThenInclude(k => k.User).ToListAsync()
              ,
                    CustomerReturnsList = await db.CustomerReturns.Where(k => !k.IsArchived && h.Contains(k.SessionId)).Include(k => k.Session).ThenInclude(k => k.User).ToListAsync()
              ,
                    GenericReturnsList = await db.GenericReturns.Where(k => !k.IsArchived && h.Contains(k.SessionId)).Include(k => k.Session).ThenInclude(k => k.User).ToListAsync()
                 ,
                    CashOrdersList = await db.CashOrders.Where(k => !k.IsArchived && h.Contains(k.SessionId)).Include(k => k.Session).ThenInclude(k => k.User).ToListAsync()
                 ,
                    ExpencesList = await db.Expences.Where(k => !k.IsArchived && h.Contains(k.SessionId)).Include(k => k.Session).ThenInclude(k => k.User).ToListAsync()
                 ,
                    SupplyPaymentList = await db.supplyPayments.Where(k => !k.IsArchived && h.Contains(k.SessionId)).Include(k => k.Session).ThenInclude(k => k.User).ToListAsync()
                 ,
                    SupplierPaymentList = await db.supplierPayments.Where(k => !k.IsArchived && h.Contains(k.SessionId)).Include(k => k.Session).ThenInclude(k => k.User).ToListAsync()
                 ,
                    SupplierReturnList = await db.SupplierReturns.Where(k => !k.IsArchived && h.Contains(k.SessionId)).Include(k => k.Session).ThenInclude(k => k.User).ToListAsync()

                };

            });
        }
        #endregion

        #region Supply
        public static async Task<NewSupplyViewData> GetNewSupplyViewData()
        {
            return await SelectAsync(async () =>
            {
                return new NewSupplyViewData()
                {
                    ProductsList = await GetAllProducts()
          ,
                    CategoriesList = await GetAllCategories()
          ,
                    SupList = await GetAllSuppliers()
          ,
                    PaymentsList = await GetAllPaymentOptions()

                };
            });
        }

        public static async Task GetSupplyViewData(Supply md)
        {
            md.ItemsList = await db.SupplyItems.Where(h => h.SupplyId == md.Id).Include(y => y.Product).Include(y => y.Unit).Include(y => y.Store).ToListAsync();


            md.PaymentsList = await db.supplyPayments.Where(h => h.SupplyId == md.Id).Include(y => y.Session).ToListAsync();


        }
        public static async Task<bool> AddNewSupply(Supply md)
        {
            return await ExecuteTRansActionAsync(async () =>
            {
                int u = 0;
                await db.Supplys.AddAsync(md);
                u += await db.SaveChangesAsync();


                u += await SupplierTotalUp(md.SupplierId, md.Rest);

                if (md.PaymentsList != null)
                {
                    foreach (var item in md.PaymentsList)
                    {
                        u += await CashDown(item.PaymentId, item.Value);
                    }
                }

                if (md.Expence != null)
                {
                    u += await CashDown(md.Expence.PaymentId, md.Expence.Value);
                    u += await BalanceProfitDown(md.Expence.Value);
                }
                if (md.Discount > 0)
                {
                    u += await BalanceProfitUp(md.Discount);
                }



                foreach (var item in md.ItemsList)
                    await StockUp(item.ProductId, item.StoreId, item.UnitId, item.UnitQuntity * item.UnitPrice, item.UnitQuntity);
                await IsBalanced();
                return u;
            }, CloseSideBar: false, SkipAlert: true);
        }





        public static async Task<bool> DeleteSupply(int IId)
        {
            return await ExecuteTRansActionAsync(async () =>
            {
                DeletedSupply dsup = new DeletedSupply();

                var md = await db.Supplys.AsTracking().Where(j => j.Id == IId).Include(v => v.Expence).SingleAsync();
                int u = 0;
                dsup.SupplierId = md.SupplierId;
                dsup.Discount = md.Discount;
                dsup.Total = md.Total;
                dsup.Expence = md.Expence;
                dsup.Paid = md.Paid;
                db.DeletedSupplys.Add(dsup);
                u += await db.SaveChangesAsync();


                var ItemsList = await db.SupplyItems.Where(n => n.SupplyId == IId).ToListAsync();
                foreach (var item in ItemsList)
                {
                    u += await StockDown(item.ProductId, item.StoreId, item.UnitId, item.UnitQuntity * item.UnitPrice, item.UnitQuntity);
                    dsup.ItemsList.Add(new DeletedSupplyItem()
                    {

                    });
                }
                var Pays = await db.supplyPayments.Where(n => n.SupplyId == IId).ToListAsync();
                foreach (var item in Pays)
                    u += await CashUp(item.PaymentId, item.Value);
                if (md.Expence != null)
                {
                    u += await CashUp(md.Expence.PaymentId, md.Expence.Value);
                    u += await BalanceProfitUp(md.Expence.Value);
                }
                if (md.Discount > 0)
                {
                    u += await BalanceProfitDown(md.Discount);
                }



                db.Supplys.Remove(md);

                u += await db.SaveChangesAsync();
                u += await SupplierTotalDown(md.SupplierId, md.Rest);




                await IsBalanced();

                return u;
            });
        }
        public static async Task<bool> UpdateSupplyDiscount(int IId, decimal val)
      => await ExecuteTRansActionAsync(async () =>
      {
          int u = 0;
          var md = await db.Supplys.AsTracking().Where(g => g.Id == IId).SingleAsync();
          var Val = KAfSubtract(val, md.Discount);
          md.Discount = val;
          if (md.Rest < 0)
          { throw new NegativeSupplyRestException(); }

          u += await db.SaveChangesAsync();

          if (Val > 0)
          {
              u += await SupplierTotalDown(md.SupplierId, Val);
              u += await BalanceProfitUp(Val);

          }
          else
          {
              Val = Math.Abs(Val);
              u += await SupplierTotalUp(md.SupplierId, Val);
              u += await BalanceProfitDown(Val);


          }
          await IsBalanced();
          return u;
      });
        public static async Task<bool> UpdateSupplyExpence(int IId, decimal val)
      => await ExecuteTRansActionAsync(async () =>
      {
          int u = 0;
          var md = await db.Supplys.AsTracking().Where(g => g.Id == IId).SingleAsync();
          var Val = KAfSubtract(val, md.ExpenceValue);

          if (md.ExpenceValue > 0)
          {
              var exd = await db.Expences.AsTracking().Where(g => g.Id == md.ExpenceId).SingleAsync();
              exd.Value = val;
          }
          md.ExpenceValue = val;
          u += await db.SaveChangesAsync();
          await IsBalanced();
          return u;
      });
        static async Task<int> SupplyPaidUp(int IId, decimal val)
        {
            int u = 0;
            var supply = await db.Supplys.AsTracking().SingleAsync(j => j.Id == IId);
            supply.Paid += val;
            if (supply.Rest < 0)
            { throw new NegativeSupplyRestException(); }
            u += await db.SaveChangesAsync();
            u += await SupplierTotalDown(supply.SupplierId, val);
            return u;
        }
        static async Task<int> SupplyPaidDown(int IId, decimal val)
        {
            int u = 0;
            var supply = await db.Supplys.AsTracking().SingleAsync(j => j.Id == IId);
            supply.Paid = KAfSubtract(supply.Paid, val);
            u += await db.SaveChangesAsync();
            u += await SupplierTotalUp(supply.SupplierId, val);
            return u;

        }




        #endregion





        #region CashInOut
        public static async Task<bool> AddNewCapitalDeposit(CapitalDeposit md)
        {
            return await ExecuteTRansActionAsync(async () =>
            {
                int u = 0;
                await db.CapitalDeposits.AddAsync(md);
                u += await db.SaveChangesAsync();
                u += await CashUp(md.PaymentId, md.Value);
                u += await PartnerCreditUp(md.PartnerId, md.Value);


                await IsBalanced();

                return u;
            }, true);
        }
        public static async Task<bool> AddNewCapitalWithdrawal(CapitalWithdrawal md)
        {
            return await ExecuteTRansActionAsync(async () =>
            {
                int u = 0;
                await db.CapitalWithdrawals.AddAsync(md);
                u += await db.SaveChangesAsync();
                u += await CashDown(md.PaymentId, md.Value);
                u += await PartnerCreditDown(md.PartnerId, md.Value);

                await IsBalanced();

                return u;
            }, true);
        }
        public static async Task<bool> DeleteCapitalDeposit(int IId)
        {
            return await ExecuteTRansActionAsync(async () =>
            {
                int u = 0;
                var md = await db.CapitalDeposits.Where(g => g.Id == IId).SingleAsync();
                db.CapitalDeposits.Remove(md);
                u += await db.SaveChangesAsync();
                u += await CashDown(md.PaymentId, md.Value);
                u += await PartnerCreditDown(md.PartnerId, md.Value);


                await IsBalanced();

                return u;
            });
        }
        public static async Task<bool> DeleteCapitalWithdrawal(int IId)
        {
            return await ExecuteTRansActionAsync(async () =>
            {
                int u = 0;
                var md = await db.CapitalWithdrawals.Where(g => g.Id == IId).SingleAsync();
                db.CapitalWithdrawals.Remove(md);
                u += await db.SaveChangesAsync();
                u += await CashUp(md.PaymentId, md.Value);
                u += await PartnerCreditUp(md.PartnerId, md.Value);

                await IsBalanced();

                return u;
            });
        }
        public static async Task<bool> AddNewSupplierPayment(SupplierPayment md)
        {
            return await ExecuteTRansActionAsync(async () =>
            {
                int u = 0;
                await db.supplierPayments.AddAsync(md);
                u += await db.SaveChangesAsync();
                u += await SupplierPaidUp(md.SupplierId, md.Value);
                await CashDown(md.PaymentId, md.Value);
                await IsBalanced();
                return u;
            }, true);
        }
        public static async Task<bool> DeleteSupplierPayment(int IId)
        {
            return await ExecuteTRansActionAsync(async () =>
            {
                int u = 0;
                var md = await db.supplierPayments.Where(g => g.Id == IId).SingleAsync();
                db.supplierPayments.Remove(md);
                u += await db.SaveChangesAsync();
                u += await SupplierPaidDown(md.SupplierId, md.Value);
                u += await CashUp(md.PaymentId, md.Value);
                await IsBalanced();
                return u;
            });
        }
        public static async Task<bool> UpdateSupplierPaymentValue(int IId, decimal val)
        {
            return await ExecuteTRansActionAsync(async () =>
            {
                int u = 0;
                var md = await db.supplierPayments.AsTracking().Where(g => g.Id == IId).SingleAsync();
                var Val = KAfSubtract(val, md.Value);
                md.Value = val;

                u += await db.SaveChangesAsync();
                if (Val > 0)
                {
                    u += await SupplierPaidUp(md.SupplierId, Val);
                    u += await CashDown(md.PaymentId, Val);

                }
                else
                {
                    Val = Math.Abs(Val);
                    u += await SupplierPaidDown(md.SupplierId, Val);
                    u += await CashUp(md.PaymentId, Val);


                }


                await IsBalanced();
                return u;
            });
        }


        public static async Task<bool> AddNewSupplyPayment(SupplyPayment md)
       => await ExecuteTRansActionAsync(async () =>
            {
                int u = 0;
                await db.supplyPayments.AddAsync(md);
                u += await db.SaveChangesAsync();
                u += await SupplyPaidUp(md.SupplyId, md.Value);
                u += await CashDown(md.PaymentId, md.Value);
                await IsBalanced();
                return u;
            }, true);
        public static async Task<bool> DeleteSupplyPayment(int IId)
       => await ExecuteTRansActionAsync(async () =>
            {
                int u = 0;
                var md = await db.supplyPayments.Where(g => g.Id == IId).SingleAsync();
                db.supplyPayments.Remove(md);
                u += await db.SaveChangesAsync();
                u += await SupplyPaidDown(md.SupplyId, md.Value);
                u += await CashUp(md.PaymentId, md.Value);
                await IsBalanced();
                return u;
            });

        public static async Task<bool> UpdateSupplyPaymentValue(int IId, decimal val)
       => await ExecuteTRansActionAsync(async () =>
            {
                int u = 0;
                var md = await db.supplyPayments.AsTracking().Where(g => g.Id == IId).SingleAsync();
                var Val = KAfSubtract(val, md.Value);
                md.Value = val;
                u += await db.SaveChangesAsync();

                if (Val > 0)
                {
                    u += await SupplyPaidUp(md.SupplyId, Val);
                    u += await CashDown(md.PaymentId, Val);

                }
                else
                {
                    Val = Math.Abs(Val);
                    u += await SupplyPaidDown(md.SupplyId, Val);
                    u += await CashUp(md.PaymentId, Val);


                }
                await IsBalanced();
                return u;
            });



        #endregion






        #endregion


        #region Stock













        #region Product
        public static async Task<bool> AddProduct(Product md)
        {


            return await ExecuteTRansActionAsync(async () =>
            {
                int u = 0;
                await db.Products.AddAsync(md);
                u += await db.SaveChangesAsync();
                var StoresIds = db.Stores.Select(s => s.Id).ToList();

                foreach (var s in StoresIds)
                {
                    await db.ProductInStores.AddAsync(new ProductInStore() { ProductId = md.Id, StoreId = s });

                }

                u += await db.SaveChangesAsync();



                return u;
            }, CloseSideBar: true);
        }
        public static async Task<List<Product>> GetAllProducts()
                    => await SelectAsync(() =>
                  db.Products.Where(o => !o.IsArchived).Include(u => u.Unit).OrderBy(u => u.Name).ToListAsync());
        public static async Task<ProductsViewData> GetProductsViewData()
        {
            return new ProductsViewData
            {
                RowProductslist = await GetAllProducts()
          ,
                RowCategorieslist = await GetAllCategories()
            };

        }
        public static async Task GetProductData(Product md)
        {

            md.ProductInStoreList = await db.ProductInStores.Where(k => k.ProductId == md.Id && k.Quntity > 0).Include(k => k.Store).ToListAsync();
            md.ProductUnitList = await db.Productsunits.Where(k => k.ProductId == md.Id).Include(k => k.Unit).ToListAsync();

        }
        public static async Task<bool> RemoveProduct(int IId)
                => await ExecuteTRansActionAsync(()
                    => db.Products.Where(p => p.Id == IId).ExecuteDeleteAsync());

        public static async Task<bool> UpdateProductName(int IId, string txt) =>
    await ExecuteTRansActionAsync(() =>
     db.Products.Where(p => p.Id == IId).ExecuteUpdateAsync(
         l => l.SetProperty(k => k.Name, txt)));
        public static async Task<bool> UpdateProductDescription(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Products.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Description, txt)));
        public static async Task<bool> UpdateProductCompany(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Products.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Company, txt)));
        public static async Task<bool> UpdateProdsuctCategory(int IId, int val) =>
         await ExecuteTRansActionAsync(() =>
          db.Products.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.CategoryId, val)));
        public static async Task<bool> UpdateProductMinimumQuntity(int IId, int val) =>
         await ExecuteTRansActionAsync(() =>
          db.Products.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.MinimumQuantity, val)));
        public static async Task<bool> ArchiveProduct(int IId) =>
        await ExecuteTRansActionAsync(() =>
         db.Products.Where(p => p.Id == IId).ExecuteUpdateAsync(
             l => l.SetProperty(k => k.IsArchived, true)));
        public static async Task<bool> RestoreProduct(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.Products.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsArchived, false)));

        public static async Task<bool> MarkUser(int IId) =>
        await ExecuteTRansActionAsync(() =>
         db.Products.Where(p => p.Id == IId).ExecuteUpdateAsync(
             l => l.SetProperty(k => k.IsFav, true)));
        public static async Task<bool> DeMarkUser(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.Products.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsFav, false)));





        static async Task<int> StockDown(int ProductId, int StoreId, int UnitId, decimal val, decimal UnitQuntity)
        {
            var Qun = (await db.Productsunits
                        .SingleAsync(t => t.ProductId == ProductId && t.UnitId == UnitId)).UnitValue * UnitQuntity;


            var prodinstore = await db.ProductInStores.AsTracking().SingleAsync(j => j.ProductId == ProductId && j.StoreId == StoreId);
            if (prodinstore.Quntity < Qun)
                throw new NoQuntityAvailableException();
            prodinstore.Quntity = KAfSubtract(prodinstore.Quntity, Qun);

            var prod = await db.Products.AsTracking().SingleAsync(j => j.Id == ProductId);
            if (prod.Quntity < Qun)
                throw new NoQuntityAvailableException();
            prod.Value = KAfSubtract(prod.Value, val);
            prod.Quntity = KAfSubtract(prod.Quntity, Qun);
            await BalanceStockDown(val);
            return await db.SaveChangesAsync();

        }
        static async Task<int> StockUp(int ProductId, int StoreId, int UnitId, decimal val, decimal UnitQuntity)
        {
            var Qun = (await db.Productsunits
                        .SingleAsync(t => t.ProductId == ProductId && t.UnitId == UnitId)).UnitValue * UnitQuntity;

            var prodinstore = await db.ProductInStores.AsTracking().SingleAsync(j => j.ProductId == ProductId && j.StoreId == StoreId);
            prodinstore.Quntity += Qun;
            var prod = await db.Products.AsTracking().SingleAsync(j => j.Id == ProductId);
            prod.Value += val;
            prod.Quntity += Qun;
            await BalanceStockUp(val);
            return await db.SaveChangesAsync();
        }

        #endregion

        #region Store
        public static async Task<bool> AddStore(Store md)
        {
            
            return await ExecuteTRansActionAsync(async () =>
            {
                Validate(md);
                int u = 0;
                await db.Stores.AddAsync(md);
                u += await db.SaveChangesAsync();
                var productsIds = db.Products.Select(s => s.Id).ToList();

                foreach (var s in productsIds)
                {
                    await db.ProductInStores.AddAsync(new ProductInStore() { ProductId = s, StoreId = md.Id });

                }

                u += await db.SaveChangesAsync();



                return u;
            }, CloseSideBar: true);
        }
        public static async Task GetStoreData(Store md)
        {

            md.ProductInStoreList = await db.ProductInStores.Where(k => k.StoreId == md.Id && k.Quntity > 0).Include(k => k.Product).ToListAsync();

        }
        public static async Task<List<Store>> GetAllStores()
       => await SelectAsync(() =>
     db.Stores.Where(o => !o.IsArchived).OrderBy(u => u.Name).ToListAsync());
        public static async Task<bool> RemoveStore(int IId)
                   => await ExecuteTRansActionAsync(()
                       => db.Stores.Where(p => p.Id == IId).ExecuteDeleteAsync());

        public static async Task<bool> UpdateStoreName(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Stores.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Name, txt)));

        public static async Task<bool> UpdateStoreAddress(int IId, string txt) =>
          await ExecuteTRansActionAsync(() =>
           db.Stores.Where(p => p.Id == IId).ExecuteUpdateAsync(
               l => l.SetProperty(k => k.Address, txt)));
        public static async Task<bool> UpdateStorePhone(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Stores.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Phone, txt)));
        public static async Task<bool> UpdateStoreNotes(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Stores.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Notes, txt)));
        public static async Task<bool> ArchiveStore(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.Stores.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsArchived, true)));
        public static async Task<bool> RestoreStore(int IId) =>
         await ExecuteTRansActionAsync(() =>
          db.Stores.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.IsArchived, false)));





        #endregion

        #region Unit
        public static async Task<bool> AddUnit(Unit md)
        {
            return await ExecuteTRansActionAsync(async () =>
            {
                await db.Units.AddAsync(md);
                return await db.SaveChangesAsync();
            }, CloseSideBar: true);
        }
        public static async Task<List<Unit>> GetAllUnits()
           => await SelectAsync(() =>
         db.Units.OrderBy(u => u.Name).ToListAsync());
        public static async Task<bool> RemoveUnit(int IId)
            => await ExecuteTRansActionAsync(()
            => db.Units.Where(p => p.Id == IId).ExecuteDeleteAsync());


        public static async Task<bool> UpdateUnitName(int IId, string txt) =>
         await ExecuteTRansActionAsync(() =>
          db.Units.Where(p => p.Id == IId).ExecuteUpdateAsync(
              l => l.SetProperty(k => k.Name, txt)));
        #endregion

        #region Category
        public static async Task<bool> AddCategory(Category md)
        => await ExecuteTRansActionAsync(async () =>
        {
            await db.Categories.AddAsync(md);
            return await db.SaveChangesAsync();
        }, CloseSideBar: true);

        public static async Task<List<Category>> GetAllCategories()
            => await SelectAsync(() =>
          db.Categories.OrderBy(u => u.Name).ToListAsync());
        public static async Task<bool> RemoveCategory(int IId)
   => await ExecuteTRansActionAsync(()
       => db.Categories.Where(p => p.Id == IId).ExecuteDeleteAsync());

        public static async Task<bool> UpdateCategoryName(int IId, string txt) =>
       await ExecuteTRansActionAsync(() =>
        db.Categories.Where(p => p.Id == IId).ExecuteUpdateAsync(
            l => l.SetProperty(k => k.Name, txt)));
        #endregion

        #endregion


        #region SearchGate
        public static async Task<SearchSupplysData> GetArchivedSupplys(int LastId)
        {

            return new SearchSupplysData()
            {
                Count = await SelectAsync(() =>
                  db.Supplys.Where(o => o.IsArchived && o.Id < LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Supplys.Where(o => o.IsArchived && o.Id < LastId).OrderByDescending(g => g.Id).Take(50).ToListAsync())
            };
        }
        public static async Task<SearchSupplysData> GetDeletedSupplys(int LastId)
        {

            return new SearchSupplysData()
            {
                Count = await SelectAsync(() =>
                  db.Supplys.Where(o => o.Id < LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Supplys.Where(o => o.Id < LastId).OrderByDescending(g => g.Id).Include(h => h.Session).Take(50).ToListAsync())
            };
        }
        public static async Task<SearchSupplysData> GetSupplyById(int IId)
        {

            return new SearchSupplysData()
            {
                Count = await SelectAsync(() =>
                  db.Supplys.Where(o => o.Id == IId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Supplys.Where(o => o.Id == IId).Include(h => h.Session).ToListAsync())
            };
        }
        public static async Task<SearchSupplysData> GetAllSupplys(int LastId)
        {

            return new SearchSupplysData()
            {
                Count = await SelectAsync(() =>
                  db.Supplys.Where(o => o.Id < LastId).CountAsync())

                ,
                RowItemlist = await SelectAsync(() =>
                  db.Supplys.Where(o => o.Id < LastId).OrderByDescending(g => g.Id).Include(h => h.Session).Take(50).ToListAsync())
            };
        }
        #endregion

    }
}
