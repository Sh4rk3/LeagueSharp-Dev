﻿#region License

/*
 Copyright 2014 - 2015 Nikita Bernthaler
 Range.cs is part of SFXUtility.

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

namespace SFXUtility.Features.Drawings
{
    #region

    using System;
    using System.Drawing;
    using System.Linq;
    using Classes;
    using LeagueSharp;
    using LeagueSharp.Common;
    using SFXLibrary;
    using SFXLibrary.Extensions.SharpDX;
    using SFXLibrary.Logger;

    #endregion

    internal class Range : Base
    {
        private const float ExperienceRange = 1400f;
        private const float TurretRange = 900f;
        private Drawings _parent;

        public override bool Enabled
        {
            get { return !Unloaded && _parent != null && _parent.Enabled && Menu != null && Menu.Item(Name + "Enabled").GetValue<bool>(); }
        }

        public override string Name
        {
            get { return Language.Get("F_Range"); }
        }

        private void DrawAttack()
        {
            try
            {
                var drawAlly = Menu.Item(Name + "AttackAlly").GetValue<bool>();
                var drawEnemy = Menu.Item(Name + "AttackEnemy").GetValue<bool>();
                var drawSelf = Menu.Item(Name + "AttackSelf").GetValue<bool>();
                var thickness = Menu.Item(Name + "DrawingCircleThickness").GetValue<Slider>().Value;

                if (!drawAlly && !drawEnemy && !drawSelf)
                    return;

                var allyColor = Menu.Item(Name + "AttackColorAlly").GetValue<Color>();
                var enemyColor = Menu.Item(Name + "AttackColorEnemy").GetValue<Color>();
                var selfColor = Menu.Item(Name + "AttackColorSelf").GetValue<Color>();

                foreach (var hero in
                    HeroManager.AllHeroes.Where(hero => !hero.IsDead && hero.IsVisible)
                        .Where(hero => (hero.IsAlly && drawAlly || hero.IsMe && drawSelf || hero.IsEnemy && drawEnemy) && !(hero.IsMe && !drawSelf)))
                {
                    var radius = hero.BoundingRadius + hero.AttackRange;
                    if (hero.Position.IsOnScreen(radius))
                    {
                        Render.Circle.DrawCircle(hero.Position, radius, hero.IsMe ? selfColor : (hero.IsEnemy ? enemyColor : allyColor), thickness);
                    }
                }
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        private void DrawExperience()
        {
            try
            {
                var drawAlly = Menu.Item(Name + "ExperienceAlly").GetValue<bool>();
                var drawEnemy = Menu.Item(Name + "ExperienceEnemy").GetValue<bool>();
                var drawSelf = Menu.Item(Name + "ExperienceSelf").GetValue<bool>();
                var thickness = Menu.Item(Name + "DrawingCircleThickness").GetValue<Slider>().Value;

                if (!drawAlly && !drawEnemy && !drawSelf)
                    return;

                var allyColor = Menu.Item(Name + "ExperienceColorAlly").GetValue<Color>();
                var enemyColor = Menu.Item(Name + "ExperienceColorEnemy").GetValue<Color>();
                var selfColor = Menu.Item(Name + "ExperienceColorSelf").GetValue<Color>();

                foreach (var hero in
                    HeroManager.AllHeroes.Where(hero => !hero.IsDead && hero.IsVisible)
                        .Where(
                            hero =>
                                (hero.IsAlly && drawAlly || hero.IsMe && drawSelf || hero.IsEnemy && drawEnemy) && !(hero.IsMe && !drawSelf) &&
                                hero.Position.IsOnScreen(ExperienceRange)))
                {
                    Render.Circle.DrawCircle(hero.Position, ExperienceRange, hero.IsMe ? selfColor : (hero.IsEnemy ? enemyColor : allyColor),
                        thickness);
                }
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        private void DrawSpell()
        {
            try
            {
                var thickness = Menu.Item(Name + "DrawingCircleThickness").GetValue<Slider>().Value;

                var drawAllyQ = Menu.Item(Name + "SpellAllyQ").GetValue<bool>();
                var drawAllyW = Menu.Item(Name + "SpellAllyW").GetValue<bool>();
                var drawAllyE = Menu.Item(Name + "SpellAllyE").GetValue<bool>();
                var drawAllyR = Menu.Item(Name + "SpellAllyR").GetValue<bool>();
                var drawAlly = drawAllyQ || drawAllyW || drawAllyE || drawAllyR;

                var drawEnemyQ = Menu.Item(Name + "SpellEnemyQ").GetValue<bool>();
                var drawEnemyW = Menu.Item(Name + "SpellEnemyW").GetValue<bool>();
                var drawEnemyE = Menu.Item(Name + "SpellEnemyE").GetValue<bool>();
                var drawEnemyR = Menu.Item(Name + "SpellEnemyR").GetValue<bool>();
                var drawEnemy = drawEnemyQ || drawEnemyW || drawEnemyE || drawEnemyR;

                var drawSelfQ = Menu.Item(Name + "SpellSelfQ").GetValue<bool>();
                var drawSelfW = Menu.Item(Name + "SpellSelfW").GetValue<bool>();
                var drawSelfE = Menu.Item(Name + "SpellSelfE").GetValue<bool>();
                var drawSelfR = Menu.Item(Name + "SpellSelfR").GetValue<bool>();
                var drawSelf = drawSelfQ || drawSelfW || drawSelfE || drawSelfR;

                if (!drawAlly && !drawEnemy && !drawSelf)
                    return;

                var spellMaxRange = Menu.Item(Name + "SpellMaxRange").GetValue<Slider>().Value;

                foreach (var hero in HeroManager.AllHeroes.Where(hero => !hero.IsDead && hero.IsVisible))
                {
                    if ((hero.IsAlly && drawAllyQ || hero.IsEnemy && drawEnemyQ || hero.IsMe && drawSelfQ) && !(hero.IsMe && !drawSelfQ))
                    {
                        var range = hero.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange;
                        if (range <= spellMaxRange && hero.Position.IsOnScreen(range))
                            Render.Circle.DrawCircle(hero.Position, range,
                                Menu.Item(Name + "Spell" + (hero.IsMe ? "Self" : (hero.IsEnemy ? "Enemy" : "Ally")) + "ColorQ").GetValue<Color>(),
                                thickness);
                    }
                    if ((hero.IsAlly && drawAllyW || hero.IsEnemy && drawEnemyW || hero.IsMe && drawSelfW) && !(hero.IsMe && !drawSelfW))
                    {
                        var range = hero.Spellbook.GetSpell(SpellSlot.W).SData.CastRange;
                        if (range <= spellMaxRange && hero.Position.IsOnScreen(range))
                            Render.Circle.DrawCircle(hero.Position, range,
                                Menu.Item(Name + "Spell" + (hero.IsMe ? "Self" : (hero.IsEnemy ? "Enemy" : "Ally")) + "ColorW").GetValue<Color>(),
                                thickness);
                    }
                    if ((hero.IsAlly && drawAllyE || hero.IsEnemy && drawEnemyE || hero.IsMe && drawSelfE) && !(hero.IsMe && !drawSelfE))
                    {
                        var range = hero.Spellbook.GetSpell(SpellSlot.E).SData.CastRange;
                        if (range <= spellMaxRange && hero.Position.IsOnScreen(range))
                            Render.Circle.DrawCircle(hero.Position, range,
                                Menu.Item(Name + "Spell" + (hero.IsMe ? "Self" : (hero.IsEnemy ? "Enemy" : "Ally")) + "ColorE").GetValue<Color>(),
                                thickness);
                    }
                    if ((hero.IsAlly && drawAllyR || hero.IsEnemy && drawEnemyR || hero.IsMe && drawSelfR) && !(hero.IsMe && !drawSelfR))
                    {
                        var range = hero.Spellbook.GetSpell(SpellSlot.R).SData.CastRange;
                        if (range <= spellMaxRange && hero.Position.IsOnScreen(range))
                            Render.Circle.DrawCircle(hero.Position, range,
                                Menu.Item(Name + "Spell" + (hero.IsMe ? "Self" : (hero.IsEnemy ? "Enemy" : "Ally")) + "ColorR").GetValue<Color>(),
                                thickness);
                    }
                }
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        private void DrawTurret()
        {
            try
            {
                var drawAlly = Menu.Item(Name + "TurretAlly").GetValue<bool>();
                var drawEnemy = Menu.Item(Name + "TurretEnemy").GetValue<bool>();
                var thickness = Menu.Item(Name + "DrawingCircleThickness").GetValue<Slider>().Value;

                if (!drawAlly && !drawEnemy)
                    return;

                var allyColor = Menu.Item(Name + "TurretColorAlly").GetValue<Color>();
                var enemyColor = Menu.Item(Name + "TurretColorEnemy").GetValue<Color>();

                foreach (var turret in
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(
                            t =>
                                t.IsValid && !t.IsDead && t.Health > 1f && t.IsVisible && (t.IsAlly && drawAlly || t.IsEnemy && drawEnemy) &&
                                t.Position.IsOnScreen(TurretRange)))
                {
                    Render.Circle.DrawCircle(turret.Position, TurretRange, turret.IsAlly ? allyColor : enemyColor, thickness);
                }
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        private void OnDrawingDraw(EventArgs args)
        {
            try
            {
                DrawExperience();
                DrawTurret();
                DrawAttack();
                DrawSpell();
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

                var drawingMenu = new Menu(Language.Get("G_Drawing"), Name + "Drawing");
                drawingMenu.AddItem(
                    new MenuItem(drawingMenu.Name + "CircleThickness", Language.Get("G_Circle") + " " + Language.Get("G_Thickness")).SetValue(
                        new Slider(2, 1, 10)));

                var experienceMenu = new Menu(Language.Get("G_Experience"), Name + "Experience");
                experienceMenu.AddItem(
                    new MenuItem(experienceMenu.Name + "ColorSelf", Language.Get("G_Color") + " " + Language.Get("G_Self")).SetValue(Color.Gray));
                experienceMenu.AddItem(
                    new MenuItem(experienceMenu.Name + "ColorAlly", Language.Get("G_Color") + " " + Language.Get("G_Ally")).SetValue(Color.Gray));
                experienceMenu.AddItem(
                    new MenuItem(experienceMenu.Name + "ColorEnemy", Language.Get("G_Color") + " " + Language.Get("G_Enemy")).SetValue(Color.Gray));
                experienceMenu.AddItem(new MenuItem(experienceMenu.Name + "Self", Language.Get("G_Self")).SetValue(false));
                experienceMenu.AddItem(new MenuItem(experienceMenu.Name + "Ally", Language.Get("G_Ally")).SetValue(false));
                experienceMenu.AddItem(new MenuItem(experienceMenu.Name + "Enemy", Language.Get("G_Enemy")).SetValue(false));

                var attackMenu = new Menu(Language.Get("G_Attack"), Name + "Attack");
                attackMenu.AddItem(
                    new MenuItem(attackMenu.Name + "ColorSelf", Language.Get("G_Color") + " " + Language.Get("G_Self")).SetValue(Color.Yellow));
                attackMenu.AddItem(
                    new MenuItem(attackMenu.Name + "ColorAlly", Language.Get("G_Color") + " " + Language.Get("G_Ally")).SetValue(Color.Yellow));
                attackMenu.AddItem(
                    new MenuItem(attackMenu.Name + "ColorEnemy", Language.Get("G_Color") + " " + Language.Get("G_Enemy")).SetValue(Color.Yellow));
                attackMenu.AddItem(new MenuItem(attackMenu.Name + "Self", Language.Get("G_Self")).SetValue(false));
                attackMenu.AddItem(new MenuItem(attackMenu.Name + "Ally", Language.Get("G_Ally")).SetValue(false));
                attackMenu.AddItem(new MenuItem(attackMenu.Name + "Enemy", Language.Get("G_Enemy")).SetValue(false));

                var turretMenu = new Menu(Language.Get("G_Turret"), Name + "Turret");
                turretMenu.AddItem(
                    new MenuItem(turretMenu.Name + "ColorAlly", Language.Get("G_Color") + " " + Language.Get("G_Ally")).SetValue(Color.DarkGreen));
                turretMenu.AddItem(
                    new MenuItem(turretMenu.Name + "ColorEnemy", Language.Get("G_Color") + " " + Language.Get("G_Enemy")).SetValue(Color.DarkRed));
                turretMenu.AddItem(new MenuItem(turretMenu.Name + "Ally", Language.Get("G_Ally")).SetValue(false));
                turretMenu.AddItem(new MenuItem(turretMenu.Name + "Enemy", Language.Get("G_Enemy")).SetValue(false));

                var spellMenu = new Menu(Language.Get("G_Spell"), Name + "Spell");
                spellMenu.AddItem(
                    new MenuItem(spellMenu.Name + "MaxRange",
                        Language.Get("G_Maximum") + " " + Language.Get("G_Spell") + " " + Language.Get("G_Range")).SetValue(new Slider(1000, 500, 3000)));

                var spellSelfMenu = new Menu(Language.Get("G_Self"), spellMenu.Name + "Self");
                spellSelfMenu.AddItem(new MenuItem(spellSelfMenu.Name + "ColorQ", Language.Get("G_Color") + " Q").SetValue(Color.Purple));
                spellSelfMenu.AddItem(new MenuItem(spellSelfMenu.Name + "ColorW", Language.Get("G_Color") + " W").SetValue(Color.Purple));
                spellSelfMenu.AddItem(new MenuItem(spellSelfMenu.Name + "ColorE", Language.Get("G_Color") + " E").SetValue(Color.Purple));
                spellSelfMenu.AddItem(new MenuItem(spellSelfMenu.Name + "ColorR", Language.Get("G_Color") + " R").SetValue(Color.Purple));
                spellSelfMenu.AddItem(new MenuItem(spellSelfMenu.Name + "Q", "Q").SetValue(false));
                spellSelfMenu.AddItem(new MenuItem(spellSelfMenu.Name + "W", "W").SetValue(false));
                spellSelfMenu.AddItem(new MenuItem(spellSelfMenu.Name + "E", "E").SetValue(false));
                spellSelfMenu.AddItem(new MenuItem(spellSelfMenu.Name + "R", "R").SetValue(false));

                spellMenu.AddSubMenu(spellSelfMenu);

                var spellAllyMenu = new Menu(Language.Get("G_Ally"), spellMenu.Name + "Ally");
                spellAllyMenu.AddItem(new MenuItem(spellAllyMenu.Name + "ColorQ", Language.Get("G_Color") + " Q").SetValue(Color.Green));
                spellAllyMenu.AddItem(new MenuItem(spellAllyMenu.Name + "ColorW", Language.Get("G_Color") + " W").SetValue(Color.Green));
                spellAllyMenu.AddItem(new MenuItem(spellAllyMenu.Name + "ColorE", Language.Get("G_Color") + " E").SetValue(Color.Green));
                spellAllyMenu.AddItem(new MenuItem(spellAllyMenu.Name + "ColorR", Language.Get("G_Color") + " R").SetValue(Color.Green));
                spellAllyMenu.AddItem(new MenuItem(spellAllyMenu.Name + "Q", "Q").SetValue(false));
                spellAllyMenu.AddItem(new MenuItem(spellAllyMenu.Name + "W", "W").SetValue(false));
                spellAllyMenu.AddItem(new MenuItem(spellAllyMenu.Name + "E", "E").SetValue(false));
                spellAllyMenu.AddItem(new MenuItem(spellAllyMenu.Name + "R", "R").SetValue(false));

                spellMenu.AddSubMenu(spellAllyMenu);

                var spellEnemyMenu = new Menu(Language.Get("G_Enemy"), spellMenu.Name + "Enemy");
                spellEnemyMenu.AddItem(new MenuItem(spellEnemyMenu.Name + "ColorQ", Language.Get("G_Color") + " Q").SetValue(Color.Red));
                spellEnemyMenu.AddItem(new MenuItem(spellEnemyMenu.Name + "ColorW", Language.Get("G_Color") + " W").SetValue(Color.Red));
                spellEnemyMenu.AddItem(new MenuItem(spellEnemyMenu.Name + "ColorE", Language.Get("G_Color") + " E").SetValue(Color.Red));
                spellEnemyMenu.AddItem(new MenuItem(spellEnemyMenu.Name + "ColorR", Language.Get("G_Color") + " R").SetValue(Color.Red));
                spellEnemyMenu.AddItem(new MenuItem(spellEnemyMenu.Name + "Q", "Q").SetValue(false));
                spellEnemyMenu.AddItem(new MenuItem(spellEnemyMenu.Name + "W", "W").SetValue(false));
                spellEnemyMenu.AddItem(new MenuItem(spellEnemyMenu.Name + "E", "E").SetValue(false));
                spellEnemyMenu.AddItem(new MenuItem(spellEnemyMenu.Name + "R", "R").SetValue(false));

                spellMenu.AddSubMenu(spellEnemyMenu);

                Menu.AddSubMenu(drawingMenu);
                Menu.AddSubMenu(experienceMenu);
                Menu.AddSubMenu(attackMenu);
                Menu.AddSubMenu(turretMenu);
                Menu.AddSubMenu(spellMenu);

                Menu.AddItem(new MenuItem(Name + "Enabled", Language.Get("G_Enabled")).SetValue(false));

                _parent.Menu.AddSubMenu(Menu);

                HandleEvents(_parent);
                RaiseOnInitialized();
            }
            catch (Exception ex)
            {
                Global.Logger.AddItem(new LogItem(ex));
            }
        }

        protected override void OnEnable()
        {
            Drawing.OnDraw += OnDrawingDraw;
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            Drawing.OnDraw -= OnDrawingDraw;
            base.OnDisable();
        }

        protected override void OnGameLoad(EventArgs args)
        {
            try
            {
                if (Global.IoC.IsRegistered<Drawings>())
                {
                    _parent = Global.IoC.Resolve<Drawings>();
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
    }
}