using System;
using System.Collections.Generic;
using UserInterface;
using UserInterface.Commands;
using UserInterface.Presenters;

/// <summary>
/// This script selects all nodes in the standard toolbox, testing to 
/// see that no error is thrown.
/// </summary>
public class Script
{
    public void Execute(MainPresenter mainPresenter)
    {
        // Open the standard toolbox in a tab
        mainPresenter.OnStandardToolboxClick(null, null);
    
        // Get the presenter for this tab.
        ExplorerPresenter presenter = mainPresenter.presenters1[0];
    
        // Loop through all nodes in the standard toolbox and select each in turn.
        while (presenter.SelectNextNode());
        
        // Close the user interface.
        mainPresenter.Close(askToSave:false);
    }


}