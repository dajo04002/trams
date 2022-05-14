using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace SpaceRace2
{
    internal class Game1 : Game
    {
        #region Variabler
        private GraphicsDeviceManager graphics;
        private SpriteBatch sprites;
        private SpriteFont consolas16;
        private SpriteFont consolas30;
        private Texture2D skeppTexture;
        private Texture2D debrisTexture;
        private Texture2D timerTexture;
        private Texture2D playActiveTexture;
        private Texture2D playDullTexture;
        private Texture2D volDotTexture;
        private Texture2D volSliderTexture;

        private Rectangle playButtonBox;
        private Rectangle volDotRect;
        private Rectangle volSliderRect;

        private Vector2 p1;
        private Vector2 p2;
        private Vector2 spelTimer;
        private Vector2 volDot;
        private Vector2 volSlider;

        private List<Vector2> debrisLeft;
        private List<Vector2> debrisRight;

        private Song music;

        private int debrisTimer = 20;
        private int debrisDelay = 7; // frequency av debris, kan ändras med hjälp av att svårighetsgrad väljs
        private int easy = 50;
        private int medium = 25;
        private int hard = 7;
        private int p1StartX = 250;
        private int p2StartX = 500;
        private int startY = 445;
        private int p1Score = 0;
        private int p2Score = 0;
        private int p1ScoreAfter;
        private int p2ScoreAfter;
        private int volDotBoundUp = 15;
        private int volDotBoundDown = 102;

        private const int volDotStartX = 800 - 50;

        private float musicVolume;

        private string difficulty;

        private bool p1Hit;
        private bool p2Hit;
        private bool isPlaying;

        Random random = new Random();

        #endregion
        public Game1()

            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsFixedTimeStep = true;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            p1 = new Vector2(p1StartX, startY);
            p2 = new Vector2(p2StartX, startY);
            spelTimer = new Vector2(GraphicsDevice.Viewport.Width/2 -10, GraphicsDevice.Viewport.Height);

            volDot = new Vector2(GraphicsDevice.Viewport.Width - 50, 13);
            volSlider = new Vector2(GraphicsDevice.Viewport.Width - 41, 25);

            debrisLeft = new List<Vector2>();
            debrisRight = new List<Vector2>();

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.01f;
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            sprites = new SpriteBatch(GraphicsDevice);

            consolas16 = Content.Load<SpriteFont>("Consolas16");
            consolas30 = Content.Load<SpriteFont>("Consolas30");

            skeppTexture = Content.Load<Texture2D>("skepp");
            debrisTexture = Content.Load<Texture2D>("debris");
            timerTexture = Content.Load<Texture2D>("timer");

            volDotTexture = Content.Load<Texture2D>("VolumeDot2");
            volSliderTexture = Content.Load<Texture2D>("VolumeSlider");
            playActiveTexture = Content.Load<Texture2D>("playButtonActive");
            playDullTexture = Content.Load<Texture2D>("playButtonDull");

            playButtonBox = new Rectangle(400 - 125, 30, playActiveTexture.Width, playActiveTexture.Height);

            music = Content.Load<Song>("Music");
            MediaPlayer.Play(music);


            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            var keyState = Keyboard.GetState();
            var mouseState = Mouse.GetState();
            var mousePosition = new Point(mouseState.X, mouseState.Y);

            if (!isPlaying)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.D1))
                {
                    debrisDelay = easy;
                    difficulty = "Easy";
                }
                    
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.D2))
                {
                    debrisDelay = medium;
                    difficulty = "Medium";
                }
                    
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.D3))
                {
                    debrisDelay = hard;
                    difficulty = "Hard";
                }
            }

            #region Volymslider

            volDotRect = new Rectangle((int)volDot.X, (int)volDot.Y, volDotTexture.Width, volDotTexture.Height);
            volSliderRect = new Rectangle((int)volSlider.X, (int)volSlider.Y, volSliderTexture.Width, volSliderTexture.Height);

            if (!isPlaying) 
            { 
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Up))
                volDot.Y--;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Down))
                volDot.Y++;
            }

            if (volDot.Y < volDotBoundUp)
                volDot.Y = volDotBoundUp;
            if (volDot.Y > volDotBoundDown)
                volDot.Y = volDotBoundDown;

            if (volSliderRect.Contains(mousePosition) | volDotRect.Contains(mousePosition))
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    volDot = new Vector2(mouseState.X, mouseState.Y - 17);
                    volDot.X = volDotStartX;
                    if (volDot.Y < volDotBoundUp)
                        volDot.Y = volDotBoundUp;
                    if (volDot.Y > volDotBoundDown)
                        volDot.Y = volDotBoundDown;
                }
            }

            musicVolume = volDot.Y/800;
            MediaPlayer.Volume = musicVolume;

            #endregion

            if (keyState.IsKeyDown(Keys.Enter) && !isPlaying)
                isPlaying = true;

            if (!isPlaying)
                return;

            if (spelTimer.Y > GraphicsDevice.Viewport.Height) //om speltimern når slutet av skärmen, kalla på Reset() funktionen
                Reset();

            spelTimer = spelTimer + new Vector2(0, 0.40f); //flytta texturen nedåt så att det ser ut som en timer/spalt som rör sig nedåt

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Escape))
                Exit();

            #region Movement

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.W))
                p1.Y--;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.S))
                p1.Y++;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Up))
                p2.Y--;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyState.IsKeyDown(Keys.Down))
                p2.Y++;

            #endregion

            #region Score/OutofBounds
            if (p1.Y <= 0)
            {
                p1.Y = startY;
                p1Score++;
            }
            else if (p1.Y >= startY)
            {
                p1.Y = startY;
            }

            if (p2.Y <= 0)
            {
                p2.Y = startY;
                p2Score++;
            }
            else if (p2.Y >= startY)
            {
                p2.Y = startY;
            }

            #endregion

            #region Debrisspawn
            debrisTimer--;
            if (debrisTimer <= 0)
            {
                debrisTimer = debrisDelay;
                debrisLeft.Add(new Vector2(0, random.Next(0, 420)));
                debrisRight.Add(new Vector2(800, random.Next(0, 420)));
            }

            for (int i = 0; i < debrisLeft.Count; i++)
            {
                debrisLeft[i] = debrisLeft[i] + new Vector2(2, 0);
            }

            for (int i = 0; i < debrisRight.Count; i++)
            {
                debrisRight[i] = debrisRight[i] + new Vector2(-2, 0);
            }

            #endregion

            #region Kollision

            Rectangle p1Box = new Rectangle((int)p1.X, (int)p1.Y, skeppTexture.Width, skeppTexture.Height);
            Rectangle p2Box = new Rectangle((int)p2.X, (int)p2.Y, skeppTexture.Width, skeppTexture.Height);

            p1Hit = false;
            p2Hit = false;

            foreach (var debris in debrisLeft) //kolla om skepp 1 kolliderar med den debris som kommer från vänster
            {
                Rectangle debrisBox = new Rectangle((int)debris.X, (int)debris.Y, debrisTexture.Width, debrisTexture.Height);

                var kollision = Intersection(p1Box, debrisBox);

                if (kollision.Width > 0 && kollision.Height > 0)
                {
                    Rectangle r1 = Normalize(p1Box, kollision);
                    Rectangle r2 = Normalize(debrisBox, kollision);
                    p1Hit = TestCollision(skeppTexture, r1, debrisTexture, r2);
                }
            }

            foreach (var debris in debrisLeft) //kolla om skepp 2 kolliderar med den debris som kommer från vänster
            {
                Rectangle debrisBox = new Rectangle((int)debris.X, (int)debris.Y, debrisTexture.Width, debrisTexture.Height);

                var kollision = Intersection(p2Box, debrisBox);

                if (kollision.Width > 0 && kollision.Height > 0)
                {
                    Rectangle r1 = Normalize(p2Box, kollision);
                    Rectangle r2 = Normalize(debrisBox, kollision);
                    p2Hit = TestCollision(skeppTexture, r1, debrisTexture, r2);
                }
            }

            foreach (var debris in debrisRight) //kolla om skepp 1 kolliderar med den debris som kommer från höger
            {
                Rectangle debrisBox = new Rectangle((int)debris.X, (int)debris.Y, debrisTexture.Width, debrisTexture.Height);

                var kollision = Intersection(p1Box, debrisBox);

                if (kollision.Width > 0 && kollision.Height > 0)
                {
                    Rectangle r1 = Normalize(p1Box, kollision);
                    Rectangle r2 = Normalize(debrisBox, kollision);
                    p1Hit = TestCollision(skeppTexture, r1, debrisTexture, r2);
                }
            }

            foreach (var debris in debrisRight) //kolla om skepp 2 kolliderar med den debris som kommer från höger
            {
                Rectangle debrisBox = new Rectangle((int)debris.X, (int)debris.Y, debrisTexture.Width, debrisTexture.Height);

                var kollision = Intersection(p2Box, debrisBox);

                if (kollision.Width > 0 && kollision.Height > 0)
                {
                    Rectangle r1 = Normalize(p2Box, kollision);
                    Rectangle r2 = Normalize(debrisBox, kollision);
                    p2Hit = TestCollision(skeppTexture, r1, debrisTexture, r2);
                }
            }

            if (p1Hit) //resetta positionen på skepp 1 om kollision är sant
            {
                p1.X = p1StartX;
                p1.Y = startY;
            }

            if (p2Hit) //resetta positionen på skepp 2 om kollision är sant
            {
                p2.X = p2StartX;
                p2.Y = startY;
            }

            #endregion

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            var mouseState = Mouse.GetState();
            var mousePosition = new Point(mouseState.X, mouseState.Y);


            sprites.Begin();

            sprites.DrawString(consolas16, "Difficulty: " + difficulty, new Vector2(60, 40), Color.White);

            if (isPlaying)
            {
                sprites.Draw(skeppTexture, p1, Color.White);
                sprites.Draw(skeppTexture, p2, Color.White);
                sprites.Draw(timerTexture, spelTimer, Color.White);

                sprites.DrawString(consolas16, p1Score.ToString(), new Vector2(30, 30), Color.White); //score
                sprites.DrawString(consolas16, p2Score.ToString(), new Vector2(770, 30), Color.White);

                foreach (var debris in debrisLeft) //rita upp all debris
                {
                    sprites.Draw(debrisTexture, debris, Color.White);
                }

                foreach (var debris in debrisRight)
                {
                    sprites.Draw(debrisTexture, debris, Color.White);
                }

                
            }
            else
            {
                sprites.Draw(playDullTexture, playButtonBox, Color.White); //om isPlaying är false, alltså om spelaren inte spelar så ritas en play knapp upp

                sprites.Draw(volSliderTexture, volSlider , Color.White);
                sprites.Draw(volDotTexture, volDot, Color.White);

                sprites.DrawString(consolas16, musicVolume.ToString(), new Vector2(5, 100), Color.White);


                if (playButtonBox.Contains(mousePosition)) //hover, but texture till en som är mer ljus så att du ser att du håller över knappen
                {
                    sprites.Draw(playActiveTexture, playButtonBox, Color.White);
                    if (mouseState.LeftButton == ButtonState.Pressed) //om klick på knapp, starta spelet
                    {
                        isPlaying = true;
                    }
                }

                sprites.DrawString(consolas16, "Choose difficulty", new Vector2(GraphicsDevice.Viewport.Width / 2 - 70, GraphicsDevice.Viewport.Height / 2 + 80), Color.White);
                sprites.DrawString(consolas16, "Easy = 1", new Vector2(GraphicsDevice.Viewport.Width / 2 - 70, GraphicsDevice.Viewport.Height / 2 + 100), Color.White);
                sprites.DrawString(consolas16, "Medium = 2", new Vector2(GraphicsDevice.Viewport.Width / 2 - 70, GraphicsDevice.Viewport.Height / 2 + 120), Color.White);
                sprites.DrawString(consolas16, "Hard = 3", new Vector2(GraphicsDevice.Viewport.Width / 2 - 70, GraphicsDevice.Viewport.Height / 2 + 140), Color.White);

                if (Winner(p1ScoreAfter, p2ScoreAfter) == 1)
                    sprites.DrawString(consolas30, "Player 1 Wins", new Vector2(GraphicsDevice.Viewport.Width / 2 - 150, 200), Color.White);

                else if(Winner(p1ScoreAfter, p2ScoreAfter) == 2)
                    sprites.DrawString(consolas30, "Player 2 Wins", new Vector2(GraphicsDevice.Viewport.Width / 2 - 150, 200), Color.White);

                else if(Winner(p1ScoreAfter, p2ScoreAfter) == 3)
                    sprites.DrawString(consolas30, "Draw", new Vector2(GraphicsDevice.Viewport.Width / 2 - 150, 200), Color.White);
            }

            sprites.End();
            base.Draw(gameTime);
        }

        private void Reset()
        {
            debrisLeft.Clear();
            debrisRight.Clear();
            p1.X = p1StartX;
            p1.Y = startY;
            p2.X = p2StartX;
            p2.Y = startY;
            isPlaying = false;
            spelTimer.Y = 0;
            p1ScoreAfter = p1Score;
            p2ScoreAfter = p2Score;
            p1Score = 0;
            p2Score = 0;
        }

        public static int Winner(int p1, int p2)
        {
            if (p1 > p2)
                return 1;

            else if (p1 < p2)
                return 2;

            else if (p1 == p2 & p1 != 0 & p2 != 0)
                return 3;

            else
                return 0;
        }
        //kod nedan från Csharpskolan
        #region PerPixelCollision 

        public static Rectangle Intersection(Rectangle r1, Rectangle r2)
        {
            int x1 = Math.Max(r1.Left, r2.Left);
            int y1 = Math.Max(r1.Top, r2.Top);
            int x2 = Math.Min(r1.Right, r2.Right);
            int y2 = Math.Min(r1.Bottom, r2.Bottom);

            if ((x2 >= x1) && (y2 >= y1))
            {
                return new Rectangle(x1, y1, x2 - x1, y2 - y1);
            }
            return Rectangle.Empty;
        }
        public static Rectangle Normalize(Rectangle reference, Rectangle overlap)
        {
            //Räkna ut en rektangel som kan användas relativt till referensrektangeln
            return new Rectangle(
              overlap.X - reference.X,
              overlap.Y - reference.Y,
              overlap.Width,
              overlap.Height);
        }
        public static bool TestCollision(Texture2D t1, Rectangle r1, Texture2D t2, Rectangle r2)
        {
            //Beräkna hur många pixlar som finns i området som ska undersökas
            int pixelCount = r1.Width * r1.Height;
            uint[] texture1Pixels = new uint[pixelCount];
            uint[] texture2Pixels = new uint[pixelCount];

            //Kopiera ut pixlarna från båda områdena
            t1.GetData(0, r1, texture1Pixels, 0, pixelCount);
            t2.GetData(0, r2, texture2Pixels, 0, pixelCount);

            //Jämför om vi har några pixlar som överlappar varandra i områdena
            for (int i = 0; i < pixelCount; ++i)
            {
                if (((texture1Pixels[i] & 0xff000000) > 0) && ((texture2Pixels[i] & 0xff000000) > 0))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}