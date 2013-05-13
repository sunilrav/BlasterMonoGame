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
        int _screenWidth;
        int _screenHeight;

        private Background _background;
        private Tank _tank;
        private Cannon _cannon;
        private Bullet _bullet;
        private Alien _alien;
        private Explosion _explosion;
        private List<ParticleData> _particleList;
        private SpriteFont _font;

        private SoundEffect _launchSound;
        private SoundEffect _hitSound;

        private readonly Random _randomizer = new Random();
        private int _score;
        private int _tankPositionX;
        private int _tankPositionY;

        public Blaster()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _screenWidth = GraphicsDevice.Viewport.Width;
            _screenHeight = GraphicsDevice.Viewport.Height;

            _background = new Background();
            _tank = new Tank();
            _cannon = new Cannon();
            _bullet = new Bullet();
            _alien = new Alien();
            _explosion = new Explosion();
            _particleList = new List<ParticleData>();

            _tankPositionX = _screenWidth/2;
            _tankPositionY = _screenHeight - 193;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _background.Texture = Content.Load<Texture2D>(Constants.BackgroundImageName);

            _tank.Texture = Content.Load<Texture2D>(Constants.TankImageName);
            _tank.Position = new Vector2(_tankPositionX, _tankPositionY);

            _cannon.Texture = Content.Load<Texture2D>(Constants.CannonImageName);
            _cannon.Position = new Vector2(_tank.Position.X + 45, _tank.Position.Y + 15);
            _cannon.Angle = -MathHelper.PiOver2;

            _bullet.Texture = Content.Load<Texture2D>(Constants.BulletImageName);
            _bullet.Position = new Vector2(_cannon.Position.X + 2, _cannon.Position.Y);
            _bullet.ColorArray = TextureTo2DArray(_bullet.Texture);
            _bullet.Angle = _cannon.Angle;

            _alien.Texture = Content.Load<Texture2D>(Constants.AlienImageName);
            _alien.Position = new Vector2(50, 0);
            _alien.ColorArray = TextureTo2DArray(_alien.Texture);

            _explosion.Texture = Content.Load<Texture2D>(Constants.ExplosionImageName);

            _font = Content.Load<SpriteFont>(Constants.ScoreFontName);

            _bullet.IsFlying = false;
            _alien.IsFlying = true;
            _score = 0;

            _launchSound = Content.Load<SoundEffect>(Constants.LaunchSoundName);
            _hitSound = Content.Load<SoundEffect>(Constants.HitSoundName);
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
            _spriteBatch.Draw(_background.Texture, screenRectangle, Color.White);
        }

        private void DrawTank()
        {
            _spriteBatch.Draw(_tank.Texture, _tank.Position, null, Color.White, 0, new Vector2(0, 0), 0.5f, SpriteEffects.None, 0);
        }

        private void DrawCannon()
        {
            _spriteBatch.Draw(_cannon.Texture, _cannon.Position, null, Color.White, _cannon.Angle, new Vector2(0, 0), 0.18f, SpriteEffects.None, 0);
        }

        private void DrawBullet()
        {
            if (_bullet.IsFlying)
            {
                _spriteBatch.Draw(_bullet.Texture, _bullet.Position, null, Color.White, _cannon.Angle, new Vector2(0, 0), 0.15f, SpriteEffects.None, 0);
            }
        }

        private void UpdateBullet()
        {
            if (_bullet.IsFlying)
            {
                _bullet.Position += _bullet.Direction * 8;
            }
        }

        private void DrawAlien()
        {
            if (_alien.IsFlying)
                _spriteBatch.Draw(_alien.Texture, _alien.Position, null, Color.White, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
        }

        private void UpdateAlien()
        {
            if (_alien.Position.Y > _screenHeight)
            {
                _alien.Position.X = _randomizer.Next(_screenWidth);
                _alien.Position.Y = 0;
            }

            if (_alien.IsFlying)
                _alien.Position.Y += 2;
        }

        private void DrawExplosion()
        {
            for (int i = 0; i < _particleList.Count; i++)
            {
                ParticleData particle = _particleList[i];
                _spriteBatch.Draw(_explosion.Texture, particle.Position, null, particle.ModColor, i, new Vector2(256, 256), particle.Scaling, SpriteEffects.None, 1);
            }
        }

        private void ProcessKeyboard()
        {
            var keybState = Keyboard.GetState();
            if (keybState.IsKeyDown(Keys.Left))
            {
                if (_cannon.Angle > -3)
                    _cannon.Angle -= 0.02f;
            }
            if (keybState.IsKeyDown(Keys.Right))
            {
                if (_cannon.Angle < -0.5)
                    _cannon.Angle += 0.02f;
            }

            if (keybState.IsKeyDown(Keys.Enter) || keybState.IsKeyDown(Keys.Space))
            {
                _launchSound.Play();
                _bullet.IsFlying = true;
                _bullet.Angle = _cannon.Angle;
                _bullet.Position = _cannon.Position;
                 var up = new Vector2(0, -1);
                 var rotMatrix = Matrix.CreateRotationZ(_bullet.Angle + MathHelper.PiOver2);
                _bullet.Direction = Vector2.Transform(up, rotMatrix);
            }
        }

        private void IsOutScreen()
        {
            if (_bullet.Position.Y < 0)
                _bullet.IsFlying = false;
        }

        private static Vector2 TexturesCollide(Color[,] tex1, Matrix mat1, Color[,] tex2, Matrix mat2)
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

        private static Color[,] TextureTo2DArray(Texture2D texture)
        {
            var colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);

            var colors2D = new Color[texture.Width, texture.Height];
            for (var x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                    colors2D[x, y] = colors1D[x + y * texture.Width];

            return colors2D;
        }

        private void CheckCollision(GameTime gameTime)
        {
            var bulletMat = Matrix.CreateRotationZ(_bullet.Angle) * Matrix.CreateScale(0.15f) * Matrix.CreateTranslation(_bullet.Position.X, _bullet.Position.Y, 0);
            var alienMat = Matrix.CreateTranslation(_alien.Position.X, _alien.Position.Y, 0);
            var collisionPoint = TexturesCollide(_bullet.ColorArray, bulletMat, _alien.ColorArray, alienMat);

            if (collisionPoint.X > -1 && _bullet.IsFlying && _alien.IsFlying)
            {
                _hitSound.Play();
                _score++;
                _bullet.IsFlying = false;
                _alien.Position.X = _randomizer.Next(_screenWidth);
                _alien.Position.Y = 0;
                AddExplosion(collisionPoint, 10, 150.0f, 3000.0f, gameTime);
            }
        }

        private void AddExplosion(Vector2 explosionPos, int numberOfParticles, float size, float maxAge, GameTime gameTime)
        {
            for (var i = 0; i < numberOfParticles; i++)
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
