﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Models.Core;
using UserInterface.Commands;

namespace UserInterface
{

    /// <summary>
    /// Simple Command history class.
    /// </summary>
    public class CommandHistory
    {
        // Based on http://www.catnapgames.com/blog/2009/03/19/simple-undo-redo-system-for-csharp.html

        private List<ICommand> commands = new List<ICommand>();
        private int lastExecuted = -1;
        private int lastSaved = -1;

        public delegate void Changed(bool haveUnsavedChanges);
        public event Changed OnChanged = (h) => { };

        public delegate void ModelChangedDelegate(object changedModel);
        public event ModelChangedDelegate ModelChanged;

        public delegate void ModelStructureChangedDelegate(IModel model);
        public event ModelStructureChangedDelegate ModelStructureChanged;

        public void Clear()
        {
            commands.Clear();
            lastExecuted = -1;
            lastSaved = -1;

            OnChanged(false);
        }


        public void Save()
        {
            lastSaved = lastExecuted;

            OnChanged(false);
        }


        public bool Modified
        {
            get { return lastSaved != lastExecuted; }
        }


        public int Size
        {
            get { return commands.Count; }
        }


        public int LastExecuted
        {
            get { return lastExecuted; }
        }


        public void Limit(int numCommands)
        {
            while (commands.Count > numCommands)
            {
                commands.RemoveAt(0);
                if (lastExecuted >= 0)
                {
                    lastExecuted--;
                }
                if (lastSaved >= 0)
                {
                    lastSaved--;
                }
            }
        }


        public void Add(ICommand command, bool execute = true)
        {
            if (lastExecuted + 1 < commands.Count)
            {
                int numCommandsToRemove = commands.Count
                          - (lastExecuted + 1);
                for (int i = 0; i < numCommandsToRemove; i++)
                {
                    commands.RemoveAt(lastExecuted + 1);
                }
                lastSaved = -1;
            }
            commands.Add(command);
            lastExecuted = commands.Count - 1; 

            if (execute)
            {
                command.Do(this);
            }

            OnChanged(true);
        }

        public void Undo()
        {
            if (lastExecuted >= 0)
            {
                if (commands.Count > 0)
                {
                    commands[lastExecuted].Undo(this);
                    lastExecuted--;
                    OnChanged(lastExecuted != lastSaved);
                }
            }
        }

        public void Redo()
        {
            if (lastExecuted + 1 < commands.Count)
            {
                commands[lastExecuted + 1].Do(this);
                lastExecuted++;
                OnChanged(lastExecuted != lastSaved);
            }
        }

        public void InvokeModelChanged(object Model)
        {
            if (ModelChanged != null && Model != null)
                ModelChanged(Model);
        }

        public void InvokeModelStructureChanged(IModel model)
        {
            if (ModelStructureChanged != null && model != null)
                ModelStructureChanged(model);
        }
    }
}
