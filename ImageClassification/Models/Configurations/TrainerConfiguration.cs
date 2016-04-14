﻿using System.Collections.Generic;
using ImageClassification.Models.Dto;

namespace ImageClassification.Models.Configurations
{
    public class TrainerConfiguration
    {
        public IEnumerable<Category> Categories { get; set; } 
        public InputOutputsDataNative InputsOutputsData { get; set; }
        public int[] Layers { get; set; }
    }
}