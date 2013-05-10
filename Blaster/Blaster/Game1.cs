using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Blaster
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        
        GraphicsDevice device;

        public static int ScreenWidth;
        public static int ScreenHeight;

        Texture2D backgroundTexture;
        Texture2D tankTexture;
        Texture2D cannonTexture;
        Texture2D bulletTexture;
        Texture2D alienTexture;
        Texture2D explodeTexture;

        Vector2 tankPosition;
        Vector2 cannonPosition;
        Vector2 bulletPosition;
        Vector2 alienPosition;

        Vector2 bulletDirection;

        float cannonAngle;
        float bulletAngle;

        int screenWidth;
        int screenHeight;

        bool bulletFlying;
        bool alienFlying;

        Color[,] bulletColorArray;
        Color[,] alienColorArray;

        List<ParticleData> particleList = new List<ParticleData>();

        Random randomizer = new Random();

        SpriteFont font;

        int score;

        private SoundEffect launchSound;
        private SoundEffect hitSound;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            ScreenWidth = GraphicsDevice.Viewport.Width;
            ScreenHeight = GraphicsDevice.Viewport.Height;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            device = _graphics.GraphicsDevice;
            screenWidth = device.PresentationParameters.BackBufferWidth;
            screenHeight = device.PresentationParameters.BackBufferHeight;

            backgroundTexture = Content.Load<Texture2D>("space1_background");

            tankTexture = Content.Load<Texture2D>("tank");
            tankPosition = new Vector2(255, 370);

            cannonTexture = Content.Load<Texture2D>("cannon");
            cannonPosition = new Vector2(tankPosition.X + 45, tankPosition.Y + 15);
            cannonAngle = -MathHelper.PiOver2;

            bulletTexture = Content.Load<Texture2D>("bullet");
            bulletPosition = new Vector2(cannonPosition.X + 2, cannonPosition.Y);
            bulletColorArray = TextureTo2DArray(bulletTexture);
            bulletAngle = cannonAngle;

            alienTexture = Content.Load<Texture2D>("alien1");
            alienPosition = new Vector2(50, 0);
            alienColorArray = TextureTo2DArray(alienTexture);

            explodeTexture = Content.Load<Texture2D>("explosion");

            font = Content.Load<SpriteFont>("scorefont");

            bulletFlying = false;
            alienFlying = true;
            score = 0;

            launchSound = Content.Load<SoundEffect>("launch");
            hitSound = Content.Load<SoundEffect>("launch");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            ScreenWidth = GraphicsDevice.Viewport.Width;
            ScreenHeight = GraphicsDevice.Viewport.Height;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            ProcessKeyboard();

            isOutScreen();
            UpdateBullet();
            UpdateAlien();
            CheckCollision(gameTime);

            if (particleList.Count > 0)
                UpdateParticles(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            DrawBackground();
            DrawTank();
            DrawCannon();
            DrawBullet();
            DrawAlien();
            DrawText();
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive , SamplerState.PointWrap,
                               DepthStencilState.None, RasterizerState.CullNone);
            DrawExplosion();
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawBackground()
        {
            Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            _spriteBatch.Draw(backgroundTexture, screenRectangle, Color.White);
        }

        private void DrawTank()
        {
            _spriteBatch.Draw(tankTexture, tankPosition, null, Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
        }

        private void DrawCannon()
        {
            _spriteBatch.Draw(cannonTexture, cannonPosition, null, Color.White, cannonAngle, new Vector2(0, 0), 0.18f, SpriteEffects.None, 0);
        }

        private void DrawBullet()
        {
            if (bulletFlying)
            {
                _spriteBatch.Draw(bulletTexture, bulletPosition, null, Color.White, cannonAngle, new Vector2(0, 0), 0.15f, SpriteEffects.None, 0);
            }
        }

        private void UpdateBullet()
        {
            if (bulletFlying)
            {
                bulletPosition += bulletDirection * 8;
            }
        }

        private void DrawAlien()
        {
            if (alienFlying)
                _spriteBatch.Draw(alienTexture, alienPosition, null, Color.White, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
        }

        private void UpdateAlien()
        {
            if (alienPosition.Y > screenHeight)
            {
                alienPosition.X = randomizer.Next(screenWidth);
                alienPosition.Y = 0;
            }

            if (alienFlying)
                alienPosition.Y += 2;
        }

        private void DrawExplosion()
        {
            for (int i = 0; i < particleList.Count; i++)
            {
                ParticleData particle = particleList[i];
                _spriteBatch.Draw(explodeTexture, particle.Position, null, particle.ModColor, i, new Vector2(256, 256), particle.Scaling, SpriteEffects.None, 1);
            }
        }

        private void ProcessKeyboard()
        {
            KeyboardState keybState = Keyboard.GetState();
            if (keybState.IsKeyDown(Keys.Left))
            {
                if (cannonAngle > -3)
                    cannonAngle -= 0.02f;
            }
            if (keybState.IsKeyDown(Keys.Right))
            {
                if (cannonAngle < -0.5)
                    cannonAngle += 0.02f;
            }

            if (keybState.IsKeyDown(Keys.Enter) || keybState.IsKeyDown(Keys.Space))
            {
                launchSound.Play();
                bulletFlying = true;
                bulletAngle = cannonAngle;
                bulletPosition = cannonPosition;
                Vector2 up = new Vector2(0, -1);
                Matrix rotMatrix = Matrix.CreateRotationZ(bulletAngle + MathHelper.PiOver2);
                bulletDirection = Vector2.Transform(up, rotMatrix);
            }
        }

        private void isOutScreen()
        {
            if (bulletPosition.Y < 0)
                bulletFlying = false;
        }

        private Vector2 TexturesCollide(Color[,] tex1, Matrix mat1, Color[,] tex2, Matrix mat2)
        {
            Matrix mat1to2 = mat1 * Matrix.Invert(mat2);
            int width1 = tex1.GetLength(0);
            int height1 = tex1.GetLength(1);
            int width2 = tex2.GetLength(0);
            int height2 = tex2.GetLength(1);

            for (int x1 = 0; x1 < width1; x1++)
            {
                for (int y1 = 0; y1 < height1; y1++)
                {
                    Vector2 pos1 = new Vector2(x1, y1);
                    Vector2 pos2 = Vector2.Transform(pos1, mat1to2);

                    int x2 = (int)pos2.X;
                    int y2 = (int)pos2.Y;
                    if ((x2 >= 0) && (x2 < width2))
                    {
                        if ((y2 >= 0) && (y2 < height2))
                        {
                            if (tex1[x1, y1].A > 0)
                            {
                                if (tex2[x2, y2].A > 0)
                                {
                                    Vector2 screenPos = Vector2.Transform(pos1, mat1);
                                    return screenPos;
                                }
                            }
                        }
                    }
                }
            }

            return new Vector2(-1, -1);
        }

        private Color[,] TextureTo2DArray(Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);

            Color[,] colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    colors2D[x, y] = colors1D[x + y * texture.Width];

            return colors2D;
        }

        private void CheckCollision(GameTime gameTime)
        {
            Matrix bulletMat = Matrix.CreateRotationZ(bulletAngle) * Matrix.CreateScale(0.15f) * Matrix.CreateTranslation(bulletPosition.X, bulletPosition.Y, 0);
            Matrix alienMat = Matrix.CreateTranslation(alienPosition.X, alienPosition.Y, 0);
            Vector2 collisionPoint = TexturesCollide(bulletColorArray, bulletMat, alienColorArray, alienMat);

            if (collisionPoint.X > -1 && bulletFlying && alienFlying)
            {
                hitSound.Play();
                score++;
                bulletFlying = false;
                alienPosition.X = randomizer.Next(screenWidth);
                alienPosition.Y = 0;
                AddExplosion(collisionPoint, 10, 150.0f, 3000.0f, gameTime);
            }
        }

        private void AddExplosion(Vector2 explosionPos, int numberOfParticles, float size, float maxAge, GameTime gameTime)
        {
            for (int i = 0; i < numberOfParticles; i++)
                AddExplosionParticle(explosionPos, size, maxAge, gameTime);
        }

        private void AddExplosionParticle(Vector2 explosionPos, float explosionSize, float maxAge, GameTime gameTime)
        {
            ParticleData particle = new ParticleData();

            particle.OrginalPosition = explosionPos;
            particle.Position = particle.OrginalPosition;

            particle.BirthTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            particle.MaxAge = maxAge;
            particle.Scaling = 0.25f;
            particle.ModColor = Color.White;

            float particleDistance = (float)randomizer.NextDouble() * explosionSize;
            Vector2 displacement = new Vector2(particleDistance, 0);
            float angle = MathHelper.ToRadians(randomizer.Next(360));
            displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(angle));

            particle.Direction = displacement * 2.0f;
            particle.Accelaration = -particle.Direction;

            particleList.Add(particle);
        }

        private void UpdateParticles(GameTime gameTime)
        {
            float now = (float)gameTime.TotalGameTime.TotalMilliseconds;
            for (int i = particleList.Count - 1; i >= 0; i--)
            {
                ParticleData particle = particleList[i];
                float timeAlive = now - particle.BirthTime;

                if (timeAlive > particle.MaxAge)
                {
                    particleList.RemoveAt(i);
                }
                else
                {
                    float relAge = timeAlive / particle.MaxAge;
                    particle.Position = 0.5f * particle.Accelaration * relAge * relAge + particle.Direction * relAge + particle.OrginalPosition;

                    float invAge = 1.0f - relAge;
                    particle.ModColor = new Color(new Vector4(invAge, invAge, invAge, invAge));

                    Vector2 positionFromCenter = particle.Position - particle.OrginalPosition;
                    float distance = positionFromCenter.Length();
                    particle.Scaling = (50.0f + distance) / 200.0f;

                    particleList[i] = particle;
                }
            }
        }

        private void DrawText()
        {
            _spriteBatch.DrawString(font, "Hits: " + score.ToString(), new Vector2(20, 45), Color.Red);
        }
    }
}
