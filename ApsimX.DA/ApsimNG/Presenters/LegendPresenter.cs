﻿using Models.Graph;
using UserInterface.Views;
using System;
using System.Collections.Generic;

namespace UserInterface.Presenters
{
    /// <summary>
    /// This presenter connects an instance of a Model.Graph.Axis with a 
    /// UserInterface.Views.AxisView
    /// </summary>
    class LegendPresenter : IPresenter
    {
        private Graph Graph;
        private ILegendView View;
        private ExplorerPresenter ExplorerPresenter;
        private GraphPresenter graphPresenter;

        /// <summary>Initializes a new instance of the <see cref="LegendPresenter"/> class.</summary>
        /// <param name="graphPresenter">The graph presenter.</param>
        public LegendPresenter(GraphPresenter graphPresenter)
        {
            this.graphPresenter = graphPresenter;
        }

        /// <summary>
        /// Attach the specified Model and View.
        /// </summary>
        public void Attach(object model, object view, ExplorerPresenter explorerPresenter)
        {
            Graph = model as Graph;
            View = view as ILegendView;
            ExplorerPresenter = explorerPresenter;

            // Trap change event from the model.
            ExplorerPresenter.CommandHistory.ModelChanged += OnModelChanged;

            // Trap events from the view.
            View.OnPositionChanged += OnTitleChanged;

            // Tell the view to populate the axis.
            PopulateView();
        }

        /// <summary>Populates the view.</summary>
        private void PopulateView()
        {
            List<string> values = new List<string>();
            foreach (Graph.LegendPositionType value in Enum.GetValues(typeof(Graph.LegendPositionType)))
                values.Add(value.ToString());

            View.Populate(Graph.LegendPosition.ToString(), values.ToArray());

            List<string> seriesNames = GetSeriesNames();
            View.SetSeriesNames(seriesNames.ToArray());
            View.SetDisabledSeriesNames(Graph.DisabledSeries.ToArray());

            View.DisabledSeriesChanged += OnDisabledSeriesChanged;
        }

        private List<string> GetSeriesNames()
        {
            List<string> seriesNames = new List<string>();
            foreach (string seriesName in graphPresenter.GetSeriesNames())
                if (!seriesNames.Contains(seriesName))
                    seriesNames.Add(seriesName);
            return seriesNames;
        }

        /// <summary>Called when user changes a disabled series.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void OnDisabledSeriesChanged(object sender, EventArgs e)
        {
            ExplorerPresenter.CommandHistory.ModelChanged -= OnModelChanged;

            List<string> disabledSeries = new List<string>();
            disabledSeries.AddRange(View.GetDisabledSeriesNames());
            if (disabledSeries.Count < GetSeriesNames().Count)
                ExplorerPresenter.CommandHistory.Add(new Commands.ChangeProperty(Graph, "DisabledSeries", disabledSeries));

            ExplorerPresenter.CommandHistory.ModelChanged += OnModelChanged;
        }

        /// <summary>
        /// Detach the model from the view.
        /// </summary>
        public void Detach()
        {
            // Trap change event from the model.
            ExplorerPresenter.CommandHistory.ModelChanged -= OnModelChanged;

            // Trap events from the view.
            View.OnPositionChanged -= OnTitleChanged;

            View.DisabledSeriesChanged -= OnDisabledSeriesChanged;
        }
        
        /// <summary>
        /// The 'Model' has changed so we need to update the 'View'. Usually the result of an 'Undo' or 'Redo'
        /// </summary>
        private void OnModelChanged(object Model)
        {
            if (Model == Graph)
                PopulateView();
        }

        /// <summary>
        /// The user has changed the title field on the form. Need to tell the model this via
        /// executing a command.
        /// </summary>
        void OnTitleChanged(string NewText)
        {
            Graph.LegendPositionType LegendPosition;
            Enum.TryParse<Graph.LegendPositionType>(NewText, out LegendPosition);
            ExplorerPresenter.CommandHistory.Add(new Commands.ChangeProperty(Graph, "LegendPosition", LegendPosition));
        }

    }
}
