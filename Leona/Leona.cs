using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Color = System.Drawing.Color;

namespace Leona
{
    internal class Program
    {
        public static List<Spell> SpellList = new List<Spell>();
        public const string ChampionName = "Leona";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Menu Config;
        private static Obj_AI_Hero Player;

        static Program()
        {
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Program.Player = ObjectManager.Player;
            if (Player.BaseSkinName != "Leona") return;
            Q = new Spell(SpellSlot.Q, 125f);
            W = new Spell(SpellSlot.W, 250f);
            E = new Spell(SpellSlot.E, 875f);
            R = new Spell(SpellSlot.R, 1200f);

            E.SetSkillshot(0.25f, 80f, 1225f, false, Prediction.SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 315f, float.MaxValue, false, Prediction.SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Config = new Menu(ChampionName, ChampionName, true);

            // Targetselector menu
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            // Orbwalker menu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            // Combo menu
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            //Config.SubMenu("Combo").AddItem(new MenuItem("WhenAA", "When Q").SetValue(new StringList(new[] { "Before AA", "On AA", "After AA"}, 0)));
            Config.SubMenu("Combo").AddItem(new MenuItem("WhenAA", "When Q").SetValue(new StringList(new[] { "Before AA", "On AA", "After AA" }, 0)));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            // Harass menu
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Ultimate", "Ultimate"));
            Config.SubMenu("Ultimate").AddItem(new MenuItem("AutoR", "Manual R").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));



            // Misc menu
            //  Config.AddSubMenu(new Menu("Misc", "Misc"));
            //Config.SubMenu("Misc").AddItem(new MenuItem("AutoR", "Auto Ultimate (Not fixed)").SetValue(new Slider(3, 5, 1)));
            //  Config.SubMenu("Misc").AddItem(new MenuItem("WhenAA", "When to use Q").SetValue(new StringList(new[] { "Before AA", "On AA", "After AA" }, 1)));


            // Drawings menu
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(true));

            // Add everything to main menu
            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.OnAttack += Orbwalker_OnAttack;
            Orbwalking.BeforeAttack += Orbwalker_BeforeAttack;
            Orbwalking.AfterAttack += Orbwalker_AfterAttack;

        }

        private static void Orbwalker_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (!unit.IsMe || !(target is Obj_AI_Hero)) return;
            var iAfter = Config.Item("WhenAA").GetValue<StringList>().SelectedIndex;

            if (iAfter == 2)
            {
                if ((Config.Item("ComboActive").GetValue<KeyBind>().Active) && Config.Item("UseQ").GetValue<bool>())
                {
                    Q.Cast();
                }
                if ((Config.Item("HarassActive").GetValue<KeyBind>().Active) && Config.Item("UseQHarass").GetValue<bool>())
                {
                    Q.Cast();
                }
            }
        }

        private static void Orbwalker_OnAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (!unit.IsMe || !(target is Obj_AI_Hero)) return;
            var iOn = Config.Item("WhenAA").GetValue<StringList>().SelectedIndex;

            if (iOn == 1)
            {
                if ((Config.Item("ComboActive").GetValue<KeyBind>().Active) && Config.Item("UseQ").GetValue<bool>())
                {
                    Q.Cast();
                }
                if ((Config.Item("HarassActive").GetValue<KeyBind>().Active) && Config.Item("UseQHarass").GetValue<bool>())
                {
                    Q.Cast();
                }
            }
        }

        private static void Orbwalker_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var iBefore = Config.Item("WhenAA").GetValue<StringList>().SelectedIndex;

            if (iBefore == 0)
            {
                if ((Config.Item("ComboActive").GetValue<KeyBind>().Active) && Config.Item("UseQ").GetValue<bool>())
                {
                    Q.Cast();
                }
                if ((Config.Item("HarassActive").GetValue<KeyBind>().Active) && Config.Item("UseQHarass").GetValue<bool>())
                {
                    Q.Cast();
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Draw our E range
            if (Config.Item("ERange").GetValue<bool>())
            {
                Drawing.DrawCircle(Player.Position, E.Range, Color.Yellow);
            }

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetAttacks(true);
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();
            }
            if (Config.Item("AutoR").GetValue<KeyBind>().Active)
            {
                AimR();
            }

            //Need to implement the Auto Ultimate 
        }

        private static void AimR()
        {
            var rtarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            if (R.IsReady() && (rtarget != null))
            {
                R.CastIfHitchanceEquals(rtarget, Prediction.HitChance.HighHitchance, true);
            }
        }

        private static void Harass()
        {
            var htarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            Orbwalker.SetAttacks(true);

            if (htarget != null)
            {
                if (Player.Distance(htarget) <= E.Range)
                {
                    if (Config.Item("UseEHarass").GetValue<bool>() && E.IsReady())
                    {
                        E.CastIfHitchanceEquals(htarget, Prediction.HitChance.HighHitchance, true);
                    }
                    if (Config.Item("UseWHarass").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast();
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            Orbwalker.SetAttacks(!(E.IsReady() || Player.Distance(target) >= E.Range));

            if (target != null)
            {
                if (Player.Distance(target) <= E.Range)
                {
                    if (Config.Item("UseE").GetValue<bool>() && E.IsReady())
                    {
                        E.CastIfHitchanceEquals(target, Prediction.HitChance.HighHitchance, true);
                    }
                    if (Config.Item("UseW").GetValue<bool>() && W.IsReady() && (Player.Distance(target) <= 450))
                    {
                        W.Cast();
                    }
                    if (Config.Item("UseR").GetValue<bool>() && R.IsReady() && (Player.Distance(target) <= R.Range) && (target.HasBuff("leonazenithbladeroot")))
                    {
                        R.CastOnUnit(target, true);
                    }
                }
                Orbwalker.SetAttacks(true);
            }
        }
    }
}
