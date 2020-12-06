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
        public int InputSize { get; set; }
        public List<float> Input { get; set; } = new List<float>();

        public int OutputSize { get; set; }
        public List<float> Output { get; set; } = new List<float>();

        public int Count { get; set; }

        public DataSet(string inputFile_, string outputFile_)
        {
            LoadInputs(inputFile_);
            LoadOutputs(outputFile_);
        }

        private void LoadInputs(string filename_)
        {
            var lines = File.ReadAllLines(filename_);
            InputSize = lines[0].Split(';').Length - 1;
            foreach (var line in lines)
            {
                var data = line.Split(';');
                Count++;

                for (int i = 1; i < data.Length; i++)
                {
                    Input.Add(float.Parse(data[i]));
                }
            }
        }

        private void LoadOutputs(string filename_)
        {
            var lines = File.ReadAllLines(filename_);
            OutputSize = lines[0].Split(';').Length;
            foreach (var line in lines)
            {
                var data = line.Split(';');

                for (int i = 0; i < data.Length; i++)
                {
                    Output.Add(float.Parse(data[i]));
                }
            }
        }
    }
}
