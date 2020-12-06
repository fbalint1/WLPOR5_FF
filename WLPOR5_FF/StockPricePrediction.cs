using CNTK;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace WLPOR5_FF
{
    class StockPricePrediction
    {
        private const int BATCH_SIZE = 20;
        private const int EPOCH_COUNT = 100;

        private readonly Variable _x;
        private readonly Function _y;
        private readonly DataSet _dataSet;

        public StockPricePrediction(int hiddenNeuronCount_, DataSet dataSet_)
        {
            _dataSet = dataSet_;

            var layers = new int[] { _dataSet.InputSize, hiddenNeuronCount_, hiddenNeuronCount_, hiddenNeuronCount_, _dataSet.OutputSize };
            _x = Variable.InputVariable(new int[] { layers[0] }, DataType.Float);

            var lastLayer = _x;
            for (int i = 0; i < layers.Length - 1; i++)
            {
                Parameter weight = new Parameter(new int[] { layers[i + 1], layers[i] }, DataType.Float, CNTKLib.GlorotNormalInitializer());
                Parameter bias = new Parameter(new int[] { layers[i + 1] }, DataType.Float, CNTKLib.GlorotNormalInitializer());

                Function times = CNTKLib.Times(weight, lastLayer);
                Function plus = CNTKLib.Plus(times, bias);
                if (i != layers.Length - 2)
                {
                    lastLayer = CNTKLib.Sigmoid(plus);
                }
                else
                {
                    lastLayer = CNTKLib.Abs(plus);
                }
            }
            _y = lastLayer;
        }

        public void Train()
        {
            var yt = Variable.InputVariable(new int[] { _dataSet.OutputSize }, DataType.Float);

            var y_yt = CNTKLib.Abs(CNTKLib.Minus(_y, yt));
            var loss = CNTKLib.ReduceSum(y_yt, Axis.AllAxes());

            var learner = CNTKLib.SGDLearner(new ParameterVector(_y.Parameters().ToArray()), new TrainingParameterScheduleDouble(1.0, BATCH_SIZE));
            var trainer = Trainer.CreateTrainer(_y, loss, null, new List<Learner> { learner });

            for (int i = 0; i < EPOCH_COUNT; i++)
            {
                var sumLoss = 0.0;
                var sumEval = 0.0;

                for (int j = 0; j < _dataSet.Count / BATCH_SIZE - 1; j++)
                {
                    var x_value = Value.CreateBatch(_x.Shape, _dataSet.Input.GetRange(j * BATCH_SIZE * _dataSet.InputSize, BATCH_SIZE * _dataSet.InputSize), DeviceDescriptor.CPUDevice);
                    var yt_value = Value.CreateBatch(yt.Shape, _dataSet.Output.GetRange(j * BATCH_SIZE * _dataSet.OutputSize, BATCH_SIZE * _dataSet.OutputSize), DeviceDescriptor.CPUDevice);
                    var inputDataMap = new Dictionary<Variable, Value>()
                    {
                        { _x, x_value },
                        { yt, yt_value }
                    };

                    trainer.TrainMinibatch(inputDataMap, false, DeviceDescriptor.CPUDevice);
                    sumLoss += trainer.PreviousMinibatchLossAverage() * trainer.PreviousMinibatchSampleCount();
                }

                Console.WriteLine($"Iter: {i}\tLoss: {sumLoss / _dataSet.Count}");
            }
        }

        public List<float> Prediction(List<float> inputs_)
        {
            Value x_value = Value.CreateBatch(_x.Shape, inputs_, DeviceDescriptor.CPUDevice);
            var inputDataMap = new Dictionary<Variable, Value> { { _x, x_value } };
            var outputDataMap = new Dictionary<Variable, Value> { { _y, null } };
            _y.Evaluate(inputDataMap, outputDataMap, DeviceDescriptor.CPUDevice);

            return outputDataMap[_y].GetDenseData<float>(_y)[0].ToList();
        }
    }
}
