﻿using System.Collections.Generic;
using Wkiro.ImageClassification.Core.Engines;
using Wkiro.ImageClassification.Core.Infrastructure.Logging;
using Wkiro.ImageClassification.Core.Models.Configurations;
using Wkiro.ImageClassification.Core.Models.Dto;

namespace Wkiro.ImageClassification.Core.Facades
{
    public class ClassifierFacade 
    {
        private readonly DataProvider _dataProvider;
        private readonly Classifier _classifier;

        internal ClassifierFacade(DataProvider dataProvider, Classifier classifier)
        {
            _classifier = classifier;
            _dataProvider = dataProvider;
        }

        public ClassifierFacade(
            string savedNetworkPath, 
            DataProviderConfiguration dataProviderConfiguration, 
            IGuiLogger logger)
        {
            _dataProvider = new DataProvider(dataProviderConfiguration);
            _classifier = new Classifier(savedNetworkPath, logger);
        }

        public IEnumerable<Category> GetAvailableCategories()
        {
            return _dataProvider.GetAvailableCategories();
        }

        public CategoryClassification ClassifyToCategory(string imageToClassifyPath, ClassifierConfiguration configuration)
        {
            var preparedImage = _dataProvider.PrepareImageByPath(imageToClassifyPath);
            var classifiedCategory = _classifier.ClassifyToCategory(preparedImage, configuration);
            
            return classifiedCategory;
        }

        public void SaveClassifier(string saveLocationFilePath)
        {
            _classifier.SaveClassifier(saveLocationFilePath);
        }
    }
}
