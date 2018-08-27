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
		private Texture2D _primaryScene, _backgroundScene, _renderReference;
		private TracingOptions _primaryTracingOptions, _backgroundTracingOptions;
		private readonly Raytracer _raytracer;
		private Scene _scene;
		private Camera _camera;
		private SpriteBatch _spriteBatch;
		private bool _sceneChanged;
		private Stopwatch _watch;
		private Task _backgroundTask;

		private MouseState _lastMouse;
		private bool _exited;
		private CancellationTokenSource _cancelBackgroundTask;
		private bool _backBufferReady;
		private long? _ellapsedBackgroundMs;

		private bool _wasActiveLastUpdate;

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

			_raytracer = _options.Multithreaded ? new MultiThreadedRaytracer() : new Raytracer();

			if (options.Width % options.RealtimeRasterLevel != 0)
			{
				throw new NotSupportedException("Width must be divisible by RealtimeRasterSize");
			}
			if (options.Height % options.RealtimeRasterLevel != 0)
			{
				throw new NotSupportedException("Height must be divisible by RealtimeRasterSize");
			}
			if (options.Width % options.BackgroundRasterLevel != 0)
			{
				throw new NotSupportedException("Width must be divisible by RealtimeRasterSize");
			}
			if (options.Height % options.BackgroundRasterLevel != 0)
			{
				throw new NotSupportedException("Height must be divisible by RealtimeRasterSize");
			}
		}

		protected override void Initialize()
		{
			base.Initialize();

			SetupScene();
			IsMouseVisible = false;
			// instead of creating a texture of the size width * height where we then end up filling Raster*Raster sized blocks with the same pixeldata
			// we create a smaller buffer and scale it when rendering
			var primaryWidth = _options.Width / _options.RealtimeRasterLevel;
			var primaryHeight = _options.Height / _options.RealtimeRasterLevel;
			var pixels = new Color[primaryWidth * primaryHeight];

			_primaryTracingOptions = new TracingOptions(primaryWidth, primaryHeight, pixels, _options.RealtimeSampleCount);

			if (_options.BackgroundRasterLevel < _options.RealtimeRasterLevel)
			{
				// only enable background rendering if it is more accurate than realtime rendering, otherwise disable it by leaving the backgroundTracingOptions null
				var backgroundWidth = _options.Width / _options.BackgroundRasterLevel;
				var backgroundHeight = _options.Width / _options.BackgroundRasterLevel;
				var secondBuffer = new Color[backgroundWidth * backgroundHeight];
				_backgroundTracingOptions = new TracingOptions(backgroundWidth, backgroundHeight, secondBuffer, _options.BackgroundSampleCount);
				_backgroundScene = new Texture2D(GraphicsDevice, backgroundWidth, backgroundHeight);
			}

			_primaryScene = new Texture2D(GraphicsDevice, primaryWidth, primaryHeight);
			// we have 2 distinct textures: primary and background
			// primary is usually smaller (for real time rendering)
			// for rendering we always use _renderReference which just points to one of the 2 other textures

			// start of with the primary scene
			_renderReference = _primaryScene;
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_sceneChanged = true;
			_watch = new Stopwatch();
		}

		private void SetupScene()
		{
			_scene = new Scene(_options.ShowLightSources);
			_scene.Add(new Light(new Vector3(2, 2, 0), Color.White));
			//_scene.Add(new Light(new Vector3(2, 2, 2), Color.Yellow, 0.5f));

			_scene.Add(new Sphere(new Vector3(0, 2.5f, 0), 1, new BasicSurface()));
			_scene.Add(new Sphere(new Vector3(2, 1, 2), 0.5f, new BasicSurface()));
			_scene.Add(new Sphere(new Vector3(4, 2, 2), 1.5f, new BasicSurface()));
			_scene.Add(new Sphere(new Vector3(0, 3, 2), 1f, new BasicSurface()));
			_scene.Add(new Plane(new Vector3(0, 1, 0), 0, new CheckerboardSurface()));
			_camera = new Camera(GraphicsDevice, new Vector3(4, 2, 0));
		}

		private void RaytraceScene()
		{
			_raytracer.TraceScene(_scene, _camera, _primaryTracingOptions);
			_primaryScene.SetData(_primaryTracingOptions.TracingTarget);
			_renderReference = _primaryScene;

			// if we are not tracing at max detail we will trace max detail in a background thread
			// once the background thread finishes we will replace our buffer with the max detail buffer
			// this allows us to move around inside the raytraced scene smoothly
			// anytime the user stops giving input the scene will also be traced in full detail and appear less blocky
			if (_backgroundTracingOptions != null)
			{
				// schedule a background task to rasterize in detail

				// freeze camera by cloning it, thus any user input will not affect the background rendering
				var copy = _camera.Clone();
				var previousTask = _backgroundTask;
				var previousCts = _cancelBackgroundTask;
				if (previousTask != null)
				{
					// kill any existing task
					previousCts.Cancel();
					previousTask.Wait();
					_backBufferReady = false;
				}
				_cancelBackgroundTask = new CancellationTokenSource();
				_backgroundTask = Task.Run(() =>
				{
					var sw = new Stopwatch();
					sw.Start();

					if (_raytracer.TraceScene(_scene, copy, _backgroundTracingOptions, _cancelBackgroundTask.Token))
					{
						// task only returns true if the entire scene was rendered

						// we call set only on main render thread, otherwise it might be called from background thread while the texture is being rendered
						// so delegate work to main thread by setting this flag to true
						_backBufferReady = true;
						sw.Stop();
						_ellapsedBackgroundMs = sw.ElapsedMilliseconds;
					}
					sw.Stop();
				});
			}
		}

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
			var kb = Keyboard.GetState();
			if (_options.Input.InputAction("CloseApplication", k => kb.IsKeyDown(k)))
			{
				_cancelBackgroundTask?.Cancel();
				Environment.Exit(0);
				_exited = true;
				return;
			}
			if (_backBufferReady)
			{
				if (_ellapsedBackgroundMs.HasValue)
				{
					Window.Title = $"Raytraced in background {_ellapsedBackgroundMs.Value}ms";
					_ellapsedBackgroundMs = null;
				}
				_backgroundScene.SetData(_backgroundTracingOptions.TracingTarget);
				_renderReference = _backgroundScene;
				_backBufferReady = false;
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

			// we can find out which scene we are rendering by checking which references equals our renderReference
			// if not primaryScene then it must be _backgroundScene
			var realtime = _renderReference == _primaryScene;
			_spriteBatch.Begin(SpriteSortMode.Deferred, null, realtime ? _options.RealtimeSamplerState : _options.BackgroundSamplerState);
			_spriteBatch.Draw(_renderReference, GraphicsDevice.Viewport.Bounds, Color.White);
			_spriteBatch.End();
		}
	}
}