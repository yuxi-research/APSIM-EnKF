using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Core;
using System.Xml.Serialization;

namespace Models.DataAssimilation.DataType
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class StatesOfTheDay : Model
    {
        #region ******* Public Field. *******
        /// <summary>  </summary>
        [XmlIgnore]
        public int ID { get; set; }

        /// <summary>  </summary>
        [XmlIgnore]
        public DateTime Date { get; set; }

        /// <summary>  </summary>
        [XmlIgnore]
        public int DOY { get; set; }

        /// <summary>  </summary>
        [XmlIgnore]
        public List<double> Truth { get; set; }

        /// <summary>  </summary>
        [XmlIgnore]
        public List<double> PriorOL { get; set; }

        /// <summary>  </summary>
        [XmlIgnore]
        public List<double[]> Prior { get; set; }

        /// <summary>  </summary>
        [XmlIgnore]
        public List<double> PriorMean { get; set; }

        /// <summary>  </summary>
        [XmlIgnore]
        public List<double> PosteriorOL { get; set; }

        /// <summary>  </summary>
        [XmlIgnore]
        public List<double[]> Posterior { get; set; }

        /// <summary>  </summary>
        [XmlIgnore]
        public List<double> PosteriorMean { get; set; }

        /// <summary>  </summary>
        [XmlIgnore]
        public List<double> Obs { get; set; }

        /// <summary>  </summary>
        [XmlIgnore]
        public List<double[]> ObsPerturb { get; set; }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public StatesOfTheDay()
        {
            Truth = new List<double>();
            PriorOL = new List<double>();
            Prior = new List<double[]>();
            PriorMean = new List<double>();
            PosteriorOL = new List<double>();
            Posterior = new List<double[]>();
            PosteriorMean = new List<double>();
            Obs = new List<double>();
            ObsPerturb = new List<double[]>();
        }

        /// <summary>
        /// Transfer List to 2-dimensional matrix.
        /// </summary>
        /// <param name="List"></param>
        /// <returns></returns>
        public Matrix ListToMat(List<double[]> List)
        {
            Matrix Mat = new Matrix(List);
            return Mat;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="List"></param>
        /// <returns></returns>
        public Matrix ListToVec(List<double> List)
        {
            Matrix Mat = new Matrix(List);
            return Mat;
        }

        /// <summary>
        /// Transfer 2-dimensional matrix to List.
        /// </summary>
        /// <param name="Mat"></param>
        /// <returns></returns>
        public List<double[]> MatToList(Matrix Mat)
        {
            List<double[]> List = new List<double[]>();
            double[] temp;
            for (int i = 0; i < Mat.Row; i++)
            {
                temp = new double[Mat.Col];
                for (int j = 0; j < Mat.Col; j++)
                {
                    temp[j] = Mat.Arr[i, j];
                }
                List.Add(temp);
            }
            return List;
        }

        /// <summary>
        /// Transfer 1-dimensional col vector to List.
        /// </summary>
        /// <param name="Mat"></param>
        /// <returns></returns>
        public List<double> VecToList(Matrix Mat)
        {
            List<double> List = new List<double>();
            for (int i = 0; i < Mat.Row; i++)
            {
                List.Add(Mat.Arr[i, 0]);
            }
            return List;
        }
    }
}

