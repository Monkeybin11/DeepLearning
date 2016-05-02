﻿using System.Drawing;
using Wkiro.ImageClassification.Core.Models.Configurations;

namespace Wkiro.ImageClassification.Core.Engines.ImagePreprocessing
{
    public class AutoCrop : IImagePreprocessingStrategy
    {
        public Bitmap Process(Bitmap bitmap, DataProviderConfiguration configuration)
        {
            double actualRatio = (double) bitmap.Width/bitmap.Height;
            double expectedRatio = (double) configuration.ProcessingWidth
                                   /configuration.ProcessingHeight;

            return actualRatio > expectedRatio
                ? CropFromLeftAndRight(bitmap, expectedRatio)
                : CropFromTopAndBottom(bitmap, expectedRatio);
        }

        private static Bitmap CropFromLeftAndRight(Bitmap bitmap, double expectedRatio)
        {
            int cropWidth = (int) (bitmap.Height*expectedRatio);
            int dW = (bitmap.Width - cropWidth)/2;
            return bitmap.Clone(new Rectangle(dW, 0, cropWidth, bitmap.Height), bitmap.PixelFormat);
        }

        private static Bitmap CropFromTopAndBottom(Bitmap bitmap, double expectedRatio)
        {
            int cropHeight = (int) (bitmap.Width/expectedRatio);
            int dH = (bitmap.Height - cropHeight)/2;
            return bitmap.Clone(new Rectangle(0, dH, bitmap.Width, cropHeight), bitmap.PixelFormat);
        }
    }
}