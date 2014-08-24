using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SFTemplate
{
    class Program
    {
        public static string ChampName = "Alistar";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base Player = ObjectManager.Player;
        public static Spell Q, W;
        public static Items.Item DFG;
        public static Menu Config;
        public static float WQRange = 375f;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 365f);
            W = new Spell(SpellSlot.W, 650f);
            DFG = new Items.Item(3128, 750f);

            //Base menu
            Config = new Menu(ChampName, ChampName, true);

            //Orbwalker and menu
            Config.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
            
            //Target selector 
            var ts = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(ts);
            Config.AddSubMenu(ts);

            //Combo menu
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("useQ", "Use Q?").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("useW", "Use W?").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Killsteal menu
            Config.AddSubMenu(new Menu("Killsteal", "Killsteal"));
            Config.SubMenu("Killsteal").AddItem(new MenuItem("useQks", "Use Q?").SetValue(true));
            Config.SubMenu("Killsteal").AddItem(new MenuItem("useWks", "Use W?").SetValue(true));
            Config.SubMenu("Killsteal").AddItem(new MenuItem("ks", "KillSteal?").SetValue(true));

            //Interrupt menu
            Config.AddSubMenu(new Menu("Interrupt", "Interrupt"));
            Config.SubMenu("Interrupt").AddItem(new MenuItem("interrupt", "Interrupt?").SetValue(true));

            //Drawing menu
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W Range").SetValue(true));
            
            //Exploits
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("NFE", "PacketCast").SetValue(true));

            //Make the menu visible
            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPosibleToInterrupt += Interrupter_OnPosibleToInterrupt;

            Game.PrintChat(ChampName + " loaded!");
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("interrupt").GetValue<bool>()) return;

            if (Player.Distance(unit) < W.Range && W.IsReady())
            {
                W.CastOnUnit(unit, Config.Item("NFE").GetValue<bool>());
            } 
            else if (Player.Distance(unit) < Q.Range && Q.IsReady())
            {
                Q.Cast();
            }
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetAttacks(true);
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (Config.Item("ks").GetValue<bool>())
            {
                KillSteal();
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("QRange").GetValue<bool>())
            {
                Drawing.DrawCircle(Player.Position, Q.Range, Color.Yellow);
            }
            if (Config.Item("WRange").GetValue<bool>())
            {
                Drawing.DrawCircle(Player.Position, W.Range, Color.Yellow);
            }
        }

        public static void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(Q.Range)))
            {
                if (Q.IsReady() && hero.Distance(ObjectManager.Player) <= Q.Range && DamageLib.getDmg(hero, DamageLib.SpellType.Q) >= hero.Health) Q.Cast();
            }

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(W.Range)))
            {
                if (W.IsReady() && hero.Distance(ObjectManager.Player) <= W.Range && DamageLib.getDmg(hero, DamageLib.SpellType.W) >= hero.Health) W.CastOnUnit(hero, Config.Item("NFE").GetValue<bool>());
            }
        }


        public static void Combo()
        {
            var target = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;

            
            Orbwalker.SetAttacks(!(Q.IsReady() || W.IsReady() || Player.Distance(target) >= W.Range));

            if (Player.Distance(target) <= W.Range)
            {
                if (Config.Item("useW").GetValue<bool>() && Config.Item("useQ").GetValue<bool>() && W.IsReady() && Q.IsReady() && (Player.Distance(target) > WQRange))
                {
                    if (DFG.IsReady())
                    {
                        DFG.Cast(target);
                    }
                    W.CastOnUnit(target, Config.Item("NFE").GetValue<bool>());
                }
                else if (Config.Item("useQ").GetValue<bool>() && Q.IsReady() && (Player.Distance(target) <= Q.Range))
                {
                    Q.Cast();
                }
            }
        }

    }
}