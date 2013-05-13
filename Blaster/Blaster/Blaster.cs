using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Blaster
{
    public class Blaster : Game
    {
        readonly GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        Texture2D _backgroundTexture;
        Texture2D _tankTexture;
        Texture2D _cannonTexture;
        Texture2D _bulletTexture;
        Texture2D _alienTexture;
        Texture2D _explodeTexture;

        Vector2 _tankPosition;
        Vector2 _cannonPosition;
        Vector2 _bulletPosition;
        Vector2 _alienPosition;

        Vector2 _bulletDirection;

        float _cannonAngle;
        float _bulletAngle;

        int _screenWidth;
        int _screenHeight;

        bool _bulletFlying;
        bool _alienFlying;

        Color[,] _bulletColorArray;
        Color[,] _alienColorArray;

        readonly List<ParticleData> _particleList = new List<ParticleData>();

        readonly Random _randomizer = new Random();

        SpriteFont _font;

        int _score;

        private SoundEffect _launchSound;
        private SoundEffect _hitSound;
        public Blaster()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _screenWidth = GraphicsDevice.Viewport.Width;
            _screenHeight = GraphicsDevice.Viewport.Height;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _backgroundTexture = Content.Load<Texture2D>("space1_background");

            _tankTexture = Content.Load<Texture2D>("tank");
            _tankPosition = new Vector2(255, 370);

            _cannonTexture = Content.Load<Texture2D>("cannon");
            _cannonPosition = new Vector2(_tankPosition.X + 45, _tankPosition.Y + 15);
            _cannonAngle = -MathHelper.PiOver2;

            _bulletTexture = Content.Load<Texture2D>("bullet");
            _bulletPosition = new Vector2(_cannonPosition.X + 2, _cannonPosition.Y);
            _bulletColorArray = TextureTo2DArray(_bulletTexture);
            _bulletAngle = _cannonAngle;

            _alienTexture = Content.Load<Texture2D>("alien1");
            _alienPosition = new Vector2(50, 0);
            _alienColorArray = TextureTo2DArray(_alienTexture);

            _explodeTexture = Content.Load<Texture2D>("explosion");

            _font = Content.Load<SpriteFont>("scorefont");

            _bulletFlying = false;
            _alienFlying = true;
            _score = 0;

            _launchSound = Content.Load<SoundEffect>("launch");
            _hitSound = Content.Load<SoundEffect>("launch");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            _screenWidth = GraphicsDevice.Viewport.Width;
            _screenHeight = GraphicsDevice.Viewport.Height;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            ProcessKeyboard();

            IsOutScreen();
            UpdateBullet();
            UpdateAlien();
            CheckCollision(gameTime);

            if (_particleList.Count > 0)
                UpdateParticles(gameTime);

            base.Update(gameTime);
        }

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
            var screenRectangle = new Rectangle(0, 0, _screenWidth, _screenHeight);
            _spriteBatch.Draw(_backgroundTexture, screenRectangle, Color.White);
        }

        private void DrawTank()
        {
            _spriteBatch.Draw(_tankTexture, _tankPosition, null, Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
        }

        private void DrawCannon()
        {
            _spriteBatch.Draw(_cannonTexture, _cannonPosition, null, Color.White, _cannonAngle, new Vector2(0, 0), 0.18f, SpriteEffects.None, 0);
        }

        private void DrawBullet()
        {
            if (_bulletFlying)
            {
                _spriteBatch.Draw(_bulletTexture, _bulletPosition, null, Color.White, _cannonAngle, new Vector2(0, 0), 0.15f, SpriteEffects.None, 0);
            }
        }

        private void UpdateBullet()
        {
            if (_bulletFlying)
            {
                _bulletPosition += _bulletDirection * 8;
            }
        }

        private void DrawAlien()
        {
            if (_alienFlying)
                _spriteBatch.Draw(_alienTexture, _alienPosition, null, Color.White, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
        }

        private void UpdateAlien()
        {
            if (_alienPosition.Y > _screenHeight)
            {
                _alienPosition.X = _randomizer.Next(_screenWidth);
                _alienPosition.Y = 0;
            }

            if (_alienFlying)
                _alienPosition.Y += 2;
        }

        private void DrawExplosion()
        {
            for (int i = 0; i < _particleList.Count; i++)
            {
                ParticleData particle = _particleList[i];
                _spriteBatch.Draw(_explodeTexture, particle.Position, null, particle.ModColor, i, new Vector2(256, 256), particle.Scaling, SpriteEffects.None, 1);
            }
        }

        private void ProcessKeyboard()
        {
            var keybState = Keyboard.GetState();
            if (keybState.IsKeyDown(Keys.Left))
            {
                if (_cannonAngle > -3)
                    _cannonAngle -= 0.02f;
            }
            if (keybState.IsKeyDown(Keys.Right))
            {
                if (_cannonAngle < -0.5)
                    _cannonAngle += 0.02f;
            }

            if (keybState.IsKeyDown(Keys.Enter) || keybState.IsKeyDown(Keys.Space))
            {
                _launchSound.Play();
                _bulletFlying = true;
                _bulletAngle = _cannonAngle;
                _bulletPosition = _cannonPosition;
                 var up = new Vector2(0, -1);
                 var rotMatrix = Matrix.CreateRotationZ(_bulletAngle + MathHelper.PiOver2);
                _bulletDirection = Vector2.Transform(up, rotMatrix);
            }
        }

        private void IsOutScreen()
        {
            if (_bulletPosition.Y < 0)
                _bulletFlying = false;
        }

        private Vector2 TexturesCollide(Color[,] tex1, Matrix mat1, Color[,] tex2, Matrix mat2)
        {
            var mat1To2 = mat1 * Matrix.Invert(mat2);
            var width1 = tex1.GetLength(0);
            var height1 = tex1.GetLength(1);
            var width2 = tex2.GetLength(0);
            var height2 = tex2.GetLength(1);

            for (var x1 = 0; x1 < width1; x1++)
            {
                for (var y1 = 0; y1 < height1; y1++)
                {
                    var pos1 = new Vector2(x1, y1);
                    var pos2 = Vector2.Transform(pos1, mat1To2);

                    var x2 = (int)pos2.X;
                    var y2 = (int)pos2.Y;
                    if ((x2 >= 0) && (x2 < width2))
                    {
                        if ((y2 >= 0) && (y2 < height2))
                        {
                            if (tex1[x1, y1].A > 0)
                            {
                                if (tex2[x2, y2].A > 0)
                                {
                                    var screenPos = Vector2.Transform(pos1, mat1);
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
            var colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);

            var colors2D = new Color[texture.Width, texture.Height];
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    colors2D[x, y] = colors1D[x + y * texture.Width];

            return colors2D;
        }

        private void CheckCollision(GameTime gameTime)
        {
            var bulletMat = Matrix.CreateRotationZ(_bulletAngle) * Matrix.CreateScale(0.15f) * Matrix.CreateTranslation(_bulletPosition.X, _bulletPosition.Y, 0);
            var alienMat = Matrix.CreateTranslation(_alienPosition.X, _alienPosition.Y, 0);
            var collisionPoint = TexturesCollide(_bulletColorArray, bulletMat, _alienColorArray, alienMat);

            if (collisionPoint.X > -1 && _bulletFlying && _alienFlying)
            {
                _hitSound.Play();
                _score++;
                _bulletFlying = false;
                _alienPosition.X = _randomizer.Next(_screenWidth);
                _alienPosition.Y = 0;
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
            var particle = new ParticleData {OrginalPosition = explosionPos};

            particle.Position = particle.OrginalPosition;

            particle.BirthTime = (float)gameTime.TotalGameTime.TotalMilliseconds;
            particle.MaxAge = maxAge;
            particle.Scaling = 0.25f;
            particle.ModColor = Color.White;

            var particleDistance = (float)_randomizer.NextDouble() * explosionSize;
            var displacement = new Vector2(particleDistance, 0);
            var angle = MathHelper.ToRadians(_randomizer.Next(360));
            displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(angle));

            particle.Direction = displacement * 2.0f;
            particle.Accelaration = -particle.Direction;

            _particleList.Add(particle);
        }

        private void UpdateParticles(GameTime gameTime)
        {
            var now = (float)gameTime.TotalGameTime.TotalMilliseconds;
            for (var i = _particleList.Count - 1; i >= 0; i--)
            {
                var particle = _particleList[i];
                var timeAlive = now - particle.BirthTime;

                if (timeAlive > particle.MaxAge)
                {
                    _particleList.RemoveAt(i);
                }
                else
                {
                    var relAge = timeAlive / particle.MaxAge;
                    particle.Position = 0.5f * particle.Accelaration * relAge * relAge + particle.Direction * relAge + particle.OrginalPosition;

                    var invAge = 1.0f - relAge;
                    particle.ModColor = new Color(new Vector4(invAge, invAge, invAge, invAge));

                    var positionFromCenter = particle.Position - particle.OrginalPosition;
                    var distance = positionFromCenter.Length();
                    particle.Scaling = (50.0f + distance) / 200.0f;

                    _particleList[i] = particle;
                }
            }
        }

        private void DrawText()
        {
            _spriteBatch.DrawString(_font, "Hits: " + _score.ToString(), new Vector2(20, 45), Color.Red);
        }
    }
}
