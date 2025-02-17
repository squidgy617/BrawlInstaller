using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlInstaller.Services;
using BrawlInstaller.StaticClasses;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static BrawlInstaller.ViewModels.MainControlsViewModel;

namespace BrawlInstaller.ViewModels
{
    public interface IFighterInfoViewModel
    {

    }

    [Export(typeof(IFighterInfoViewModel))]
    internal class FighterInfoViewModel : ViewModelBase, IFighterInfoViewModel
    {
        // Private properties
        private ObservableCollection<FighterInfo> _fighterInfoList;
        private FighterInfo _selectedFighterInfo;

        // Services
        IFighterService _fighterService { get; }
        ISettingsService _settingsService { get; }

        // Commands
        public ICommand AddFighterCommand => new RelayCommand(param => AddFighter());
        public ICommand RemoveFighterCommand => new RelayCommand(param => RemoveFighter());
        public ICommand RefreshFightersCommand => new RelayCommand(param => GetFighters());
        public ICommand LoadFightersCommand => new RelayCommand(param => LoadFighters());
        public ICommand SaveFightersCommand => new RelayCommand(param => SaveFighters());
        public ICommand MoveUpCommand => new RelayCommand(param => MoveUp());
        public ICommand MoveDownCommand => new RelayCommand(param => MoveDown());

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public FighterInfoViewModel(IFighterService fighterService, ISettingsService settingsService)
        {
            _fighterService = fighterService;
            _settingsService = settingsService;

            GetFighters();

            WeakReferenceMessenger.Default.Register<UpdateFighterListMessage>(this, (recipient, message) =>
            {
                UpdateFighterList(message);
            });

            WeakReferenceMessenger.Default.Register<UpdateSettingsMessage>(this, (recipient, message) =>
            {
                UpdateSettings();
            });

            WeakReferenceMessenger.Default.Register<SettingsSavedMessage>(this, (recipient, message) =>
            {
                SaveFighters();
            });

            WeakReferenceMessenger.Default.Register<SettingsLoadedMessage>(this, (recipient, message) =>
            {
                GetFighters();
            });

            WeakReferenceMessenger.Default.Register<LoadDefaultSettingsMessage>(this, (recipient, message) =>
            {
                ApplyDefaultSetting(message.Value);
            });
        }

        // Properties
        public ObservableCollection<FighterInfo> FighterInfoList { get => _fighterInfoList; set { _fighterInfoList = value; OnPropertyChanged(nameof(FighterInfoList)); } }

        [DependsUpon(nameof(FighterInfoList))]
        public FighterInfo SelectedFighterInfo { get => _selectedFighterInfo; set { _selectedFighterInfo = value; OnPropertyChanged(nameof(SelectedFighterInfo)); } }

        public Dictionary<string, int> FighterEffectPacs { get => EffectPacs.FighterEffectPacs; }

        [DependsUpon(nameof(SelectedFighterInfo))]
        public int? SelectedFighterEffectPac { get => SelectedFighterInfo?.EffectPacId; set { SelectedFighterInfo.EffectPacId = value; OnPropertyChanged(nameof(SelectedFighterEffectPac)); } }

        [DependsUpon(nameof(SelectedFighterInfo))]
        public int? SelectedKirbyEffectPac { get => SelectedFighterInfo?.KirbyEffectPacId; set { SelectedFighterInfo.KirbyEffectPacId = value; OnPropertyChanged(nameof(SelectedKirbyEffectPac)); } }

        [DependsUpon(nameof(SelectedFighterInfo))]
        public string FighterFileName { get => SelectedFighterInfo?.FighterFileName; set { SelectedFighterInfo.FighterFileName = value; UpdateFighterName(); OnPropertyChanged(nameof(FighterFileName)); } }

        // Methods
        private void AddFighter()
        {
            var newFighter = new FighterInfo 
            { 
                Ids = new BrawlIds 
                { 
                    FighterConfigId = 0,
                    SlotConfigId = 0,
                    CSSSlotConfigId = 0,
                    CosmeticConfigId = 0,
                    CosmeticId = 0,
                    FranchiseId = 0
                }
            };
            FighterInfoList.Add(newFighter);
            SelectedFighterInfo = newFighter;
            OnPropertyChanged(nameof(FighterInfoList));
            OnPropertyChanged(nameof(SelectedFighterInfo));
        }

        private void RemoveFighter()
        {
            FighterInfoList.Remove(SelectedFighterInfo);
            OnPropertyChanged(nameof(FighterInfoList));
            OnPropertyChanged(nameof(SelectedFighterInfo));
        }

        private void GetFighters()
        {
            var list = _settingsService.LoadFighterInfoSettings();
            _settingsService.FighterInfoList = list;
            FighterInfoList = new ObservableCollection<FighterInfo>(list.Copy());
            OnPropertyChanged(nameof(FighterInfoList));
            OnPropertyChanged(nameof(SelectedFighterInfo));
        }

        private void LoadFighters()
        {
            var currentFighterList = FighterInfoList.ToList();
            var newFighterList = new List<FighterInfo>();
            var fighterList = _fighterService.GetAllFighterInfo();
            // Get only fighters where all configs could be found
            var completeFighters = fighterList.Where(x => x.FighterConfig != "" && x.CosmeticConfig != "" && x.CSSSlotConfig != "" && x.SlotConfig != "").ToList();
            foreach (var fighter in completeFighters)
            {
                // Remove attributes
                fighter.FighterAttributes = null;
                fighter.CosmeticAttributes = null;
                fighter.SlotAttributes = null;
                fighter.CSSSlotAttributes = null;

                // Only add fighters if there is no entry with the same fighter ID
                var fighterMatch = currentFighterList.FirstOrDefault(x => x.Ids.FighterConfigId == fighter.Ids.FighterConfigId
                && x.Ids.SlotConfigId == fighter.Ids.SlotConfigId && x.Ids.CSSSlotConfigId == fighter.Ids.CSSSlotConfigId
                && x.Ids.CosmeticConfigId == fighter.Ids.CosmeticConfigId);

                if (fighterMatch == null)
                {
                    newFighterList.Add(fighter);
                }
                // Otherwise, replace the match
                else
                {
                    // Set fields that aren't grabbed from GetAllFighterInfo
                    fighter.EffectPacId = fighterMatch.EffectPacId;
                    fighter.KirbyEffectPacId = fighterMatch.KirbyEffectPacId;
                    fighter.Ids.MasqueradeId = fighterMatch.Ids.MasqueradeId;
                    fighter.CreditsThemeId = fighterMatch.CreditsThemeId;
                    currentFighterList.Insert(currentFighterList.IndexOf(fighterMatch), fighter);
                    currentFighterList.Remove(fighterMatch);
                }
            }
            currentFighterList.AddRange(newFighterList.OrderBy(x => x.Ids.FighterConfigId));
            FighterInfoList = new ObservableCollection<FighterInfo>(currentFighterList);
            OnPropertyChanged(nameof(FighterInfoList));
        }

        private void SaveFighters() 
        {
            _settingsService.SaveFighterInfoSettings(FighterInfoList.ToList());
            WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
        }

        private void MoveUp()
        {
            var selected = SelectedFighterInfo;
            FighterInfoList.MoveUp(SelectedFighterInfo);
            SelectedFighterInfo = selected;
            OnPropertyChanged(nameof(FighterInfoList));
        }

        private void MoveDown()
        {
            var selected = SelectedFighterInfo;
            FighterInfoList.MoveDown(SelectedFighterInfo);
            SelectedFighterInfo = selected;
            OnPropertyChanged(nameof(FighterInfoList));
        }

        private void UpdateFighterList(UpdateFighterListMessage message)
        {
            var selectedIndex = FighterInfoList.IndexOf(SelectedFighterInfo);
            FighterInfoList = new ObservableCollection<FighterInfo>(message.Value.Copy());
            OnPropertyChanged(nameof(FighterInfoList));
            if (selectedIndex > -1 && FighterInfoList.Count > selectedIndex)
            {
                SelectedFighterInfo = FighterInfoList[selectedIndex];
                OnPropertyChanged(nameof(SelectedFighterInfo));
            }
        }

        private void UpdateSettings()
        {
            GetFighters();
            OnPropertyChanged(nameof(FighterInfoList));
            WeakReferenceMessenger.Default.Send(new UpdateFighterListMessage(_settingsService.FighterInfoList));
        }

        public void UpdateFighterName()
        {
            if (SelectedFighterInfo?.FighterFileName != null)
            {
                var fileName = SelectedFighterInfo?.FighterFileName;
                SelectedFighterInfo.FullPacFileName = $"{fileName.ToLower()}/Fit{fileName}.pac";
                SelectedFighterInfo.FullKirbyPacFileName = $"kirby/FitKirby{fileName}.pac";
                SelectedFighterInfo.ModuleFileName = $"ft_{fileName.ToLower()}.rel";
                SelectedFighterInfo.InternalName = fileName.ToUpper();
                OnPropertyChanged(nameof(SelectedFighterInfo));
            }
        }

        private void ApplyDefaultSetting(string selectedOption)
        {
            var json = GetSelectedSettings("FighterList.json", selectedOption);
            FighterInfoList = JsonConvert.DeserializeObject<ObservableCollection<FighterInfo>>(json);
            OnPropertyChanged(nameof(FighterInfoList));
        }

        private string GetSelectedSettings(string file, string selectedOption)
        {
            var json = string.Empty;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"BrawlInstaller.Resources.DefaultSettings.{selectedOption}.{file}"))
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {
                    json = streamReader.ReadToEnd();
                }
            }
            return json;
        }
    }
}
