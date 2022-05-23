﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;  //enumerator
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Models.Core;

namespace Models.WholeFarm.Resources
{
    ///<summary>
    /// Parent model of Land Types.
    ///</summary> 
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(ResourcesHolder))]
    public class Land: ResourceBaseWithTransactions
	{
        /// <summary>
        /// Current state of this resource.
        /// </summary>
        [XmlIgnore]
        public List<LandType> Items;

		/// <summary>
		/// Unit of area to be used in this simulation
		/// </summary>
		[Description("Unit of area to be used in this simulation")]
		public Common.UnitsOfAreaType UnitOfArea { get; set; }

		/// <summary>An event handler to allow us to initialise ourselves.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		[EventSubscribe("Commencing")]
        private void OnSimulationCommencing(object sender, EventArgs e)
        {
			foreach (var child in Children)
			{
				if (child is IResourceWithTransactionType)
				{
					(child as IResourceWithTransactionType).TransactionOccurred += Resource_TransactionOccurred; ;
				}
			}

			//Items = new List<LandType>();

   //         List<IModel> childNodes = Apsim.Children(this, typeof(IModel));

   //         foreach (IModel childModel in childNodes)
   //         {
   //             //cast the generic IModel to a specfic model.
   //             LandType land = childModel as LandType;
			//	land.TransactionOccurred += Resource_TransactionOccurred;
			//	Items.Add(land);
   //         }
        }

		#region Transactions

		// Must be included away from base class so that APSIM Event.Subscriber can find them 

		/// <summary>
		/// Override base event
		/// </summary>
		protected new void OnTransactionOccurred(EventArgs e)
		{
			EventHandler invoker = TransactionOccurred;
			if (invoker != null) invoker(this, e);
		}

		/// <summary>
		/// Override base event
		/// </summary>
		public new event EventHandler TransactionOccurred;

		private void Resource_TransactionOccurred(object sender, EventArgs e)
		{
			LastTransaction = (e as TransactionEventArgs).Transaction;
			OnTransactionOccurred(e);
		}

		#endregion
	}


}
