﻿#region License

/*
 Copyright 2014 - 2015 Nikita Bernthaler
 Game.cs is part of SFXUtility.

 SFXUtility is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 SFXUtility is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with SFXUtility. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion License

namespace SFXUtility.Features.Events
{
    #region

    using System;
    using System.Linq;
    using Classes;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SFXLibrary;
    using SFXLibrary.Logger;

    #endregion

    internal class Game : Base
    {
        private bool _onEndTriggerd;
        private bool _onStartTriggerd;
        private Events _parent;

        public Game()
        {
            LeagueSharp.Game.OnStart += delegate { _onStartTriggerd = true; };
        }

        public override bool Enabled
        {
            get { return !Unloaded && _parent != null && _parent.Enabled && Menu != null && Menu.Item(Name + "Enabled").GetValue<bool>(); }
        }

        public override string Name
        {
            get { return Language.Get("F_Game"); }
        }

        protected override void OnEnable()
        {
            LeagueSharp.Game.OnNotify += OnGameNotify;
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            LeagueSharp.Game.OnNotify -= OnGameNotify;
            base.OnDisable();
        }

        protected override void OnGameLoad(EventArgs args)
        {
            try
            {
                if (Global.IoC.IsRegistered<Events>())
                {
                    _parent = Global.IoC.Resolve<Events>();
                    if (_parent.Initialized)
                        OnParentInitialized(null, null);
                    else
                        _parent.OnInitialized += OnParentInitialized;
                }
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        private void OnParentInitialized(object sender, EventArgs eventArgs)
        {
            try
            {
                if (_parent.Menu == null)
                    return;

                Menu = new Menu(Name, Name);

                var startMenu = new Menu(Language.Get("Game_OnStart"), Name + "OnStart");
                startMenu.AddItem(new MenuItem(startMenu.Name + "Delay", Language.Get("G_Delay")).SetValue(new Slider(20, 0, 75)));
                startMenu.AddItem(
                    new MenuItem(startMenu.Name + "Greeting", Language.Get("Game_Greeting")).SetValue(
                        new StringList(Language.GetList("Game_GreetingList"))));
                startMenu.AddItem(
                    new MenuItem(startMenu.Name + "SayGreeting", Language.Get("G_Say") + " " + Language.Get("Game_Greeting")).SetValue(false));

                var endMenu = new Menu(Language.Get("Game_OnEnd"), Name + "OnEnd");
                endMenu.AddItem(
                    new MenuItem(endMenu.Name + "Ending", Language.Get("Game_Ending")).SetValue(new StringList(Language.GetList("Game_EndingList"))));
                endMenu.AddItem(new MenuItem(endMenu.Name + "SayEnding", Language.Get("G_Say") + " " + Language.Get("Game_Ending")).SetValue(false));

                Menu.AddSubMenu(startMenu);
                Menu.AddSubMenu(endMenu);

                Menu.AddItem(new MenuItem(Name + "Enabled", Language.Get("G_Enabled")).SetValue(false));

                _parent.Menu.AddSubMenu(Menu);

                HandleEvents(_parent);

                RaiseOnInitialized();

                if (_onStartTriggerd)
                {
                    if (Menu.Item(Name + "OnStartSayGreeting").GetValue<bool>() && !HeroManager.AllHeroes.Any(h => h.Level >= 2))
                    {
                        Utility.DelayAction.Add(Menu.Item(Name + "OnStartDelay").GetValue<Slider>().Value*1000,
                            delegate { LeagueSharp.Game.Say("/all " + Menu.Item(Name + "OnStartGreeting").GetValue<StringList>().SelectedValue); });
                    }
                }
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        private void OnGameNotify(GameNotifyEventArgs args)
        {
            if (_onEndTriggerd ||
                (args.EventId != GameEventId.OnEndGame && args.EventId != GameEventId.OnHQDie && args.EventId != GameEventId.OnHQKill))
                return;

            _onEndTriggerd = true;
            try
            {
                if (Menu.Item(Name + "OnEndSayEnding").GetValue<bool>())
                    LeagueSharp.Game.Say("/all " + Menu.Item(Name + "OnEndEnding").GetValue<StringList>().SelectedValue);
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }
    }
}