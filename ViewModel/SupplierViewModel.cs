using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KafApp.Models;
using KafApp.Repo;
using KafApp.View;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace KafApp.ViewModels
{
    
    public partial class SupplierViewModel(Supplier md) : ObservableObject
    {
        public bool CanDelete { get { return Helper.currentSesstion.DeleteSupplier && md.Credit==0; } }
        public bool CanEdit { get { return Helper.currentSesstion.EditSupplier; } }

        public ObservableCollection<Supply> SupplysList;
        public ObservableCollection<SupplierReturn> SupplierReturnsList;
        public ObservableCollection<SupplierPayment> SupplierPaymentList;

        public string Name { get => md.Name; set { md.Name = value; OnPropertyChanged(); } }
        public string Address { get => md.Address; set { md.Address = value; OnPropertyChanged(); } }
        public string Mail { get => md.Mail; set { md.Mail = value; OnPropertyChanged(); } }
        public string Phone { get => md.Phone; set { md.Phone = value; OnPropertyChanged(); } }
        public string Notes { get => md.Notes; set { md.Notes = value; OnPropertyChanged(); } }
        public int Id { get => md.Id; set { md.Id = value; OnPropertyChanged(); } }
        public bool IsArchived { get => md.IsArchived; set { md.IsArchived = value; OnPropertyChanged(); } }
        public bool IsFav { get => md.IsFav; set { md.IsFav = value; OnPropertyChanged(); } }
       
        
        public decimal Paid { get => md.Paid; set { md.Paid = value; OnPropertyChanged(); } }
        public decimal SupplyTotal { get => md.SupplyTotal; set { md.SupplyTotal = value; OnPropertyChanged(); } }
        public decimal Perior { get => md.Perior; set { md.Perior = value; OnPropertyChanged(); } }
        public decimal SupplierReturnTotal { get => md.SupplierReturnTotal; set { md.SupplierReturnTotal = value; OnPropertyChanged(); } }
        public decimal Total { get => md.Credit; set { OnPropertyChanged(); } }

       


        public delegate void DeletedHandler();
        public event DeletedHandler Deleted;

        [RelayCommand]
        private async void show()
        {
            SupplysList = new();
            SupplierReturnsList = new();
            SupplierPaymentList = new();
            AppNavigator.ShowSupplierView(this);
            RefreshData();
        }
        async void RefreshData()
        {
            await Access.GetSupplierData(md);
            ReCalculate();

        }
        internal void ReCalculate()
        {
            md.SupplysList.ToObservable(SupplysList);
            md.SupplierReturnsList.ToObservable(SupplierReturnsList);
            md.SupplierPaymentsList.ToObservable(SupplierPaymentList);
            SupplyTotal = SupplysList.Select(g => g.Rest).Sum();
            SupplierReturnTotal = SupplierReturnsList.Select(g => g.Rest).Sum();
            Paid = SupplierPaymentList.Select(g => g.Value).Sum();
            Total = 0;

        }

        #region Editing


        void DeleteMe()
        {
           
            SuppliersViewModel.Instance.RemoveFromList(md);
            Deleted?.Invoke();
        }

        [RelayCommand]
        private async void delete()
        {
            if (await Access.RemoveSupplier(Id))
                DeleteMe();
               

        }


        [RelayCommand]
        private async void updateName()
        {
            var namesArray = SuppliersViewModel.Instance.Supplierslist.Select(g => g.Name).ToArray();

            if (Helper.GetNonNullableNormalText( out string nn, namesArray))
            {
                if (await Access.UpdateSupplierName(Id,nn))
                    Name = nn;
            }
        }

        [RelayCommand]
        private async void updateEmail()
        {
            if (Helper.GetNullableNormalText( out string nn))
            {
                if (await Access.UpdateSupplierMail(Id, nn))
                    Mail = nn;
            }
        }



        [RelayCommand]
        private async void updatephone()
        {
            if (Helper.GetInteger( out int nn))
            {
                if (await Access.UpdateSupplierPhone(Id, nn.ToString()))
                     Phone = nn.ToString();
                
            }
        }
        [RelayCommand]
        private async void updateaddress()
        {
            if (Helper.GetNullableNormalText( out string nn))
            {
                if (await Access.UpdateSupplierAddress(Id, nn))
                    Address = nn;
            }
        }
        [RelayCommand]
        private async void updatenotes()
        {
            if (Helper.GetNullableFullText( out string nn))
            {
                if (await Access.UpdateSupplierNotes(Id, nn))
                     Notes = nn;
            }
        }



      

        [RelayCommand]
        private async void archive()
        {
            if (await Access.ArchiveSupplier(Id))
            {
                IsArchived = true;
                DeleteMe();
            }
            

        }
        [RelayCommand]
        private async void restore()
        {
            if(await Access.RestoreSupplier(Id))
            {
                IsArchived = true;
                DeleteMe();
            }
        }
        [RelayCommand]
        private async void changefav()
        {
            if (IsFav)
            {
                if (await Access.DeMarkSupplier(Id))
                    IsFav = false;
            }
            else
            {
                if (await Access.MarkSupplier(Id))
                    IsFav = true;
            }
        }
        #endregion
        [RelayCommand]
        private void updateview()
           => AppNavigator.ShowUpdateSupplierView(this);

        [RelayCommand]
        private void generateReport()
           => Helper.Report(md);


        [RelayCommand]
        private async void AddNew()
        {
            if (await Access.AddSupplier(md))
                    SuppliersViewModel.Instance.AddToList(md); 
        }

        internal void ShowSupply(Supply md)
        {
            var vmd = new SupplyViewModel(md);
            vmd.Deleted += RefreshData;
            vmd.DataChanged += ReCalculate;
            AppNavigator.ShowSupply(vmd);
        }
        [RelayCommand]
        private void AddNewPayment()
        {
            var vmd = new NewSupplierPaymentViewModel(this);
            
            AppNavigator.AddNewSupplyPayment(vmd);
        }

        internal void Add(SupplierPayment mmd)
        {
            SupplierPaymentList.Add(mmd);
            md.SupplierPaymentsList.Add(mmd);
            ReCalculate();  
        }

        internal async void EditPayment(SupplierPayment pay)
        {
            if (Helper.GetDecimal(out decimal nn,max:(md.Credit+pay.Value)))
            {
                if (await Access.UpdateSupplierPaymentValue(pay.Id, nn))
                {
                    pay.Value= nn;
                    ReCalculate();
                }
            }
        }

        internal async Task DeletaPaymentAsync(SupplierPayment pay)
        {
            if (await Access.DeleteSupplierPayment(pay.Id))
            {
                md.SupplierPaymentsList.Remove(pay);
                SupplierPaymentList.Remove(pay);
                ReCalculate();
            }
        }
    } 
    }
