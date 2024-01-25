using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GTA;
using System.Drawing;
using System.Windows.Forms;

namespace HotelRoomIV
{
    public class HotelRoomIV : Script
    {
        #region Fields
        SettingsFile Inifile = SettingsFile.Open(".\\scripts\\Okoniewitz\\Hotel\\config.ini");
        bool FirstTimeBlipBuy, FirstTimeBlipHotel, OwnedFirst, ShowTipBool, firstTime = true, Saved;
        int Timer1, Timer3;

        string Text; bool Background;
        bool TipBool; int TipLenght;

        Vector3 HotelPosition = new Vector3(-166.27f, 595.42f, 15.71f);
        Blip HotelMarker, BuyMarker, RoomMarker;
        bool FirstTimeBlipBuy2;
        bool Fading;
        bool lockTeleport;
        int Timer2;
        bool FadeBool;
        int FadeAlpha = 0;
        int saves;
        bool InPublic; int TimePublic;

        #endregion

        #region ConfigFields
        int Price;
        bool debug, Owned;
        bool Hotel;
        #endregion

        public HotelRoomIV()
        {
            ConfigRead();
            Tick += Main;
            PerFrameDrawing += Graphics;
            saves = Game.GetIntegerStatistic(IntegerStatistic.SAVES_MADE);
        }


        void Main(System.Object sender, EventArgs e)
        {
            if (debug)
            {
                if (Game.isGameKeyPressed(GameKey.NavDown))
                {
                    Player.Money += 1000;
                    Owned = true;
                }
                if(Game.isGameKeyPressed(GameKey.NavLeft))
                {
                    TimeSpan ts = World.CurrentDayTime;
                    if (ts.Hours >= 18)
                    {
                        GTA.Native.Function.Call("SET_TIME_ONE_DAY_FORWARD");
                    }
                    GTA.Native.Function.Call("SET_TIME_OF_DAY", ts.Hours + 6, ts.Minutes);
                }
            }
            if (firstTime && CanBuy())
            {
                HotelMarker = Blip.AddBlip(new Vector3(-166.16f, 605.42f, 14.71f));
                HotelMarker.Name = "Majestic Hotel";
                HotelMarker.Icon = BlipIcon.Building_Safehouse;
                HotelMarker.RouteActive = false;
                HotelMarker.ShowOnlyWhenNear = false;
                HotelMarker.Display = BlipDisplay.MapOnly;
                HotelMarker.Color = BlipColor.Red;
                if(Owned && Hotel)
                {
                    Player.Character.Position = new Vector3(-180.73f, 587.17f, 122.78f);
                    Player.Character.Heading = 0;
                    Game.DefaultCamera.Rotation = Game.DefaultCamera.Rotation;
                    Player.Character.Health = 100;
                    Game.SetIntegerStatistic(IntegerStatistic.SAVES_MADE, Game.GetIntegerStatistic(IntegerStatistic.SAVES_MADE) + 1);
                    saves = Game.GetIntegerStatistic(IntegerStatistic.SAVES_MADE);
                    if (World.CurrentDayTime.Hours >= 18)
                    {
                        GTA.Native.Function.Call("SET_TIME_ONE_DAY_FORWARD");
                    }
                    GTA.Native.Function.Call("SET_TIME_OF_DAY", World.CurrentDayTime.Hours + 6, World.CurrentDayTime.Minutes);
                }
                firstTime = false;
            }
            if (!Owned && CanBuy()) NotOwned();
            if (Owned && CanBuy()) RoomOwned();
            if (Game.GameTime >= Timer1) TipBool = false;
            if (Game.GameTime >= Timer2)
            {
                FadeBool = true;
                Fade(InPublic, TimePublic);
            }
            if (Game.GetIntegerStatistic(IntegerStatistic.SAVES_MADE) > saves)
            {
                Inifile.SetValue("Hotel", "SAVE", false);
                Inifile.Save();
            }
        }
        void Graphics(System.Object sender, GraphicsEventArgs e)
        {
            if (TipBool)
            {
                if (Background) e.Graphics.DrawRectangle(TipLenght / 2 + 95, 55 + 22, TipLenght, 44, Color.FromArgb(120, Color.Black));
                e.Graphics.DrawText(Text, 110, 60);
            }
            e.Graphics.DrawRectangle(1920 / 2, 1080 / 2, 1920, 1080, Color.FromArgb(FadeAlpha, 0, 0, 0));
        }

        void ConfigRead()
        {
            Inifile.Load();
            Price = Inifile.GetValueInteger("Price", "CONFIG", 150000);
            debug = Inifile.GetValueBool("Debug", "DEBUG", false);
            Owned = Inifile.GetValueBool("Owned", "SAVE", false);
            Hotel = Inifile.GetValueBool("Hotel", "SAVE", false);
        }

        #region Bools
        bool InHotel()
        {
            if (Player.Character.isInArea(new Vector3(-183f, 609f, 14), new Vector3(-150f, 591, 18), false))
            {
                return true;
            }
            else return false;
        }
        bool NextToReception()
        {
            if (Player.Character.Position.DistanceTo(HotelPosition) <= 1.5)
            {
                return true;
            }
            else return false;
        }
        bool NextToElevator()
        {
            if (Player.Character.Position.DistanceTo(new Vector3(-150.95f, 597.31f, 15.61f)) <= 1 || Player.Character.Position.DistanceTo(new Vector3(-161.03f, 592.34f, 118.79f)) <= 1)
            {
                return true;
            }
            else return false;
        }
        bool NextToBed()
        {
            if (Player.Character.Position.DistanceTo(new Vector3(-183.57f, 579, 122.78f)) <= 1.5 || Player.Character.Position.DistanceTo(new Vector3(-183.57f, 582.53f, 122.78f)) <= 1.5)
                return true;
            else return false;
        }
        bool InHotelRoom()
        {
            if (Owned)
            {
                if (Player.Character.isInArea(new Vector3(-147.60f, 600.77f, 115), new Vector3(-185.77f, 575.65f, 132f), false))
                    return true;
                else return false;
            }
            else return false;
        }
        bool CanBuy()
        {
            if (Game.GetIntegerStatistic(IntegerStatistic.ISLANDS_UNLOCKED) > 1)
                return true;
            else return false;
        }

        #endregion

        void NotOwned()
        {
            if (InHotel())
            {
                if (NextToReception())
                {
                    if (FirstTimeBlipBuy)
                    {
                        BuyMarker.Delete(); FirstTimeBlipBuy = false;
                    }
                    Tip("Click E to buy a hotel room for " + Price + "$", true, 450, 100);
                    if (Game.isKeyPressed(Keys.E))
                    {
                        if (Player.Money >= Price)
                        {
                            Owned = true;
                            Inifile.SetValue("Owned", "SAVE", true);
                            Inifile.Save();
                            Player.Money -= Price;
                            Tip("You bought a Majestic Hotel room!", true, 407, 2000);
                        }
                        else Tip("You dont have enough money!", true, 363, 1500);
                        Wait(1500);

                    }
                }
                if(!NextToReception())
                {
                    if (!FirstTimeBlipBuy)
                    {
                        if (!FirstTimeBlipBuy2) Tip("Go to receptionist to buy a hotel room", true, 450, 3000);
                        FirstTimeBlipBuy2 = true;
                        FirstTimeBlipBuy = true;
                        BuyMarker = Blip.AddBlip(HotelPosition);
                        BuyMarker.Display = BlipDisplay.ArrowAndMap;
                        BuyMarker.Color = BlipColor.Cyan;
                    }
                }
            }
            if (!InHotel())
            {
                if(FirstTimeBlipBuy)
                {
                    BuyMarker.Delete();
                    FirstTimeBlipBuy2 = false;
                    FirstTimeBlipBuy = false;
                }
            }
        }
        void RoomOwned()
        {
            if (!OwnedFirst)
            {
                if (FirstTimeBlipBuy)
                {
                    BuyMarker.Delete();
                    FirstTimeBlipBuy = false;
                }
                HotelMarker.Color = BlipColor.Green;
                OwnedFirst = true;
            }
            if (!FirstTimeBlipHotel && !NextToElevator() && InHotel())
            {
                RoomMarker = Blip.AddBlip(new Vector3(-150.95f, 597.31f, 15.61f));
                FirstTimeBlipHotel = true;
            }
            if (NextToElevator()) Elevator(); else lockTeleport = false;
            if (!InHotel())
            {
                if (FirstTimeBlipHotel)
                {
                    RoomMarker.Delete();
                    FirstTimeBlipHotel = false;
                }
            }
            if (InHotelRoom()) InRoom();
        }
        void InRoom()
        {
            if (NextToBed())
            {
                Tip("Click F to save the game", true, 300, 100);
                if (Game.isKeyPressed(Keys.F))
                {
                    Timer3 = Game.GameTime + 100;
                    Saved = true;
                    Game.ShowSaveMenu();
                }

            }
            if(Game.GameTime >=Timer3 && Saved)
            {
                if (GTA.Native.Function.Call<bool>("DID_SAVE_COMPLETE_SUCCESSFULLY"))
                {
                    Inifile.SetValue("Hotel", "SAVE", true);
                    Inifile.Save();
                    Player.Character.Health = 100;
                    Saved = false;
                    Game.SetIntegerStatistic(IntegerStatistic.SAVES_MADE, Game.GetIntegerStatistic(IntegerStatistic.SAVES_MADE) + 1);
                    saves = Game.GetIntegerStatistic(IntegerStatistic.SAVES_MADE);
                    if (World.CurrentDayTime.Hours >= 18)
                    {
                        GTA.Native.Function.Call("SET_TIME_ONE_DAY_FORWARD");
                    }
                    GTA.Native.Function.Call("SET_TIME_OF_DAY", World.CurrentDayTime.Hours + 6, World.CurrentDayTime.Minutes);
                }
            }
        }
        void Elevator()
        {
            if (FirstTimeBlipHotel)
            {
                RoomMarker.Delete();
                FirstTimeBlipHotel = false;
            }
            if (InHotelRoom() && !lockTeleport)
            {
                Fade(true, 400);
                if (FadeAlpha > 250 && !Fading)
                {
                    Player.Character.Position = new Vector3(-150.95f, 597.31f, 15.61f);
                    Player.Character.Heading = 90;
                    Game.DefaultCamera.Rotation = Game.DefaultCamera.Rotation;
                    lockTeleport = true;
                    Wait(1000);
                    Fade(false, 400);
                }
            }

            if (InHotel() && !lockTeleport)
            {
                Fade(true, 400);
                if (FadeAlpha > 250 && !Fading)
                {
                    Player.Character.Position = new Vector3(-161.03f, 592.34f, 118.79f);
                    Player.Character.Heading = 90;
                    Game.DefaultCamera.Rotation = Game.DefaultCamera.Rotation;
                    lockTeleport = true;
                    Wait(1000);
                    Fade(false, 400);
                }
            }
        }


        #region Methods
        void Tip(string text, bool background, int tiplenght, int time)
        {
            ShowTipBool = true;
            if (ShowTipBool)
            {
                ShowTipBool = false;
                Text = text;
                Background = background;
                TipLenght = tiplenght;
                TipBool = true;
                Timer1 = Game.GameTime + time;
            }
        }

        void Fade(bool In, int Time)
        {
            InPublic = In; TimePublic = Time;
            if (FadeBool)
            {
                if (In && FadeAlpha <= 250)
                {
                    FadeAlpha += 5;
                }
                if (!In && FadeAlpha >= 5)
                {
                    FadeAlpha -= 5;
                }
                FadeBool = false;
                Timer2 = Game.GameTime + Time / 50;
            }
            if (FadeAlpha >= 250 || FadeAlpha <= 5) Fading = false; else Fading = true;
        }
        #endregion
    }
}