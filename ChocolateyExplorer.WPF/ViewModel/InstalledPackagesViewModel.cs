﻿namespace ChocolateyExplorer.WPF.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Chocolatey.DomainModel;
    using Chocolatey.Manager;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;

    public class InstalledPackagesViewModel : ViewModelBase
    {
        private readonly IInstalledPackagesManager _packagesManager;
        private readonly IChocolateyInstaller _installer;
        private readonly ConsoleViewModel _console;

        private bool _isWorking;
        private string _statusMessage;
        private ChocolateyPackageVersion _selectedPackage;

        public InstalledPackagesViewModel(
            IInstalledPackagesManager packagesManager, 
            IChocolateyInstaller installer, 
            ConsoleViewModel console)
        {
            this._packagesManager = packagesManager;
            this._installer = installer;
            this._console = console;

            this.UninstallPackageCommand = new RelayCommand<ChocolateyPackageVersion>(async p => await this.UninstallPackage(p), p => p != null && !this.IsWorking);
            this.UpdatePackageCommand = new RelayCommand<ChocolateyPackageVersion>(async p => await this.UpdatePackage(p), p => p != null && !this.IsWorking);
            this.RefreshInstalledPackagesCommand = new RelayCommand(async () => await this.RefreshPackages(), () => !this.IsWorking);
            this.UpdateAllCommand = new RelayCommand(async () => await this.UpdateAllPackages(), () => !this.IsWorking);

            this.IsWorking = false;
            this.StatusMessage = "Ready";
            this.SelectedPackage = null;

            this.InstalledPackages = new ObservableCollection<ChocolateyPackageVersion>();
        }

        public ObservableCollection<ChocolateyPackageVersion> InstalledPackages { get; private set; }

        public bool CanSelectPackage
        {
            get
            {
                return !this.IsWorking;
            }
        }

        public bool IsWorking
        {
            get
            {
                return this._isWorking;
            }
            set
            {
                this._isWorking = value;
                
                this.RaisePropertyChanged(() => this.IsWorking);
                this.RaisePropertyChanged(() => this.CanSelectPackage);
                this.RefreshInstalledPackagesCommand.RaiseCanExecuteChanged();
                this.UpdatePackageCommand.RaiseCanExecuteChanged();
                this.UninstallPackageCommand.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get
            {
                return this._statusMessage;
            }
            set
            {
                this._statusMessage = value;

                this.RaisePropertyChanged(() => this.StatusMessage);
            }
        }

        public ChocolateyPackageVersion SelectedPackage
        {
            get
            {
                return this._selectedPackage;
            }
            set
            {
                this._selectedPackage = value;

                this.RaisePropertyChanged(() => this.SelectedPackage);
                this.UninstallPackageCommand.RaiseCanExecuteChanged();
                this.UpdatePackageCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand<ChocolateyPackageVersion> UninstallPackageCommand { get; private set; }

        public RelayCommand<ChocolateyPackageVersion> UpdatePackageCommand { get; private set; }

        public RelayCommand RefreshInstalledPackagesCommand { get; private set; }

        public RelayCommand UpdateAllCommand { get; private set; }

        public async Task RefreshPackages()
        {
            this.StatusMessage = "Loading Installed Packages";
            this.IsWorking = true;

            var installedPackages = await this._packagesManager.RetrieveInstalledPackages(true);

            this.InstalledPackages.Clear();

            foreach (var package in installedPackages)
            {
                this.InstalledPackages.Add(package);
            }

            this.IsWorking = false;
            this.StatusMessage = "Ready";
        }

        private async Task UninstallPackage(ChocolateyPackageVersion package)
        {
            this.IsWorking = true;
            this.StatusMessage = string.Format("Uninstalling {0}", package.Id);

            this._installer.OutputReceived += this.OutputReceived;

            await this._installer.Uninstall(package);

            this._installer.OutputReceived -= this.OutputReceived;

            await this.RefreshPackages();

            this.StatusMessage = "Ready";
            this.IsWorking = false;
        }

        private async Task UpdatePackage(ChocolateyPackageVersion package)
        {
            this.IsWorking = true;
            this.StatusMessage = string.Format("Updating {0}", package.Id);

            this._installer.OutputReceived += this.OutputReceived;

            await this._installer.Update(package);

            this._installer.OutputReceived -= this.OutputReceived;
            
            await this.RefreshPackages();

            this.StatusMessage = "Ready";
            this.IsWorking = false;
        }

        private async Task UpdateAllPackages()
        {
            this.IsWorking = true;
            this.StatusMessage = "Updating All Packages.  This may take some time...";

            this._installer.OutputReceived += this.OutputReceived;

            await this._installer.UpdateAll();

            this._installer.OutputReceived -= this.OutputReceived;

            await this.RefreshPackages();

            this.StatusMessage = "Ready";
            this.IsWorking = false;
        }

        private void OutputReceived(string obj)
        {
            this._console.AddConsoleLine(obj);
        }
    }
}