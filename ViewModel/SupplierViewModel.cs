using ClientShared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KafApp.Models;
using KafApp.Repo;
using KafApp.Reports;
using KafApp.View;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;

namespace KafApp.ViewModels
{

    public partial class SupplierViewModel(Supplier md, SuppliersViewModel ParentVm) : PersonViewModel<Supplier>(md), IPayableInvoice
    {
        public Supplier Md => md;
        //public bool CanDelete { get { return Helper.currentSesstion.DeleteSupplier && md.Credit==0; } }
        //public bool CanEdit { get { return Helper.currentSesstion.EditSupplier; } }

        public ObservableCollection<Supply> SupplysList = new();
        public ObservableCollection<SupplierReturn> SupplierReturnsList = new();
        public ObservableCollection<SupplierPayment> SupplierPaymentList = new();


        public bool IsFav { get => md.IsFav; set { md.IsFav = value; OnPropertyChanged(); } }

        public long Paid { get => md.Paid; set { md.Paid = value; OnPropertyChanged(); } }
        public long SupplyTotal { get => md.SupplyTotal; set { md.SupplyTotal = value; OnPropertyChanged(); } }
        public long ConstCredit { get => md.ConstCredit; set { md.ConstCredit = value; OnPropertyChanged(); } }
        public int MaxCredit { get => md.MaxCredit; set { md.MaxCredit = value; OnPropertyChanged(); } }
        public long SupplierReturnTotal { get => md.SupplierReturnTotal; set { md.SupplierReturnTotal = value; OnPropertyChanged(); } }
        public long Total { get => md.Credit; set { OnPropertyChanged(); } }



        KafSearchOption option = new() { ParentId = md.Id, ExecptArchived = true };
        public override async void DisplayUI()
        {
            base.DisplayUI();
            await RefreshData();
            NavigationService.PushParent(new SupplierView(this));

        }
        async Task RefreshData()
        {
            md.SupplysList = await AccessService.GetSupplys(option);
            md.SupplierReturnsList = await AccessService.GetSupplierReturns(option);
            md.SupplierPaymentsList = await AccessService.GetPayments<SupplierPayment>(option);
            md.SupplysList.ToObservable(SupplysList);
            md.SupplierReturnsList.ToObservable(SupplierReturnsList);
            md.SupplierPaymentsList.ToObservable(SupplierPaymentList);
            ReCalculate();

        }
        internal void ReCalculate(long AddConstCredit = 0)
        {

            ConstCredit += AddConstCredit;
            SupplyTotal = SupplysList.Select(g => g.Rest).Sum();
            SupplierReturnTotal = SupplierReturnsList.Select(g => g.Rest).Sum();
            Paid = SupplierPaymentList.Select(g => g.Amount).Sum();
            Total = 0;

        }

        #region Editing


        protected override void DeleteMe()
        {
            base.DeleteMe();
            ParentVm.RemoveFromList(md);
            NavigationService.CloseSideBar();
        }

        [RelayCommand]
        private async void AddNewSupply()
        {
            NewSupplyViewModel vm = new NewSupplyViewModel();
            vm.DisplayUI(this);
        }
        [RelayCommand]
        private async void AddNewSupplierReturn()
        {
            NewSupplierReturnViewModel vm = new NewSupplierReturnViewModel(this);
            vm.DisplayUI();
        }




        [RelayCommand]
        private async void updateName()
        {
            var namesArray = ParentVm.Supplierslist.Select(g => g.Name).ToArray();
            await base.UpdateName(namesArray);

        }
        [RelayCommand]
        private async void changefav()
        {
            if (IsFav)
            {
                if (await AccessService.DeMark<Supplier>(Id))
                    IsFav = false;
            }
            else
            {
                if (await AccessService.Mark<Supplier>(Id))
                    IsFav = true;
            }

        }
        [RelayCommand]
        private async void updateMaxCredit()
        {
            if (Helper.GetInteger(out int nn))
            {
                if (await AccessService.UpdateSupplierMaxCredit(Id, nn))
                    MaxCredit = nn;

            }
        }

        #endregion






        [RelayCommand]
        internal void ShowPayment(object md)
        {
            // var vmd = new paymen((SupplyPayment)md);
            // vmd.Deleted +=async ()=>await RefreshData();
            // vmd.DataChanged += ReCalculate;
            //vmd.Show();
        }
        [RelayCommand]
        internal void ShowInvoice(object md)
        {
            var vmd = new SupplyViewModel((Supply)md);
            vmd.Deleted += async () => await RefreshData();
            vmd.DataChanged += ReCalculate;
            vmd.Show();
        }
        [RelayCommand]
        internal void ShowReturn(object md)
        {
            var vmd = new SupplierReturnViewModel((SupplierReturn)md);
            vmd.Deleted += async () => await RefreshData();
            vmd.DataChanged += ReCalculate;
            vmd.Show();
        }
        [RelayCommand]
        private void GenerateReport()
        {
            var report = new SupplierReport(md);
            report.ShowReport();
        }
        [RelayCommand]
        private void AddNewPayment()
        {
            BaseNewPaymentViewModel vmd = new BaseNewPaymentViewModel(this);
            vmd.Show();
        }
        public async Task<bool> AddPayment(BasePayment pay)
        {

            SupplierPayment payment = new SupplierPayment();
            payment.Amount = pay.Amount;
            payment.Notes = pay.Notes;
            payment.ReferenceNumber = pay.ReferenceNumber;
            payment.PaymentId = pay.PaymentId;
            payment.SessionId = Helper.currentSesstion.Session.Id;
            payment.SupplierId = Id;
            var result = await AccessService.AddNewSupplierPayment(payment);
            if (result.IsQueryCompleted)
            {
                var savedmd = result.GetObject<SupplierPayment>();


                SupplierPaymentList.Add(savedmd);
                md.SupplierPaymentsList.Add(savedmd);

                ReCalculate();

                return true;
            }
            return false;
        }
        internal void Add(SupplierPayment mmd)
        {
            SupplierPaymentList.Add(mmd);
            md.SupplierPaymentsList.Add(mmd);
            ReCalculate();
        }

        internal async void EditPayment(SupplierPayment pay)
        {
            if (Helper.GetDecimal(out long nn, max: (md.Credit + pay.Amount)))
            {
                if (await AccessService.UpdateSupplierPaymentValue(pay.Id, nn))
                {
                    pay.Amount = nn;
                    ReCalculate();
                    NotificationService.DoneAlert();
                }
            }
        }
        internal async void ArchivePayment(SupplierPayment pay)
        {
            if (Helper.CanArchive())
                if (await AccessService.ArchiveSupplierPayment(pay.Id))
                {
                    ConstCredit = ConstCredit.KAfSubtract(pay.Amount);
                    md.SupplierPaymentsList.Remove(pay);
                    ReCalculate();
                    NotificationService.DoneAlert();
                }
        }
        internal async Task DeletaPaymentAsync(SupplierPayment pay)
        {
            if (Helper.CanDelete())
                if (await AccessService.DeleteSupplierPayment(pay.Id))
                {
                    md.SupplierPaymentsList.Remove(pay);
                    SupplierPaymentList.Remove(pay);
                    ReCalculate();
                    NotificationService.DoneAlert();
                }
        }
    }
}
