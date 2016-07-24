using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Raytracer
{
	public class RaytracerGame : Game
	{
		private readonly int _width = 800, _height = 600;
		private Texture2D _raytracedScene;
		private Color[] _pixels;
		private readonly Raytracer _raytracer;
		private Scene _scene;
		private Camera _camera;
		private SpriteBatch _spriteBatch;
		private bool _sceneChanged;
		private Stopwatch _watch;

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

			_raytracedScene = new Texture2D(GraphicsDevice, _width, _height);

			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_sceneChanged = true;
			_watch = new Stopwatch();
		}

		private void SetupScene()
		{
			_scene = new Scene();
			_camera = new Camera();
		}

		private void RaytraceScene()
		{
			_raytracer.TraceScene(_scene, _camera, _width, _height, ref _pixels);
			_raytracedScene.SetData(_pixels);
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				Exit();
			}

			if (_sceneChanged)
			{
				_sceneChanged = false;
				_watch.Restart();
				RaytraceScene();
				_watch.Stop();
				Window.Title = $"Raytracer - Last trace took {_watch.ElapsedMilliseconds}ms.";
			}
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