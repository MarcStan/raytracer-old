using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Raytracer.SceneObjects;
using Raytracer.Surfaces;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Plane = Raytracer.SceneObjects.Plane;

namespace Raytracer
{
	public class RaytracerGame : Game
	{
		private readonly IniOptions _options;
		private Texture2D _raytracedScene;
		private readonly Color[] _pixels, _secondBuffer;
		private readonly Raytracer _raytracer;
		private Scene _scene;
		private Camera _camera;
		private SpriteBatch _spriteBatch;
		private bool _sceneChanged;
		private Stopwatch _watch;

		private MouseState _lastMouse;
		private bool _exited;
		private CancellationTokenSource _cancelBackgroundTask;
		private bool _backBufferReady;
		private long? _ellapsedBackgroundMs;

		public RaytracerGame(IniOptions options)
		{
			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			_options = options;
			new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = options.Width,
				PreferredBackBufferHeight = options.Height
			};
			_raytracer = new Raytracer(options.RealtimeRasterLevel);
			_pixels = new Color[options.Width * options.Height];
			_secondBuffer = new Color[options.Width * options.Height];
		}

		protected override void Initialize()
		{
			base.Initialize();

			SetupScene();
			IsMouseVisible = true;

			_raytracedScene = new Texture2D(GraphicsDevice, _options.Width, _options.Height);

			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_sceneChanged = true;
			_watch = new Stopwatch();
		}

		private void SetupScene()
		{
			_scene = new Scene(_options.ShowLightSources);
			_scene.Add(new Light(new Vector3(-2, 2, 0), Color.White));
			_scene.Add(new Light(new Vector3(-2, 2, 2), Color.Yellow, 0.5f));

			_scene.Add(new Sphere(new Vector3(0, 2.5f, 0), 1, new BasicSurface()));
			_scene.Add(new Sphere(new Vector3(-2, 1, 2), 0.5f, new BasicSurface()));
			_scene.Add(new Plane(new Vector3(0, 1, 0), 0, new CheckerboardSurface()));
			_camera = new Camera(GraphicsDevice, new Vector3(-4, 2, 0));
		}

		private void RaytraceScene()
		{
			_raytracer.TraceScene(_scene, _camera, new TracingOptions(_options.Width, _options.Height, _pixels));
			_raytracedScene.SetData(_pixels);

			// if we are not tracing at max detail we will trace max detail in a background thread
			// once the background thread finishes we will replace our buffer with the max detail buffer
			// this allows us to move around inside the raytraced scene smoothly
			// anytime the user stops giving input the scene will also be traced in full detail and appear less blocky
			if (_raytracer.RasterSize > 1)
			{
				// schedule a background task to rasterize in detail; kill any existing task
				CancelBackgroundTaskIfAny();
				_backBufferReady = false;
				_cancelBackgroundTask = new CancellationTokenSource();

				// freeze camera by cloning it, thus any user input will not affect the background rendering
				var copy = _camera.Clone();
				Task.Run(() =>
				{
					var sw = new Stopwatch();
					sw.Start();
					if (_raytracer.TraceScene(_scene, copy, new TracingOptions(_options.Width, _options.Height, _secondBuffer, _options.BackgroundRasterLevel, _cancelBackgroundTask?.Token)))
					{
						// task only returns true if the entire scene was rendered
						_backBufferReady = true;
						sw.Stop();
						_ellapsedBackgroundMs = sw.ElapsedMilliseconds;
					}
					sw.Stop();
				});
			}
		}

		private bool _wasActiveLastUpdate;

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (_exited)
			{
				// do not call anything else in update, some monogame methods will just throw
				return;
			}
			if (!IsActive)
			{
				_wasActiveLastUpdate = false;
				return;
			}
			if (!_wasActiveLastUpdate)
			{
				// center mouse, otherwise the user tabbing back in with the mouse at a different position will rotate the scene around
				CenterMouse();
			}
			_wasActiveLastUpdate = true;
			if (_ellapsedBackgroundMs.HasValue)
			{
				Window.Title = $"Raytraced in background {_ellapsedBackgroundMs.Value}ms";
				_ellapsedBackgroundMs = null;
			}
			var kb = Keyboard.GetState();
			if (_options.Input.InputAction("CloseApplication", k => kb.IsKeyDown(k)))
			{
				CancelBackgroundTaskIfAny();
				Exit();
				_exited = true;
				return;
			}
			if (_backBufferReady)
			{
				_raytracedScene.SetData(_secondBuffer);
				_backBufferReady = false;
			}

			HandleInput(gameTime);

			if (_sceneChanged)
			{
				// if scene changed there is no point in keeping the background task alive
				CancelBackgroundTaskIfAny();
				_sceneChanged = false;
				_watch.Restart();
				RaytraceScene();
				_watch.Stop();
				Window.Title = $"Raytraced in {_watch.ElapsedMilliseconds}ms";
			}
		}

		private void CancelBackgroundTaskIfAny()
		{
			_cancelBackgroundTask?.Cancel();
			_cancelBackgroundTask = null;
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
			if (_options.Input.InvertXAxis)
			{
				diff.X *= -1;
			}
			if (_options.Input.InvertYAxis)
			{
				diff.Y *= -1;
			}
			if (diff.X != 0 || diff.Y != 0)
			{
				_sceneChanged = true;
				const float mouseSpeed = 0.001f;
				_camera.Rotate((float)(diff.X * mouseSpeed * gameTime.ElapsedGameTime.TotalMilliseconds) / GraphicsDevice.Viewport.AspectRatio * _options.Input.MouseAccelerationX,
					(float)(diff.Y * mouseSpeed * gameTime.ElapsedGameTime.TotalMilliseconds * _options.Input.MouseAccelerationY));
			}
			CenterMouse();
			_lastMouse = Mouse.GetState();

			// movement
			var kb = Keyboard.GetState();
			float x = 0, y = 0;
			const float movementSpeed = 0.001f;
			if (_options.Input.InputAction("MoveForward", k => kb.IsKeyDown(k)))
			{
				x += movementSpeed;
			}
			if (_options.Input.InputAction("MoveBackward", k => kb.IsKeyDown(k)))
			{
				x -= movementSpeed;
			}
			if (_options.Input.InputAction("MoveRight", k => kb.IsKeyDown(k)))
			{
				y += movementSpeed;
			}
			if (_options.Input.InputAction("MoveLeft", k => kb.IsKeyDown(k)))
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