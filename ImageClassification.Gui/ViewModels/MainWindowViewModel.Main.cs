﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Wkiro.ImageClassification.Core.Facades;
using Wkiro.ImageClassification.Core.Infrastructure.Logging;
using Wkiro.ImageClassification.Core.Models.Configurations;
using Wkiro.ImageClassification.Core.Models.Dto;
using Wkiro.ImageClassification.Gui.Configuration;
using Wkiro.ImageClassification.Gui.Infrastructure;
using Application = System.Windows.Application;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace Wkiro.ImageClassification.Gui.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, ILogger
    {
        private const string SavedNetworkFileExtension = "dbn";

        private LearningFacade _learningFacade;
        private ClassifierFacade _classifierFacade;

        private readonly IConfigurationManager _configurationManager;

        public MainWindowViewModel(bool isNotDesignMode)
        {
            _configurationManager = new HardcodedConfigurationManager();

            InitializeCommands();
            DataProviderConfiguration = _configurationManager.GetInitialDataProviderConfiguration();
        }

        private void ConfigureNewTraining()
        {
            GlobalTrainerConfiguration = _configurationManager.GetInitialGlobalTrainerConfiguration();
            Training1Parameters = _configurationManager.GetInitialTraining1Parameters();
            Training2Parameters = _configurationManager.GetInitialTraining2Parameters();

            var learningFacade = new LearningFacade(DataProviderConfiguration, GlobalTrainerConfiguration, this);
            AvailableCategories = new ObservableCollection<Category>(learningFacade.GetAvailableCategories());
        }

        #region Load / save network

        private void LoadSavedNetwork()
        {
            var fileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Title = "Load network from selected save file",
                Multiselect = false,
                RestoreDirectory = true,
                DefaultExt = SavedNetworkFileExtension,
                AddExtension = true,
                Filter = $"Deep network file (*.{SavedNetworkFileExtension})|*.{SavedNetworkFileExtension}",
            };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            var fileName = fileDialog.FileName;
            var classifierConfiguration = new ClassifierConfiguration
            {
                Categories = SelectedCategories,
            };

            _classifierFacade = new ClassifierFacade(
                savedNetworkPath:          fileName, 
                dataProviderConfiguration: _dataProviderConfiguration, 
                classifierConfiguration:   classifierConfiguration, 
                logger:                    this);
        }

        private void SaveNetwork()
        {
            var dateTimeFormatted = DateTime.Now.ToString("yyyyMMdd.HHmmss");

            var fileDialog = new SaveFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Title = "Save trained network file",
                RestoreDirectory = true,
                AddExtension = true,
                Filter = $"Deep network file (*.{SavedNetworkFileExtension})|*.{SavedNetworkFileExtension}",
                DefaultExt = SavedNetworkFileExtension,
                FileName = $"Deep_{dateTimeFormatted}.{SavedNetworkFileExtension}",
            };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            var fileName = fileDialog.FileName;
            _classifierFacade.SaveClassifier(fileName);
        }

        #endregion

        private async void StartTraining()
        {
            _learningFacade = new LearningFacade(DataProviderConfiguration, GlobalTrainerConfiguration,  this);
            var categories = SelectedCategories.Select((x, i) =>
            {
                x.Index = i;
                return x;
            });

            var trainingParameters = new TrainingParameters
            {
                Training1Parameters = Training1Parameters,
                Training2Parameters = Training2Parameters,
                SelectedCategories = categories,
            };

            var task = _learningFacade.RunTrainingForSelectedCategories(trainingParameters);
            _classifierFacade = await task;
        }

        private void ClassifyImage()
        {
            var fileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Title = "Select file to classify",
                Multiselect = false,
                Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif",
                RestoreDirectory = true,
            };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            var fileName = fileDialog.FileName;
            var classification = _classifierFacade.ClassifyToCategory(fileName);
            LogWriteLine($"Classified to category: {classification.Category} with {classification.Probability} probability.");
        }

        public void LogWriteLine(string logMessage)
        {
            const string separatorPattern = "-";
            const int separatorPatternMultiplier = 10;

            var date = DateTime.Now.ToString("HH:mm:ss.fff");
            var separator = Enumerable.Repeat(separatorPattern, separatorPatternMultiplier).Aggregate((x, y) => x + y);

            Application.Current.Dispatcher.Invoke(() =>
            {
                OutputTextBoxContent += $"{date}\n{logMessage}\n{separator}\n";
            });
        }
    }
}
