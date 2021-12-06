using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Models.Core;

namespace Models.DataAssimilation.DataType
{

    /// <summary>
    /// A data type to store EnKF observation operator for a single observation type. (Linear matrix).
    /// </summary>
    /// 
    [Serializable]
    public class ObsOperator : Model
    {
        /// <summary> The table Name. </summary>
        [XmlIgnore]
        public string ObsName { get; }
        /// <summary> A list of state variables related to the specific obs.  </summary>
        [XmlIgnore]
        public string[] StateList { get; }
        /// <summary> A list of H values linking state variables to obs. </summary>
        [XmlIgnore]
        public double[] HList { get; }
        /// <summary> The table Name. </summary>
        public ObsOperator(string obsName, string[] stateList, double[] hList)
        {
            ObsName = obsName;
            StateList = stateList;
            HList = hList;
        }
        /// <summary> Display data. </summary>
        public void Display()
        {
            Console.Write("[" + ObsName + "], [");
            foreach (string value in StateList)
                Console.Write(value + ", ");
            Console.Write("], [");
            foreach (double value in HList)
                Console.Write(value + ", ");
            Console.WriteLine("]");
        }

    }
}
