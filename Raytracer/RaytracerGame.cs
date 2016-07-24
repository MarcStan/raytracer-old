using Microsoft.Xna.Framework;

namespace Raytracer
{
	public class RaytracerGame : Game
	{
		public RaytracerGame()
		{
			new GraphicsDeviceManager(this);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.CornflowerBlue);
		}
	}
}