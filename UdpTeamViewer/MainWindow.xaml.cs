using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UdpTeamViewer
{
	//SERVER
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		#region PropertyChanged Realization

		public event PropertyChangedEventHandler PropertyChanged;
		private void InvokePropChanged([CallerMemberName] string prop = default)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}

		#endregion

		//Port And Image
		#region Bindings
		private int _port;
		public int Port
		{
			get { return _port; }
			set { _port = value; InvokePropChanged(); }
		}

		private ImageSource _image;

		public ImageSource Image
		{
			get { return _image; }
			set { _image = value; InvokePropChanged(); }
		}

		#endregion
		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
			Port = 8080;
		}
		private async void ConnectButon_Click(object sender, RoutedEventArgs e)
		{
			await RecieveImage();
		}

		public Task RecieveImage()
		{
			return Task.Run(() =>
			{
				try
				{
					UdpClient udpClient = new UdpClient(Port); 
					IPEndPoint endPoint = null;

					using (MemoryStream memory = new MemoryStream())
					{
						while (true)
						{
							byte[] bytes = udpClient.Receive(ref endPoint);
							if (bytes.Length == 3 && bytes[0] == 1 && bytes[1] == 2 && bytes[2] == 3)
							{
								Application.Current.Dispatcher.Invoke(() =>
								{
									Image = BytesToImage(memory.GetBuffer());
									memory.SetLength(0);
								});
							}
							else
							{
								memory.Write(bytes, 0, bytes.Length);

							}
						}

					}

				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			});
		}
		public ImageSource BytesToImage(byte[] imageData)
		{
			try
			{
				BitmapImage bitImage = new BitmapImage();
				MemoryStream ms = new MemoryStream(imageData);

				bitImage.BeginInit();
				bitImage.StreamSource = ms;
				bitImage.EndInit();

				ImageSource imgSource = bitImage;
				return imgSource;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				throw;
			}
		}
	}
}
