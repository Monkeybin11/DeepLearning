﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using Wkiro.ImageClassification.Core.Models.Dto;
using Wkiro.ImageClassification.Gui.Infrastructure;

namespace Wkiro.ImageClassification.Gui.ViewModels
{
    public partial class MainWindowViewModel
    {
        public RelayCommand BrowseForTrainFilesPathCommand { get; set; }
        public RelayCommand ConfigureNewTrainingCommand { get; set; }
        public RelayCommand LoadTrainingDataCommand { get; set; }
        public RelayCommand<object> SelectedCategoriesChangedCommand { get; set; }
        public RelayCommand StartTrainingCommand { get; set; }

        private void InitializeCommands()
        {
            BrowseForTrainFilesPathCommand = new RelayCommand(BrowseForTrainFilesPath);
            ConfigureNewTrainingCommand = new RelayCommand(ConfigureNewTraining);
            LoadTrainingDataCommand = new RelayCommand(LoadTrainingData);
            SelectedCategoriesChangedCommand = new RelayCommand<object>(SelectedCategoriesChanged);
            StartTrainingCommand = new RelayCommand(StartTraining);
        }

        private void BrowseForTrainFilesPath()
        {
            var directory = _dataProviderConfiguration.TrainFilesLocationPath;
            var dialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.DesktopDirectory,
                Description = "Select directory containing training folders",
                ShowNewFolderButton = false
            };

            if (Directory.Exists(directory))
                dialog.SelectedPath = directory;

            if (dialog.ShowDialog() == DialogResult.OK)
                _dataProviderConfiguration.TrainFilesLocationPath = dialog.SelectedPath;
        }

        private void SelectedCategoriesChanged(object categories)
        {
            var casted = (IList) categories;
            var casted2 = casted.Cast<Category>();
            SelectedCategories = new ObservableCollection<Category>(casted2);
        }
    }
}
