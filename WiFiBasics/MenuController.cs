using Meadow.Devices;
using Meadow.Devices.Esp32.MessagePayloads;
using Meadow.Foundation;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Graphics;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectLab_Demo
{
    public partial class DisplayController
    {

        // Menu ///////////////////////////////////
        // Implements a reusable menu that can be selcted from
        // Upon selection a delegate can be called as a void(int) or Task(int)
        // The int is the selection index.
        // SetMenu writes the menu.
        // - Requires a list<string> and one of the two delegates.

        const int MaxMenuWidth = 24;       // Max width of menu items (truncated if larger).
        const int MenuLineSpacing = 20;    // The line spacing in pixels.
        const int MaxNumberMenuItems = 12; // Maximum number of menu items

        // The Task delgate
        public delegate Task ActionMenuSelectionDelegate(int ap);
        private ActionMenuSelectionDelegate _ActionMenuSelection = null;
        public ActionMenuSelectionDelegate ActionMenuSelection
        {
            get { return _ActionMenuSelection; }
            set { _ActionMenuSelection = value; _ActionMenuSelectionVoid = null; }
        }

        // The void delegate
        public delegate void ActionMenuSelectionVoidDelegate(int ap);
        private ActionMenuSelectionVoidDelegate _ActionMenuSelectionVoid = null;
        public ActionMenuSelectionVoidDelegate ActionMenuSelectionVoid
        {
            get { return _ActionMenuSelectionVoid; }
            set { _ActionMenuSelectionVoid = value; _ActionMenuSelection = null; }
        }

        // As used in Demo app
        private IProjectLabHardware ProjLab;
        MicroAudio Audio;

        // The menu items displayed
        private List<string> MenuItems = new List<string>();

        // The current selection (which is highlighted).
        private int Current { get; set; } = -1;

        // The buttons
        // Note Top (Up) button is used to move down the menu
        //      Down is used to move up teh menu
        //      Right is used for selection
        private enum Directions { right, down, left, up };


        /// <summary>
        /// General setup of the menu, including buttons handler.
        /// Note that SetMenu() actually loads it
        /// </summary>
        /// <param name="projLab">Intatiated in the main code so passed to here</param>
        /// <param name="audio">Intatiated in the main code so passed to here</param>
        public void InitMenu(IProjectLabHardware projLab, MicroAudio audio)
        {
            ProjLab = projLab;
            Audio = audio;

            //---- buttons
            if (ProjLab.RightButton is { } rightButton)
            {
                rightButton.PressStarted += (s, e) => ButtonPressed(Directions.right, true);
                rightButton.PressEnded += (s, e) => ButtonPressed(Directions.right, false);
            }

            if (ProjLab.DownButton is { } downButton)
            {
                downButton.PressStarted += (s, e) => ButtonPressed(Directions.down, true);
                downButton.PressEnded += (s, e) => ButtonPressed(Directions.down, false);
            }
            if (ProjLab.LeftButton is { } leftButton)
            {
                leftButton.PressStarted += (s, e) => ButtonPressed(Directions.left, true);
                leftButton.PressEnded += (s, e) => ButtonPressed(Directions.left, false);
            }
            if (ProjLab.UpButton is { } upButton)
            {
                upButton.PressStarted += (s, e) => ButtonPressed(Directions.up, true);
                upButton.PressEnded += (s, e) => ButtonPressed(Directions.up, false);
            }
        }

        /// <summary>
        /// General button handler
        /// </summary>
        /// <param name="butt">The button pressed</param>
        /// <param name="state">Pressed or released</param>
        /// <returns></returns>
        private void ButtonPressed(Directions butt, bool state)
        {
            switch (butt)
            {
                case Directions.right:

                    if (state)
                    {
                        if ((Current < (MenuItems.Count))
                            &&
                            (Current > -1))
                        {
                            // Call delegate
                            if (ActionMenuSelectionVoid != null)
                                ActionMenuSelectionVoid(Current);
                            else if (ActionMenuSelection != null)
                                ActionMenuSelection(Current).GetAwaiter();
                        }
                    }
                    break;
                case Directions.up:
                    if (state)
                    {
                        MenuDown();
                    }
                    break;
                case Directions.down:
                    if (state)
                    {
                        MenuUp();
                    }
                    break;
            }
        }


        /// <summary>
        /// Setup a menu
        /// </summary>
        /// <param name="menuItems">List of menu items</param>
        /// <param name="actionMenuSelectionVoid">void delegate. Takes an int</param>
        /// <param name="actionMenuSelection">Task delegate. Takes an int</param>
        /// <param name="colr">Optional color</param>
        /// <param name="posn">Optional starting position in pixels</param>
        public void SetMenu(List<string> menuItems, ActionMenuSelectionVoidDelegate actionMenuSelectionVoid, ActionMenuSelectionDelegate actionMenuSelection = null, Meadow.Foundation.Color? colr = null, int posn = 2)
        {
            if (actionMenuSelectionVoid != null)
                ActionMenuSelectionVoid = actionMenuSelectionVoid;
            else if (actionMenuSelection != null)
                ActionMenuSelection = actionMenuSelection;
            // Save manu list
            MenuItems = menuItems;
            //Note global Current is set in MenuWrite()
            int current = 0;
            MenuWrite(current);
        }

        public void MenuUp()
        {
            int current = Current - 1;
            if ((current < MenuItems.Count) && (current > -1))
            {
                MenuWrite(current);
                Audio.PlayGameSound(GameSoundEffect.MenuNavigate);
            }
        }

        public void MenuDown()
        {
            int current = Current + 1;
            if ((current < MenuItems.Count) && (current > -1))
            {
                MenuWrite(current);
                Audio.PlayGameSound(GameSoundEffect.MenuNavigate);
            }
        }

        /// <summary>
        /// Display menu with Current item highlighted
        /// </summary>
        /// <param name="current">Set Current item</param>
        /// <param name="colr">optional color</param>
        /// <param name="posn">optional start position on line</param>
        public void MenuWrite(int current, Meadow.Foundation.Color? colr = null, int posn = 2)
        {
            Current = current;
            List<string> menuItems = MenuItems;

            if (colr is null)
            {
                //Ignored at the moment 2Do
                colr = WildernessLabsColors.AzureBlue;
            }
            int count = 0;
            graphics.Clear();
            foreach (var menuItem in menuItems)
            {
                string item = menuItem;
                if (menuItem.Length > MaxMenuWidth) //Truncate long menu items
                    item = menuItem.Substring(0, MaxMenuWidth);
                if (count == Current) // Highlighted
                    graphics.DrawText(x: posn, count * MenuLineSpacing, item, WildernessLabsColors.Sandrift);
                else
                    graphics.DrawText(x: posn, count * MenuLineSpacing, item, WildernessLabsColors.AzureBlue);

                count++;
                if (count > MaxNumberMenuItems)
                    break;
            }
            graphics.Show();
        }

        /// <summary>
        /// Write one line. Used in development only
        /// </summary>
        /// <param name="line">line number indexed from 0</param>
        /// <param name="message">Message to be displayed</param>
        /// <param name="colr"></param>
        /// <param name="posn"></param>
        public void Write(int line, string message, Meadow.Foundation.Color? colr = null, int posn = 2)
        {

            if (message.Length>24)
                message = message.Substring(0, 24);
            if (colr is null)
            {
                colr = WildernessLabsColors.AzureBlue;
            }

            message = message.Substring(0, 24);

            if (line == 1)
            {
                graphics.Clear();
                graphics.DrawText(x: posn, 0, message, WildernessLabsColors.AzureBlue);
                graphics.Show();
            }
            else
            {
                graphics.DrawText(x: posn, y: (line - 1) * 20, message, WildernessLabsColors.AzureBlue);
                graphics.Show();
            }
        }
    }
}