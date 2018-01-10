using CNTK;
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.CNTK.API.Classifier
{
    public class CNTKModelPredictionResources
    {
        public CNTKModelPredictionSettings Settings { get; private set; }
        public string[] Labels { get; private set; }
        public Function Model { get; private set; }

        private static CNTKModelPredictionResources _resources;
        public static CNTKModelPredictionResources Resources {
            get {
                if (_resources == null)
                    Initialize();
                return _resources;
            }
        }

        public static void Initialize()
        {
            if (_resources != null) return;

            var settings = CNTKModelPredictionSettings.FromWebSettings();
            var labels = LoadLabels(settings.LabelsFilename);
            var model = LoadModel(settings.ModelFilename);
            _resources = new CNTKModelPredictionResources()
            {
                Settings = settings,
                Labels = labels,
                Model = model
            };
        }

        private static string[] LoadLabels(string labelsFilename)
        {
            labelsFilename = Path.Combine(HttpRuntime.BinDirectory, labelsFilename);
            if (!File.Exists(labelsFilename))
                throw new FileNotFoundException("Labels file not found", labelsFilename);

            var labels = File.ReadAllLines(labelsFilename);

            return labels.Where(c => !String.IsNullOrWhiteSpace(c)).ToArray();
        }

        private static Function LoadModel(string modelFilename)
        {
            modelFilename = Path.Combine(HttpRuntime.BinDirectory, modelFilename);
            if (!File.Exists(modelFilename))
                throw new FileNotFoundException("Model file not found", modelFilename);

            Function model = null;
            try
            {
                model = Function.Load(modelFilename, CNTKModelPrediction.CPUDeviceDescriptor);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return model;
        }

    }
}
