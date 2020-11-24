using CNTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WLPOR5_FF
{
    class StockPricePrediction
    {
        private const int BATCH_SIZE = 50;
        private const int EPOCH_COUNT = 100;

        private readonly Variable _x;
        private readonly Function _y;

        public StockPricePrediction(int hiddenNeuronCount_)
        {
            var layers = new int[] { DataSet.INPUT_SIZE, hiddenNeuronCount_, hiddenNeuronCount_, DataSet.OUTPUT_SIZE };

            _x = Variable.InputVariable(new int[] { layers[0] }, DataType.Float);


            var lastLayer = _x;
            for (int i = 0; i < layers.Length; i++)
            {
                Parameter weight = new Parameter(new int[] { layers[i + 1], layers[i] }, DataType.Float, CNTKLib.GlorotNormalInitializer());
                Parameter bias = new Parameter(new int[] { layers[i + 1] }, DataType.Float, CNTKLib.GlorotNormalInitializer());

                Function times = CNTKLib.Times(weight, _x);
                Function plus = CNTKLib.Plus(times, bias);

                if (i != layers.Length - 2)
                {
                    lastLayer = CNTKLib.Sigmoid(plus);
                }
                else
                {
                    lastLayer = CNTKLib.Softmax(plus);
                }
            }
            _y = lastLayer;
        }

        public void Train(DataSet dataSet_)
        {
            var yt = Variable.InputVariable(new int[] { DataSet.OUTPUT_SIZE }, DataType.Float);
            var loss = CNTKLib.CrossEntropyWithSoftmax(_y, yt);
            var error = CNTKLib.ClassificationError(_y, yt);

            var learner = CNTKLib.SGDLearner(new ParameterVector(_y.Parameters().ToArray()), new TrainingParameterScheduleDouble(1.0, BATCH_SIZE));
            var trainer = Trainer.CreateTrainer(_y, loss, error, new List<Learner> { learner });

            for (int i = 0; i < EPOCH_COUNT; i++)
            {
                var sumLoss = 0.0;
                var sumError = 0.0;

                for (int j = 0; j < dataSet_.Input.Count / BATCH_SIZE; j++)
                {
                    var x_value = Value.CreateBatch(_x.Shape, dataSet_.Input.GetRange(j * BATCH_SIZE * DataSet.INPUT_SIZE, BATCH_SIZE * DataSet.INPUT_SIZE), DeviceDescriptor.CPUDevice);
                    var yt_value = Value.CreateBatch(yt.Shape, dataSet_.Output.GetRange(j * BATCH_SIZE * DataSet.INPUT_SIZE, BATCH_SIZE * DataSet.INPUT_SIZE), DeviceDescriptor.CPUDevice);
                    var inputDataMap = new UnorderedMapVariableValuePtr()
                    {
                        { _x, x_value },
                        { yt, yt_value }
                    };

                    trainer.TrainMinibatch(inputDataMap, false, DeviceDescriptor.CPUDevice);
                    sumLoss += trainer.PreviousMinibatchLossAverage() * trainer.PreviousMinibatchSampleCount();
                    sumError += trainer.PreviousMinibatchEvaluationAverage() * trainer.PreviousMinibatchSampleCount();
                }

                Console.WriteLine($"{i}\t{sumLoss / dataSet_.Count}\t{1.0 - sumError / dataSet_.Count}");
            }
        }

        public void Evaluate(DataSet dataSet_)
        {
            var yt = Variable.InputVariable(new int[] { DataSet.OUTPUT_SIZE }, DataType.Float);
            var loss = CNTKLib.CrossEntropyWithSoftmax(_y, yt);
            var error = CNTKLib.ClassificationError(_y, yt);

            var evaluateLoss = CNTKLib.CreateEvaluator(loss);
            var evaluateError = CNTKLib.CreateEvaluator(error);

            var sumLoss = 0.0;
            var sumEval = 0.0;

            for (int j = 0; j < dataSet_.Input.Count / BATCH_SIZE; j++)
            {
                var x_value = Value.CreateBatch(_x.Shape, dataSet_.Input.GetRange(j * BATCH_SIZE * DataSet.INPUT_SIZE, BATCH_SIZE * DataSet.INPUT_SIZE), DeviceDescriptor.CPUDevice);
                var yt_value = Value.CreateBatch(yt.Shape, dataSet_.Output.GetRange(j * BATCH_SIZE * DataSet.INPUT_SIZE, BATCH_SIZE * DataSet.INPUT_SIZE), DeviceDescriptor.CPUDevice);
                var inputDataMap = new UnorderedMapVariableValuePtr()
                    {
                        { _x, x_value },
                        { yt, yt_value }
                    };

                sumLoss += evaluateLoss.TestMinibatch(inputDataMap) * BATCH_SIZE;
                sumEval += evaluateError.TestMinibatch(inputDataMap) * BATCH_SIZE;
            }

            Console.WriteLine($"Loss: {sumLoss / dataSet_.Count}, Accuracy: {1 - sumEval / dataSet_.Count}");
        }
    }
}
