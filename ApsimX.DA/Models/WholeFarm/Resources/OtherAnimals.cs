using Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Models.WholeFarm.Resources
{
	///<summary>
	/// Parent model of Ruminant Types.
	///</summary> 
	[Serializable]
	[ViewName("UserInterface.Views.GridView")]
	[PresenterName("UserInterface.Presenters.PropertyPresenter")]
	[ValidParent(ParentType = typeof(ResourcesHolder))]
	public class OtherAnimals: ResourceBaseWithTransactions
	{
		/// <summary>An event handler to allow us to initialise ourselves.</summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		[EventSubscribe("Commencing")]
		private void OnSimulationCommencing(object sender, EventArgs e)
		{

			// create cohort list that can be modified by simulation
			foreach (var child in Children)
			{
				if (child is IResourceWithTransactionType)
				{
					(child as IResourceWithTransactionType).TransactionOccurred += OtherAnimals_TransactionOccurred;
				}
			}
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

		private void OtherAnimals_TransactionOccurred(object sender, EventArgs e)
		{
			LastTransaction = (e as TransactionEventArgs).Transaction;
			OnTransactionOccurred(e);
		}

		#endregion

	}
}
