﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace Microsoft.VisualStudio.Shell.Mocks
{
    public sealed class MenuCommandServiceMock : IMenuCommandService
    {
        Dictionary<CommandID, MenuCommand> _commands = new Dictionary<CommandID, MenuCommand>();

        public DesignerVerbCollection Verbs
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void AddCommand(MenuCommand command)
        {
            _commands[command.CommandID] = command;
        }

        public void AddVerb(DesignerVerb verb)
        {
        }

        public MenuCommand FindCommand(CommandID commandID)
        {
            MenuCommand command;
            _commands.TryGetValue(commandID, out command);

            return command;
        }

        public bool GlobalInvoke(CommandID commandID)
        {
            MenuCommand command;
            _commands.TryGetValue(commandID, out command);

            if(command != null)
            {
                command.Invoke();
                return true;
            }

            return false;
        }

        public void RemoveCommand(MenuCommand command)
        {
            _commands.Remove(command.CommandID);
        }

        public void RemoveVerb(DesignerVerb verb)
        {
        }

        public void ShowContextMenu(CommandID menuID, int x, int y)
        {
        }
    }
}