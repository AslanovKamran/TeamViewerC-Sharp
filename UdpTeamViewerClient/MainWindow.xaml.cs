using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace UdpTeamViewerClient
{
	//CLIENT
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		#region PropertyChanged Realization

		public event PropertyChangedEventHandler PropertyChanged;
		private void InvokePropChanged([CallerMemberName] string prop = default)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}

		#endregion
		// Ip And Port
		#region Bindings

		private string _ip;
		public string Ip
		{
			get { return _ip; }
			set { _ip = value; InvokePropChanged(); }
		}
		private int _port;
		public int Port
		{
			get { return _port; }
			set { _port = value; InvokePropChanged(); }
		}

		#endregion
		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
			Port = 8080;
			Ip = "127.0.0.1";
		}
		private async void ClientStartButton_Click(object sender, RoutedEventArgs e)
		{
			await SendScreen();
		}
		public byte[] CaptureScreen()
		{
			int width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
			int height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
			using (MemoryStream memory = new MemoryStream())
			{
				Bitmap bmp = new Bitmap(width, height);
				using (Graphics g = Graphics.FromImage(bmp))
				{
					g.CopyFromScreen(0, 0, 0, 0, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size);
					Bitmap objBitmap = new Bitmap(bmp, (int)(width), (int)(height));
					objBitmap.Save(memory, ImageFormat.Jpeg);
				}
				return memory.GetBuffer();
			}
		}
		public Task SendScreen()
		{
			return Task.Run(() =>
			{
				UdpClient udpClient = new UdpClient();
				while (true)
				{
					Thread.Sleep(100);
					byte[] bytes = CaptureScreen();
					byte[] chunk = new byte[5_000];
					int bytesCount = 0;
					using (MemoryStream memory = new MemoryStream(bytes))
					{
						while ((bytesCount = memory.Read(chunk, 0, chunk.Length)) != 0)
						{
							udpClient.Send(chunk, bytesCount, Ip, Port);
						}
					}

					//Token of bytes end
					udpClient.Send(new byte[3] {1,2,3}, 3, Ip, Port);
				}
			});
		}
	}
}
