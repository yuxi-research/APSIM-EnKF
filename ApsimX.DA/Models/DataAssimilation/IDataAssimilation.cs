using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Core;

namespace Models.DataAssimilation
{
    /// <summary>
    /// A interface for Data Assimilation classes.
    /// </summary>
    public interface IDataAssimilation
    {
        /// <summary>
        /// 
        /// </summary>
        bool DAActivated { get; }
    }
}
