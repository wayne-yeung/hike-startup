
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamarinCRM.Extensions;
using XamarinCRM.Statics;
using XamarinCRM.ViewModels.Base;
using XamarinCRM.Models;
using XamarinCRM.Services;
using System;

namespace XamarinCRM
{
    public class SalesDashboardLeadsViewModel : BaseViewModel
    {
        IDataService _DataClient;

        Command _LoadSeedDataCommand;

        Command _LoadLeadsCommand;

        ObservableCollection<Account> _Leads;

        public Command PushTabbedLeadPageCommand { get; private set; }

        public bool NeedsRefresh { get; set; }

        public SalesDashboardLeadsViewModel(Command pushTabbedLeadPageCommand, INavigation navigation = null)
            : base(navigation)
        {
            PushTabbedLeadPageCommand = pushTabbedLeadPageCommand;

            _DataClient = DependencyService.Get<IDataService>();

            Leads = new ObservableCollection<Account>();

            MessagingCenter.Subscribe<Account>(this, MessagingServiceConstants.SAVE_ACCOUNT, (account) =>
                {
                    var index = Leads.IndexOf(account);
                    if (index >= 0)
                    {
                        Leads[index] = account;
                    }
                    else
                    {
                        Leads.Add(account);
                    }
                    Leads = new ObservableCollection<Account>(Leads.OrderBy(l => l.Company));
                });

            IsInitialized = false;
        }

        public Command LoadSeedDataCommand
        {
            get
            {
                return _LoadSeedDataCommand ?? (_LoadSeedDataCommand = new Command(async () => await ExecuteLoadSeedDataCommand()));
            }
        }

        /// <summary>
        /// Used for pull-to-refresh of Leads list
        /// </summary>
        /// <value>The load leads command, used for pull-to-refresh.</value>
        public Command LoadLeadsCommand
        { 
            get
            { 
                return _LoadLeadsCommand ?? (_LoadLeadsCommand = new Command(ExecuteLoadLeadsCommand, () => !IsBusy)); 
            } 
        }

        public async Task ExecuteLoadSeedDataCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            if (!_DataClient.IsSeeded)
            {
                await _DataClient.SeedLocalDataAsync();
            }

            var rnd = new Random();
            Leads = (await _DataClient.GetAccountsAsync(true)).ToObservableCollection();
            string[] names = 
            {
                "iShares Core S&P 500 UCITS ETF",
                "iShares NASDAQ 100 UCITS ETF",
                "iShares US Equity Buyback Achievers UCITS ETF",
                "iShares STOXX Europe 600 Banks UCITS ETF (DE)",
                "iShares Select Dividend ETF",
                "iShares Core High Dividend ETF",
                "iShares Core S&P U.S. Value ETF",
                "iShares MSCI Turkey ETF",
                "iShares MSCI Netherlands ETF",
                "iShares MSCI Ireland Capped ETF",
                "iShares Asia/Pacific Dividend ETF"
            };

            foreach (var lead in Leads)
            {
                lead.Company = names[rnd.Next(0, 9)];
            }

            IsInitialized = true;
            IsBusy = false;
        }

        /// <summary>
        /// Executes the LoadLeadsCommand.
        /// </summary>
        public async void ExecuteLoadLeadsCommand()
        { 
            if (IsBusy)
                return; 

            IsBusy = true;
            LoadLeadsCommand.ChangeCanExecute(); 

            Leads.Clear();
            Leads.AddRange(await _DataClient.GetAccountsAsync(true));

            IsBusy = false;
            LoadLeadsCommand.ChangeCanExecute(); 
        }

        public ObservableCollection<Account> Leads
        {
            get { return _Leads; }
            set
            {
                _Leads = value;
                OnPropertyChanged("Leads");
            }
        }
    }
}

