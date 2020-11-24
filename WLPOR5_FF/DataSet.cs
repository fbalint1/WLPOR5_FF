using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WLPOR5_FF
{
    class DataSet
    {
        public const int INPUT_SIZE = 20;
        public List<float> Input { get; set; } = new List<float>();

        public const int OUTPUT_SIZE = 10;
        public List<float> Output { get; set; }

        public int Count { get; set; }

        public DataSet(string inputFile_, string outputFile_)
        {
            LoadInputs(inputFile_);
            LoadOutputs(outputFile_);
        }

        private void LoadInputs(string filename_)
        {
            var lines = File.ReadAllLines(filename_);

            foreach (var line in lines)
            {
                var data = line.Split(';');
                Count += data.Length - 1;

                for (int i = 1; i < data.Length; i++)
                {
                    Input.Add(float.Parse(data[i]));
                }
            }
        }

        private void LoadOutputs(string filename)
        {

        }
    }
}
