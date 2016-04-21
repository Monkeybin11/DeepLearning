﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Accord.Imaging.Converters;
using Wkiro.ImageClassification.Core.Models.Configurations;
using Wkiro.ImageClassification.Core.Models.Dto;

namespace Wkiro.ImageClassification.Core.Engines
{
    internal class DataProvider
    {
        private readonly DataProviderConfiguration _dataProviderconfiguration;
        private readonly GlobalTrainerConfiguration _globalTrainerConfiguration;
        private readonly ImageToArray _imageToArray;

        public DataProvider(DataProviderConfiguration dataProviderconfiguration)
        {
            _dataProviderconfiguration = dataProviderconfiguration;
            _imageToArray = new ImageToArray(min: 0, max: 1);
        }

        public DataProvider(
            DataProviderConfiguration dataProviderConfiguration,
            GlobalTrainerConfiguration globalTrainerConfiguration)
            : this(dataProviderConfiguration)
        {
            _globalTrainerConfiguration = globalTrainerConfiguration;
        }

        public IEnumerable<Category> GetAvailableCategories()
        {
            var filesLocationPath = _dataProviderconfiguration.TrainFilesLocationPath;
            var categoriesFolders = Directory.GetDirectories(filesLocationPath);

            var itemCategoryEntries = categoriesFolders.Select((categoryFolderPath, i) =>
            {
                var categoryDirectoryInfo = new DirectoryInfo(categoryFolderPath);
                var filesOfCategory = GetFilesOfCategoryFolder(categoryDirectoryInfo);

                var category = new Category(
                    name:       categoryDirectoryInfo.Name,
                    fullPath:   categoryFolderPath,
                    files:      filesOfCategory);

                return category;
            });

            return itemCategoryEntries;
        }

        public LearningSet GetLearningSetForCategories(List<Category> categories)
        {
            var learningSet = new LearningSet();

            foreach (var category in categories)
            {
                var categoryInputsOutputs = GetCategoryLearningSet(category, categories.Count);
                learningSet.AddData(categoryInputsOutputs);
            }

            return learningSet;
        }

        private LearningSet SplitOnTrainAndTest(InputsOutputsData inputOutputsData)
        {
            var numOfSamples = inputOutputsData.Count;
            var trainDataRatio = _globalTrainerConfiguration.TrainDataRatio;
            var trainSamplesNum = (int)Math.Round(trainDataRatio * numOfSamples);
            var testSamplesNum = numOfSamples - trainSamplesNum;

            var rand = new Random();

            var learningSet = new LearningSet();
            var trainingData = learningSet.TrainingData;
            var testData = learningSet.TestData;

            for (int i = 0; i < numOfSamples; i++)
            {
                if (trainingData.Count < trainSamplesNum && testData.Count < testSamplesNum)
                {
                    var randChoice = rand.Next(1);
                    if (randChoice == 1)
                        trainingData.AddData(inputOutputsData.Inputs[i], inputOutputsData.Outputs[i]);
                    else
                        testData.AddData(inputOutputsData.Inputs[i], inputOutputsData.Outputs[i]);

                    continue;
                }

                // Executed only if one training or test data set is full.
                if (trainingData.Count < trainSamplesNum)
                    trainingData.AddData(inputOutputsData.Inputs[i], inputOutputsData.Outputs[i]);
                else
                    testData.AddData(inputOutputsData.Inputs[i], inputOutputsData.Outputs[i]);
            }

            return learningSet;
        }

        private Bitmap ShrinkImage(Image bitmap)
        {
            var newBitmap = new Bitmap(
                _globalTrainerConfiguration.ProcessingWidth, 
                _globalTrainerConfiguration.ProcessingHeight);

            var graphics = Graphics.FromImage(newBitmap);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(bitmap, 0, 0, newBitmap.Width, newBitmap.Height);
            graphics.Dispose();

            return newBitmap;
        }

        private LearningSet GetCategoryLearningSet(Category category, int numberOfCategories)
        {
            var inputOutputsData = new InputsOutputsData();

            foreach (var file in category.Files)
            {
                var image = (Bitmap)Image.FromFile(file.FullName, true);
                if (image.Width < _dataProviderconfiguration.CropWidth || image.Height < _dataProviderconfiguration.CropHeight)
                    continue;

                // Crop the image
                image = image.Clone(new Rectangle(0, 0, _dataProviderconfiguration.CropWidth, _dataProviderconfiguration.CropHeight), image.PixelFormat);

                // Downsample the image to save memory
                var smallImage = ShrinkImage(image);
                image.Dispose();

                double[] input;
                _imageToArray.Convert(smallImage, out input);
                smallImage.Dispose();

                var output = new double[numberOfCategories];
                output[category.Index] = 1;

                inputOutputsData.Inputs.Add(input);
                inputOutputsData.Outputs.Add(output);
            }

            var splittedSets = SplitOnTrainAndTest(inputOutputsData);
            return splittedSets;
        }

        private FileInfo[] GetFilesOfCategoryFolder(DirectoryInfo categoryDirectory)
        {
            var filesExtensions = _dataProviderconfiguration.FileExtensions;

            var files = Enumerable.Empty<FileInfo>();
            foreach (var extension in filesExtensions)
            {
                files = files.Concat(categoryDirectory.GetFiles($"*.{extension}"));
            }

            return files.ToArray();
        }

        public double[] PrepareImageByPath(string imageFilePath)
        {
            var image = (Bitmap)Image.FromFile(imageFilePath, true);
            var shrinked = ShrinkImage(image);

            double[] converted;
            _imageToArray.Convert(shrinked, out converted);

            return converted;
        }
    }
}
