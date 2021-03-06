﻿#region License

/*
 Copyright 2014 - 2015 Nikita Bernthaler
 AutoLantern.cs is part of SFXUtility.

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

namespace SFXUtility.Features.Others
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

    internal class AutoLantern : Base
    {
        private Others _parent;

        public override bool Enabled
        {
            get { return !Unloaded && _parent != null && _parent.Enabled && Menu != null && Menu.Item(Name + "Enabled").GetValue<bool>(); }
        }

        public override string Name
        {
            get { return Language.Get("F_AutoLantern"); }
        }

        protected override void OnEnable()
        {
            Game.OnUpdate += OnGameUpdate;
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            Game.OnUpdate -= OnGameUpdate;
            base.OnDisable();
        }

        protected override void OnGameLoad(EventArgs args)
        {
            try
            {
                if (Global.IoC.IsRegistered<Others>())
                {
                    _parent = Global.IoC.Resolve<Others>();
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

                Menu.AddItem(new MenuItem(Name + "Percent", Language.Get("G_Health") + " " + Language.Get("G_Percent")).SetValue(new Slider(20, 0, 50)));
                Menu.AddItem(new MenuItem(Name + "Hotkey", Language.Get("G_Hotkey")).SetValue(new KeyBind('U', KeyBindType.Press)));

                Menu.AddItem(new MenuItem(Name + "Enabled", Language.Get("G_Enabled")).SetValue(false));

                _parent.Menu.AddSubMenu(Menu);

                if (HeroManager.Allies.Any(a => !a.IsMe && a.ChampionName.Equals("Thresh", StringComparison.OrdinalIgnoreCase)))
                    return;

                HandleEvents(_parent);
                RaiseOnInitialized();
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        private void OnGameUpdate(EventArgs args)
        {
            try
            {
                if (ObjectManager.Player.IsDead)
                    return;

                if (ObjectManager.Player.HealthPercent <= Menu.Item(Name + "Percent").GetValue<Slider>().Value ||
                    Menu.Item(Name + "Hotkey").IsActive())
                {
                    var lantern =
                        ObjectManager.Get<Obj_AI_Base>()
                            .FirstOrDefault(obj => obj.IsValid && obj.IsAlly && obj.Name.Equals("ThreshLantern", StringComparison.OrdinalIgnoreCase));
                    if (lantern != null && lantern.IsValidTarget(500, false, ObjectManager.Player.ServerPosition))
                    {
                        ObjectManager.Player.Spellbook.CastSpell((SpellSlot) 62, lantern);
                    }
                }
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }
    }
}