using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Raytracer.SceneObjects;
using System.Diagnostics;

namespace Raytracer
{
	public class RaytracerGame : Game
	{
		private readonly int _width = 100, _height = 100;
		private Texture2D _raytracedScene;
		private Color[] _pixels;
		private readonly Raytracer _raytracer;
		private Scene _scene;
		private Camera _camera;
		private SpriteBatch _spriteBatch;
		private bool _sceneChanged;
		private Stopwatch _watch;

		private MouseState _lastMouse;
		private bool _exited;

		public RaytracerGame()
		{
			new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = _width,
				PreferredBackBufferHeight = _height
			};
			_raytracer = new Raytracer();
			_pixels = new Color[_width * _height];
		}

		protected override void Initialize()
		{
			base.Initialize();

			SetupScene();
			IsMouseVisible = true;

			_raytracedScene = new Texture2D(GraphicsDevice, _width, _height);

			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_sceneChanged = true;
			_watch = new Stopwatch();
		}

		private void SetupScene()
		{
			_scene = new Scene();
			_scene.Add(new Light(new Vector3(0, 2, 0), Color.White));

			_scene.Add(new Sphere(new Vector3(0, 0, 0), 1));
			_scene.Add(new Sphere(new Vector3(2, 0, 0), 0.25f));
			_scene.Add(new Sphere(new Vector3(0, 0, 2), 0.25f));
			_scene.Add(new Sphere(new Vector3(1, 2, 1), 0.25f));

			_camera = new Camera(GraphicsDevice, new Vector3(0, 0, 4));
		}

		private void RaytraceScene()
		{
			_raytracer.TraceScene(_scene, _camera, _width, _height, ref _pixels);
			_raytracedScene.SetData(_pixels);
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (_exited)
			{
				// do not call anything else in update, some monogame methods will just throw
				return;
			}

			if (Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				Exit();
				_exited = true;
				return;
			}

			HandleInput(gameTime);

			if (_sceneChanged)
			{
				_sceneChanged = false;
				_watch.Restart();
				RaytraceScene();
				_watch.Stop();
				Window.Title = $"Raytraced in {_watch.ElapsedMilliseconds}ms";
			}
		}

		private void HandleInput(GameTime gameTime)
		{
			// rotation
			MouseState mouse;
			if (_lastMouse == default(MouseState))
			{
				// first update; center mouse first then get state
				CenterMouse();
				_lastMouse = mouse = Mouse.GetState();
			}
			else
			{
				mouse = Mouse.GetState();
			}

			var diff = mouse.Position - _lastMouse.Position;
			if (diff.X != 0 || diff.Y != 0)
			{
				_sceneChanged = true;
				const float mouseSpeed = 0.001f;
				_camera.Rotate((float)(diff.X * mouseSpeed * gameTime.ElapsedGameTime.TotalMilliseconds) / GraphicsDevice.Viewport.AspectRatio,
					(float)(diff.Y * mouseSpeed * gameTime.ElapsedGameTime.TotalMilliseconds));
			}
			CenterMouse();
			_lastMouse = Mouse.GetState();

			// movement
			var kb = Keyboard.GetState();
			float x = 0, y = 0;
			const float movementSpeed = 0.001f;
			if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up))
			{
				x += movementSpeed;
			}
			if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down))
			{
				x -= movementSpeed;
			}
			if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right))
			{
				y += movementSpeed;
			}
			if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left))
			{
				y -= movementSpeed;
			}
			if (x != 0 || y != 0)
			{
				_sceneChanged = true;
				_camera.Move((float)(x * gameTime.ElapsedGameTime.TotalMilliseconds), (float)(y * gameTime.ElapsedGameTime.TotalMilliseconds));
			}
		}

		private void CenterMouse()
		{
			Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.CornflowerBlue);

			_spriteBatch.Begin();
			_spriteBatch.Draw(_raytracedScene, Vector2.Zero, Color.White);
			_spriteBatch.End();
		}
	}
}